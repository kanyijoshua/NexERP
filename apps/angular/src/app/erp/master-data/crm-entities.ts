import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import {
  ActivityTypeDto,
  ActivityTypeService,
  CampaignDto,
  CampaignService,
  CampaignTargetDto,
  CampaignTargetService,
  CloseOpportunityCodeDto,
  CloseOpportunityCodeService,
  CloseOpportunityType,
  ContactDto,
  ContactService,
  ContactType,
  CreateUpdateActivityTypeDto,
  CreateUpdateCampaignDto,
  CreateUpdateCampaignTargetDto,
  CreateUpdateCloseOpportunityCodeDto,
  CreateUpdateContactDto,
  CreateUpdateInteractionLogEntryDto,
  CreateUpdateOpportunityDto,
  CreateUpdatePipelineStageDto,
  InteractionLogEntryDto,
  InteractionLogEntryService,
  OpportunityDto,
  OpportunityKind,
  OpportunityPriority,
  OpportunityService,
  OpportunityStatus,
  PipelineStageService,
  PipelineStageSetupDto,
  closeOpportunityTypeOptions,
  contactTypeOptions,
  opportunityKindOptions,
  opportunityPriorityOptions,
  opportunityStatusOptions,
} from '@proxy/crm';
import { EMPTY, map } from 'rxjs';
import { RecordEntity } from '../erp-shared';
import { codeField, codeTableEntity, enumOptions } from './entity-helpers';

const CRM = 'Erp.Crm';
const SETUP = 'Erp.CrmSetup';
const today = () => new Date().toISOString().substring(0, 10);

/** Contacts, leads and opportunities, campaigns, interactions and the pipeline setup. */
@Injectable({ providedIn: 'root' })
export class CrmEntities {
  private readonly router = inject(Router);
  private readonly contacts = inject(ContactService);
  private readonly opportunities = inject(OpportunityService);
  private readonly closeCodes = inject(CloseOpportunityCodeService);
  private readonly activityTypes = inject(ActivityTypeService);
  private readonly stages = inject(PipelineStageService);
  private readonly campaigns = inject(CampaignService);
  private readonly targets = inject(CampaignTargetService);
  private readonly interactions = inject(InteractionLogEntryService);

  readonly contact: RecordEntity<ContactDto, CreateUpdateContactDto> = {
    key: 'contact',
    titleKey: 'Erp::Contact',
    pluralKey: 'Erp::Contacts',
    icon: 'fas fa-address-book',
    permission: CRM,
    listRoute: ['/erp/contacts'],
    chatterEntityType: 'Contact',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'name', labelKey: 'Erp::Name' },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(contactTypeOptions, 'ContactType'), width: 110 },
      { field: 'companyName', labelKey: 'Erp::CompanyName', width: 180 },
      { field: 'email', labelKey: 'Erp::Email', width: 200 },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', width: 140 },
      { field: 'customerNo', labelKey: 'Erp::CustomerNo', width: 120 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'communication', labelKey: 'Erp::Communication' },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, placeholderKey: 'Erp::NextFromSeries' },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(contactTypeOptions, 'ContactType') },
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 100 },
      codeField('companyNo', 'Erp::CompanyNo', 'contact'),
      { field: 'jobTitle', labelKey: 'Erp::JobTitle', type: 'text', maxLength: 50 },
      codeField('salespersonCode', 'Erp::SalespersonCode', 'salespersonPurchaser'),
      { field: 'email', labelKey: 'Erp::Email', type: 'email', maxLength: 80, section: 'communication' },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', maxLength: 30, section: 'communication' },
      { field: 'mobilePhoneNo', labelKey: 'Erp::MobilePhoneNo', type: 'text', maxLength: 30, section: 'communication' },
      { field: 'address', labelKey: 'Erp::Address', type: 'text', maxLength: 100, section: 'communication' },
      { field: 'city', labelKey: 'Erp::City', type: 'text', maxLength: 30, section: 'communication' },
      { field: 'countryRegionCode', labelKey: 'Erp::CountryRegionCode', type: 'text', maxLength: 10, section: 'communication' },
      { field: 'customerNo', labelKey: 'Erp::CustomerNo', type: 'readonly' },
      codeField('vendorNo', 'Erp::VendorNo', 'vendor'),
    ],
    getList: query => this.contacts.getList(query),
    get: id => this.contacts.get(id),
    create: input => this.contacts.create(input),
    update: (id, input) => this.contacts.update(id, input),
    delete: id => this.contacts.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.name ?? undefined }),
    newRecord: term => ({ type: ContactType.Company, name: term ?? '' }),
    actions: [
      {
        key: 'makeCustomer',
        labelKey: 'Erp::MakeCustomer',
        icon: 'fas fa-user-plus',
        permission: `${CRM}.Update`,
        visible: dto => !dto.customerNo,
        run: dto => this.contacts.makeCustomer(dto.id!),
      },
    ],
  };

  readonly opportunity: RecordEntity<OpportunityDto, CreateUpdateOpportunityDto> = {
    key: 'opportunity',
    titleKey: 'Erp::Opportunity',
    pluralKey: 'Erp::Opportunities',
    icon: 'fas fa-bullseye',
    permission: CRM,
    listRoute: ['/erp/opportunities'],
    chatterEntityType: 'Opportunity',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'kind', labelKey: 'Erp::Kind', type: 'select', options: enumOptions(opportunityKindOptions, 'OpportunityKind'), width: 120 },
      { field: 'contactName', labelKey: 'Erp::Contact', width: 170 },
      { field: 'stageName', labelKey: 'Erp::Stage', width: 140 },
      { field: 'expectedRevenue', labelKey: 'Erp::ExpectedRevenue', type: 'currency', width: 140 },
      { field: 'probability', labelKey: 'Erp::Probability', type: 'number', width: 110 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(opportunityStatusOptions, 'OpportunityStatus'), width: 110 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'prospect', labelKey: 'Erp::Prospect', collapsed: true },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, placeholderKey: 'Erp::NextFromSeries' },
      { field: 'kind', labelKey: 'Erp::Kind', type: 'select', options: enumOptions(opportunityKindOptions, 'OpportunityKind'), createOnly: true },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', required: true, maxLength: 250, wide: true },
      codeField('contactNo', 'Erp::ContactNo', 'contact'),
      codeField('salespersonCode', 'Erp::SalespersonCode', 'salespersonPurchaser'),
      codeField('campaignNo', 'Erp::CampaignNo', 'campaign'),
      { field: 'priority', labelKey: 'Erp::Priority', type: 'select', options: enumOptions(opportunityPriorityOptions, 'OpportunityPriority') },
      { field: 'expectedRevenue', labelKey: 'Erp::ExpectedRevenue', type: 'currency', min: 0 },
      { field: 'probability', labelKey: 'Erp::Probability', type: 'number', min: 0 },
      { field: 'expectedCloseDate', labelKey: 'Erp::ExpectedCloseDate', type: 'date' },
      { field: 'stageName', labelKey: 'Erp::Stage', type: 'readonly' },
      { field: 'salesQuoteNo', labelKey: 'Erp::SalesQuoteNo', type: 'readonly' },
      { field: 'contactCompanyName', labelKey: 'Erp::CompanyName', type: 'text', maxLength: 100, section: 'prospect' },
      { field: 'email', labelKey: 'Erp::Email', type: 'email', maxLength: 80, section: 'prospect' },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', maxLength: 30, section: 'prospect' },
    ],
    getList: query => this.opportunities.getList(query),
    get: id => this.opportunities.get(id),
    create: input => this.opportunities.create(input),
    update: (id, input) => this.opportunities.update(id, input),
    delete: id => this.opportunities.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: term => ({ kind: OpportunityKind.Lead, description: term ?? '', priority: OpportunityPriority.Normal, expectedRevenue: 0 }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: OpportunityStatus[dto.status] },
      { labelKey: 'Erp::WeightedRevenue', value: dto.weightedRevenue, type: 'currency' },
      { labelKey: 'Erp::CloseOpportunityCode', value: dto.closeOpportunityCode },
    ],
    actions: [
      {
        key: 'convert',
        labelKey: 'Erp::ConvertToOpportunity',
        icon: 'fas fa-arrow-right-arrow-left',
        permission: `${CRM}.Update`,
        visible: dto => dto.kind === OpportunityKind.Lead && dto.status === OpportunityStatus.Open,
        run: dto => this.opportunities.convert(dto.id!),
      },
      {
        key: 'quote',
        labelKey: 'Erp::MakeSalesQuote',
        icon: 'fas fa-file-lines',
        permission: `${CRM}.Update`,
        visible: dto => dto.kind === OpportunityKind.Opportunity && !dto.salesQuoteNo && !!dto.contactNo,
        run: dto => this.opportunities.makeSalesQuote(dto.id!),
      },
      {
        key: 'win',
        labelKey: 'Erp::MarkWon',
        icon: 'fas fa-trophy',
        permission: `${CRM}.Update`,
        visible: dto => dto.status === OpportunityStatus.Open,
        run: dto => this.opportunities.win(dto.id!, {}),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-rotate-left',
        permission: `${CRM}.Update`,
        visible: dto => dto.status !== OpportunityStatus.Open,
        run: dto => this.opportunities.reopen(dto.id!),
      },
      {
        key: 'pipeline',
        labelKey: 'Erp::Pipeline',
        icon: 'fas fa-table-columns',
        run: () => {
          void this.router.navigate(['/erp/crm/pipeline']);
          return EMPTY;
        },
      },
    ],
  };

  readonly closeOpportunityCode = codeTableEntity<CloseOpportunityCodeDto, CreateUpdateCloseOpportunityCodeDto>(this.closeCodes, {
    key: 'closeOpportunityCode',
    titleKey: 'Erp::CloseOpportunityCode',
    pluralKey: 'Erp::CloseOpportunityCodes',
    icon: 'fas fa-flag-checkered',
    permission: SETUP,
    route: '/erp/close-opportunity-codes',
    columns: [{ field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(closeOpportunityTypeOptions, 'CloseOpportunityType'), width: 110 }],
    fields: [{ field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(closeOpportunityTypeOptions, 'CloseOpportunityType') }],
    defaults: { type: CloseOpportunityType.Lost },
  });

  readonly activityType = codeTableEntity<ActivityTypeDto, CreateUpdateActivityTypeDto>(this.activityTypes, {
    key: 'activityType',
    titleKey: 'Erp::ActivityType',
    pluralKey: 'Erp::ActivityTypes',
    icon: 'fas fa-list-check',
    permission: SETUP,
    route: '/erp/activity-types',
    columns: [{ field: 'icon', labelKey: 'Erp::Icon', width: 160 }],
    fields: [{ field: 'icon', labelKey: 'Erp::Icon', type: 'text', maxLength: 50 }],
  });

  readonly pipelineStage: RecordEntity<PipelineStageSetupDto, CreateUpdatePipelineStageDto> = {
    key: 'pipelineStage',
    titleKey: 'Erp::PipelineStage',
    pluralKey: 'Erp::PipelineStages',
    icon: 'fas fa-table-columns',
    permission: SETUP,
    listRoute: ['/erp/pipeline-stages'],
    columns: [
      { field: 'pipelineType', labelKey: 'Erp::Pipeline', width: 130 },
      { field: 'sequence', labelKey: 'Erp::Sequence', type: 'number', width: 100 },
      { field: 'name', labelKey: 'Erp::Name' },
      { field: 'defaultProbability', labelKey: 'Erp::Probability', type: 'number', width: 110 },
      { field: 'isWonStage', labelKey: 'Erp::IsWonStage', type: 'boolean', width: 100 },
      { field: 'isLostStage', labelKey: 'Erp::IsLostStage', type: 'boolean', width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      {
        field: 'pipelineType',
        labelKey: 'Erp::Pipeline',
        type: 'select',
        options: [
          { value: 'Lead', label: 'Erp::Enum:OpportunityKind.Lead' },
          { value: 'Opportunity', label: 'Erp::Enum:OpportunityKind.Opportunity' },
        ],
      },
      { field: 'sequence', labelKey: 'Erp::Sequence', type: 'number', min: 0 },
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 100 },
      { field: 'defaultProbability', labelKey: 'Erp::Probability', type: 'number', min: 0 },
      { field: 'foldedInKanban', labelKey: 'Erp::FoldedInKanban', type: 'checkbox' },
      { field: 'isWonStage', labelKey: 'Erp::IsWonStage', type: 'checkbox' },
      { field: 'isLostStage', labelKey: 'Erp::IsLostStage', type: 'checkbox' },
    ],
    getList: query => this.stages.getList(query),
    get: id => this.stages.get(id),
    create: input => this.stages.create(input),
    update: (id, input) => this.stages.update(id, input),
    delete: id => this.stages.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.name ?? '', name: dto.pipelineType ?? undefined }),
    newRecord: () => ({ pipelineType: 'Opportunity', sequence: 10, defaultProbability: 0, foldedInKanban: false, isWonStage: false, isLostStage: false }),
  };

  readonly campaign: RecordEntity<CampaignDto, CreateUpdateCampaignDto> = {
    key: 'campaign',
    titleKey: 'Erp::Campaign',
    pluralKey: 'Erp::Campaigns',
    icon: 'fas fa-bullhorn',
    permission: CRM,
    listRoute: ['/erp/campaigns'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', width: 120 },
      { field: 'endingDate', labelKey: 'Erp::EndingDate', type: 'date', width: 120 },
      { field: 'budget', labelKey: 'Erp::Budget', type: 'currency', width: 130 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, createOnly: true, placeholderKey: 'Erp::NextFromSeries' },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date' },
      { field: 'endingDate', labelKey: 'Erp::EndingDate', type: 'date' },
      codeField('salespersonCode', 'Erp::SalespersonCode', 'salespersonPurchaser'),
      { field: 'budget', labelKey: 'Erp::Budget', type: 'currency', min: 0 },
    ],
    getList: query => this.campaigns.getList(query),
    get: id => this.campaigns.get(id),
    create: input => this.campaigns.create(input),
    update: (id, input) => this.campaigns.update(id, input),
    delete: id => this.campaigns.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ budget: 0, startingDate: today() }),
    parts: [
      {
        entity: 'campaignTarget',
        lines: dto => this.targets.getList({ campaignNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ campaignNo: dto.no }),
        columns: ['contactNo', 'contactName'],
      },
    ],
  };

  readonly campaignTarget: RecordEntity<CampaignTargetDto, CreateUpdateCampaignTargetDto> = {
    key: 'campaignTarget',
    titleKey: 'Erp::CampaignTarget',
    pluralKey: 'Erp::CampaignTargets',
    icon: 'fas fa-crosshairs',
    permission: CRM,
    listRoute: ['/erp/campaigns'],
    columns: [
      { field: 'contactNo', labelKey: 'Erp::ContactNo', width: 130 },
      { field: 'contactName', labelKey: 'Erp::ContactName' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [codeField('contactNo', 'Erp::ContactNo', 'contact', 'general', { required: true }), { field: 'contactName', labelKey: 'Erp::ContactName', type: 'readonly' }],
    getList: query => this.targets.getList(query),
    get: id => this.targets.get(id),
    create: input => this.targets.create(input),
    update: (id, input) => this.targets.update(id, input),
    delete: id => this.targets.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.contactNo ?? '', name: dto.contactName ?? undefined }),
    newRecord: () => ({}),
  };

  readonly interactionLogEntry: RecordEntity<InteractionLogEntryDto, CreateUpdateInteractionLogEntryDto> = {
    key: 'interactionLogEntry',
    titleKey: 'Erp::InteractionLogEntry',
    pluralKey: 'Erp::InteractionLogEntries',
    icon: 'fas fa-comments',
    permission: CRM,
    listRoute: ['/erp/interaction-log-entries'],
    columns: [
      { field: 'date', labelKey: 'Erp::Date', type: 'date', width: 120 },
      { field: 'contactNo', labelKey: 'Erp::ContactNo', width: 120 },
      { field: 'contactName', labelKey: 'Erp::ContactName', width: 170 },
      { field: 'activityTypeCode', labelKey: 'Erp::ActivityType', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'opportunityNo', labelKey: 'Erp::OpportunityNo', width: 130 },
      { field: 'durationMinutes', labelKey: 'Erp::DurationMinutes', type: 'number', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'date', labelKey: 'Erp::Date', type: 'date', required: true, createOnly: true },
      codeField('contactNo', 'Erp::ContactNo', 'contact', 'general', { required: true, createOnly: true }),
      codeField('activityTypeCode', 'Erp::ActivityType', 'activityType', 'general', { createOnly: true }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', required: true, maxLength: 250, wide: true, createOnly: true },
      codeField('opportunityNo', 'Erp::OpportunityNo', 'opportunity'),
      codeField('campaignNo', 'Erp::CampaignNo', 'campaign'),
      { field: 'durationMinutes', labelKey: 'Erp::DurationMinutes', type: 'number', min: 0 },
      { field: 'cost', labelKey: 'Erp::Cost', type: 'currency', min: 0 },
      codeField('salespersonCode', 'Erp::SalespersonCode', 'salespersonPurchaser'),
    ],
    getList: query => this.interactions.getList(query),
    get: id => this.interactions.get(id),
    create: input => this.interactions.create(input),
    update: (id, input) => this.interactions.update(id, input),
    delete: id => this.interactions.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.contactNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ date: today(), activityTypeCode: 'CALL', durationMinutes: 0, cost: 0 }),
  };

  get all(): RecordEntity[] {
    return [
      this.contact,
      this.opportunity,
      this.closeOpportunityCode,
      this.activityType,
      this.pipelineStage,
      this.campaign,
      this.campaignTarget,
      this.interactionLogEntry,
    ];
  }
}
