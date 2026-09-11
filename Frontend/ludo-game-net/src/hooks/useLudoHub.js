import { useState, useEffect, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

export function useLudoHub(callbacks = {}) {
  const [connection, setConnection] = useState(null);
  const [room, setRoom] = useState(null);
  const [error, setError] = useState(null);

  const callbacksRef = useRef(callbacks);
  useEffect(() => {
    callbacksRef.current = callbacks;
  }, [callbacks]);

  useEffect(() => {
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5286/hubs/ludo") // Adjusted URL based on backend launchSettings
      .withAutomaticReconnect()
      .build();

    newConnection.on('RoomStateUpdated', (updatedRoom) => {
      setRoom(updatedRoom);
      if (callbacksRef.current.onRoomStateUpdated) callbacksRef.current.onRoomStateUpdated(updatedRoom);
    });

    newConnection.on('GameStarted', (updatedRoom) => {
      setRoom(updatedRoom);
      if (callbacksRef.current.onGameStarted) callbacksRef.current.onGameStarted(updatedRoom);
    });

    newConnection.on('DiceRolled', (updatedRoom) => {
      setRoom(updatedRoom);
      if (callbacksRef.current.onDiceRolled) callbacksRef.current.onDiceRolled(updatedRoom);
    });

    newConnection.on('PieceMoved', (updatedRoom) => {
      setRoom(updatedRoom);
      if (callbacksRef.current.onPieceMoved) callbacksRef.current.onPieceMoved(updatedRoom);
    });

    newConnection.start().then(() => {
        setError(null);
    }).catch(err => {
      console.error('SignalR Connection Error: ', err);
      setError('Could not connect to game server.');
    });

    setConnection(newConnection);

    return () => {
      newConnection.stop();
    };
  }, []);

  const createRoom = useCallback(async () => {
    if (!connection) return;
    try {
      const roomCode = await connection.invoke('CreateRoom');
      return roomCode;
    } catch (e) {
      setError(e.toString());
      return null;
    }
  }, [connection]);

  const joinRoom = useCallback(async (roomCode) => {
    if (!connection) return;
    try {
      const success = await connection.invoke('JoinRoom', roomCode);
      if (!success) setError('Failed to join room.');
      return success;
    } catch (e) {
      setError(e.toString());
      return false;
    }
  }, [connection]);

  const tryClaimSeat = useCallback(async (roomCode, color) => {
    if (!connection) return;
    try {
      return await connection.invoke('TryClaimSeat', roomCode, color);
    } catch (e) {
      setError(e.toString());
      return false;
    }
  }, [connection]);

  const startGame = useCallback(async (roomCode) => {
    if (!connection) return;
    try {
      await connection.invoke('StartGame', roomCode);
    } catch (e) {
      setError(e.toString());
    }
  }, [connection]);

  const rollDice = useCallback(async (roomCode) => {
    if (!connection) return;
    try {
      await connection.invoke('RollDice', roomCode);
    } catch (e) {
      setError(e.toString());
    }
  }, [connection]);

  const movePiece = useCallback(async (roomCode, pieceId, diceValue) => {
    if (!connection) return;
    try {
      await connection.invoke('MovePiece', roomCode, pieceId, diceValue);
    } catch (e) {
      setError(e.toString());
    }
  }, [connection]);

  const passTurn = useCallback(async (roomCode) => {
    if (!connection) return;
    try {
      await connection.invoke('PassTurn', roomCode);
    } catch (e) {
      setError(e.toString());
    }
  }, [connection]);

  return {
    connection,
    room,
    error,
    createRoom,
    joinRoom,
    tryClaimSeat,
    startGame,
    rollDice,
    movePiece,
    passTurn
  };
}
