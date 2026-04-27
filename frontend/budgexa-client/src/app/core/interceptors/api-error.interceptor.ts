import { Injectable, Injector, inject } from '@angular/core';
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
import { TranslateService } from '@ngx-translate/core';

@Injectable({ providedIn: 'root' })
export class ApiErrorInterceptor implements HttpInterceptor {
  private injector = inject(Injector);
  private readonly toast = inject(ToastService);

  private get translate(): TranslateService {
    return this.injector.get(TranslateService);
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.error) {
          const apiError = error.error as ApiErrorResponse;

          if (apiError?.tag) {
            const translatedMessage = this.translate.instant(apiError.tag);
            this.toast.show(translatedMessage, ToastType.Error);
          }
        }

        return throwError(() => error);
      })
    );
  }
}

export interface ApiErrorResponse {
  tag: string;
  message: string;
  traceId: string;
  metadata?: Record<string, string>;
  detail?: string;
}
