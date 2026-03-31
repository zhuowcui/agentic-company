import type { ReactNode } from 'react';

interface AppShellProps {
  children: ReactNode;
  onNavigate?: (page: 'dashboard' | 'nodes') => void;
  currentPage?: string;
}

export function AppShell({ children, onNavigate, currentPage }: AppShellProps) {
  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="flex items-center justify-between">
          <h1 className="text-xl font-semibold text-gray-900">
            🏢 Agentic Company
          </h1>
          <nav className="flex gap-1">
            <button
              onClick={() => onNavigate?.('dashboard')}
              className={`px-3 py-1.5 text-sm rounded-lg transition-colors ${
                currentPage === 'dashboard'
                  ? 'bg-blue-100 text-blue-700'
                  : 'text-gray-600 hover:text-gray-900 hover:bg-gray-100'
              }`}
            >
              Dashboard
            </button>
            <button
              onClick={() => onNavigate?.('nodes')}
              className={`px-3 py-1.5 text-sm rounded-lg transition-colors ${
                currentPage === 'nodes'
                  ? 'bg-blue-100 text-blue-700'
                  : 'text-gray-600 hover:text-gray-900 hover:bg-gray-100'
              }`}
            >
              Organization
            </button>
          </nav>
        </div>
      </header>
      <main className="p-6">
        {children}
      </main>
    </div>
  );
}
