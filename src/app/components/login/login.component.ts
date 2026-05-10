import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email = '';
  password = '';
  errorMessage = '';
  isLoading = false;

  constructor(
    private auth: AuthService, 
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  login() {
    console.log('Login attempt started for:', this.email);
    this.errorMessage = '';
    this.isLoading = true;

    const data = {
      email: this.email,
      password: this.password
    };

    this.auth.login(data).subscribe({
      next: (res) => {
        console.log('Login successful', res);
        this.isLoading = false;
        this.cdr.detectChanges();
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        console.error('Login Error Object:', err);
        this.isLoading = false;
        
        if (err.status === 401) {
          this.errorMessage = 'Invalid email or password';
        } else if (err.status === 0 || err.name === 'TimeoutError' || err === 'Request timed out. Please check if the API is running.') {
          this.errorMessage = 'Cannot connect to server. Please ensure the backend is running.';
        } else {
          this.errorMessage = err.error?.message || err.error || 'An unexpected error occurred';
        }
        
        console.log('Component Status - isLoading:', this.isLoading, 'errorMessage:', this.errorMessage);
        this.cdr.detectChanges(); 
      }
    });
  }
}

