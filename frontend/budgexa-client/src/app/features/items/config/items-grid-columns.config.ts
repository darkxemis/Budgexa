import { GridColumnDef } from '../../../core/models/grid.model';
import { ItemGridDto, ItemType } from '../models/item.model';

/**
 * Returns the columns for the items grid.
 * @param translateType Function that converts an `ItemType` value into its localized label.
 */
export function buildItemsGridColumns(
  translateType: (type: ItemType) => string
): GridColumnDef<ItemGridDto>[] {
  return [
    {
      field: 'sku',
      header: 'items.grid.sku',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '140px',
    },
    {
      field: 'name',
      header: 'items.grid.name',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '220px',
    },
    {
      field: 'type',
      header: 'items.grid.type',
      sortable: true,
      filterable: true,
      filterType: 'select',
      width: '130px',
      cellTemplate: (row: ItemGridDto) => translateType(row.type),
    },
    {
      field: 'unit',
      header: 'items.grid.unit',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '110px',
    },
    {
      field: 'unitPrice',
      header: 'items.grid.unitPrice',
      sortable: true,
      filterable: true,
      filterType: 'number',
      width: '140px',
      cellTemplate: (row: ItemGridDto) =>
        `${row.unitPrice?.toFixed(2) ?? '0.00'} ${row.currency ?? ''}`.trim(),
    },
    {
      field: 'taxRate',
      header: 'items.grid.taxRate',
      sortable: true,
      filterable: true,
      filterType: 'number',
      width: '110px',
      cellTemplate: (row: ItemGridDto) => `${row.taxRate ?? 0}%`,
    },
    {
      field: 'currency',
      header: 'items.grid.currency',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '100px',
    },
    {
      field: 'statusName',
      header: 'items.grid.status',
      sortable: true,
      filterable: true,
      filterType: 'text',
      width: '120px',
    },
    {
      field: 'createdAt',
      header: 'items.grid.createdAt',
      sortable: true,
      filterable: true,
      filterType: 'date',
      width: '140px',
      cellTemplate: (row: ItemGridDto) => new Date(row.createdAt).toLocaleDateString(),
    },
  ];
}
