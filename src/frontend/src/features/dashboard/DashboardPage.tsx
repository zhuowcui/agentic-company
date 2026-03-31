import { useOrgOverview, useNodeStats, useNodeActivity } from '../../api/hooks/useDashboard';
import type { NodeStatsResponse, ActivityItem } from '../../api/hooks/useDashboard';

interface DashboardPageProps {
  nodeId?: string;
}

const statusColors: Record<string, string> = {
  Draft: 'bg-gray-400',
  InReview: 'bg-yellow-400',
  Approved: 'bg-green-500',
  Rejected: 'bg-red-400',
  Archived: 'bg-gray-300',
  Active: 'bg-blue-500',
  Completed: 'bg-green-500',
  Pending: 'bg-gray-400',
  InProgress: 'bg-blue-500',
  Blocked: 'bg-red-400',
  Cascaded: 'bg-purple-400',
};

function StatCard({ label, value, sub }: { label: string; value: number | string; sub: string }) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-6">
      <h3 className="text-sm font-semibold text-gray-500 uppercase">{label}</h3>
      <p className="text-3xl font-bold text-gray-900 mt-2">{value}</p>
      <p className="text-sm text-gray-500 mt-1">{sub}</p>
    </div>
  );
}

function StatusBar({ data, label }: { data: Record<string, number>; label: string }) {
  const total = Object.values(data).reduce((a, b) => a + b, 0);
  if (total === 0) return (
    <div className="mb-4">
      <h4 className="text-sm font-medium text-gray-700 mb-2">{label}</h4>
      <p className="text-sm text-gray-400">No items</p>
    </div>
  );

  return (
    <div className="mb-4">
      <h4 className="text-sm font-medium text-gray-700 mb-2">{label} ({total})</h4>
      <div className="flex rounded-full overflow-hidden h-4 bg-gray-100">
        {Object.entries(data).map(([status, count]) => (
          <div
            key={status}
            className={`${statusColors[status] ?? 'bg-gray-400'} transition-all`}
            style={{ width: `${(count / total) * 100}%` }}
            title={`${status}: ${count}`}
          />
        ))}
      </div>
      <div className="flex flex-wrap gap-3 mt-2">
        {Object.entries(data).map(([status, count]) => (
          <span key={status} className="flex items-center gap-1.5 text-xs text-gray-600">
            <span className={`w-2.5 h-2.5 rounded-full ${statusColors[status] ?? 'bg-gray-400'}`} />
            {status}: {count}
          </span>
        ))}
      </div>
    </div>
  );
}

function ActivityFeed({ activities }: { activities: ActivityItem[] }) {
  if (activities.length === 0) return <p className="text-sm text-gray-400">No recent activity</p>;

  return (
    <div className="space-y-2">
      {activities.map((item) => (
        <div key={`${item.type}-${item.id}`} className="flex items-center gap-3 py-2 border-b border-gray-100 last:border-0">
          <span className={`px-2 py-0.5 text-xs rounded font-medium ${
            item.type === 'spec' ? 'bg-blue-100 text-blue-700' : 'bg-green-100 text-green-700'
          }`}>
            {item.type}
          </span>
          <span className="text-sm text-gray-900 flex-1 truncate">{item.title}</span>
          <span className={`px-2 py-0.5 text-xs rounded ${statusColors[item.status] ? 'text-white' : 'text-gray-600 bg-gray-100'} ${statusColors[item.status] ?? ''}`}>
            {item.status}
          </span>
          <span className="text-xs text-gray-400 whitespace-nowrap">
            {new Date(item.timestamp).toLocaleDateString()}
          </span>
        </div>
      ))}
    </div>
  );
}

function NodeDashboard({ nodeId, stats }: { nodeId: string; stats: NodeStatsResponse }) {
  const { data: activity } = useNodeActivity(nodeId);

  const totalSpecs = Object.values(stats.specsByStatus).reduce((a, b) => a + b, 0);
  const totalPlans = Object.values(stats.plansByStatus).reduce((a, b) => a + b, 0);
  const totalTasks = Object.values(stats.tasksByStatus).reduce((a, b) => a + b, 0);

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="Specs" value={totalSpecs} sub="Total specifications" />
        <StatCard label="Plans" value={totalPlans} sub="Total plans" />
        <StatCard label="Tasks" value={totalTasks} sub="Total tasks" />
        <StatCard label="Children" value={stats.childNodeCount} sub={`${stats.activePrincipleCount} principles`} />
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Status Breakdown</h3>
        <StatusBar data={stats.specsByStatus} label="Specs" />
        <StatusBar data={stats.plansByStatus} label="Plans" />
        <StatusBar data={stats.tasksByStatus} label="Tasks" />
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Recent Activity</h3>
        <ActivityFeed activities={activity?.activities ?? []} />
      </div>
    </div>
  );
}

function OrgOverview() {
  const { data, isLoading } = useOrgOverview();

  if (isLoading) return <p className="text-gray-500">Loading overview...</p>;
  if (!data) return <p className="text-gray-400">Could not load overview</p>;

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard label="Nodes" value={data.totalNodes} sub="Organizational units" />
        <StatCard label="Specs" value={data.totalSpecs} sub="Specifications" />
        <StatCard label="Plans" value={data.totalPlans} sub="Plans" />
        <StatCard label="Tasks" value={data.totalTasks} sub="Tasks" />
      </div>

      <div className="bg-white rounded-xl border border-gray-200 p-6">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Node Type Breakdown</h3>
        {Object.keys(data.nodesByType).length > 0 ? (
          <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
            {Object.entries(data.nodesByType).map(([type, count]) => (
              <div key={type} className="text-center p-3 bg-gray-50 rounded-lg">
                <p className="text-2xl font-bold text-gray-900">{count}</p>
                <p className="text-sm text-gray-500">{type}</p>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-sm text-gray-400">No nodes yet</p>
        )}
      </div>

      {data.maxCascadeDepth > 0 && (
        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Cascade Depth</h3>
          <p className="text-sm text-gray-600">
            Maximum cascade depth: <span className="font-semibold">{data.maxCascadeDepth} levels</span>
          </p>
        </div>
      )}
    </div>
  );
}

export function DashboardPage({ nodeId }: DashboardPageProps) {
  const { data: stats, isLoading } = useNodeStats(nodeId ?? '');

  return (
    <div className="max-w-5xl mx-auto">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">
        {nodeId ? 'Node Dashboard' : 'Dashboard'}
      </h1>
      {nodeId ? (
        isLoading ? (
          <p className="text-gray-500">Loading stats...</p>
        ) : stats ? (
          <NodeDashboard nodeId={nodeId} stats={stats} />
        ) : (
          <p className="text-gray-400">Node not found</p>
        )
      ) : (
        <OrgOverview />
      )}
    </div>
  );
}
