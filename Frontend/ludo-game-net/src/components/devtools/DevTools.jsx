import { useState } from 'react';
import { Bug, X, RefreshCw, Copy, Check, Trash2, Dices, Flag, SkipForward, Wand2, RotateCcw } from 'lucide-react';
import { gameApi, API_BASE } from '../../api/gameApi.js';

/**
 * Floating debug panel, pinned to the bottom-right corner.
 *
 * Whether this renders at all is controlled by `DEV_TOOLS_ENABLED` in
 * `src/config/devtools.js` — App.jsx only mounts <DevTools /> when that
 * flag is true, so turning the tool off is a one-line change there.
 */
const PIECE_STATES = ['Base', 'OnBoard', 'Finished'];

export default function DevTools({ uiState }) {
  const [open, setOpen] = useState(false);
  const [tab, setTab] = useState('state');
  const [copied, setCopied] = useState(false);
  const [serverState, setServerState] = useState(null);
  const [serverError, setServerError] = useState('');
  const [fetching, setFetching] = useState(false);
  const [logs, setLogs] = useState([]);

  // --- devtools "actions" tab: manual dice + piece/turn overrides ---
  const [actionBusy, setActionBusy] = useState(false);
  const [actionError, setActionError] = useState('');
  const [diceStatus, setDiceStatus] = useState(null);
  const [diceInput, setDiceInput] = useState(6);
  const [diceLock, setDiceLock] = useState(false);
  const [actionColor, setActionColor] = useState('');
  const [forcePieceId, setForcePieceId] = useState(0);
  const [forceState, setForceState] = useState('OnBoard');
  const [forcePathIndex, setForcePathIndex] = useState(0);
  const [turnIndexInput, setTurnIndexInput] = useState(0);
  const [sixesInput, setSixesInput] = useState(2);

  const {
    screen, gameState, diceValue, diceDisplayValue, validPieces, rolling,
    busy, error, muted, canRoll, currentPlayer, cellGroups, selectedColors,
    COLORS, applyServerState,
  } = uiState;

  const snapshot = {
    screen, diceValue, diceDisplayValue, rolling, busy, error, muted,
    canRoll, selectedColors, currentPlayer, validPieces, cellGroups, gameState,
  };

  const gameColors = gameState?.players?.map((p) => p.color) || COLORS || [];
  const effectiveActionColor = actionColor || gameColors[0] || '';
  const maxPathIndex = 50; // TotalPathLength - 2 (51 common steps + 6 home stretch - 2)

  const pushLog = (label, payload) => {
    setLogs((prev) => [
      { id: Date.now(), time: new Date().toLocaleTimeString(), label, payload },
      ...prev,
    ].slice(0, 20));
  };

  // Runs a dev-tools API call, logs it, and — if the response looks like a
  // full GameStateDto (has `players`) — resyncs the whole UI from it.
  const runDevAction = async (label, fn) => {
    setActionBusy(true);
    setActionError('');
    try {
      const data = await fn();
      pushLog(label, data);
      if (data && Array.isArray(data.players)) {
        applyServerState(data);
      }
      if (data && ('forcedValue' in data || 'locked' in data)) {
        setDiceStatus(data);
      }
      return data;
    } catch (e) {
      setActionError(e.message);
      pushLog(`${label} (error)`, e.message);
      return null;
    } finally {
      setActionBusy(false);
    }
  };

  const handleFetchServerState = async () => {
    setFetching(true);
    setServerError('');
    try {
      const data = await gameApi.getState();
      setServerState(data);
      pushLog('GET /api/game', data);
    } catch (e) {
      setServerError(e.message);
      pushLog('GET /api/game (error)', e.message);
    } finally {
      setFetching(false);
    }
  };

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(JSON.stringify(snapshot, null, 2));
      setCopied(true);
      setTimeout(() => setCopied(false), 1200);
    } catch (e) {
      pushLog('Copy failed', e.message);
    }
  };

  const handleLogToConsole = () => {
    // eslint-disable-next-line no-console
    console.log('[DevTools] client state snapshot', snapshot);
    pushLog('Logged snapshot to console', null);
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
        {['state', 'actions', 'server', 'logs'].map((t) => (
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

        {tab === 'actions' && (
          <div className="flex flex-col gap-3">
            {!gameState && (
              <div className="text-slate-500">Mulai game dulu supaya ada state untuk dioprek.</div>
            )}
            {actionError && <div className="text-red-400">{actionError}</div>}

            {gameState && (
              <>
                {/* Dadu manual */}
                <section className="flex flex-col gap-1.5 border-b border-slate-800 pb-3">
                  <div className="flex items-center gap-1 text-amber-300"><Dices size={12} /> Dadu manual</div>
                  <div className="flex items-center gap-1.5">
                    <input
                      type="number" min={1} max={6} value={diceInput}
                      onChange={(e) => setDiceInput(Number(e.target.value))}
                      className="w-14 rounded bg-slate-800 px-1.5 py-1 text-slate-100"
                    />
                    <label className="flex items-center gap-1 text-slate-300">
                      <input type="checkbox" checked={diceLock} onChange={(e) => setDiceLock(e.target.checked)} />
                      kunci
                    </label>
                  </div>
                  <div className="flex flex-wrap gap-1.5">
                    <button
                      disabled={actionBusy}
                      onClick={() => runDevAction('Set dadu', () => gameApi.devSetDice(diceInput, diceLock))}
                      className="rounded bg-amber-700/70 px-2 py-1 text-slate-100 hover:bg-amber-600/70 disabled:opacity-50"
                    >
                      Set roll berikutnya
                    </button>
                    <button
                      disabled={actionBusy || busy}
                      onClick={async () => {
                        const ok = await runDevAction('Set dadu (lalu roll)', () => gameApi.devSetDice(diceInput, false));
                        if (ok) uiState.rollDice();
                      }}
                      className="rounded bg-slate-700 px-2 py-1 text-slate-100 hover:bg-slate-600 disabled:opacity-50"
                    >
                      Set &amp; roll sekarang
                    </button>
                    <button
                      disabled={actionBusy}
                      onClick={() => runDevAction('Clear dadu', () => gameApi.devClearDice())}
                      className="rounded bg-slate-700 px-2 py-1 text-slate-100 hover:bg-slate-600 disabled:opacity-50"
                    >
                      Clear
                    </button>
                  </div>
                  {diceStatus && (
                    <div className="text-slate-400">
                      forced: <span className="text-slate-200">{diceStatus.forcedValue ?? '—'}</span>{' '}
                      {diceStatus.locked ? '(terkunci)' : ''}
                    </div>
                  )}
                </section>

                {/* Aksi per warna */}
                <section className="flex flex-col gap-1.5 border-b border-slate-800 pb-3">
                  <div className="text-amber-300">Aksi per warna</div>
                  <select
                    value={effectiveActionColor}
                    onChange={(e) => setActionColor(e.target.value)}
                    className="rounded bg-slate-800 px-1.5 py-1 text-slate-100"
                  >
                    {gameColors.map((c) => <option key={c} value={c}>{c}</option>)}
                  </select>
                  <div className="flex flex-wrap gap-1.5">
                    <button
                      disabled={actionBusy}
                      onClick={() => runDevAction(`Enter-all ${effectiveActionColor}`, () => gameApi.devEnterAll(effectiveActionColor))}
                      className="flex items-center gap-1 rounded bg-sky-800/70 px-2 py-1 text-slate-100 hover:bg-sky-700/70 disabled:opacity-50"
                    >
                      <Wand2 size={12} /> Keluarkan semua dari base
                    </button>
                    <button
                      disabled={actionBusy}
                      onClick={() => runDevAction(`Finish-all ${effectiveActionColor}`, () => gameApi.devFinishAll(effectiveActionColor))}
                      className="flex items-center gap-1 rounded bg-emerald-800/70 px-2 py-1 text-slate-100 hover:bg-emerald-700/70 disabled:opacity-50"
                    >
                      <Flag size={12} /> Semua ke goal (menang)
                    </button>
                    <button
                      disabled={actionBusy}
                      onClick={() => runDevAction(`Reset-base ${effectiveActionColor}`, () => gameApi.devResetToBase(effectiveActionColor))}
                      className="flex items-center gap-1 rounded bg-slate-700 px-2 py-1 text-slate-100 hover:bg-slate-600 disabled:opacity-50"
                    >
                      <RotateCcw size={12} /> Reset ke base
                    </button>
                  </div>
                </section>

                {/* Teleport satu piece — untuk edge case bebas */}
                <section className="flex flex-col gap-1.5 border-b border-slate-800 pb-3">
                  <div className="text-amber-300">Teleport 1 piece (edge case bebas)</div>
                  <div className="flex flex-wrap items-center gap-1.5">
                    <select value={effectiveActionColor} onChange={(e) => setActionColor(e.target.value)} className="rounded bg-slate-800 px-1.5 py-1 text-slate-100">
                      {gameColors.map((c) => <option key={c} value={c}>{c}</option>)}
                    </select>
                    <select value={forcePieceId} onChange={(e) => setForcePieceId(Number(e.target.value))} className="rounded bg-slate-800 px-1.5 py-1 text-slate-100">
                      {[0, 1, 2, 3].map((id) => <option key={id} value={id}>piece #{id}</option>)}
                    </select>
                    <select value={forceState} onChange={(e) => setForceState(e.target.value)} className="rounded bg-slate-800 px-1.5 py-1 text-slate-100">
                      {PIECE_STATES.map((s) => <option key={s} value={s}>{s}</option>)}
                    </select>
                    {forceState === 'OnBoard' && (
                      <input
                        type="number" min={0} max={maxPathIndex} value={forcePathIndex}
                        onChange={(e) => setForcePathIndex(Number(e.target.value))}
                        title={`pathIndex (0-${maxPathIndex})`}
                        className="w-16 rounded bg-slate-800 px-1.5 py-1 text-slate-100"
                      />
                    )}
                  </div>
                  <button
                    disabled={actionBusy}
                    onClick={() => runDevAction(
                      `Force-piece ${effectiveActionColor}#${forcePieceId} → ${forceState}`,
                      () => gameApi.devForcePiece({
                        color: effectiveActionColor,
                        pieceId: forcePieceId,
                        state: forceState,
                        pathIndex: forceState === 'OnBoard' ? forcePathIndex : null,
                      }),
                    )}
                    className="self-start rounded bg-purple-800/70 px-2 py-1 text-slate-100 hover:bg-purple-700/70 disabled:opacity-50"
                  >
                    Terapkan
                  </button>
                </section>

                {/* Giliran & consecutive sixes */}
                <section className="flex flex-col gap-1.5">
                  <div className="flex items-center gap-1 text-amber-300"><SkipForward size={12} /> Giliran &amp; sixes</div>
                  <div className="flex flex-wrap items-center gap-1.5">
                    <select
                      value={turnIndexInput}
                      onChange={(e) => setTurnIndexInput(Number(e.target.value))}
                      className="rounded bg-slate-800 px-1.5 py-1 text-slate-100"
                    >
                      {gameState.players.map((p, idx) => (
                        <option key={p.id} value={idx}>#{idx} — {p.color}</option>
                      ))}
                    </select>
                    <button
                      disabled={actionBusy}
                      onClick={() => runDevAction(`Set-turn ${turnIndexInput}`, () => gameApi.devSetTurn(turnIndexInput))}
                      className="rounded bg-slate-700 px-2 py-1 text-slate-100 hover:bg-slate-600 disabled:opacity-50"
                    >
                      Loncat giliran
                    </button>
                  </div>
                  <div className="flex flex-wrap items-center gap-1.5">
                    <input
                      type="number" min={0} value={sixesInput}
                      onChange={(e) => setSixesInput(Number(e.target.value))}
                      className="w-16 rounded bg-slate-800 px-1.5 py-1 text-slate-100"
                    />
                    <button
                      disabled={actionBusy}
                      onClick={() => runDevAction(`Set consecutive sixes = ${sixesInput}`, () => gameApi.devSetSixes(sixesInput))}
                      className="rounded bg-slate-700 px-2 py-1 text-slate-100 hover:bg-slate-600 disabled:opacity-50"
                    >
                      Set consecutiveSixes (tes forfeit di 6 ke-3)
                    </button>
                  </div>
                </section>
              </>
            )}
          </div>
        )}

        {tab === 'server' && (
          <div className="flex flex-col gap-2">
            <div className="text-slate-400">API_BASE: <span className="text-slate-200">{API_BASE}</span></div>
            {serverError && <div className="text-red-400">{serverError}</div>}
            {serverState && (
              <pre className="whitespace-pre-wrap break-words text-sky-300">
                {JSON.stringify(serverState, null, 2)}
              </pre>
            )}
            {!serverState && !serverError && (
              <div className="text-slate-500">No fetch yet — hit “Fetch server state” below.</div>
            )}
          </div>
        )}

        {tab === 'logs' && (
          <div className="flex flex-col gap-2">
            {logs.length === 0 && <div className="text-slate-500">No actions logged yet.</div>}
            {logs.map((l) => (
              <div key={l.id} className="border-b border-slate-800 pb-1">
                <div className="text-slate-400">{l.time} — <span className="text-amber-300">{l.label}</span></div>
                {l.payload != null && (
                  <pre className="whitespace-pre-wrap break-words text-slate-300">
                    {typeof l.payload === 'string' ? l.payload : JSON.stringify(l.payload, null, 2)}
                  </pre>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="flex flex-wrap gap-1.5 border-t border-slate-700 bg-slate-800/60 p-2">
        <button
          onClick={handleFetchServerState}
          disabled={fetching}
          className="flex items-center gap-1 rounded bg-slate-700 px-2 py-1 text-[11px] text-slate-100 hover:bg-slate-600 disabled:opacity-50"
        >
          <RefreshCw size={12} className={fetching ? 'animate-spin' : ''} />
          Fetch server state
        </button>
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
        <button
          onClick={() => setLogs([])}
          className="flex items-center gap-1 rounded bg-slate-700 px-2 py-1 text-[11px] text-slate-100 hover:bg-slate-600"
        >
          <Trash2 size={12} />
          Clear logs
        </button>
      </div>
    </div>
  );
}
