import type {
  MyProfileDto,
  ProfileDto,
  ProfileRoleAssignmentDto,
  SetMyProfileInput,
  SetProfileRoleAssignmentInput,
} from './models';
import { RestService, Rest } from '@abp/ng.core';
import type { ListResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ProfileService {
  apiName = 'Erp';


  getMy = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, MyProfileDto>({
      method: 'GET',
      url: '/api/erp/profile/my',
    },
    { apiName: this.apiName,...config });


  getProfiles = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ProfileDto>>({
      method: 'GET',
      url: '/api/erp/profile/profiles',
    },
    { apiName: this.apiName,...config });


  getRoleAssignments = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListResultDto<ProfileRoleAssignmentDto>>({
      method: 'GET',
      url: '/api/erp/profile/role-assignments',
    },
    { apiName: this.apiName,...config });


  setMy = (input: SetMyProfileInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MyProfileDto>({
      method: 'POST',
      url: '/api/erp/profile/set-my',
      body: input,
    },
    { apiName: this.apiName,...config });


  setRoleAssignment = (input: SetProfileRoleAssignmentInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/erp/profile/set-role-assignment',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
