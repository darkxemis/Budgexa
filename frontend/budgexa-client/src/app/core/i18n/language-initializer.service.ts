import { effect, Injectable } from "@angular/core";
import { UserStore } from "../state/user.store";
import { LanguageService } from "./language.service";

@Injectable({ providedIn: 'root' })
export class LanguageSyncService {
  constructor(
    private userStore: UserStore,
    private languageService: LanguageService
  ) {
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