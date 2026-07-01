import { GridColumnDef } from '../../../core/models/grid.model';
import { InvoiceGridDto } from '../models/invoice.model';

export const INVOICES_GRID_COLUMNS: GridColumnDef<InvoiceGridDto>[] = [
  {
    field: 'series',
    header: 'invoices.grid.series',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '100px',
  },
  {
    field: 'number',
    header: 'invoices.grid.number',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '160px',
  },
  {
    field: 'issueDate',
    header: 'invoices.grid.issueDate',
    sortable: true,
    filterable: true,
    filterType: 'date',
    width: '140px',
    cellTemplate: (row: InvoiceGridDto) =>
      row.issueDate ? new Date(row.issueDate).toLocaleDateString() : '',
  },
  {
    field: 'dueDate',
    header: 'invoices.grid.dueDate',
    sortable: true,
    filterable: true,
    filterType: 'date',
    width: '140px',
    cellTemplate: (row: InvoiceGridDto) =>
      row.dueDate ? new Date(row.dueDate).toLocaleDateString() : '',
  },
  {
    field: 'customerName',
    header: 'invoices.grid.customer',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '220px',
  },
  {
    field: 'total',
    header: 'invoices.grid.total',
    sortable: true,
    filterable: true,
    filterType: 'number',
    width: '150px',
    cellTemplate: (row: InvoiceGridDto) =>
      `${Number(row.total ?? 0).toFixed(2)} ${row.currency ?? ''}`.trim(),
  },
  {
    field: 'amountPaid',
    header: 'invoices.grid.amountPaid',
    sortable: true,
    filterable: true,
    filterType: 'number',
    width: '140px',
    cellTemplate: (row: InvoiceGridDto) =>
      `${Number(row.amountPaid ?? 0).toFixed(2)} ${row.currency ?? ''}`.trim(),
  },
  {
    field: 'amountDue',
    header: 'invoices.grid.amountDue',
    sortable: true,
    filterable: true,
    filterType: 'number',
    width: '140px',
    cellTemplate: (row: InvoiceGridDto) =>
      `${Number(row.amountDue ?? 0).toFixed(2)} ${row.currency ?? ''}`.trim(),
  },
  {
    field: 'currency',
    header: 'invoices.grid.currency',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '100px',
  },
  {
    field: 'statusName',
    header: 'invoices.grid.status',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '120px',
  },
  {
    field: 'createdAt',
    header: 'invoices.grid.createdAt',
    sortable: true,
    filterable: true,
    filterType: 'date',
    width: '140px',
    cellTemplate: (row: InvoiceGridDto) => new Date(row.createdAt).toLocaleDateString(),
  },
];
