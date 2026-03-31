import { QueryClientProvider } from '@tanstack/react-query';
import { queryClient } from './lib/query-client';
import { AppShell } from './components/layout/AppShell';

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AppShell>
        <div className="max-w-4xl mx-auto">
          <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-8">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">
              Welcome to Agentic Company
            </h2>
            <p className="text-gray-600 mb-6">
              A multi-layered spec-driven platform for running companies with AI agents.
            </p>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="p-4 bg-blue-50 rounded-lg">
                <h3 className="font-semibold text-blue-900">🌳 Org Tree</h3>
                <p className="text-sm text-blue-700 mt-1">
                  Model your company hierarchy
                </p>
              </div>
              <div className="p-4 bg-green-50 rounded-lg">
                <h3 className="font-semibold text-green-900">📋 Specs</h3>
                <p className="text-sm text-green-700 mt-1">
                  Define what and why at every level
                </p>
              </div>
              <div className="p-4 bg-purple-50 rounded-lg">
                <h3 className="font-semibold text-purple-900">🤖 Agents</h3>
                <p className="text-sm text-purple-700 mt-1">
                  AI-assisted authoring and execution
                </p>
              </div>
            </div>
          </div>
        </div>
      </AppShell>
    </QueryClientProvider>
  );
}

export default App;
