import { COLOR_LABEL, PALETTE } from '../../constants/colors.js';

export default function PlayersList({ players, currentPlayerIndex }) {
  return (
    <div className="rounded-2xl p-4 flex flex-col gap-2.5" style={{ backgroundColor: '#2C334E' }}>
      <p style={{ color: '#8890B5' }} className="text-xs font-medium uppercase tracking-wide">Pemain</p>
      {players.map((p, idx) => {
        const finished = p.pieces.filter((pc) => pc.state === 'Finished').length;
        const isTurn = idx === currentPlayerIndex;
        return (
          <div
            key={p.id}
            className="flex items-center justify-between rounded-lg px-3 py-2"
            style={{
              backgroundColor: isTurn ? PALETTE[p.color].dark : '#232A44',
              boxShadow: isTurn ? `0 0 0 2px ${PALETTE[p.color].ring}` : 'none',
            }}
          >
            <div className="flex items-center gap-2">
              <span className="w-3 h-3 rounded-full" style={{ backgroundColor: PALETTE[p.color].main }} />
              <span style={{ color: '#F3EBDA' }} className="text-sm font-semibold">{COLOR_LABEL[p.color]}</span>
            </div>
            <div className="flex items-center gap-1">
              {p.pieces.map((pc) => (
                <span
                  key={pc.id}
                  className="w-2.5 h-2.5 rounded-full"
                  style={{
                    backgroundColor: pc.state === 'Finished' ? PALETTE[p.color].ring : pc.state === 'OnBoard' ? PALETTE[p.color].main : '#484F72',
                  }}
                  title={pc.state}
                />
              ))}
              <span style={{ color: '#8890B5' }} className="text-xs ml-1">{finished}/4</span>
            </div>
          </div>
        );
      })}
    </div>
  );
}
