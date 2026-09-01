import { X, Github, Linkedin } from 'lucide-react';
import { WOOD_PANEL } from '../../constants/colors.js';

const AUTHOR_NAME = 'Vio Gustian Nur Alamsyah';
const GITHUB_URL = 'https://github.com/viogustian';
const LINKEDIN_URL = 'https://www.linkedin.com/in/vio-gustian-nur-alamsyah-a29399213/';

export default function AboutModal({ open, onClose }) {
  if (!open) return null;

  return (
    <div
      className="fixed inset-0 flex items-center justify-center p-4"
      style={{ backgroundColor: 'rgba(15,18,30,0.75)', zIndex: 50 }}
      onClick={onClose}
    >
      <div
        className="rounded-2xl p-6 sm:p-8 text-center max-w-sm w-full relative"
        style={{ ...WOOD_PANEL }}
        onClick={(e) => e.stopPropagation()}
      >
        <button
          onClick={onClose}
          aria-label="Tutup"
          className="cursor-target absolute top-3 right-3 flex items-center justify-center w-8 h-8 rounded-lg"
          style={{ color: '#F3EBDA', backgroundColor: 'rgba(0,0,0,0.25)' }}
        >
          <X size={16} />
        </button>

        <img src="/logo.png" alt="Ludo" className="w-14 h-14 object-contain mx-auto mb-3" />

        <h3
          style={{ fontFamily: "'Baloo 2', sans-serif", color: '#F3EBDA' }}
          className="text-xl font-bold mb-1"
        >
          Wood Ludo
        </h3>
        <p style={{ color: '#8890B5' }} className="text-sm mb-5">
          Dibuat oleh
        </p>

        <p
          style={{ fontFamily: "'Baloo 2', sans-serif", color: '#D1A02E' }}
          className="text-lg font-bold mb-4"
        >
          {AUTHOR_NAME}
        </p>

        <div className="flex items-center justify-center gap-3">
          <a
            href={GITHUB_URL}
            target="_blank"
            rel="noopener noreferrer"
            className="cursor-target flex items-center gap-2 text-sm font-medium px-4 py-2 rounded-xl"
            style={{ backgroundColor: 'rgba(0,0,0,0.25)', color: '#F3EBDA' }}
          >
            <Github size={16} /> GitHub
          </a>
          <a
            href={LINKEDIN_URL}
            target="_blank"
            rel="noopener noreferrer"
            className="cursor-target flex items-center gap-2 text-sm font-medium px-4 py-2 rounded-xl"
            style={{ backgroundColor: 'rgba(0,0,0,0.25)', color: '#F3EBDA' }}
          >
            <Linkedin size={16} /> LinkedIn
          </a>
        </div>
      </div>
    </div>
  );
}
