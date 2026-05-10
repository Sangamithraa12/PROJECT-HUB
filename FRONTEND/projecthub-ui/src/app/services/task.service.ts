import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { TaskItem, CreateTaskDto, Comment } from '../models/project.model';

@Injectable({
  providedIn: 'root'
})
export class TaskService {

  private apiUrl = `${environment.apiUrl}/Task`;

  constructor(private http: HttpClient) { }

  getTasks(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(this.apiUrl);
  }

  getTaskById(id: number): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${this.apiUrl}/${id}`);
  }

  createTask(task: CreateTaskDto): Observable<TaskItem> {
    return this.http.post<TaskItem>(this.apiUrl, task);
  }

  updateTask(id: number, task: CreateTaskDto): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${this.apiUrl}/${id}`, task);
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  assignTask(taskId: number, userId: number): Observable<string> {
    return this.http.post(`${this.apiUrl}/${taskId}/assign`, userId, { responseType: 'text' });
  }

  updateStatus(taskId: number, status: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${taskId}/status`, `"${status}"`, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  addComment(taskId: number, content: string): Observable<Comment> {
    return this.http.post<Comment>(`${this.apiUrl}/${taskId}/comment`, `"${content}"`, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  addCommentWithFile(taskId: number, content: string, file: File): Observable<Comment> {
    const formData = new FormData();
    formData.append('content', content);
    formData.append('file', file);
    return this.http.post<Comment>(`${this.apiUrl}/${taskId}/comment/file`, formData);
  }

  updateComment(commentId: number, content: string): Observable<Comment> {
    return this.http.put<Comment>(`${this.apiUrl}/comment/${commentId}`, `"${content}"`, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  deleteComment(commentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/comment/${commentId}`);
  }

  submitProof(taskId: number, proofUrl: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${taskId}/proof`, `"${proofUrl}"`, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  uploadProof(taskId: number, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.apiUrl}/${taskId}/upload`, formData, {
      reportProgress: true,
      observe: 'events'
    });
  }

  uploadFolder(taskId: number, files: FileList): Observable<any> {
    const formData = new FormData();
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      const path = (file as any).webkitRelativePath || file.name;
      formData.append('files', file, path);
    }
    return this.http.post(`${this.apiUrl}/${taskId}/upload-folder`, formData, {
      reportProgress: true,
      observe: 'events'
    });
  }
}

