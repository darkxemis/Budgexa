import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

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
    path: 'invoices',
    loadComponent: () =>
      import('./features/invoices/pages/invoices.component').then((m) => m.InvoicesComponent),
    canActivate: [authGuard],
  },
  {
    path: 'users',
    loadComponent: () =>
      import('./features/users/pages/users.component').then((m) => m.UsersComponent),
    canActivate: [authGuard],
  },
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: '**', redirectTo: 'login' },
];
