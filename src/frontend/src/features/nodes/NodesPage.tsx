import { useState } from 'react';
import { useRootNodes, useNodeTree } from '../../api/hooks/useNodes';
import { TreeNode } from '../../components/org-tree/TreeNode';
import { NodeDetail } from './NodeDetail';
import { CreateNodeDialog } from './CreateNodeDialog';
import type { Node } from '../../api/schemas/node';

export function NodesPage() {
  const { data: roots, isLoading } = useRootNodes();
  const [selectedNode, setSelectedNode] = useState<Node | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [expandedRootId, setExpandedRootId] = useState<string | undefined>();

  const selectedRootId = expandedRootId ?? roots?.[0]?.id;
  const { data: treeData } = useNodeTree(selectedRootId ?? '');

  const handleNodeSelect = (node: Node) => {
    setSelectedNode(node);
    if (!node.parentId) {
      setExpandedRootId(node.id);
    }
  };

  return (
    <div className="max-w-6xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Organization</h1>
          <p className="text-sm text-gray-500 mt-1">
            Manage your organizational hierarchy
          </p>
        </div>
        <button
          onClick={() => setShowCreate(true)}
          className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700"
        >
          + New Node
        </button>
      </div>

      {isLoading ? (
        <div className="text-gray-500">Loading organization...</div>
      ) : !roots || roots.length === 0 ? (
        <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
          <p className="text-gray-500 text-lg mb-4">No organizations yet</p>
          <button
            onClick={() => setShowCreate(true)}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Create your first organization
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Tree panel */}
          <div className="lg:col-span-1 bg-white rounded-xl border border-gray-200 p-4">
            <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">
              Hierarchy
            </h2>
            {roots.map((root) => (
              <TreeNode
                key={root.id}
                node={treeData && selectedRootId === root.id ? treeData : root}
                selectedId={selectedNode?.id}
                onSelect={handleNodeSelect}
              />
            ))}
            {selectedNode && (
              <button
                onClick={() => setShowCreate(true)}
                className="mt-4 w-full px-3 py-2 text-sm text-blue-600 border border-blue-200 rounded-lg hover:bg-blue-50"
              >
                + Add child to {selectedNode.name}
              </button>
            )}
          </div>

          {/* Detail panel */}
          <div className="lg:col-span-2 bg-white rounded-xl border border-gray-200 p-6">
            {selectedNode ? (
              <NodeDetail nodeId={selectedNode.id} />
            ) : (
              <div className="text-gray-400 text-center py-12">
                Select a node from the tree to view details
              </div>
            )}
          </div>
        </div>
      )}

      {showCreate && (
        <CreateNodeDialog
          parentId={selectedNode?.id}
          onClose={() => setShowCreate(false)}
        />
      )}
    </div>
  );
}
