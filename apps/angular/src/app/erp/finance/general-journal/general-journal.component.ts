import { LocalizationService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  CreateUpdateGenJournalLineDto,
  GLAccountDto,
  GLEntryDocumentType,
  GenJournalAccountType,
  GeneralPostingType,
  GenJournalBatchDto,
  GenJournalLineDto,
  GenJournalTemplateDto,
  GeneralJournalService,
  GlAccountService,
  JournalTemplateService,
  PostingPreviewDto,
  RecurringMethod,
  StandardJournalService,
} from '@proxy/finance';
import { BankAccountService } from '@proxy/cash-management';
import { EmployeeService } from '@proxy/human-resources';
import { VendorService } from '@proxy/purchasing';
import { CustomerService } from '@proxy/sales';
import { Observable, forkJoin, map } from 'rxjs';
import { DocumentLineChange, DocumentLineColumn, JournalBatch, LookupItem } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

/** Marks which list a looked-up account came from, so the line's account type can follow it. */
const ACCOUNT_KIND: Record<string, GenJournalAccountType> = {
  gl: GenJournalAccountType.GLAccount,
  customer: GenJournalAccountType.Customer,
  vendor: GenJournalAccountType.Vendor,
  bank: GenJournalAccountType.BankAccount,
  employee: GenJournalAccountType.Employee,
};

/**
 * The general journal. pick a batch, fill in the lines, watch
 * the balance, then check, preview and post.
 */
/** Line fields the journal grid does not show; a save sends them back unchanged. */
const UNSHOWN_LINE_FIELDS = [
  'postingGroup',
  'shortcutDimension1Code',
  'shortcutDimension2Code',
  'salespersPurchCode',
  'sourceCode',
  'reasonCode',
  'paymentMethodCode',
  'appliesToId',
  'bankPaymentType',
  'correction',
  'quantity',
  'dueDate',
  'onHold',
  'vatRegistrationNo',
  'countryRegionCode',
  'paymentReference',
  'messageToRecipient',
] as const satisfies readonly (keyof GenJournalLineDto)[];

function pick<T extends object, K extends keyof T>(source: T, keys: readonly K[]): Pick<T, K> {
  const result = {} as Pick<T, K>;
  for (const key of keys) {
    result[key] = source[key];
  }
  return result;
}

@Component({
  selector: 'app-general-journal',
  templateUrl: './general-journal.component.html',
  standalone: false,
})
export class GeneralJournalComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(FormBuilder);
  private readonly toaster = inject(ToasterService);
  private readonly localization = inject(LocalizationService);
  private readonly companyService = inject(CompanyService);

  private readonly journals = inject(GeneralJournalService);
  private readonly templateService = inject(JournalTemplateService);
  private readonly standardJournals = inject(StandardJournalService);
  private readonly glAccounts = inject(GlAccountService);
  private readonly customers = inject(CustomerService);
  private readonly vendors = inject(VendorService);
  private readonly bankAccounts = inject(BankAccountService);
  private readonly employees = inject(EmployeeService);

  readonly savePermission = 'Erp.Journals.Create';
  readonly postPermission = 'Erp.Journals.Post';

  templates: GenJournalTemplateDto[] = [];
  batchList: GenJournalBatchDto[] = [];
  selectedTemplateName: string | null = null;
  selectedBatchId: string | null = null;

  lines = new FormArray<FormGroup>([]);
  busy = false;
  preview: PostingPreviewDto | null = null;
  standardJournalCode = '';

  /** Line ids that came from the server, so saving knows what to update rather than create. */
  private lineIds: (string | null)[] = [];

  readonly documentTypeOptions = [
    { value: GLEntryDocumentType.None, label: 'Erp::DocumentTypeNone' },
    { value: GLEntryDocumentType.Invoice, label: 'Erp::DocumentTypeInvoice' },
    { value: GLEntryDocumentType.Payment, label: 'Erp::DocumentTypePayment' },
    { value: GLEntryDocumentType.CreditMemo, label: 'Erp::DocumentTypeCreditMemo' },
  ];

  readonly accountTypeOptions = [
    { value: GenJournalAccountType.GLAccount, label: 'Erp::GLAccount' },
    { value: GenJournalAccountType.Customer, label: 'Erp::Customer' },
    { value: GenJournalAccountType.Vendor, label: 'Erp::Vendor' },
    { value: GenJournalAccountType.BankAccount, label: 'Erp::BankAccount' },
    { value: GenJournalAccountType.Employee, label: 'Erp::Employee' },
  ];

  /** Purchase or sale decides the VAT a G/L account line carries; none carries no VAT. */
  readonly genPostingTypeOptions = [
    { value: GeneralPostingType.None, label: 'Erp::Enum:GeneralPostingType.None' },
    { value: GeneralPostingType.Purchase, label: 'Erp::Enum:GeneralPostingType.Purchase' },
    { value: GeneralPostingType.Sale, label: 'Erp::Enum:GeneralPostingType.Sale' },
  ];

  readonly recurringOptions = [
    { value: RecurringMethod.None, label: 'Erp::RecurringNone' },
    { value: RecurringMethod.Fixed, label: 'Erp::RecurringFixed' },
    { value: RecurringMethod.Variable, label: 'Erp::RecurringVariable' },
    { value: RecurringMethod.ReversingFixed, label: 'Erp::RecurringReversingFixed' },
    { value: RecurringMethod.ReversingVariable, label: 'Erp::RecurringReversingVariable' },
  ];

  get selectedBatch(): GenJournalBatchDto | undefined {
    return this.batchList.find(b => b.id === this.selectedBatchId);
  }

  get isRecurring(): boolean {
    return this.selectedBatch?.recurring ?? false;
  }

  get batches(): JournalBatch[] {
    return this.batchList.map(b => ({ id: b.id!, name: b.name ?? '' }));
  }

  /** The recurring columns only appear under a recurring template. */
  get lineColumns(): DocumentLineColumn[] {
    const columns: DocumentLineColumn[] = [
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: '140px' },
      {
        field: 'documentType',
        labelKey: 'Erp::DocumentType',
        type: 'select',
        width: '130px',
        options: this.documentTypeOptions,
      },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', type: 'text', width: '140px' },
      {
        field: 'accountType',
        labelKey: 'Erp::AccountType',
        type: 'select',
        width: '130px',
        options: this.accountTypeOptions,
      },
      {
        field: 'accountNo',
        labelKey: 'Erp::AccountNo',
        type: 'lookup',
        width: '170px',
        lookupSource: term => this.searchAccounts(term),
        lookupAllowFreeText: true,
      },
      { field: 'description', labelKey: 'Erp::Description', type: 'text' },
      {
        field: 'genPostingType',
        labelKey: 'Erp::GenPostingType',
        type: 'select',
        width: '120px',
        options: this.genPostingTypeOptions,
      },
      { field: 'vatProdPostingGroup', labelKey: 'Erp::VatProdPostingGroup', type: 'lookup', width: '130px', lookupEntity: 'vatProdPostingGroup' },
      { field: 'currencyCode', labelKey: 'Erp::CurrencyCode', type: 'lookup', width: '100px', lookupEntity: 'currency' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: '130px' },
      { field: 'vatAmount', labelKey: 'Erp::VatAmount', type: 'readonly', width: '100px' },
      { field: 'amountLcy', labelKey: 'Erp::AmountLcy', type: 'readonly', width: '120px' },
      {
        field: 'balAccountType',
        labelKey: 'Erp::BalAccountType',
        type: 'select',
        width: '130px',
        options: this.accountTypeOptions,
      },
      {
        field: 'balAccountNo',
        labelKey: 'Erp::BalancingAccountNo',
        type: 'lookup',
        width: '160px',
        lookupSource: term => this.searchAccounts(term),
        lookupAllowFreeText: true,
      },
      { field: 'appliesToDocNo', labelKey: 'Erp::AppliesToDocNo', type: 'text', width: '130px' },
    ];

    if (this.isRecurring) {
      columns.push(
        {
          field: 'recurringMethod',
          labelKey: 'Erp::RecurringMethod',
          type: 'select',
          width: '160px',
          options: this.recurringOptions,
        },
        {
          field: 'recurringFrequency',
          labelKey: 'Erp::RecurringFrequency',
          type: 'text',
          width: '110px',
        },
        { field: 'expirationDate', labelKey: 'Erp::ExpirationDate', type: 'date', width: '140px' },
      );
    }

    return columns;
  }

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  onTemplateChange(name: string): void {
    this.selectedTemplateName = name;
    this.selectedBatchId = null;
    this.loadBatches();
  }

  onBatchChange(batchId: string | null): void {
    this.selectedBatchId = batchId;
    this.preview = null;
    this.loadLines();
  }

  addLine(): void {
    this.lines.push(this.buildLine());
    this.lineIds.push(null);
  }

  removeLine(index: number): void {
    const id = this.lineIds[index];

    this.lines.removeAt(index);
    this.lineIds.splice(index, 1);

    if (id) {
      this.journals
        .deleteLine(id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.loadLines());
    }
  }

  /**
   * A looked-up account carries which list it came from, so the account type follows it. A G/L
   * account also brings its posting type and VAT groups;
   * any other account carries no VAT.
   */
  onLineChange(change: DocumentLineChange): void {
    const line = this.lines.at(change.index);
    if (!line || !change.item?.id || (change.field !== 'accountNo' && change.field !== 'balAccountNo')) {
      return;
    }

    const accountType = ACCOUNT_KIND[change.item.id];
    if (accountType === undefined) {
      return;
    }

    if (change.field === 'balAccountNo') {
      line.get('balAccountType')?.setValue(accountType);
      return;
    }

    line.get('accountType')?.setValue(accountType);
    const account = accountType === GenJournalAccountType.GLAccount ? (change.item.data as GLAccountDto | undefined) : undefined;
    line.patchValue({
      genPostingType: account?.genPostingType ?? GeneralPostingType.None,
      vatBusPostingGroup: account?.vatBusPostingGroup ?? null,
      vatProdPostingGroup: account?.genPostingType ? (account.vatProdPostingGroup ?? null) : null,
    });
  }

  save(): void {
    if (!this.selectedBatchId || this.lines.length === 0) {
      return;
    }

    if (this.lines.invalid) {
      this.lines.markAllAsTouched();
      return;
    }

    const requests = this.lines.controls.map((control, index) => {
      const input = this.toInput(control as FormGroup);
      const id = this.lineIds[index];
      return id ? this.journals.updateLine(id, input) : this.journals.createLine(input);
    });

    this.busy = true;
    forkJoin(requests)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.busy = false;
          this.toaster.success('Erp::SavedSuccessfully');
          this.loadLines();
        },
        error: () => (this.busy = false),
      });
  }

  check(): void {
    if (!this.selectedBatchId) {
      return;
    }

    this.busy = true;
    this.journals
      .check(this.selectedBatchId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: result => {
          this.busy = false;

          if (result.isValid) {
            this.toaster.success('Erp::JournalIsReadyToPost');
            return;
          }

          // Each message is an error code that already has a sentence in the localization file.
          (result.messages ?? []).forEach(message => this.toaster.warn(message));
        },
        error: () => (this.busy = false),
      });
  }

  runPreview(): void {
    if (!this.selectedBatchId) {
      return;
    }

    this.busy = true;
    this.journals
      .preview(this.selectedBatchId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: preview => {
          this.busy = false;
          this.preview = preview;
        },
        error: () => (this.busy = false),
      });
  }

  post(): void {
    if (!this.selectedBatchId) {
      return;
    }

    this.busy = true;
    this.journals
      .runPosting(this.selectedBatchId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: result => {
          this.busy = false;
          this.preview = null;
          this.toaster.success(
            this.localization.instant('Erp::PostedSuccessfully', String(result.registerNo)),
          );
          this.loadLines();
        },
        error: () => (this.busy = false),
      });
  }

  saveAsStandardJournal(): void {
    const code = this.standardJournalCode.trim();
    if (!this.selectedBatchId || !code) {
      return;
    }

    this.standardJournals
      .saveFromBatch({ batchId: this.selectedBatchId, code, description: code })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.standardJournalCode = '';
        this.toaster.success('Erp::SavedSuccessfully');
      });
  }

  private load(): void {
    this.templateService
      .getList()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.templates = result.items ?? [];
        this.selectedTemplateName = this.templates[0]?.name ?? null;
        this.loadBatches();
      });
  }

  private loadBatches(): void {
    this.journals
      .getBatches({ journalTemplateName: this.selectedTemplateName ?? undefined })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.batchList = result.items ?? [];
        this.selectedBatchId = this.batchList[0]?.id ?? null;
        this.loadLines();
      });
  }

  private loadLines(): void {
    this.lines = new FormArray<FormGroup>([]);
    this.lineIds = [];

    if (!this.selectedBatchId) {
      return;
    }

    this.journals
      .getLines(this.selectedBatchId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        const items = result.items ?? [];

        this.lines = new FormArray<FormGroup>(
          items.length ? items.map(line => this.buildLine(line)) : [this.buildLine()],
        );
        this.lineIds = items.length ? items.map(line => line.id ?? null) : [null];
      });
  }

  /** What the server holds for each line, so the fields the grid does not show survive a save. */
  private readonly lineSources = new WeakMap<FormGroup, GenJournalLineDto>();

  private buildLine(line?: GenJournalLineDto): FormGroup {
    const group = this.fb.group({
      postingDate: [
        line?.postingDate?.substring(0, 10) ?? new Date().toISOString().substring(0, 10),
        Validators.required,
      ],
      documentType: [line?.documentType ?? GLEntryDocumentType.None],
      documentNo: [line?.documentNo ?? '', Validators.maxLength(20)],
      accountType: [line?.accountType ?? GenJournalAccountType.GLAccount],
      accountNo: [line?.accountNo ?? '', [Validators.required, Validators.maxLength(20)]],
      description: [line?.description ?? '', Validators.maxLength(250)],
      amount: [line?.amount ?? 0, Validators.required],
      balAccountType: [line?.balAccountType ?? GenJournalAccountType.GLAccount],
      balAccountNo: [line?.balAccountNo ?? '', Validators.maxLength(20)],
      genPostingType: [line?.genPostingType ?? GeneralPostingType.None],
      vatBusPostingGroup: [line?.vatBusPostingGroup ?? null],
      vatProdPostingGroup: [line?.vatProdPostingGroup ?? null],
      currencyCode: [line?.currencyCode ?? null],
      appliesToDocNo: [line?.appliesToDocNo ?? '', Validators.maxLength(20)],
      // Shown only: the server works both out when the line is saved.
      vatAmount: [{ value: line?.vatAmount ?? 0, disabled: true }],
      amountLcy: [{ value: line?.amountLcy ?? 0, disabled: true }],
      recurringMethod: [line?.recurringMethod ?? RecurringMethod.None],
      recurringFrequency: [line?.recurringFrequency ?? ''],
      expirationDate: [line?.expirationDate?.substring(0, 10) ?? null],
    });

    if (line) {
      this.lineSources.set(group, line);
    }

    return group;
  }

  private toInput(form: FormGroup): CreateUpdateGenJournalLineDto {
    const value = form.getRawValue();
    const source = this.lineSources.get(form);

    return {
      // Fields the grid has no column for keep what the server holds.
      ...(source ? pick(source, UNSHOWN_LINE_FIELDS) : {}),
      genJournalBatchId: this.selectedBatchId!,
      postingDate: value.postingDate,
      documentType: value.documentType,
      documentNo: value.documentNo,
      accountType: value.accountType,
      accountNo: value.accountNo,
      description: value.description,
      amount: value.amount,
      // A blank balancing account means the line balances against the other lines instead.
      balAccountNo: value.balAccountNo || undefined,
      balAccountType: value.balAccountNo ? value.balAccountType : undefined,
      genPostingType: value.genPostingType,
      vatBusPostingGroup: value.genPostingType ? value.vatBusPostingGroup || undefined : undefined,
      vatProdPostingGroup: value.genPostingType ? value.vatProdPostingGroup || undefined : undefined,
      currencyCode: value.currencyCode || undefined,
      appliesToDocNo: value.appliesToDocNo?.trim() || undefined,
      recurringMethod: this.isRecurring ? value.recurringMethod : RecurringMethod.None,
      recurringFrequency: value.recurringFrequency || undefined,
      expirationDate: value.expirationDate || undefined,
    } as CreateUpdateGenJournalLineDto;
  }

  /**
   * Searches G/L accounts, customers, vendors, bank accounts and employees together: a journal
   * line can point at any of them, and the account type is set from whichever list the choice
   * came out of.
   */
  private searchAccounts(term: string): Observable<LookupItem[]> {
    const query = { filter: term, maxResultCount: 10, skipCount: 0 } as never;

    return forkJoin({
      gl: this.glAccounts.getList(query),
      customers: this.customers.getList(query),
      vendors: this.vendors.getList(query),
      banks: this.bankAccounts.getList(query),
      employees: this.employees.getList(query),
    }).pipe(
      map(({ gl, customers, vendors, banks, employees }) => [
        ...(gl.items ?? []).map(a => ({ id: 'gl', code: a.no ?? '', name: a.name ?? undefined, data: a })),
        ...(customers.items ?? []).map(c => ({
          id: 'customer',
          code: c.no ?? '',
          name: c.name ?? undefined,
        })),
        ...(vendors.items ?? []).map(v => ({
          id: 'vendor',
          code: v.no ?? '',
          name: v.name ?? undefined,
        })),
        ...(banks.items ?? []).map(b => ({ id: 'bank', code: b.no ?? '', name: b.name ?? undefined })),
        ...(employees.items ?? []).map(e => ({
          id: 'employee',
          code: e.no ?? '',
          name: [e.firstName, e.lastName].filter(Boolean).join(' ') || undefined,
        })),
      ]),
    );
  }
}
