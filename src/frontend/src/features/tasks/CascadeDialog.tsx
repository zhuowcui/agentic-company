import { useState } from 'react';
import { useRootNodes, useNodeTree } from '../../api/hooks/useNodes';
import { useCascadeTask } from '../../api/hooks/useTasks';
import type { TaskItem } from '../../api/schemas/task';
import type { Node } from '../../api/schemas/node';

interface CascadeDialogProps {
  task: TaskItem;
  onClose: () => void;
  onSuccess: (spawnedSpecId: string) => void;
}

export function CascadeDialog({ task, onClose, onSuccess }: CascadeDialogProps) {
  const { data: roots } = useRootNodes();
  const [selectedNodeId, setSelectedNodeId] = useState(task.targetNodeId ?? '');
  const [expandedRootId, setExpandedRootId] = useState<string | undefined>();
  const cascade = useCascadeTask();

  const activeRootId = expandedRootId ?? roots?.[0]?.id;
  const { data: treeData } = useNodeTree(activeRootId ?? '');

  const handleNodeSelect = (node: Node) => {
    setSelectedNodeId(node.id);
    if (!node.parentId) {
      setExpandedRootId(node.id);
    }
  };

  const handleCascade = async () => {
    if (!selectedNodeId) return;
    try {
      const result = await cascade.mutateAsync({
        id: task.id,
        targetNodeId: selectedNodeId,
      });
      onSuccess(result.spawnedSpec.id);
    } catch {
      // Error displayed via mutation state
    }
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl border border-gray-200 p-6 w-full max-w-lg mx-4">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Cascade Task</h2>

        <div className="mb-4 p-3 bg-gray-50 rounded-lg">
          <p className="text-sm font-medium text-gray-700">{task.title}</p>
          {task.description && (
            <p className="text-sm text-gray-500 mt-1">{task.description}</p>
          )}
        </div>

        <div className="mb-4">
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Target Node
          </label>
          <div className="border border-gray-300 rounded-lg p-3 max-h-60 overflow-y-auto">
            {roots?.length ? (
              roots.map((root) => (
                <CascadeNodeSelector
                  key={root.id}
                  node={treeData && activeRootId === root.id ? treeData : root}
                  selectedId={selectedNodeId}
                  onSelect={handleNodeSelect}
                />
              ))
            ) : (
              <div className="text-gray-400 text-sm">Loading nodes...</div>
            )}
          </div>
        </div>

        {cascade.error && (
          <p className="text-sm text-red-600 mb-4">{cascade.error.message}</p>
        )}

        <div className="flex justify-end gap-2">
          <button
            onClick={onClose}
            className="px-4 py-2 text-gray-600 text-sm rounded-lg hover:bg-gray-100"
          >
            Cancel
          </button>
          <button
            onClick={handleCascade}
            disabled={!selectedNodeId || cascade.isPending}
            className="px-4 py-2 bg-purple-600 text-white text-sm rounded-lg hover:bg-purple-700 disabled:opacity-50"
          >
            {cascade.isPending ? 'Cascading...' : 'Cascade'}
          </button>
        </div>
      </div>
    </div>
  );
}

function CascadeNodeSelector({
  node,
  selectedId,
  onSelect,
  depth = 0,
}: {
  node: Node;
  selectedId?: string;
  onSelect: (node: Node) => void;
  depth?: number;
}) {
  const [expanded, setExpanded] = useState(depth === 0);
  const hasChildren = node.children && node.children.length > 0;
  const isSelected = node.id === selectedId;

  return (
    <div>
      <div
        className={`flex items-center gap-1 py-1 px-2 rounded cursor-pointer text-sm ${
          isSelected
            ? 'bg-purple-100 text-purple-800'
            : 'hover:bg-gray-100 text-gray-700'
        }`}
        style={{ paddingLeft: `${depth * 16 + 8}px` }}
      >
        {hasChildren ? (
          <button
            onClick={(e) => {
              e.stopPropagation();
              setExpanded(!expanded);
            }}
            className="w-4 h-4 flex items-center justify-center text-gray-400"
          >
            {expanded ? '▾' : '▸'}
          </button>
        ) : (
          <span className="w-4" />
        )}
        <button
          onClick={() => onSelect(node)}
          className="flex-1 text-left truncate"
        >
          {node.name}
          <span className="ml-1 text-xs text-gray-400">({node.type})</span>
        </button>
      </div>
      {expanded &&
        hasChildren &&
        node.children!.map((child) => (
          <CascadeNodeSelector
            key={child.id}
            node={child}
            selectedId={selectedId}
            onSelect={onSelect}
            depth={depth + 1}
          />
        ))}
    </div>
  );
}
