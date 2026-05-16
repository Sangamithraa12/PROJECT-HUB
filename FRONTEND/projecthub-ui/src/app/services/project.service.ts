import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Project, CreateProjectDto } from '../models/project.model';
import { ServiceResponse } from '../models/service-response.model';

@Injectable({
  providedIn: 'root'
})
export class ProjectService {

  private apiUrl = `${environment.apiUrl}/Project`;

  constructor(private http: HttpClient) { }

  getProjects(): Observable<ServiceResponse<Project[]>> {
    return this.http.get<ServiceResponse<Project[]>>(this.apiUrl);
  }

  getProject(id: number): Observable<ServiceResponse<Project>> {
    return this.http.get<ServiceResponse<Project>>(`${this.apiUrl}/${id}`);
  }

  createProject(project: CreateProjectDto): Observable<ServiceResponse<Project>> {
    return this.http.post<ServiceResponse<Project>>(this.apiUrl, project);
  }

  updateProject(id: number, project: CreateProjectDto): Observable<ServiceResponse<Project>> {
    return this.http.put<ServiceResponse<Project>>(`${this.apiUrl}/${id}`, project);
  }

  deleteProject(id: number): Observable<ServiceResponse<boolean>> {
    return this.http.delete<ServiceResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  uploadFile(id: number, file: File): Observable<ServiceResponse<string>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ServiceResponse<string>>(`${this.apiUrl}/${id}/upload`, formData);
  }

  uploadFolder(id: number, files: File[]): Observable<ServiceResponse<string>> {
    const formData = new FormData();
    files.forEach(file => {
      const path = (file as any).webkitRelativePath || file.name;
      formData.append('files', file, path);
    });
    return this.http.post<ServiceResponse<string>>(`${this.apiUrl}/${id}/upload-folder`, formData);
  }
}
