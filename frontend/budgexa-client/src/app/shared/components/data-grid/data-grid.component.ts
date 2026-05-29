import {
  Component,
  Input,
  Output,
  EventEmitter,
  signal,
  computed,
  effect,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import {
  GridColumnDef,
  GridFilterDto,
  GridSortDto,
  GridOperator,
  GridRequestDto,
  GridResponseDto,
} from '../../../core/models/grid.model';

@Component({
  selector: 'app-data-grid',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './data-grid.component.html',
  styleUrl: './data-grid.component.scss',
})
export class DataGridComponent<T = any> implements OnInit {
  // Inputs
  @Input({ required: true }) columns: GridColumnDef<T>[] = [];
  @Input() pageSizeOptions = [10, 25, 50, 100];
  @Input() defaultPageSize = 10;
  @Input() showSearch = true;
  @Input() showPagination = true;
  @Input() loading = false;

  @Output() gridStateChange = new EventEmitter<GridRequestDto>();

  data = signal<GridResponseDto<T>>({
    data: [],
    totalCount: 0,
    page: 1,
    pageSize: this.defaultPageSize,
    totalPages: 0,
  });

  // Grid State Signals
  protected readonly currentPage = signal(1);
  protected readonly pageSize = signal(this.defaultPageSize);
  protected readonly sorting = signal<GridSortDto[]>([]);
  protected readonly filters = signal<GridFilterDto[]>([]);
  protected readonly searchTerm = signal('');
  protected readonly inputSearchValue = signal('');

  // UI State
  protected readonly showFilters = signal(false);
  protected readonly filterColumn = signal<string | null>(null);
  protected readonly filterOperator = signal<GridOperator>(GridOperator.Contains);
  protected readonly filterValue = signal('');

  // Search debounce
  private searchDebounceTimer?: ReturnType<typeof setTimeout>;
  private readonly searchDebounceMs = 500;

  // Expose Math for template
  protected readonly Math = Math;

  // Computed
  protected readonly hasData = computed(() => this.data().data.length > 0);
  protected readonly totalPages = computed(() => this.data().totalPages);
  protected readonly totalCount = computed(() => this.data().totalCount);
  protected readonly pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];

    if (total <= 7) {
      for (let i = 1; i <= total; i++) {
        pages.push(i);
      }
    } else {
      if (current <= 4) {
        pages.push(1, 2, 3, 4, 5, -1, total);
      } else if (current >= total - 3) {
        pages.push(1, -1, total - 4, total - 3, total - 2, total - 1, total);
      } else {
        pages.push(1, -1, current - 1, current, current + 1, -1, total);
      }
    }

    return pages;
  });

  // Available operators for filtering
  protected readonly operators = [
    { value: GridOperator.Equal, label: 'grid.operators.equal' },
    { value: GridOperator.NotEqual, label: 'grid.operators.notEqual' },
    { value: GridOperator.Contains, label: 'grid.operators.contains' },
    { value: GridOperator.NotContains, label: 'grid.operators.notContains' },
    { value: GridOperator.StartsWith, label: 'grid.operators.startsWith' },
    { value: GridOperator.EndsWith, label: 'grid.operators.endsWith' },
    { value: GridOperator.GreaterThan, label: 'grid.operators.greaterThan' },
    { value: GridOperator.LessThan, label: 'grid.operators.lessThan' },
    { value: GridOperator.GreaterThanOrEqual, label: 'grid.operators.greaterThanOrEqual' },
    { value: GridOperator.LessThanOrEqual, label: 'grid.operators.lessThanOrEqual' },
  ];

  constructor() {
    effect(() => {
      const request: GridRequestDto = {
        page: this.currentPage(),
        pageSize: this.pageSize(),
        sorting: this.sorting().length > 0 ? this.sorting() : undefined,
        filters: this.filters().length > 0 ? this.filters() : undefined,
        search: this.searchTerm() || undefined,
      };

      this.gridStateChange.emit(request);
    });
  }

  ngOnInit() {
    this.pageSize.set(this.defaultPageSize);
  }

  setData(response: GridResponseDto<T>) {
    this.data.set(response);
  }

  // Sorting
  protected toggleSort(column: GridColumnDef<T>) {
    if (!column.sortable) return;

    const currentSort = this.sorting().find(s => s.column === column.field);

    if (!currentSort) {
      // Add ascending sort
      this.sorting.set([{ column: column.field as string, isDescending: false }]);
    } else if (!currentSort.isDescending) {
      // Change to descending
      this.sorting.set([{ column: column.field as string, isDescending: true }]);
    } else {
      // Remove sort
      this.sorting.set([]);
    }

    this.currentPage.set(1);
  }

  protected getSortIcon(column: GridColumnDef<T>): string {
    const sort = this.sorting().find(s => s.column === column.field);
    if (!sort) return '';
    return sort.isDescending ? '↓' : '↑';
  }

  // Filtering
  protected openFilterDialog(column: GridColumnDef<T>) {
    if (!column.filterable) return;
    
    this.filterColumn.set(column.field as string);
    this.showFilters.set(true);
    
    // Check if there's an existing filter for this column
    const existingFilter = this.filters().find(f => f.column === column.field);
    if (existingFilter) {
      this.filterOperator.set(existingFilter.operator);
      this.filterValue.set(existingFilter.value || '');
    } else {
      this.filterOperator.set(GridOperator.Contains);
      this.filterValue.set('');
    }
  }

  protected applyFilter() {
    const column = this.filterColumn();
    if (!column) return;

    const currentFilters = this.filters().filter(f => f.column !== column);
    
    if (this.filterValue()) {
      currentFilters.push({
        column,
        operator: this.filterOperator(),
        value: this.filterValue(),
      });
    }

    this.filters.set(currentFilters);
    this.showFilters.set(false);
    this.currentPage.set(1);
  }

  protected cancelFilter() {
    this.showFilters.set(false);
    this.filterColumn.set(null);
    this.filterValue.set('');
  }

  protected removeFilter(column: string) {
    this.filters.set(this.filters().filter(f => f.column !== column));
    this.currentPage.set(1);
  }

  protected hasFilter(column: GridColumnDef<T>): boolean {
    return this.filters().some(f => f.column === column.field);
  }

  protected getFilterForColumn(column: GridColumnDef<T>): GridFilterDto | undefined {
    return this.filters().find(f => f.column === column.field);
  }

  // Search
  protected onSearchChange(value: string) {
    // Update input value immediately for UI responsiveness
    this.inputSearchValue.set(value);

    // Clear previous timer
    if (this.searchDebounceTimer) {
      clearTimeout(this.searchDebounceTimer);
    }

    // Set new timer to update search term after debounce delay
    this.searchDebounceTimer = setTimeout(() => {
      this.searchTerm.set(value);
      this.currentPage.set(1);
    }, this.searchDebounceMs);
  }

  protected clearSearch() {
    // Clear debounce timer when clearing search
    if (this.searchDebounceTimer) {
      clearTimeout(this.searchDebounceTimer);
    }
    this.inputSearchValue.set('');
    this.searchTerm.set('');
  }

  // Pagination
  protected goToPage(page: number) {
    if (page < 1 || page > this.totalPages() || page === this.currentPage()) return;
    this.currentPage.set(page);
  }

  protected nextPage() {
    if (this.currentPage() < this.totalPages()) {
      this.currentPage.update(p => p + 1);
    }
  }

  protected previousPage() {
    if (this.currentPage() > 1) {
      this.currentPage.update(p => p - 1);
    }
  }

  protected changePageSize(size: number) {
    this.pageSize.set(size);
    this.currentPage.set(1);
  }

  // Cell rendering
  protected getCellValue(row: T, column: GridColumnDef<T>): any {
    if (column.cellTemplate) {
      return column.cellTemplate(row);
    }
    
    const field = column.field as string;
    return this.getNestedValue(row, field);
  }

  private getNestedValue(obj: any, path: string): any {
    return path.split('.').reduce((current, prop) => current?.[prop], obj);
  }

  // Refresh
  refresh() {
    this.currentPage.set(1);
    this.gridStateChange.emit({
      page: this.currentPage(),
      pageSize: this.pageSize(),
      sorting: this.sorting().length > 0 ? this.sorting() : undefined,
      filters: this.filters().length > 0 ? this.filters() : undefined,
      search: this.searchTerm() || undefined,
    });
  }

  // Clear all filters and sorting
  clearAll() {
    // Clear debounce timer
    if (this.searchDebounceTimer) {
      clearTimeout(this.searchDebounceTimer);
    }
    
    this.sorting.set([]);
    this.filters.set([]);
    this.inputSearchValue.set('');
    this.searchTerm.set('');
    this.currentPage.set(1);
  }
}
