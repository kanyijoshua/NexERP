import { applicationMethodOptions, currencyInvoiceRoundingTypeOptions } from '@proxy/finance';
import { employeeGenderOptions } from '@proxy/human-resources';
import { RecordField, RecordSection } from '../erp-shared';
import { enumOptions } from './entity-helpers';

/** The FastTab the additional fields of a card sit on; collapsed until wanted. */
export const ADDITIONAL_SECTION: RecordSection = { key: 'additional', labelKey: 'Erp::AdditionalFields', collapsed: true };

/** Customer: the card fields beyond those the posting routines read. */
export const CUSTOMER_ADDITIONAL_FIELDS: RecordField[] = [
  { field: 'searchName', labelKey: 'Erp::SearchName', type: 'text', maxLength: 100, section: 'additional', cardOnly: true },
  { field: 'name2', labelKey: 'Erp::Name2', type: 'text', maxLength: 50, section: 'additional', cardOnly: true },
  { field: 'address2', labelKey: 'Erp::Address2', type: 'text', maxLength: 50, section: 'additional', cardOnly: true },
  { field: 'county', labelKey: 'Erp::County', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'contact', labelKey: 'Erp::Contact', type: 'text', maxLength: 100, section: 'additional', cardOnly: true },
  { field: 'mobilePhoneNo', labelKey: 'Erp::MobilePhoneNo', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'homePage', labelKey: 'Erp::HomePage', type: 'text', maxLength: 80, section: 'additional', cardOnly: true },
  { field: 'vatRegistrationNo', labelKey: 'Erp::VatRegistrationNo', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'registrationNumber', labelKey: 'Erp::RegistrationNumber', type: 'text', maxLength: 50, section: 'additional', cardOnly: true },
  { field: 'globalDimension1Code', labelKey: 'Erp::GlobalDimension1Code', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'globalDimension2Code', labelKey: 'Erp::GlobalDimension2Code', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'languageCode', labelKey: 'Erp::LanguageCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'locationCode', labelKey: 'Erp::LocationCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'shipmentMethodCode', labelKey: 'Erp::ShipmentMethodCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'responsibilityCenter', labelKey: 'Erp::ResponsibilityCenter', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'customerPriceGroup', labelKey: 'Erp::CustomerPriceGroup', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'customerDiscGroup', labelKey: 'Erp::CustomerDiscGroup', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'invoiceDiscCode', labelKey: 'Erp::InvoiceDiscCode', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'finChargeTermsCode', labelKey: 'Erp::FinChargeTermsCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'reminderTermsCode', labelKey: 'Erp::ReminderTermsCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'applicationMethod', labelKey: 'Erp::ApplicationMethod', type: 'select', options: enumOptions(applicationMethodOptions, 'ApplicationMethod'), section: 'additional', cardOnly: true },
  { field: 'pricesIncludingVat', labelKey: 'Erp::PricesIncludingVat', type: 'checkbox', section: 'additional', cardOnly: true },
  { field: 'taxAreaCode', labelKey: 'Erp::TaxAreaCode', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'taxLiable', labelKey: 'Erp::TaxLiable', type: 'checkbox', section: 'additional', cardOnly: true },
  { field: 'blockPaymentTolerance', labelKey: 'Erp::BlockPaymentTolerance', type: 'checkbox', section: 'additional', cardOnly: true },
  { field: 'prepaymentPct', labelKey: 'Erp::PrepaymentPct', type: 'number', section: 'additional', cardOnly: true },
  { field: 'printStatements', labelKey: 'Erp::PrintStatements', type: 'checkbox', section: 'additional', cardOnly: true },
  { field: 'lastStatementNo', labelKey: 'Erp::LastStatementNo', type: 'number', section: 'additional', cardOnly: true },
  { field: 'combineShipments', labelKey: 'Erp::CombineShipments', type: 'checkbox', section: 'additional', cardOnly: true },
  { field: 'preferredBankAccountCode', labelKey: 'Erp::PreferredBankAccountCode', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'primaryContactNo', labelKey: 'Erp::PrimaryContactNo', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'privacyBlocked', labelKey: 'Erp::PrivacyBlocked', type: 'checkbox', section: 'additional', cardOnly: true },
];

/** Vendor: the card fields beyond those the posting routines read. */
export const VENDOR_ADDITIONAL_FIELDS: RecordField[] = [
  { field: 'county', labelKey: 'Erp::County', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'faxNo', labelKey: 'Erp::FaxNo', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'registrationNumber', labelKey: 'Erp::RegistrationNumber', type: 'text', maxLength: 50, section: 'additional', cardOnly: true },
  { field: 'globalDimension1Code', labelKey: 'Erp::GlobalDimension1Code', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'globalDimension2Code', labelKey: 'Erp::GlobalDimension2Code', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'languageCode', labelKey: 'Erp::LanguageCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'payToVendorNo', labelKey: 'Erp::PayToVendorNo', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'priority', labelKey: 'Erp::Priority', type: 'number', section: 'additional', cardOnly: true },
  { field: 'applicationMethod', labelKey: 'Erp::ApplicationMethod', type: 'select', options: enumOptions(applicationMethodOptions, 'ApplicationMethod'), section: 'additional', cardOnly: true },
  { field: 'responsibilityCenter', labelKey: 'Erp::ResponsibilityCenter', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'preferredBankAccountCode', labelKey: 'Erp::PreferredBankAccountCode', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'primaryContactNo', labelKey: 'Erp::PrimaryContactNo', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'privacyBlocked', labelKey: 'Erp::PrivacyBlocked', type: 'checkbox', section: 'additional', cardOnly: true },
];

/** Employee: the card fields beyond those the posting routines read. */
export const EMPLOYEE_ADDITIONAL_FIELDS: RecordField[] = [
  { field: 'initials', labelKey: 'Erp::Initials', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'searchName', labelKey: 'Erp::SearchName', type: 'text', maxLength: 250, wide: true, section: 'additional', cardOnly: true },
  { field: 'address2', labelKey: 'Erp::Address2', type: 'text', maxLength: 50, section: 'additional', cardOnly: true },
  { field: 'county', labelKey: 'Erp::County', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'gender', labelKey: 'Erp::Gender', type: 'select', options: enumOptions(employeeGenderOptions, 'EmployeeGender'), section: 'additional', cardOnly: true },
  { field: 'extension', labelKey: 'Erp::Extension', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'faxNo', labelKey: 'Erp::FaxNo', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'pager', labelKey: 'Erp::Pager', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'managerNo', labelKey: 'Erp::ManagerNo', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'statisticsGroupCode', labelKey: 'Erp::StatisticsGroupCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'causeOfInactivityCode', labelKey: 'Erp::CauseOfInactivityCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'globalDimension1Code', labelKey: 'Erp::GlobalDimension1Code', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'globalDimension2Code', labelKey: 'Erp::GlobalDimension2Code', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'altAddressCode', labelKey: 'Erp::AltAddressCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'altAddressStartDate', labelKey: 'Erp::AltAddressStartDate', type: 'date', section: 'additional', cardOnly: true },
  { field: 'altAddressEndDate', labelKey: 'Erp::AltAddressEndDate', type: 'date', section: 'additional', cardOnly: true },
  { field: 'bankBranchNo', labelKey: 'Erp::BankBranchNo', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'swiftCode', labelKey: 'Erp::SwiftCode', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'currencyCode', labelKey: 'Erp::CurrencyCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'applicationMethod', labelKey: 'Erp::ApplicationMethod', type: 'select', options: enumOptions(applicationMethodOptions, 'ApplicationMethod'), section: 'additional', cardOnly: true },
  { field: 'unionMembershipNo', labelKey: 'Erp::UnionMembershipNo', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'privacyBlocked', labelKey: 'Erp::PrivacyBlocked', type: 'checkbox', section: 'additional', cardOnly: true },
];

/** Bank Account: the card fields beyond those the posting routines read. */
export const BANK_ACCOUNT_ADDITIONAL_FIELDS: RecordField[] = [
  { field: 'name2', labelKey: 'Erp::Name2', type: 'text', maxLength: 50, section: 'additional', cardOnly: true },
  { field: 'address2', labelKey: 'Erp::Address2', type: 'text', maxLength: 50, section: 'additional', cardOnly: true },
  { field: 'postCode', labelKey: 'Erp::PostCode', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'county', labelKey: 'Erp::County', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'countryRegionCode', labelKey: 'Erp::CountryRegionCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'email', labelKey: 'Erp::Email', type: 'text', maxLength: 80, section: 'additional', cardOnly: true },
  { field: 'faxNo', labelKey: 'Erp::FaxNo', type: 'text', maxLength: 30, section: 'additional', cardOnly: true },
  { field: 'homePage', labelKey: 'Erp::HomePage', type: 'text', maxLength: 80, section: 'additional', cardOnly: true },
  { field: 'globalDimension1Code', labelKey: 'Erp::GlobalDimension1Code', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'globalDimension2Code', labelKey: 'Erp::GlobalDimension2Code', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'ourContactCode', labelKey: 'Erp::OurContactCode', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'minBalance', labelKey: 'Erp::MinBalance', type: 'number', section: 'additional', cardOnly: true },
  { field: 'lastStatementNo', labelKey: 'Erp::LastStatementNo', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'balanceLastStatement', labelKey: 'Erp::BalanceLastStatement', type: 'number', section: 'additional', cardOnly: true },
  { field: 'lastPaymentStatementNo', labelKey: 'Erp::LastPaymentStatementNo', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'lastCheckNo', labelKey: 'Erp::LastCheckNo', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'transitNo', labelKey: 'Erp::TransitNo', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'bankClearingCode', labelKey: 'Erp::BankClearingCode', type: 'text', maxLength: 50, section: 'additional', cardOnly: true },
];

/** G/L Account: the card fields beyond those the posting routines read. */
export const GL_ACCOUNT_ADDITIONAL_FIELDS: RecordField[] = [
  { field: 'globalDimension1Code', labelKey: 'Erp::GlobalDimension1Code', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'globalDimension2Code', labelKey: 'Erp::GlobalDimension2Code', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
  { field: 'indentation', labelKey: 'Erp::Indentation', type: 'number', section: 'additional', cardOnly: true },
  { field: 'noOfBlankLines', labelKey: 'Erp::NoOfBlankLines', type: 'number', section: 'additional', cardOnly: true },
  { field: 'newPage', labelKey: 'Erp::NewPage', type: 'checkbox', section: 'additional', cardOnly: true },
];

/** Payment Terms: the card fields beyond those the posting routines read. */
export const PAYMENT_TERMS_ADDITIONAL_FIELDS: RecordField[] = [
  { field: 'calcPmtDiscOnCrMemos', labelKey: 'Erp::CalcPmtDiscOnCrMemos', type: 'checkbox', section: 'additional', cardOnly: true },
];

/** Payment Method: the card fields beyond those the posting routines read. */
export const PAYMENT_METHOD_ADDITIONAL_FIELDS: RecordField[] = [
  { field: 'directDebit', labelKey: 'Erp::DirectDebit', type: 'checkbox', section: 'additional', cardOnly: true },
  { field: 'directDebitPmtTermsCode', labelKey: 'Erp::DirectDebitPmtTermsCode', type: 'text', maxLength: 10, section: 'additional', cardOnly: true },
  { field: 'pmtExportLineDefinition', labelKey: 'Erp::PmtExportLineDefinition', type: 'text', maxLength: 20, section: 'additional', cardOnly: true },
];

/** Currency: the card fields beyond those the posting routines read. */
export const CURRENCY_ADDITIONAL_FIELDS: RecordField[] = [
  { field: 'isoCode', labelKey: 'Erp::IsoCode', type: 'text', maxLength: 3, section: 'additional', cardOnly: true },
  { field: 'isoNumericCode', labelKey: 'Erp::IsoNumericCode', type: 'text', maxLength: 3, section: 'additional', cardOnly: true },
  { field: 'unitAmountRoundingPrecision', labelKey: 'Erp::UnitAmountRoundingPrecision', type: 'number', section: 'additional', cardOnly: true },
  { field: 'invoiceRoundingPrecision', labelKey: 'Erp::InvoiceRoundingPrecision', type: 'number', section: 'additional', cardOnly: true },
  { field: 'invoiceRoundingType', labelKey: 'Erp::InvoiceRoundingType', type: 'select', options: enumOptions(currencyInvoiceRoundingTypeOptions, 'CurrencyInvoiceRoundingType'), section: 'additional', cardOnly: true },
  { field: 'applnRoundingPrecision', labelKey: 'Erp::ApplnRoundingPrecision', type: 'number', section: 'additional', cardOnly: true },
  { field: 'amountDecimalPlaces', labelKey: 'Erp::AmountDecimalPlaces', type: 'text', maxLength: 5, section: 'additional', cardOnly: true },
  { field: 'unitAmountDecimalPlaces', labelKey: 'Erp::UnitAmountDecimalPlaces', type: 'text', maxLength: 5, section: 'additional', cardOnly: true },
  { field: 'emuCurrency', labelKey: 'Erp::EmuCurrency', type: 'checkbox', section: 'additional', cardOnly: true },
  { field: 'paymentTolerancePct', labelKey: 'Erp::PaymentTolerancePct', type: 'number', section: 'additional', cardOnly: true },
  { field: 'maxPaymentToleranceAmount', labelKey: 'Erp::MaxPaymentToleranceAmount', type: 'number', section: 'additional', cardOnly: true },
];
