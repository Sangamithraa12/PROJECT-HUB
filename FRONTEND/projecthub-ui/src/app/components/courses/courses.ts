import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { CourseService } from '../../services/course.service';
import { AuthService } from '../../services/auth.service';
import { Course, Enrollment } from '../../models/course.model';
import { SidebarComponent } from '../../shared/sidebar/sidebar.component';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

import { FormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-courses',
  standalone: true,
  imports: [CommonModule, SidebarComponent, FormsModule, RouterModule],
  templateUrl: './courses.html',
  styleUrls: ['./courses.css']
})
export class Courses implements OnInit {
  courses: Course[] = [];
  myEnrollments: Enrollment[] = [];
  users: any[] = [];
  isLoadingAll: boolean = false;
  currentUserId: number = 0;
  userRole: string = '';

  showCreateModal: boolean = false;
  showEditModal: boolean = false;
  showAssignModal: boolean = false;
  isSaving: boolean = false;

  newCourse: any = { title: '', description: '', thumbnailUrl: '', duration: '', category: 'Development', videoUrl: '', resourceUrl: '', quizData: '' };
  editingCourse: Course = { id: 0, title: '', description: '', thumbnailUrl: '', duration: '', category: 'Development', createdAt: '', videoUrl: '', resourceUrl: '', quizData: '' };
  
  assignmentData = { courseId: 0, userId: 0, assignedById: 0, dueDate: '' };

  constructor(
    private courseService: CourseService,
    private userService: UserService,
    private auth: AuthService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.auth.getUserId();
    this.userRole = this.auth.getUserRole();
    this.loadCourses();
    this.loadMyEnrollments();
    if (this.userRole === 'Manager' || this.userRole === 'Admin') {
      this.loadUsers();
    }
  }

  loadUsers(): void {
    this.userService.getUsers().subscribe(data => {

      this.users = data;
      this.cdr.detectChanges();
    });
  }

  loadCourses(): void {
    this.isLoadingAll = true;
    this.courseService.getCourses()
      .pipe(
        catchError(() => of([])),
        finalize(() => {
          this.isLoadingAll = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe(data => {
        if (this.userRole === 'Employee') {
          this.courses = data.filter((c: any) => c.targetRole !== 'Manager');
        } else {
          this.courses = data;
        }
      });
  }

  loadMyEnrollments(): void {
    this.courseService.getMyCourses(this.currentUserId)
      .subscribe(data => {
        this.myEnrollments = data;
        this.cdr.detectChanges();
      });
  }

  isEnrolled(courseId: number): boolean {
    return this.myEnrollments.some(e => e.courseId === courseId);
  }

  enroll(courseId: number): void {
    this.courseService.enroll(courseId, this.currentUserId)
      .subscribe(() => {
        this.loadMyEnrollments();
        this.router.navigate(['/courses', courseId]);
      });
  }

  goToDetails(courseId: number, event?: Event): void {
    if (event) {
      const target = event.target as HTMLElement;
      if (target.tagName.toLowerCase() === 'button' || target.closest('button')) {
        return; // Prevent navigation if a button was clicked
      }
    }
    this.router.navigate(['/courses', courseId]);
  }

  openCreateModal(): void {
    this.newCourse = { title: '', description: '', thumbnailUrl: '', duration: '', category: 'Development', videoUrl: '', resourceUrl: '', quizData: '' };
    this.showCreateModal = true;
  }

  createCourse(): void {
    if (this.isSaving) return;
    this.isSaving = true;
    this.courseService.createCourse(this.newCourse)
      .pipe(
        catchError((err) => {
          console.error('Error creating course:', err);
          alert('Failed to create course. Please ensure database migrations are applied.');
          return of(null);
        }),
        finalize(() => this.isSaving = false)
      )
      .subscribe((res: any) => {
        if (res) {
          this.loadCourses();
          this.showCreateModal = false;
        }
      });
  }

  openEditModal(course: Course): void {
    this.editingCourse = { ...course };
    this.showEditModal = true;
  }

  updateCourse(): void {
    if (this.isSaving) return;
    this.isSaving = true;
    this.courseService.updateCourse(this.editingCourse.id, this.editingCourse)
      .pipe(
        catchError((err) => {
          console.error('Error updating course:', err);
          alert('Failed to update course.');
          return of(null);
        }),
        finalize(() => this.isSaving = false)
      )
      .subscribe((res: any) => {
        if (res !== null) {
          this.loadCourses();
          this.showEditModal = false;
        }
      });
  }

  deleteCourse(id: number): void {
    if (confirm('Are you sure you want to delete this course? All enrollments will also be removed.')) {
      this.courseService.deleteCourse(id).subscribe(() => {
        this.loadCourses();
      });
    }
  }

  openAssignModal(course: Course): void {
    this.assignmentData = { 
      courseId: course.id, 
      userId: 0, 
      assignedById: this.currentUserId, 
      dueDate: '' 
    };
    this.showAssignModal = true;
  }

  submitAssignment(): void {
    if (this.isSaving || !this.assignmentData.userId || !this.assignmentData.dueDate) return;
    this.isSaving = true;
    this.courseService.assignCourse(this.assignmentData)
      .pipe(
        catchError(err => {
          console.error('Error assigning course:', err);
          alert('Failed to assign course.');
          return of(null);
        }),
        finalize(() => {
          this.isSaving = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe(res => {
        if (res) {
          alert('Course successfully assigned to team member!');
          this.showAssignModal = false;
        }
      });
  }
}
