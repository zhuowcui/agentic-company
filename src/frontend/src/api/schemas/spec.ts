import { z } from 'zod';

export const SpecStatus = z.enum(['Draft', 'InReview', 'Approved', 'Rejected', 'Archived']);
export type SpecStatus = z.infer<typeof SpecStatus>;

export const SpecVersionSchema = z.object({
  id: z.string().uuid(),
  version: z.number().int(),
  content: z.string(),
  createdBy: z.string().nullable(),
  createdAt: z.string().datetime(),
});

export type SpecVersion = z.infer<typeof SpecVersionSchema>;

export const SpecSchema = z.object({
  id: z.string().uuid(),
  nodeId: z.string().uuid(),
  title: z.string().min(1),
  status: SpecStatus,
  sourceTaskId: z.string().uuid().nullable(),
  createdAt: z.string().datetime(),
  updatedAt: z.string().datetime(),
  versions: z.array(SpecVersionSchema).optional(),
});

export type Spec = z.infer<typeof SpecSchema>;
