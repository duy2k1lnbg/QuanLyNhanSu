import React, { useState, useRef, useEffect, useLayoutEffect, useMemo } from 'react';
import {
  Drawer,
  Alert,
  Space,
  Avatar,
  Typography,
  Tag,
  Input,
  Button,
  Spin,
  message,
  Tooltip,
  Badge,
} from 'antd';
import {
  RobotOutlined,
  SendOutlined,
  ArrowRightOutlined,
  CopyOutlined,
  ClearOutlined,
  CheckCircleFilled,
  CloseCircleFilled,
  SyncOutlined,
  QuestionCircleOutlined,
} from '@ant-design/icons';
import api from '../services/api';
import { createAiTypingTimeline } from '../utils/aiTyping';
import type { AiServiceStatus } from '../utils/aiConversation';
import { AiRequestGuard, buildAiChatPayload, aiConversationStorageKey, getAiAvailability } from '../utils/aiConversation';
import type { AIChatMessage, AiClarification, InterpretedRequestSummary } from '../types/hrms';
import { useAppLanguage } from '../services/i18n';
import { useAppTheme } from '../theme/ThemeContext';

const { Text } = Typography;

interface AiChatDrawerProps {
  open: boolean;
  userId: number;
  onClose: () => void;
  isMobile: boolean;
  onNavigate?: (route: string) => void;
}

interface ActionPrompt {
  label: string;
  query: string;
  icon?: React.ReactNode;
}



const generateSafeUUID = (): string => {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return 'req_' + Date.now().toString(36) + '_' + Math.random().toString(36).substring(2, 10);
};

const generateSessionConversationId = (): string => {
  return 'conv_' + Date.now().toString(36) + '_' + Math.random().toString(36).substring(2, 8);
};

export const AiChatDrawer: React.FC<AiChatDrawerProps> = ({
  open,
  userId,
  onClose,
  isMobile,
  onNavigate,
}) => {
  const { t } = useAppLanguage();
  const { tokens } = useAppTheme();

  const initialMessage = useMemo<AIChatMessage>(() => ({
    id: 'init-1',
    sender: 'assistant',
    content: t('aiChat.greeting'),
    timestamp: t('aiChat.justNow'),
    source: 'AI_Assistant',
  }), [t]);

  const quickPrompts = useMemo<ActionPrompt[]>(() => [
    {
      label: t('aiChat.promptBirthday'),
      query: t('aiChat.promptBirthdayQuery'),
    },
    {
      label: t('aiChat.promptContracts'),
      query: t('aiChat.promptContractsQuery'),
    },
    {
      label: t('aiChat.promptRaise'),
      query: t('aiChat.promptRaiseQuery'),
    },
    {
      label: t('aiChat.promptDepartment'),
      query: t('aiChat.promptDepartmentQuery'),
    },
    {
      label: t('aiChat.promptAllEmp'),
      query: t('aiChat.promptAllEmpQuery'),
    },
    {
      label: t('aiChat.promptPayroll'),
      query: t('aiChat.promptPayrollQuery'),
    },
  ], [t]);

  // A fresh mount starts a new conversation because the visible history is kept only in memory.
  const [conversationId, setConversationId] = useState<string>(generateSessionConversationId);
  const [chatMessages, setChatMessages] = useState<AIChatMessage[]>([initialMessage]);
  const [chatInput, setChatInput] = useState('');
  const [chatLoading, setChatLoading] = useState(false);
  const [isResetting, setIsResetting] = useState(false);
  const [typingMessageId, setTypingMessageId] = useState<string | null>(null);
  const [isAiConnected, setIsAiConnected] = useState<boolean | null>(null);
  const [isQueryReady, setIsQueryReady] = useState<boolean | null>(null);
  const [checkingStatus, setCheckingStatus] = useState<boolean>(false);

  const chatContainerRef = useRef<HTMLDivElement>(null);
  const activeTypingIdRef = useRef<string | null>(null);
  const abortControllerRef = useRef<AbortController | null>(null);
  const requestGuardRef = useRef(new AiRequestGuard());
  const conversationVersionRef = useRef<number | undefined>(undefined);
  const pendingClarificationRef = useRef<AiClarification | null>(null);
  const isResettingRef = useRef(false);
  const isSendingRef = useRef(false);

  // Lưu conversationId vào sessionStorage theo user
  useEffect(() => {
    try { sessionStorage.setItem(aiConversationStorageKey(userId), conversationId); } catch { /* Conversation remains in memory. */ }
  }, [conversationId, userId]);

  useEffect(() => () => {
    requestGuardRef.current.invalidate();
    abortControllerRef.current?.abort();
    activeTypingIdRef.current = null;
  }, []);
  // Kiểm tra trạng thái máy chủ AI
  const checkAiStatus = async () => {
    try {
      setCheckingStatus(true);
      const res = await api.get<AiServiceStatus>('/ai/status');
      const availability = getAiAvailability(res.data);
      setIsAiConnected(availability.connected);
      setIsQueryReady(availability.queryReady);
    } catch {
      setIsAiConnected(false);
      setIsQueryReady(null);
    } finally {
      setCheckingStatus(false);
    }
  };

  // Scroll only this drawer, after React has committed the new text and layout.
  const scrollToBottom = (_behavior: ScrollBehavior = 'auto') => {
    const container = chatContainerRef.current;
    if (container) container.scrollTop = container.scrollHeight;
  };

  useLayoutEffect(() => {
    if (open) scrollToBottom();
  }, [open, chatMessages, chatLoading, typingMessageId]);

  useEffect(() => {
    if (!open) return;
    checkAiStatus();
    const container = chatContainerRef.current;
    if (!container) return;
    // Drawer opening and viewport resizing can change its height without a message update.
    const observer = new ResizeObserver(() => scrollToBottom());
    observer.observe(container);
    return () => observer.disconnect();
  }, [open]);

  // Điều hướng nghiệp vụ dựa trên interpretedRequest.domain chuẩn xác (Section 12)
  const getActionForMessage = (msg: AIChatMessage): { label: string; route: string } | null => {
    if (msg.status !== 'answered') {
      return null;
    }
    const domain = msg.interpretedRequest?.domain?.toUpperCase();
    if (domain === 'PAYROLL') {
      return { label: t('aiChat.openPayroll'), route: 'bangluong' };
    }
    if (domain === 'CONTRACT') {
      return { label: t('aiChat.openContracts'), route: 'hopdong' };
    }
    if (domain === 'ATTENDANCE' || domain === 'OVERTIME') {
      return { label: t('aiChat.openAttendance'), route: 'chamcong' };
    }
    if (domain === 'EMPLOYEE') {
      return { label: t('aiChat.openEmployees'), route: 'nhanvien' };
    }
    if (domain === 'ALLOWANCE') {
      return { label: t('aiChat.openAllowances'), route: 'phucap' };
    }
    if (domain === 'SALARY_CHANGE') {
      return { label: t('aiChat.openSalaryChanges'), route: 'nangluong' };
    }

    // Dự phòng dựa trên nội dung câu trả lời nếu không có domain metadata
    const lower = msg.content.toLowerCase();
    if (lower.includes('quỹ lương') || lower.includes('thực lĩnh') || lower.includes('bảng lương')) {
      return { label: t('aiChat.openPayroll'), route: 'bangluong' };
    }
    if (lower.includes('hợp đồng') || lower.includes('hết hạn') || lower.includes('tái ký')) {
      return { label: t('aiChat.openContracts'), route: 'hopdong' };
    }
    if (lower.includes('chấm công') || lower.includes('vắng') || lower.includes('tăng ca')) {
      return { label: t('aiChat.openAttendance'), route: 'chamcong' };
    }
    if (lower.includes('nhân sự') || lower.includes('nhân viên') || lower.includes('hồ sơ')) {
      return { label: t('aiChat.openEmployees'), route: 'nhanvien' };
    }
    if (lower.includes('tăng lương') || lower.includes('nâng ngạch') || lower.includes('nâng lương')) {
      return { label: t('aiChat.openSalaryChanges'), route: 'nangluong' };
    }
    return null;
  };

  // Hiệu ứng chữ chạy từng từ (typewriter / streaming)
  const streamTypingEffect = async (
    targetMsgId: string,
    fullText: string,
    metadata?: Partial<AIChatMessage>
  ) => {
    activeTypingIdRef.current = targetMsgId;
    setTypingMessageId(targetMsgId);

    const timeline = createAiTypingTimeline(fullText);
    const startedAt = performance.now();
    let previousLength = -1;
    while (activeTypingIdRef.current === targetMsgId) {
      const length = timeline.visibleLength(performance.now() - startedAt);
      if (length !== previousLength) {
        const content = fullText.slice(0, length);
        setChatMessages(prev => prev.map(m => m.id === targetMsgId ? { ...m, content } : m));
        previousLength = length;
      }
      if (length === fullText.length) break;
      await new Promise(resolve => setTimeout(resolve, 30));
    }

    if (activeTypingIdRef.current === targetMsgId) {
      setChatMessages((prev) =>
        prev.map((m) =>
          m.id === targetMsgId ? { ...m, content: fullText, ...metadata } : m
        )
      );
      setTypingMessageId(null);
      activeTypingIdRef.current = null;
    }
  };
  const handleResetChat = async () => {
    if (isResettingRef.current) return;
    isResettingRef.current = true;
    setIsResetting(true);

    requestGuardRef.current.invalidate();
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      abortControllerRef.current = null;
    }
    const oldConvId = conversationId;
    const newId = generateSessionConversationId();
    setConversationId(newId);
    conversationVersionRef.current = undefined;
    pendingClarificationRef.current = null;
    setChatLoading(false);
    activeTypingIdRef.current = null;
    setTypingMessageId(null);
    setChatMessages([initialMessage]);

    try {
      await api.post('/ai/reset', { conversationId: oldConvId });
      checkAiStatus();
    } catch {
      // Bỏ qua lỗi mạng
    } finally {
      isResettingRef.current = false;
      setIsResetting(false);
    }
    message.info('Đã làm mới phiên hội thoại AI.');
    setTimeout(() => scrollToBottom('auto'), 50);
  };

  const handleSendMessage = async (customPrompt?: string, optionToken?: string) => {
    const question = customPrompt || chatInput;
    if ((!question.trim() && !optionToken) || isBusy || isResettingRef.current || isSendingRef.current) return;
    isSendingRef.current = true;

    const currentGen = requestGuardRef.current.begin();
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    const controller = new AbortController();
    abortControllerRef.current = controller;

    const userDisplay = question.trim() || 'Lựa chọn làm rõ';
    const userMsg: AIChatMessage = {
      id: Date.now().toString(),
      sender: 'user',
      content: userDisplay,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    };

    setChatMessages((prev) => [...prev, userMsg]);
    if (!customPrompt) setChatInput('');
    setChatLoading(true);
    setTimeout(() => scrollToBottom('smooth'), 50);

    try {
      const res = await api.post<{
        status?: 'answered' | 'needs_clarification' | 'forbidden' | 'unsupported' | 'no_data' | 'error';
        answer: string;
        conversationId?: string;
        conversationVersion?: number;
        requestId?: string;
        clarification?: AiClarification;
        interpretedRequest?: InterpretedRequestSummary;
        resultMetadata?: {
          total?: number;
          hasMore?: boolean;
          asOf?: string;
          sourcePublicLabel?: string;
        };
        source?: string;
        sqlQuery?: string;
      }>('/ai/chat', buildAiChatPayload(question, conversationId, generateSafeUUID(),
        conversationVersionRef.current, pendingClarificationRef.current, optionToken), {
        signal: controller.signal,
        // API owns the configurable 30–900s deadline; allow its response to arrive.
        timeout: 930000
      });

      if (!requestGuardRef.current.accepts(currentGen)) {
        return; // Discard late response after reset or new message
      }

      if (res.data?.conversationId && res.data.conversationId !== conversationId) {
        setConversationId(res.data.conversationId);
      }

      if (res.data?.conversationVersion && res.data.conversationVersion > 0) conversationVersionRef.current = res.data.conversationVersion;
      pendingClarificationRef.current = res.data?.status === "needs_clarification" ? res.data.clarification ?? null : null;
      const fullAnswer = res.data?.answer || 'Không nhận được câu trả lời từ hệ thống.';
      setIsAiConnected(true);

      const botMsgId = (Date.now() + 1).toString();
      const botReplyPlaceholder: AIChatMessage = {
        id: botMsgId,
        sender: 'assistant',
        content: '',
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        source: res.data?.source || 'HR_Copilot_V2',
        status: res.data?.status || 'answered',
        clarification: res.data?.clarification,
        interpretedRequest: res.data?.interpretedRequest,
        resultMetadata: res.data?.resultMetadata,
      };

      setChatLoading(false);
      setChatMessages((prev) => [...prev, botReplyPlaceholder]);

      await streamTypingEffect(botMsgId, fullAnswer, {
        status: res.data?.status,
        clarification: res.data?.clarification,
        interpretedRequest: res.data?.interpretedRequest,
        resultMetadata: res.data?.resultMetadata,
        source: res.data?.source,
        sqlQuery: res.data?.sqlQuery,
      });
    } catch (err: any) {
      if (!requestGuardRef.current.accepts(currentGen)) {
        return; // Discard error callback from cancelled request
      }
      if (err?.name === 'CanceledError' || err?.message === 'canceled') {
        return;
      }

      setIsAiConnected(!!err?.response);
      setChatLoading(false);

      if (err?.response?.data?.errorCode === "AI_SETUP_REQUIRED") setIsQueryReady(false);
      const backendAnswer = err?.response?.data?.answer;
      const backendStatus = err?.response?.data?.status;
      const backendVersion = err?.response?.data?.conversationVersion;
      if (typeof backendVersion === 'number') conversationVersionRef.current = backendVersion;
      pendingClarificationRef.current = null;

      const errorMsgId = (Date.now() + 1).toString();
      const errorReply: AIChatMessage = {
        id: errorMsgId,
        sender: 'assistant',
        content: backendAnswer || (err?.response?.status === 403
          ? t('aiChat.forbidden')
          : t('aiChat.serviceUnavailable')),
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        status: backendStatus || (err?.response?.status === 403 ? 'forbidden' : 'error'),
      };
      setChatMessages((prev) => [...prev, errorReply]);
    } finally {
      isSendingRef.current = false;
    }
  };

  const isBusy = chatLoading || typingMessageId !== null || isResetting;

  const handleCopyMessage = async (content: string) => {
    try {
      if (navigator?.clipboard?.writeText) {
        await navigator.clipboard.writeText(content);
        message.success(t('aiChat.copiedSuccess'));
        return;
      }
      const textArea = document.createElement('textarea');
      textArea.value = content;
      textArea.style.position = 'fixed';
      textArea.style.opacity = '0';
      document.body.appendChild(textArea);
      textArea.focus();
      textArea.select();
      const successful = document.execCommand('copy');
      document.body.removeChild(textArea);
      if (successful) {
        message.success(t('aiChat.copiedSuccess'));
      } else {
        message.warning('Không thể sao chép văn bản.');
      }
    } catch {
      message.warning('Không thể sao chép văn bản vào bộ nhớ tạm.');
    }
  };

  return (
    <Drawer
      className="hrms-ai-chat"
      title={
        <Space align="center" size={10}>
          <Badge
            status={isAiConnected ? 'success' : isAiConnected === false ? 'error' : 'processing'}
            offset={[-2, 28]}
          >
            <Avatar
              style={{
                background:
                  isAiConnected === false
                    ? 'linear-gradient(135deg, #ef4444 0%, #b91c1c 100%)'
                    : 'linear-gradient(135deg, #722ed1 0%, #1677ff 100%)',
              }}
              icon={<RobotOutlined />}
            />
          </Badge>
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
              <Text strong style={{ fontSize: 15 }}>
                HRMS AI Copilot V2
              </Text>
              <Tooltip
                title={
                  checkingStatus
                    ? t('aiChat.connectionChecking')
                    : isAiConnected
                    ? t('aiChat.connectionOnline')
                    : t('aiChat.statusDisconnected')
                }
              >
                {checkingStatus && isAiConnected === null ? (
                  <SyncOutlined spin style={{ color: tokens.primary, fontSize: 13 }} />
                ) : isAiConnected ? (
                  <CheckCircleFilled
                    style={{
                      color: tokens.successText,
                      fontSize: 15,
                      filter: 'drop-shadow(0 0 2px rgba(82, 196, 26, 0.4))',
                      cursor: 'pointer',
                    }}
                    onClick={checkAiStatus}
                  />
                ) : (
                  <CloseCircleFilled
                    style={{
                      color: tokens.dangerText,
                      fontSize: 15,
                      filter: 'drop-shadow(0 0 2px rgba(255, 77, 79, 0.4))',
                      cursor: 'pointer',
                    }}
                    onClick={checkAiStatus}
                  />
                )}
              </Tooltip>
            </div>
            <div>
              <Tag
                color={isAiConnected ? (isQueryReady === false ? 'warning' : 'success') : isAiConnected === false ? 'error' : 'default'}
                style={{
                  fontSize: 10,
                  borderRadius: 4,
                  padding: '0 6px',
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: 4,
                  cursor: 'pointer',
                }}
                onClick={checkAiStatus}
              >
                {isAiConnected ? (
                  <>
                    <CheckCircleFilled style={{ fontSize: 10 }} />
                    <span>{isQueryReady === false ? t('aiChat.queryUnavailable') : t('aiChat.statusConnected')}</span>
                  </>
                ) : isAiConnected === false ? (
                  <>
                    <CloseCircleFilled style={{ fontSize: 10 }} />
                    <span>{t('aiChat.statusDisconnected')}</span>
                  </>
                ) : (
                  <span>{t('aiChat.statusConnecting')}</span>
                )}
              </Tag>
            </div>
          </div>
        </Space>
      }
      extra={
        <Tooltip title={t('aiChat.resetChat')}>
          <Button
            type="text"
            icon={<ClearOutlined style={{ color: tokens.textSecondary }} />}
            onClick={handleResetChat}
            disabled={isBusy}
          />
        </Tooltip>
      }
      placement="right"
      width={isMobile ? '100%' : 'min(480px, 95vw)'}
      onClose={onClose}
      open={open}
      bodyStyle={{ display: 'flex', flexDirection: 'column', padding: '16px' }}
    >
      <style>{`
        @keyframes cursorBlink {
          0%, 100% { opacity: 1; }
          50% { opacity: 0; }
        }
      `}</style>
      <div style={{ display: 'flex', flexDirection: 'column', height: '100%', minHeight: 0 }}>
        {isAiConnected && isQueryReady === false && (
          <Alert type="warning" showIcon style={{ marginBottom: 12 }}
            message={t('aiChat.queryUnavailable')}
            description={t('aiChat.queryUnavailableHelp')} />
        )}
        {/* DANH SÁCH TIN NHẮN */}
        <div
          ref={chatContainerRef}
          style={{
            flex: 1,
            minHeight: 0,
            overflowY: 'auto',
            overflowX: 'hidden',
            paddingRight: 4,
            display: 'flex',
            flexDirection: 'column',
            gap: 14,
            scrollBehavior: 'auto',
          }}
        >
          {chatMessages.map((msg) => {
            const displayedContent = msg.id === initialMessage.id ? initialMessage.content : msg.content;
            const displayedTimestamp = msg.id === initialMessage.id ? initialMessage.timestamp : msg.timestamp;
            const action = msg.sender === 'assistant' ? getActionForMessage(msg) : null;
            const isCurrentlyTyping = typingMessageId === msg.id;

            return (
              <div
                key={msg.id}
                style={{
                  alignSelf: msg.sender === 'user' ? 'flex-end' : 'flex-start',
                  maxWidth: '92%',
                  flexShrink: 0,
                  overflowWrap: 'anywhere',
                  background: msg.sender === 'user' ? tokens.btnPrimaryBg : tokens.cardSecondaryBg,
                  color: msg.sender === 'user' ? tokens.btnPrimaryText : tokens.textPrimary,
                  border: msg.sender === 'user' ? 'none' : `1px solid ${tokens.borderMedium}`,
                  padding: '12px 16px',
                  borderRadius: msg.sender === 'user' ? '14px 14px 2px 14px' : '14px 14px 14px 2px',
                  fontSize: '13.5px',
                  lineHeight: '1.55',
                  boxShadow: '0 1px 3px rgba(0,0,0,0.03)',
                }}
              >
                {/* TÓM TẮT DIỄN GIẢI AN TOÀN (SAFE INTERPRETED REQUEST) */}
                {msg.interpretedRequest?.domain && !isCurrentlyTyping && (
                  <div style={{ marginBottom: 6 }}>
                    <Tag color="purple" style={{ fontSize: 10.5, borderRadius: 4, padding: '0 6px' }}>
                      {[
                        msg.interpretedRequest.effectiveScope || msg.interpretedRequest.requestedScope || t('aiChat.scopeGranted'),
                        msg.interpretedRequest.resolvedPeriod,
                        msg.interpretedRequest.selectedEntityDisplay
                      ].filter(Boolean).join(' • ')}
                    </Tag>
                  </div>
                )}

                {/* NỘI DUNG VĂN BẢN */}
                <div style={{ whiteSpace: 'pre-wrap' }}>
                  {displayedContent}
                  {isCurrentlyTyping && (
                    <span
                      style={{
                        display: 'inline-block',
                        width: 7,
                        height: 15,
                        backgroundColor: tokens.primary,
                        marginLeft: 3,
                        verticalAlign: 'middle',
                        animation: 'cursorBlink 0.7s infinite',
                        borderRadius: 1,
                      }}
                    />
                  )}
                </div>

                {/* RESULT METADATA (BẢO TOÀN SỐ LIỆU NGUỒN) */}
                {msg.resultMetadata && !isCurrentlyTyping && (
                  <div style={{ marginTop: 6, fontSize: 11, color: tokens.textSecondary }}>
                    {msg.resultMetadata.sourcePublicLabel && (
                      <span>{t('aiChat.sourceLabel', { source: msg.resultMetadata.sourcePublicLabel })}</span>
                    )}
                    {msg.resultMetadata.total !== undefined && msg.resultMetadata.total !== null && (
                      <span> • {t('aiChat.totalRecords', { count: msg.resultMetadata.total })}</span>
                    )}
                  </div>
                )}

                {/* CÁC PHƯƠNG ÁN LÀM RÕ (CLARIFICATION OPTIONS GATE) */}
                {msg.clarification && msg.clarification.options?.length > 0 && !isCurrentlyTyping && (() => {
                  const isClarificationActive = pendingClarificationRef.current && pendingClarificationRef.current.clarificationId === msg.clarification.clarificationId;
                  return (
                    <div
                      style={{
                        marginTop: 10,
                        paddingTop: 8,
                        borderTop: `1px dashed ${tokens.borderMedium}`,
                        display: 'flex',
                        flexDirection: 'column',
                        gap: 6,
                      }}
                    >
                      <Text strong style={{ fontSize: 11, color: isClarificationActive ? tokens.violetText : tokens.textSecondary, display: 'flex', alignItems: 'center', gap: 4 }}>
                        <QuestionCircleOutlined /> {msg.clarification.question || t('aiChat.clarificationChoice')}
                        {!isClarificationActive && <span style={{ fontSize: 10, fontStyle: 'italic' }}>{t('aiChat.clarificationComplete')}</span>}
                      </Text>
                      <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6 }}>
                        {msg.clarification.options.map((opt) => (
                          <Button
                            key={opt.optionToken}
                            size="small"
                            type="default"
                            onClick={() => {
                              if (!isBusy && isClarificationActive) {
                                pendingClarificationRef.current = null;
                                handleSendMessage(opt.label, opt.optionToken);
                              }
                            }}
                            disabled={isBusy || !isClarificationActive}
                            style={{
                              borderRadius: 12,
                              fontSize: 12,
                              borderColor: isClarificationActive ? tokens.primary : tokens.borderMedium,
                              color: isClarificationActive ? tokens.violetText : tokens.textSecondary,
                              backgroundColor: isClarificationActive ? tokens.activeBg : tokens.cardSecondaryBg,
                              fontWeight: 500,
                            }}
                          >
                            {opt.label}
                          </Button>
                        ))}
                      </div>
                    </div>
                  );
                })()}

                {/* NÚT ĐIỀU HƯỚNG NHANH THEO NGỮ CẢNH */}
                {action && onNavigate && !isCurrentlyTyping && (
                  <div style={{ marginTop: 10, paddingTop: 8, borderTop: `1px solid ${tokens.borderMedium}` }}>
                    <Button
                      type="primary"
                      size="small"
                      icon={<ArrowRightOutlined />}
                      onClick={() => {
                        onClose();
                        onNavigate(action.route);
                      }}
                      style={{
                        background: tokens.btnPrimaryBg,
                        borderColor: tokens.btnPrimaryBg,
                        fontWeight: 600,
                        fontSize: 12,
                      }}
                    >
                      {action.label}
                    </Button>
                  </div>
                )}

                <div
                  style={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between',
                    marginTop: 6,
                  }}
                >
                  {msg.sender === 'assistant' && !isCurrentlyTyping && displayedContent ? (
                    <Button
                      type="text"
                      size="small"
                      icon={<CopyOutlined style={{ fontSize: 11, color: tokens.textSecondary }} />}
                      onClick={() => handleCopyMessage(displayedContent)}
                      style={{ padding: '0 4px', height: 20, fontSize: 11, color: tokens.textSecondary }}
                    >
                      {t('aiChat.btnCopy')}
                    </Button>
                  ) : <span />}
                  <div
                    style={{
                      fontSize: '10px',
                      color: msg.sender === 'user' ? tokens.btnPrimaryText : tokens.textSecondary,
                    }}
                  >
                    {displayedTimestamp}
                  </div>
                </div>
              </div>
            );
          })}

          {chatLoading && (
            <div style={{ alignSelf: 'flex-start', padding: 12, background: tokens.cardSecondaryBg, borderRadius: 8, border: `1px solid ${tokens.borderMedium}` }}>
              <Spin size="small" /> <Text type="secondary" style={{ fontSize: 12, marginLeft: 8, color: tokens.textSecondary }}>{t('aiChat.thinking')}</Text>
            </div>
          )}


        </div>

        {/* CÂU HỎI NHANH */}
        <div style={{ margin: '14px 0 8px 0' }}>
          <Text strong type="secondary" style={{ fontSize: 11, textTransform: 'uppercase', letterSpacing: 0.5, color: tokens.textSecondary }}>
            {t('aiChat.quickPromptsTitle')}
          </Text>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 6, marginTop: 6 }}>
            {quickPrompts.map((p: ActionPrompt, idx: number) => (
              <div
                key={idx}
                onClick={() => !isBusy && handleSendMessage(p.query)}
                style={{
                  padding: '6px 10px',
                  background: tokens.cardSecondaryBg,
                  borderRadius: 6,
                  fontSize: 12,
                  color: isBusy ? tokens.textSecondary : tokens.textPrimary,
                  cursor: isBusy ? 'not-allowed' : 'pointer',
                  border: `1px solid ${tokens.borderMedium}`,
                  transition: 'background 0.15s ease',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                }}
                onMouseEnter={(e) => {
                  if (!isBusy) e.currentTarget.style.backgroundColor = tokens.hoverBg;
                }}
                onMouseLeave={(e) => {
                  if (!isBusy) e.currentTarget.style.backgroundColor = tokens.cardSecondaryBg;
                }}
              >
                <span>{p.label}</span>
                <ArrowRightOutlined style={{ fontSize: 10, color: tokens.textSecondary }} />
              </div>
            ))}
          </div>
        </div>

        {/* THANH NHẬP CÂU HỎI */}
        <Space.Compact style={{ width: '100%' }}>
          <Input
            placeholder={t('aiChat.inputPlaceholder')}
            maxLength={2000}
            value={chatInput}
            onChange={(e) => setChatInput(e.target.value)}
            onPressEnter={() => handleSendMessage()}
            disabled={isBusy}
          />
          <Button
            type="primary"
            icon={<SendOutlined />}
            onClick={() => handleSendMessage()}
            loading={chatLoading}
            disabled={isBusy}
            style={{ background: isBusy ? tokens.cardSecondaryBg : tokens.btnPrimaryBg, borderColor: tokens.borderMedium, color: isBusy ? tokens.textSecondary : tokens.btnPrimaryText }}
          >
            {t('aiChat.btnSend')}
          </Button>
        </Space.Compact>
      </div>
    </Drawer>
  );
};

export default AiChatDrawer;
