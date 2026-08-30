import { PALETTE } from '../../constants/colors.js';

export default function Cell({ meta, hasPiece }) {
  let bg = '#F3EBDA';
  let content = null;

  if (meta.type === 'Yard') {
    bg = PALETTE[meta.color].main;
  } else if (meta.type === 'HomeStretch') {
    bg = PALETTE[meta.color].dark;
  } else if (meta.type === 'Safe') {
    bg = '#EDE2C4';
    content = <span style={{ color: '#B7A76C', fontSize: '0.55rem' }}>★</span>;
  } else if (meta.type === 'Goal') {
    bg = PALETTE[meta.color].main;
  }

  const cellStyle =
    meta.type === 'Yard'
      ? { background: bg }
      : { background: bg, boxShadow: 'inset 0 0 0 1px #D8CBA8' };

  return (
    <div className="relative flex items-center justify-center" style={cellStyle}>
      {meta.type === 'Yard' && meta.holding && !hasPiece && (
        <span
          className="rounded-full"
          style={{ width: '55%', height: '55%', backgroundColor: 'rgba(255,255,255,0.35)' }}
        />
      )}
      {content}
    </div>
  );
}