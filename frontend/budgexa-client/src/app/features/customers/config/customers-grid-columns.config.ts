import { GridColumnDef } from '../../../core/models/grid.model';
import { CustomerGridDto } from '../models/customer.model';

export const CUSTOMERS_GRID_COLUMNS: GridColumnDef<CustomerGridDto>[] = [
  {
    field: 'legalName',
    header: 'customers.grid.legalName',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '220px'
  },
  {
    field: 'tradeName',
    header: 'customers.grid.tradeName',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '180px'
  },
  {
    field: 'taxId',
    header: 'customers.grid.taxId',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '140px'
  },
  {
    field: 'email',
    header: 'customers.grid.email',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '220px'
  },
  {
    field: 'phone',
    header: 'customers.grid.phone',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '140px'
  },
  {
    field: 'city',
    header: 'customers.grid.city',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '140px'
  },
  {
    field: 'country',
    header: 'customers.grid.country',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '140px'
  },
  {
    field: 'statusName',
    header: 'customers.grid.status',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '120px'
  },
  {
    field: 'createdAt',
    header: 'customers.grid.createdAt',
    sortable: true,
    filterable: true,
    filterType: 'date',
    cellTemplate: (row: CustomerGridDto) => {
      return new Date(row.createdAt).toLocaleDateString();
    },
    width: '140px'
  }
];
