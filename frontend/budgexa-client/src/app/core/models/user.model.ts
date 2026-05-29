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
