import { COLORS, PALETTE, COLOR_LABEL } from '../../constants/colors.js';

export default function SeatSelection({ 
  roomCode, 
  colorConnections, 
  myConnectionId, 
  hostConnectionId, 
  onClaimSeat, 
  onStartGame, 
  busy 
}) {
  const isHost = myConnectionId && myConnectionId === hostConnectionId;
  const numPlayers = Object.keys(colorConnections || {}).length;
  const canStart = isHost && numPlayers >= 2;

  const handleCopyCode = () => {
    navigator.clipboard.writeText(roomCode);
  };

  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] py-8">
      <div className="bg-black/40 backdrop-blur-md p-8 rounded-3xl shadow-2xl border border-white/10 w-full max-w-2xl">
        
        <div className="text-center mb-8">
          <h2 className="text-3xl font-bold text-white mb-2">Room Lobby</h2>
          <div 
            onClick={handleCopyCode}
            className="inline-flex items-center gap-3 bg-black/50 px-6 py-3 rounded-full cursor-pointer hover:bg-black/70 transition-colors border border-white/10 group"
            title="Click to copy"
          >
            <span className="text-white/60 text-sm">Room Code:</span>
            <span className="text-2xl font-mono text-emerald-400 font-bold tracking-widest">{roomCode}</span>
            <svg className="w-5 h-5 text-white/40 group-hover:text-emerald-400 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"></path></svg>
          </div>
        </div>

        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-10">
          {COLORS.map((colorStr, index) => {
            const colorKeys = [colorStr, colorStr.toLowerCase(), index.toString()];
            let foundOccupantId = null;
            if (colorConnections) {
                for (const key of colorKeys) {
                    if (key !== undefined && colorConnections[key]) {
                        foundOccupantId = colorConnections[key];
                        break;
                    }
                }
            }
            const isTaken = !!foundOccupantId;
            const isMe = foundOccupantId === myConnectionId;
            const hexColor = PALETTE[colorStr].main;
            const displayName = COLOR_LABEL[colorStr] || colorStr;

            return (
              <button
                key={colorStr}
                onClick={() => onClaimSeat(colorStr)}
                disabled={busy || (isTaken && !isMe)}
                className={`
                  relative flex flex-col items-center p-4 rounded-2xl border-2 transition-all duration-300
                  ${isTaken 
                    ? isMe 
                      ? 'bg-white/20 border-white shadow-[0_0_20px_rgba(255,255,255,0.3)] transform scale-105' 
                      : 'bg-black/40 border-transparent opacity-60 cursor-not-allowed grayscale-[50%]'
                    : 'bg-black/20 border-white/10 hover:border-white/40 hover:bg-white/10 cursor-pointer hover:scale-105'
                  }
                `}
                style={{
                  boxShadow: isMe ? `0 0 30px ${hexColor}40` : 'none'
                }}
              >
                <div 
                  className="w-12 h-12 rounded-full mb-3 shadow-inner flex items-center justify-center border-2 border-black/20"
                  style={{ backgroundColor: hexColor }}
                >
                  {isMe && (
                    <svg className="w-6 h-6 text-white drop-shadow-md" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M5 13l4 4L19 7"></path></svg>
                  )}
                </div>
                <span className="text-white font-bold text-md mb-1">{displayName}</span>
                <span className={`text-xs ${isTaken ? (isMe ? 'text-green-300 font-medium' : 'text-white/50') : 'text-white/30'}`}>
                  {isTaken ? (isMe ? 'You' : 'Taken') : 'Available'}
                </span>
              </button>
            );
          })}
        </div>

        <div className="flex flex-col items-center min-h-[80px] justify-center">
          {isHost ? (
            <button
              onClick={onStartGame}
              disabled={busy || !canStart}
              className={`
                px-10 py-4 rounded-xl font-bold text-lg transition-all transform
                ${canStart 
                  ? 'bg-gradient-to-r from-emerald-500 to-green-600 hover:from-emerald-400 hover:to-green-500 text-white shadow-[0_0_30px_rgba(16,185,129,0.4)] hover:scale-105 active:scale-95' 
                  : 'bg-gray-600 text-gray-300 cursor-not-allowed'
                }
              `}
            >
              Start Game
            </button>
          ) : (
            <div className="text-white/60 text-lg flex items-center gap-3">
              <svg className="animate-spin h-5 w-5 text-white/50" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Waiting for host to start...
            </div>
          )}
          
          {isHost && !canStart && (
            <p className="text-yellow-400/80 text-sm mt-4 text-center">
              Waiting for at least 1 more player to join...
            </p>
          )}
        </div>

      </div>
    </div>
  );
}
