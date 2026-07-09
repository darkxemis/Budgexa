import { Component, signal, inject, ChangeDetectionStrategy } from '@angular/core';
import { UpperCasePipe } from '@angular/common';
import { LanguageService } from '../../../core/i18n/language.service';
import { TranslateModule } from '@ngx-translate/core';

interface LanguageOption {
  code: string;
  label: string;
  flag: string;
}

@Component({
  selector: 'app-language-selector',
  standalone: true,
  imports: [TranslateModule, UpperCasePipe],
  templateUrl: './language-selector.component.html',
  styleUrl: './language-selector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LanguageSelectorComponent {
  private readonly languageService = inject(LanguageService);

  protected readonly languages: LanguageOption[] = [
    { code: 'en', label: 'English', flag: '🇬🇧' },
    { code: 'es', label: 'Español', flag: '🇪🇸' },
    { code: 'de', label: 'Deutsch', flag: '🇩🇪' },
    { code: 'hr', label: 'Hrvatski', flag: '🇭🇷' },
  ];

  protected readonly isOpen = signal(false);
  protected readonly currentLang = signal(this.getInitialLang());

  protected get currentLanguage(): LanguageOption {
    return this.languages.find(l => l.code === this.currentLang()) ?? this.languages[0];
  }

  protected toggleDropdown(): void {
    this.isOpen.update(v => !v);
  }

  protected selectLanguage(lang: LanguageOption): void {
    this.currentLang.set(lang.code);
    this.languageService.set(lang.code);
    localStorage.setItem('budgexa-public-lang', lang.code);
    this.isOpen.set(false);
  }

  private getInitialLang(): string {
    const stored = localStorage.getItem('budgexa-public-lang');
    if (stored && this.languages.some(l => l.code === stored)) {
      this.languageService.set(stored);
      return stored;
    }
    const browserLang = navigator.language?.split('-')[0] ?? 'en';
    const matched = this.languages.find(l => l.code === browserLang);
    if (matched) {
      this.languageService.set(matched.code);
      return matched.code;
    }
    this.languageService.set('en');
    return 'en';
  }
}
