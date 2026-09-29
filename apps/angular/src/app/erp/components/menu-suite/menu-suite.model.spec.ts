import { ABP, TreeNode } from '@abp/ng.core';
import { buildMenuSuite, filterMenuSuite, indexPages, pagesOf, searchPages } from './menu-suite.model';

/** A tree node the way RoutesService hands it out. */
function node(route: Partial<ABP.Route>, children: TreeNode<ABP.Route>[] = []): TreeNode<ABP.Route> {
  return { name: '', ...route, children, isLeaf: children.length === 0 } as TreeNode<ABP.Route>;
}

describe('menu suite', () => {
  const tree = [
    node({ name: 'Home', path: '/' }),
    node({ name: 'ERP', path: '/erp' }, [
      node({ name: 'Sales Invoices', path: '/erp/sales-invoices' }),
      node({ name: 'Chart of Accounts', path: '/erp/chart-of-accounts', requiredPolicy: 'Erp.GLAccounts' }),
      node({ name: 'Setup' }, [
        node({ name: 'Modules', path: '/erp/setup/modules' }),
        node({ name: 'RapidStart' }, [
          node({ name: 'Packages', path: '/erp/rapid-start/packages' }),
          node({ name: 'Import', path: '/erp/rapid-start/import' }),
        ]),
      ]),
      node({ name: 'Hidden group', requiredPolicy: 'Denied' }, [node({ name: 'Secret', path: '/secret' })]),
    ]),
  ];

  const options = {
    isGranted: (policy?: string) => policy !== 'Denied' && policy !== 'Erp.GLAccounts',
    translate: (key: string) => key,
  };

  it('makes a column of the top level pages, then one per group, and leaves Home out', () => {
    const sections = buildMenuSuite(tree, options);

    expect(sections.map(s => s.name)).toEqual(['ERP', 'Setup']);
    expect(sections[0].entries.map(e => e.name)).toEqual(['Sales Invoices']);
  });

  it('counts the pages under a column, nested groups included', () => {
    const setup = buildMenuSuite(tree, options)[1];

    expect(setup.count).toBe(3);
    expect(setup.entries[1].name).toBe('RapidStart');
    expect(setup.entries[1].count).toBe(2);
  });

  it('leaves out what the user may not open', () => {
    const names = pagesOf(buildMenuSuite(tree, options)).map(p => p.name);

    expect(names).not.toContain('Chart of Accounts');
    expect(names).not.toContain('Secret');
  });

  it('finds pages inside nested groups and drops columns with no match', () => {
    const found = filterMenuSuite(buildMenuSuite(tree, options), 'pack');

    expect(found.map(s => s.name)).toEqual(['Setup']);
    expect(pagesOf(found).map(p => p.path)).toEqual(['/erp/rapid-start/packages']);
  });

  it('keeps a whole column when its heading matches', () => {
    const found = filterMenuSuite(buildMenuSuite(tree, options), 'setup');

    expect(found[0].count).toBe(3);
  });

  describe('page search', () => {
    const pages = indexPages(buildMenuSuite(tree, options));

    it('says where each page sits in the menu', () => {
      expect(pages.find(p => p.name === 'Import')?.trail).toBe('Setup › RapidStart');
    });

    it('matches every word typed, against the name or the trail', () => {
      expect(searchPages(pages, 'rapid imp').map(p => p.name)).toEqual(['Import']);
    });

    it('ranks names that start with the text first', () => {
      const all = [
        { name: 'Reports list', path: '/a', trail: 'X' },
        { name: 'Financial reports', path: '/b', trail: 'X' },
        { name: 'Other', path: '/c', trail: 'Reports' },
      ];

      expect(searchPages(all, 'reports').map(p => p.path)).toEqual(['/a', '/b', '/c']);
    });

    it('returns nothing for empty text', () => {
      expect(searchPages(pages, '  ')).toEqual([]);
    });
  });
});
