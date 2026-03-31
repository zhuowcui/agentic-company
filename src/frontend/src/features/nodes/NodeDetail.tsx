import { useNode } from '../../api/hooks/useNodes';
import { useEffectivePrinciples } from '../../api/hooks/usePrinciples';

interface NodeDetailProps {
  nodeId: string;
}

export function NodeDetail({ nodeId }: NodeDetailProps) {
  const { data: node, isLoading } = useNode(nodeId);
  const { data: principles } = useEffectivePrinciples(nodeId);

  if (isLoading) return <div className="p-4 text-gray-500">Loading...</div>;
  if (!node) return <div className="p-4 text-gray-500">Node not found</div>;

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-bold text-gray-900">{node.name}</h2>
        <p className="text-sm text-gray-500 mt-1">{node.type} · Depth {node.depth}</p>
        {node.description && (
          <p className="text-gray-700 mt-3">{node.description}</p>
        )}
      </div>

      <div>
        <h3 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">
          Effective Principles
        </h3>
        {principles && principles.length > 0 ? (
          <div className="space-y-2">
            {principles.map((ep) => (
              <div
                key={ep.principle.id}
                className={`p-3 rounded-lg border ${
                  ep.isInherited
                    ? 'bg-gray-50 border-gray-200'
                    : 'bg-white border-blue-200'
                } ${ep.isOverridden ? 'opacity-50 line-through' : ''}`}
              >
                <div className="flex items-center gap-2">
                  <span className="font-medium text-sm">{ep.principle.title}</span>
                  {ep.isInherited && (
                    <span className="text-xs bg-gray-200 text-gray-600 px-1.5 py-0.5 rounded">
                      inherited
                    </span>
                  )}
                </div>
                <p className="text-sm text-gray-600 mt-1">{ep.principle.content}</p>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-sm text-gray-400">No principles defined</p>
        )}
      </div>
    </div>
  );
}
