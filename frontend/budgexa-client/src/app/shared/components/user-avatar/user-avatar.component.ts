import { Component, input } from '@angular/core';
import { UpperCasePipe } from '@angular/common';

@Component({
  selector: 'app-user-avatar',
  standalone: true,
  imports: [UpperCasePipe],
  template: `
    @if (profileImageUrl(); as imageUrl) {
      <img [src]="imageUrl" [alt]="'User avatar'" class="user-avatar-img" />
    } @else {
      <span class="user-avatar-letter">{{ firstName().charAt(0) | uppercase }}</span>
    }
  `,
  styles: `
    :host {
      display: inline-flex;
    }

    .user-avatar-img {
      width: 2.2rem;
      height: 2.2rem;
      border-radius: 50%;
      object-fit: cover;
    }

    .user-avatar-letter {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 2.2rem;
      height: 2.2rem;
      border-radius: 50%;
      background-color: #7b2ff2;
      color: white;
      font-weight: bold;
      font-size: 1.2rem;
    }
  `,
})
export class UserAvatarComponent {
  readonly firstName = input.required<string>();
  readonly profileImageUrl = input<string | null | undefined>(null);
}
