import { apiFetch } from './client';
import type { TaskItem } from './schemas/task';
import type { Spec } from './schemas/spec';

export interface CreateTaskData {
  title: string;
  description?: string;
  assignedTo?: string;
  targetNodeId?: string;
  order?: number;
}

export interface UpdateTaskData {
  title?: string;
  description?: string;
  status?: string;
  assignedTo?: string;
  targetNodeId?: string;
  order?: number;
}

export interface CascadeResult {
  task: TaskItem;
  spawnedSpec: Spec;
}

export const taskApi = {
  listByPlan: (planId: string) => apiFetch<TaskItem[]>(`/plans/${planId}/tasks`),
  getById: (id: string) => apiFetch<TaskItem>(`/tasks/${id}`),
  create: (planId: string, data: CreateTaskData) =>
    apiFetch<TaskItem>(`/plans/${planId}/tasks`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  update: (id: string, data: UpdateTaskData) =>
    apiFetch<TaskItem>(`/tasks/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
  cascade: (id: string, targetNodeId: string) =>
    apiFetch<CascadeResult>(`/tasks/${id}/cascade`, {
      method: 'POST',
      body: JSON.stringify({ targetNodeId }),
    }),
};
