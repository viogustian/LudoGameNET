export const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5080/api/game';

async function callApi(path, options = {}) {
  let res;
  try {
    res = await fetch(`${API_BASE}${path}`, {
      headers: { 'Content-Type': 'application/json' },
      ...options,
    });
  } catch (e) {
    throw new Error(
      `Tidak bisa menghubungi ${API_BASE}. Pastikan backend (dotnet run) sedang berjalan dan alamatnya benar.`
    );
  }
  const text = await res.text();
  let data = null;
  try { data = text ? JSON.parse(text) : null; } catch (e) { /* ignore */ }
  if (!res.ok) {
    throw new Error((data && data.error) || `Permintaan gagal (status ${res.status}).`);
  }
  return data;
}

export const gameApi = {
  getState: () => callApi('', { method: 'GET' }),
  startGame: (colors) => callApi('', { method: 'POST', body: JSON.stringify({ colors }) }),
  rollDice: () => callApi('/roll', { method: 'POST' }),
  movePiece: (pieceId, diceValue) =>
    callApi('/move', { method: 'POST', body: JSON.stringify({ pieceId, diceValue }) }),

  // ---------------------------------------------------------------------
  // Dev-tools only. The backend refuses all of these (404) unless it's
  // running in the Development environment, so they're safe to ship even
  // if this bundle somehow ends up pointed at a non-dev API.
  // ---------------------------------------------------------------------
  devGetDiceStatus: () => callApi('/dev/dice', { method: 'GET' }),
  devSetDice: (value, lock) =>
    callApi('/dev/dice', { method: 'POST', body: JSON.stringify({ value, lock }) }),
  devClearDice: () => callApi('/dev/dice/clear', { method: 'POST' }),
  devEnterAll: (color) =>
    callApi('/dev/enter-all', { method: 'POST', body: JSON.stringify({ color }) }),
  devFinishAll: (color) =>
    callApi('/dev/finish-all', { method: 'POST', body: JSON.stringify({ color }) }),
  devResetToBase: (color) =>
    callApi('/dev/reset-base', { method: 'POST', body: JSON.stringify({ color }) }),
  devForcePiece: ({ color, pieceId, state, pathIndex }) =>
    callApi('/dev/force-piece', {
      method: 'POST',
      body: JSON.stringify({ color, pieceId, state, pathIndex }),
    }),
  devSetTurn: (playerIndex) =>
    callApi('/dev/set-turn', { method: 'POST', body: JSON.stringify({ playerIndex }) }),
  devSetSixes: (count) =>
    callApi('/dev/set-sixes', { method: 'POST', body: JSON.stringify({ count }) }),
};
