import { Component, inject, signal } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [TranslateModule, UserMenuComponent],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent {
  protected readonly loading = signal(false);
}
