import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ProjectService } from '../../services/project.service';
import { TaskService } from '../../services/task.service';
import { CourseService } from '../../services/course.service';
import { Leaderboard } from '../leaderboard/leaderboard';


import { SidebarComponent } from '../../shared/sidebar/sidebar.component';
import { NavbarComponent } from '../../shared/navbar/navbar';
import { RippleDirective } from '../../shared/directives/ripple.directive';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent, NavbarComponent, Leaderboard, RippleDirective],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  currentUser: any;
  userRole: string = '';
  stats = {
    projects: 0,
    tasks: 0,


    completedTasks: 0,
    pendingTasks: 0
  };
  myTasks: any[] = [];
  myEnrollments: any[] = [];
  isLoadingStats: boolean = true;
  isLoadingTasks: boolean = true;
  isLoadingLearning: boolean = false;
  isLoadingTeam: boolean = false;
  teamStats: any = null;
  teamAchievements: any[] = [];

  constructor(
    private auth: AuthService,
    private projectService: ProjectService,
    private taskService: TaskService,
    private courseService: CourseService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.currentUser = this.auth.currentUserValue;
    this.userRole = this.auth.getUserRole();
    this.loadStats();
    if (this.userRole === 'Employee') {
      this.loadMyLearning();
    } else if (this.userRole === 'Manager' || this.userRole === 'Admin') {
      this.loadTeamStats();
    }
  }

  loadStats(): void {
    this.projectService.getProjects().pipe(
      timeout(30000),
      catchError(err => {
        console.error('Dashboard Project load failed:', err);
        return of({ success: false, data: [] });
      }),
      finalize(() => {
        this.isLoadingStats = false;
        this.cdr.detectChanges();
      })
    ).subscribe((result: any) => {
      if (result.success) {
        this.stats.projects = result.data?.length || 0;
      }
      this.cdr.detectChanges();
    });

    this.taskService.getTasks().pipe(
      timeout(30000),
      catchError(err => {
        console.error('Dashboard Task load failed:', err);
        return of([]);
      }),
      finalize(() => {
        this.isLoadingTasks = false;
        this.cdr.detectChanges();
      })
    ).subscribe((tasks: any) => {
      this.stats.tasks = tasks.length;
      this.stats.completedTasks = tasks.filter((t: any) => t.status === 'Completed').length;
      this.stats.pendingTasks = tasks.length - this.stats.completedTasks;

      const userId = this.auth.getUserId();
      this.myTasks = tasks.filter((t: any) => t.assignedTo === userId);
      
      this.cdr.detectChanges();
    });
  }

  loadMyLearning(): void {
    const userId = this.auth.getUserId();
    this.isLoadingLearning = true;
    this.courseService.getMyCourses(userId).pipe(
      finalize(() => {
        this.isLoadingLearning = false;
        this.cdr.detectChanges();
      })
    ).subscribe(data => {
      this.myEnrollments = data;
    });
  }

  loadTeamStats(): void {
    this.isLoadingTeam = true;
    this.courseService.getTeamStats().pipe(
      finalize(() => {
        this.isLoadingTeam = false;
        this.cdr.detectChanges();
      })
    ).subscribe(stats => {
      this.teamStats = stats;
      this.cdr.detectChanges();
    });

    this.loadTeamAchievements();
  }

  loadTeamAchievements(): void {
    this.courseService.getTeamAchievements().subscribe(achievements => {
      this.teamAchievements = achievements;
      this.cdr.detectChanges();
    });
  }

  getDaysRemaining(dueDate: string | Date): string {
    if (!dueDate) return '';
    const due = new Date(dueDate);
    const now = new Date();
    const diffTime = due.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    if (diffDays < 0) return 'Overdue';
    if (diffDays === 0) return 'Due today';
    if (diffDays === 1) return 'Due tomorrow';
    return `${diffDays} days left`;
  }

  logout(): void {
    this.auth.logout();
  }

  navigateToDetails(type: string): void {
    switch (type) {
      case 'projects':
        this.router.navigate(['/projects']);
        break;
      case 'active':
        this.router.navigate(['/tasks']);
        break;
      case 'completed':
        this.router.navigate(['/tasks'], { queryParams: { status: 'Completed' } });
        break;
      case 'pending':
        this.router.navigate(['/tasks'], { queryParams: { status: 'Pending' } });
        break;
    }
  }
}

