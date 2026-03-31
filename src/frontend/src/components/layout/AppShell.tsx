import type { ReactNode } from 'react';

export function AppShell({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-6 py-4">
        <div className="flex items-center justify-between">
          <h1 className="text-xl font-semibold text-gray-900">
            🏢 Agentic Company
          </h1>
          <nav className="flex gap-4 text-sm text-gray-600">
            <a href="/" className="hover:text-gray-900">Dashboard</a>
            <a href="/nodes" className="hover:text-gray-900">Organization</a>
          </nav>
        </div>
      </header>
      <main className="p-6">
        {children}
      </main>
    </div>
  );
}
