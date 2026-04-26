import { importProvidersFrom } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TranslateModule, TranslateService, TranslateLoader } from '@ngx-translate/core';
import { NgxTranslateHttpLoader } from './ngx-translate-loader';

export function provideNgxTranslate() {
  return importProvidersFrom(
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: (http: HttpClient) => new NgxTranslateHttpLoader(http),
        deps: [HttpClient],
      },
      fallbackLang: 'en'
    })
  );
}
