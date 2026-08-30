import { Loader2 } from 'lucide-react';
import { COLOR_LABEL, PALETTE } from '../../constants/colors.js';
import Dice3D from '../dice/Dice3D.jsx';

export default function TurnPanel({ currentPlayer, diceDisplayValue, rollToken, rolling, busy, canRoll, onRollDice, showHint }) {
  return (
    <div className="rounded-2xl p-4" style={{ backgroundColor: '#2C334E' }}>
      <p style={{ color: '#8890B5' }} className="text-xs font-medium mb-2 uppercase tracking-wide">Giliran</p>
      <div className="flex items-center gap-2 mb-4">
        <span
          className="w-3.5 h-3.5 rounded-full"
          style={{ backgroundColor: currentPlayer ? PALETTE[currentPlayer.color].main : '#888' }}
        />
        <span style={{ fontFamily: "'Baloo 2', sans-serif", color: '#F3EBDA' }} className="text-lg font-bold">
          {currentPlayer ? COLOR_LABEL[currentPlayer.color] : '—'}
        </span>
      </div>

      <div className="flex items-center gap-4">
        <Dice3D
          value={diceDisplayValue}
          rollToken={rollToken}
          clickable={canRoll}
          onClick={onRollDice}
        />
        <button
          onClick={onRollDice}
          disabled={!canRoll}
          className="cursor-target flex-1 rounded-xl py-3 font-bold text-sm disabled:opacity-40 flex items-center justify-center gap-2"
          style={{ backgroundColor: '#D1A02E', color: '#1E2438', fontFamily: "'Baloo 2', sans-serif" }}
        >
          {busy && !rolling ? <Loader2 size={15} className="animate-spin" /> : null}
          Lempar Dadu
        </button>
      </div>
      {showHint && (
        <p className="text-xs mt-3" style={{ color: '#8890B5' }}>
          Ketuk bidak yang berdenyut di papan untuk menjalankannya.
        </p>
      )}
    </div>
  );
}
