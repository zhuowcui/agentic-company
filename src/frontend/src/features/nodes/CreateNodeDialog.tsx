import { useState } from 'react';
import { useCreateNode } from '../../api/hooks/useNodes';
import type { NodeType } from '../../api/schemas/node';

interface CreateNodeDialogProps {
  parentId?: string | null;
  onClose: () => void;
}

const nodeTypes: NodeType[] = ['Company', 'Organization', 'Squad', 'Team', 'Project'];

export function CreateNodeDialog({ parentId, onClose }: CreateNodeDialogProps) {
  const [name, setName] = useState('');
  const [type, setType] = useState<NodeType>(parentId ? 'Team' : 'Company');
  const [description, setDescription] = useState('');
  const createNode = useCreateNode();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    await createNode.mutateAsync({
      name,
      type,
      description: description || null,
      parentId: parentId ?? null,
    });
    onClose();
  };

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
      <div className="bg-white rounded-xl shadow-xl p-6 w-full max-w-md">
        <h2 className="text-lg font-semibold mb-4">
          {parentId ? 'Create Child Node' : 'Create Organization'}
        </h2>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              placeholder="e.g., Acme Corp"
              required
              autoFocus
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
            <select
              value={type}
              onChange={(e) => setType(e.target.value as NodeType)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
            >
              {nodeTypes.map((t) => (
                <option key={t} value={t}>{t}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              rows={3}
              placeholder="Optional description..."
            />
          </div>
          <div className="flex justify-end gap-3 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-lg"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={!name.trim() || createNode.isPending}
              className="px-4 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
            >
              {createNode.isPending ? 'Creating...' : 'Create'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
