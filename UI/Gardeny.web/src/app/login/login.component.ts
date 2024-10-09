import { Component, OnInit, inject } from '@angular/core';
import { AuthService } from '../auth.service';
import { UserLogin } from '../Models/UserLogin'
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule, CommonModule, RouterModule]
})
export class LoginComponent implements OnInit {
  form: UserLogin = {
    Email: '',
    Password: '',
    RememberMe: false
  };
  isLoggedIn = false;
  isLoginFailed = false;
  errorMessage = '';
  

  authService = inject(AuthService);
  router = inject(Router);

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.isLoggedIn = true;
    }
  }

  onSubmit(): void {
    this.authService.login(this.form).subscribe({
      next: data => {
        this.isLoginFailed = false;
        this.isLoggedIn = true;
        this.router.navigate(['newsfeed']);
      },
      error: err => {
        this.errorMessage = err.error.message;
        this.isLoginFailed = true;
      }
    });
  }

  reloadPage(): void {
    window.location.reload();
  }
}