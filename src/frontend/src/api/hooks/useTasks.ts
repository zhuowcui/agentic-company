import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { taskApi } from '../tasks';
import type { CreateTaskData, UpdateTaskData } from '../tasks';

export const taskKeys = {
  all: ['tasks'] as const,
  byPlan: (planId: string) => [...taskKeys.all, 'byPlan', planId] as const,
  detail: (id: string) => [...taskKeys.all, 'detail', id] as const,
};

export function useTasks(planId: string) {
  return useQuery({
    queryKey: taskKeys.byPlan(planId),
    queryFn: () => taskApi.listByPlan(planId),
    enabled: !!planId,
  });
}

export function useTask(id: string) {
  return useQuery({
    queryKey: taskKeys.detail(id),
    queryFn: () => taskApi.getById(id),
    enabled: !!id,
  });
}

export function useCreateTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ planId, data }: { planId: string; data: CreateTaskData }) =>
      taskApi.create(planId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: taskKeys.all });
    },
  });
}

export function useUpdateTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTaskData }) =>
      taskApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: taskKeys.all });
    },
  });
}

export function useCascadeTask() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, targetNodeId }: { id: string; targetNodeId: string }) =>
      taskApi.cascade(id, targetNodeId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: taskKeys.all });
    },
  });
}
