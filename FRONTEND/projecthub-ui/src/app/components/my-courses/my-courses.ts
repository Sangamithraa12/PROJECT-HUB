import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CourseService, EnrollmentInfo } from '../../services/course.service';
import { AuthService } from '../../services/auth.service';
import { SidebarComponent } from '../../shared/sidebar/sidebar.component';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-my-courses',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent],
  templateUrl: './my-courses.html',
  styleUrls: ['./my-courses.css']
})
export class MyCoursesComponent implements OnInit {
  enrollments: EnrollmentInfo[] = [];
  isLoading: boolean = true;
  activeTab: 'all' | 'in-progress' | 'completed' = 'all';

  constructor(
    private courseService: CourseService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.loadMyCourses();
  }

  loadMyCourses(): void {
    const userId = this.auth.getUserId();
    if (!userId) {
      this.isLoading = false;
      return;
    }

    this.courseService.getMyCourses(userId).pipe(
      catchError(err => {
        console.error('Error fetching my courses:', err);
        return of([]);
      })
    ).subscribe(data => {
      this.enrollments = data;
      this.isLoading = false;
    });
  }

  get filteredCourses(): EnrollmentInfo[] {
    if (this.activeTab === 'in-progress') {
      return this.enrollments.filter(e => !e.isCompleted);
    }
    if (this.activeTab === 'completed') {
      return this.enrollments.filter(e => e.isCompleted);
    }
    return this.enrollments;
  }

  setTab(tab: 'all' | 'in-progress' | 'completed'): void {
    this.activeTab = tab;
  }
}
 
