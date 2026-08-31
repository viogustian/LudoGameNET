import { PALETTE_TINT } from '../../constants/colors.js';

export default function Cell({ meta, hasPiece }) {
  let bg = 'transparent';
  let content = null;

  if (meta.type === 'Yard') {
    bg = PALETTE_TINT[meta.color].main;
  } else if (meta.type === 'HomeStretch') {
    bg = PALETTE_TINT[meta.color].dark;
  } else if (meta.type === 'Safe') {
    bg = 'rgba(59, 36, 23, 0.12)';
    content = <span style={{ color: '#5B3F27', fontSize: '0.55rem' }}>★</span>;
  } else if (meta.type === 'Goal') {
    bg = PALETTE_TINT[meta.color].main;
  }

  const cellStyle = { background: bg, boxShadow: 'inset 0 0 0 1px rgba(59, 36, 23, 0.35)' };

  return (
    <div className="relative flex items-center justify-center" style={cellStyle}>
      {meta.type === 'Yard' && meta.holding && !hasPiece && (
        <span
          className="rounded-full"
          style={{ width: '55%', height: '55%', backgroundColor: 'rgba(59, 36, 23, 0.22)' }}
        />
      )}
      {content}
    </div>
  );
}
