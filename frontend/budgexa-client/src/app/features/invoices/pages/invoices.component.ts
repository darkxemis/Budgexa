import { Component, signal } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { UserMenuComponent } from '../../../shared/components/user-menu/user-menu.component';

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [TranslateModule, SpinnerComponent, UserMenuComponent],
  templateUrl: './invoices.component.html',
  styleUrl: './invoices.component.scss'
})
export class InvoicesComponent {
  protected readonly loading = signal(false);
}

