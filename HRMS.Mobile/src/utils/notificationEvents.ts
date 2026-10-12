const listeners = new Set<() => void>();
export function subscribeNotificationChanges(listener: () => void): () => void {
  listeners.add(listener);
  return () => { listeners.delete(listener); };
}
export function notifyNotificationsChanged(): void { for (const listener of listeners) listener(); }
