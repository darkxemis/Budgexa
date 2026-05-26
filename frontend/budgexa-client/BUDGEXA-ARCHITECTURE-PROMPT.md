# 🎯 BUDGEXA - Complete Frontend Architecture (Angular 21)

## 📋 PROJECT OVERVIEW

**Name**: Budgexa Client  
**Type**: Frontend Web Application  
**Framework**: Angular 21.2.0  
**Language**: TypeScript 5.9.2  
**Package Manager**: npm 11.13.0  
**Testing**: Vitest 4.0.8  
**Internationalization**: ngx-translate 17.0.0  

### Description
Budgexa is a financial management application built with Angular 21, following Clean Architecture and Clean Code principles. It uses standalone components, signals for reactive state management, and a highly modular and scalable feature-based architecture.

---

## 🏗️ COMPLETE FOLDER STRUCTURE

```
budgexa-client/
│
├── src/
│   ├── app/
│   │   ├── app.ts                        # Root application component
│   │   ├── app.html                      # Main template
│   │   ├── app.scss                      # Global component styles
│   │   ├── app.config.ts                 # Global providers configuration
│   │   ├── app.routes.ts                 # Main route definitions
│   │   │
│   │   ├── core/                         # CORE: Singleton services and global configuration
│   │   │   │
│   │   │   ├── api/                      # Backend communication services
│   │   │   │   ├── auth-api.service.ts   # Authentication API (login, logout, refresh)
│   │   │   │   ├── user-api.service.ts   # User API (profile, updates)
│   │   │   │   └── language-api.service.ts # Language API
│   │   │   │
│   │   │   ├── guards/                   # Route guards
│   │   │   │   └── auth.guard.ts         # Protected route authentication
│   │   │   │
│   │   │   ├── interceptors/             # HTTP Interceptors
│   │   │   │   └── api-error.interceptor.ts # Centralized API error handling
│   │   │   │                                 # (401 -> refresh token, error translation)
│   │   │   │
│   │   │   ├── i18n/                     # Internationalization system
│   │   │   │   ├── language.service.ts           # Language switching service
│   │   │   │   ├── language-initializer.service.ts # Initial synchronization
│   │   │   │   ├── ngx-translate-loader.ts       # Custom loader
│   │   │   │   └── ngx-translate-provider.ts     # DI provider
│   │   │   │
│   │   │   ├── models/                   # Global models and interfaces
│   │   │   │   ├── auth.model.ts         # AuthCredentials, LoginResponse
│   │   │   │   ├── user.model.ts         # UserProfileResult, UserUpdateDto
│   │   │   │   └── api-error.model.ts    # ApiErrorResponse
│   │   │   │
│   │   │   ├── services/                 # Global domain services
│   │   │   │   ├── auth.service.ts       # Authentication business logic
│   │   │   │   ├── user.service.ts       # User business logic
│   │   │   │   └── language-data.service.ts # Available language data
│   │   │   │
│   │   │   ├── state/                    # Global state management
│   │   │   │   └── user.store.ts         # User store with signals
│   │   │   │
│   │   │   └── utils/                    # Utilities and helpers
│   │   │       └── auth.utils.ts         # performLogout(), token helpers
│   │   │
│   │   ├── shared/                       # SHARED: Reusable components
│   │   │   │
│   │   │   ├── components/               # Reusable UI components
│   │   │   │   │
│   │   │   │   ├── form-error/           # Form error component
│   │   │   │   │   ├── form-error.component.ts
│   │   │   │   │   ├── form-error.component.html
│   │   │   │   │   └── form-error.component.scss
│   │   │   │   │
│   │   │   │   ├── spinner/              # Loading indicator
│   │   │   │   │   ├── spinner.component.ts
│   │   │   │   │   ├── spinner.component.html
│   │   │   │   │   └── spinner.component.scss
│   │   │   │   │
│   │   │   │   ├── toast/                # Notification system
│   │   │   │   │   ├── toast.component.ts
│   │   │   │   │   ├── toast.component.html
│   │   │   │   │   ├── toast.component.scss
│   │   │   │   │   ├── toast.service.ts   # Service to show toasts
│   │   │   │   │   └── toast.type.ts      # Type enum (success, error, etc)
│   │   │   │   │
│   │   │   │   ├── user-menu/            # User menu in header
│   │   │   │   │   ├── user-menu.component.ts
│   │   │   │   │   ├── user-menu.component.html
│   │   │   │   │   └── user-menu.component.scss
│   │   │   │   │
│   │   │   │   └── user-settings-modal/  # User settings modal
│   │   │   │       ├── user-settings-modal.component.ts
│   │   │   │       ├── user-settings-modal.component.html
│   │   │   │       └── user-settings-modal.component.scss
│   │   │   │
│   │   │   └── models/                   # Shared models
│   │   │       └── toast.model.ts        # ToastConfig, ToastMessage
│   │   │
│   │   └── features/                     # FEATURES: Domain modules
│   │       │
│   │       ├── login/                    # Authentication feature
│   │       │   └── pages/
│   │       │       ├── login.component.ts    # Login page
│   │       │       ├── login.component.html  # Reactive form
│   │       │       └── login.component.scss  # Login styles
│   │       │
│   │       ├── dashboard/                # Main dashboard feature
│   │       │   └── pages/
│   │       │       ├── dashboard.component.ts
│   │       │       ├── dashboard.component.html
│   │       │       └── dashboard.component.scss
│   │       │
│   │       └── invoices/                 # Invoices feature (in development)
│   │           └── pages/
│   │
│   ├── assets/                           # Static resources
│   │   └── i18n/                         # Translation files
│   │       ├── en.json                   # English
│   │       ├── es.json                   # Spanish
│   │       ├── de.json                   # German
│   │       └── hr.json                   # Croatian
│   │
│   ├── environments/                     # Environment configuration
│   │   ├── environment.ts                # Development
│   │   └── environment.prod.ts           # Production
│   │
│   ├── index.html                        # Main HTML
│   ├── main.ts                           # Application entry point
│   └── styles.scss                       # Global styles
│
├── public/                               # Public static files
│
├── angular.json                          # Angular CLI configuration
├── package.json                          # Dependencies and scripts
├── tsconfig.json                         # Base TypeScript configuration
├── tsconfig.app.json                     # TypeScript config for app
├── tsconfig.spec.json                    # TypeScript config for tests
├── ARQUITECTURA.md                       # Architecture documentation (Spanish)
├── README.md                             # Project documentation
├── Dockerfile.dev                        # Docker for development
├── Dockerfile.prod                       # Docker for production
└── nginx.conf                            # Nginx configuration
```

---

## 🎨 ARCHITECTURAL PRINCIPLES

### 1. **Clean Architecture & Clean Code**
- **Single Responsibility Principle (SRP)**: Each module/class has a single responsibility
- **Separation of Concerns**: Clear separation between layers (presentation, logic, data)
- **Dependency Injection**: Extensive use of Angular's DI system
- **Immutability**: Use of readonly signals when appropriate

### 2. **Feature-Based Architecture**
- Each feature is independent and self-contained
- Internal structure per feature: `pages/`, `components/`, `services/`, `state/` (as needed)
- Facilitates lazy loading and scalability

### 3. **Standalone Components**
- The entire project uses standalone components (no NgModules)
- Explicit imports in each component
- Better clarity and optimized tree-shaking

### 4. **Reactive State Management with Signals**
- **Signals** for local and global state (Angular 21+)
- Centralized stores in `core/state/` for global state
- Pattern: private `_signal` + public `signal.asReadonly()`
- Example: `UserStore` manages authenticated user

### 5. **Responsive & Accessible Design**
- **Flexbox** as first choice for layouts
- Mobile-first design: mobile → tablet → desktop
- HTML5 semantics
- Accessibility (ARIA, roles, labels)

---

## 🔧 IMPLEMENTED PATTERNS AND PRACTICES

### **Layered Services Pattern**

```
Component (UI)
    ↓ injects
Service (Business Logic)
    ↓ injects
ApiService (HTTP)
    ↓
Backend API
```

**Example: Authentication**
1. `LoginComponent` → calls `AuthService.login()`
2. `AuthService` → calls `AuthApiService.login()`
3. `AuthApiService` → makes POST to `/api/auth/login`

### **Global State Pattern with Signals**

```typescript
@Injectable({ providedIn: 'root' })
export class UserStore {
  private readonly _user = signal<UserProfileResult | null>(null);
  readonly user = this._user.asReadonly();

  setUser(user: UserProfileResult | null) {
    this._user.set(user);
  }

  clearUser() {
    this._user.set(null);
  }
}
```

### **Interceptor for Centralized Error Handling**

- `ApiErrorInterceptor` intercepts all HTTP errors
- **401 Unauthorized**: Automatically attempts token refresh
- **Errors with tag**: Translates and shows toast with backend message
- **Other errors**: Propagation or default handling

### **Functional Guards**

```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const userService = inject(UserService);
  const router = inject(Router);

  return userService.getUser().pipe(
    map(user => user?.id ? true : router.createUrlTree(['/login'])),
    catchError(() => of(router.createUrlTree(['/login'])))
  );
};
```

### **Lazy Loading of Features**

```typescript
{
  path: 'dashboard',
  loadComponent: () => import('./features/dashboard/pages/dashboard.component')
    .then((m) => m.DashboardComponent),
  canActivate: [authGuard],
}
```

### **Reactive Forms with Validation**

- Use of `NonNullableFormBuilder`
- Angular validators + custom validators
- Reusable `FormErrorComponent` to display errors

### **Internationalization (i18n)**

- **ngx-translate** for translations
- JSON files per language in `assets/i18n/`
- `LanguageService` for language switching
- `LanguageSyncService` for initial synchronization

---

## 📦 DETAILED MODULES AND RESPONSIBILITIES

### **CORE** (`src/app/core/`)

**Purpose**: Singleton services, global configuration, shared business logic.

#### **api/**
- **auth-api.service.ts**: 
  - `login()`, `logout()`, `refreshToken()`
  - Direct communication with authentication endpoints
  
- **user-api.service.ts**: 
  - `getProfile()`, `updateProfile()`
  - User profile management
  
- **language-api.service.ts**: 
  - Language-related endpoints (if applicable)

#### **guards/**
- **auth.guard.ts**: 
  - Protects routes requiring authentication
  - Redirects to `/login` if user is not authenticated

#### **interceptors/**
- **api-error.interceptor.ts**: 
  - Centralized HTTP error handling
  - Automatic token refresh on 401
  - Backend error message translation
  - Shows automatic toasts for errors

#### **i18n/**
- **language.service.ts**: Simple API for language switching
- **language-initializer.service.ts**: Syncs language on app startup
- **ngx-translate-loader.ts**: Custom loader for ngx-translate
- **ngx-translate-provider.ts**: Provider function for DI

#### **models/**
- **auth.model.ts**: `AuthCredentials`, `LoginResponse`
- **user.model.ts**: `UserProfileResult`, `UserUpdateDto`
- **api-error.model.ts**: `ApiErrorResponse` (backend error structure)

#### **services/**
- **auth.service.ts**: Authentication business logic
- **user.service.ts**: User business logic (includes store updates)
- **language-data.service.ts**: Available language data

#### **state/**
- **user.store.ts**: 
  - Global store for authenticated user
  - Uses signals: `_user` (private) and `user()` (readonly)
  - Methods: `setUser()`, `clearUser()`

#### **utils/**
- **auth.utils.ts**: 
  - `performLogout()`: clears store, tokens, redirects to login
  - Token and authentication helpers

---

### **SHARED** (`src/app/shared/`)

**Purpose**: Components, directives, pipes, and models reusable across the entire app.

#### **components/**

##### **form-error/**
- Displays form validation errors
- Receives `AbstractControl` and shows translated message based on error

##### **spinner/**
- Visual loading indicator
- Used in login, async requests, etc.

##### **toast/**
- Toast notification system
- `toast.service.ts` to display messages
- Types: success, error, warning, info
- Configurable auto-dismiss

##### **user-menu/**
- Dropdown menu in header with user options
- Logout, settings, etc.

##### **user-settings-modal/**
- Modal to edit user settings
- Language change, personal data, etc.

#### **models/**
- **toast.model.ts**: `ToastConfig`, `ToastMessage`

---

### **FEATURES** (`src/app/features/`)

**Purpose**: Business domain functionality modules.

#### **login/**
```
login/
└── pages/
    ├── login.component.ts
    ├── login.component.html
    └── login.component.scss
```

**Features**:
- Reactive form with email and password
- Validation: required email with valid format, required password
- `loading` signal for loading indicator
- Injects `AuthService`, `UserStore`, `Router`
- `ngOnInit`: redirects to dashboard if already authenticated
- `login()` method: calls AuthService and navigates to dashboard

**Does not have**: `components/`, `services/`, `state/` (uses core)

---

#### **dashboard/**
```
dashboard/
└── pages/
    ├── dashboard.component.ts
    ├── dashboard.component.html
    └── dashboard.component.scss
```

**Features**:
- Main post-login page
- Protected by `authGuard`
- Shows `user-menu` component
- Main container of authenticated application

---

#### **invoices/** (in development)
```
invoices/
└── pages/
```

**Planned for**: Invoice management

---

## 🔄 MAIN FLOWS

### **Authentication Flow**

```
1. User accesses the app
   ↓
2. Router redirects to /login (default route)
   ↓
3. LoginComponent loads
   ↓
4. User enters credentials and submits
   ↓
5. LoginComponent.login() → AuthService.login()
   ↓
6. AuthService → AuthApiService.login(credentials)
   ↓
7. Backend responds with tokens and user
   ↓
8. UserService.getUser() updates UserStore
   ↓
9. Router navigates to /dashboard
   ↓
10. authGuard validates that UserStore.user() exists
   ↓
11. DashboardComponent loads
```

### **Automatic Refresh Token Flow**

```
1. User makes request to protected API
   ↓
2. Backend responds 401 (token expired)
   ↓
3. ApiErrorInterceptor detects 401
   ↓
4. Calls AuthService.refreshToken()
   ↓
5. Backend responds with new access token
   ↓
6. Interceptor retries original request with new token
   ↓
7. Original request completes successfully
```

If refresh fails:
```
1. Interceptor detects error on /refresh
   ↓
2. Calls performLogout()
   ↓
3. Clears UserStore
   ↓
4. Redirects to /login
```

### **Error Handling with Toast Flow**

```
1. HTTP request fails with backend error
   ↓
2. ApiErrorInterceptor detects error with `tag`
   ↓
3. Translates message using TranslateService
   ↓
4. ToastService.show() displays error toast
   ↓
5. User sees visual notification
   ↓
6. Toast automatically disappears after 5s
```

---

## 🎯 IMPLEMENTATION GUIDES

### **Adding a New Feature**

1. Create folder in `features/feature-name/`
2. Minimum structure: `pages/`
3. Add route in `app.routes.ts` with lazy loading
4. If needs protection: add `canActivate: [authGuard]`

**Example**:
```typescript
{
  path: 'invoices',
  loadComponent: () => import('./features/invoices/pages/invoices.component')
    .then(m => m.InvoicesComponent),
  canActivate: [authGuard]
}
```

### **Adding a Reusable Component**

1. Create in `shared/components/component-name/`
2. Structure: `.ts`, `.html`, `.scss`
3. Make standalone and exportable
4. Document props and events

### **Adding a Global Service**

1. If it's API: create in `core/api/`
2. If it's business logic: create in `core/services/`
3. Mark as `@Injectable({ providedIn: 'root' })`

### **Adding a Model**

- **Global** (used in multiple features): `core/models/`
- **Feature-specific**: `features/feature-name/models/`

### **Adding a Translation**

1. Edit `assets/i18n/[lang].json`
2. Add key in all languages
3. Use in template: `{{ 'key' | translate }}`
4. Use in component: `this.translate.instant('key')`

### **Adding a Store**

1. Create in `core/state/store-name.store.ts`
2. Use pattern of private signal + readonly public
3. Inject as singleton in components/services

---

## 🛠️ COMMANDS AND SCRIPTS

### **Development**
```bash
npm start          # Start development server (port 4200)
npm run dev        # Alias for npm start
```

### **Build**
```bash
npm run build      # Production build
npm run watch      # Build in watch mode
```

### **Testing**
```bash
npm test           # Run tests with Vitest
```

### **Angular CLI**
```bash
ng generate component features/new-feature/pages/new-page
ng generate service core/services/new-service
```

---

## 📐 CODE CONVENTIONS

### **Naming Conventions**

- **Files**: kebab-case → `user-menu.component.ts`
- **Classes**: PascalCase → `UserMenuComponent`
- **Interfaces**: PascalCase → `UserProfileResult`
- **Variables/Methods**: camelCase → `getUserProfile()`
- **Constants**: UPPER_SNAKE_CASE → `API_BASE_URL`
- **Private signals**: `_signalName`
- **Public readonly signals**: `signalName`

### **Component Structure**

```typescript
@Component({
  selector: 'app-name',
  standalone: true,
  imports: [/* dependencies */],
  templateUrl: './name.component.html',
  styleUrl: './name.component.scss'
})
export class NameComponent {
  // 1. Injections (private readonly)
  private readonly service = inject(Service);
  
  // 2. Signals and public properties (protected for template)
  protected readonly loading = signal(false);
  
  // 3. Lifecycle methods
  ngOnInit() {}
  
  // 4. Public/protected methods
  protected method() {}
  
  // 5. Private methods
  private helper() {}
}
```

### **Dependency Injection**

- Prefer `inject()` over constructor
- Use `private readonly` for injected services
- `protected` if used in template

### **Forms**

- Use `NonNullableFormBuilder` when appropriate
- Declarative validators
- `markAllAsTouched()` before submit if invalid

### **HTML/CSS**

- **Flexbox first** for layouts
- Mobile-first responsive design
- Use CSS variables for colors, spacing
- BEM or similar methodology for CSS classes

---

## 🔐 SECURITY

- **Guards** for route protection
- **Interceptor** for automatic token handling
- **HttpOnly cookies** (if applicable in backend)
- **Automatic refresh token** without user intervention
- **Automatic logout** if refresh fails

---

## 🌍 INTERNATIONALIZATION

### **Supported Languages**
- 🇬🇧 English (en)
- 🇪🇸 Spanish (es)
- 🇩🇪 German (de)
- 🇭🇷 Croatian (hr)

### **Usage in Templates**
```html
<h1>{{ 'login.title' | translate }}</h1>
<button>{{ 'common.submit' | translate }}</button>
```

### **Usage in Components**
```typescript
private readonly translate = inject(TranslateService);

message = this.translate.instant('errors.required');
```

---

## 📊 PROJECT STATUS

### **Completed ✅**
- [x] Feature-based base architecture
- [x] Complete authentication system
- [x] Automatic refresh token
- [x] State management with signals
- [x] Internationalization system
- [x] Basic shared components (toast, spinner, form-error)
- [x] Guards and interceptors
- [x] Functional login
- [x] Basic dashboard

### **In Development 🚧**
- [ ] Invoices feature
- [ ] Complete unit tests
- [ ] E2E tests

### **Planned 📋**
- [ ] More business features
- [ ] PWA support
- [ ] Performance optimizations
- [ ] Storybook for components

---

## 💡 KEY ARCHITECTURAL DECISIONS

### **Why Signals instead of RxJS for state?**
- Signals are native to Angular 21+
- Simpler syntax and less boilerplate
- Better performance and optimized change detection
- RxJS still used for HTTP and complex async logic

### **Why feature-based instead of layer-based?**
- Greater cohesion: everything related to a feature is together
- Facilitates lazy loading per feature
- Scalable: adding features doesn't affect others
- Team-friendly: teams can work on independent features

### **Why standalone components?**
- Angular 21+ recommends standalone
- Less boilerplate (no NgModules)
- Explicit imports improve clarity
- Better tree-shaking

### **Why separate services into `api/` and `services/`?**
- `api/`: only HTTP, single responsibility
- `services/`: business logic, orchestration, store usage
- Facilitates testing and mocking
- Clear layer separation

### **Why no state in the login feature?**
- Login is simple and uses global state from `core/state/user.store.ts`
- Avoids unnecessary duplication
- Principle: feature state only if complex and local to feature

---

## 🎓 GLOSSARY

- **Signal**: Angular's reactive primitive for state
- **Store**: Service that encapsulates state with signals
- **Guard**: Function that protects routes
- **Interceptor**: HTTP middleware
- **Standalone Component**: Component without NgModule
- **Feature**: Business functionality module
- **Lazy Loading**: Deferred loading of modules/components
- **DI**: Dependency Injection

---

## 📞 USING THIS PROMPT WITH OTHER AIs

### **For Claude, GPT-4, Gemini, etc.**

**Recommended initial context:**

> "I'm working on an Angular 21 project called Budgexa. Below I'm providing the complete project architecture. Please read all the information carefully before answering any questions or making any changes.
>
> [PASTE THIS COMPLETE DOCUMENT]
>
> Now, my question/task is: [YOUR SPECIFIC QUESTION]"

### **Usage Examples:**

1. **Add new feature:**
   > "Using the Budgexa architecture, help me create an 'expenses' feature with a main page, service, and necessary models."

2. **Solve bug:**
   > "In the Budgexa project, I have an error in login where the user isn't being saved. How do I debug this following the architecture?"

3. **Refactor code:**
   > "I want to refactor the dashboard component to add charts. How should I structure it following Budgexa principles?"

4. **Add functionality:**
   > "I need to add a real-time notification system. Where would you place it in the Budgexa architecture?"

---

## ✅ CHECKLIST FOR NEW IMPLEMENTATIONS

Before implementing any change, verify:

- [ ] Does the code follow the feature-based structure?
- [ ] Are models in the correct location (core vs feature)?
- [ ] Are services separated (api vs business logic)?
- [ ] Is the component standalone?
- [ ] Are signals used for reactive state?
- [ ] Do injections use `inject()` and are they `readonly`?
- [ ] Is the code responsive and accessible?
- [ ] Are translations used for UI texts?
- [ ] Do forms have validation and error handling?
- [ ] Is the naming convention followed?

---

## 📄 LICENSE AND AUTHORSHIP

**Project**: Budgexa  
**Author**: [Your name/company]  
**Creation date**: 2026  
**Last update**: May 2026

---

## 🎯 CONCLUSION

This architecture is designed to be:
- ✨ **Scalable**: Easy to add new features
- 🧹 **Maintainable**: Clean and well-organized code
- 🚀 **Performant**: Lazy loading, signals, standalone components
- 👥 **Collaborative**: Clear separation of responsibilities
- 📚 **Documented**: Every decision has its justification

**Remember**: Consistency is key. Always follow these patterns and principles in any new implementation.

---

> 💡 **Tip for AIs**: This document is your complete project map. Before suggesting any changes, ensure they are aligned with the principles and structure defined here.
