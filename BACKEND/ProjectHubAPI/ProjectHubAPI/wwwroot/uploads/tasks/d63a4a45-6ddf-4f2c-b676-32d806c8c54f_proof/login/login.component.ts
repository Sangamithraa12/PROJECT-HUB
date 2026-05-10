import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  username = '';
  password = '';
  errorMsg = '';
  isLoading = false;

  constructor(
    private http: HttpClient,
    private router: Router,
    private cd: ChangeDetectorRef
  ) {}

  login() {
    this.isLoading = true;
    this.errorMsg = '';

    const loginData = {
      username: this.username,
      password: this.password
    };

    this.http.post<any>('http://localhost:5148/api/Auth/login', loginData)
      .subscribe({
        next: (res) => {
          this.isLoading = false;
          localStorage.setItem('token', res.token);
          this.cd.detectChanges();
          this.router.navigate(['/tasks']);
        },
        error: (err) => {
          this.isLoading = false;
          console.error('Login Error:', err);
          
          if (err.status === 401) {
            this.errorMsg = "User not found or invalid password. Please sign up if you're new!";
          } else if (err.status === 0) {
            this.errorMsg = "Cannot connect to server. Please ensure the backend is running.";
          } else {
            this.errorMsg = "Login failed. Please check your credentials and try again.";
          }
          this.cd.detectChanges();
        }
      });
  }
}