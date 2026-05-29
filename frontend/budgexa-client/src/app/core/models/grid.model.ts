import { SelectorOption } from './selector.model';

// Grid Request Models
export interface GridRequestDto {
  page: number;
  pageSize: number;
  sorting?: GridSortDto[];
  filters?: GridFilterDto[];
  search?: string;
}

export interface GridFilterDto {
  column: string;
  operator: GridOperator;
  value?: string;
}

export interface GridSortDto {
  column: string;
  isDescending: boolean;
}

// Grid Response Models
export interface GridResponseDto<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// Grid Operators (matching backend Gridify operators)
export enum GridOperator {
  Equal = '=',
  NotEqual = '!=',
  LessThan = '<',
  GreaterThan = '>',
  GreaterThanOrEqual = '>=',
  LessThanOrEqual = '<=',
  Contains = '=*',
  NotContains = '!*',
  StartsWith = '^',
  NotStartsWith = '!^',
  EndsWith = '$',
  NotEndsWith = '!$',
}

// Grid Column Configuration
export interface GridColumnDef<T = any> {
  field: keyof T | string;
  header: string;
  sortable?: boolean;
  filterable?: boolean;
  filterField?: string; // Optional: different field name for filtering (e.g., 'languageId' for 'languageName' column)
  filterType?: 'text' | 'number' | 'date' | 'select';
  filterOptions?: SelectorOption[] | ((searchQuery?: string) => Promise<SelectorOption[]>);
  width?: string;
  cellTemplate?: (row: T) => string;
}

// GridFilterOption is just an alias for SelectorOption
export type GridFilterOption = SelectorOption;

// Grid State Management
export interface GridState {
  page: number;
  pageSize: number;
  sorting: GridSortDto[];
  filters: GridFilterDto[];
  search: string;
}

// Pagination Options
export interface PaginationOptions {
  pageSizeOptions: number[];
  showFirstLastButtons: boolean;
}
