import { GridColumnDef } from '../../../core/models/grid.model';
import { SelectorOption } from '../../../core/models/selector.model';
import { ItemGridDto, ItemType } from '../models/item.model';
import { UnitMeasure } from '../../../shared/models/unit-measure.model';

/**
 * Returns the columns for the items grid.
 * @param translateType Function that converts an `ItemType` value into its localized label.
 * @param translateUnitMeasure Function that converts a `UnitMeasure` value into its localized label.
 */
export function buildItemsGridColumns(
  translateType: (type: ItemType) => string,
  translateUnitMeasure?: (um: UnitMeasure) => string
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
      field: 'unitMeasure',
      header: 'items.grid.unitMeasure',
      sortable: true,
      filterable: true,
      filterType: 'select',
      width: '130px',
      cellTemplate: (row: ItemGridDto) =>
        translateUnitMeasure ? translateUnitMeasure(row.unitMeasure) : String(row.unitMeasure),
      filterOptions: async (): Promise<SelectorOption[]> => [
        { id: String(UnitMeasure.Quantity) as never, name: translateUnitMeasure ? translateUnitMeasure(UnitMeasure.Quantity) : 'Quantity' },
        { id: String(UnitMeasure.Time) as never, name: translateUnitMeasure ? translateUnitMeasure(UnitMeasure.Time) : 'Time' },
        { id: String(UnitMeasure.Weight) as never, name: translateUnitMeasure ? translateUnitMeasure(UnitMeasure.Weight) : 'Weight' },
      ],
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
