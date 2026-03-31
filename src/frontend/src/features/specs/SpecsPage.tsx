import { useState } from 'react';
import { useRootNodes, useNodeTree } from '../../api/hooks/useNodes';
import { useSpecs } from '../../api/hooks/useSpecs';
import type { Node } from '../../api/schemas/node';
import type { Spec } from '../../api/schemas/spec';

interface SpecsPageProps {
  onNavigate: (page: string) => void;
}

export function SpecsPage({ onNavigate }: SpecsPageProps) {
  const { data: roots, isLoading: rootsLoading } = useRootNodes();
  const [selectedNode, setSelectedNode] = useState<Node | null>(null);

  const selectedRootId = selectedNode
    ? (roots?.find((r) => r.id === selectedNode.id)?.id ?? roots?.[0]?.id)
    : roots?.[0]?.id;

  const { data: treeData } = useNodeTree(selectedRootId ?? '');

  const { data: specs, isLoading: specsLoading } = useSpecs(selectedNode?.id ?? '');

  const handleNewSpec = () => {
    if (selectedNode) {
      onNavigate(`specs/new?nodeId=${selectedNode.id}`);
    }
  };

  const handleSpecClick = (spec: Spec) => {
    onNavigate(`specs/${spec.id}/edit`);
  };

  const statusColors: Record<string, string> = {
    Draft: 'bg-yellow-100 text-yellow-800',
    InReview: 'bg-blue-100 text-blue-800',
    Approved: 'bg-green-100 text-green-800',
    Rejected: 'bg-red-100 text-red-800',
    Archived: 'bg-gray-100 text-gray-800',
  };

  return (
    <div className="max-w-6xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Specs</h1>
          <p className="text-sm text-gray-500 mt-1">
            Manage specifications for your organization
          </p>
        </div>
        {selectedNode && (
          <button
            onClick={handleNewSpec}
            className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700"
          >
            + New Spec
          </button>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Node selector panel */}
        <div className="lg:col-span-1 bg-white rounded-xl border border-gray-200 p-4">
          <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">
            Select a Node
          </h2>
          {rootsLoading ? (
            <div className="text-gray-500 text-sm">Loading...</div>
          ) : treeData ? (
            <NodeTreeSelector
              node={treeData}
              selectedId={selectedNode?.id}
              onSelect={setSelectedNode}
            />
          ) : roots?.length ? (
            roots.map((root) => (
              <NodeTreeSelector
                key={root.id}
                node={root}
                selectedId={selectedNode?.id}
                onSelect={setSelectedNode}
              />
            ))
          ) : (
            <div className="text-gray-400 text-sm">No nodes found</div>
          )}
        </div>

        {/* Specs list panel */}
        <div className="lg:col-span-2">
          {!selectedNode ? (
            <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
              <p className="text-gray-400">Select a node to view its specs</p>
            </div>
          ) : specsLoading ? (
            <div className="text-gray-500">Loading specs...</div>
          ) : !specs || specs.length === 0 ? (
            <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
              <p className="text-gray-500 text-lg mb-4">
                No specs for {selectedNode.name}
              </p>
              <button
                onClick={handleNewSpec}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                Create first spec
              </button>
            </div>
          ) : (
            <div className="space-y-3">
              {specs.map((spec) => (
                <button
                  key={spec.id}
                  onClick={() => handleSpecClick(spec)}
                  className="w-full text-left bg-white rounded-xl border border-gray-200 p-4 hover:border-blue-300 hover:shadow-sm transition-all"
                >
                  <div className="flex items-center justify-between">
                    <h3 className="font-medium text-gray-900">{spec.title}</h3>
                    <span
                      className={`px-2 py-0.5 text-xs font-medium rounded-full ${statusColors[spec.status] ?? 'bg-gray-100 text-gray-800'}`}
                    >
                      {spec.status}
                    </span>
                  </div>
                  <div className="flex items-center gap-4 mt-2 text-xs text-gray-500">
                    <span>
                      Created {new Date(spec.createdAt).toLocaleDateString()}
                    </span>
                    {spec.versions && (
                      <span>v{spec.versions.length}</span>
                    )}
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

// Simple tree selector component for picking a node
function NodeTreeSelector({
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
            ? 'bg-blue-100 text-blue-800'
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
          <NodeTreeSelector
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
