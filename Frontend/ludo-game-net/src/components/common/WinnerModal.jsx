import { Crown } from 'lucide-react';
import { COLOR_LABEL, PALETTE } from '../../constants/colors.js';

export default function WinnerModal({ winnerColor, onPlayAgain }) {
  if (!winnerColor) return null;
  return (
    <div className="fixed inset-0 flex items-center justify-center p-4" style={{ backgroundColor: 'rgba(15,18,30,0.75)', zIndex: 50 }}>
      <div className="rounded-2xl p-8 text-center max-w-xs w-full" style={{ backgroundColor: '#2C334E' }}>
        <Crown size={40} color={PALETTE[winnerColor].ring} className="mx-auto mb-3" />
        <h3 style={{ fontFamily: "'Baloo 2', sans-serif", color: '#F3EBDA' }} className="text-xl font-bold mb-1">
          {COLOR_LABEL[winnerColor]} Menang!
        </h3>
        <p style={{ color: '#8890B5' }} className="text-sm mb-5">Semua bidak sudah sampai finish.</p>
        <button
          onClick={onPlayAgain}
          className="cursor-target w-full rounded-xl py-3 font-bold text-sm"
          style={{ backgroundColor: '#D1A02E', color: '#1E2438', fontFamily: "'Baloo 2', sans-serif" }}
        >
          Main Lagi
        </button>
      </div>
    </div>
  );
}
