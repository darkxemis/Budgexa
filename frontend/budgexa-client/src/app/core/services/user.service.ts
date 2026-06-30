import { Injectable, inject } from '@angular/core';
import { UserApiService } from '../api/user-api.service';
import { UserProfileResult, UpdateCurrentUserDto } from '../models/user.model';
import { UserStore } from '../state/user.store';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { ADMIN_ROLES, RoleName } from '../constants/roles.constants';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly api = inject(UserApiService);
  private readonly userStore = inject(UserStore);

  fetchUser(): Observable<UserProfileResult | null> {
    return this.api.me().pipe(
      tap((user: UserProfileResult) => this.userStore.setUser(user)),
      catchError(() => {
        this.userStore.clearUser();
        return of(null);
      })
    );
  }

  getUser(): Observable<UserProfileResult | null> {
    const cached = this.userStore.user();
    if (cached) {
      return of(cached);
    }

    return this.fetchUser();
  }

  updateUser(dto: UpdateCurrentUserDto): Observable<UserProfileResult | null> {
    return this.api.updateMe(dto).pipe(
      tap((user: UserProfileResult) => this.userStore.setUser(user)),
      catchError(() => of(null))
    );
  }

  clearUser() {
    this.userStore.clearUser();
  }

  /** Returns true if the cached current user has any of the provided roles. */
  hasAnyRole(roles: readonly RoleName[]): boolean {
    const user = this.userStore.user();
    if (!user?.roles?.length) {
      return false;
    }
    return user.roles.some(role => roles.includes(role as RoleName));
  }

  /** Reactive check resolved against the (possibly cached) current user. */
  hasAnyRole$(roles: readonly RoleName[]): Observable<boolean> {
    return this.getUser().pipe(
      map(user => !!user?.roles?.some(role => roles.includes(role as RoleName)))
    );
  }

  isAdmin(): boolean {
    return this.hasAnyRole(ADMIN_ROLES);
  }
}
