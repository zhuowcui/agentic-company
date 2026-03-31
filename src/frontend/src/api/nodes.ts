import { apiFetch } from './client';
import type { Node, CreateNode } from './schemas/node';

export const nodeApi = {
  listRoots: () => apiFetch<Node[]>('/nodes'),
  getById: (id: string) => apiFetch<Node>(`/nodes/${id}`),
  getTree: (id: string) => apiFetch<Node>(`/nodes/${id}/tree`),
  create: (data: CreateNode) => apiFetch<Node>('/nodes', {
    method: 'POST',
    body: JSON.stringify(data),
  }),
  update: (id: string, data: Partial<CreateNode>) => apiFetch<Node>(`/nodes/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data),
  }),
  delete: (id: string) => apiFetch<void>(`/nodes/${id}`, { method: 'DELETE' }),
};
