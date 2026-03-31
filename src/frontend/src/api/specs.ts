import { apiFetch } from './client';
import type { Spec } from './schemas/spec';

export interface CreateSpecData {
  title: string;
  content: string;
}

export interface UpdateSpecData {
  content: string;
}

export const specApi = {
  listByNode: (nodeId: string) => apiFetch<Spec[]>(`/nodes/${nodeId}/specs`),
  getById: (id: string) => apiFetch<Spec>(`/specs/${id}`),
  create: (nodeId: string, data: CreateSpecData) =>
    apiFetch<Spec>(`/nodes/${nodeId}/specs`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  update: (id: string, data: UpdateSpecData) =>
    apiFetch<Spec>(`/specs/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
};
