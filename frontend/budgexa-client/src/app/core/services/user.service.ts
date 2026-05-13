import { Injectable, inject } from '@angular/core';
import { UserApiService } from '../api/user-api.service';
import { UserProfileResult, UpdateCurrentUserDto } from '../models/user.model';
import { UserStore } from '../state/user.store';
import { Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

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
    console.log('Updating user with DTO:', dto);
    return this.api.updateMe(dto).pipe(
      tap((user: UserProfileResult) => this.userStore.setUser(user)),
      catchError(() => of(null))
    );
  }

  clearUser() {
    this.userStore.clearUser();
  }
}
