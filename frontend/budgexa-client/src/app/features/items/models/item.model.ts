import { Guid } from '../../../core/models/guid.model';

/**
 * Item type. Must stay in sync with backend enum `Budgexa.Domain.Enums.ItemType`.
 */
export enum ItemType {
  Product = 1,
  Service = 2,
}

export interface ItemDto {
  id: Guid;
  sku: string | null;
  name: string;
  description: string | null;
  type: ItemType;
  unit: string;
  unitPrice: number;
  taxRate: number;
  currency: string;
  companyId: Guid;
  companyName: string;
  statusId: Guid;
  statusName: string;
  createdAt: Date;
  updatedAt: Date | null;
}

export interface ItemCreateDto {
  sku: string | null;
  name: string;
  description: string | null;
  type: ItemType;
  unit: string;
  unitPrice: number;
  taxRate: number;
  currency: string;
}

export interface ItemUpdateDto {
  sku: string | null;
  name: string;
  description: string | null;
  type: ItemType;
  unit: string;
  unitPrice: number;
  taxRate: number;
  currency: string;
}

export interface ItemGridDto {
  id: Guid;
  sku: string | null;
  name: string;
  description: string | null;
  type: ItemType;
  unit: string;
  unitPrice: number;
  taxRate: number;
  currency: string;
  statusId: Guid;
  statusName: string;
  createdAt: Date;
  updatedAt: Date | null;
}
