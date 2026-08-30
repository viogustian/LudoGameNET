import { PALETTE } from '../../constants/colors.js';
import { key } from '../../lib/boardGeometry.js';
import { BOARD_META } from '../../lib/boardGeometry.js';
import Cell from './Cell.jsx';
import PieceLayer from './PieceLayer.jsx';

const HUB_TRIANGLES = [
  { color: 'Red', clipPath: 'polygon(0% 0%, 0% 100%, 50% 50%)' },
  { color: 'Green', clipPath: 'polygon(0% 0%, 100% 0%, 50% 50%)' },
  { color: 'Yellow', clipPath: 'polygon(100% 0%, 100% 100%, 50% 50%)' },
  { color: 'Blue', clipPath: 'polygon(0% 100%, 100% 100%, 50% 50%)' },
];

export default function Board({ cellGroups, currentColor, validIds, onPieceClick, canClick }) {
  return (
    <div
      className="w-full max-w-[560px] rounded-2xl p-2 sm:p-3 shadow-2xl"
      style={{ backgroundColor: '#F3EBDA', border: '6px solid #2C334E' }}
    >
      <div className="relative w-full" style={{ aspectRatio: '1 / 1' }}>
        <div
          className="grid w-full h-full"
          style={{
            gridTemplateColumns: 'repeat(15, 1fr)',
            gridTemplateRows: 'repeat(15, 1fr)',
          }}
        >
          {BOARD_META.map((row, r) =>
            row.map((meta, c) => (
              <Cell key={key(r, c)} meta={meta} hasPiece={!!cellGroups[key(r, c)]} />
            ))
          )}
        </div>
        <div
          className="absolute pointer-events-none overflow-hidden rounded-sm"
          style={{ left: `${(6 / 15) * 100}%`, top: `${(6 / 15) * 100}%`, width: `${(3 / 15) * 100}%`, height: `${(3 / 15) * 100}%` }}
        >
          {HUB_TRIANGLES.map((t) => (
            <div
              key={t.color}
              className="absolute inset-0"
              style={{ backgroundColor: PALETTE[t.color].main, clipPath: t.clipPath }}
            />
          ))}
        </div>
        <PieceLayer
          cellGroups={cellGroups}
          currentColor={currentColor}
          validIds={validIds}
          onPieceClick={onPieceClick}
          canClick={canClick}
        />
      </div>
    </div>
  );
}