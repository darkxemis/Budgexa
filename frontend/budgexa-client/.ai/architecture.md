# Frontend Architecture - Budgexa

## Project Structure
```
src/app/
├── core/
│   ├── api/              # HTTP services
│   ├── guards/           # Route guards  
│   ├── interceptors/     # HTTP interceptors
│   ├── i18n/            # Translation services
│   ├── models/          # Global models
│   ├── services/        # Business logic
│   ├── state/           # Global stores
│   └── utils/           # Utilities
│
├── shared/
│   └── components/      # Reusable components
│
└── features/
    └── [feature]/
        ├── pages/       # Route components
        ├── components/  # Feature components
        └── services/    # Feature services

assets/
└── i18n/               # Translation JSON files
```

## Service Pattern
```
Component
  ↓
Service (business logic)
  ↓
ApiService (HTTP)
  ↓
Backend
```

## State Management
Use **signals** for reactive state:
```typescript
@Injectable({ providedIn: 'root' })
export class Store {
  private readonly _data = signal<Data[]>([]);
  readonly data = this._data.asReadonly();
  
  setData(data: Data[]) {
    this._data.set(data);
  }
}
```

## Routing
- Lazy loaded features
- Protected by `authGuard`
- Standalone component loading

## Key Patterns
- **DI**: `inject()` function
- **Forms**: Reactive with `NonNullableFormBuilder`
- **Errors**: Centralized in `ApiErrorInterceptor`
- **i18n**: `TranslateModule` with JSON files
