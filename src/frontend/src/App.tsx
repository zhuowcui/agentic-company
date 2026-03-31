import { useState, useEffect } from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from './lib/query-client';
import { AppShell } from './components/layout/AppShell';
import { NodesPage } from './features/nodes/NodesPage';
import { DashboardPage } from './features/dashboard/DashboardPage';
import { SpecsPage } from './features/specs/SpecsPage';
import { SpecEditorPage } from './features/specs/SpecEditorPage';
import { LoginPage } from './features/auth/LoginPage';
import { RegisterPage } from './features/auth/RegisterPage';

type Page =
  | { name: 'dashboard'; nodeId?: string }
  | { name: 'nodes' }
  | { name: 'specs' }
  | { name: 'spec-editor'; specId?: string; nodeId?: string }
  | { name: 'login' }
  | { name: 'register' };

function parseHash(): Page {
  const hash = window.location.hash;
  if (hash.startsWith('#/login')) return { name: 'login' };
  if (hash.startsWith('#/register')) return { name: 'register' };
  if (hash.startsWith('#/specs/new')) {
    const params = new URLSearchParams(hash.split('?')[1] ?? '');
    return { name: 'spec-editor', nodeId: params.get('nodeId') ?? undefined };
  }
  if (hash.match(/^#\/specs\/[^/]+\/edit/)) {
    const specId = hash.split('/')[2];
    return { name: 'spec-editor', specId };
  }
  if (hash === '#/specs') return { name: 'specs' };
  if (hash === '#/nodes') return { name: 'nodes' };
  if (hash.startsWith('#/dashboard/')) {
    const nodeId = hash.split('/')[2];
    if (nodeId) return { name: 'dashboard', nodeId };
  }
  return { name: 'dashboard' };
}

function App() {
  const [page, setPage] = useState<Page>(parseHash);

  useEffect(() => {
    const onHashChange = () => setPage(parseHash());
    window.addEventListener('hashchange', onHashChange);
    return () => window.removeEventListener('hashchange', onHashChange);
  }, []);

  const navigate = (target: string) => {
    if (target === 'dashboard') {
      window.location.hash = '#/';
    } else if (target === 'login') {
      window.location.hash = '#/login';
    } else if (target === 'register') {
      window.location.hash = '#/register';
    } else if (target === 'nodes') {
      window.location.hash = '#/nodes';
    } else if (target === 'specs') {
      window.location.hash = '#/specs';
    } else if (target.startsWith('dashboard/')) {
      window.location.hash = `#/${target}`;
    } else if (target.startsWith('specs/new')) {
      window.location.hash = `#/${target}`;
    } else if (target.match(/^specs\/[^/]+\/edit/)) {
      window.location.hash = `#/${target}`;
    }
  };

  const renderPage = () => {
    switch (page.name) {
      case 'login':
        return <LoginPage onNavigate={navigate} />;
      case 'register':
        return <RegisterPage onNavigate={navigate} />;
      case 'nodes':
        return <NodesPage />;
      case 'specs':
        return <SpecsPage onNavigate={navigate} />;
      case 'spec-editor':
        return (
          <SpecEditorPage
            specId={page.specId}
            nodeId={page.nodeId}
            onNavigate={navigate}
          />
        );
      case 'dashboard':
        return <DashboardPage nodeId={page.nodeId} />;
    }
  };

  const isAuthPage = page.name === 'login' || page.name === 'register';

  return (
    <QueryClientProvider client={queryClient}>
      {isAuthPage ? (
        renderPage()
      ) : (
        <AppShell onNavigate={navigate} currentPage={page.name}>
          {renderPage()}
        </AppShell>
      )}
    </QueryClientProvider>
  );
}

export default App;
