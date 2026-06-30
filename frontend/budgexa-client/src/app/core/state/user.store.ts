import { Injectable, signal } from '@angular/core';
import { UserProfileResult } from '../models/user.model';

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
