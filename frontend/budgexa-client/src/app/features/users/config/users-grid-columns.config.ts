import { GridColumnDef } from '../../../core/models/grid.model';
import { UserGridDto } from '../models/user-grid.model';

export const USERS_GRID_COLUMNS: GridColumnDef<UserGridDto>[] = [
  {
    field: 'email',
    header: 'users.grid.email',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '200px'
  },
  {
    field: 'firstName',
    header: 'users.grid.firstName',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '150px'
  },
  {
    field: 'lastName',
    header: 'users.grid.lastName',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '150px'
  },
  {
    field: 'companyName',
    header: 'users.grid.company',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '180px'
  },
  {
    field: 'languageName',
    header: 'users.grid.language',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '120px'
  },
  {
    field: 'statusName',
    header: 'users.grid.status',
    sortable: true,
    filterable: true,
    filterType: 'text',
    width: '120px'
  },
  {
    field: 'roles',
    header: 'users.grid.roles',
    sortable: false,
    filterable: false,
    cellTemplate: (row: UserGridDto) => {
      return row.roles.map(r => r.name).join(', ');
    },
    width: '180px'
  },
  {
    field: 'createdAt',
    header: 'users.grid.createdAt',
    sortable: true,
    filterable: true,
    filterType: 'date',
    cellTemplate: (row: UserGridDto) => {
      return new Date(row.createdAt).toLocaleDateString();
    },
    width: '130px'
  }
];
