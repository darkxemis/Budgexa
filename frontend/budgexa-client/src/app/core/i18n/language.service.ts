import { Injectable } from "@angular/core";
import { TranslateService } from "@ngx-translate/core";

@Injectable({ providedIn: 'root' })
export class LanguageService {
  constructor(private translate: TranslateService) {}

  set(lang: string) {
    console.log('Setting language to', lang);
    this.translate.setFallbackLang(lang);
  }

  get current() {
    return this.translate.getFallbackLang();
  }
}