import type { AiClarification } from '../types/hrms';

// Shared by the actual Drawer and its behavioral tests.
export class AiRequestGuard {
  private generation = 0;
  begin(): number { return ++this.generation; }
  invalidate(): void { this.generation++; }
  accepts(generation: number): boolean { return this.generation === generation; }
}
export function buildAiChatPayload(question: string, conversationId: string, clientRequestId: string,
  version?: number, pending?: AiClarification | null, optionToken?: string) {
  return {
    question: optionToken ? '' : question.trim(), conversationId, clientRequestId,
    expectedConversationVersion: version,
    clarification: pending ? {
      clarificationId: pending.clarificationId,
      optionToken,
      freeTextAnswer: optionToken ? undefined : question.trim(),
    } : undefined,
  };
}
export function aiConversationStorageKey(userId: number | string): string {
  return `hrms_ai_conv_id_user_${userId}`;
}

export interface AiServiceStatus {
  connected?: boolean;
  queryReady?: boolean;
  llmAvailable?: boolean;
  message?: string;
}
export function getAiAvailability(status?: AiServiceStatus | null) {
  const connected = status?.connected === true;
  const queryReady = status?.queryReady;
  return { connected, queryReady: connected && typeof queryReady === 'boolean' ? queryReady : null };
}
