import { useEffect, useRef, useState } from 'react';

const BASE_ROTATION = {
  1: { x: 0, y: 0 },
  2: { x: -90, y: 0 },
  3: { x: 0, y: -90 },
  4: { x: 0, y: 90 },
  5: { x: 90, y: 0 },
  6: { x: 0, y: 180 },
};

function shortestPositiveDelta(target, current) {
  const cur = ((current % 360) + 360) % 360;
  const tgt = ((target % 360) + 360) % 360;
  let delta = tgt - cur;
  if (delta < 0) delta += 360;
  return delta;
}

export default function Dice3D({ value, rollToken, size = 58, clickable = false, onClick }) {
  const diceRef = useRef(null);
  const bounceRef = useRef(null);
  const tiltRef = useRef(null);
  const shadowRef = useRef(null);
  const rotationRef = useRef({ x: 0, y: 0 });
  const firstRun = useRef(true);
  const [rolling, setRolling] = useState(false);

  useEffect(() => {
    // Mount pertama (belum pernah roll) — jangan animasikan apa-apa.
    if (firstRun.current) {
      firstRun.current = false;
      return;
    }
    if (value == null || !diceRef.current) return;

    const base = BASE_ROTATION[value];
    const spinsX = 2 + Math.floor(Math.random() * 3); // 2–4 putaran penuh ekstra
    const spinsY = 2 + Math.floor(Math.random() * 3);
    const deltaX = shortestPositiveDelta(base.x, rotationRef.current.x);
    const deltaY = shortestPositiveDelta(base.y, rotationRef.current.y);

    rotationRef.current = {
      x: rotationRef.current.x + spinsX * 360 + deltaX,
      y: rotationRef.current.y + spinsY * 360 + deltaY,
    };
    diceRef.current.style.transform = `rotateX(${rotationRef.current.x}deg) rotateY(${rotationRef.current.y}deg)`;

    setRolling(true);
    [bounceRef, tiltRef, shadowRef].forEach((r) => r.current?.classList.remove('dice3d-rolling'));
    void bounceRef.current?.offsetWidth; // restart animasi CSS (bounce/tilt/shadow)
    [bounceRef, tiltRef, shadowRef].forEach((r) => r.current?.classList.add('dice3d-rolling'));

    const node = diceRef.current;
    const handleEnd = (e) => {
      if (e.propertyName !== 'transform') return;
      setRolling(false);
    };
    node.addEventListener('transitionend', handleEnd);
    return () => node.removeEventListener('transitionend', handleEnd);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [rollToken]);

  return (
    <div
      className="dice3d-scene"
      style={{ '--dice3d-size': `${size}px` }}
      title={rolling ? undefined : value ?? undefined}
    >
      <style>{DICE3D_CSS}</style>
      <div ref={bounceRef} className="dice3d-bounce-layer">
        <div ref={tiltRef} className="dice3d-tilt-layer">
          <div
            ref={diceRef}
            onClick={clickable ? onClick : undefined}
            className={`dice3d-cube${clickable ? ' dice3d-clickable cursor-target' : ''}`}
          >
            <div className="dice3d-face dice3d-front"><span className="dice3d-pip" style={{ gridArea: 'e' }} /></div>
            <div className="dice3d-face dice3d-top">
              <span className="dice3d-pip" style={{ gridArea: 'a' }} />
              <span className="dice3d-pip" style={{ gridArea: 'i' }} />
            </div>
            <div className="dice3d-face dice3d-right">
              <span className="dice3d-pip" style={{ gridArea: 'a' }} />
              <span className="dice3d-pip" style={{ gridArea: 'e' }} />
              <span className="dice3d-pip" style={{ gridArea: 'i' }} />
            </div>
            <div className="dice3d-face dice3d-left">
              <span className="dice3d-pip" style={{ gridArea: 'a' }} />
              <span className="dice3d-pip" style={{ gridArea: 'c' }} />
              <span className="dice3d-pip" style={{ gridArea: 'g' }} />
              <span className="dice3d-pip" style={{ gridArea: 'i' }} />
            </div>
            <div className="dice3d-face dice3d-bottom">
              <span className="dice3d-pip" style={{ gridArea: 'a' }} />
              <span className="dice3d-pip" style={{ gridArea: 'c' }} />
              <span className="dice3d-pip" style={{ gridArea: 'e' }} />
              <span className="dice3d-pip" style={{ gridArea: 'g' }} />
              <span className="dice3d-pip" style={{ gridArea: 'i' }} />
            </div>
            <div className="dice3d-face dice3d-back">
              <span className="dice3d-pip" style={{ gridArea: 'a' }} />
              <span className="dice3d-pip" style={{ gridArea: 'd' }} />
              <span className="dice3d-pip" style={{ gridArea: 'g' }} />
              <span className="dice3d-pip" style={{ gridArea: 'c' }} />
              <span className="dice3d-pip" style={{ gridArea: 'f' }} />
              <span className="dice3d-pip" style={{ gridArea: 'i' }} />
            </div>
          </div>
          {value == null && <span className="dice3d-idle-mark">?</span>}
        </div>
      </div>
      <div ref={shadowRef} className="dice3d-ground-shadow" />
    </div>
  );
}

const DICE3D_CSS = `
.dice3d-scene {
  --die: var(--dice3d-size, 58px);
  perspective: 900px;
  width: calc(var(--die) * 1.9);
  height: calc(var(--die) * 1.9);
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  flex-shrink: 0;
}
.dice3d-bounce-layer { width: var(--die); height: var(--die); transform-style: preserve-3d; }
.dice3d-bounce-layer.dice3d-rolling { animation: dice3d-bounce 1.3s cubic-bezier(.36,.07,.19,.97); }
.dice3d-tilt-layer {
  width: 100%; height: 100%; transform-style: preserve-3d;
  transform: rotateX(0deg) rotateY(0deg); position: relative;
}
.dice3d-tilt-layer.dice3d-rolling { animation: dice3d-tilt-tumble 1.3s cubic-bezier(.22,.9,.34,1) forwards; }
.dice3d-cube {
  width: 100%; height: 100%; position: relative; transform-style: preserve-3d;
  transform: rotateX(0deg) rotateY(0deg);
  transition: transform 1.3s cubic-bezier(.22,.9,.34,1);
}
.dice3d-cube.dice3d-clickable { cursor: pointer; }
.dice3d-face {
  position: absolute; width: var(--die); height: var(--die);
  display: grid; grid-template-columns: repeat(3, 1fr); grid-template-rows: repeat(3, 1fr);
  grid-template-areas: "a b c" "d e f" "g h i";
  padding: 16%; border-radius: 16%;
  background: linear-gradient(155deg, #faf3e3, #e2d2ac);
  box-shadow:
    inset 0 0 0 1px rgba(0,0,0,0.08),
    inset 0 6px 14px rgba(255,255,255,0.55),
    inset 0 -10px 18px rgba(120,95,45,0.22);
}
.dice3d-pip {
  width: 100%; aspect-ratio: 1; border-radius: 50%; justify-self: center; align-self: center;
  background: radial-gradient(circle at 35% 30%, #3c3325, #17130c 75%);
  box-shadow: inset 0 1px 2px rgba(0,0,0,0.6), 0 1px 0 rgba(255,255,255,0.15);
}
.dice3d-front  { transform: translateZ(calc(var(--die) / 2)); }
.dice3d-back   { transform: rotateY(180deg) translateZ(calc(var(--die) / 2)); }
.dice3d-right  { transform: rotateY(90deg) translateZ(calc(var(--die) / 2)); }
.dice3d-left   { transform: rotateY(-90deg) translateZ(calc(var(--die) / 2)); }
.dice3d-top    { transform: rotateX(90deg) translateZ(calc(var(--die) / 2)); }
.dice3d-bottom { transform: rotateX(-90deg) translateZ(calc(var(--die) / 2)); }
.dice3d-ground-shadow {
  position: absolute; bottom: calc(50% - var(--die) * 0.95);
  width: calc(var(--die) * 0.85); height: calc(var(--die) * 0.18);
  border-radius: 50%;
  background: radial-gradient(closest-side, rgba(0,0,0,0.55), transparent 72%);
  filter: blur(2px);
}
.dice3d-ground-shadow.dice3d-rolling { animation: dice3d-shadow-pulse 1.3s ease; }
.dice3d-idle-mark {
  position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
  font-weight: 700; font-size: calc(var(--die) * 0.4); color: #B7A76C; pointer-events: none;
}
@keyframes dice3d-bounce {
  0% { transform: translateY(0); }
  15% { transform: translateY(-34px); }
  30% { transform: translateY(0); }
  42% { transform: translateY(-16px); }
  54% { transform: translateY(0); }
  64% { transform: translateY(-6px); }
  74% { transform: translateY(0); }
  100% { transform: translateY(0); }
}
@keyframes dice3d-tilt-tumble {
  0% { transform: rotateX(0deg) rotateY(0deg); }
  10% { transform: rotateX(-18deg) rotateY(-30deg); }
  65% { transform: rotateX(-18deg) rotateY(-30deg); }
  85% { transform: rotateX(-6deg) rotateY(-10deg); }
  100% { transform: rotateX(0deg) rotateY(0deg); }
}
@keyframes dice3d-shadow-pulse {
  0% { transform: scale(1); opacity: 0.6; }
  15% { transform: scale(0.55); opacity: 0.25; }
  30% { transform: scale(1); opacity: 0.6; }
  100% { transform: scale(1); opacity: 0.6; }
}
@media (prefers-reduced-motion: reduce) {
  .dice3d-cube, .dice3d-bounce-layer.dice3d-rolling, .dice3d-tilt-layer.dice3d-rolling, .dice3d-ground-shadow.dice3d-rolling {
    transition: none !important;
    animation: none !important;
  }
}
`;
