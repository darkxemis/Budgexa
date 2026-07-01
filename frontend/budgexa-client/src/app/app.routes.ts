import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { adminGuard } from './core/guards/admin.guard';
import { customersGuard } from './core/guards/customers.guard';
import { itemsGuard } from './core/guards/items.guard';
import { budgetsGuard } from './core/guards/budgets.guard';
import { invoicesGuard } from './core/guards/invoices.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/pages/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/pages/dashboard.component').then((m) => m.DashboardComponent),
    canActivate: [authGuard],
  },
  {
    path: '',
    loadComponent: () => import('./shared/layouts/main-layout/main-layout.component').then((m) => m.MainLayoutComponent),
    canActivate: [authGuard],
    children: [
      {
        path: 'customers',
        loadComponent: () =>
          import('./features/customers/pages/customers.component').then((m) => m.CustomersComponent),
        canActivate: [customersGuard],
      },
      {
        path: 'items',
        loadComponent: () =>
          import('./features/items/pages/items.component').then((m) => m.ItemsComponent),
        canActivate: [itemsGuard],
      },
      {
        path: 'budgets',
        loadComponent: () =>
          import('./features/budgets/pages/budgets.component').then((m) => m.BudgetsComponent),
        canActivate: [budgetsGuard],
      },
      {
        path: 'invoices',
        loadComponent: () =>
          import('./features/invoices/pages/invoices.component').then((m) => m.InvoicesComponent),
        canActivate: [invoicesGuard],
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./features/users/pages/users.component').then((m) => m.UsersComponent),
        canActivate: [adminGuard],
      },
    ],
  },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' },
];
