import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { specApi } from '../specs';
import type { CreateSpecData, UpdateSpecData } from '../specs';

export const specKeys = {
  all: ['specs'] as const,
  byNode: (nodeId: string) => [...specKeys.all, 'byNode', nodeId] as const,
  detail: (id: string) => [...specKeys.all, 'detail', id] as const,
};

export function useSpecs(nodeId: string) {
  return useQuery({
    queryKey: specKeys.byNode(nodeId),
    queryFn: () => specApi.listByNode(nodeId),
    enabled: !!nodeId,
  });
}

export function useSpec(id: string) {
  return useQuery({
    queryKey: specKeys.detail(id),
    queryFn: () => specApi.getById(id),
    enabled: !!id,
  });
}

export function useCreateSpec() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ nodeId, data }: { nodeId: string; data: CreateSpecData }) =>
      specApi.create(nodeId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: specKeys.all });
    },
  });
}

export function useUpdateSpec() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateSpecData }) =>
      specApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: specKeys.all });
    },
  });
}
