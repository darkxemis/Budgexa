# Frontend AI Rules - Budgexa

## Tech Stack
- **Angular 21** - Standalone components
- **TypeScript 5.9** 
- **Signals** - Reactive state management
- **ngx-translate** - i18n (en, es, de, hr)

## Core Principles
1. **Reuse existing components** - Check `shared/components/` first
2. **Signals for state** - `signal()` over simple variables
3. **inject()** - Use with `readonly` keyword
4. **Standalone** - All components standalone with explicit imports
5. **TranslateModule** - Import when using `| translate` pipe

## Component Pattern
```typescript
@Component({
  selector: 'app-example',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './example.component.html',
  styleUrl: './example.component.scss' // singular
})
export class ExampleComponent {
  private readonly service = inject(Service);
  protected readonly data = signal<Data[]>([]);
}
```

## Architecture
```
core/           # Singleton services, guards, interceptors
├── api/        # HTTP services (AuthApiService, UserApiService)
├── services/   # Business logic (AuthService, UserService)
├── state/      # Global stores (UserStore)
└── guards/     # Route guards (authGuard)

shared/         # Reusable components
└── components/ # SpinnerComponent, ToastComponent, etc.

features/       # Feature modules
└── feature/
    ├── pages/      # Route components
    └── components/ # Feature-specific (if needed)
```

## Key Components
- **UserStore** - Global user state with signals
- **ToastService** - Notifications
- **performLogout()** - Logout utility from `core/utils/auth.utils`

## Responsive Layout
```scss
.grid {
  display: grid;
  grid-template-columns: 1fr;  // Mobile
  
  @media (min-width: 768px) {
    grid-template-columns: repeat(2, 1fr);  // Tablet
  }
  
  @media (min-width: 1024px) {
    grid-template-columns: repeat(3, 1fr);  // Desktop
  }
}
```

## Translations
Always add to all 4 languages: `en.json`, `es.json`, `de.json`, `hr.json`
