import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiFetch } from '../client';
import { dashboardKeys } from './useDashboard';
import type { Principle, EffectivePrinciple } from '../schemas/principle';

export const principleKeys = {
  all: ['principles'] as const,
  byNode: (nodeId: string) => [...principleKeys.all, 'node', nodeId] as const,
  effective: (nodeId: string) => [...principleKeys.all, 'effective', nodeId] as const,
  allEffective: [...['principles'], 'effective'] as const,
};

export function usePrinciples(nodeId: string) {
  return useQuery({
    queryKey: principleKeys.byNode(nodeId),
    queryFn: () => apiFetch<Principle[]>(`/nodes/${nodeId}/principles`),
    enabled: !!nodeId,
  });
}

export function useEffectivePrinciples(nodeId: string) {
  return useQuery({
    queryKey: principleKeys.effective(nodeId),
    queryFn: () => apiFetch<EffectivePrinciple[]>(`/nodes/${nodeId}/principles/effective`),
    enabled: !!nodeId,
  });
}

export function useCreatePrinciple(nodeId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: { title: string; content: string; order: number; isOverride?: boolean }) =>
      apiFetch<Principle>(`/nodes/${nodeId}/principles`, {
        method: 'POST',
        body: JSON.stringify(data),
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: principleKeys.byNode(nodeId) });
      // Invalidate ALL effective principle queries (descendants inherit from ancestors)
      queryClient.invalidateQueries({ queryKey: principleKeys.allEffective });
      queryClient.invalidateQueries({ queryKey: dashboardKeys.all });
    },
  });
}
