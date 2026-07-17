import {
  Component,
  input,
  output,
  signal,
  computed,
  inject,
  ChangeDetectionStrategy,
  OnDestroy,
} from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../../core/i18n/language.service';

/**
 * BCP-47 locale tags the Web Speech API expects, keyed by
 * the app short language codes.
 * Kept here because it is only relevant to speech recognition.
 */
const SPEECH_LOCALE: Readonly<Record<string, string>> = {
  en: 'en-US',
  es: 'es-ES',
  de: 'de-DE',
  hr: 'hr-HR',
} as const;

/** Minimal shape of a speech recognition result event. */
interface SpeechResultEvent {
  readonly results: {
    readonly length: number;
    [index: number]: {
      readonly isFinal: boolean;
      readonly 0: { readonly transcript: string };
    };
  };
}

/** Minimal shape needed from the browser SpeechRecognition API. */
interface SpeechRecognitionHandle {
  lang: string;
  continuous: boolean;
  interimResults: boolean;
  onresult: ((event: SpeechResultEvent) => void) | null;
  onerror: (() => void) | null;
  onend: (() => void) | null;
  start(): void;
  stop(): void;
}

/** Returns the SpeechRecognition constructor if available, or null. */
function getSpeechRecognitionCtor(): (new () => SpeechRecognitionHandle) | null {
  const w = window as unknown as Record<string, unknown>;
  return (w['SpeechRecognition'] ?? w['webkitSpeechRecognition']) as
    | (new () => SpeechRecognitionHandle)
    | null;
}

@Component({
  selector: 'app-voice-input',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './voice-input.component.html',
  styleUrl: './voice-input.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VoiceInputComponent implements OnDestroy {
  private readonly languageService = inject(LanguageService);

  /** Current text already in the target field - new speech appends to it. */
  currentText = input<string>('');

  /** Max length for the combined text. */
  maxLength = input<number>(2000);

  /** Whether the host control is disabled (e.g. while loading). */
  disabled = input<boolean>(false);

  /** Emits the updated text (existing + transcribed). */
  textChange = output<string>();

  protected readonly isListening = signal(false);
  protected readonly speechSupported = computed(() => getSpeechRecognitionCtor() !== null);

  private recognition: SpeechRecognitionHandle | null = null;

  ngOnDestroy(): void {
    this.stop();
  }

  protected toggle(): void {
    if (this.isListening()) {
      this.stop();
    } else {
      this.start();
    }
  }

  stop(): void {
    this.recognition?.stop();
    this.recognition = null;
    this.isListening.set(false);
  }

  private readonly isMobile = /Android|iPhone|iPad/i.test(navigator.userAgent);
  private finalTranscript = '';

  private start(): void {
    const Ctor = getSpeechRecognitionCtor();
    if (!Ctor) return;

    const recognition = new Ctor();
    recognition.lang = SPEECH_LOCALE[this.languageService.current] ?? 'en-US';
    recognition.continuous = !this.isMobile;
    recognition.interimResults = !this.isMobile;

    this.finalTranscript = '';
    const baseText = this.currentText();

    recognition.onresult = (event: SpeechResultEvent) => {
      let interim = '';
      let final = '';
      for (let i = 0; i < event.results.length; i++) {
        const transcript = event.results[i][0].transcript;
        if (event.results[i].isFinal) {
          final += transcript;
        } else {
          interim += transcript;
        }
      }

      if (this.isMobile) {
        this.finalTranscript += final;
        const separator = baseText.length > 0 || this.finalTranscript.length > 0 ? ' ' : '';
        const combined = (baseText + separator + this.finalTranscript).trim().slice(0, this.maxLength());
        this.textChange.emit(combined);
      } else {
        const separator = baseText.length > 0 ? ' ' : '';
        const combined = (baseText + separator + final + interim).slice(0, this.maxLength());
        this.textChange.emit(combined);
      }
    };

    recognition.onerror = () => this.isListening.set(false);
    recognition.onend = () => {
      if (this.isMobile && this.isListening()) {
        recognition.start();
      } else {
        this.isListening.set(false);
      }
    };

    recognition.start();
    this.recognition = recognition;
    this.isListening.set(true);
  }
}
