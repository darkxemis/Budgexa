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
  filterType?: 'text' | 'number' | 'date' | 'select';
  filterOptions?: { label: string; value: any }[];
  width?: string;
  cellTemplate?: (row: T) => string;
}

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
