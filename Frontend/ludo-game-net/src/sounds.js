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

export function setMuted(value) {
  muted = value;
}

export function isMuted() {
  return muted;
}

/**
 * Memainkan efek suara berdasarkan nama key di SOUND_FILES.
 * Meng-clone elemen <audio> supaya suara yang tumpang tindih (mis. dua
 * capture beruntun) tetap bisa terdengar semuanya, bukan saling memotong.
 * Kalau file belum ada / gagal dimuat, gagal secara diam-diam (tidak
 * melempar error ke UI) supaya game tetap bisa dimainkan tanpa suara.
 */
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
    // Autoplay diblokir atau file tidak ada — abaikan saja.
  }
}
