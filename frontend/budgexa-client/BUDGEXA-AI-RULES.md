# 🎯 BUDGEXA - AI Quick Reference Guide

> **IMPORTANT**: Read this COMPLETE document before making any code changes.
> Apply ALL rules in every implementation.

---

## 🚨 RULE #0: DON'T REINVENT THE WHEEL (MOST CRITICAL)

### **BEFORE creating ANY component, service, or logic:**

1. **CHECK what ALREADY EXISTS in the project**
2. **REUSE existing components and services**
3. **DON'T duplicate logic that already exists**

### **Existing Components & Services (DO NOT RECREATE):**

#### **🎯 Shared Components** (`src/app/shared/components/`)
- ✅ **UserMenuComponent** - User dropdown menu (logout, settings)
- ✅ **UserSettingsModalComponent** - Modal to edit user profile
- ✅ **SpinnerComponent** - Loading indicator
- ✅ **ToastComponent** + **ToastService** - Notifications system
- ✅ **FormErrorComponent** - Display form validation errors
- ✅ **DashboardCardComponent** - Reusable card for dashboard with icon, title, description
  ```typescript
  // ✅ USE THIS for dashboard cards
  <app-dashboard-card
    iconSrc="assets/images/dashboard/icon.svg"
    title="dashboard.cards.title"
    description="dashboard.cards.description"
    (cardClick)="navigate()">
  </app-dashboard-card>
  ```

#### **🔧 Core Services** (`src/app/core/`)
- ✅ **UserStore** (`core/state/user.store.ts`) - Global user state with signals
  ```typescript
  // ✅ USE THIS to get user data
  private readonly userStore = inject(UserStore);
  readonly user = this.userStore.user; // Signal<UserProfileResult | null>
  ```
- ✅ **AuthService** (`core/services/auth.service.ts`) - Login, logout, refresh token
- ✅ **UserService** (`core/services/user.service.ts`) - User profile operations
- ✅ **LanguageService** (`core/i18n/language.service.ts`) - Change language
- ✅ **performLogout()** (`core/utils/auth.utils.ts`) - Logout utility function

#### **🌐 API Services** (`core/api/`)
- ✅ **AuthApiService** - Authentication endpoints
- ✅ **UserApiService** - User profile endpoints
- ✅ **LanguageApiService** - Language endpoints

#### **📦 Existing Features** (`features/`)
- ✅ **DashboardComponent** (`features/dashboard/pages/`) - Main dashboard with navigation cards
  - Uses DashboardCardComponent for clickable feature cards
  - Displays welcome message with user name from UserStore
  - Routes: `/dashboard`
  
- ✅ **InvoicesComponent** (`features/invoices/pages/`) - Invoices management
  - Grid layout prepared for invoice cards (1/2/3 columns: mobile/tablet/desktop)
  - Routes: `/invoices`
  
- ✅ **UsersComponent** (`features/users/pages/`) - User management
  - Grid layout prepared for user cards (1/2/3 columns: mobile/tablet/desktop)
  - Routes: `/users`

**Pattern for all features:**
```scss
// Grid responsive común para features
.feature-grid {
  display: grid;
  grid-template-columns: 1fr;  // Mobile
  gap: 1rem;
  
  @media (min-width: 768px) {
    grid-template-columns: repeat(2, 1fr);  // Tablet
  }
  
  @media (min-width: 1024px) {
    grid-template-columns: repeat(3, 1fr);  // Desktop
  }
}
```

#### **🎨 Assets** (`src/assets/`)
- ✅ **Dashboard Icons** (`images/dashboard/`)
  - `invoices-icon.svg` - Purple gradient SVG icon for invoices
  - `users-icon.svg` - Purple gradient SVG icon with 3 person silhouettes
  - **Pattern**: All dashboard icons use purple gradient (#7b2ff2, #6b1fe5, #5b1fd9)
  - **Format**: SVG for scalability (120x120 viewBox)

### **❌ WRONG Example - Duplicating existing logic:**
```typescript
// ❌ DON'T DO THIS - UserStore already exists!
@Component({...})
export class UserComponent {
  protected readonly userName = signal('John Doe'); // WRONG: hardcoded
  protected readonly userEmail = signal('john@example.com'); // WRONG
  
  protected logout(): void {
    this.isLoggedIn.set(false); // WRONG: use performLogout()
  }
}
```

### **✅ CORRECT Example - Using existing services:**
```typescript
// ✅ DO THIS - Use existing UserStore and services
import { Component, inject } from '@angular/core';
import { UserStore } from '../../../core/state/user.store';
import { performLogout } from '../../../core/utils/auth.utils';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [TranslateModule, UserMenuComponent],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.scss'
})
export class UserProfileComponent {
  private readonly userStore = inject(UserStore);
  private readonly router = inject(Router);
  
  // ✅ Get user from global store (already exists!)
  readonly user = this.userStore.user;
  
  protected logout(): void {
    // ✅ Use existing utility function
    performLogout(this.router);
  }
}
```

---

## ⚡ CRITICAL RULES (MANDATORY)

### 1. **Component Structure**
```typescript
// ✅ CORRECT
import { Component, signal } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-example',
  standalone: true,                    // ALWAYS standalone
  imports: [TranslateModule],          // Explicit imports
  templateUrl: './example.component.html',
  styleUrl: './example.component.scss' // styleUrl (singular, not styleUrls)
})
export class ExampleComponent {
  // 1. Injections with inject() and readonly
  private readonly service = inject(SomeService);
  
  // 2. Signals for reactive state
  protected readonly loading = signal(false);
  protected readonly data = signal<Data[]>([]);
  
  // 3. Methods
  protected loadData(): void {
    this.loading.set(true);
    // logic
  }
}

// ❌ INCORRECT
styleUrls: ['./example.component.scss']  // DON'T use styleUrls (plural)
private service = inject(SomeService);   // Missing readonly
protected loading = false;                // DON'T use direct booleans, use signal
```

### 2. **Signals (Reactive State)**
```typescript
// ✅ ALWAYS use signals for state
protected readonly loading = signal(false);
protected readonly user = signal<User | null>(null);
protected readonly items = signal<Item[]>([]);

// Update values
this.loading.set(true);
this.user.set(userData);

// ❌ NEVER use simple variables for state
protected loading = false;  // BAD
protected user: User | null = null; // BAD
```

### 3. **Injections**
```typescript
// ✅ CORRECT - inject() with readonly
private readonly router = inject(Router);
private readonly userStore = inject(UserStore);

// ❌ INCORRECT
private router = inject(Router);  // Missing readonly
constructor(private router: Router) {} // DON'T use constructor
```

### 4. **TranslateModule**
```typescript
// ✅ If HTML uses {{ 'key' | translate }}, you MUST import TranslateModule
@Component({
  imports: [TranslateModule, OtherModules]
})

// Template
<h1>{{ 'invoices.title' | translate }}</h1>
<button>{{ 'common.save' | translate }}</button>
```

### 5. **i18n Translations**
```json
// ✅ ALWAYS add to all 4 languages: en.json, es.json, de.json, hr.json
// en.json
{
  "invoices": {
    "title": "Invoices",
    "create": "Create Invoice"
  }
}

// es.json
{
  "invoices": {
    "title": "Facturas",
    "create": "Crear Factura"
  }
}
```

### 6. **Folder Structure**
```
features/
  feature-name/
    pages/          # Pages/containers
    components/     # Only if there are reusable components WITHIN the feature
    services/       # Only if there's feature-specific logic
    models/         # Only if there are feature-specific models

core/
  api/              # HTTP services
  services/         # Global business logic
  models/           # Global models
  state/            # Global stores with signals
  guards/           # Route guards
  interceptors/     # HTTP interceptors

shared/
  components/       # Reusable components across the ENTIRE project
```

### 7. **API vs Service Separation**
```typescript
// ✅ CORRECT - API only does HTTP
@Injectable({ providedIn: 'root' })
export class InvoiceApiService {
  private readonly http = inject(HttpClient);
  
  getInvoices(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>('/api/invoices');
  }
}

// ✅ CORRECT - Service has business logic
@Injectable({ providedIn: 'root' })
export class InvoiceService {
  private readonly api = inject(InvoiceApiService);
  private readonly store = inject(InvoiceStore);
  
  loadInvoices(): Observable<Invoice[]> {
    return this.api.getInvoices().pipe(
      tap(invoices => this.store.setInvoices(invoices))
    );
  }
}
```

### 8. **CSS/SCSS**
```scss
// ✅ CORRECT - DON'T use non-existent variables
.container {
  background-color: #f5f6fa;  // Direct values
  color: #333;
}

// ❌ INCORRECT - DON'T use undefined variables
@import '../../../styles/variables';  // If it doesn't exist, DON'T use it
.container {
  background-color: $background-color;  // BAD if not defined
}
```

### 9. **Flexbox for Layouts**
```scss
// ✅ ALWAYS use Flexbox
.container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

// Responsive: mobile first
.header {
  display: flex;
  flex-direction: column;  // Mobile
  
  @media (min-width: 768px) {
    flex-direction: row;  // Tablet/Desktop
  }
}
```

---

## 📋 PRE-IMPLEMENTATION CHECKLIST

Before writing code, verify:

### **🔍 FIRST: Check Existing Code**
- [ ] Did you check if a similar component/service already exists?
- [ ] Are you reusing UserStore for user data instead of creating new signals?
- [ ] Are you using UserMenuComponent instead of creating a new user menu?
- [ ] Are you using performLogout() instead of implementing logout logic?
- [ ] Are you using existing API services instead of creating new HTTP calls?

### **✅ Component Rules**
- [ ] Is the component `standalone: true`?
- [ ] Do you use `inject()` with `readonly`?
- [ ] Does state use `signal()` instead of simple variables?
- [ ] Do you use `styleUrl` (singular) not `styleUrls`?
- [ ] Do you import `TranslateModule` if using `| translate`?
- [ ] Did you add translations to all 4 languages (en, es, de, hr)?
- [ ] Do methods called from HTML exist in the TS?
- [ ] Don't inject services you don't use?
- [ ] Does CSS not import non-existent files?
- [ ] Do you use Flexbox for layouts?

---

## 🎨 LAYERED ARCHITECTURE

```
Component (UI)
    ↓ inject
Service (Business logic)
    ↓ inject
ApiService (HTTP)
    ↓
Backend API
```

**Login Example:**
```
LoginComponent 
  → AuthService.login() 
    → AuthApiService.login() 
      → POST /api/auth/login
```

---

## 🔄 COMMON PATTERNS

### **Store with Signals**
```typescript
@Injectable({ providedIn: 'root' })
export class FeatureStore {
  private readonly _items = signal<Item[]>([]);
  readonly items = this._items.asReadonly();
  
  setItems(items: Item[]): void {
    this._items.set(items);
  }
  
  addItem(item: Item): void {
    this._items.update(current => [...current, item]);
  }
  
  clear(): void {
    this._items.set([]);
  }
}
```

### **Typical Page Component**
```typescript
@Component({
  selector: 'app-feature-page',
  standalone: true,
  imports: [TranslateModule, CommonModule, UserMenuComponent],
  templateUrl: './feature-page.component.html',
  styleUrl: './feature-page.component.scss'
})
export class FeaturePageComponent {
  private readonly service = inject(FeatureService);
  private readonly router = inject(Router);
  
  protected readonly loading = signal(false);
  protected readonly data = signal<Data[]>([]);
  
  ngOnInit(): void {
    this.loadData();
  }
  
  protected loadData(): void {
    this.loading.set(true);
    this.service.getData().subscribe({
      next: (data) => {
        this.data.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }
}
```

### **Functional Guard**
```typescript
export const authGuard: CanActivateFn = () => {
  const userStore = inject(UserStore);
  const router = inject(Router);
  
  if (userStore.user()?.id) {
    return true;
  }
  
  return router.createUrlTree(['/login']);
};
```

### **Route Lazy Loading**
```typescript
{
  path: 'invoices',
  loadComponent: () => import('./features/invoices/pages/invoices.component')
    .then(m => m.InvoicesComponent),
  canActivate: [authGuard]
}
```

---

## 🎯 NAMING CONVENTIONS

```typescript
// Files: kebab-case
user-menu.component.ts
auth.service.ts
invoice-api.service.ts

// Classes/Interfaces: PascalCase
class UserMenuComponent
interface UserProfile
type InvoiceStatus

// Variables/methods: camelCase
const userName = 'John';
getUserProfile()
loadInvoices()

// Constants: UPPER_SNAKE_CASE
const API_BASE_URL = 'https://api.example.com';
const MAX_RETRIES = 3;

// Private signals: _signalName
private readonly _user = signal<User | null>(null);
readonly user = this._user.asReadonly();

// Protected for template
protected readonly loading = signal(false);
```

---

## 🚨 COMMON MISTAKES TO AVOID

### ❌ Mistake 0: DUPLICATING EXISTING LOGIC (MOST CRITICAL!)
```typescript
// ❌ VERY BAD - Creating user logic when UserStore exists!
@Component({...})
export class UserComponent {
  protected readonly userName = signal('John Doe');     // WRONG: hardcoded
  protected readonly userEmail = signal('john@...');    // WRONG: use UserStore
  protected readonly isLoggedIn = signal(true);         // WRONG: use UserStore
  
  protected logout(): void {
    this.isLoggedIn.set(false);  // WRONG: use performLogout()
  }
}

// ✅ SOLUTION: Use existing services and stores
import { Component, inject } from '@angular/core';
import { UserStore } from '../../../core/state/user.store';
import { performLogout } from '../../../core/utils/auth.utils';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [TranslateModule, UserMenuComponent],  // Use existing components!
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.scss'
})
export class UserProfileComponent {
  private readonly userStore = inject(UserStore);
  private readonly router = inject(Router);
  
  // ✅ Get user from existing global store
  readonly user = this.userStore.user;
  
  // ✅ Use existing utility
  protected logout(): void {
    performLogout(this.router);
  }
}
```

### ❌ Mistake 1: Forgetting TranslateModule
```typescript
// HTML uses translate but it's not imported
<h1>{{ 'title' | translate }}</h1>

// SOLUTION: Import TranslateModule
@Component({
  imports: [TranslateModule]
})
```

### ❌ Mistake 2: Undefined methods
```html
<!-- HTML calls method that doesn't exist -->
<button (click)="createInvoice()">Create</button>

<!-- SOLUTION: Define method in TS -->
protected createInvoice(): void {
  // logic
}
```

### ❌ Mistake 3: styleUrls plural
```typescript
// BAD
styleUrls: ['./component.scss']

// GOOD (Angular 21+)
styleUrl: './component.scss'
```

### ❌ Mistake 4: Injections without readonly
```typescript
// BAD
private router = inject(Router);

// GOOD
private readonly router = inject(Router);
```

### ❌ Mistake 5: State without signals
```typescript
// BAD
protected loading = false;
protected items: Item[] = [];

// GOOD
protected readonly loading = signal(false);
protected readonly items = signal<Item[]>([]);
```

### ❌ Mistake 6: Importing non-existent SCSS variables
```scss
// BAD - if the file doesn't exist
@import '../../../styles/variables';

// GOOD - use direct values
.container {
  background-color: #f5f6fa;
}
```

### ❌ Mistake 7: Poor mobile optimization
```scss
// BAD - Fixed padding that causes overflow on mobile
.container {
  padding: 2rem;  // Too much on mobile
}

.card {
  padding: 2rem 1.5rem;  // Same padding for all screens
}

// GOOD - Responsive padding with mobile-first
.container {
  padding: 1.5rem 0.5rem;  // Minimal on mobile
  box-sizing: border-box;
  overflow-x: hidden;  // Prevent horizontal scroll
  
  @media (min-width: 640px) {
    padding: 2rem 1.5rem;
  }
  
  @media (min-width: 768px) {
    padding: 3rem 2rem;
  }
}

.card {
  padding: 1.25rem 1rem;  // Compact on mobile
  width: 100%;
  box-sizing: border-box;
  max-width: 100%;
  
  @media (min-width: 640px) {
    padding: 2rem 1.5rem;
  }
}
```

---

## 🎓 REASONING (Chain of Thought)

When implementing something new, think:

0. **Does this ALREADY EXIST? (CHECK FIRST!)**
   - User data? → Use `UserStore` (don't create new signals)
   - User menu? → Use `UserMenuComponent` (don't create new)
   - User settings? → Use `UserSettingsModalComponent`
   - Logout? → Use `performLogout()` utility
   - Authentication? → Use `AuthService`
   - HTTP calls? → Check if API service exists in `core/api/`
   - Notifications? → Use `ToastService`
   - Loading indicator? → Use `SpinnerComponent`

1. **Where does this go?**
   - Is it global? → `core/`
   - Is it reusable? → `shared/`
   - Is it domain-specific? → `features/feature-name/`

2. **What type of file is it?**
   - Does it do HTTP? → `api/`
   - Does it have business logic? → `services/`
   - Does it manage state? → `state/`
   - Is it UI? → `components/` or `pages/`

3. **Does it need state?**
   - Is it reactive? → Use `signal()`
   - Is it global? → Create store in `core/state/`
   - Is it local? → Signal in the component

4. **Does it need translation?**
   - Does it have visible text? → Add to 4 languages
   - Does it use translate pipe? → Import `TranslateModule`

5. **Does it follow conventions?**
   - Names in kebab-case?
   - Standalone imports?
   - Readonly in injections?
   - styleUrl singular?

---

## ✅ COMPLETE EXAMPLE: Creating "Expenses" Feature

### 1. Structure
```
features/
  expenses/
    pages/
      expenses.component.ts
      expenses.component.html
      expenses.component.scss
```

### 2. Component
```typescript
import { Component, signal } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';

@Component({
  selector: 'app-expenses',
  standalone: true,
  imports: [TranslateModule, UserMenuComponent],
  templateUrl: './expenses.component.html',
  styleUrl: './expenses.component.scss'
})
export class ExpensesComponent {
  protected readonly loading = signal(false);
  
  protected createExpense(): void {
    console.log('Creating expense...');
  }
}
```

### 3. Template
```html
<div class="expenses-container">
  <header class="expenses-header">
    <h1>{{ 'expenses.title' | translate }}</h1>
    <app-user-menu></app-user-menu>
  </header>
  
  <main class="expenses-main">
    <button (click)="createExpense()">
      {{ 'expenses.create' | translate }}
    </button>
  </main>
</div>
```

### 4. Styles
```scss
.expenses-container {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background-color: #f5f6fa;
}

.expenses-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 2rem;
  background: linear-gradient(120deg, #232526 0%, #7b2ff2 100%);
  color: white;
}
```

### 5. Translations (4 languages)
```json
// en.json
{
  "expenses": {
    "title": "Expenses",
    "create": "Create Expense"
  }
}

// es.json
{
  "expenses": {
    "title": "Gastos",
    "create": "Crear Gasto"
  }
}

// de.json, hr.json... (same structure)
```

### 6. Route
```typescript
// app.routes.ts
{
  path: 'expenses',
  loadComponent: () => import('./features/expenses/pages/expenses.component')
    .then(m => m.ExpensesComponent),
  canActivate: [authGuard]
}
```

---

## 🎯 ULTRA-QUICK SUMMARY

**🚨 MOST IMPORTANT:**
- ❗ **CHECK what exists BEFORE creating anything**
- ❗ **REUSE UserStore, UserMenuComponent, performLogout(), etc.**
- ❗ **DON'T duplicate existing logic**

**Component:**
- ✅ `standalone: true`
- ✅ `styleUrl` (singular)
- ✅ `inject()` + `readonly`
- ✅ `signal()` for state
- ✅ `TranslateModule` if using `| translate`

**Architecture:**
- 📁 `core/` → Global
- 📁 `shared/` → Reusable
- 📁 `features/` → Domain-specific

**Always:**
- 🌍 Translate to 4 languages
- 📱 Flexbox for layouts
- 🔒 `readonly` in injections
- ⚡ Signals for reactive state

---

> **For AI**: Read ALL these rules before generating code. Check the checklist. Reason step by step where each piece goes. Don't forget any critical rule!
