import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { planApi } from '../plans';
import type { CreatePlanData, UpdatePlanData } from '../plans';

export const planKeys = {
  all: ['plans'] as const,
  bySpec: (specId: string) => [...planKeys.all, 'bySpec', specId] as const,
  detail: (id: string) => [...planKeys.all, 'detail', id] as const,
};

export function usePlans(specId: string) {
  return useQuery({
    queryKey: planKeys.bySpec(specId),
    queryFn: () => planApi.listBySpec(specId),
    enabled: !!specId,
  });
}

export function usePlan(id: string) {
  return useQuery({
    queryKey: planKeys.detail(id),
    queryFn: () => planApi.getById(id),
    enabled: !!id,
  });
}

export function useCreatePlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ specId, data }: { specId: string; data: CreatePlanData }) =>
      planApi.create(specId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: planKeys.all });
    },
  });
}

export function useUpdatePlan() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdatePlanData }) =>
      planApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: planKeys.all });
    },
  });
}
