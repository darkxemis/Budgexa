import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserStore } from '../../state/user.store';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterModule, CommonModule],
  template: `
    <div class="dashboard-container">
      <h1>Welcome</h1>
      <div *ngIf="user() as userInfo">
        <p><strong>Email:</strong> {{ userInfo.email }}</p>
        <p><strong>Name:</strong> {{ userInfo.firstName }} {{ userInfo.lastName }}</p>
      </div>
      <div *ngIf="!user()">
        <p>No user info loaded.</p>
      </div>
    </div>
  `,
  styles: [
    `
      .dashboard-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        height: 100vh;
        background: #f5f6fa;
      }
      h1 {
        font-size: 2.5rem;
        color: #333;
      }
      p {
        font-size: 1.2rem;
        color: #444;
      }
    `,
  ],
})
export class DashboardComponent {
  private readonly userStore = inject(UserStore);
  readonly user = this.userStore.user;
}
