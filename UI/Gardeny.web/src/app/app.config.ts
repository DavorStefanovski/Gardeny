import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';

import  routeConfig  from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { AuthInterceptor } from './auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [provideRouter(routeConfig), provideHttpClient(withInterceptors([AuthInterceptor])) ]
};
