import { ToasterService } from '@abp/ng.theme.shared';
import { IdentityRoleService } from '@abp/ng.identity/proxy';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProfileDto, ProfileService } from '@proxy/profiles';
import { catchError, forkJoin, map, of } from 'rxjs';
import { ErpTableColumn } from '../../erp-shared';
import { RoleNavigationService } from '../../services/role-navigation.service';

/** One role and the profile its members work as. */
interface RoleRow {
  roleName: string;
  profileId: string | null;
}

/**
 * Profiles (Roles). Mirrors Business Central's Profiles page, where an administrator decides which
 * role center people open on. Here a profile is given per role; users can still pick their own
 * under "My role" on the navigation bar.
 */
@Component({
  selector: 'app-profiles',
  templateUrl: './profiles.component.html',
  standalone: false,
})
export class ProfilesComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly service = inject(ProfileService);
  private readonly identityRoles = inject(IdentityRoleService);
  private readonly toaster = inject(ToasterService);
  private readonly navigation = inject(RoleNavigationService);

  readonly profiles = signal<ProfileDto[]>([]);
  readonly rows = signal<RoleRow[]>([]);
  readonly savingRole = signal<string | null>(null);

  newRoleName = '';

  readonly columns: ErpTableColumn<RoleRow>[] = [
    { field: 'roleName', labelKey: 'Erp::RoleName', width: 240 },
    { field: 'profileId', labelKey: 'Erp::AssignedProfile', type: 'custom', width: 320, sortable: false, filterable: false },
  ];

  ngOnInit(): void {
    this.load();
  }

  /**
   * Every role of the identity service, with the profile assigned to it. Without the right to
   * list roles, the roles that already have a profile are still shown and more can be typed in.
   */
  load(): void {
    const roles$ = this.identityRoles.getAllList().pipe(
      map(result => (result.items ?? []).map(r => r.name ?? '').filter(Boolean)),
      catchError(() => of([] as string[])),
    );

    forkJoin([this.service.getProfiles(), this.service.getRoleAssignments(), roles$])
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(([profiles, assignments, roles]) => {
        this.profiles.set(profiles.items ?? []);
        const byRole = new Map((assignments.items ?? []).map(a => [a.roleName ?? '', a.profileId ?? null]));
        const names = [...new Set([...roles, ...byRole.keys()])].filter(Boolean).sort((a, b) => a.localeCompare(b));
        this.rows.set(names.map(roleName => ({ roleName, profileId: byRole.get(roleName) ?? null })));
      });
  }

  assign(row: RoleRow, profileId: string | null): void {
    this.savingRole.set(row.roleName);
    this.service
      .setRoleAssignment({ roleName: row.roleName, profileId })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.savingRole.set(null);
          this.rows.update(rows => rows.map(r => (r.roleName === row.roleName ? { ...r, profileId } : r)));
          this.toaster.success('Erp::SavedSuccessfully');
          // The administrator's own bar follows if one of their roles was changed.
          this.navigation.reload();
        },
        error: () => this.savingRole.set(null),
      });
  }

  addRole(): void {
    const roleName = this.newRoleName.trim();
    if (!roleName || this.rows().some(r => r.roleName === roleName)) {
      return;
    }

    this.rows.update(rows => [...rows, { roleName, profileId: null }].sort((a, b) => a.roleName.localeCompare(b.roleName)));
    this.newRoleName = '';
  }
}
