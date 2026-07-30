import { Component, inject, signal, output, input, AfterViewInit, ChangeDetectionStrategy, ElementRef, viewChild } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import SignaturePad from '../../utils/signature_pad.js';
import { SpinnerComponent } from '../spinner/spinner.component';
import { ToastService } from '../toast/toast.service';
import { ToastType } from '../toast/toast.type';

@Component({
  selector: 'app-signature-pad',
  standalone: true,
  imports: [TranslateModule, SpinnerComponent],
  templateUrl: './signature-pad.component.html',
  styleUrl: './signature-pad.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignaturePadComponent implements AfterViewInit {
  private readonly toastService = inject(ToastService);

  readonly existingImageUrl = input<string | null>(null);
  readonly saving = input(false);

  readonly saved = output<File>();
  readonly deleted = output<void>();
  readonly cleared = output<void>();

  readonly signatureCanvas = viewChild<ElementRef<HTMLCanvasElement>>('signatureCanvas');
  readonly showPad = signal(true);

  private signaturePad: SignaturePad | null = null;

  ngAfterViewInit() {
    const canvas = this.signatureCanvas()?.nativeElement;
    if (canvas) {
      this.resizeCanvas(canvas);
      this.signaturePad = new SignaturePad(canvas);
    }
  }

  clear() {
    this.signaturePad?.clear();
    this.cleared.emit();
  }

  save() {
    if (!this.signaturePad || this.signaturePad.isEmpty()) {
      this.toastService.show('signatureEmpty', ToastType.Error);
      return;
    }

    const dataUrl = this.signaturePad.toDataURL('image/png');
    const blob = this.dataURLToBlob(dataUrl);
    const file = new File([blob], 'signature.png', { type: 'image/png' });
    this.saved.emit(file);
  }

  delete() {
    this.deleted.emit();
  }

  reset() {
    this.signaturePad?.clear();
    this.showPad.set(true);
  }

  private resizeCanvas(canvas: HTMLCanvasElement) {
    const rect = canvas.getBoundingClientRect();
    canvas.width = rect.width * devicePixelRatio;
    canvas.height = rect.height * devicePixelRatio;
    const ctx = canvas.getContext('2d');
    if (ctx) {
      ctx.scale(devicePixelRatio, devicePixelRatio);
    }
  }

  private dataURLToBlob(dataUrl: string): Blob {
    const parts = dataUrl.split(',');
    const mime = parts[0].match(/:(.*?);/)![1];
    const bytes = atob(parts[1]);
    const buffer = new ArrayBuffer(bytes.length);
    const view = new Uint8Array(buffer);
    for (let i = 0; i < bytes.length; i++) {
      view[i] = bytes.charCodeAt(i);
    }
    return new Blob([buffer], { type: mime });
  }
}
