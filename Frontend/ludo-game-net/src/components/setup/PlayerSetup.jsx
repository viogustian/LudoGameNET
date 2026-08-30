import { Loader2 } from 'lucide-react';
import { COLORS, COLOR_LABEL, PALETTE } from '../../constants/colors.js';

export default function PlayerSetup({ selectedColors, onToggleColor, onStart, busy }) {
  return (
    <div className="w-full max-w-md rounded-2xl p-6 sm:p-8 shadow-xl" style={{ backgroundColor: '#2C334E' }}>
      <h2
        style={{ fontFamily: "'Baloo 2', sans-serif", color: '#F3EBDA' }}
        className="text-xl font-bold mb-4"
      >
        Pilih 2–4 pemain untuk mulai
      </h2>

      <div className="grid grid-cols-2 gap-3 mb-5">
        {COLORS.map((c) => {
          const active = selectedColors.includes(c);
          return (
            <button
              key={c}
              onClick={() => onToggleColor(c)}
              className="cursor-target flex items-center gap-2.5 rounded-xl px-4 py-3 font-semibold text-sm transition"
              style={{
                backgroundColor: active ? PALETTE[c].main : '#232A44',
                color: active ? '#FFF8EC' : '#8890B5',
                boxShadow: active ? `0 0 0 2px ${PALETTE[c].ring}` : 'none',
              }}
            >
              <span
                className="w-4 h-4 rounded-full shrink-0"
                style={{ backgroundColor: active ? '#FFF8EC' : PALETTE[c].main }}
              />
              {COLOR_LABEL[c]}
            </button>
          );
        })}
      </div>

      <button
        onClick={onStart}
        disabled={selectedColors.length < 2 || busy}
        className="cursor-target w-full rounded-xl py-3 font-bold text-sm transition disabled:opacity-40 flex items-center justify-center gap-2"
        style={{ backgroundColor: '#D1A02E', color: '#1E2438', fontFamily: "'Baloo 2', sans-serif" }}
      >
        {busy ? <Loader2 size={16} className="animate-spin" /> : null}
        Mulai Permainan
      </button>
      {selectedColors.length < 2 && (
        <p className="text-xs mt-2 text-center" style={{ color: '#8890B5' }}>Pilih minimal 2 warna.</p>
      )}
    </div>
  );
}
