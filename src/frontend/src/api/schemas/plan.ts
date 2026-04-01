import { z } from 'zod';

export const PlanStatus = z.enum(['Draft', 'Active', 'Completed', 'Archived']);
export type PlanStatus = z.infer<typeof PlanStatus>;

export const PlanType = z.enum(['Strategic', 'Technical']);
export type PlanType = z.infer<typeof PlanType>;

export const PlanSchema = z.object({
  id: z.string().uuid(),
  specId: z.string().uuid(),
  content: z.string(),
  planType: PlanType,
  status: PlanStatus,
  createdAt: z.string().datetime(),
  updatedAt: z.string().datetime(),
});

export type Plan = z.infer<typeof PlanSchema>;
