import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { nodeApi } from '../nodes';
import type { CreateNode } from '../schemas/node';

export const nodeKeys = {
  all: ['nodes'] as const,
  roots: () => [...nodeKeys.all, 'roots'] as const,
  detail: (id: string) => [...nodeKeys.all, 'detail', id] as const,
  tree: (id: string) => [...nodeKeys.all, 'tree', id] as const,
};

export function useRootNodes() {
  return useQuery({
    queryKey: nodeKeys.roots(),
    queryFn: nodeApi.listRoots,
  });
}

export function useNode(id: string) {
  return useQuery({
    queryKey: nodeKeys.detail(id),
    queryFn: () => nodeApi.getById(id),
    enabled: !!id,
  });
}

export function useNodeTree(id: string) {
  return useQuery({
    queryKey: nodeKeys.tree(id),
    queryFn: () => nodeApi.getTree(id),
    enabled: !!id,
  });
}

export function useCreateNode() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateNode) => nodeApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: nodeKeys.all });
    },
  });
}

export function useDeleteNode() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => nodeApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: nodeKeys.all });
    },
  });
}
