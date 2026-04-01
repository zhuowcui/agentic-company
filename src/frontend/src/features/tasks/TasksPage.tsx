import { useState } from 'react';
import { useTasks, useCreateTask, useUpdateTask } from '../../api/hooks/useTasks';
import { usePlan } from '../../api/hooks/usePlans';
import type { TaskItem, TaskItemStatus } from '../../api/schemas/task';
import { CascadeDialog } from './CascadeDialog';

interface TasksPageProps {
  planId: string;
  onNavigate: (page: string) => void;
}

const statusColors: Record<string, string> = {
  Pending: 'bg-gray-100 text-gray-800',
  InProgress: 'bg-blue-100 text-blue-800',
  Completed: 'bg-green-100 text-green-800',
  Blocked: 'bg-red-100 text-red-800',
  Cascaded: 'bg-purple-100 text-purple-800',
};

const allStatuses: TaskItemStatus[] = ['Pending', 'InProgress', 'Completed', 'Blocked', 'Cascaded'];

export function TasksPage({ planId, onNavigate }: TasksPageProps) {
  const { data: plan } = usePlan(planId);
  const { data: tasks, isLoading } = useTasks(planId);
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [cascadeTask, setCascadeTask] = useState<TaskItem | null>(null);
  const [newTitle, setNewTitle] = useState('');
  const [newDescription, setNewDescription] = useState('');
  const [newTargetNodeId, setNewTargetNodeId] = useState('');

  const createTask = useCreateTask();
  const updateTask = useUpdateTask();

  const filteredTasks =
    tasks?.filter((t) => statusFilter === 'all' || t.status === statusFilter) ?? [];

  const planLabel = plan
    ? plan.content.length > 60
      ? plan.content.slice(0, 60) + '…'
      : plan.content
    : '';

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!planId || !newTitle.trim()) return;
    try {
      const nextOrder = tasks?.length ? Math.max(...tasks.map((t) => t.order)) + 1 : 0;
      await createTask.mutateAsync({
        planId,
        data: {
          title: newTitle.trim(),
          description: newDescription.trim() || undefined,
          order: nextOrder,
          targetNodeId: newTargetNodeId.trim() || undefined,
        },
      });
      // After creation, update assignedTo if provided
      // (assignedTo is on UpdateTaskRequest, not CreateTaskRequest)
      setNewTitle('');
      setNewDescription('');
      setNewTargetNodeId('');
      setShowCreateForm(false);
    } catch {
      // Error displayed via mutation state
    }
  };

  const handleStatusChange = async (task: TaskItem, newStatus: string) => {
    try {
      await updateTask.mutateAsync({ id: task.id, data: { status: newStatus } });
    } catch {
      // Error displayed via mutation state
    }
  };

  if (!planId) {
    return (
      <div className="max-w-6xl mx-auto">
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
          <p className="text-gray-400 mb-4">Navigate to a plan to view its tasks</p>
          <button
            onClick={() => onNavigate('plans')}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Go to Plans
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div>
          <button
            onClick={() => onNavigate(plan ? `plans?specId=${plan.specId}` : 'plans')}
            className="text-sm text-blue-600 hover:text-blue-800 mb-1"
          >
            ← Back to Plans
          </button>
          <h1 className="text-2xl font-bold text-gray-900">
            {plan ? `Tasks: ${planLabel}` : 'Tasks'}
          </h1>
          <p className="text-sm text-gray-500 mt-1">Manage tasks for this plan</p>
        </div>
        <button
          onClick={() => setShowCreateForm(true)}
          className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700"
        >
          + New Task
        </button>
      </div>

      {/* Status filter */}
      <div className="flex flex-wrap gap-2 mb-4">
        <button
          onClick={() => setStatusFilter('all')}
          className={`px-3 py-1.5 text-sm rounded-lg transition-colors ${
            statusFilter === 'all'
              ? 'bg-gray-900 text-white'
              : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50'
          }`}
        >
          All
        </button>
        {allStatuses.map((s) => (
          <button
            key={s}
            onClick={() => setStatusFilter(s)}
            className={`px-3 py-1.5 text-sm rounded-lg transition-colors ${
              statusFilter === s
                ? 'bg-gray-900 text-white'
                : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50'
            }`}
          >
            {s}
          </button>
        ))}
      </div>

      {/* Create form */}
      {showCreateForm && (
        <div className="bg-white rounded-xl border border-blue-200 p-4 mb-4">
          <form onSubmit={handleCreateTask} className="space-y-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Title</label>
              <input
                type="text"
                value={newTitle}
                onChange={(e) => setNewTitle(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Task title"
                autoFocus
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Description
              </label>
              <textarea
                value={newDescription}
                onChange={(e) => setNewDescription(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Optional description"
                rows={2}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Target Node ID
              </label>
              <input
                type="text"
                value={newTargetNodeId}
                onChange={(e) => setNewTargetNodeId(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                placeholder="Optional — UUID of target node for cascade"
              />
            </div>
            <div className="flex gap-2">
              <button
                type="submit"
                disabled={createTask.isPending || !newTitle.trim()}
                className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 disabled:opacity-50"
              >
                {createTask.isPending ? 'Creating...' : 'Create Task'}
              </button>
              <button
                type="button"
                onClick={() => setShowCreateForm(false)}
                className="px-4 py-2 text-gray-600 text-sm rounded-lg hover:bg-gray-100"
              >
                Cancel
              </button>
            </div>
            {createTask.error && (
              <p className="text-sm text-red-600">{createTask.error.message}</p>
            )}
          </form>
        </div>
      )}

      {/* Task list */}
      {isLoading ? (
        <div className="text-gray-500">Loading tasks...</div>
      ) : filteredTasks.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
          <p className="text-gray-500 text-lg mb-4">
            {statusFilter === 'all' ? 'No tasks yet' : `No ${statusFilter} tasks`}
          </p>
          {statusFilter === 'all' && (
            <button
              onClick={() => setShowCreateForm(true)}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
            >
              Create first task
            </button>
          )}
        </div>
      ) : (
        <div className="space-y-3">
          {filteredTasks.map((task) => (
            <TaskCard
              key={task.id}
              task={task}
              onStatusChange={handleStatusChange}
              onCascade={() => setCascadeTask(task)}
              onNavigate={onNavigate}
            />
          ))}
        </div>
      )}

      {cascadeTask && (
        <CascadeDialog
          task={cascadeTask}
          onClose={() => setCascadeTask(null)}
          onSuccess={(spawnedSpecId) => {
            setCascadeTask(null);
            onNavigate(`specs/${spawnedSpecId}/edit`);
          }}
        />
      )}
    </div>
  );
}

function TaskCard({
  task,
  onStatusChange,
  onCascade,
  onNavigate,
}: {
  task: TaskItem;
  onStatusChange: (task: TaskItem, status: string) => void;
  onCascade: () => void;
  onNavigate: (page: string) => void;
}) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 p-4">
      <div className="flex items-start justify-between gap-4">
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <h3 className="font-medium text-gray-900">{task.title}</h3>
            <span
              className={`px-2 py-0.5 text-xs font-medium rounded-full ${statusColors[task.status] ?? 'bg-gray-100 text-gray-800'}`}
            >
              {task.status}
            </span>
          </div>
          {task.description && (
            <p className="text-sm text-gray-500 mt-1">{task.description}</p>
          )}
          <div className="flex items-center gap-4 mt-2 text-xs text-gray-500">
            {task.assignedTo && <span>Assigned to: {task.assignedTo}</span>}
            <span>Created {new Date(task.createdAt).toLocaleDateString()}</span>
            {task.spawnedSpecId && (
              <button
                onClick={() => onNavigate(`specs/${task.spawnedSpecId}/edit`)}
                className="text-purple-600 hover:text-purple-800"
              >
                Cascaded → View Spec
              </button>
            )}
          </div>
        </div>
        <div className="flex items-center gap-2 shrink-0">
          {task.status !== 'Cascaded' && (
            <select
              value={task.status}
              onChange={(e) => onStatusChange(task, e.target.value)}
              className="text-sm border border-gray-300 rounded-lg px-2 py-1 focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            >
              {allStatuses
                .filter((s) => s !== 'Cascaded')
                .map((s) => (
                  <option key={s} value={s}>
                    {s}
                  </option>
                ))}
            </select>
          )}
          {task.status !== 'Cascaded' && (
            <button
              onClick={onCascade}
              className="px-3 py-1 text-sm bg-purple-600 text-white rounded-lg hover:bg-purple-700"
            >
              Cascade
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
