import { effect, Injectable, inject } from "@angular/core";
import { UserStore } from "../state/user.store";
import { LanguageService } from "./language.service";

@Injectable({ providedIn: 'root' })
export class LanguageSyncService {
  private readonly userStore = inject(UserStore);
  private readonly languageService = inject(LanguageService);

  constructor() {
    effect(() => {
      const user = this.userStore.user();

      const lang = user?.language;
      if (!lang) return;

      if (this.languageService.current !== lang) {
        this.languageService.set(lang);
      }
    });
  }
}