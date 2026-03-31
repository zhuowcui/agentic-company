import { z } from 'zod';

export const PlanStatus = z.enum(['Draft', 'Active', 'Completed']);
export type PlanStatus = z.infer<typeof PlanStatus>;

export const PlanSchema = z.object({
  id: z.string().uuid(),
  specId: z.string().uuid(),
  title: z.string().min(1),
  description: z.string().nullable(),
  status: PlanStatus,
  createdAt: z.string().datetime(),
  updatedAt: z.string().datetime(),
});

export type Plan = z.infer<typeof PlanSchema>;
