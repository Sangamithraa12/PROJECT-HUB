import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Course, Enrollment } from '../models/course.model';

export interface CourseModule {
  id: number;
  courseId: number;
  title: string;
  content: string;
  orderIndex: number;
  isCompleted?: boolean;
}

export interface EnrollmentInfo extends Enrollment {
  courseTitle?: string;
  thumbnailUrl?: string;
  totalModules?: number;
  completedModules?: number;
  completedModuleIds?: number[];
}

@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private apiUrl = `${environment.apiUrl}/Course`;

  constructor(private http: HttpClient) { }

  getCourses(): Observable<Course[]> {
    return this.http.get<Course[]>(this.apiUrl);
  }

  getCourse(id: number): Observable<Course> {
    return this.http.get<Course>(`${this.apiUrl}/${id}`);
  }

  getMyCourses(userId: number): Observable<EnrollmentInfo[]> {
    return this.http.get<EnrollmentInfo[]>(`${this.apiUrl}/my-courses/${userId}`);
  }

  enroll(courseId: number, userId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/enroll/${courseId}`, userId);
  }

  createCourse(course: any): Observable<Course> {
    return this.http.post<Course>(this.apiUrl, course);
  }

  updateCourse(id: number, course: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, course);
  }

  deleteCourse(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  completeModule(enrollmentId: number, moduleId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/complete-module/${enrollmentId}/${moduleId}`, {});
  }

  submitQuiz(enrollmentId: number, score: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/submit-quiz/${enrollmentId}`, score);
  }

  updateProgress(enrollmentId: number, progress: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/update-progress/${enrollmentId}`, progress);
  }

  getTeamStats(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/team-stats`);
  }

  getTeamAchievements(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/team-achievements`);
  }

  assignCourse(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/assign`, data);
  }

  uploadCourseVideo(id: number, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/${id}/upload-video`, formData);
  }

  uploadModuleVideo(moduleId: number, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/modules/${moduleId}/upload-video`, formData);
  }
}
