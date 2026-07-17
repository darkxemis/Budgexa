import { Guid } from '../../../core/models/guid.model';

export interface CustomerDto {
  id: Guid;
  legalName: string;
  tradeName: string | null;
  taxId: string;
  email: string | null;
  phone: string | null;
  addressLine: string | null;
  city: string | null;
  postalCode: string | null;
  province: string | null;
  country: string | null;
  notes: string | null;
  companyId: Guid;
  companyName: string;
  statusId: Guid;
  statusName: string;
  createdAt: Date;
  updatedAt: Date | null;
}

export interface CustomerCreateDto {
  legalName: string;
  tradeName: string | null;
  taxId: string;
  email: string | null;
  phone: string | null;
  addressLine: string | null;
  city: string | null;
  postalCode: string | null;
  province: string | null;
  country: string | null;
  notes: string | null;
}

export interface CustomerUpdateDto {
  legalName: string;
  tradeName: string | null;
  taxId: string;
  email: string | null;
  phone: string | null;
  addressLine: string | null;
  city: string | null;
  postalCode: string | null;
  province: string | null;
  country: string | null;
  notes: string | null;
}

export interface CustomerGridDto {
  id: Guid;
  legalName: string;
  tradeName: string | null;
  taxId: string;
  email: string | null;
  phone: string | null;
  city: string | null;
  country: string | null;
  statusId: Guid;
  statusName: string;
  createdAt: Date;
  updatedAt: Date | null;
}
