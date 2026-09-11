import { useState, useEffect, useCallback, useRef } from 'react';
import * as signalR from '@microsoft/signalr';

export function useLudoHub(callbacks = {}) {
  const [connection, setConnection] = useState(null);
  const [room, setRoom] = useState(null);
  const [error, setError] = useState(null);
  const [connectionStatus, setConnectionStatus] = useState('connecting'); // 'connecting' | 'connected' | 'reconnecting' | 'disconnected'

  const callbacksRef = useRef(callbacks);
  useEffect(() => {
    callbacksRef.current = callbacks;
  }, [callbacks]);

  useEffect(() => {
    // Determine backend base URL:
    // 1. VITE_API_URL if configured (e.g. https://ludogamenet-api.onrender.com)
    // 2. http://localhost:5286 when running in local development
    // 3. window.location.origin when deployed in same-origin monolith container
    const rawApiUrl = import.meta.env.VITE_API_URL;
    const apiBase = rawApiUrl
      ? rawApiUrl.replace(/\/+$/, '')
      : (import.meta.env.DEV ? 'http://localhost:5286' : window.location.origin);

    const hubUrl = `${apiBase}/hubs/ludo`;

    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 20000])
      .build();

    newConnection.onreconnecting(() => {
      setConnectionStatus('reconnecting');
      setError('Connection lost. Reconnecting to game server...');
    });

    newConnection.onreconnected(() => {
      setConnectionStatus('connected');
      setError(null);
    });

    newConnection.onclose(() => {
      setConnectionStatus('disconnected');
    });

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

    setConnectionStatus('connecting');
    newConnection.start().then(() => {
      setConnectionStatus('connected');
      setError(null);
    }).catch(err => {
      console.error('SignalR Connection Error: ', err);
      setConnectionStatus('disconnected');
      setError('Could not connect to game server. If the server is on a free tier (e.g. Render), please wait ~30-50 seconds for it to wake up and refresh.');
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
    connectionStatus,
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
