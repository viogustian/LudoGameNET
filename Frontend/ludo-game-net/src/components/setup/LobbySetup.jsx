import { useState } from 'react';

export default function LobbySetup({ onCreateRoom, onJoinRoom, busy, error }) {
  const [roomCode, setRoomCode] = useState('');

  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh]">
      <div className="bg-black/40 backdrop-blur-md p-8 rounded-2xl shadow-2xl border border-white/10 w-full max-w-md">
        <h2 className="text-3xl font-bold text-white text-center mb-8 drop-shadow-md">Ludo Multiplayer</h2>
        
        {error && (
          <div className="bg-red-500/20 border border-red-500/50 text-red-200 p-3 rounded-lg mb-6 text-sm text-center">
            {error}
          </div>
        )}

        <div className="space-y-6">
          <button
            onClick={onCreateRoom}
            disabled={busy}
            className="w-full py-4 bg-gradient-to-r from-green-500 to-emerald-600 hover:from-green-400 hover:to-emerald-500 text-white font-bold rounded-xl transition-all transform hover:scale-[1.02] active:scale-[0.98] shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Create New Room
          </button>

          <div className="relative flex items-center py-2">
            <div className="flex-grow border-t border-white/20"></div>
            <span className="flex-shrink-0 mx-4 text-white/50 text-sm">OR</span>
            <div className="flex-grow border-t border-white/20"></div>
          </div>

          <div className="space-y-3">
            <input
              type="text"
              placeholder="Enter Room Code"
              maxLength={6}
              value={roomCode}
              onChange={(e) => setRoomCode(e.target.value.toUpperCase())}
              disabled={busy}
              className="w-full px-4 py-3 bg-black/30 border border-white/20 rounded-xl text-white placeholder-white/40 text-center text-xl tracking-widest focus:outline-none focus:border-blue-500 transition-colors uppercase disabled:opacity-50"
            />
            <button
              onClick={() => onJoinRoom(roomCode)}
              disabled={busy || roomCode.length !== 6}
              className="w-full py-3 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 text-white font-bold rounded-xl transition-all transform hover:scale-[1.02] active:scale-[0.98] shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Join Room
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
