import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ProjectsComponent } from './components/projects/projects.component';
import { TasksComponent } from './components/tasks/tasks.component';
import { Courses } from './components/courses/courses';
import { CourseDetails } from './components/course-details/course-details';
import { Certificates } from './components/certificates/certificates';


import { UsersComponent } from './components/users/users.component';
import { ProjectDetailsComponent } from './components/project-details/project-details';
import { AuthGuard } from './guards/auth.guard';
import { MyCoursesComponent } from './components/my-courses/my-courses';

export const routes: Routes = [
  { path: '', component: LoginComponent, data: { animation: 'LoginPage' } },
  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard], data: { animation: 'DashboardPage' } },
  { path: 'projects', component: ProjectsComponent, canActivate: [AuthGuard], data: { animation: 'ProjectsPage' } },
  { path: 'projects/:id', component: ProjectDetailsComponent, canActivate: [AuthGuard], data: { animation: 'ProjectDetailsPage' } },
  { path: 'tasks', component: TasksComponent, canActivate: [AuthGuard], data: { animation: 'TasksPage' } },
  { path: 'users', component: UsersComponent, canActivate: [AuthGuard], data: { animation: 'UsersPage' } },
  { path: 'courses', component: Courses, canActivate: [AuthGuard], data: { animation: 'CoursesPage' } },
  { path: 'courses/:id', component: CourseDetails, canActivate: [AuthGuard], data: { animation: 'CourseDetailsPage' } },
  { path: 'my-courses', component: MyCoursesComponent, canActivate: [AuthGuard], data: { animation: 'MyCoursesPage' } },
  { path: 'certificates', component: Certificates, canActivate: [AuthGuard], data: { animation: 'CertificatesPage' } }

];
 
