import { Injectable, inject, signal } from '@angular/core';
import { LanguageApiService } from '../api/language-api.service';
import { Language } from '../models/user.model';
import { Observable, tap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LanguageDataService {
  private readonly api = inject(LanguageApiService);
  private readonly _languages = signal<Language[]>([]);
  readonly languages = this._languages.asReadonly();

  loadLanguages(): Observable<Language[]> {
    return this.api.getLanguages().pipe(
      tap((languages) => this._languages.set(languages))
    );
  }

  getLanguageByCode(code: string): Language | undefined {
    return this._languages().find((lang) => lang.code === code);
  }
}
