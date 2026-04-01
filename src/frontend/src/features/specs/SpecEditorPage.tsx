import { useState, useEffect } from 'react';
import { useSpec, useCreateSpec, useUpdateSpec } from '../../api/hooks/useSpecs';
import { useDraftSpec, useReviewSpec } from '../../api/hooks/useAgent';
import { useNode } from '../../api/hooks/useNodes';
import type { ReviewSpecResponse } from '../../api/agent';

interface SpecEditorPageProps {
  specId?: string;
  nodeId?: string;
  onNavigate: (page: string) => void;
}

const PROVIDERS = [
  { value: 'echo', label: 'Echo (Dev)' },
  { value: 'openai', label: 'OpenAI' },
  { value: 'claude', label: 'Claude' },
];

export function SpecEditorPage({ specId, nodeId, onNavigate }: SpecEditorPageProps) {
  const isEditing = !!specId;

  const { data: existingSpec, isLoading: specLoading } = useSpec(specId ?? '');
  const { data: node } = useNode(nodeId ?? existingSpec?.nodeId ?? '');

  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [prompt, setPrompt] = useState('');
  const [provider, setProvider] = useState('echo');
  const [review, setReview] = useState<ReviewSpecResponse | null>(null);

  const createSpec = useCreateSpec();
  const updateSpec = useUpdateSpec();
  const draftSpec = useDraftSpec();
  const reviewSpec = useReviewSpec();

  // Populate form when editing an existing spec
  useEffect(() => {
    if (existingSpec) {
      setTitle(existingSpec.title);
      const latestVersion = existingSpec.versions
        ?.sort((a, b) => b.version - a.version)[0];
      if (latestVersion) {
        setContent(latestVersion.content);
      }
    }
  }, [existingSpec]);

  const effectiveNodeId = nodeId ?? existingSpec?.nodeId;

  const handleGenerate = () => {
    if (!effectiveNodeId || !prompt.trim()) return;
    draftSpec.mutate(
      { nodeId: effectiveNodeId, prompt, provider },
      {
        onSuccess: (data) => {
          setContent(data.draft);
        },
      }
    );
  };

  const handleReview = () => {
    if (!specId) return;
    reviewSpec.mutate(
      { specId, provider },
      {
        onSuccess: (data) => {
          setReview(data);
        },
      }
    );
  };

  const handleSave = () => {
    if (!title.trim() || !content.trim()) return;

    if (isEditing && specId) {
      updateSpec.mutate(
        { id: specId, data: { title, content } },
        { onSuccess: () => onNavigate('specs') }
      );
    } else if (effectiveNodeId) {
      createSpec.mutate(
        { nodeId: effectiveNodeId, data: { title, content } },
        { onSuccess: () => onNavigate('specs') }
      );
    }
  };

  if (specLoading && isEditing) {
    return <div className="text-gray-500">Loading spec...</div>;
  }

  const isSaving = createSpec.isPending || updateSpec.isPending;

  return (
    <div className="max-w-6xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div>
          <button
            onClick={() => onNavigate('specs')}
            className="text-sm text-blue-600 hover:text-blue-800 mb-2"
          >
            ← Back to Specs
          </button>
          <h1 className="text-2xl font-bold text-gray-900">
            {isEditing ? 'Edit Spec' : 'New Spec'}
          </h1>
          {node && (
            <p className="text-sm text-gray-500 mt-1">
              Node: {node.name} ({node.type})
            </p>
          )}
        </div>
        <button
          onClick={handleSave}
          disabled={isSaving || !title.trim() || !content.trim()}
          className="px-4 py-2 bg-blue-600 text-white text-sm rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isSaving ? 'Saving...' : 'Save Spec'}
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main editor */}
        <div className="lg:col-span-2 space-y-4">
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Title
            </label>
            <input
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Spec title..."
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
            />
          </div>

          {/* AI generation controls */}
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <h3 className="text-sm font-semibold text-gray-700 mb-3">
              🤖 AI Assistant
            </h3>
            <div className="flex items-end gap-3">
              <div className="flex-1">
                <label className="block text-xs text-gray-500 mb-1">
                  Describe what this spec should cover
                </label>
                <input
                  type="text"
                  value={prompt}
                  onChange={(e) => setPrompt(e.target.value)}
                  placeholder="e.g. Build a user authentication system..."
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
              <div>
                <label className="block text-xs text-gray-500 mb-1">
                  Provider
                </label>
                <select
                  value={provider}
                  onChange={(e) => setProvider(e.target.value)}
                  className="px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  {PROVIDERS.map((p) => (
                    <option key={p.value} value={p.value}>
                      {p.label}
                    </option>
                  ))}
                </select>
              </div>
              <button
                onClick={handleGenerate}
                disabled={draftSpec.isPending || !prompt.trim() || !effectiveNodeId}
                className="px-4 py-2 bg-purple-600 text-white text-sm rounded-lg hover:bg-purple-700 disabled:opacity-50 disabled:cursor-not-allowed whitespace-nowrap"
              >
                {draftSpec.isPending ? 'Generating...' : '✨ Generate with AI'}
              </button>
            </div>
            {draftSpec.isError && (
              <p className="mt-2 text-sm text-red-600">
                Generation failed: {draftSpec.error.message}
              </p>
            )}
          </div>

          {/* Content editor */}
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <div className="flex items-center justify-between mb-2">
              <label className="block text-sm font-medium text-gray-700">
                Spec Content (Markdown)
              </label>
              {isEditing && (
                <button
                  onClick={handleReview}
                  disabled={reviewSpec.isPending}
                  className="px-3 py-1.5 text-xs bg-amber-100 text-amber-800 rounded-lg hover:bg-amber-200 disabled:opacity-50"
                >
                  {reviewSpec.isPending ? 'Reviewing...' : '🔍 Review against Principles'}
                </button>
              )}
            </div>
            <textarea
              value={content}
              onChange={(e) => setContent(e.target.value)}
              placeholder="Write your spec in markdown..."
              rows={20}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm font-mono focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-y"
            />
          </div>

          {/* Review feedback */}
          {review && (
            <div className={`rounded-xl border p-4 ${review.aligned ? 'bg-green-50 border-green-200' : 'bg-amber-50 border-amber-200'}`}>
              <div className="flex items-center justify-between mb-3">
                <h3 className="text-sm font-semibold text-gray-700">
                  Principle Review
                </h3>
                <div className="flex items-center gap-2">
                  <span
                    className={`px-2 py-0.5 text-xs font-medium rounded-full ${
                      review.aligned
                        ? 'bg-green-100 text-green-800'
                        : 'bg-amber-100 text-amber-800'
                    }`}
                  >
                    {review.aligned ? '✅ Aligned' : '⚠️ Needs Work'}
                  </span>
                  <span className="text-sm font-medium text-gray-600">
                    Score: {review.score}/100
                  </span>
                </div>
              </div>
              <p className="text-sm text-gray-700 mb-3">{review.feedback}</p>
              {review.suggestions.length > 0 && (
                <div>
                  <h4 className="text-xs font-semibold text-gray-600 uppercase mb-1">
                    Suggestions
                  </h4>
                  <ul className="space-y-1">
                    {review.suggestions.map((s, i) => (
                      <li key={i} className="text-sm text-gray-600 flex items-start gap-2">
                        <span className="text-amber-500 mt-0.5">•</span>
                        {s}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Sidebar — Node info */}
        <div className="lg:col-span-1 space-y-4">
          {node && (
            <div className="bg-white rounded-xl border border-gray-200 p-4">
              <h3 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">
                Node Info
              </h3>
              <dl className="space-y-2 text-sm">
                <div>
                  <dt className="text-gray-500">Name</dt>
                  <dd className="font-medium text-gray-900">{node.name}</dd>
                </div>
                <div>
                  <dt className="text-gray-500">Type</dt>
                  <dd className="font-medium text-gray-900">{node.type}</dd>
                </div>
                <div>
                  <dt className="text-gray-500">Depth</dt>
                  <dd className="font-medium text-gray-900">{node.depth}</dd>
                </div>
                {node.description && (
                  <div>
                    <dt className="text-gray-500">Description</dt>
                    <dd className="text-gray-700">{node.description}</dd>
                  </div>
                )}
              </dl>
            </div>
          )}

          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <h3 className="text-sm font-semibold text-gray-700 uppercase tracking-wide mb-3">
              Effective Principles
            </h3>
            <p className="text-xs text-gray-400">
              Principles are loaded from the node&apos;s hierarchy. Use &quot;Review against Principles&quot; to check alignment.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
