import { Component, input, output } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-dashboard-card',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './dashboard-card.component.html',
  styleUrl: './dashboard-card.component.scss'
})
export class DashboardCardComponent {
  // Inputs
  readonly iconSrc = input.required<string>();
  readonly title = input.required<string>();
  readonly description = input.required<string>();
  
  // Output
  readonly cardClick = output<void>();
  
  protected handleClick(): void {
    this.cardClick.emit();
  }
}
