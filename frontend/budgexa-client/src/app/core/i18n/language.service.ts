import { Injectable } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";

@Injectable({ providedIn: 'root' })
export class LanguageService {
  constructor(private translate: TranslateService) {}

  set(lang: string) {
    this.translate.setFallbackLang(lang);
  }

  get current() {
    return this.translate.getFallbackLang();
  }
}