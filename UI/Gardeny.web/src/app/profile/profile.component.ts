import { Component, inject } from '@angular/core';
import { AuthService } from '../auth.service';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {
  auth = inject(AuthService);
  router = inject(ActivatedRoute);
  posts: any;
  server: string = 'http://localhost:5083';
  user: any;
  userId: number = -1;
  showDetails: boolean = false;
  isCurrent: boolean = false;
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
    this.router.paramMap.subscribe(params => {
      this.userId = Number(params.get('id'));
      this.auth.getuserbyid(this.userId).subscribe((result) => {
        this.user = result;
        
      });
      this.auth.getpostsbyuser(this.userId).subscribe((result) => {
        this.posts = result;
        
      });
      this.auth.getuser().subscribe((result) => {
        if(result.id == this.user.user.id)
          this.isCurrent = true;
        console.log("ova e na najaveniot "+result.id);
        console.log("ova e na baraniot "+this.user.user.id);
      });
    });
    
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

  follow(id: number) {
    this.auth.follow(id).subscribe({
      complete: () => {
        console.log('Follow successful:');  // Check if this logs
        setTimeout(() => {
          window.location.reload();  // Delay the reload
        }, 500);  // 500ms delay to ensure the update
      }
    });
  }
  

}
