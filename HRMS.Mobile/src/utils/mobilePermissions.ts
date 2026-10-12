import type { UserSession } from '../types/auth';
export type MobileAction = 'View' | 'Add' | 'Edit' | 'Delete' | 'Print';
export function canMobile(user: UserSession | null | undefined, code: string, action: MobileAction = 'View'): boolean {
  if (!user) return false;
  const detail = Object.entries(user.detailedRights || {}).find(([key]) => key.toUpperCase() === code.toUpperCase())?.[1];
  if (detail) {
    const value = detail[`can${action}`] ?? detail[`Can${action}`] ?? detail[`CAN_${action.toUpperCase()}`];
    return value === true || value === 1;
  }
  return action === 'View' && (user.rights || []).some(right => right.toUpperCase() === code.toUpperCase());
}
export const requestViewCodes = ['MOBILE_REQUEST_LEAVE','MOBILE_REQUEST_CORRECTION','MOBILE_REQUEST_OVERTIME','MOBILE_REQUEST_ADVANCE','MOBILE_PROFILE_EDIT','MOBILE_REQUEST_INSURANCE','MOBILE_REQUEST_DEPENDENT'];
export function canViewRequests(user: UserSession | null | undefined): boolean { return requestViewCodes.some(code => canMobile(user, code)); }
