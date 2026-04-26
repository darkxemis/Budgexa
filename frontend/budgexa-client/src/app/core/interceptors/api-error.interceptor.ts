import { Injectable, inject } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpErrorResponse,
} from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { ToastService } from '../../shared/components/toast/toast.service';
import { ToastType } from '../../shared/components/toast/toast.type';

@Injectable({ providedIn: 'root' })
export class ApiErrorInterceptor implements HttpInterceptor {
  private readonly toastService = inject(ToastService);

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: any) => {
        if (error instanceof HttpErrorResponse && error.error && error.error.message) {
          this.toastService.show(error.error.message, ToastType.Error);
        }
        return throwError(() => error);
      })
    );
  }
}
