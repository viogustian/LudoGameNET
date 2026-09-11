import { useState, useMemo, useCallback, useRef, useEffect } from 'react';
import { COLORS } from '../constants/colors.js';
import { YARD_HOLDING_POINTS } from '../constants/board.js';
import { key as cellKey } from '../lib/boardGeometry.js';
import { piecePosition, isPieceOnBoard, pieceKey, computeWalkSteps, sleep } from '../lib/gameLogic.js';
import { playSound, setMuted as setSoundMuted, playBgm, stopBgm } from '../sounds.js';
import { useLudoHub } from './useLudoHub.js';

export function useGameState() {
  const [screen, setScreen] = useState('lobby'); // lobby -> seatSelection -> game
  const [muted, setMuted] = useState(false);
  const [renderPositions, setRenderPositions] = useState({});
  const [rolling, setRolling] = useState(false);
  const [rollToken, setRollToken] = useState(0);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [diceDisplayValue, setDiceDisplayValue] = useState(null);
  
  const lastGameStateRef = useRef(null);
  const hubRef = useRef(null);
  const [internalDiceValue, setInternalDiceValue] = useState(null);
  const [internalValidPieces, setInternalValidPieces] = useState([]);

  const toggleMuted = useCallback(() => {
    setMuted((prev) => {
      const next = !prev;
      setSoundMuted(next);
      return next;
    });
  }, []);

  const syncRenderPositions = useCallback((game) => {
    if (!game || !game.players) return;
    const next = {};
    game.players.forEach((p) => {
      p.pieces.filter(isPieceOnBoard).forEach((pc) => {
        next[pieceKey(p.color, pc.id)] = piecePosition(pc);
      });
    });
    setRenderPositions(next);
  }, []);

  const onRoomStateUpdated = useCallback((updatedRoom) => {
    if (updatedRoom?.game) {
       lastGameStateRef.current = updatedRoom.game;
       // Only sync if we are not animating
       if(!busy) syncRenderPositions(updatedRoom.game);
    }
  }, [busy, syncRenderPositions]);

  const onGameStarted = useCallback((updatedRoom) => {
    setScreen('game');
    setDiceDisplayValue(null);
    setInternalDiceValue(null);
    setInternalValidPieces([]);
    syncRenderPositions(updatedRoom.game);
    lastGameStateRef.current = updatedRoom.game;
    playSound('start');
    playBgm();
  }, [syncRenderPositions]);

  const onDiceRolled = useCallback(async (updatedRoom) => {
    setRolling(true);
    setBusy(true);
    playSound('diceRoll');
    setRollToken((t) => t + 1);
    
    const gs = updatedRoom?.game || updatedRoom?.Game;
    const value = gs?.currentDiceValue ?? gs?.CurrentDiceValue;
    
    setDiceDisplayValue(value);
    await sleep(1300);
    
    setInternalDiceValue(value);
    const validPieces = gs?.currentValidPieces || gs?.CurrentValidPieces || [];
    setInternalValidPieces(validPieces);
    setRolling(false);
    
    if (validPieces.length === 0 && value !== null) {
      setTimeout(() => {
        setInternalDiceValue(null);
        setDiceDisplayValue(null);
        if (hubRef.current) {
          hubRef.current.passTurn(updatedRoom.roomCode || updatedRoom.RoomCode);
        }
        setBusy(false);
      }, 800);
    } else {
      setBusy(false);
    }
    
    lastGameStateRef.current = gs;
  }, [playSound]);

  const onPieceMoved = useCallback(async (updatedRoom) => {
    setBusy(true);
    const beforeGameState = lastGameStateRef.current;
    const afterGameState = updatedRoom.game;
    
    setInternalDiceValue(null);
    setInternalValidPieces([]);
    
    if (beforeGameState && afterGameState) {
        // Find who moved by comparing pieces
        let movedPieceBefore = null;
        let movedPieceAfter = null;
        let moverColor = null;

        const beforeMap = new Map();
        beforeGameState.players.forEach((p) => p.pieces.forEach((pc) => beforeMap.set(pieceKey(p.color, pc.id), pc)));

        afterGameState.players.forEach((p) => p.pieces.forEach((pc) => {
            const before = beforeMap.get(pieceKey(p.color, pc.id));
            if (before && before.pathIndex !== pc.pathIndex) {
                // If it went back to base, it was captured (or moved). But mover moves forward.
                if (pc.state !== 'Base' || pc.pathIndex === 0) {
                    movedPieceBefore = before;
                    movedPieceAfter = pc;
                    moverColor = p.color;
                }
            }
        }));

        if (movedPieceBefore && movedPieceAfter && moverColor != null) {
            const walkSteps = computeWalkSteps(moverColor, movedPieceBefore, movedPieceAfter);
            const moverKey = pieceKey(moverColor, movedPieceAfter.id);
            
            let captured = false;
            let pieceFinished = movedPieceAfter.state === 'Finished' || movedPieceAfter.state === 3;
            const capturedKeys = [];

            afterGameState.players.forEach((p) => {
                p.pieces.forEach((pc) => {
                    const before = beforeMap.get(pieceKey(p.color, pc.id));
                    const isMover = p.color === moverColor && pc.id === movedPieceAfter.id;
                    if (before && before.state !== 'Base' && before.state !== 0 && (pc.state === 'Base' || pc.state === 0) && !isMover) {
                        captured = true;
                        capturedKeys.push(pieceKey(p.color, pc.id));
                    }
                });
            });

            const wonGame = afterGameState.state === 'Finished' || afterGameState.state === 2;

            if (wonGame) playSound('win');
            else if (captured) playSound('capture');
            else if (pieceFinished) playSound('finish');
            else playSound('move');

            for (const [r, c] of walkSteps) {
                setRenderPositions((prev) => ({ ...prev, [moverKey]: [r, c] }));
                await sleep(220);
            }

            if (capturedKeys.length > 0) {
                setRenderPositions((prev) => {
                    const next = { ...prev };
                    capturedKeys.forEach((k) => {
                        const [color, idStr] = k.split('-');
                        next[k] = YARD_HOLDING_POINTS[color][Number(idStr) % 4];
                    });
                    return next;
                });
                await sleep(320);
            }
        }
    }
    
    syncRenderPositions(afterGameState);
    lastGameStateRef.current = afterGameState;
    setBusy(false);
  }, [syncRenderPositions]);

  const callbacks = useMemo(() => ({
    onRoomStateUpdated,
    onGameStarted,
    onDiceRolled,
    onPieceMoved
  }), [onRoomStateUpdated, onGameStarted, onDiceRolled, onPieceMoved]);

  const hub = useLudoHub(callbacks);
  
  useEffect(() => {
    hubRef.current = hub;
  }, [hub]);

  const { connection, room, error: hubError } = hub;

  const gameState = room?.game || room?.Game;
  
  // Use internal states while we wait for animations
  const backendDiceValue = gameState?.currentDiceValue ?? gameState?.CurrentDiceValue;
  const backendValidPieces = gameState?.currentValidPieces || gameState?.CurrentValidPieces || [];
  
  const diceValue = internalDiceValue !== null ? internalDiceValue : backendDiceValue;
  const validPieces = internalValidPieces.length > 0 ? internalValidPieces : backendValidPieces;
  
  const colorConnections = room?.colorConnections || room?.ColorConnections || {};
  const myConnectionId = connection?.connectionId;
  const hostConnectionId = room?.hostConnectionId || room?.HostConnectionId;
  
  let myColor = null;
  if (colorConnections && myConnectionId) {
     for (const key of Object.keys(colorConnections)) {
         if (colorConnections[key] === myConnectionId) {
             myColor = key; 
             break;
         }
     }
  }

  // Handle case where Enums serialize to integers (Playing = 1)
  const isPlaying = gameState && (gameState.state === 'Playing' || gameState.state === 1 || gameState.State === 'Playing' || gameState.State === 1);

  const players = gameState?.players || gameState?.Players;
  const currentPlayerIndex = gameState?.currentPlayerIndex ?? gameState?.CurrentPlayerIndex;
  const currentPlayer = players ? players[currentPlayerIndex] : null;
  const isMyTurn = currentPlayer && (currentPlayer.color === myColor || currentPlayer.color.toString() === myColor);

  const handleCreateRoom = async () => {
    setError('');
    setBusy(true);
    const code = await hub.createRoom();
    setBusy(false);
    if (code) setScreen('seatSelection');
    else setError('Failed to create room');
  };

  const handleJoinRoom = async (code) => {
    setError('');
    setBusy(true);
    const success = await hub.joinRoom(code);
    setBusy(false);
    if (success) setScreen('seatSelection');
    else setError('Failed to join room');
  };

  const handleClaimSeat = async (colorId) => {
    if (!room) return;
    setError('');
    setBusy(true);
    await hub.tryClaimSeat(room.roomCode, colorId);
    setBusy(false);
  };

  const handleStartGame = async () => {
    if (!room) return;
    setError('');
    setBusy(true);
    await hub.startGame(room.roomCode);
    setBusy(false);
  };

  const rollDice = async () => {
    if (!room || !isMyTurn || busy || rolling) return;
    await hub.rollDice(room.roomCode);
  };

  const movePiece = async (pieceId) => {
    if (!room || !isMyTurn || diceValue == null || busy) return;
    await hub.movePiece(room.roomCode, pieceId, diceValue);
  };

  const resetGame = useCallback(() => {
    setScreen('lobby');
    stopBgm();
  }, []);

  const cellGroups = useMemo(() => {
    const map = {};
    Object.entries(renderPositions).forEach(([pk, pos]) => {
      const k = cellKey(pos[0], pos[1]);
      if (!map[k]) map[k] = [];
      map[k].push(pk);
    });
    return map;
  }, [renderPositions]);

  const validIds = useMemo(() => new Set(validPieces.map((p) => p.id)), [validPieces]);
  const canRoll = isPlaying && !busy && !(diceValue != null && validPieces.length > 0) && isMyTurn;

  return {
    screen,
    gameState,
    diceValue,
    diceDisplayValue,
    rollToken,
    validPieces,
    rolling,
    busy,
    error: error || hubError,
    muted,
    cellGroups,
    validIds,
    canRoll,
    currentPlayer,
    room,
    colorConnections,
    myConnectionId,
    hostConnectionId,
    COLORS,
    toggleMuted,
    createRoom: handleCreateRoom,
    joinRoom: handleJoinRoom,
    claimSeat: handleClaimSeat,
    startGame: handleStartGame,
    rollDice,
    movePiece,
    resetGame,
  };
}