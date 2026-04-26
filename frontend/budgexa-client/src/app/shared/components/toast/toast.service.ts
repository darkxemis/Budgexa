import { Injectable, signal } from '@angular/core';
import { ToastType } from './toast.type';

export interface ToastMessage {
  message: string;
  type: ToastType;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _toast = signal<ToastMessage | null>(null);
  readonly toast = this._toast.asReadonly();

  show(message: string, type: ToastType = ToastType.Info) {
    this._toast.set({ message, type });
    setTimeout(() => {
      this._toast.set(null);
    }, 4000);
  }
}
