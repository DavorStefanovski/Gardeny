import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { AuthService } from '../auth.service';
import { FormsModule } from '@angular/forms';
import { CreatePost } from '../Models/CreatePost';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-newsfeed',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './newsfeed.component.html',
  styleUrls: ['./newsfeed.component.css']
})
export class NewsfeedComponent {
  http = inject(HttpClient);
  auth = inject(AuthService);
  router = inject(Router);
  posts: any;
  server: string = 'http://localhost:5083';
  user: { email: string, firstName: string, lastName: string, pictureUrl: string, id: number } = { 
    email: '', 
    pictureUrl: '',
    firstName: '',
    lastName: '',
    id: -1
  };
  Users: { firstName: string, lastName: string, pictureUrl: string, id: number }[] = [];
  newpost: CreatePost = {
    Text: '',
    Files: [] as File[]
  };
  newRating: any = {
    Value: -1,
    PostId: -1
  };
  newComment: any = {
    Text: '',
    PostId: -1
  };
  errorMessage: string = '';

  ngOnInit() { 
    this.auth.getfeed().subscribe((result) => {
      this.posts = result;
    });
    this.auth.getuser().subscribe((result) => {
      this.user = result;
      console.log(this.user.email);
    });

  } 

  createpost() {
    this.auth.post(this.newpost).subscribe({
      next: data => {
        console.log(data);
        window.location.reload()
      },
      error: err => {
        console.log('Error:', err); // Log the error response
        this.errorMessage = err.error.message;
      }
    });
  }

  onFileChange(event: any) {
    if (event.target.files.length > 0) {
      this.newpost.Files = event.target.files;
    }
  }

  addComment(postId: number, commentText: string) {
    this.newComment.PostId = postId;
    this.newComment.Text = commentText;
    this.auth.comment(this.newComment).subscribe({
      next: data => {
        console.log(data);
        window.location.reload()
      },
      error: err => {
        console.log('Error:', err); // Log the error response
        this.errorMessage = err.error.message;
      }
    });
  }

  ratePost(postId: number,rating: string) {
    console.log("Inside ratePost");
    this.newRating.PostId = postId;
    this.newRating.Value = rating;
    this.auth.rating(this.newRating).subscribe({
      next: data => {
        console.log(data);
        window.location.reload()
      },
      error: err => {
        console.log('Error:', err); // Log the error response
        this.errorMessage = err.error.message;
      }
    });
  }

  logout(){
    this.auth.logout();
    this.router.navigate(['/']);
  }

  searchUsers(value: string) {
    this.auth.getusers(value).subscribe({
      next: data => {
        this.Users = data;
        console.log(this.Users);
      }
    });
  }
}
