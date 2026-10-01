import type { ProfileSource } from './profile-source.enum';

export interface MyProfileDto {
  profileId?: string;
  displayName?: string;
  source: ProfileSource;
  roleName?: string;
  navigation: RoleCenterNavItemDto[];
  profiles: ProfileDto[];
}

export interface ProfileDto {
  id?: string;
  displayName?: string;
  description?: string;
}

export interface ProfileRoleAssignmentDto {
  roleName?: string;
  profileId?: string;
}

export interface RoleCenterNavItemDto {
  key?: string;
  displayName?: string;
  route?: string;
  children: RoleCenterNavItemDto[];
}

export interface SetMyProfileInput {
  profileId?: string | null;
}

export interface SetProfileRoleAssignmentInput {
  roleName: string;
  profileId?: string | null;
}
