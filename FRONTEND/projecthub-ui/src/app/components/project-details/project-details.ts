import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '../../services/project.service';
import { TaskService } from '../../services/task.service';
import { AuthService } from '../../services/auth.service';
import { Project, TaskItem } from '../../models/project.model';
import { SidebarComponent } from '../../shared/sidebar/sidebar.component';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { of } from 'rxjs';

import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-project-details',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, SidebarComponent],
  templateUrl: './project-details.html',
  styleUrls: ['./project-details.css']
})
export class ProjectDetailsComponent implements OnInit {
  projectId: number = 0;
  project?: Project;
  isLoading: boolean = true;
  isUploading: boolean = false;
  userRole: string = '';
  errorMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private projectService: ProjectService,
    public taskService: TaskService, // Inject TaskService
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  showTaskDrawer: boolean = false;
  showTeamDrawer: boolean = false;
  activeTask: TaskItem | null = null;
  isUpdatingTaskStatus: boolean = false;
  currentUserId: number = 0;

  teamMetrics: any[] = []; 

  ngOnInit(): void {
    this.userRole = this.auth.getUserRole();
    this.route.params.subscribe(params => {
      this.projectId = +params['id'];
      this.loadProjectDetails();
    });
  }

  loadProjectDetails(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.projectService.getProject(this.projectId).pipe(
      timeout(5000),
      catchError(err => {
        console.error('Error loading project details:', err);
        this.errorMessage = 'Could not load project details. Please ensure the backend API is running and migrations are applied.';
        this.isLoading = false;
        this.cdr.detectChanges();
        return of(undefined);
      }),
      finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
      })
    ).subscribe(response => {

      this.project = response?.data;
      if (!response?.data && !this.errorMessage) {
        this.errorMessage = 'Project not found.';
      }
      this.isLoading = false;
      this.cdr.detectChanges();

    });
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.uploadFile(file);
    }
  }

  onFolderSelected(event: any): void {
    const files: FileList = event.target.files;
    if (files && files.length > 0) {
      this.uploadFolder(Array.from(files));
    }
  }

  uploadFile(file: File): void {
    this.isUploading = true;
    this.projectService.uploadFile(this.projectId, file).pipe(
      finalize(() => {
          this.isUploading = false;
          this.cdr.detectChanges();
      })
    ).subscribe({
      next: (res) => {
        if (this.project) {
          this.project.filesUrl = res.data;
        }
        this.cdr.detectChanges();
        alert('File uploaded successfully!');
      },
      error: (err) => {
        console.error('Upload failed:', err);
        alert('Upload failed. Please try again.');
      }
    });
  }

  uploadFolder(files: File[]): void {
    this.isUploading = true;
    this.projectService.uploadFolder(this.projectId, files).pipe(
      finalize(() => {
          this.isUploading = false;
          this.cdr.detectChanges();
      })
    ).subscribe({
      next: (res) => {
        if (this.project) {
          this.project.filesUrl = res.data;
        }
        this.cdr.detectChanges();
        alert('Folder uploaded successfully!');
      },
      error: (err) => {
        console.error('Upload failed:', err);
        alert('Upload failed. Please try again.');
      }
    });
  }

  getFileName(url?: string): string {
    if (!url) return '';
    return url.split('/').pop() || 'Uploaded Files';
  }

  isFolder(url?: string): boolean {
    if (!url) return false;
    return url.includes('_files') || url.includes('_proof');
  }

  hasTaskProofs(): boolean {
    return !!this.project?.tasks?.some(t => t.proofUrl);
  }

  getFullUrl(url?: string): string {
    if (!url) return '';
    return url.startsWith('http') ? url : `${environment.fileBaseUrl}${url}`;
  }


  openTaskDrawer(task: TaskItem): void {
    this.activeTask = { ...task }; 
    this.showTaskDrawer = true;
    this.currentUserId = this.auth.getUserId();
    
   
    this.taskService.getTaskById(task.id).subscribe(fullTask => {
      this.activeTask = fullTask;
      this.cdr.detectChanges();
    });
  }

  closeTaskDrawer(): void {
    this.showTaskDrawer = false;
    this.activeTask = null;
  }

  updateTaskStatus(event: any): void {
    if (!this.activeTask) return;
    const newStatus = event.target.value;
    this.isUpdatingTaskStatus = true;
    
    this.taskService.updateStatus(this.activeTask.id, newStatus).pipe(
      finalize(() => {
        this.isUpdatingTaskStatus = false;
        this.cdr.detectChanges();
      })
    ).subscribe(() => {
      if (this.activeTask) {
        const taskId = this.activeTask.id;
        this.activeTask.status = newStatus;

        if (this.project?.tasks) {
          const task = this.project.tasks.find(t => t.id === taskId);
          if (task) task.status = newStatus;
        }
      }
    });
  }

  isHighPriorityTask(task: TaskItem): boolean {
    if (!task.dueDate || task.status === 'Completed') return false;
    const due = new Date(task.dueDate);
    const now = new Date();
    const diffTime = due.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)); 
    return diffDays <= 2;
  }

  
  getTaskStats() {
    if (!this.project?.tasks) return { total: 0, completed: 0, percentage: 0 };
    const total = this.project.tasks.length;
    const completed = this.project.tasks.filter(t => t.status === 'Completed').length;
    const percentage = total > 0 ? Math.round((completed / total) * 100) : 0;
    return { total, completed, percentage };
  }

  getTeamSize(): number {
    if (!this.project?.tasks) return 0;
    const assignees = new Set(this.project.tasks.map(t => t.assignedTo).filter(id => !!id));
    return assignees.size;
  }

  getDaysRemaining(): number {
    if (!this.project?.dueDate) return 0;
    const due = new Date(this.project.dueDate);
    const now = new Date();
    const diff = due.getTime() - now.getTime();
    return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
  }

  getPriorityClass(): string {

    const days = this.getDaysRemaining();
    if (days <= 3) return 'high-priority';
    if (days <= 7) return 'mid-priority';
    return 'normal-priority';
  }


  scrollToTasks(): void {
    const element = document.querySelector('.tasks-section');
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }

  openTeamDrawer(): void {
    this.calculateTeamMetrics();
    this.showTeamDrawer = true;
  }

  closeTeamDrawer(): void {
    this.showTeamDrawer = false;
  }

  private calculateTeamMetrics(): void {
    if (!this.project?.tasks) return;

    const userMap = new Map<number, any>();

    this.project.tasks.forEach(task => {
      const userId = task.assignedTo;
      if (!userId) return;

      if (!userMap.has(userId)) {
        userMap.set(userId, {
          userId: userId,
          userName: task.assignedToName || 'Unknown',
          totalTasks: 0,
          completedTasks: 0
        });
      }

      const metrics = userMap.get(userId);
      metrics.totalTasks++;
      if (task.status === 'Completed') {
        metrics.completedTasks++;
      }
    });

    this.teamMetrics = Array.from(userMap.values()).map(m => ({
      ...m,
      completionRate: m.totalTasks > 0 ? Math.round((m.completedTasks / m.totalTasks) * 100) : 0
    })).sort((a, b) => b.completionRate - a.completionRate);
  }
}
