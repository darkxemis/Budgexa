import { Component, signal, computed, inject, ChangeDetectionStrategy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { SpinnerComponent } from '../../../../shared/components/spinner/spinner.component';
import { LanguageSelectorComponent } from '../../../../shared/components/language-selector/language-selector.component';
import { LanguageService } from '../../../../core/i18n/language.service';
import { PublicBudgetApiService } from '../../services/public-budget-api.service';
import { PublicBudgetStore } from '../../services/public-budget.store';
import { PublicBudgetAiItem, PublicItemDto } from '../../models/public-budget.model';
import { Guid } from '../../../../core/models/guid.model';

interface ReviewItem extends PublicBudgetAiItem {
  selected: boolean;
}

@Component({
  selector: 'app-public-budget-review',
  standalone: true,
  imports: [FormsModule, DecimalPipe, TranslateModule, SpinnerComponent, LanguageSelectorComponent],
  templateUrl: './public-budget-review.component.html',
  styleUrl: './public-budget-review.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicBudgetReviewComponent {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(PublicBudgetApiService);
  private readonly store = inject(PublicBudgetStore);
  private readonly languageService = inject(LanguageService);

  protected readonly items = signal<ReviewItem[]>([]);
  protected readonly firstName = signal('');
  protected readonly lastName = signal('');
  protected readonly phone = signal('');
  protected readonly email = signal('');
  protected readonly downloading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly downloadSuccess = signal(false);

  // Add item search state
  protected readonly showAddItem = signal(false);
  protected readonly itemSearchQuery = signal('');
  protected readonly searchResults = signal<PublicItemDto[]>([]);
  protected readonly searching = signal(false);
  private _searchDebounce?: ReturnType<typeof setTimeout>;

  private get companyId(): Guid {
    return this.store.companyId() ?? this.route.snapshot.paramMap.get('companyId') as Guid;
  }

  protected readonly selectedItems = computed(() =>
    this.items().filter(item => item.selected)
  );

  protected readonly totals = computed(() => {
    const selected = this.selectedItems();
    const subtotal = selected.reduce((sum, item) => sum + item.unitPrice * item.quantity, 0);
    const taxAmount = selected.reduce((sum, item) => sum + item.unitPrice * item.quantity * (item.taxRate / 100), 0);
    return {
      subtotal,
      taxAmount,
      total: subtotal + taxAmount,
    };
  });

  protected readonly isFormValid = computed(() =>
    this.firstName().trim().length >= 2 &&
    this.lastName().trim().length >= 2 &&
    (this.phone().trim().length > 0 || this.email().trim().length > 0) &&
    this.selectedItems().length > 0
  );

  constructor() {
    const response = this.store.aiResponse();
    if (response) {
      this.items.set(response.items.map(item => ({ ...item, selected: true })));
    } else {
      this.router.navigate(['../prompt'], { relativeTo: this.route });
    }
  }

  protected toggleItem(index: number): void {
    this.items.update(items =>
      items.map((item, i) => i === index ? { ...item, selected: !item.selected } : item)
    );
  }

  protected updateQuantity(index: number, quantity: number): void {
    if (quantity < 1) return;
    this.items.update(items =>
      items.map((item, i) => i === index ? { ...item, quantity } : item)
    );
  }

  protected removeItem(index: number): void {
    this.items.update(items => items.filter((_, i) => i !== index));
  }

  protected toggleAddItem(): void {
    this.showAddItem.update(v => !v);
    if (!this.showAddItem()) {
      this.itemSearchQuery.set('');
      this.searchResults.set([]);
    }
  }

  protected onSearchInput(query: string): void {
    this.itemSearchQuery.set(query);
    clearTimeout(this._searchDebounce);

    if (query.trim().length < 3) {
      this.searchResults.set([]);
      return;
    }

    this._searchDebounce = setTimeout(() => {
      this.searching.set(true);
      this.api.getItems(this.companyId, query).subscribe({
        next: (results) => {
          // Filter out items already in the list
          const existingIds = new Set(this.items().map(i => i.itemId));
          this.searchResults.set(results.filter(r => !existingIds.has(r.itemId)));
          this.searching.set(false);
        },
        error: () => {
          this.searchResults.set([]);
          this.searching.set(false);
        },
      });
    }, 500);
  }

  protected addItem(item: PublicItemDto): void {
    const newItem: ReviewItem = {
      itemId: item.itemId,
      productName: item.productName,
      unitPrice: item.unitPrice,
      taxRate: item.taxRate,
      unit: item.unit,
      quantity: 1,
      selected: true,
    };
    this.items.update(items => [...items, newItem]);
    // Remove from search results
    this.searchResults.update(results => results.filter(r => r.itemId !== item.itemId));
  }

  protected goBack(): void {
    this.router.navigate(['../prompt'], { relativeTo: this.route });
  }

  protected onConfirmAndDownload(): void {
    if (!this.isFormValid() || this.downloading()) return;

    this.downloading.set(true);
    this.error.set(null);

    const languageCode = this.languageService.current || 'en';

    this.api.confirmAndDownload({
      companyId: this.companyId,
      customerFirstName: this.firstName().trim(),
      customerLastName: this.lastName().trim(),
      customerPhone: this.phone().trim() || null,
      customerEmail: this.email().trim() || null,
      lines: this.selectedItems().map(item => ({
        itemId: item.itemId,
        quantity: item.quantity,
      })),
    }, languageCode).subscribe({
      next: (blob) => {
        this.downloading.set(false);
        this.downloadSuccess.set(true);
        this.triggerDownload(blob);
      },
      error: (err) => {
        this.downloading.set(false);
        if (err.status === 429) {
          this.error.set('publicBudget.errors.rateLimit');
        } else if (err.status === 404) {
          this.error.set('publicBudget.errors.itemsNotFound');
        } else {
          this.error.set('publicBudget.errors.generic');
        }
      },
    });
  }

  private triggerDownload(blob: Blob): void {
    const url = window.URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = `budget-${Date.now()}.pdf`;
    anchor.click();
    window.URL.revokeObjectURL(url);
  }
}
