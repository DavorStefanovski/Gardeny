import { HttpInterceptorFn } from '@angular/common/http';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
  const jwtToken = getJwtToken();
  const authReq = req.clone({
    headers: req.headers.set('Authorization',`Bearer ${jwtToken}`)
  });
  console.log(authReq);
  return next(authReq);
};

function getJwtToken(): string | null {
  return localStorage.getItem('JWT_TOKEN');
}