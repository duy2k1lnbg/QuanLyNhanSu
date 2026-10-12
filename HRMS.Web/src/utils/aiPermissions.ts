export type AiEffect = 'ALLOW' | 'DENY';
export type AiScope = 'SELF' | 'DEPARTMENT' | 'COMPANY' | 'ALL';
export interface AiGrant {
  GrantId?: number; CapabilityCode?: string; Effect: AiEffect; ScopeType: AiScope;
  ScopeKey?: string | null; ScopeName?: string; ValidFrom?: string | null; ValidTo?: string | null;
  GroupName?: string; IsActive?: boolean;
}
export interface AiCapability {
  CapabilityCode: string; Domain: string; Description: string; IsCapabilityEnabled: boolean;
  RequiredFunctionCode?: string; HasRequiredFunctionRight: boolean; AvailabilityStatus?: string;
  DirectGrants: AiGrant[]; InheritedGrants: AiGrant[]; EffectiveEffect: string;
  EffectiveScopeSummary?: string; ExplanationNotes?: string;
}
export interface AiOverview {
  SubjectType: 'USER' | 'GROUP'; SubjectId: number; SubjectCode: string; SubjectName: string;
  EmployeeId?: number; HasAiFeatureRight: boolean; IsReady: boolean; NotReadyReason?: string;
  CurrentRevision: number; Capabilities: AiCapability[];
}
export type AiDraft = Record<string, AiGrant[]>;
export function createAiDraft(capabilities: AiCapability[]): AiDraft {
  return Object.fromEntries(capabilities.map(c => [c.CapabilityCode, (c.DirectGrants ?? []).map(g => ({ ...g }))]));
}
export function buildAiGrants(capabilities: AiCapability[], draft: AiDraft) {
  return capabilities.filter(c => c.IsCapabilityEnabled).flatMap(c => (draft[c.CapabilityCode] ?? []).map(g => {
    if (!['ALLOW', 'DENY'].includes(g.Effect) || !['SELF', 'DEPARTMENT', 'COMPANY', 'ALL'].includes(g.ScopeType))
      throw new Error(c.CapabilityCode + ': invalid grant');
    const key = g.ScopeType === 'SELF' || g.ScopeType === 'ALL' ? null : g.ScopeKey?.trim();
    if (key === undefined || key === '') throw new Error(c.CapabilityCode + ': scope selection required');
    const from = g.ValidFrom ? Date.parse(g.ValidFrom) : null;
    const to = g.ValidTo ? Date.parse(g.ValidTo) : null;
    if ((from !== null && !Number.isFinite(from)) || (to !== null && !Number.isFinite(to)) ||
      (from !== null && to !== null && from >= to)) throw new Error(c.CapabilityCode + ': invalid effective interval');
    return { GrantId: g.GrantId, CapabilityCode: c.CapabilityCode, Effect: g.Effect, ScopeType: g.ScopeType,
      ScopeKey: key, ValidFrom: g.ValidFrom || null, ValidTo: g.ValidTo || null };
  }));
}

// The binary editor changes the allow/deny decision, not the existing data scopes.
export function isAiGrantActive(grant: AiGrant, now = Date.now()): boolean {
  return grant.IsActive !== false && (!grant.ValidFrom || Date.parse(grant.ValidFrom) <= now) &&
    (!grant.ValidTo || Date.parse(grant.ValidTo) > now);
}
export function hasInheritedAiDeny(capability: AiCapability): boolean {
  return (capability.InheritedGrants ?? []).some(g => g.Effect === 'DENY' && isAiGrantActive(g));
}
export function isAiPermissionOn(capability: AiCapability, grants: AiGrant[]): boolean {
  if (!capability.IsCapabilityEnabled) return false;
  const active = [...grants, ...(capability.InheritedGrants ?? [])].filter(g => isAiGrantActive(g));
  if (active.some(g => g.Effect === 'DENY')) return false;
  return active.some(g => g.Effect === 'ALLOW') ||
    (['INSURANCE_SELF', 'PAYROLL_SELF'].includes(capability.CapabilityCode) && capability.EffectiveEffect === 'ALLOW');
}
export function toggleAiPermission(capability: AiCapability, current: AiGrant[], enabled: boolean): AiGrant[] {
  if (!capability.IsCapabilityEnabled || (enabled && hasInheritedAiDeny(capability))) return current.map(g => ({ ...g }));
  const grants = current.filter(g => g.Effect === 'ALLOW').map(g => ({ ...g }));
  if (!enabled) return [...grants, { Effect: 'DENY', ScopeType: 'ALL', IsActive: true }];
  if (![...grants, ...(capability.InheritedGrants ?? [])].some(g => g.Effect === 'ALLOW' && isAiGrantActive(g))) {
    const self = grants.find(g => g.ScopeType === 'SELF');
    if (self) { self.ValidFrom = null; self.ValidTo = null; self.IsActive = true; }
    else grants.push({ Effect: 'ALLOW', ScopeType: 'SELF', IsActive: true });
  }
  return grants;
}

export function aiCapabilityLabel(capability: AiCapability, translate: (key: string) => string): string {
  const key = 'aiPermissions.capabilities.' + capability.CapabilityCode;
  const label = translate(key);
  return label === key ? capability.Description : label;
}
