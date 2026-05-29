import { Guid } from './guid.model';

export interface UserProfileResult {
  id: Guid;
  email: string;
  firstName: string;
  lastName: string;
  companyId: Guid;
  companyName: string;
  languageId: Guid;
  language: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface UpdateCurrentUserDto {
  firstName: string;
  lastName: string;
  password?: string;
  languageId: Guid;
}

export interface Language {
  id: Guid;
  code: string;
  name: string;
}
