import { Injectable, inject } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly translate = inject(TranslateService);

  set(lang: string) {
    this.translate.use(lang);
  }

  get current() {
    return this.translate.getCurrentLang();
  }
}