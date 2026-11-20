/**
 * Groups of a card's action bar, in the order they appear. `manage` holds the record's own
 * housekeeping (refresh, copy, link, delete) and is shown last, as "Actions".
 */
export type ErpActionCategory =
  | 'process'
  | 'release'
  | 'posting'
  | 'approval'
  | 'prepare'
  | 'newDocument'
  | 'printSend'
  | 'navigate'
  | 'manage';

export const ACTION_CATEGORY_ORDER: readonly ErpActionCategory[] = [
  'process',
  'release',
  'posting',
  'approval',
  'prepare',
  'newDocument',
  'printSend',
  'navigate',
  'manage',
];

export const ACTION_CATEGORY_LABELS: Record<ErpActionCategory, string> = {
  process: 'Erp::ActionCategory.Process',
  release: 'Erp::ActionCategory.Release',
  posting: 'Erp::ActionCategory.Posting',
  approval: 'Erp::ActionCategory.Approval',
  prepare: 'Erp::ActionCategory.Prepare',
  newDocument: 'Erp::ActionCategory.NewDocument',
  printSend: 'Erp::ActionCategory.PrintSend',
  navigate: 'Erp::ActionCategory.Navigate',
  manage: 'Erp::ActionCategory.Actions',
};

export const ACTION_CATEGORY_ICONS: Record<ErpActionCategory, string> = {
  process: 'fas fa-gears',
  release: 'fas fa-lock',
  posting: 'fas fa-file-invoice-dollar',
  approval: 'fas fa-user-check',
  prepare: 'fas fa-wand-magic-sparkles',
  newDocument: 'fas fa-file-circle-plus',
  printSend: 'fas fa-print',
  navigate: 'fas fa-compass',
  manage: 'fas fa-bolt',
};

/** One action of a card's action bar. */
export interface ErpBarAction {
  key: string;
  labelKey: string;
  icon: string;
  /** The group it is listed under; worked out from the key when omitted (see `actionCategoryOf`). */
  category?: ErpActionCategory;
  /** Also shown as a button at the front of the bar; worked out from the key when omitted. */
  promoted?: boolean;
  /** Full permission name; the action is left out without it. */
  permission?: string;
  disabled?: boolean;
  /** Bootstrap button classes for the promoted button. */
  cssClass?: string;
  /** Shown in red, after a divider (e.g. Delete). */
  danger?: boolean;
}

/** A group of the bar with the actions the user may run, in the order they were given. */
export interface ErpActionGroup {
  category: ErpActionCategory;
  labelKey: string;
  icon: string;
  actions: ErpBarAction[];
}

const RULES: ReadonlyArray<[ErpActionCategory, RegExp]> = [
  ['approval', /approv|^(approve|reject|delegate|submit|recall)$/i],
  ['release', /^(release|reopen|open|hold|ready)$/i],
  [
    'posting',
    /^(run)?post|posting|^(undo\w*|ship|receive|invoice|invoiceall|pay|consume|output|scrap|ringup|issue|register)$/i,
  ],
  ['newDocument', /^(make\w*|create\w*|new\w*|convert\w*|copydocuments)$/i],
  ['printSend', /^(send|sendall|sendemail|print\w*|export\w*|publish|message|email\w*)$/i],
  [
    'prepare',
    /^(get\w*|copy\w*|calculate\w*|calc\w*|suggest\w*|apply\w*|allocate\w*|split|fill\w*|reserve\w*|reset|schedule|plan\w*|explode\w*|import\w*)$/i,
  ],
];

/** The group an action belongs to when its descriptor does not say. */
export function actionCategoryOf(action: Pick<ErpBarAction, 'key' | 'category'>): ErpActionCategory {
  if (action.category) {
    return action.category;
  }
  return RULES.find(([, pattern]) => pattern.test(action.key))?.[0] ?? 'process';
}

const PROMOTED = /^(runPosting|post|release|approve|submit|sendApprovalRequest|sendApproval|makeOrder|complete|finish)$/i;

/** Whether an action is shown as a button at the front of the bar as well as in its group. */
export function isPromoted(action: Pick<ErpBarAction, 'key' | 'promoted'>): boolean {
  return action.promoted ?? PROMOTED.test(action.key);
}

/** Splits actions into the bar's groups, in bar order, leaving out empty groups. */
export function groupActions(actions: readonly ErpBarAction[]): ErpActionGroup[] {
  const byCategory = new Map<ErpActionCategory, ErpBarAction[]>();
  for (const action of actions) {
    const category = actionCategoryOf(action);
    const list = byCategory.get(category) ?? [];
    list.push(action);
    byCategory.set(category, list);
  }
  return ACTION_CATEGORY_ORDER.filter(c => byCategory.has(c)).map(category => ({
    category,
    labelKey: ACTION_CATEGORY_LABELS[category],
    icon: ACTION_CATEGORY_ICONS[category],
    actions: byCategory.get(category)!,
  }));
}
