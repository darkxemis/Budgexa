import { GridColumnDef } from '../../../core/models/grid.model';
import { BudgetGridDto } from '../models/budget.model';

export const BUDGETS_GRID_COLUMNS: GridColumnDef<BudgetGridDto>[] = [
  {
    field: 'number',
    header: 'budgets.grid.number',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '160px',
  },
  {
    field: 'issueDate',
    header: 'budgets.grid.issueDate',
    sortable: true,
    filterable: true,
    filterType: 'date',
    width: '140px',
    cellTemplate: (row: BudgetGridDto) =>
      row.issueDate ? new Date(row.issueDate).toLocaleDateString() : '',
  },
  {
    field: 'validUntil',
    header: 'budgets.grid.validUntil',
    sortable: true,
    filterable: true,
    filterType: 'date',
    width: '140px',
    cellTemplate: (row: BudgetGridDto) =>
      row.validUntil ? new Date(row.validUntil).toLocaleDateString() : '',
  },
  {
    field: 'customerName',
    header: 'budgets.grid.customer',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '220px',
  },
  {
    field: 'total',
    header: 'budgets.grid.total',
    sortable: true,
    filterable: true,
    filterType: 'number',
    width: '150px',
    cellTemplate: (row: BudgetGridDto) =>
      `${Number(row.total ?? 0).toFixed(2)} ${row.currency ?? ''}`.trim(),
  },
  {
    field: 'currency',
    header: 'budgets.grid.currency',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '100px',
  },
  {
    field: 'statusName',
    header: 'budgets.grid.status',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '120px',
  },
  {
    field: 'createdAt',
    header: 'budgets.grid.createdAt',
    sortable: true,
    filterable: true,
    filterType: 'date',
    width: '140px',
    cellTemplate: (row: BudgetGridDto) => new Date(row.createdAt).toLocaleDateString(),
  },
];
