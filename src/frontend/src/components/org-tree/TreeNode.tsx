import { useState } from 'react';
import type { Node } from '../../api/schemas/node';

const typeIcons: Record<string, string> = {
  Company: '🏢',
  Organization: '🏛️',
  Squad: '👥',
  Team: '👤',
  Project: '📁',
};

const typeColors: Record<string, string> = {
  Company: 'bg-blue-100 text-blue-800 border-blue-200',
  Organization: 'bg-purple-100 text-purple-800 border-purple-200',
  Squad: 'bg-green-100 text-green-800 border-green-200',
  Team: 'bg-orange-100 text-orange-800 border-orange-200',
  Project: 'bg-gray-100 text-gray-800 border-gray-200',
};

interface TreeNodeProps {
  node: Node;
  selectedId?: string;
  onSelect: (node: Node) => void;
  depth?: number;
}

export function TreeNode({ node, selectedId, onSelect, depth = 0 }: TreeNodeProps) {
  const [expanded, setExpanded] = useState(depth < 2);
  const hasChildren = node.children && node.children.length > 0;
  const isSelected = node.id === selectedId;

  return (
    <div>
      <div
        className={`flex items-center gap-2 px-3 py-2 rounded-lg cursor-pointer transition-colors
          ${isSelected ? 'bg-blue-50 ring-1 ring-blue-300' : 'hover:bg-gray-50'}`}
        style={{ paddingLeft: `${depth * 20 + 12}px` }}
        onClick={() => onSelect(node)}
      >
        {hasChildren ? (
          <button
            onClick={(e) => { e.stopPropagation(); setExpanded(!expanded); }}
            className="w-5 h-5 flex items-center justify-center text-gray-400 hover:text-gray-600"
          >
            {expanded ? '▼' : '▶'}
          </button>
        ) : (
          <span className="w-5" />
        )}
        <span className="text-lg">{typeIcons[node.type] || '📄'}</span>
        <span className="font-medium text-gray-900 text-sm">{node.name}</span>
        <span className={`text-xs px-1.5 py-0.5 rounded border ${typeColors[node.type] || ''}`}>
          {node.type}
        </span>
      </div>
      {expanded && hasChildren && (
        <div>
          {node.children!.map((child) => (
            <TreeNode
              key={child.id}
              node={child}
              selectedId={selectedId}
              onSelect={onSelect}
              depth={depth + 1}
            />
          ))}
        </div>
      )}
    </div>
  );
}
