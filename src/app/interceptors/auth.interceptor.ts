import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private authService: AuthService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        console.log('>>> [INTERCEPTOR] Outgoing request:', request.url);

        const currentUser = this.authService.currentUserValue;
        const isLoggedIn = currentUser && currentUser.token;
        const isApiUrl = request.url.includes('/api/');
        
        if (isLoggedIn && isApiUrl) {
            console.log('>>> [INTERCEPTOR] Adding Bearer token to request');
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${currentUser.token}`
                }
            });
        }

        return next.handle(request).pipe(
            tap({
                next: (event) => {
                    if (event.type !== 0) { 
                        console.log('>>> [INTERCEPTOR] Response Event:', event);
                    }
                },
                error: (err) => {
                    console.error('>>> [INTERCEPTOR] Response Error:', err);
                }
            })
        );

    }

}

