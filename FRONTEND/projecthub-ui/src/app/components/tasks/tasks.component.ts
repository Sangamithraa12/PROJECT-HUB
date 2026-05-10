import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TaskService } from '../../services/task.service';
import { ProjectService } from '../../services/project.service';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { SidebarComponent } from '../../shared/sidebar/sidebar.component';
import { NavbarComponent } from '../../shared/navbar/navbar';
import { of } from 'rxjs';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { TaskItem, Project, CreateTaskDto, Comment } from '../../models/project.model';
import { HttpEvent, HttpEventType } from '@angular/common/http';
import { ViewChild, ElementRef } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ChatService } from '../../services/chat.service';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, SidebarComponent, NavbarComponent],
  templateUrl: './tasks.component.html',
  styleUrls: ['./tasks.component.css']
})
export class TasksComponent implements OnInit {
  tasks: TaskItem[] = [];
  pendingTasks: TaskItem[] = [];
  inProgressTasks: TaskItem[] = [];
  completedTasks: TaskItem[] = [];
  projects: Project[] = [];
  users: any[] = [];
  filterStatus: string | null = null;
  searchTerm: string = '';

  taskStats = {
    total: 0,
    pending: 0,
    inProgress: 0,
    completed: 0,
    overdue: 0,
    dueToday: 0
  };

  draggedTask: TaskItem | null = null;

  userRole: string = '';
  currentUserId: number = 0;
  showCreateModal: boolean = false;
  showEditModal: boolean = false;
  isCreatingTask: boolean = false;
  isUpdatingTask: boolean = false;
  
  showProofModal: boolean = false;
  proofUrlInput: string = '';
  selectedFile: File | null = null;
  selectedFolder: FileList | null = null;
  submittingTaskId: number | null = null;
  isSubmittingProof: boolean = false;
  uploadProgress: number = 0;
  
  newTask: CreateTaskDto = { title: '', status: 'Pending', projectId: 0, assignedTo: 0, dueDate: '' };
  editingTask: TaskItem = { id: 0, title: '', status: '', projectId: 0, assignedTo: 0, dueDate: '' };

  editingCommentId: number | null = null;
  editCommentContent: string = '';

  constructor(
    public taskService: TaskService,
    public projectService: ProjectService,
    private userService: UserService,
    public auth: AuthService,
    private route: ActivatedRoute,
    private chatService: ChatService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.userRole = this.auth.getUserRole();
    this.currentUserId = this.auth.getUserId();

    this.route.queryParams.subscribe(params => {
      this.filterStatus = params['status'] || null;
      this.loadTasks();
    });

    this.loadProjects();
    this.loadUsers();
  }

  setFilter(status: string | null): void {
    if (this.filterStatus === status) {
      this.filterStatus = null; // Clear if clicking same card
    } else {
      this.filterStatus = status;
    }
  }

  openCreateModal(): void {
    this.loadUsers();
    this.showCreateModal = true;
  }

  loadTasks(): void {
    this.taskService.getTasks().pipe(
      timeout(30000),
      catchError(err => {
        console.error('Task loading failed:', err);
        return of([]);
      })
    ).subscribe((data: TaskItem[]) => {
      if (this.filterStatus) {
        this.tasks = data.filter(t => t.status === this.filterStatus);
      } else {
        this.tasks = data;
      }
      this.pendingTasks = this.tasks.filter(t => t.status === 'Pending');
      this.inProgressTasks = this.tasks.filter(t => t.status === 'In Progress');
      this.completedTasks = this.tasks.filter(t => t.status === 'Completed');
      this.calculateStats();
      this.cdr.detectChanges();
    });
  }

  get filteredTasks(): TaskItem[] {
    let filtered = this.tasks;
    
    // 1. Status Filter (from clickable cards)
    if (this.filterStatus) {
      if (this.filterStatus === 'Overdue') {
        const now = new Date();
        filtered = filtered.filter(t => t.status !== 'Completed' && t.dueDate && new Date(t.dueDate) < now);
      } else if (this.filterStatus === 'Due Today') {
        const todayStr = new Date().toISOString().split('T')[0];
        filtered = filtered.filter(t => t.status !== 'Completed' && t.dueDate && t.dueDate.toString().startsWith(todayStr));
      } else {
        filtered = filtered.filter(t => t.status === this.filterStatus);
      }
    }

    // 2. Search Filter
    if (this.searchTerm) {
      filtered = filtered.filter(t => 
        t.title.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        t.projectName?.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    }
    return filtered;
  }

  // Helper arrays for internal use if needed, though they now follow filteredTasks
  get pendingTasksCount(): number { return this.tasks.filter(t => t.status === 'Pending').length; }
  get inProgressTasksCount(): number { return this.tasks.filter(t => t.status === 'In Progress').length; }
  get completedTasksCount(): number { return this.tasks.filter(t => t.status === 'Completed').length; }

  calculateStats(): void {
    const now = new Date();
    const todayStr = now.toISOString().split('T')[0];

    this.taskStats = {
      total: this.tasks.length,
      pending: this.tasks.filter(t => t.status === 'Pending').length,
      inProgress: this.tasks.filter(t => t.status === 'In Progress').length,
      completed: this.tasks.filter(t => t.status === 'Completed').length,
      overdue: this.tasks.filter(t => t.status !== 'Completed' && t.dueDate && new Date(t.dueDate) < now).length,
      dueToday: this.tasks.filter(t => t.status !== 'Completed' && t.dueDate && t.dueDate.toString().startsWith(todayStr)).length
    };
  }

  isOverdue(task: TaskItem): boolean {
    if (!task.dueDate || task.status === 'Completed') return false;
    return new Date(task.dueDate) < new Date();
  }

  onDragStart(task: TaskItem): void {
    this.draggedTask = task;
  }

  onDragOver(event: Event): void {
    event.preventDefault(); // Required for drop to work
  }

  onDrop(status: string, event: Event): void {
    event.preventDefault();
    if (this.draggedTask && this.draggedTask.status !== status) {
      if (this.userRole === 'Employee' && this.draggedTask.assignedTo !== this.currentUserId) {
        return; // Employee can only move their own tasks
      }
      // Instantly update UI for perceived performance
      const prevStatus = this.draggedTask.status;
      this.draggedTask.status = status;
      this.updateLocalArrays();
      
      this.taskService.updateStatus(this.draggedTask.id, status).subscribe({
        next: () => {
          this.loadTasks(); // refresh from server
        },
        error: () => {
          // Revert on error
          if (this.draggedTask) this.draggedTask.status = prevStatus;
          this.updateLocalArrays();
          alert('Failed to update task status.');
        }
      });
      this.draggedTask = null;
    }
  }

  private updateLocalArrays(): void {
    this.pendingTasks = this.tasks.filter(t => t.status === 'Pending');
    this.inProgressTasks = this.tasks.filter(t => t.status === 'In Progress');
    this.completedTasks = this.tasks.filter(t => t.status === 'Completed');
    this.cdr.detectChanges();
  }

  loadProjects(): void {
    this.projectService.getProjects().pipe(
      timeout(30000),
      catchError(err => {
        console.error('Project loading failed:', err);
        return of([]);
      })
    ).subscribe((data: Project[]) => {
      this.projects = data;
      this.cdr.detectChanges();
    });
  }

  isLoadingUsers: boolean = false;
  userLoadError: string = '';

  loadUsers(): void {
    this.isLoadingUsers = true;
    this.userLoadError = '';
    
    this.userService.getUsers().pipe(
      timeout(30000),
      catchError(error => {
        console.error('User fetch failed or timed out:', error);
        this.userLoadError = 'Connection failed. Please check if API is running.';
        return of([]);
      }),
      finalize(() => {
        this.isLoadingUsers = false;
      })
    ).subscribe(
      (data: any[]) => {
        if (!data || !Array.isArray(data)) {
          this.users = [];
          return;
        }
        this.users = data
          .map(u => ({
            id: u.id || u.Id,
            name: u.name || u.Name || 'Unknown User',
            email: u.email || u.Email || '',
            roleName: u.roleName || u.RoleName || (u.role && u.role.name) || (u.Role && u.Role.Name) || 'Member'
          }))
          .filter(u => u.roleName.toLowerCase().includes('employee'));
        this.cdr.detectChanges();
      }
    );
  }

  createTask(): void {
    if (this.isCreatingTask) return;
    this.isCreatingTask = true;
    this.cdr.detectChanges();

    this.taskService.createTask(this.newTask).pipe(
      finalize(() => {
        this.isCreatingTask = false;
        this.cdr.detectChanges();
      })
    ).subscribe({
      next: (res) => {
        if (res) {
          this.loadTasks();
          this.showCreateModal = false;
          this.newTask = { title: '', status: 'Pending', projectId: 0, assignedTo: 0, dueDate: '' };
          alert('Task created and assigned successfully!');
        }
      },
      error: (err) => {
        console.error('Task creation failed:', err);
        alert('Failed to create task. Please check all fields.');
      }
    });
  }

  updateStatus(taskId: number, event: any): void {
    const status = event.target.value;
    this.taskService.updateStatus(taskId, status).subscribe(() => {
      this.loadTasks();
    });
  }

  openProofModal(taskId: number): void {
    this.submittingTaskId = taskId;
    this.proofUrlInput = this.tasks.find(t => t.id === taskId)?.proofUrl || '';
    this.selectedFile = null;
    this.selectedFolder = null;
    this.showProofModal = true;
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      this.selectedFolder = null;
      this.proofUrlInput = '';
    }
  }

  onFolderSelected(event: any): void {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.selectedFolder = files;
      this.selectedFile = null;
      this.proofUrlInput = '';
    }
  }

  submitProof(): void {
    if (!this.submittingTaskId) return;
    
    this.isSubmittingProof = true;
    this.uploadProgress = 0;
    
    const onProgress = (event: HttpEvent<any>) => {
      if (event.type === HttpEventType.UploadProgress) {
        this.uploadProgress = Math.round(100 * event.loaded / (event.total || 1));
        this.cdr.detectChanges();
      } else if (event.type === HttpEventType.Response) {
        this.loadTasks();
        this.closeProofModal();
      }
    };

    const onError = (error: any) => {
      console.error('Upload failed', error);
      this.isSubmittingProof = false;
      this.cdr.detectChanges();
    };

    if (this.selectedFolder) {
      this.taskService.uploadFolder(this.submittingTaskId, this.selectedFolder).pipe(
        finalize(() => {
          this.isSubmittingProof = false;
          this.cdr.detectChanges();
        })
      ).subscribe({
        next: onProgress,
        error: onError
      });
    } else if (this.selectedFile) {
      this.taskService.uploadProof(this.submittingTaskId, this.selectedFile).pipe(
        finalize(() => {
          this.isSubmittingProof = false;
          this.cdr.detectChanges();
        })
      ).subscribe({
        next: onProgress,
        error: onError
      });
    } else if (this.proofUrlInput.trim()) {
      this.taskService.submitProof(this.submittingTaskId, this.proofUrlInput).pipe(
        finalize(() => {
          this.isSubmittingProof = false;
          this.cdr.detectChanges();
        })
      ).subscribe(() => {
        this.loadTasks();
        this.closeProofModal();
      });
    } else {
      this.isSubmittingProof = false;
    }
  }

  closeProofModal(): void {
    this.showProofModal = false;
    this.submittingTaskId = null;
    this.proofUrlInput = '';
    this.selectedFile = null;
    this.selectedFolder = null;
    this.uploadProgress = 0;
  }


  startEditComment(comment: Comment): void {
    this.editingCommentId = comment.id;
    this.editCommentContent = comment.content;
  }

  cancelEdit(): void {
    this.editingCommentId = null;
    this.editCommentContent = '';
  }

  updateComment(commentId: number): void {
    if (this.editCommentContent && this.editCommentContent.trim()) {
      this.taskService.updateComment(commentId, this.editCommentContent).subscribe(() => {
        this.editingCommentId = null;
        this.editCommentContent = '';
        this.loadTasks();
      });
    }
  }

  deleteComment(commentId: number): void {
    if (confirm('Are you sure you want to delete this comment?')) {
      this.taskService.deleteComment(commentId).subscribe(() => {
        this.loadTasks();
      });
    }
  }

  deleteTask(id: number): void {
    if (confirm('Are you sure you want to delete this task?')) {
      this.taskService.deleteTask(id).subscribe(() => {
        this.loadTasks();
      });
    }
  }

  openEditModal(task: TaskItem): void {
    this.editingTask = { ...task };
    this.loadUsers();
    this.showEditModal = true;
  }

  updateTask(): void {
    if (this.isUpdatingTask) return;
    this.isUpdatingTask = true;
    this.cdr.detectChanges();

    const dto: CreateTaskDto = {
        title: this.editingTask.title,
        status: this.editingTask.status,
        projectId: this.editingTask.projectId,
        assignedTo: this.editingTask.assignedTo,
        dueDate: this.editingTask.dueDate
    };

    this.taskService.updateTask(this.editingTask.id, dto).pipe(
      timeout(30000),
      catchError(err => {
        console.error('Task update failed:', err);
        return of(null);
      }),
      finalize(() => {
        this.isUpdatingTask = false;
        this.cdr.detectChanges();
      })
    ).subscribe((res) => {
      if (res) {
        this.loadTasks();
        this.showEditModal = false;
      }
    });
  }

  logout(): void {
    this.auth.logout();
  }


  openChat(task: TaskItem): void {
    this.chatService.openChat(task);
  }

  getFullUrl(url?: string): string {
    if (!url) return '';
    return url.startsWith('http') ? url : `${environment.fileBaseUrl}${url}`;
  }

  isHighPriority(task: TaskItem): boolean {
    if (!task.dueDate || task.status === 'Completed') return false;
    const due = new Date(task.dueDate);
    const now = new Date();
    // High priority if due within 48 hours or already overdue
    const diffTime = due.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)); 
    return diffDays <= 2;
  }
}
