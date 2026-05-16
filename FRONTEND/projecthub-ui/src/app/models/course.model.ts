export interface CourseModule {
    id: number;
    courseId: number;
    title: string;
    content: string;
    orderIndex: number;
    isCompleted?: boolean;
}

export interface Course {
    id: number;
    title: string;
    description: string;
    thumbnailUrl: string;
    duration: string;
    category: string;
    createdAt: string | Date;
    videoUrl?: string;
    resourceUrl?: string;
    quizData?: string;
    targetRole?: string;
    modules?: CourseModule[];
}

export interface Enrollment {
    id: number;
    userId: number;
    courseId: number;
    enrolledDate: string | Date;
    status: string;
    progressPercentage: number;
    isCompleted: boolean;
    completionDate?: string | Date;
    quizScore?: number;
    course?: Course;
    courseTitle?: string;
    completedModules?: number;
    totalModules?: number;
    isMandatory?: boolean;
    assignedById?: number;
    dueDate?: string | Date;
}
