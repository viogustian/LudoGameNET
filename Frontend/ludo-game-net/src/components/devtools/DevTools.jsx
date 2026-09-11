import { useState } from 'react';
import { Bug, X, Copy, Check } from 'lucide-react';

export default function DevTools({ uiState }) {
  const [open, setOpen] = useState(false);
  const [tab, setTab] = useState('state');
  const [copied, setCopied] = useState(false);

  const snapshot = { ...uiState };

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(JSON.stringify(snapshot, null, 2));
      setCopied(true);
      setTimeout(() => setCopied(false), 1200);
    } catch (e) {
      console.error('Copy failed', e.message);
    }
  };

  const handleLogToConsole = () => {
    console.log('[DevTools] client state snapshot', snapshot);
  };

  if (!open) {
    return (
      <button
        type="button"
        onClick={() => setOpen(true)}
        title="Open dev tools"
        className="fixed bottom-4 right-4 z-50 flex h-11 w-11 items-center justify-center rounded-full bg-slate-800 text-amber-300 shadow-lg ring-1 ring-slate-600 hover:bg-slate-700 transition-colors"
      >
        <Bug size={20} />
      </button>
    );
  }

  return (
    <div
      className="fixed bottom-4 right-4 z-50 flex w-[360px] max-w-[92vw] flex-col overflow-hidden rounded-xl border border-slate-600 bg-slate-900/95 text-slate-100 shadow-2xl backdrop-blur"
      style={{ fontFamily: "'JetBrains Mono', monospace" }}
    >
      <div className="flex items-center justify-between bg-slate-800 px-3 py-2">
        <div className="flex items-center gap-2 text-xs font-semibold tracking-wide text-amber-300">
          <Bug size={14} />
          DEV TOOLS
        </div>
        <button onClick={() => setOpen(false)} className="text-slate-400 hover:text-white">
          <X size={16} />
        </button>
      </div>

      <div className="flex border-b border-slate-700 text-[11px]">
        {['state'].map((t) => (
          <button
            key={t}
            onClick={() => setTab(t)}
            className={`flex-1 px-2 py-1.5 uppercase tracking-wide ${
              tab === t ? 'bg-slate-800 text-amber-300' : 'text-slate-400 hover:text-slate-200'
            }`}
          >
            {t}
          </button>
        ))}
      </div>

      <div className="max-h-96 overflow-y-auto p-3 text-[11px] leading-snug">
        {tab === 'state' && (
          <pre className="whitespace-pre-wrap break-words text-emerald-300">
            {JSON.stringify(snapshot, null, 2)}
          </pre>
        )}
      </div>

      <div className="flex flex-wrap gap-1.5 border-t border-slate-700 bg-slate-800/60 p-2">
        <button
          onClick={handleCopy}
          className="flex items-center gap-1 rounded bg-slate-700 px-2 py-1 text-[11px] text-slate-100 hover:bg-slate-600"
        >
          {copied ? <Check size={12} /> : <Copy size={12} />}
          {copied ? 'Copied' : 'Copy snapshot'}
        </button>
        <button
          onClick={handleLogToConsole}
          className="flex items-center gap-1 rounded bg-slate-700 px-2 py-1 text-[11px] text-slate-100 hover:bg-slate-600"
        >
          Log to console
        </button>
      </div>
    </div>
  );
}
