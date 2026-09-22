import type { ActivityCueTone } from './activity-cue-tone.enum';
import type { ErpModuleDto } from '../modules/models';

export interface ActivityCueDto {
  key?: string;
  displayName?: string;
  value: number;
  isAmount: boolean;
  tone: ActivityCueTone;
  icon?: string;
  route?: string;
}

export interface HomeSummaryDto {
  companyName?: string;
  userName?: string;
  cues: ActivityCueDto[];
  apps: ErpModuleDto[];
}
