import { CENTER, YARD_HOLDING_POINTS } from '../constants/board.js';
import { PATHS } from './boardGeometry.js';

export function piecePosition(piece) {
  if (piece.state === 'Base') {
    return YARD_HOLDING_POINTS[piece.color][piece.id % 4];
  }
  return PATHS[piece.color][piece.pathIndex] || CENTER;
}

export function isPieceOnBoard(piece) {
  return piece.state !== 'Finished';
}

export function pieceKey(color, id) {
  return `${color}-${id}`;
}

export const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

export function computeWalkSteps(color, before, after) {
  if (!before || !after) return [];
  if (before.state === 'Base') {
    if (after.state === 'OnBoard') return [PATHS[color][after.pathIndex]];
    if (after.state === 'Finished') return [PATHS[color][PATHS[color].length - 1]];
    return [];
  }
  if (before.state === 'OnBoard') {
    const fromIdx = before.pathIndex;
    const toIdx = after.state === 'Finished' ? PATHS[color].length - 1 : after.pathIndex;
    const steps = [];
    for (let i = fromIdx + 1; i <= toIdx; i++) steps.push(PATHS[color][i]);
    return steps;
  }
  return [];
}
