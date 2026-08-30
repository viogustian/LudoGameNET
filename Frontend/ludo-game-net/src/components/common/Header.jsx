import { RotateCcw, Sparkles, Volume2, VolumeX } from 'lucide-react';

export default function Header({ screen, muted, onToggleMuted, onResetGame }) {
  return (
    <header className="w-full max-w-5xl flex items-center justify-between mb-5">
      <div className="flex items-center gap-2">
        <div
          style={{ backgroundColor: '#D1A02E' }}
          className="w-9 h-9 rounded-xl flex items-center justify-center shadow-md"
        >
          <Sparkles size={18} color="#1E2438" />
        </div>
        <h1
          style={{ fontFamily: "'Baloo 2', sans-serif", color: '#F3EBDA' }}
          className="text-2xl sm:text-3xl font-extrabold tracking-tight"
        >
          Ludo
        </h1>
      </div>
      <div className="flex items-center gap-2">
        <button
          onClick={onToggleMuted}
          title={muted ? 'Aktifkan suara' : 'Bisukan suara'}
          className="cursor-target flex items-center justify-center w-9 h-9 rounded-lg transition"
          style={{ color: '#F3EBDA', backgroundColor: '#2C334E' }}
        >
          {muted ? <VolumeX size={16} /> : <Volume2 size={16} />}
        </button>
        {screen === 'game' && (
          <button
            onClick={onResetGame}
            className="cursor-target flex items-center gap-1.5 text-sm font-medium px-3 py-2 rounded-lg transition"
            style={{ color: '#F3EBDA', backgroundColor: '#2C334E' }}
          >
            <RotateCcw size={14} /> Game baru
          </button>
        )}
      </div>
    </header>
  );
}
