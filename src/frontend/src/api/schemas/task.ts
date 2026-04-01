import { z } from 'zod';

export const TaskItemStatus = z.enum(['Pending', 'InProgress', 'Completed', 'Blocked', 'Cascaded']);
export type TaskItemStatus = z.infer<typeof TaskItemStatus>;

export const TaskItemSchema = z.object({
  id: z.string().uuid(),
  planId: z.string().uuid(),
  title: z.string().min(1),
  description: z.string().nullable(),
  status: TaskItemStatus,
  assignedTo: z.string().nullable(),
  targetNodeId: z.string().uuid().nullable(),
  spawnedSpecId: z.string().uuid().nullable(),
  order: z.number().int(),
  createdAt: z.string().datetime(),
  updatedAt: z.string().datetime(),
});

export type TaskItem = z.infer<typeof TaskItemSchema>;
