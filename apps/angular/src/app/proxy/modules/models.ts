
export interface ErpModuleDto {
  code?: string;
  displayName?: string;
  description?: string;
  group?: string;
  icon?: string;
  route?: string;
  enabled: boolean;
  isCore: boolean;
  dependsOn: string[];
  blockedBy: string[];
}

export interface SetModuleEnabledInput {
  code: string;
  enabled: boolean;
}
