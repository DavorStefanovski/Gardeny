import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, tap, throwError } from 'rxjs';
import { RegisterUser } from './Models/RegisterUser';
import { catchError } from 'rxjs/operators';
import { CreatePost } from './Models/CreatePost';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  public readonly JWT_TOKEN = 'JWT_TOKEN';
  public loggedUser?: string;
  public isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  private http = inject(HttpClient);
  constructor() { }

  login(user: { 
    Email: string, Password: string, RememberMe: Boolean 
  }): Observable<any> {
    return this.http.post('http://localhost:5083/api/Users/login', user).pipe(
      tap((response: any) => this.doLoginUser(user.Email, response.token))
    );
  }

  private doLoginUser(Email: string, token: any) {
    this.loggedUser = Email;
    this.storeJwtToken(token);
    this.isAuthenticatedSubject.next(true);
  }

  private storeJwtToken(jwt: string) {
    localStorage.setItem(this.JWT_TOKEN, jwt);
  }

  logout() {
    localStorage.removeItem(this.JWT_TOKEN);
    this.loggedUser = "";
    this.isAuthenticatedSubject.next(false);
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.getValue();
  }

  register(user: RegisterUser): Observable<any> {
    const formData = new FormData();
    formData.append('User.FirstName', user.User.FirstName);
    formData.append('User.LastName', user.User.LastName);
    formData.append('User.Email', user.User.Email);
    formData.append('User.DateOfBirth',user.User.DateOfBirth.toString()); // Convert to ISO string
    formData.append('User.Location', user.User.Location);
    formData.append('User.Price', user.User.Price.toString());
    formData.append('Password', user.Password);
    if (user.ProfilePicture) {
      formData.append('ProfilePicture', user.ProfilePicture);
    }

    console.log('Request body:', formData); // Log the request body
    return this.http.post('http://localhost:5083/api/Users/register', formData, { responseType: 'text' });
  }

  getfeed(): Observable<any>{
    return this.http.get('http://localhost:5083/api/Posts/Feed');
  }

  getuser(): Observable<any>{
    return this.http.get('http://localhost:5083/api/Users/current');
  }

  post(newPost:CreatePost): Observable<any>{
    const formData = new FormData();
    formData.append('Text', newPost.Text);
    for (let file of newPost.Files) {
      formData.append('Files', file);
    }
    console.log(newPost);
    return this.http.post('http://localhost:5083/api/Posts/', formData, {responseType: 'text'});
  }

  comment(newComment: any): Observable<any>{
    return this.http.post('http://localhost:5083/api/Posts/Comment', newComment, {responseType: 'text'});
  }

  rating(newRating: any): Observable<any>{
    return this.http.post('http://localhost:5083/api/Posts/Rating', newRating, {responseType: 'text'});
  }

  getusers(value: string): Observable<any>{
    return this.http.get('http://localhost:5083/api/Users?searchString='+value);
  }

  getposts(value: string): Observable<any>{
    return this.http.get('http://localhost:5083/api/Users?searchString='+value);
  }
  getuserbyid(value: number): Observable<any>{
    return this.http.get('http://localhost:5083/api/Users/'+value);
  }
  getpostsbyuser(value: number): Observable<any>{
    return this.http.get('http://localhost:5083/api/Posts/UserPosts/'+value);
  }

  follow(value: number): Observable<any>{
    return this.http.get('http://localhost:5083/api/Follows/follow/'+value, { responseType: 'text' as 'json' });
  }


}
