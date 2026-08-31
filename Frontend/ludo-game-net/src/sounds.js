/**
 * Sound manager sederhana untuk efek suara game Ludo.
 *
 * Taruh file audio kamu di `public/sfx/` dengan nama persis seperti pada
 * objek SOUND_FILES di bawah (format .mp3 direkomendasikan). Vite otomatis
 * menyajikan isi folder `public/` di root, jadi `public/sfx/dice-roll.mp3`
 * bisa diakses browser lewat path `/sfx/dice-roll.mp3` — tidak perlu import.
 */

export const SOUND_FILES = {
  start: '/sfx/game-start.mp3',    // dimainkan saat game baru dimulai
  diceRoll: '/sfx/dice-roll.mp3',  // dimainkan saat tombol "Lempar Dadu" ditekan
  move: '/sfx/piece-move.mp3',     // dimainkan setelah bidak berhasil melangkah
  capture: '/sfx/piece-capture.mp3', // dimainkan saat bidak lawan tertangkap
  finish: '/sfx/piece-finish.mp3', // dimainkan saat bidak sampai finish
  win: '/sfx/victory.mp3',         // dimainkan saat ada pemenang
};

// Musik latar (background music). Taruh/ganti file di `public/sfx/bgm.mp3`
// — tidak perlu ubah kode apa pun, tinggal timpa filenya.
export const BGM_FILE = '/sfx/bgm.mp3';

const cache = {};

function getAudio(name) {
  if (!cache[name]) {
    const audio = new Audio(SOUND_FILES[name]);
    audio.preload = 'auto';
    cache[name] = audio;
  }
  return cache[name];
}

let muted = false;

let bgmAudio = null;
let bgmVolume = 0.35;

function getBgmAudio() {
  if (!bgmAudio) {
    bgmAudio = new Audio(BGM_FILE);
    bgmAudio.loop = true;
    bgmAudio.preload = 'auto';
  }
  return bgmAudio;
}

export function playBgm({ volume = bgmVolume } = {}) {
  bgmVolume = volume;
  try {
    const audio = getBgmAudio();
    audio.volume = muted ? 0 : bgmVolume;
    if (audio.paused) {
      audio.play().catch(() => {

      });
    }
  } catch (e) {
  }
}

export function stopBgm() {
  if (!bgmAudio) return;
  bgmAudio.pause();
  bgmAudio.currentTime = 0;
}

export function pauseBgm() {
  if (!bgmAudio) return;
  bgmAudio.pause();
}

export function setMuted(value) {
  muted = value;
  if (bgmAudio) bgmAudio.volume = muted ? 0 : bgmVolume;
}

export function isMuted() {
  return muted;
}

export function playSound(name, { volume = 1 } = {}) {
  if (muted) return;
  const source = SOUND_FILES[name];
  if (!source) return;
  try {
    const base = getAudio(name);
    const node = base.cloneNode();
    node.volume = volume;
    node.play().catch(() => {});
  } catch (e) {
  }
}