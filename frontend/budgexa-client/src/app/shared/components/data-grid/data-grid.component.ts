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
import { SelectorOption } from '../../../core/models/selector.model';
import { Guid } from '../../../core/models/guid.model';
import { AutocompleteSelectorComponent } from '../autocomplete-selector/autocomplete-selector.component';

@Component({
  selector: 'app-data-grid',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, AutocompleteSelectorComponent],
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
  @Input() enableColumnReorder = true;

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
  protected readonly filterColumn = signal<GridColumnDef<T> | null>(null);
  protected readonly filterOperator = signal<GridOperator>(GridOperator.Contains);
  protected readonly filterValue = signal('');
  protected readonly filterOptions = signal<SelectorOption[]>([]);
  protected readonly selectedFilterId = signal<Guid | null>(null);
  protected readonly showColumnSelector = signal(false);

  // Search debounce
  private searchDebounceTimer?: ReturnType<typeof setTimeout>;
  private readonly searchDebounceMs = 500;

  // Drag and drop for column reordering
  protected draggedColumnIndex: number | null = null;
  protected dragOverColumnIndex: number | null = null;
  protected readonly orderedColumns = signal<GridColumnDef<T>[]>([]);
  protected readonly visibleColumns = signal<Set<string>>(new Set());

  // Expose Math for template
  protected readonly Math = Math;

  // Computed
  protected readonly hasData = computed(() => this.data().data.length > 0);
  protected readonly totalPages = computed(() => this.data().totalPages);
  protected readonly totalCount = computed(() => this.data().totalCount);
  protected readonly columnsReordered = computed(() => {
    const original = this.columns;
    const ordered = this.orderedColumns();
    if (original.length !== ordered.length) return false;
    return !original.every((col, index) => col.field === ordered[index].field);
  });
  protected readonly displayColumns = computed(() => {
    return this.orderedColumns().filter(col => 
      this.visibleColumns().has(col.field as string)
    );
  });
  protected readonly hiddenColumnsCount = computed(() => 
    this.columns.length - this.visibleColumns().size
  );
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
  protected readonly allOperators = [
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

  // Filtered operators based on current column type
  protected readonly operators = computed(() => {
    const column = this.filterColumn();
    if (!column) return this.allOperators;

    const filterType = column.filterType || 'text';

    switch (filterType) {
      case 'select':
        // Selectors: only exact match operators
        return this.allOperators.filter(op => 
          op.value === GridOperator.Equal || 
          op.value === GridOperator.NotEqual
        );
      
      case 'number':
      case 'date':
        // Numbers and dates: comparison operators only
        return this.allOperators.filter(op => 
          op.value === GridOperator.Equal || 
          op.value === GridOperator.NotEqual ||
          op.value === GridOperator.GreaterThan ||
          op.value === GridOperator.LessThan ||
          op.value === GridOperator.GreaterThanOrEqual ||
          op.value === GridOperator.LessThanOrEqual
        );
      
      case 'text':
      default:
        // Text: all string operators
        return this.allOperators.filter(op => 
          op.value === GridOperator.Equal || 
          op.value === GridOperator.NotEqual ||
          op.value === GridOperator.Contains ||
          op.value === GridOperator.NotContains ||
          op.value === GridOperator.StartsWith ||
          op.value === GridOperator.EndsWith
        );
    }
  });

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
    this.orderedColumns.set([...this.columns]);
    
    // Initialize all columns as visible
    const visibleSet = new Set<string>();
    this.columns.forEach(col => visibleSet.add(col.field as string));
    this.visibleColumns.set(visibleSet);
  }

  setData(response: GridResponseDto<T>) {
    this.data.set(response);
  }

  // Sorting
  protected toggleSort(column: GridColumnDef<T>) {
    if (!column.sortable) return;

    const currentSorts = [...this.sorting()];
    const currentSortIndex = currentSorts.findIndex(s => s.column === column.field);

    if (currentSortIndex === -1) {
      // Column not sorted, add ascending sort to the end
      currentSorts.push({ column: column.field as string, isDescending: false });
    } else {
      const currentSort = currentSorts[currentSortIndex];
      if (!currentSort.isDescending) {
        // Change to descending
        currentSorts[currentSortIndex] = { ...currentSort, isDescending: true };
      } else {
        // Remove this sort
        currentSorts.splice(currentSortIndex, 1);
      }
    }

    this.sorting.set(currentSorts);
    this.currentPage.set(1);
  }

  protected getSortIcon(column: GridColumnDef<T>): string {
    const sortIndex = this.sorting().findIndex(s => s.column === column.field);
    if (sortIndex === -1) return '';
    
    const sort = this.sorting()[sortIndex];
    const arrow = sort.isDescending ? '↓' : '↑';
    
    // Show sort order number if multiple sorts
    if (this.sorting().length > 1) {
      return `${arrow}${sortIndex + 1}`;
    }
    
    return arrow;
  }

  protected getSortOrder(column: GridColumnDef<T>): number | null {
    const sortIndex = this.sorting().findIndex(s => s.column === column.field);
    return sortIndex === -1 ? null : sortIndex + 1;
  }

  // Filtering
  protected async openFilterDialog(column: GridColumnDef<T>) {
    if (!column.filterable) return;
    
    this.filterColumn.set(column);
    
    // Check if there's an existing filter for this column (use filterField if available)
    const filterFieldName = column.filterField || (column.field as string);
    const existingFilter = this.filters().find(f => f.column === filterFieldName);
    
    // Set filter options for select type
    if (column.filterType === 'select' && column.filterOptions) {
      this.filterOptions.set(
        typeof column.filterOptions === 'function' ? [] : column.filterOptions
      );
    } else {
      this.filterOptions.set([]);
    }
    
    // Set filter values
    if (existingFilter) {
      this.filterOperator.set(existingFilter.operator);
      this.filterValue.set(existingFilter.value || '');
      
      // For select type, set the selected ID
      if (column.filterType === 'select' && existingFilter.value) {
        this.selectedFilterId.set(existingFilter.value as Guid);
      } else {
        this.selectedFilterId.set(null);
      }
    } else {
      // Set default operator based on column type
      const defaultOperator = this.getDefaultOperator(column.filterType || 'text');
      this.filterOperator.set(defaultOperator);
      this.filterValue.set('');
      this.selectedFilterId.set(null);
    }
    
    // Show dialog after everything is ready
    this.showFilters.set(true);
  }

  protected onAutocompleteValueChange(value: Guid | null) {
    this.selectedFilterId.set(value);
  }

  private getDefaultOperator(filterType: string): GridOperator {
    switch (filterType) {
      case 'select':
      case 'number':
      case 'date':
        return GridOperator.Equal;
      case 'text':
      default:
        return GridOperator.Contains;
    }
  }

  protected applyFilter() {
    const column = this.filterColumn();
    if (!column) return;

    const filterFieldName = column.filterField || (column.field as string);
    const currentFilters = this.filters().filter(f => f.column !== filterFieldName);
    
    let filterValue: string;
    
    // For select type, use the selected ID from autocomplete
    if (column.filterType === 'select') {
      const selectedId = this.selectedFilterId();
      if (!selectedId) {
        // If no selection, don't apply filter
        this.showFilters.set(false);
        return;
      }
      filterValue = selectedId;
    } else {
      filterValue = this.filterValue();
    }
    
    if (filterValue) {
      currentFilters.push({
        column: filterFieldName,
        operator: this.filterOperator(),
        value: filterValue,
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
    const filterFieldName = column.filterField || (column.field as string);
    return this.filters().some(f => f.column === filterFieldName);
  }

  protected getFilterForColumn(column: GridColumnDef<T>): GridFilterDto | undefined {
    const filterFieldName = column.filterField || (column.field as string);
    return this.filters().find(f => f.column === filterFieldName);
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

  // Reset column order to original
  resetColumnOrder() {
    this.orderedColumns.set([...this.columns]);
  }

  // Column visibility management
  protected toggleColumnSelector() {
    this.showColumnSelector.update(v => !v);
  }

  protected closeColumnSelector() {
    this.showColumnSelector.set(false);
  }

  protected toggleColumnVisibility(column: GridColumnDef<T>) {
    const field = column.field as string;
    const visible = new Set(this.visibleColumns());
    
    if (visible.has(field)) {
      // Don't allow hiding the last column
      if (visible.size > 1) {
        visible.delete(field);
      }
    } else {
      visible.add(field);
    }
    
    this.visibleColumns.set(visible);
  }

  protected isColumnVisible(column: GridColumnDef<T>): boolean {
    return this.visibleColumns().has(column.field as string);
  }

  protected showAllColumns() {
    const visibleSet = new Set<string>();
    this.columns.forEach(col => visibleSet.add(col.field as string));
    this.visibleColumns.set(visibleSet);
  }

  // Column reordering (Drag and Drop)
  protected onDragStart(event: DragEvent, index: number) {
    if (!this.enableColumnReorder) return;
    
    this.draggedColumnIndex = index;
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/html', '');
    }
  }

  protected onDragOver(event: DragEvent, index: number) {
    if (!this.enableColumnReorder || this.draggedColumnIndex === null) return;
    
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
    this.dragOverColumnIndex = index;
  }

  protected onDragLeave() {
    this.dragOverColumnIndex = null;
  }

  protected onDrop(event: DragEvent, dropIndex: number) {
    if (!this.enableColumnReorder || this.draggedColumnIndex === null) return;
    
    event.preventDefault();
    
    const dragIndex = this.draggedColumnIndex;
    if (dragIndex !== dropIndex) {
      // Get the visible columns
      const visibleCols = this.displayColumns();
      const draggedCol = visibleCols[dragIndex];
      const targetCol = visibleCols[dropIndex];
      
      // Find their positions in orderedColumns
      const allColumns = [...this.orderedColumns()];
      const draggedOriginalIndex = allColumns.findIndex(c => c.field === draggedCol.field);
      const targetOriginalIndex = allColumns.findIndex(c => c.field === targetCol.field);
      
      // Reorder in orderedColumns
      if (draggedOriginalIndex !== -1 && targetOriginalIndex !== -1) {
        const [removed] = allColumns.splice(draggedOriginalIndex, 1);
        allColumns.splice(targetOriginalIndex, 0, removed);
        this.orderedColumns.set(allColumns);
      }
    }
    
    this.onDragEnd();
  }

  protected onDragEnd() {
    this.draggedColumnIndex = null;
    this.dragOverColumnIndex = null;
  }

  protected isDragging(index: number): boolean {
    return this.draggedColumnIndex === index;
  }

  protected isDragOver(index: number): boolean {
    return this.dragOverColumnIndex === index && this.draggedColumnIndex !== index;
  }
}
