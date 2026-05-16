import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CourseService } from '../../services/course.service';
import { AuthService } from '../../services/auth.service';
import { Enrollment } from '../../models/course.model';
import { SidebarComponent } from '../../shared/sidebar/sidebar.component';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-certificates',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent],
  templateUrl: './certificates.html',
  styleUrls: ['./certificates.css']
})
export class Certificates implements OnInit {
  certificates: Enrollment[] = [];
  isLoading: boolean = true;
  currentUser: any;
  selectedCertIdForPrint: number | null = null;

  constructor(
    private courseService: CourseService,
    private auth: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.currentUser = this.auth.currentUserValue;
    this.loadCertificates();
  }

  loadCertificates(): void {
    const userId = this.auth.getUserId();
    
    if (!userId || userId === 0) {
      this.isLoading = false;
      this.cdr.detectChanges();
      return;
    }

    this.isLoading = true;
    this.courseService.getMyCourses(userId).subscribe({
      next: (data) => {

        this.certificates = data.filter((e: any) => e.isCompleted);
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error fetching certificates:', err);
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  printCertificate(cert: Enrollment): void {
    this.selectedCertIdForPrint = cert.id;
    setTimeout(() => {
        window.print();
        this.selectedCertIdForPrint = null;
    }, 100);
  }
}
 
