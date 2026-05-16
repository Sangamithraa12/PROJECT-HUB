import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CourseService } from '../../services/course.service';
import { AuthService } from '../../services/auth.service';
import { Course, Enrollment, CourseModule } from '../../models/course.model';
import { SidebarComponent } from '../../shared/sidebar/sidebar.component';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { of } from 'rxjs';
import { SafeHtmlPipe } from '../../pipes/safe.pipe';

@Component({
  selector: 'app-course-details',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent, SafeHtmlPipe],
  templateUrl: './course-details.html',
  styleUrls: ['./course-details.css']
})
export class CourseDetails implements OnInit {
  courseId: number = 0;
  course?: Course;
  enrollment?: any;
  isLoading: boolean = true;
  safeVideoUrl?: SafeResourceUrl;
  activeModule?: CourseModule;
  
  showQuiz: boolean = false;
  quizQuestions: any[] = [];
  quizStep: number = 0;
  quizScore: number = 0;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private courseService: CourseService,
    private auth: AuthService,
    private sanitizer: DomSanitizer,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.courseId = +params['id'];
      this.loadCourseData();
    });
  }

  loadCourseData(): void {
    this.isLoading = true;

    
    this.courseService.getCourse(this.courseId).pipe(
      timeout(3000),
      catchError(err => {

        this.isLoading = false;
        return of(null);
      })
    ).subscribe(course => {
      if (course) {

        this.course = course;
        if (this.course.modules && this.course.modules.length > 0) {
            this.activeModule = this.course.modules[0];
        }
        this.processVideoUrl();
        this.parseQuizData();
        this.checkEnrollment();
      } else {

        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  processVideoUrl(overrideUrl?: string): void {
    const urlToCheck = (overrideUrl || this.course?.videoUrl || '').trim();
    if (!urlToCheck) {
      this.safeVideoUrl = undefined;
      return;
    }

    let videoId = '';
    const ytRegex = /(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^"&?\/\s>]{11})/i;
    const match = urlToCheck.match(ytRegex);
    
    if (match && match[1]) {
      videoId = match[1];
    } else if (urlToCheck.length === 11) {
      videoId = urlToCheck; 
    }

    if (videoId && videoId.length === 11) {
      this.safeVideoUrl = this.sanitizer.bypassSecurityTrustResourceUrl(`https://www.youtube.com/embed/${videoId}?autoplay=0&rel=0`);
    } else {
      this.safeVideoUrl = undefined;
    }
    this.cdr.detectChanges();
  }

  parseQuizData(): void {
    if (this.course?.quizData && this.course.quizData.length > 5) {
      try {
        this.quizQuestions = JSON.parse(this.course.quizData);

      } catch (e) {

      }
    }
  }

  checkEnrollment(): void {
    const userId = this.auth.getUserId();

    
    if (!userId || userId === 0) {

      this.isLoading = false;
      return;
    }

    this.courseService.getMyCourses(userId).pipe(
      timeout(10000),
      catchError(err => {

        return of([]);
      }),
      finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      })
    ).subscribe(enrollments => {
      this.enrollment = enrollments.find(e => e.courseId === this.courseId);
      if (this.enrollment) {
        if (this.enrollment.progressPercentage === undefined) {
          this.enrollment.progressPercentage = this.enrollment.isCompleted ? 100 : 0;
        }
      }
      this.isLoading = false;
      this.cdr.detectChanges();
    });
  }

  enroll(): void {
    const userId = this.auth.getUserId();
    if (!userId || userId === 0 || !this.course) return;

    this.isLoading = true;
    this.courseService.enroll(this.course.id, userId).subscribe({
      next: (res) => {

        this.checkEnrollment();
      },
      error: (err) => {

        this.isLoading = false;
        alert('Failed to enroll in course. Please try again.');
        this.cdr.detectChanges();
      }
    });
  }

  isManager(): boolean {
    return this.auth.getUserRole() === 'Manager' || this.auth.getUserRole() === 'Admin';
  }

  selectedFile: File | null = null;
  uploading = false;
  uploadProgress = 0;

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
  }

  uploadVideo(courseLevel: boolean = true): void {
    if (!this.selectedFile || !this.course) return;

    this.uploading = true;
    this.uploadProgress = 30;

    const uploadObs = courseLevel 
      ? this.courseService.uploadCourseVideo(this.course.id, this.selectedFile)
      : (this.activeModule ? this.courseService.uploadModuleVideo(this.activeModule.id, this.selectedFile) : null);

    if (!uploadObs) {
      this.uploading = false;
      return;
    }

    uploadObs.subscribe({
      next: (res: any) => {
        this.uploadProgress = 100;
        setTimeout(() => {
          this.uploading = false;
          this.selectedFile = null;
          if (courseLevel && this.course) {
            this.course.videoUrl = res.url;
            this.processVideoUrl();
          }
          else if (this.activeModule) this.activeModule.content = res.url;
          this.cdr.detectChanges();
          alert('Video uploaded successfully! 📹');
        }, 800);
      },
      error: (err) => {

        this.uploading = false;
        alert('Upload failed. Please try a smaller file.');
      }
    });
  }

  updateYouTubeUrl(url: string): void {
    if (!url || !this.course) return;

    if (!url.includes('youtube.com') && !url.includes('youtu.be')) {
      alert('Please enter a valid YouTube link.');
      return;
    }

    this.isLoading = true;
    const updatedCourse = { ...this.course, videoUrl: url };
    this.courseService.updateCourse(this.course.id, updatedCourse).subscribe({
      next: () => {
        if (this.course) this.course.videoUrl = url;
        this.processVideoUrl();
        this.isLoading = false;
        this.cdr.detectChanges();
        alert('YouTube URL updated successfully! 📺');
      },
      error: (err) => {

        this.isLoading = false;
        alert('Failed to update URL. Please check the server connection.');
      }
    });
  }

  updateQuizData(json: string): void {
    if (!json || !this.course) return;
    
    try {
      JSON.parse(json); // Validate JSON
      this.isLoading = true;
      const updatedCourse = { ...this.course, quizData: json };
      this.courseService.updateCourse(this.course.id, updatedCourse).subscribe({
        next: () => {
          if (this.course) this.course.quizData = json;
          this.parseQuizData();
          this.isLoading = false;
          this.cdr.detectChanges();
          alert('Quiz data updated successfully! 📝');
        },
        error: (err) => {

          this.isLoading = false;
          alert('Failed to update quiz data.');
        }
      });
    } catch (e) {
      alert('Invalid JSON format. Please check your quiz data.');
    }
  }

  startQuiz(): void {
    this.showQuiz = true;
    this.quizStep = 0;
    this.quizScore = 0;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  answerQuiz(isCorrect: boolean): void {

    if (isCorrect === true || isCorrect === 'true' as any) {
        this.quizScore++;
    }
    
    if (this.quizStep < this.quizQuestions.length - 1) {
      this.quizStep++;
      this.cdr.detectChanges();
    } else {
      this.completeQuiz();
    }
  }

  completeQuiz(): void {
    const finalScore = Math.round((this.quizScore / this.quizQuestions.length) * 100);
    if (this.enrollment) {
      this.courseService.submitQuiz(this.enrollment.id, finalScore).subscribe(() => {
        alert(`Quiz Complete! Your Score: ${finalScore}%`);
        this.showQuiz = false;
        if (finalScore >= 70) {
          this.markAsComplete();
        }
      });
    }
  }

  markAsComplete(): void {
    if (this.enrollment && !this.enrollment.isCompleted) {
      this.courseService.updateProgress(this.enrollment.id, 100).subscribe((res: any) => {
        this.enrollment.progressPercentage = 100;
        this.enrollment.isCompleted = true;
        this.enrollment.completionDate = new Date();
        this.cdr.detectChanges();
        alert('Congratulations! You have completed the course and earned a certificate! 🏆');
      });
    }
  }

  selectModule(mod: CourseModule): void {
    this.activeModule = mod;
    if (mod.content && (mod.content.includes('http') || mod.content.length === 11)) {
        this.processVideoUrl(mod.content);
    } else {
        this.processVideoUrl(); // Reset to course video if module has no link
    }
  }

  get isActiveModuleCompleted(): boolean {
    if (!this.activeModule || !this.enrollment?.completedModuleIds) return false;
    return this.enrollment.completedModuleIds.includes(this.activeModule.id);
  }

  get safeModuleContent(): any {
    if (!this.activeModule?.content) return null;
    return this.sanitizer.bypassSecurityTrustHtml(this.activeModule.content);
  }

  markModuleComplete(): void {
    if (!this.activeModule || !this.enrollment) return;
    
    const currentEnrollment = this.enrollment;
    const currentModule = this.activeModule;
    
    this.courseService.completeModule(currentEnrollment.id, currentModule.id).subscribe((res: any) => {
       const ids = currentEnrollment.completedModuleIds || [];
       if (!ids.includes(currentModule.id)) {
           ids.push(currentModule.id);
       }
       currentEnrollment.completedModuleIds = ids;
       currentEnrollment.progressPercentage = res.progress;
       
       if (res.status === 'Completed') {
           currentEnrollment.isCompleted = true;
           alert('Congratulations! You have completed all modules and the course! 🏆');
       } else {
           alert('Module marked as completed!');
       }
    });
  }
}
