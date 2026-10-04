import { mapEnumToOptions } from '@abp/ng.core';

export enum PensionContributionMode {
  None = 0,
  Normal = 1,
  Arrears = 2,
  TransferIn = 3,
  Gratuity = 4,
  TierII = 5,
  GroupLife = 6,
}

export const pensionContributionModeOptions = mapEnumToOptions(PensionContributionMode);
