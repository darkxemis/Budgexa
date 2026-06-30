
import { Injectable, Injector, inject } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpErrorResponse,
} from '@angular/common/http';
import { catchError, Observable, throwError, switchMap, take, Subject } from 'rxjs';
import { ToastService } from '../../shared/components/toast/toast.service';
import { ToastType } from '../../shared/components/toast/toast.type';
import { TranslateService } from '@ngx-translate/core';
import { AuthService } from '../services/auth.service';
import { UserStore } from '../state/user.store';
import { Router } from '@angular/router';

import { performLogout } from '../utils/auth.utils';
import { ApiErrorResponse } from '../models/api-error.model';

@Injectable({ providedIn: 'root' })
export class ApiErrorInterceptor implements HttpInterceptor {
  private injector = inject(Injector);
  private readonly toast = inject(ToastService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly userStore = inject(UserStore);

  // Indicates if a refresh token request is in progress
  private isRefreshing = false;
  // Subject to notify pending requests when refresh completes
  private refreshTokenSubject = new Subject<boolean>();

  // Lazy getter for TranslateService to avoid DI issues
  private get translate(): TranslateService {
    return this.injector.get(TranslateService);
  }

  /**
   * Main HTTP error interception logic:
   * - Handles 401 errors (token expired, refresh logic, or redirect to login)
   * - Handles API errors with a tag (shows translated toast)
   * - Default: propagates error
   */
  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        // --- 401 Unauthorized: handle refresh or force logout ---
        if (error.status === 401) {
          if (req.url.includes('/refresh')) {
            performLogout(this.authService, this.userStore, this.router);
            return throwError(() => error);
          }
          const wwwAuth = error.headers?.get('WWW-Authenticate') || '';
          if (wwwAuth.includes('error="invalid_token"') && wwwAuth.includes('error_description="Token expired"')) {
            // If token expired, try to refresh
            return this.handle401Error(req, next).pipe(
              catchError((refreshError) => {
                performLogout(this.authService, this.userStore, this.router);
                return throwError(() => refreshError);
              })
            );
          }
        }

        // --- Default error handler ---
        // If the error has a tag, show a translated toast message
        if (error.error) {
          const apiError = error.error as ApiErrorResponse;
          if (apiError?.tag) {
            this.toast.show(apiError.tag, ToastType.Error);
          }
        }
        // Always propagate the error to the subscriber
        return throwError(() => error);
      })
    );
  }

  private handle401Error(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (!this.isRefreshing) {
      this.isRefreshing = true;
      this.refreshTokenSubject = new Subject<boolean>();
      return this.authService.refreshToken().pipe(
        switchMap(() => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(true);
          this.refreshTokenSubject.complete();
          return next.handle(req);
        }),
        catchError((refreshError) => {
          this.isRefreshing = false;
          this.refreshTokenSubject.next(false);
          this.refreshTokenSubject.complete();
          performLogout(this.authService, this.userStore, this.router);
          return throwError(() => refreshError);
        })
      );
    } else {
      // Wait for the refresh to complete, then retry or fail
      return this.refreshTokenSubject.pipe(
        take(1),
        switchMap(success => {
          if (success) {
            return next.handle(req);
          } else {
            performLogout(this.authService, this.userStore, this.router);
            return throwError(() => new Error('Refresh failed'));
          }
        })
      );
    }
  }
}