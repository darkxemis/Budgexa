import { Component, signal, inject, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { LanguageSelectorComponent } from '../../../../shared/components/language-selector/language-selector.component';
import { PublicBudgetApiService } from '../../services/public-budget-api.service';
import { PublicBudgetStore } from '../../services/public-budget.store';
import { Guid } from '../../../../core/models/guid.model';

@Component({
  selector: 'app-public-budget-prompt',
  standalone: true,
  imports: [FormsModule, TranslateModule, SpinnerComponent, LanguageSelectorComponent],
  templateUrl: './public-budget-prompt.component.html',
  styleUrl: './public-budget-prompt.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicBudgetPromptComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(PublicBudgetApiService);
  private readonly store = inject(PublicBudgetStore);

  protected readonly userRequest = signal('');
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  private get companyId(): Guid {
    return this.route.snapshot.paramMap.get('companyId') as Guid;
  }

  protected get isValid(): boolean {
    return this.userRequest().trim().length >= 10;
  }

  protected onSubmit(): void {
    if (!this.isValid || this.loading()) return;

    this.loading.set(true);
    this.error.set(null);
    this.store.setCompanyId(this.companyId);

    this.api.generateWithAi({
      companyId: this.companyId,
      userRequest: this.userRequest().trim(),
    }).subscribe({
      next: (response) => {
        this.store.setAiResponse(response);
        this.loading.set(false);
        this.router.navigate(['../review'], { relativeTo: this.route });
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status === 429) {
          this.error.set('publicBudget.errors.rateLimit');
        } else if (err.status === 404) {
          this.error.set('publicBudget.errors.companyNotFound');
        } else {
          this.error.set('publicBudget.errors.generic');
        }
      },
    });
  }
}
