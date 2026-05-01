import { Injectable, inject } from '@angular/core';

import { AuthApiService } from '../api/auth-api.service';
import { AuthCredentials } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(AuthApiService);

  login(credentials: AuthCredentials) {
    return this.api.login(credentials);
  }

  logout() {
    this.api.logout().subscribe();
  }

  refreshToken() {
    return this.api.refreshToken();
  }
}
