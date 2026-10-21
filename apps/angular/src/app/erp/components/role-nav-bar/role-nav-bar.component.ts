import { ABP, CoreModule, LocalizationService, PermissionService, RoutesService, TreeNode } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import {
  AfterViewInit,
  Component,
  DestroyRef,
  ElementRef,
  OnInit,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { IsActiveMatchOptions, NavigationEnd, Router, RouterModule } from '@angular/router';
import { filter } from 'rxjs';
import { ProfileSource, RoleCenterNavItemDto } from '@proxy/profiles';
import { RoleNavigationService } from '../../services/role-navigation.service';
import { CompanyMenuComponent } from '../company-menu/company-menu.component';
import { MenuSuiteService } from '../menu-suite/menu-suite.service';

/** A link is current when the page shown is it or one of its cards (`/erp/customers/…`). */
const ACTIVE_MATCH: IsActiveMatchOptions = {
  paths: 'subset',
  queryParams: 'ignored',
  fragment: 'ignored',
  matrixParams: 'ignored',
};

interface AreaBlueprintSubMenu {
  key: string;
  nameKey: string;
  routes: string[];
}

interface AreaBlueprint {
  key: string;
  nameKey: string;
  directRoutes: string[];
  subMenus: AreaBlueprintSubMenu[];
}

/**
 * Business Central-style area layouts: direct primary sublinks followed by grouped dropdown sub-menus.
 * Used to enrich flat navigation menus and to provide sublinks when clicking top-level direct links.
 */
const BC_AREA_BLUEPRINTS: AreaBlueprint[] = [
  {
    key: 'finance',
    nameKey: 'Erp::Menu:Finance',
    directRoutes: [
      '/erp/chart-of-accounts',
      '/erp/finance/general-journal',
      '/erp/finance/registers',
      '/erp/finance/vat-entries',
      '/erp/finance/accounting-periods',
      '/erp/currencies',
      '/erp/payment-terms',
      '/erp/setup/dimensions',
      '/erp/setup/general-ledger',
    ],
    subMenus: [
      {
        key: 'finance-budgets',
        nameKey: 'Erp::Menu:Budgets',
        routes: ['/erp/gl-budget-names', '/erp/finance/budget-matrix', '/erp/gl-budget-entries'],
      },
      {
        key: 'finance-cash-flow',
        nameKey: 'Erp::Menu:CashFlow',
        routes: [
          '/erp/cash-flow-forecasts',
          '/erp/cash-flow-worksheet',
          '/erp/cash-flow-manual-entries',
          '/erp/cash-flow-forecast-entries',
        ],
      },
      {
        key: 'finance-periodic',
        nameKey: 'Erp::Menu:PeriodicActivities',
        routes: [
          '/erp/finance/vat-return',
          '/erp/finance/vat-settlement',
          '/erp/finance/exch-rate-adjustment',
          '/erp/deferral-templates',
          '/erp/posted-deferrals',
        ],
      },
      {
        key: 'finance-collections',
        nameKey: 'Erp::Menu:Collections',
        routes: ['/erp/reminders', '/erp/finance-charge-memos', '/erp/reminder-terms', '/erp/finance-charge-terms'],
      },
      {
        key: 'finance-fixed-assets',
        nameKey: 'Erp::Menu:FixedAssets',
        routes: [
          '/erp/fixed-assets',
          '/erp/fa-journals',
          '/erp/fa-ledger-entries',
          '/erp/fa-depreciation-books',
          '/erp/depreciation-books',
          '/erp/fa-classes',
          '/erp/fa-subclasses',
          '/erp/fa-locations',
          '/erp/fa-posting-groups',
          '/erp/maintenances',
          '/erp/maintenance-registrations',
          '/erp/main-asset-components',
          '/erp/setup/fa-setups',
        ],
      },
      {
        key: 'finance-setup',
        nameKey: 'Erp::Menu:FinanceSetup',
        routes: [
          '/erp/setup/general-ledger',
          '/erp/setup/dimensions',
          '/erp/currencies',
          '/erp/currency-exchange-rates',
          '/erp/payment-terms',
          '/erp/payment-methods',
          '/erp/source-codes',
          '/erp/reason-codes',
          '/erp/countries-regions',
          '/erp/post-codes',
          '/erp/shipment-methods',
          '/erp/responsibility-centers',
        ],
      },
      {
        key: 'posting-setup',
        nameKey: 'Erp::Menu:PostingSetup',
        routes: [
          '/erp/general-posting-setup',
          '/erp/gen-bus-posting-groups',
          '/erp/gen-prod-posting-groups',
          '/erp/vat-posting-setup',
          '/erp/vat-bus-posting-groups',
          '/erp/vat-prod-posting-groups',
          '/erp/customer-posting-groups',
          '/erp/vendor-posting-groups',
          '/erp/inventory-posting-groups',
          '/erp/inventory-posting-setup',
          '/erp/bank-account-posting-groups',
        ],
      },
    ],
  },
  {
    key: 'journals',
    nameKey: 'Erp::Menu:Journals',
    directRoutes: [
      '/erp/finance/general-journal',
      '/erp/item-journals',
      '/erp/fa-journals',
      '/erp/project-journals',
      '/erp/finance/journal-templates',
      '/erp/item-journal-templates',
      '/erp/finance/registers',
    ],
    subMenus: [],
  },
  {
    key: 'cash-management',
    nameKey: 'Erp::Menu:CashManagement',
    directRoutes: [
      '/erp/bank-accounts',
      '/erp/payment-vouchers',
      '/erp/bank-acc-reconciliations',
      '/erp/bank-account-statements',
      '/erp/finance/bank-ledger-entries',
      '/erp/check-ledger-entries',
    ],
    subMenus: [
      {
        key: 'cash-flow',
        nameKey: 'Erp::Menu:CashFlow',
        routes: [
          '/erp/cash-flow-forecasts',
          '/erp/cash-flow-worksheet',
          '/erp/cash-flow-manual-entries',
          '/erp/cash-flow-forecast-entries',
        ],
      },
      {
        key: 'cash-management-setup',
        nameKey: 'Erp::Menu:Setup',
        routes: [
          '/erp/payment-methods',
          '/erp/payment-types',
          '/erp/payment-deduction-codes',
          '/erp/bank-account-posting-groups',
          '/erp/customer-bank-accounts',
          '/erp/vendor-bank-accounts',
          '/erp/setup/cash-management',
        ],
      },
    ],
  },
  {
    key: 'sales',
    nameKey: 'Erp::Menu:Sales',
    directRoutes: [
      '/erp/customers',
      '/erp/sales-quotes',
      '/erp/sales-orders',
      '/erp/sales-invoices',
      '/erp/sales-credit-memos',
      '/erp/sales-blanket-orders',
      '/erp/sales-return-orders',
    ],
    subMenus: [
      {
        key: 'sales-posted',
        nameKey: 'Erp::Menu:SalesDocuments',
        routes: ['/erp/posted-sales-shipments', '/erp/posted-sales-invoices', '/erp/finance/customer-ledger-entries'],
      },
      {
        key: 'sales-pricing',
        nameKey: 'Erp::Menu:PriceLists',
        routes: [
          '/erp/price-lists',
          '/erp/customer-price-groups',
          '/erp/salespeople-purchasers',
          '/erp/customer-bank-accounts',
          '/erp/customer-posting-groups',
        ],
      },
      {
        key: 'sales-crm',
        nameKey: 'Erp::Menu:Crm',
        routes: [
          '/erp/crm/pipeline',
          '/erp/opportunities',
          '/erp/contacts',
          '/erp/campaigns',
          '/erp/interaction-log-entries',
          '/erp/crm/my-activities',
          '/erp/pipeline-stages',
          '/erp/close-opportunity-codes',
          '/erp/activity-types',
        ],
      },
      {
        key: 'sales-collections',
        nameKey: 'Erp::Menu:Collections',
        routes: ['/erp/reminders', '/erp/finance-charge-memos', '/erp/reminder-terms', '/erp/finance-charge-terms'],
      },
    ],
  },
  {
    key: 'purchasing',
    nameKey: 'Erp::Menu:Purchasing',
    directRoutes: [
      '/erp/vendors',
      '/erp/purchase-quotes',
      '/erp/purchase-orders',
      '/erp/purchase-invoices',
      '/erp/purchase-credit-memos',
      '/erp/purchase-blanket-orders',
      '/erp/purchase-return-orders',
    ],
    subMenus: [
      {
        key: 'purchasing-procurement',
        nameKey: 'Erp::Menu:Procurement',
        routes: ['/erp/purchase-requisitions', '/erp/procurement-plans', '/erp/tenders'],
      },
      {
        key: 'purchasing-posted',
        nameKey: 'Erp::Menu:PurchaseDocuments',
        routes: [
          '/erp/posted-purchase-receipts',
          '/erp/posted-purchase-invoices',
          '/erp/finance/vendor-ledger-entries',
        ],
      },
      {
        key: 'purchasing-setup',
        nameKey: 'Erp::Menu:PurchasingSetup',
        routes: [
          '/erp/item-vendors',
          '/erp/order-addresses',
          '/erp/vendor-bank-accounts',
          '/erp/vendor-posting-groups',
          '/erp/salespeople-purchasers',
        ],
      },
    ],
  },
  {
    key: 'inventory',
    nameKey: 'Erp::Menu:Inventory',
    directRoutes: [
      '/erp/items',
      '/erp/item-categories',
      '/erp/units-of-measure',
      '/erp/locations',
      '/erp/item-journals',
      '/erp/transfer-orders',
      '/erp/item-ledger-entries',
    ],
    subMenus: [
      {
        key: 'inventory-warehouse',
        nameKey: 'Erp::Menu:Warehouse',
        routes: [
          '/erp/posted-transfers',
          '/erp/item-variants',
          '/erp/item-tracking-codes',
          '/erp/lot-no-information',
          '/erp/serial-no-information',
          '/erp/item-journal-templates',
        ],
      },
      {
        key: 'inventory-manufacturing',
        nameKey: 'Erp::Menu:Manufacturing',
        routes: [
          '/erp/production-orders',
          '/erp/planning-worksheet',
          '/erp/production-boms',
          '/erp/routings',
          '/erp/work-centers',
          '/erp/machine-centers',
          '/erp/capacity-ledger-entries',
          '/erp/setup/manufacturing-setup',
        ],
      },
      {
        key: 'inventory-setup',
        nameKey: 'Erp::Menu:InventorySetup',
        routes: [
          '/erp/setup/inventory',
          '/erp/inventory-posting-groups',
          '/erp/inventory-posting-setup',
          '/erp/price-lists',
          '/erp/customer-price-groups',
        ],
      },
    ],
  },
  {
    key: 'projects',
    nameKey: 'Erp::Menu:Projects',
    directRoutes: [
      '/erp/projects',
      '/erp/time-sheets',
      '/erp/project-journals',
      '/erp/resources',
      '/erp/project-ledger-entries',
      '/erp/resource-ledger-entries',
      '/erp/project-posting-groups',
    ],
    subMenus: [
      {
        key: 'projects-service',
        nameKey: 'Erp::Menu:Service',
        routes: [
          '/erp/service/board',
          '/erp/service-orders',
          '/erp/service-contracts',
          '/erp/service-items',
          '/erp/service-ledger-entries',
          '/erp/warranty-ledger-entries',
          '/erp/service-order-types',
          '/erp/fault-codes',
          '/erp/resolution-codes',
          '/erp/setup/service-setup',
        ],
      },
    ],
  },
  {
    key: 'human-resources',
    nameKey: 'Erp::Menu:HumanResources',
    directRoutes: [
      '/erp/employees',
      '/erp/employee-absences',
      '/erp/finance/employee-ledger-entries',
      '/erp/employment-contracts',
      '/erp/qualifications',
    ],
    subMenus: [
      {
        key: 'hr-payroll',
        nameKey: 'Erp::Menu:Payroll',
        routes: [
          '/erp/payroll-runs',
          '/erp/payslips',
          '/erp/employee-pay-items',
          '/erp/payroll-earnings',
          '/erp/payroll-deductions',
          '/erp/payroll-tax-bands',
          '/erp/setup/payroll',
        ],
      },
      {
        key: 'hr-setup',
        nameKey: 'Erp::Menu:HumanResourcesSetup',
        routes: [
          '/erp/causes-of-absence',
          '/erp/unions',
          '/erp/grounds-for-termination',
          '/erp/hr-units-of-measure',
          '/erp/employee-posting-groups',
          '/erp/employee-statistics-groups',
          '/erp/relatives',
          '/erp/employee-relatives',
          '/erp/employee-qualifications',
          '/erp/misc-articles',
          '/erp/misc-article-informations',
          '/erp/confidentials',
          '/erp/confidential-informations',
          '/erp/alternative-addresses',
          '/erp/setup/human-resources',
        ],
      },
    ],
  },
  {
    key: 'pensions',
    nameKey: 'Erp::Menu:Pensions',
    directRoutes: [
      '/erp/pension-members',
      '/erp/pension-sponsors',
      '/erp/pension-contributions',
      '/erp/member-exits',
      '/erp/member-ledger-entries',
      '/erp/pensioners',
      '/erp/pension-payrolls',
      '/erp/pension-benefit-calculations',
      '/erp/pension-schemes',
      '/erp/pension-interest-rates',
      '/erp/exit-reasons',
      '/erp/setup/pension-setup',
    ],
    subMenus: [],
  },
  {
    key: 'academics',
    nameKey: 'Erp::Menu:Academics',
    directRoutes: [
      '/erp/students',
      '/erp/student-applications',
      '/erp/semester-registrations',
      '/erp/student-units',
      '/erp/student-bills',
      '/erp/student-receipts',
      '/erp/exam-results',
    ],
    subMenus: [
      {
        key: 'academics-campus',
        nameKey: 'Erp::Menu:CampusServices',
        routes: ['/erp/timetable', '/erp/attendance-registers', '/erp/hostel-allocations', '/erp/student-refunds'],
      },
      {
        key: 'academics-config',
        nameKey: 'Erp::Menu:AcademicConfiguration',
        routes: [
          '/erp/programmes',
          '/erp/course-units',
          '/erp/academic-years',
          '/erp/semesters',
          '/erp/fee-structure',
          '/erp/setup/academic-setup',
        ],
      },
    ],
  },
  {
    key: 'approvals',
    nameKey: 'Erp::Module:Approvals',
    directRoutes: [
      '/erp/approvals',
      '/erp/setup/workflows',
      '/erp/setup/approval-users',
      '/erp/workflow-user-groups',
      '/erp/workflow-user-group-members',
    ],
    subMenus: [],
  },
  {
    key: 'reports',
    nameKey: 'Erp::Menu:Reports',
    directRoutes: [
      '/erp/reports/financial',
      '/erp/reports/standard',
      '/erp/reports/account-schedules',
      '/erp/reports/column-layouts',
      '/erp/reports/report-layouts',
      '/erp/report-selections',
      '/erp/custom-report-selections',
    ],
    subMenus: [],
  },
  {
    key: 'setup',
    nameKey: 'Erp::Menu:Setup',
    directRoutes: [
      '/erp/setup/general-ledger',
      '/erp/setup/dimensions',
      '/erp/setup/inventory',
      '/erp/setup/human-resources',
      '/erp/setup/no-series',
      '/erp/user-setups',
      '/erp/setup/job-queue',
      '/erp/setup/profiles',
      '/erp/setup/modules',
      '/erp/setup/theme',
    ],
    subMenus: [
      {
        key: 'setup-approvals',
        nameKey: 'Erp::Module:Approvals',
        routes: [
          '/erp/setup/workflows',
          '/erp/setup/approval-users',
          '/erp/workflow-user-groups',
          '/erp/workflow-user-group-members',
        ],
      },
      {
        key: 'setup-posting',
        nameKey: 'Erp::Menu:PostingSetup',
        routes: [
          '/erp/general-posting-setup',
          '/erp/gen-bus-posting-groups',
          '/erp/gen-prod-posting-groups',
          '/erp/vat-posting-setup',
          '/erp/customer-posting-groups',
          '/erp/vendor-posting-groups',
          '/erp/inventory-posting-groups',
          '/erp/inventory-posting-setup',
          '/erp/bank-account-posting-groups',
        ],
      },
      {
        key: 'setup-integration',
        nameKey: 'Erp::Menu:Integration',
        routes: [
          '/erp/setup/web-services',
          '/erp/setup/webhooks',
          '/erp/setup/data-export',
          '/erp/rapid-start/packages',
          '/erp/rapid-start/worksheet',
          '/erp/rapid-start/templates',
          '/erp/rapid-start/import',
        ],
      },
    ],
  },
];

/**
 * The role center navigation bar.
 * <p>
 * Tier 1 (Main Navigation Bar): Company, top-level groups (Finance ▾, Setup ▾, Reports ▾)
 * and direct links, scrolled with arrows, role switcher, and Menu Suite explorer.
 * <p>
 * Tier 2 (sublinks bar): Displays the sublinks (children) of the currently selected
 * or active group directly beneath the top bar in accent styling.
 * </p>
 */
@Component({
  selector: 'app-role-nav-bar',
  templateUrl: './role-nav-bar.component.html',
  styleUrls: ['./role-nav-bar.component.scss'],
  imports: [CoreModule, RouterModule, MatMenuModule, MatDividerModule, MatTooltipModule, CompanyMenuComponent],
})
export class RoleNavBarComponent implements OnInit, AfterViewInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly toaster = inject(ToasterService);
  private readonly localization = inject(LocalizationService);
  private readonly permissions = inject(PermissionService);
  private readonly routes = inject(RoutesService);
  private readonly menuSuite = inject(MenuSuiteService);
  readonly navigation = inject(RoleNavigationService);

  @ViewChild('scroller') private scrollerRef?: ElementRef<HTMLElement>;
  private subScrollerRef?: ElementRef<HTMLElement>;
  private subScrollerCleanup?: () => void;

  @ViewChild('subScroller')
  set subScroller(ref: ElementRef<HTMLElement> | undefined) {
    this.subScrollerRef = ref;
    this.subScrollerCleanup?.();
    this.subScrollerCleanup = undefined;

    const subElement = ref?.nativeElement;
    if (!subElement) {
      return;
    }

    setTimeout(() => this.updateSubArrows());

    if (typeof ResizeObserver === 'undefined') {
      return;
    }

    const subObserver = new ResizeObserver(() => this.updateSubArrows());
    subObserver.observe(subElement);
    const subContent = new MutationObserver(() => this.updateSubArrows());
    subContent.observe(subElement, { childList: true, subtree: true });
    this.subScrollerCleanup = () => {
      subObserver.disconnect();
      subContent.disconnect();
    };
  }

  private readonly routeTree = toSignal(this.routes.visible$, { initialValue: this.routes.visible });

  /** Map of visible, permission-granted ERP routes by path. Empty in isolated unit tests. */
  private readonly availableRoutes = computed(() => {
    const map = new Map<string, RoleCenterNavItemDto>();
    const walk = (nodes: TreeNode<ABP.Route>[] | undefined) => {
      if (!nodes) {
        return;
      }
      for (const node of nodes) {
        if (node.invisible) {
          continue;
        }
        if (node.requiredPolicy && !this.permissions.getGrantedPolicy(node.requiredPolicy)) {
          continue;
        }
        if (node.path && node.path.startsWith('/erp/')) {
          map.set(node.path, {
            key: node.path,
            displayName: this.localization.instant(node.name),
            route: node.path,
            children: [],
          });
        }
        if (node.children?.length) {
          walk(node.children);
        }
      }
    };
    walk(this.routeTree());
    return map;
  });

  /** Built BC area groups from available routes and server items. */
  private readonly blueprintGroups = computed(() => {
    const routeMap = new Map<string, RoleCenterNavItemDto>(this.availableRoutes());
    // Also index any links already sent by the server so their localized names are always available.
    for (const item of this.profile()?.navigation ?? []) {
      if (item.route) {
        routeMap.set(item.route, { ...item, children: [] });
      }
      for (const child of item.children ?? []) {
        if (child.route && !child.children?.length) {
          routeMap.set(child.route, { ...child, children: [] });
        }
        for (const grandChild of child.children ?? []) {
          if (grandChild.route) {
            routeMap.set(grandChild.route, { ...grandChild, children: [] });
          }
        }
      }
    }

    const groups = new Map<string, RoleCenterNavItemDto>();
    if (this.availableRoutes().size === 0) {
      return groups;
    }

    for (const bp of BC_AREA_BLUEPRINTS) {
      const children: RoleCenterNavItemDto[] = [];
      for (const route of bp.directRoutes) {
        const link = routeMap.get(route);
        if (link) {
          children.push(link);
        }
      }
      for (const sub of bp.subMenus) {
        const subChildren: RoleCenterNavItemDto[] = [];
        for (const route of sub.routes) {
          const link = routeMap.get(route);
          if (link) {
            subChildren.push(link);
          }
        }
        if (subChildren.length > 0) {
          children.push({
            key: sub.key,
            displayName: this.localization.instant(sub.nameKey),
            route: subChildren[0].route,
            children: subChildren,
          });
        }
      }
      if (children.length > 0) {
        groups.set(bp.key, {
          key: bp.key,
          displayName: this.localization.instant(bp.nameKey),
          children,
        });
      }
    }
    return groups;
  });

  readonly profile = this.navigation.profile;

  readonly items = computed<RoleCenterNavItemDto[]>(() => {
    const raw = this.profile()?.navigation ?? [];
    const blueprints = this.blueprintGroups();
    if (blueprints.size === 0) {
      return raw;
    }

    const enriched = raw.map(item => {
      if (!this.isMenu(item) || !item.key) {
        return item;
      }
      const hasSubMenus = (item.children ?? []).some(c => (c.children?.length ?? 0) > 0);
      if (hasSubMenus) {
        return item;
      }
      const bp = blueprints.get(item.key);
      return bp ? { ...item, children: bp.children } : item;
    });

    // Ensure any top-level direct link (e.g. Items) has its area menu available in the bar if not already present.
    const existingKeys = new Set(enriched.map(i => i.key));
    for (const item of raw) {
      if (!this.isMenu(item) && item.route) {
        for (const [key, bpGroup] of blueprints.entries()) {
          if (!existingKeys.has(key) && this.groupContainsRoute(bpGroup, item.route)) {
            enriched.push(bpGroup);
            existingKeys.add(key);
            break;
          }
        }
      }
    }

    return enriched;
  });

  readonly canScrollBack = signal(false);
  readonly canScrollOn = signal(false);
  readonly canScrollSubBack = signal(false);
  readonly canScrollSubOn = signal(false);
  readonly busy = signal(false);
  readonly currentUrl = signal(this.router.url);

  /** Explicitly selected top group; null falls back to route-based or first menu group. */
  readonly selectedGroupKey = signal<string | null>(null);

  /** The top group whose sublinks are currently displayed in the sublink bar. */
  readonly selectedGroup = computed<RoleCenterNavItemDto | null>(() => {
    // Read currentUrl so computed re-evaluates on route changes
    this.currentUrl();
    const all = this.items();
    const blueprints = this.blueprintGroups();
    const key = this.selectedGroupKey();
    if (key) {
      const found = all.find(i => i.key === key && this.isMenu(i)) ?? blueprints.get(key);
      if (found && this.isMenu(found)) {
        return found;
      }
    }
    // Auto-detect based on active route
    const active =
      all.find(i => this.isMenu(i) && this.isMenuActive(i)) ??
      Array.from(blueprints.values()).find(g => this.isMenuActive(g));
    if (active) {
      return active;
    }
    // Default to first menu item with children
    return all.find(i => this.isMenu(i)) ?? null;
  });

  /** The sublinks to show in the second row (sublink bar). */
  readonly sublinks = computed<RoleCenterNavItemDto[]>(() => {
    return this.selectedGroup()?.children ?? [];
  });

  readonly ProfileSource = ProfileSource;

  ngOnInit(): void {
    this.navigation.start();
    this.destroyRef.onDestroy(() => this.subScrollerCleanup?.());

    // When navigating to a new route, update the active group if the route is inside one
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.currentUrl.set(this.router.url);
        const matchingGroup =
          this.items().find(i => this.isMenu(i) && this.isMenuActive(i)) ??
          Array.from(this.blueprintGroups().values()).find(g => this.isMenuActive(g));
        if (matchingGroup) {
          this.selectedGroupKey.set(matchingGroup.key ?? null);
        }
        setTimeout(() => this.updateSubArrows());
      });
  }

  ngAfterViewInit(): void {
    const element = this.scrollerRef?.nativeElement;
    if (!element || typeof ResizeObserver === 'undefined') {
      return;
    }

    const observer = new ResizeObserver(() => this.updateArrows());
    observer.observe(element);
    const content = new MutationObserver(() => this.updateArrows());
    content.observe(element, { childList: true, subtree: true });
    this.destroyRef.onDestroy(() => {
      observer.disconnect();
      content.disconnect();
    });
  }

  isMenu(item: RoleCenterNavItemDto): boolean {
    return (item.children?.length ?? 0) > 0;
  }

  private groupContainsRoute(group: RoleCenterNavItemDto, route: string): boolean {
    return (group.children ?? []).some(child => {
      if (child.route === route && !child.children?.length) {
        return true;
      }
      return (child.children ?? []).some(c => c.route === route);
    });
  }

  /** A menu reads as current when the page shown is one of its links. */
  isMenuActive(item: RoleCenterNavItemDto): boolean {
    return (item.children ?? []).some(child => {
      if (child.children?.length) {
        return child.children.some(c => !!c.route && this.router.isActive(c.route, ACTIVE_MATCH));
      }
      if (child.route && this.router.isActive(child.route, ACTIVE_MATCH)) {
        return true;
      }
      return false;
    });
  }

  /** Whether a top-level item is selected or its sublinks match the active route. */
  isItemActive(item: RoleCenterNavItemDto): boolean {
    return this.selectedGroup()?.key === item.key;
  }

  /** Whether a sublink in the second row matches the active route. */
  isSublinkActive(link: RoleCenterNavItemDto): boolean {
    if (link.children?.length) {
      return link.children.some(c => !!c.route && this.router.isActive(c.route, ACTIVE_MATCH));
    }
    if (link.route && this.router.isActive(link.route, ACTIVE_MATCH)) {
      return true;
    }
    return false;
  }

  selectGroup(item: RoleCenterNavItemDto): void {
    if (this.isMenu(item)) {
      this.selectedGroupKey.set(item.key ?? null);
      this.subScrollerRef?.nativeElement.scrollTo({ left: 0 });
      setTimeout(() => this.updateSubArrows());
    }
  }

  /** When clicking a top-level direct link (e.g. Customers, Vendors, Items), show its area's sublinks. */
  selectDirectLink(item: RoleCenterNavItemDto): void {
    if (!item.route) {
      return;
    }
    const matchingGroup =
      this.items().find(i => this.isMenu(i) && this.groupContainsRoute(i, item.route!)) ??
      Array.from(this.blueprintGroups().values()).find(g => this.groupContainsRoute(g, item.route!));
    if (matchingGroup?.key) {
      this.selectedGroupKey.set(matchingGroup.key);
      this.subScrollerRef?.nativeElement.scrollTo({ left: 0 });
      setTimeout(() => this.updateSubArrows());
    }
  }

  /** Where the profile came from, under the role menu's heading. */
  sourceText(): string {
    const profile = this.profile();
    switch (profile?.source) {
      case ProfileSource.User:
        return this.localization.instant('Erp::RoleFromYourChoice');
      case ProfileSource.Role:
        return this.localization.instant('Erp::RoleFromRole', profile.roleName ?? '');
      default:
        return this.localization.instant('Erp::RoleFromDefault');
    }
  }

  changeRole(profileId: string | null): void {
    if (this.busy()) {
      return;
    }

    this.busy.set(true);
    this.navigation.change(profileId).subscribe({
      next: profile => {
        this.busy.set(false);
        this.toaster.success(this.localization.instant('Erp::RoleChanged', profile.displayName ?? ''));
      },
      error: () => this.busy.set(false),
    });
  }

  openMenuSuite(): void {
    this.menuSuite.open();
  }

  /** Moves the links by most of the visible width. */
  scroll(direction: -1 | 1): void {
    const element = this.scrollerRef?.nativeElement;
    element?.scrollBy({ left: direction * element.clientWidth * 0.8, behavior: 'smooth' });
  }

  updateArrows(): void {
    const element = this.scrollerRef?.nativeElement;
    if (!element) {
      return;
    }

    this.canScrollBack.set(element.scrollLeft > 1);
    this.canScrollOn.set(element.scrollLeft + element.clientWidth < element.scrollWidth - 1);
  }

  /** Moves the sublinks by most of the visible width. */
  scrollSub(direction: -1 | 1): void {
    const element = this.subScrollerRef?.nativeElement;
    element?.scrollBy({ left: direction * element.clientWidth * 0.8, behavior: 'smooth' });
  }

  updateSubArrows(): void {
    const element = this.subScrollerRef?.nativeElement;
    if (!element) {
      return;
    }

    this.canScrollSubBack.set(element.scrollLeft > 1);
    this.canScrollSubOn.set(element.scrollLeft + element.clientWidth < element.scrollWidth - 1);
  }
}
