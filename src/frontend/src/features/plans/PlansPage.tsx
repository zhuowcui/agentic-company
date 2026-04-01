import { useState } from 'react';
import { useRootNodes, useNodeTree } from '../../api/hooks/useNodes';
import { useSpecs } from '../../api/hooks/useSpecs';
import { usePlans, useCreatePlan } from '../../api/hooks/usePlans';
import type { Node } from '../../api/schemas/node';
import type { Spec } from '../../api/schemas/spec';
import type { Plan } from '../../api/schemas/plan';

interface PlansPageProps {
  specId?: string;
  onNavigate: (page: string) => void;
}

const planStatusColors: Record<string, string> = {
  Draft: 'bg-yellow-100 text-yellow-800',
  Active: 'bg-blue-100 text-blue-800',
  Completed: 'bg-green-100 text-green-800',
  Archived: 'bg-gray-100 text-gray-800',
};

export function PlansPage({ specId: initialSpecId, onNavigate }: PlansPageProps) {
  const { data: roots, isLoading: rootsLoading } = useRootNodes();
  const [selectedNode, setSelectedNode] = useState<Node | null>(null);
  const [selectedSpec, setSelectedSpec] = useState<Spec | null>(null);
  const [expandedRootId, setExpandedRootId] = useState<string | undefined>();
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [newContent, setNewContent] = useState('');
  const [newPlanType, setNewPlanType] = useState('Strategic');

  const activeRootId = expandedRootId ?? roots?.[0]?.id;
  const { data: treeData } = useNodeTree(activeRootId ?? '');

  const activeSpecId = initialSpecId ?? selectedSpec?.id ?? '';
  const { data: specs } = useSpecs(selectedNode?.id ?? '');
  const { data: plans, isLoading: plansLoading } = usePlans(activeSpecId);
  const createPlan = useCreatePlan();

  const handleNodeSelect = (node: Node) => {
    setSelectedNode(node);
    setSelectedSpec(null);
    if (!node.parentId) {
      setExpandedRootId(node.id);
    }
  };

  const handleCreatePlan = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!activeSpecId || !newContent.trim()) return;
    try {
      await createPlan.mutateAsync({
        specId: activeSpecId,
        data: { content: newContent.trim(), planType: newPlanType },
      });
      setNewContent('');
      setNewPlanType('Strategic');
      setShowCreateForm(false);
    } catch {
      // Error displayed via mutation state
    }
  };

  return (
    <div className="max-w-6xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Plans</h1>
          <p className="text-sm text-gray-500 mt-1">
            Manage execution plans for specifications
          </p>
        </div>
        {activeSpecId && (
          <button
            onClick={() => setShowCreateForm(true)}
            className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700"
          >
            + New Plan
          </button>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Selector panel — hidden when navigated with specId */}
        {!initialSpecId && (
          <div className="lg:col-span-1 space-y-4">
            <div className="bg-white rounded-xl border border-gray-200 p-4">
              <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">
                Select Node
              </h2>
              {rootsLoading ? (
                <div className="text-gray-500 text-sm">Loading...</div>
              ) : roots?.length ? (
                roots.map((root) => (
                  <NodeTreeSelector
                    key={root.id}
                    node={treeData && activeRootId === root.id ? treeData : root}
                    selectedId={selectedNode?.id}
                    onSelect={handleNodeSelect}
                  />
                ))
              ) : (
                <div className="text-gray-400 text-sm">No nodes found</div>
              )}
            </div>

            {selectedNode && (
              <div className="bg-white rounded-xl border border-gray-200 p-4">
                <h2 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">
                  Specs for {selectedNode.name}
                </h2>
                {specs?.length ? (
                  <div className="space-y-1">
                    {specs.map((spec) => (
                      <button
                        key={spec.id}
                        onClick={() => setSelectedSpec(spec)}
                        className={`w-full text-left px-3 py-2 text-sm rounded-lg transition-colors ${
                          selectedSpec?.id === spec.id
                            ? 'bg-blue-100 text-blue-800'
                            : 'hover:bg-gray-100 text-gray-700'
                        }`}
                      >
                        {spec.title}
                      </button>
                    ))}
                  </div>
                ) : (
                  <div className="text-gray-400 text-sm">No specs for this node</div>
                )}
              </div>
            )}
          </div>
        )}

        {/* Plans panel */}
        <div className={initialSpecId ? 'lg:col-span-3' : 'lg:col-span-2'}>
          {showCreateForm && (
            <div className="bg-white rounded-xl border border-blue-200 p-4 mb-4">
              <form onSubmit={handleCreatePlan} className="space-y-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Content</label>
                  <textarea
                    value={newContent}
                    onChange={(e) => setNewContent(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                    placeholder="Plan content"
                    rows={3}
                    autoFocus
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Plan Type
                  </label>
                  <select
                    value={newPlanType}
                    onChange={(e) => setNewPlanType(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                  >
                    <option value="Strategic">Strategic</option>
                    <option value="Technical">Technical</option>
                  </select>
                </div>
                <div className="flex gap-2">
                  <button
                    type="submit"
                    disabled={createPlan.isPending || !newContent.trim()}
                    className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 disabled:opacity-50"
                  >
                    {createPlan.isPending ? 'Creating...' : 'Create Plan'}
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowCreateForm(false)}
                    className="px-4 py-2 text-gray-600 text-sm rounded-lg hover:bg-gray-100"
                  >
                    Cancel
                  </button>
                </div>
                {createPlan.error && (
                  <p className="text-sm text-red-600">{createPlan.error.message}</p>
                )}
              </form>
            </div>
          )}

          {!activeSpecId ? (
            <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
              <p className="text-gray-400">Select a spec to view its plans</p>
            </div>
          ) : plansLoading ? (
            <div className="text-gray-500">Loading plans...</div>
          ) : !plans || plans.length === 0 ? (
            <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
              <p className="text-gray-500 text-lg mb-4">No plans yet</p>
              <button
                onClick={() => setShowCreateForm(true)}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                Create first plan
              </button>
            </div>
          ) : (
            <div className="space-y-3">
              {plans.map((plan) => (
                <PlanCard
                  key={plan.id}
                  plan={plan}
                  onNavigate={onNavigate}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

function PlanCard({
  plan,
  onNavigate,
}: {
  plan: Plan;
  onNavigate: (page: string) => void;
}) {
  const truncatedContent =
    plan.content.length > 120 ? plan.content.slice(0, 120) + '…' : plan.content;

  return (
    <button
      onClick={() => onNavigate(`tasks?planId=${plan.id}`)}
      className="w-full text-left bg-white rounded-xl border border-gray-200 p-4 hover:border-blue-300 hover:shadow-sm transition-all"
    >
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <h3 className="font-medium text-gray-900">{truncatedContent}</h3>
          <span className="px-2 py-0.5 text-xs font-medium rounded-full bg-indigo-100 text-indigo-800">
            {plan.planType}
          </span>
        </div>
        <span
          className={`px-2 py-0.5 text-xs font-medium rounded-full ${planStatusColors[plan.status] ?? 'bg-gray-100 text-gray-800'}`}
        >
          {plan.status}
        </span>
      </div>
      <div className="flex items-center gap-4 mt-2 text-xs text-gray-500">
        <span>Created {new Date(plan.createdAt).toLocaleDateString()}</span>
      </div>
    </button>
  );
}

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
