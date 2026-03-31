import { useState } from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from './lib/query-client';
import { AppShell } from './components/layout/AppShell';
import { NodesPage } from './features/nodes/NodesPage';
import { DashboardPage } from './features/dashboard/DashboardPage';

function App() {
  const [page, setPage] = useState<'dashboard' | 'nodes'>(
    window.location.hash === '#/nodes' ? 'nodes' : 'dashboard'
  );

  const navigate = (target: 'dashboard' | 'nodes') => {
    window.location.hash = target === 'dashboard' ? '#/' : '#/nodes';
    setPage(target);
  };

  return (
    <QueryClientProvider client={queryClient}>
      <AppShell onNavigate={navigate} currentPage={page}>
        {page === 'nodes' ? <NodesPage /> : <DashboardPage />}
      </AppShell>
    </QueryClientProvider>
  );
}

export default App;
