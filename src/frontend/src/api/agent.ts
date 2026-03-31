import { apiFetch } from './client';

// --- Request types ---

export interface DraftSpecRequest {
  nodeId: string;
  prompt: string;
  provider?: string;
}

export interface DraftPlanRequest {
  specId: string;
  prompt?: string;
  provider?: string;
}

export interface SuggestCascadeRequest {
  taskId: string;
  provider?: string;
}

export interface ReviewSpecRequest {
  specId: string;
  provider?: string;
}

// --- Response types ---

export interface DraftSpecResponse {
  draft: string;
}

export interface SuggestedTaskItem {
  title: string;
  description: string;
}

export interface DraftPlanResponse {
  plan: string;
  suggestedTasks: SuggestedTaskItem[];
}

export interface SuggestCascadeResponse {
  suggestedNodeId: string | null;
  suggestedNodeName: string | null;
  draftSpec: string;
}

export interface ReviewSpecResponse {
  aligned: boolean;
  score: number;
  feedback: string;
  suggestions: string[];
}

// --- API functions ---

export const agentApi = {
  draftSpec: (data: DraftSpecRequest) =>
    apiFetch<DraftSpecResponse>('/agent/draft-spec', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  draftPlan: (data: DraftPlanRequest) =>
    apiFetch<DraftPlanResponse>('/agent/draft-plan', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  suggestCascade: (data: SuggestCascadeRequest) =>
    apiFetch<SuggestCascadeResponse>('/agent/suggest-cascade', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
  reviewSpec: (data: ReviewSpecRequest) =>
    apiFetch<ReviewSpecResponse>('/agent/review-spec', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
};
