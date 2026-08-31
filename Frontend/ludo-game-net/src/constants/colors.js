export const COLORS = ['Red', 'Green', 'Yellow', 'Blue'];
export const COLOR_LABEL = { Red: 'Merah', Green: 'Hijau', Yellow: 'Kuning', Blue: 'Biru' };

function hexToRgba(hex, alpha) {
  const h = hex.replace('#', '');
  const r = parseInt(h.substring(0, 2), 16);
  const g = parseInt(h.substring(2, 4), 16);
  const b = parseInt(h.substring(4, 6), 16);
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}

export const PALETTE = {
  Red: { main: '#C0503C', dark: '#8A3527', soft: '#E9C9BF', ring: '#F0A98F' },
  Green: { main: '#3E7A52', dark: '#245536', soft: '#C9DFCB', ring: '#8FCB9C' },
  Yellow: { main: '#D1A02E', dark: '#93711A', soft: '#EFDDAE', ring: '#F0CD6E' },
  Blue: { main: '#3A6690', dark: '#254864', soft: '#C6D6E4', ring: '#8FB4D6' },
};

/* Versi rgba dari PALETTE, dipakai sebagai overlay semi-transparan di atas
   texture kayu supaya serat kayunya tetap kelihatan menembus warnanya. */
export const PALETTE_TINT = Object.fromEntries(
  Object.entries(PALETTE).map(([color, p]) => [
    color,
    { main: hexToRgba(p.main, 0.62), dark: hexToRgba(p.dark, 0.72) },
  ])
);

/* Style panel bertekstur kayu, pengganti flat navy (#2C334E) di kartu-kartu
   UI (header, giliran, daftar pemain, setup, modal menang). Overlay coklat
   gelap di atas texture supaya teks krem tetap kontras dan gampang dibaca. */
export const WOOD_PANEL = {
  backgroundImage:
    'linear-gradient(rgba(46, 28, 18, 0.74), rgba(46, 28, 18, 0.74)), url(/textures/wood.jpg)',
  backgroundSize: 'cover',
  backgroundPosition: 'center',
};

/* Varian lebih gelap, dipakai untuk state tidak-aktif/tidak-terpilih
   (tombol warna yang belum dipilih, baris pemain yang bukan giliran). */
export const WOOD_PANEL_INACTIVE = {
  backgroundImage:
    'linear-gradient(rgba(22, 14, 9, 0.86), rgba(22, 14, 9, 0.86)), url(/textures/wood.jpg)',
  backgroundSize: 'cover',
  backgroundPosition: 'center',
};
