import React, { useState, useRef, useEffect } from 'react';
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
import type { AiServiceStatus } from '../utils/aiConversation';
import { AiRequestGuard, buildAiChatPayload, aiConversationStorageKey, getAiAvailability } from '../utils/aiConversation';
import type { AIChatMessage, AiClarification, InterpretedRequestSummary } from '../types/hrms';

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

// 6 câu hỏi nghiệp vụ chuẩn hóa V2 đồng bộ 100% với WinForms FrmAI_Chat & FrmAI
const WINFORMS_QUICK_PROMPTS: ActionPrompt[] = [
  {
    label: '🎂 Nhân viên sinh nhật tháng này?',
    query: 'Danh sách nhân viên sinh nhật tháng này?',
  },
  {
    label: '📄 Hợp đồng sắp hết hạn trong 30 ngày?',
    query: 'Danh sách hợp đồng hết hạn trong 30 ngày?',
  },
  {
    label: '📈 Nhân viên chuẩn bị tăng lương?',
    query: 'Danh sách nhân viên chuẩn bị tăng lương?',
  },
  {
    label: '🏢 Thống kê nhân sự theo phòng ban?',
    query: 'Thống kê số lượng nhân viên theo từng phòng ban?',
  },
  {
    label: '👥 Danh sách tất cả nhân viên công ty?',
    query: 'Danh sách tất cả nhân viên trong công ty?',
  },
  {
    label: '💰 Báo cáo tổng quỹ lương tháng này?',
    query: 'Tổng quỹ lương tháng này là bao nhiêu?',
  },
];

// Lời chào chuẩn hoá V2
const INITIAL_MESSAGE: AIChatMessage = {
  id: 'init-1',
  sender: 'assistant',
  content:
    'Xin chào! Tôi là Trợ lý AI Quản trị Nhân sự V2. Tôi có thể giúp gì cho bạn hôm nay?\n\nBạn có thể hỏi các câu hỏi nghiệp vụ trong phạm vi được cấp quyền (ví dụ: "Danh sách nhân viên sinh nhật tháng này", "Hợp đồng hết hạn trong 30 ngày", "Thống kê nhân sự theo phòng ban", "Danh sách nhân viên chuẩn bị tăng lương").',
  timestamp: 'Vừa xong',
  source: 'AI_Assistant',
};

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
  // A fresh mount starts a new conversation because the visible history is kept only in memory.
  const [conversationId, setConversationId] = useState<string>(generateSessionConversationId);
  const [chatMessages, setChatMessages] = useState<AIChatMessage[]>([INITIAL_MESSAGE]);
  const [chatInput, setChatInput] = useState('');
  const [chatLoading, setChatLoading] = useState(false);
  const [isResetting, setIsResetting] = useState(false);
  const [typingMessageId, setTypingMessageId] = useState<string | null>(null);
  const [isAiConnected, setIsAiConnected] = useState<boolean | null>(null);
  const [isQueryReady, setIsQueryReady] = useState<boolean | null>(null);
  const [checkingStatus, setCheckingStatus] = useState<boolean>(false);

  const messagesEndRef = useRef<HTMLDivElement>(null);
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

  // Hàm tự động cuộn xuống dưới cùng của khung chat
  const scrollToBottom = (behavior: ScrollBehavior = 'smooth') => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior, block: 'end' });
    }
    if (chatContainerRef.current) {
      chatContainerRef.current.scrollTop = chatContainerRef.current.scrollHeight;
    }
  };

  // Cuộn xuống và kiểm tra kết nối khi mở Drawer
  useEffect(() => {
    if (open) {
      checkAiStatus();
      setTimeout(() => scrollToBottom('auto'), 150);
    }
  }, [open]);

  // Cuộn xuống khi có tin nhắn mới hoặc thay đổi trạng thái loading
  useEffect(() => {
    scrollToBottom('smooth');
  }, [chatMessages.length, chatLoading]);

  // Điều hướng nghiệp vụ dựa trên interpretedRequest.domain chuẩn xác (Section 12)
  const getActionForMessage = (msg: AIChatMessage): { label: string; route: string } | null => {
    if (msg.status !== 'answered') {
      return null;
    }
    const domain = msg.interpretedRequest?.domain?.toUpperCase();
    if (domain === 'PAYROLL') {
      return { label: 'Xem Bảng Lương Chi Tiết', route: 'bangluong' };
    }
    if (domain === 'CONTRACT') {
      return { label: 'Xem Danh Sách Hợp Đồng', route: 'hopdong' };
    }
    if (domain === 'ATTENDANCE' || domain === 'OVERTIME') {
      return { label: 'Xem Bảng Chấm Công & Tăng Ca', route: 'chamcong' };
    }
    if (domain === 'EMPLOYEE') {
      return { label: 'Xem Danh Sách Nhân Sự', route: 'nhanvien' };
    }
    if (domain === 'ALLOWANCE') {
      return { label: 'Xem Danh Sách Phụ Cấp', route: 'phucap' };
    }
    if (domain === 'SALARY_CHANGE') {
      return { label: 'Xem Quyết Định Nâng Lương', route: 'nangluong' };
    }

    // Dự phòng dựa trên nội dung câu trả lời nếu không có domain metadata
    const lower = msg.content.toLowerCase();
    if (lower.includes('quỹ lương') || lower.includes('thực lĩnh') || lower.includes('bảng lương')) {
      return { label: 'Xem Bảng Lương Chi Tiết', route: 'bangluong' };
    }
    if (lower.includes('hợp đồng') || lower.includes('hết hạn') || lower.includes('tái ký')) {
      return { label: 'Xem Danh Sách Hợp Đồng', route: 'hopdong' };
    }
    if (lower.includes('chấm công') || lower.includes('vắng') || lower.includes('tăng ca')) {
      return { label: 'Xem Bảng Chấm Công & Heatmap', route: 'chamcong' };
    }
    if (lower.includes('nhân sự') || lower.includes('nhân viên') || lower.includes('hồ sơ')) {
      return { label: 'Xem Danh Sách Nhân Sự', route: 'nhanvien' };
    }
    if (lower.includes('tăng lương') || lower.includes('nâng ngạch') || lower.includes('nâng lương')) {
      return { label: 'Xem Quyết Định Nâng Lương', route: 'nangluong' };
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

    const tokens: string[] = fullText.match(/\S+|\s+/g) || [fullText];
    const delay = Math.max(10, Math.min(26, Math.floor(1500 / Math.max(tokens.length, 1))));

    let currentAccum = '';
    for (let i = 0; i < tokens.length; i++) {
      if (activeTypingIdRef.current !== targetMsgId) break;

      currentAccum += tokens[i];
      setChatMessages((prev) =>
        prev.map((m) => (m.id === targetMsgId ? { ...m, content: currentAccum } : m))
      );

      scrollToBottom('auto');
      await new Promise((resolve) => setTimeout(resolve, delay));
    }

    if (activeTypingIdRef.current === targetMsgId) {
      setChatMessages((prev) =>
        prev.map((m) =>
          m.id === targetMsgId ? { ...m, content: fullText, ...metadata } : m
        )
      );
      setTypingMessageId(null);
      activeTypingIdRef.current = null;
      setTimeout(() => scrollToBottom('smooth'), 50);
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
    setChatMessages([INITIAL_MESSAGE]);

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
        signal: controller.signal
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
          ? 'Bạn không có quyền thực hiện tra cứu này trong phạm vi được yêu cầu.'
          : 'Rất tiếc hiện tại không thể kết nối tới dịch vụ AI. Vui lòng kiểm tra lại kết nối API backend.'),
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        status: backendStatus || (err?.response?.status === 403 ? 'forbidden' : 'error'),
      };
      setChatMessages((prev) => [...prev, errorReply]);
      setTimeout(() => scrollToBottom('smooth'), 50);
    } finally {
      isSendingRef.current = false;
    }
  };

  const isBusy = chatLoading || typingMessageId !== null || isResetting;

  const handleCopyMessage = async (content: string) => {
    try {
      if (navigator?.clipboard?.writeText) {
        await navigator.clipboard.writeText(content);
        message.success('Đã sao chép phản hồi vào bộ nhớ tạm!');
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
        message.success('Đã sao chép phản hồi vào bộ nhớ tạm!');
      } else {
        message.warning('Không thể sao chép văn bản.');
      }
    } catch {
      message.warning('Không thể sao chép văn bản vào bộ nhớ tạm.');
    }
  };

  return (
    <Drawer
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
                    ? 'Đang kiểm tra kết nối...'
                    : isAiConnected
                    ? 'Đã kết nối máy chủ AI V2 (Trực tuyến)'
                    : 'Mất kết nối máy chủ AI'
                }
              >
                {checkingStatus && isAiConnected === null ? (
                  <SyncOutlined spin style={{ color: '#1677ff', fontSize: 13 }} />
                ) : isAiConnected ? (
                  <CheckCircleFilled
                    style={{
                      color: '#52c41a',
                      fontSize: 15,
                      filter: 'drop-shadow(0 0 2px rgba(82, 196, 26, 0.4))',
                      cursor: 'pointer',
                    }}
                    onClick={checkAiStatus}
                  />
                ) : (
                  <CloseCircleFilled
                    style={{
                      color: '#ff4d4f',
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
                    <span>{isQueryReady === false ? "Nguồn tra cứu chưa sẵn sàng" : isQueryReady === true ? "Đã kết nối • Nguồn tra cứu sẵn sàng" : "API đã kết nối"}</span>
                  </>
                ) : isAiConnected === false ? (
                  <>
                    <CloseCircleFilled style={{ fontSize: 10 }} />
                    <span>Mất kết nối máy chủ AI</span>
                  </>
                ) : (
                  <span>Đang kiểm tra kết nối...</span>
                )}
              </Tag>
            </div>
          </div>
        </Space>
      }
      extra={
        <Tooltip title="Làm mới cuộc trò chuyện (Reset Chat)">
          <Button
            type="text"
            icon={<ClearOutlined style={{ color: '#64748b' }} />}
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
      <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
        {isAiConnected && isQueryReady === false && (
          <Alert type="warning" showIcon style={{ marginBottom: 12 }}
            message="Tra cứu dữ liệu chưa sẵn sàng"
            description="Quản trị viên cần hoàn tất cấu hình nguồn dữ liệu và phân quyền. Bạn vẫn có thể gửi lời chào." />
        )}
        {/* DANH SÁCH TIN NHẮN */}
        <div
          ref={chatContainerRef}
          style={{
            flex: 1,
            overflowY: 'auto',
            paddingRight: 4,
            display: 'flex',
            flexDirection: 'column',
            gap: 14,
            scrollBehavior: 'smooth',
          }}
        >
          {chatMessages.map((msg) => {
            const action = msg.sender === 'assistant' ? getActionForMessage(msg) : null;
            const isCurrentlyTyping = typingMessageId === msg.id;

            return (
              <div
                key={msg.id}
                style={{
                  alignSelf: msg.sender === 'user' ? 'flex-end' : 'flex-start',
                  maxWidth: '92%',
                  background: msg.sender === 'user' ? '#1677ff' : '#f8fafc',
                  color: msg.sender === 'user' ? '#fff' : '#0f172a',
                  border: msg.sender === 'user' ? 'none' : '1px solid #e2e8f0',
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
                        msg.interpretedRequest.effectiveScope || msg.interpretedRequest.requestedScope || "Phạm vi được cấp",
                        msg.interpretedRequest.resolvedPeriod,
                        msg.interpretedRequest.selectedEntityDisplay
                      ].filter(Boolean).join(' • ')}
                    </Tag>
                  </div>
                )}

                {/* NỘI DUNG VĂN BẢN */}
                <div style={{ whiteSpace: 'pre-wrap' }}>
                  {msg.content}
                  {isCurrentlyTyping && (
                    <span
                      style={{
                        display: 'inline-block',
                        width: 7,
                        height: 15,
                        backgroundColor: '#722ed1',
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
                  <div style={{ marginTop: 6, fontSize: 11, color: '#64748b' }}>
                    {msg.resultMetadata.sourcePublicLabel && (
                      <span>Nguồn: {msg.resultMetadata.sourcePublicLabel}</span>
                    )}
                    {msg.resultMetadata.total !== undefined && msg.resultMetadata.total !== null && (
                      <span> • Tổng: {msg.resultMetadata.total} bản ghi</span>
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
                        borderTop: '1px dashed #cbd5e1',
                        display: 'flex',
                        flexDirection: 'column',
                        gap: 6,
                      }}
                    >
                      <Text strong style={{ fontSize: 11, color: isClarificationActive ? '#4f46e5' : '#94a3b8', display: 'flex', alignItems: 'center', gap: 4 }}>
                        <QuestionCircleOutlined /> {msg.clarification.question || 'Vui lòng chọn một phương án:'}
                        {!isClarificationActive && <span style={{ fontSize: 10, fontStyle: 'italic' }}>(đã hoàn thành / hết hạn)</span>}
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
                              borderColor: isClarificationActive ? '#818cf8' : '#cbd5e1',
                              color: isClarificationActive ? '#4338ca' : '#94a3b8',
                              backgroundColor: isClarificationActive ? '#eef2ff' : '#f1f5f9',
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
                  <div style={{ marginTop: 10, paddingTop: 8, borderTop: '1px solid #e2e8f0' }}>
                    <Button
                      type="primary"
                      size="small"
                      icon={<ArrowRightOutlined />}
                      onClick={() => {
                        onClose();
                        onNavigate(action.route);
                      }}
                      style={{
                        background: '#0f172a',
                        borderColor: '#0f172a',
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
                  {msg.sender === 'assistant' && !isCurrentlyTyping && msg.content ? (
                    <Button
                      type="text"
                      size="small"
                      icon={<CopyOutlined style={{ fontSize: 11, color: '#94a3b8' }} />}
                      onClick={() => handleCopyMessage(msg.content)}
                      style={{ padding: '0 4px', height: 20, fontSize: 11, color: '#94a3b8' }}
                    >
                      Sao chép
                    </Button>
                  ) : <span />}
                  <div
                    style={{
                      fontSize: '10px',
                      color: msg.sender === 'user' ? 'rgba(255,255,255,0.7)' : '#94a3b8',
                    }}
                  >
                    {msg.timestamp}
                  </div>
                </div>
              </div>
            );
          })}

          {chatLoading && (
            <div style={{ alignSelf: 'flex-start', padding: 12, background: '#f8fafc', borderRadius: 8, border: '1px solid #e2e8f0' }}>
              <Spin size="small" /> <Text type="secondary" style={{ fontSize: 12, marginLeft: 8 }}>AI Copilot đang phân tích CSDL & tổng hợp báo cáo...</Text>
            </div>
          )}

          <div ref={messagesEndRef} style={{ float: 'left', clear: 'both', height: 1 }} />
        </div>

        {/* CÂU HỎI NHANH */}
        <div style={{ margin: '14px 0 8px 0' }}>
          <Text strong type="secondary" style={{ fontSize: 11, textTransform: 'uppercase', letterSpacing: 0.5 }}>
            ⚡ Câu hỏi nghiệp vụ thường gặp (WinForms):
          </Text>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 6, marginTop: 6 }}>
            {WINFORMS_QUICK_PROMPTS.map((p, idx) => (
              <div
                key={idx}
                onClick={() => !isBusy && handleSendMessage(p.query)}
                style={{
                  padding: '6px 10px',
                  background: isBusy ? '#f8fafc' : '#f1f5f9',
                  borderRadius: 6,
                  fontSize: 12,
                  color: isBusy ? '#94a3b8' : '#334155',
                  cursor: isBusy ? 'not-allowed' : 'pointer',
                  border: '1px solid #e2e8f0',
                  transition: 'background 0.15s ease',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                }}
                onMouseEnter={(e) => {
                  if (!isBusy) e.currentTarget.style.backgroundColor = '#e2e8f0';
                }}
                onMouseLeave={(e) => {
                  if (!isBusy) e.currentTarget.style.backgroundColor = '#f1f5f9';
                }}
              >
                <span>{p.label}</span>
                <ArrowRightOutlined style={{ fontSize: 10, color: '#94a3b8' }} />
              </div>
            ))}
          </div>
        </div>

        {/* THANH NHẬP CÂU HỎI */}
        <Space.Compact style={{ width: '100%' }}>
          <Input
            placeholder="Hỏi AI về sinh nhật, hợp đồng, tăng lương, phòng ban..."
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
            style={{ background: '#722ed1', borderColor: '#722ed1' }}
          >
            Gửi
          </Button>
        </Space.Compact>
      </div>
    </Drawer>
  );
};

export default AiChatDrawer;
