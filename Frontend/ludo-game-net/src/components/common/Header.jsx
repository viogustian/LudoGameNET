import { RotateCcw, Volume2, VolumeX, Info } from 'lucide-react';
import { WOOD_PANEL } from '../../constants/colors.js';

export default function Header({ screen, muted, onToggleMuted, onResetGame, onShowAbout }) {
  return (
    <header className="w-full max-w-5xl flex items-center justify-between mb-5">
      <div className="flex items-center gap-2">
        <img src="/logo.png" alt="Ludo" className="w-9 h-9 object-contain" />
        <h1
          style={{ fontFamily: "'Baloo 2', sans-serif", color: '#F3EBDA' }}
          className="text-2xl sm:text-3xl font-extrabold tracking-tight"
        >
          Wood Ludo
        </h1>
      </div>
      <div className="flex items-center gap-2">
        <button
          onClick={onShowAbout}
          title="Tentang"
          className="cursor-target flex items-center justify-center w-9 h-9 rounded-lg transition"
          style={{ color: '#F3EBDA', ...WOOD_PANEL }}
        >
          <Info size={16} />
        </button>
        <button
          onClick={onToggleMuted}
          title={muted ? 'Aktifkan suara' : 'Bisukan suara'}
          className="cursor-target flex items-center justify-center w-9 h-9 rounded-lg transition"
          style={{ color: '#F3EBDA', ...WOOD_PANEL }}
        >
          {muted ? <VolumeX size={16} /> : <Volume2 size={16} />}
        </button>
        {screen === 'game' && (
          <button
            onClick={onResetGame}
            className="cursor-target flex items-center gap-1.5 text-sm font-medium px-3 py-2 rounded-lg transition"
            style={{ color: '#F3EBDA', ...WOOD_PANEL }}
          >
            <RotateCcw size={14} /> Game baru
          </button>
        )}
      </div>
    </header>
  );
}
