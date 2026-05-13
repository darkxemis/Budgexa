export interface UserProfileResult {
  id: string; // Guid
  email: string;
  firstName: string;
  lastName: string;
  companyId: string; // Guid
  companyName: string;
  languageId: string; // Guid
  language: string; // Language code
  createdAt: Date;
  updatedAt: Date;
}

export interface UpdateCurrentUserDto {
  firstName: string;
  lastName: string;
  password?: string;
  languageId: string; // Guid
}

export interface Language {
  id: string;
  code: string;
  name: string;
}
