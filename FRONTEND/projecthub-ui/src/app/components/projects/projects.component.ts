import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProjectService } from '../../services/project.service';
import { AuthService } from '../../services/auth.service';
import { SidebarComponent } from '../../shared/sidebar/sidebar.component';
import { of } from 'rxjs';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { Project, CreateProjectDto } from '../../models/project.model';
import { RealTimeChatService } from '../../services/real-time-chat.service';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, SidebarComponent],
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.css']
})
export class ProjectsComponent implements OnInit {
  projects: Project[] = [];
  userRole: string = '';
  showCreateModal: boolean = false;
  showEditModal: boolean = false;
  isProcessing: boolean = false;
  
  newProject: CreateProjectDto = { name: '', description: '', dueDate: '' };
  editingProject: Project = { id: 0, name: '', description: '', dueDate: '' };

  constructor(
    private projectService: ProjectService,
    public auth: AuthService,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private realTimeChat: RealTimeChatService
  ) {}

  ngOnInit(): void {
    this.userRole = this.auth.getUserRole();
    this.loadProjects();

    this.realTimeChat.refreshProjects$.subscribe(shouldRefresh => {
      if (shouldRefresh) {
        this.loadProjects();
      }
    });
  }

  loadProjects(): void {
    this.projectService.getProjects().pipe(
      timeout(30000),
      catchError(err => {
        console.error('Project loading failed:', err);
        return of({ success: false, data: [] as Project[], message: '' });
      })
    ).subscribe((response) => {
      this.projects = response.data ?? [];
      this.cdr.detectChanges();
    });
  }

  createProject(): void {
    if (this.isProcessing) return;
    this.isProcessing = true;
    this.projectService.createProject(this.newProject).pipe(
      finalize(() => this.isProcessing = false)
    ).subscribe({
      next: () => {
        this.loadProjects();
        this.showCreateModal = false;
        this.newProject = { name: '', description: '', dueDate: '' };
      },
      error: (err) => {
        console.error('Project creation failed:', err);
        alert('Failed to create project. Please try again.');
      }
    });
  }

  openEditModal(project: Project): void {
    this.editingProject = { ...project };
    this.showEditModal = true;
  }

  updateProject(): void {
    if (this.isProcessing) return;
    this.isProcessing = true;
    const dto: CreateProjectDto = {
        name: this.editingProject.name,
        description: this.editingProject.description,
        dueDate: this.editingProject.dueDate
    };
    this.projectService.updateProject(this.editingProject.id, dto).pipe(
      finalize(() => this.isProcessing = false)
    ).subscribe({
      next: () => {
        this.loadProjects();
        this.showEditModal = false;
      },
      error: (err) => {
        console.error('Project update failed:', err);
        alert('Failed to update project.');
      }
    });
  }

  deleteProject(id: number): void {
    if (confirm('Are you sure you want to delete this project?')) {
      this.projectService.deleteProject(id).subscribe(() => {
        this.loadProjects();
      });
    }
  }

  viewProject(id: number): void {
    this.router.navigate(['/projects', id]);
  }
}
 
