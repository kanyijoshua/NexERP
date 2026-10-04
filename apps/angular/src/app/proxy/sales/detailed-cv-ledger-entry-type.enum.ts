import { mapEnumToOptions } from '@abp/ng.core';

export enum DetailedCVLedgerEntryType {
  None = 0,
  InitialEntry = 1,
  Application = 2,
  UnrealizedLoss = 3,
  UnrealizedGain = 4,
  RealizedLoss = 5,
  RealizedGain = 6,
  PaymentDiscount = 7,
  PaymentDiscountVatExcl = 8,
  PaymentDiscountVatAdjustment = 9,
  ApplnRounding = 10,
  CorrectionOfRemainingAmount = 11,
  PaymentTolerance = 12,
  PaymentDiscountTolerance = 13,
  PaymentToleranceVatExcl = 14,
  PaymentToleranceVatAdjustment = 15,
  PaymentDiscountToleranceVatExcl = 16,
  PaymentDiscountToleranceVatAdjustment = 17,
}

export const detailedCVLedgerEntryTypeOptions = mapEnumToOptions(DetailedCVLedgerEntryType);
