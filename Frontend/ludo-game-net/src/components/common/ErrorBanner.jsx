import { AlertTriangle } from 'lucide-react';

export default function ErrorBanner({ message }) {
  if (!message) return null;
  return (
    <div
      className="w-full max-w-5xl mb-4 flex items-start gap-2 rounded-lg px-4 py-3 text-sm"
      style={{ backgroundColor: '#3A2530', color: '#F0B4B4', border: '1px solid #6B3A3A' }}
    >
      <AlertTriangle size={16} className="mt-0.5 shrink-0" />
      <span>{message}</span>
    </div>
  );
}
