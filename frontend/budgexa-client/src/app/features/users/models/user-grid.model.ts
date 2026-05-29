import { Guid } from '../../../core/models/guid.model';

export interface UserGridDto {
  id: Guid;
  email: string;
  firstName: string;
  lastName: string;
  companyId: Guid;
  companyName: string;
  languageId: Guid;
  languageName: string;
  statusId: Guid;
  statusName: string;
  createdAt: Date;
  updatedAt: Date | null;
  roles: RoleInfo[];
}

export interface RoleInfo {
  id: Guid;
  name: string;
}
