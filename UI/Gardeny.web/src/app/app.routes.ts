import { Routes } from "@angular/router";
import { LoginComponent } from "./login/login.component"
import { RegisterComponent } from "./register/register.component";
import { NewsfeedComponent } from "./newsfeed/newsfeed.component";
import { ProfileComponent } from "./profile/profile.component";
const routeConfig: Routes = [
    {
        path: '',
        component: LoginComponent,
        title: 'Log in Page'
    }
    ,
    {
        path: 'register',
        component: RegisterComponent,
        title: 'Register Page'
    }
    ,
    {
        path: 'newsfeed',
        component: NewsfeedComponent,
        title: 'Newsfeed'
    }
    ,
    {
        path: 'profile/:id',
        component: ProfileComponent,
        title: 'Profile'
    }
];

export default routeConfig;