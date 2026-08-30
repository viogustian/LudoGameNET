import { useState, useMemo, useCallback } from 'react';
import { COLORS } from '../constants/colors.js';
import { YARD_HOLDING_POINTS } from '../constants/board.js';
import { key as cellKey } from '../lib/boardGeometry.js';
import { piecePosition, isPieceOnBoard, pieceKey, computeWalkSteps, sleep } from '../lib/gameLogic.js';
import { gameApi } from '../api/gameApi.js';
import { playSound, setMuted as setSoundMuted } from '../sounds.js';

export function useGameState() {
  const [screen, setScreen] = useState('setup');
  const [selectedColors, setSelectedColors] = useState(['Red', 'Blue']);
  const [gameState, setGameState] = useState(null);
  const [diceValue, setDiceValue] = useState(null);
  const [diceDisplayValue, setDiceDisplayValue] = useState(null);
  const [rollToken, setRollToken] = useState(0);
  const [validPieces, setValidPieces] = useState([]);
  const [rolling, setRolling] = useState(false);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const [muted, setMuted] = useState(false);
  const [renderPositions, setRenderPositions] = useState({});

  const syncRenderPositions = useCallback((state) => {
    const next = {};
    state.players.forEach((p) => {
      p.pieces.filter(isPieceOnBoard).forEach((pc) => {
        next[pieceKey(p.color, pc.id)] = piecePosition(pc);
      });
    });
    setRenderPositions(next);
  }, []);

  const toggleMuted = useCallback(() => {
    setMuted((prev) => {
      const next = !prev;
      setSoundMuted(next);
      return next;
    });
  }, []);

  const toggleColor = useCallback((c) => {
    setSelectedColors((prev) => {
      if (prev.includes(c)) return prev.filter((x) => x !== c);
      if (prev.length >= 4) return prev;
      return [...prev, c];
    });
  }, []);

  const currentPlayer = gameState ? gameState.players[gameState.currentPlayerIndex] : null;

  const startGame = useCallback(async () => {
    setError('');
    setBusy(true);
    try {
      const data = await gameApi.startGame(selectedColors);
      setGameState(data);
      setDiceValue(null);
      setDiceDisplayValue(null);
      setValidPieces([]);
      syncRenderPositions(data);
      setScreen('game');
      playSound('start');
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }, [selectedColors, syncRenderPositions]);

  const rollDice = useCallback(async () => {
    if (!gameState || busy) return;
    setError('');
    setBusy(true);
    setRolling(true);
    playSound('diceRoll');
    try {
      const data = await gameApi.rollDice();

      setDiceDisplayValue(data.diceValue);
      setRollToken((t) => t + 1);
      await sleep(1300);

      setDiceValue(data.diceValue);
      setValidPieces(data.validPieces || []);

      if (!data.validPieces || data.validPieces.length === 0) {
        const fresh = await gameApi.getState();
        setGameState(fresh);
        syncRenderPositions(fresh);
      } else {
        setGameState((prev) => ({ ...prev, currentPlayerIndex: data.currentPlayerIndex }));
      }
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
      setRolling(false);
    }
  }, [gameState, busy, syncRenderPositions]);

  const movePiece = useCallback(async (pieceId) => {
    if (!gameState || diceValue == null || busy) return;
    setError('');
    setBusy(true);
    try {
      const mover = gameState.players[gameState.currentPlayerIndex];
      const beforeMap = new Map();
      gameState.players.forEach((p) => p.pieces.forEach((pc) => beforeMap.set(pieceKey(p.color, pc.id), pc)));

      const data = await gameApi.movePiece(pieceId, diceValue);

      const moverKey = pieceKey(mover.color, pieceId);
      const moverAfter = data.players.find((p) => p.color === mover.color).pieces.find((pc) => pc.id === pieceId);
      const moverBefore = beforeMap.get(moverKey);
      const walkSteps = computeWalkSteps(mover.color, moverBefore, moverAfter);

      let captured = false;
      let pieceFinished = moverAfter.state === 'Finished';
      const capturedKeys = [];

      data.players.forEach((p) => {
        p.pieces.forEach((pc) => {
          const before = beforeMap.get(pieceKey(p.color, pc.id));
          const isMover = p.color === mover.color && pc.id === pieceId;
          if (before && before.state === 'OnBoard' && pc.state === 'Base' && !isMover) {
            captured = true;
            capturedKeys.push(pieceKey(p.color, pc.id));
          }
        });
      });

      const wonGame = data.state === 'Finished' && !!data.winnerColor;

      setDiceValue(null);
      setValidPieces([]);

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

      setGameState(data);
      syncRenderPositions(data);
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }, [gameState, diceValue, busy, syncRenderPositions]);

  // Used by DevTools: after a dev-only mutation (force dice, teleport a
  // piece, send everything to Goal, etc.) the server state can jump in ways
  // the normal move/roll flow never produces, so we just resync everything
  // in one go instead of trying to animate it.
  const applyServerState = useCallback((data) => {
    setGameState(data);
    syncRenderPositions(data);
    setDiceValue(null);
    setDiceDisplayValue(null);
    setValidPieces([]);
    setError('');
  }, [syncRenderPositions]);

  const resetGame = useCallback(() => {
    setScreen('setup');
    setGameState(null);
    setDiceValue(null);
    setDiceDisplayValue(null);
    setValidPieces([]);
    setError('');
    setRenderPositions({});
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
  const canRoll = gameState && gameState.state === 'Playing' && !busy && !(diceValue != null && validPieces.length > 0);

  return {
    screen, selectedColors, gameState, diceValue, diceDisplayValue, rollToken, validPieces,
    rolling, busy, error, muted, cellGroups, validIds, canRoll, currentPlayer,
    COLORS,
    toggleMuted, toggleColor, startGame, rollDice, movePiece, resetGame,
    applyServerState,
  };
}
