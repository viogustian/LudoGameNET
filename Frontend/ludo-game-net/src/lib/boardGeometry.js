import { COLORS } from "../constants/colors";
import {
  COMMON_PATH,
  START_OFFSETS,
  HOME_STRETCHES,
  YARD_REGIONS,
  YARD_HOLDING_POINTS,
} from '../constants/board.js';

export const key = (r, c) => `${r}, ${c}`;

function buildPathForColor(color) {
  const offset = START_OFFSETS[color];
  const path = [];
  for (let i = 0; i < 51; i++) path.push(COMMON_PATH[(offset + i) % 52]);
  HOME_STRETCHES[color].forEach((p) => path.push(p));
  return path;
}

export const PATHS = Object.fromEntries(COLORS.map((c) => [c, buildPathForColor(c)]));

const COMMON_PATH_SET = new Set(COMMON_PATH.map(([r, c]) => key(r, c)));

const SAFE_SET = new Set(
  COLORS.flatMap((c) => {
    const off = START_OFFSETS[c];
    return [COMMON_PATH[off % 52], COMMON_PATH[(off + 8) % 52]].map(([r, cc]) => key(r, cc));
  })
);

const HOME_STRETCH_MAP = new Map();
const GOAL_MAP = new Map();
COLORS.forEach((c) => {
  const stretch = HOME_STRETCHES[c];
  stretch.forEach(([r, cc], i) => {
    const k = key(r, cc);
    if (i === stretch.length - 1) GOAL_MAP.set(k, c);
    else HOME_STRETCH_MAP.set(k, c);
  });
});

const HOLDING_SET = new Map();
COLORS.forEach((c) => YARD_HOLDING_POINTS[c].forEach(([r, cc]) => HOLDING_SET.set(key(r, cc), c)));

export function cellMeta(row, col) {
  const k = key(row, col);
  if (GOAL_MAP.has(k)) return { type: 'Goal', color: GOAL_MAP.get(k) };
  if (HOME_STRETCH_MAP.has(k)) return { type: 'HomeStretch', color: HOME_STRETCH_MAP.get(k) };
  if (COMMON_PATH_SET.has(k)) return { type: SAFE_SET.has(k) ? 'Safe' : 'Common' };
  for (const c of COLORS) {
    const [r0, r1, c0, c1] = YARD_REGIONS[c];
    if (row >= r0 && row <= r1 && col >= c0 && col <= c1) {
      return { type: 'Yard', color: c, holding: HOLDING_SET.get(k) === c };
    }
  }
  return { type: 'Common' };
}

export const BOARD_META = Array.from({ length: 15 }, (_, r) =>
  Array.from({ length: 15 }, (_, c) => cellMeta(r, c))
);
