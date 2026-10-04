import { ABP, TreeNode } from '@abp/ng.core';

/** A page, or a group of pages that opens in place, inside a menu suite column. */
export interface SuiteEntry {
  name: string;
  path?: string;
  icon?: string;
  children: SuiteEntry[];
  /** Pages under a group; 1 for a page. */
  count: number;
}

/** One column heading of the menu suite, like "Enrollment (6)". */
export interface SuiteSection {
  name: string;
  entries: SuiteEntry[];
  count: number;
}

export interface SuiteOptions {
  /** Is the route's requiredPolicy (which may be an expression) granted? */
  isGranted: (policy: string | undefined) => boolean;
  /** Localizes a route name. */
  translate: (key: string) => string;
}

/**
 * Lays the menu tree out as a menu suite.
 * <p>
 * Every top-level menu becomes a section of its own direct pages, and every group under it becomes
 * a section too, so "ERP → Setup" reads as a "Setup" column rather than a line inside "ERP".
 * Anything nested deeper stays inside its section as a group that opens in place. The home page is
 * left out: the suite is opened from it.
 * </p>
 */
export function buildMenuSuite(tree: TreeNode<ABP.Route>[], options: SuiteOptions): SuiteSection[] {
  const sections: SuiteSection[] = [];

  for (const top of tree) {
    if (!visible(top, options) || top.path === '/') {
      continue;
    }

    if (!top.children?.length) {
      if (top.path) {
        sections.push(...section(options.translate(top.name), [page(top, options)]));
      }
      continue;
    }

    const pages: SuiteEntry[] = [];
    const groups: SuiteSection[] = [];

    for (const child of top.children) {
      if (!visible(child, options)) {
        continue;
      }

      if (child.children?.length) {
        groups.push(...section(options.translate(child.name), entries(child.children, options)));
      } else if (child.path) {
        pages.push(page(child, options));
      }
    }

    // The top level's own pages come first, so "ERP" sits ahead of its groups.
    sections.push(...section(options.translate(top.name), pages), ...groups);
  }

  return sections;
}

/**
 * Keeps the pages whose name (or whose section's name) contains the text, and opens every group
 * that still has a match. An empty filter returns the sections as they are.
 */
export function filterMenuSuite(sections: SuiteSection[], text: string): SuiteSection[] {
  const term = text.trim().toLocaleLowerCase();

  if (!term) {
    return sections;
  }

  const result: SuiteSection[] = [];

  for (const s of sections) {
    if (s.name.toLocaleLowerCase().includes(term)) {
      result.push(s);
      continue;
    }

    const kept = filterEntries(s.entries, term);

    if (kept.length) {
      result.push({ name: s.name, entries: kept, count: countPages(kept) });
    }
  }

  return result;
}

/** Every page in the sections, in reading order. */
export function pagesOf(sections: SuiteSection[]): SuiteEntry[] {
  const pages: SuiteEntry[] = [];
  const walk = (list: SuiteEntry[]) =>
    list.forEach(e => (e.children.length ? walk(e.children) : e.path && pages.push(e)));

  sections.forEach(s => walk(s.entries));
  return pages;
}

/** A page as the search lists it: its name, and where it sits in the menu. */
export interface SuitePage {
  name: string;
  path: string;
  icon?: string;
  /** "Setup › RapidStart": the column and any group the page sits in. */
  trail: string;
}

/** Every page, with the trail that says where it lives, for searching. */
export function indexPages(sections: SuiteSection[]): SuitePage[] {
  const pages: SuitePage[] = [];
  const walk = (list: SuiteEntry[], trail: string) =>
    list.forEach(e =>
      e.children.length
        ? walk(e.children, `${trail} › ${e.name}`)
        : e.path && pages.push({ name: e.name, path: e.path, icon: e.icon, trail }),
    );

  sections.forEach(s => walk(s.entries, s.name));
  return pages;
}

/**
 * Pages whose name or trail holds every word typed, in any order. Names that start with the text
 * come first, then names that contain it, then matches on the trail alone.
 */
export function searchPages(pages: SuitePage[], text: string, limit = 8): SuitePage[] {
  const term = text.trim().toLocaleLowerCase();

  if (!term) {
    return [];
  }

  const words = term.split(/\s+/);
  const rank = (p: SuitePage): number => {
    const name = p.name.toLocaleLowerCase();
    return name.startsWith(term) ? 0 : name.includes(term) ? 1 : 2;
  };

  return pages
    .filter(p => {
      const haystack = `${p.name} ${p.trail}`.toLocaleLowerCase();
      return words.every(w => haystack.includes(w));
    })
    .map(p => ({ p, r: rank(p) }))
    .sort((a, b) => a.r - b.r || a.p.name.localeCompare(b.p.name))
    .slice(0, limit)
    .map(x => x.p);
}

function filterEntries(list: SuiteEntry[], term: string): SuiteEntry[] {
  const kept: SuiteEntry[] = [];

  for (const e of list) {
    if (e.name.toLocaleLowerCase().includes(term)) {
      kept.push(e);
    } else if (e.children.length) {
      const children = filterEntries(e.children, term);

      if (children.length) {
        kept.push({ ...e, children, count: countPages(children) });
      }
    }
  }

  return kept;
}

function entries(nodes: TreeNode<ABP.Route>[], options: SuiteOptions): SuiteEntry[] {
  const list: SuiteEntry[] = [];

  for (const node of nodes) {
    if (!visible(node, options)) {
      continue;
    }

    if (node.children?.length) {
      const children = entries(node.children, options);

      if (children.length) {
        list.push({
          name: options.translate(node.name),
          icon: node.iconClass,
          children,
          count: countPages(children),
        });
      }
    } else if (node.path) {
      list.push(page(node, options));
    }
  }

  return list;
}

function page(node: TreeNode<ABP.Route>, options: SuiteOptions): SuiteEntry {
  return { name: options.translate(node.name), path: node.path, icon: node.iconClass, children: [], count: 1 };
}

function section(name: string, list: SuiteEntry[]): SuiteSection[] {
  return list.length ? [{ name, entries: list, count: countPages(list) }] : [];
}

function countPages(list: SuiteEntry[]): number {
  return list.reduce((sum, e) => sum + (e.children.length ? countPages(e.children) : 1), 0);
}

function visible(node: TreeNode<ABP.Route>, options: SuiteOptions): boolean {
  return !node.invisible && options.isGranted(node.requiredPolicy);
}
