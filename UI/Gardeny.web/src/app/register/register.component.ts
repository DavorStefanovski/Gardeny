import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from '../auth.service';
import { RegisterUser } from '../Models/RegisterUser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  standalone: true,
  imports: [FormsModule, ReactiveFormsModule, CommonModule]
})
export class RegisterComponent {
  form: RegisterUser = {
    User: {
      FirstName: '',
      LastName: '',
      Email: '',
      DateOfBirth: new Date('1990-01-01'), // Initialize with today's date
      Location: '',
      Price: 0,
    },
    Password: '',
    ProfilePicture: null
  };
  isSuccessful = false;
  isSignUpFailed = false;
  errorMessage = '';

  authService = inject(AuthService);
  router = inject(Router);

  onSubmit(): void {
    this.authService.register(this.form).subscribe({
      next: data => {
        console.log(data);
        this.isSuccessful = true;
        this.isSignUpFailed = false;
        this.router.navigate(['/']);
      },
      error: err => {
        console.log('Error:', err); // Log the error response
        this.errorMessage = err.error.message;
        this.isSignUpFailed = true;
      }
    });

  }

  onFileChange(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.form.ProfilePicture = file;
    }
  }
}
