import { COLOR_LABEL, PALETTE } from '../../constants/colors.js';

/* PIECE LAYER — lapisan overlay absolut di atas grid papan. Posisi tiap
   bidak dihitung dari renderPositions/cellGroups (bukan langsung dari
   gameState), dan left/top-nya bertransisi lewat CSS supaya perpindahan
   selangkah demi selangkah terlihat seperti berjalan sampai ke tujuan. */

const QUADRANT_OFFSETS = [
  [-0.22, -0.22], [0.22, -0.22], [-0.22, 0.22], [0.22, 0.22],
];

export default function PieceLayer({ cellGroups, currentColor, validIds, onPieceClick, canClick }) {
  const cellPct = 100 / 15;

  return (
    <div className="absolute inset-0 pointer-events-none" style={{ zIndex: 10 }}>
      {Object.entries(cellGroups).map(([ck, pieceKeys]) => {
        const [row, col] = ck.split(',').map(Number);
        const count = pieceKeys.length;
        return pieceKeys.map((pk, idx) => {
          const [color, idStr] = pk.split('-');
          const id = Number(idStr);
          const movable = canClick && color === currentColor && validIds.has(id);

          let ox = 0, oy = 0, sizeFrac = 0.66;
          if (count > 1) {
            sizeFrac = 0.46;
            [ox, oy] = QUADRANT_OFFSETS[idx % 4];
          }

          const leftPct = (col + 0.5 + ox) * cellPct;
          const topPct = (row + 0.5 + oy) * cellPct;
          const sizePct = sizeFrac * cellPct;

          return (
            <div
              key={pk}
              className="absolute"
              style={{
                left: `${leftPct}%`,
                top: `${topPct}%`,
                width: `${sizePct}%`,
                height: `${sizePct}%`,
                transform: 'translate(-50%, -50%)',
                transition: 'left 0.2s ease-in-out, top 0.2s ease-in-out, width 0.15s ease, height 0.15s ease',
              }}
            >
              <span
                onClick={() => movable && onPieceClick(id)}
                className={`block w-full h-full rounded-full ${movable ? 'ludo-movable pointer-events-auto cursor-target' : ''}`}
                style={{
                  backgroundColor: PALETTE[color].main,
                  border: '1.5px solid #FFF8EC',
                  boxShadow: '0 1px 2px rgba(0,0,0,0.35)',
                }}
                title={`${COLOR_LABEL[color]} #${id + 1}`}
              />
            </div>
          );
        });
      })}
    </div>
  );
}
