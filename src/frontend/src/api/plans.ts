import { apiFetch } from './client';
import type { Plan } from './schemas/plan';

export interface CreatePlanData {
  content: string;
  planType: string;
}

export interface UpdatePlanData {
  content?: string;
  status?: string;
}

export const planApi = {
  listBySpec: (specId: string) => apiFetch<Plan[]>(`/specs/${specId}/plans`),
  getById: (id: string) => apiFetch<Plan>(`/plans/${id}`),
  create: (specId: string, data: CreatePlanData) =>
    apiFetch<Plan>(`/specs/${specId}/plans`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  update: (id: string, data: UpdatePlanData) =>
    apiFetch<Plan>(`/plans/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
};
