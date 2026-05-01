import { Injectable, signal } from '@angular/core';
import { ToastType } from './toast.type';
import { ToastMessage } from '../../models/toast.model';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _toast = signal<ToastMessage | null>(null);
  readonly toast = this._toast.asReadonly();
  private timeoutId?: ReturnType<typeof setTimeout>;

  show(message: string, type: ToastType = ToastType.Info) {
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }
    this._toast.set({ message, type });
    this.timeoutId = setTimeout(() => {
      this._toast.set(null);
      this.timeoutId = undefined;
    }, 4000);
  }
}
