import type { ReactNode } from 'react';
import { queryClient } from '../../lib/query-client';

interface AppShellProps {
  children: ReactNode;
  onNavigate?: (page: string) => void;
  currentPage?: string;
}

export function AppShell({ children, onNavigate, currentPage }: AppShellProps) {
  const token = localStorage.getItem('auth_token');

  const handleLogout = () => {
    localStorage.removeItem('auth_token');
    queryClient.clear();
    onNavigate?.('login');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="flex items-center justify-between">
          <h1 className="text-xl font-semibold text-gray-900">
            🏢 Agentic Company
          </h1>
          <div className="flex items-center gap-4">
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
              <button
                onClick={() => onNavigate?.('specs')}
                className={`px-3 py-1.5 text-sm rounded-lg transition-colors ${
                  currentPage === 'specs' || currentPage === 'spec-editor'
                    ? 'bg-blue-100 text-blue-700'
                    : 'text-gray-600 hover:text-gray-900 hover:bg-gray-100'
                }`}
              >
                Specs
              </button>
              <button
                onClick={() => onNavigate?.('plans')}
                className={`px-3 py-1.5 text-sm rounded-lg transition-colors ${
                  currentPage === 'plans'
                    ? 'bg-blue-100 text-blue-700'
                    : 'text-gray-600 hover:text-gray-900 hover:bg-gray-100'
                }`}
              >
                Plans
              </button>
              <button
                onClick={() => onNavigate?.('tasks')}
                className={`px-3 py-1.5 text-sm rounded-lg transition-colors ${
                  currentPage === 'tasks'
                    ? 'bg-blue-100 text-blue-700'
                    : 'text-gray-600 hover:text-gray-900 hover:bg-gray-100'
                }`}
              >
                Tasks
              </button>
            </nav>
            {token ? (
              <button
                onClick={handleLogout}
                className="px-3 py-1.5 text-sm text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors"
              >
                Sign Out
              </button>
            ) : (
              <button
                onClick={() => onNavigate?.('login')}
                className="px-3 py-1.5 text-sm text-blue-600 hover:bg-blue-50 rounded-lg transition-colors"
              >
                Sign In
              </button>
            )}
          </div>
        </div>
      </header>
      <main className="p-6">
        {children}
      </main>
    </div>
  );
}
