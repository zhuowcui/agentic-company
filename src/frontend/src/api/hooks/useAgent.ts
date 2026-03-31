import { useMutation } from '@tanstack/react-query';
import { agentApi } from '../agent';
import type {
  DraftSpecRequest,
  DraftPlanRequest,
  SuggestCascadeRequest,
  ReviewSpecRequest,
} from '../agent';

export function useDraftSpec() {
  return useMutation({
    mutationFn: (data: DraftSpecRequest) => agentApi.draftSpec(data),
  });
}

export function useDraftPlan() {
  return useMutation({
    mutationFn: (data: DraftPlanRequest) => agentApi.draftPlan(data),
  });
}

export function useSuggestCascade() {
  return useMutation({
    mutationFn: (data: SuggestCascadeRequest) => agentApi.suggestCascade(data),
  });
}

export function useReviewSpec() {
  return useMutation({
    mutationFn: (data: ReviewSpecRequest) => agentApi.reviewSpec(data),
  });
}
