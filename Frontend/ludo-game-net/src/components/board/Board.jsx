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
      style={{
        backgroundImage: 'url(/textures/wood.jpg)',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        border: '8px solid #3B2417',
        boxShadow: 'inset 0 0 0 2px rgba(0,0,0,0.25), 0 10px 30px rgba(0,0,0,0.45)',
      }}
    >
      <div
        className="relative w-full rounded-sm overflow-hidden"
        style={{ aspectRatio: '1 / 1', boxShadow: 'inset 0 0 0 2px rgba(59, 36, 23, 0.55)' }}
      >
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
          style={{
            left: `${(6 / 15) * 100}%`,
            top: `${(6 / 15) * 100}%`,
            width: `${(3 / 15) * 100}%`,
            height: `${(3 / 15) * 100}%`,
            boxShadow: 'inset 0 0 0 1px rgba(59, 36, 23, 0.55)',
          }}
        >
          {HUB_TRIANGLES.map((t) => (
            <div
              key={t.color}
              className="absolute inset-0"
              style={{ backgroundColor: PALETTE[t.color].main, opacity: 0.9, clipPath: t.clipPath }}
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
