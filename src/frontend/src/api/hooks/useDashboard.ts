import { useQuery } from '@tanstack/react-query';
import { apiFetch } from '../client';

export interface NodeStatsResponse {
  specsByStatus: Record<string, number>;
  plansByStatus: Record<string, number>;
  tasksByStatus: Record<string, number>;
  childNodeCount: number;
  activePrincipleCount: number;
}

export interface ActivityItem {
  timestamp: string;
  type: string;
  title: string;
  status: string;
  id: string;
}

export interface NodeActivityResponse {
  activities: ActivityItem[];
}

export interface OrgOverviewResponse {
  nodesByType: Record<string, number>;
  totalNodes: number;
  totalSpecs: number;
  totalPlans: number;
  totalTasks: number;
  maxCascadeDepth: number;
}

export const dashboardKeys = {
  all: ['dashboard'] as const,
  nodeStats: (nodeId: string) => [...dashboardKeys.all, 'stats', nodeId] as const,
  nodeActivity: (nodeId: string) => [...dashboardKeys.all, 'activity', nodeId] as const,
  orgOverview: () => [...dashboardKeys.all, 'org-overview'] as const,
};

export function useNodeStats(nodeId: string) {
  return useQuery({
    queryKey: dashboardKeys.nodeStats(nodeId),
    queryFn: () => apiFetch<NodeStatsResponse>(`/dashboard/node/${nodeId}/stats`),
    enabled: !!nodeId,
  });
}

export function useNodeActivity(nodeId: string) {
  return useQuery({
    queryKey: dashboardKeys.nodeActivity(nodeId),
    queryFn: () => apiFetch<NodeActivityResponse>(`/dashboard/node/${nodeId}/activity`),
    enabled: !!nodeId,
  });
}

export function useOrgOverview() {
  return useQuery({
    queryKey: dashboardKeys.orgOverview(),
    queryFn: () => apiFetch<OrgOverviewResponse>('/dashboard/org-overview'),
  });
}
