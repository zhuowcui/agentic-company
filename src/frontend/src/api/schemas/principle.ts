import { z } from 'zod';

export const PrincipleSchema = z.object({
  id: z.string().uuid(),
  nodeId: z.string().uuid(),
  title: z.string().min(1),
  content: z.string().min(1),
  order: z.number().int(),
  isOverride: z.boolean(),
  createdAt: z.string().datetime(),
  updatedAt: z.string().datetime(),
});

export type Principle = z.infer<typeof PrincipleSchema>;

export const EffectivePrincipleSchema = z.object({
  principle: PrincipleSchema,
  sourceNodeId: z.string().uuid(),
  isInherited: z.boolean(),
  isOverridden: z.boolean(),
});

export type EffectivePrinciple = z.infer<typeof EffectivePrincipleSchema>;
