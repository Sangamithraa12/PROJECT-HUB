export interface Project {
    id: number;
    name: string;
    description: string;
    dueDate?: string | Date;
    budget?: number;
    status?: string;
    filesUrl?: string;
    tasks?: TaskItem[];
}

export interface CreateProjectDto {
    name: string;
    description: string;
    dueDate?: string | Date;
    budget?: number;
    status?: string;
    filesUrl?: string;
}

export interface TaskItem {
    id: number;
    title: string;
    description?: string;
    status: string;
    projectId: number;
    projectName?: string;
    assignedTo: number;
    assignedToName?: string;
    dueDate?: string | Date;
    proofUrl?: string;
    comments?: Comment[];
}

export interface CreateTaskDto {
    title: string;
    description?: string;
    status: string;
    projectId: number;
    assignedTo: number;
    dueDate?: string | Date;
    proofUrl?: string;
}

export interface Comment {
    id: number;
    content: string;
    userId: number;
    userName?: string;
    createdAt: string | Date;
    fileUrl?: string;
    fileType?: string;
}

export interface ActivityLog {
  id: number;
  projectId: number;
  userId?: number;
  userName: string;
  action: string;
  details: string;
  type: string;
  createdAt: string;
}
