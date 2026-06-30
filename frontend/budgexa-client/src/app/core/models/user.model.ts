import { Guid } from './guid.model';
import { SelectorOption } from './selector.model';

export interface UserProfileResult {
  id: Guid;
  email: string;
  firstName: string;
  lastName: string;
  companyId: Guid;
  companyName: string;
  languageId: Guid;
  language: string;
  roles: string[];
  createdAt: Date;
  updatedAt: Date;
}

export interface UpdateCurrentUserDto {
  firstName: string;
  lastName: string;
  password?: string;
  languageId: Guid;
}

// Language uses the generic SelectorOption (id, name, code)
export interface Language extends SelectorOption {
  code: string;
}

export interface RoleInfo {
  id: Guid;
  name: string;
}

export interface CompanyInfo {
  id: Guid;
  name: string;
}

export interface LanguageInfo {
  id: Guid;
  name: string;
}

export interface UserDetailDto {
  id: Guid;
  email: string;
  firstName: string;
  lastName: string;
  company: CompanyInfo;
  language: LanguageInfo;
  roles: RoleInfo[];
  createdAt: Date;
  updatedAt: Date | null;
}

export interface UserCreateDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  languageId: Guid;
  roleIds: Guid[];
}

export interface UserUpdateDto {
  email: string;
  /** Empty string keeps current password (backend honours empty value). */
  password: string;
  firstName: string;
  lastName: string;
  languageId: Guid;
  roleIds: Guid[];
}
