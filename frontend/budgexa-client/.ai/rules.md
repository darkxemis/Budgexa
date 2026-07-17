# Frontend AI Rules - Budgexa

## Tech Stack
- **Angular 22** - Standalone components
- **TypeScript 5.9** 
- **Signals** - Reactive state management (primary approach)
- **ngx-translate** - i18n (en, es, de, hr)

## Core Principles
1. **Reuse existing components** - Check `shared/components/` first
2. **Signals for state** - `signal()` over simple variables
3. **inject()** - Use with `readonly` keyword
4. **Standalone** - All components standalone with explicit imports
5. **TranslateModule** - Import when using `| translate` pipe
6. **No `any`** - Never use `any`. Define interfaces, use `unknown`, or use `Record<string, unknown>` for untyped objects. For browser APIs not in TS stdlib, create a local interface with the minimal shape needed.

## Signals Rules (MANDATORY)
- **Use `signal()`** for all mutable component state
- **Use `computed()`** for derived/calculated values
- **Use `toSignal()`** from `@angular/core/rxjs-interop` to convert Observables (including `form.valueChanges`) to signals
- **NEVER subscribe** to form valueChanges just to update state — use `toSignal()` + `computed()` instead
- **Use `input()`** for component inputs (signal-based inputs)
- **Use `output()`** for component outputs
- **Avoid `OnInit`** for signal setup — declare signals as class fields with `toSignal()`/`computed()`
- **No `DestroyRef`/`takeUntilDestroyed`** for computed values — `toSignal()` handles cleanup automatically

### Signal Patterns
```typescript
// Reactive form → signal → computed
readonly form = this.fb.group({ ... });
private readonly formValues = toSignal(this.form.valueChanges, {
  initialValue: this.form.getRawValue(),
});
protected readonly derivedValue = computed(() => {
  const values = this.formValues();
  return /* calculation */;
});

// HTTP result → signal (one-shot)
protected readonly loading = signal(false);
// Use signal.set() in subscribe callbacks for HTTP calls only
```

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
