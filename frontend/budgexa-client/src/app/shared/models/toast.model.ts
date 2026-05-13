import { ToastType } from '../components/toast/toast.type';

export interface ToastMessage {
  message: string;
  type: ToastType;
  params?: Record<string, unknown>;
}
