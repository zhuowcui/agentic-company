export function DashboardPage() {
  return (
    <div className="max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Dashboard</h1>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <h3 className="text-sm font-semibold text-gray-500 uppercase">Nodes</h3>
          <p className="text-3xl font-bold text-gray-900 mt-2">—</p>
          <p className="text-sm text-gray-500 mt-1">Total organizational units</p>
        </div>
        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <h3 className="text-sm font-semibold text-gray-500 uppercase">Specs</h3>
          <p className="text-3xl font-bold text-gray-900 mt-2">—</p>
          <p className="text-sm text-gray-500 mt-1">Active specifications</p>
        </div>
        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <h3 className="text-sm font-semibold text-gray-500 uppercase">Tasks</h3>
          <p className="text-3xl font-bold text-gray-900 mt-2">—</p>
          <p className="text-sm text-gray-500 mt-1">In progress</p>
        </div>
      </div>
    </div>
  );
}
