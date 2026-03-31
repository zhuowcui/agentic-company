import { z } from 'zod';

export const NodeType = z.enum(['Company', 'Organization', 'Squad', 'Team', 'Project']);
export type NodeType = z.infer<typeof NodeType>;

export interface Node {
  id: string;
  tenantId: string | null;
  parentId: string | null;
  name: string;
  type: NodeType;
  description: string | null;
  path: string;
  depth: number;
  createdAt: string;
  updatedAt: string;
  children?: Node[];
}

export const NodeSchema: z.ZodType<Node> = z.object({
  id: z.string().uuid(),
  tenantId: z.string().uuid().nullable(),
  parentId: z.string().uuid().nullable(),
  name: z.string().min(1),
  type: NodeType,
  description: z.string().nullable(),
  path: z.string(),
  depth: z.number().int(),
  createdAt: z.string().datetime(),
  updatedAt: z.string().datetime(),
  children: z.lazy(() => z.array(NodeSchema)).optional(),
});

export const CreateNodeSchema = z.object({
  parentId: z.string().uuid().nullable(),
  name: z.string().min(1, 'Name is required'),
  type: NodeType,
  description: z.string().nullable().optional(),
});

export type CreateNode = z.infer<typeof CreateNodeSchema>;
