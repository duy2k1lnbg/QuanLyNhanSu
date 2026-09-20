import React, { useState, useRef, useEffect } from 'react';
import {
  Drawer,
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
} from '@ant-design/icons';
import api from '../services/api';
import type { AIChatMessage } from '../types/hrms';

const { Text } = Typography;

interface AiChatDrawerProps {
  open: boolean;
  onClose: () => void;
  isMobile: boolean;
  onNavigate?: (route: string) => void;
}

interface ActionPrompt {
  label: string;
  query: string;
  icon?: React.ReactNode;
}

// 6 câu hỏi nghiệp vụ chuẩn hóa đồng bộ 100% với WinForms FrmAI_Chat & FrmAI
const WINFORMS_QUICK_PROMPTS: ActionPrompt[] = [
  {
    label: '🎂 Nhân viên sinh nhật tháng này?',
    query: 'Danh sách nhân viên sinh nhật tháng này?',
  },
  {
    label: '📄 Hợp đồng sắp hết hạn trong 30 ngày?',
    query: 'Danh sách nhân viên sắp hết hạn hợp đồng?',
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
    label: '💰 Báo cáo tổng quỹ lương kỳ này?',
    query: 'Tổng quỹ lương tháng này là bao nhiêu?',
  },
];

// Lời chào chuẩn hoá khớp từng từ với WinForms FrmAI_Chat
const INITIAL_MESSAGE: AIChatMessage = {
  id: '1',
  sender: 'assistant',
  content:
    'Xin chào! Tôi là Trợ lý AI Quản trị Nhân sự. Tôi có thể giúp gì cho bạn hôm nay?\n\nBạn có thể hỏi bất kỳ câu hỏi nghiệp vụ nào bằng tiếng Việt tự nhiên (ví dụ: "Danh sách nhân viên sinh nhật tháng này", "Ai chuẩn bị lên lương", "Thống kê nhân sự theo phòng ban").',
  timestamp: 'Vừa xong',
  source: 'AI_Assistant',
};

export const AiChatDrawer: React.FC<AiChatDrawerProps> = ({
  open,
  onClose,
  isMobile,
  onNavigate,
}) => {
  const [chatMessages, setChatMessages] = useState<AIChatMessage[]>([INITIAL_MESSAGE]);
  const [chatInput, setChatInput] = useState('');
  const [chatLoading, setChatLoading] = useState(false);
  const [typingMessageId, setTypingMessageId] = useState<string | null>(null);
  const [isAiConnected, setIsAiConnected] = useState<boolean | null>(null);
  const [checkingStatus, setCheckingStatus] = useState<boolean>(false);

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const chatContainerRef = useRef<HTMLDivElement>(null);
  const activeTypingIdRef = useRef<string | null>(null);

  // Kiểm tra trạng thái máy chủ AI (Ollama / Qwen 2.5)
  const checkAiStatus = async () => {
    try {
      setCheckingStatus(true);
      const res = await api.get<{ connected: boolean; engine?: string; message?: string }>('/ai/status');
      setIsAiConnected(Boolean(res.data?.connected));
    } catch {
      setIsAiConnected(false);
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

  const getActionForContent = (text: string): { label: string; route: string } | null => {
    const lower = text.toLowerCase();
    if (lower.includes('quỹ lương') || lower.includes('thực lĩnh') || lower.includes('bảng lương')) {
      return { label: 'Xem Bảng Lương Chi Tiết', route: 'bangluong' };
    }
    if (lower.includes('hợp đồng') || lower.includes('hết hạn') || lower.includes('tái ký')) {
      return { label: 'Xem Danh Sách Hợp Đồng', route: 'hopdong' };
    }
    if (lower.includes('chấm công') || lower.includes('vắng') || lower.includes('ot') || lower.includes('tăng ca')) {
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

  // Hiệu ứng chữ chạy từng từ (typewriter / streaming) giống hệt chatbot thông minh
  const streamTypingEffect = async (
    targetMsgId: string,
    fullText: string,
    sqlQuery?: string,
    source?: string
  ) => {
    activeTypingIdRef.current = targetMsgId;
    setTypingMessageId(targetMsgId);

    // Tách theo từng từ và các khoảng trắng / ký tự ngắt dòng để giữ nguyên định dạng
    const tokens: string[] = fullText.match(/\S+|\s+/g) || [fullText];

    // Tốc độ đánh chữ linh hoạt (12ms - 28ms mỗi từ) tạo cảm giác AI đang phản hồi thời gian thực
    const delay = Math.max(12, Math.min(28, Math.floor(1600 / Math.max(tokens.length, 1))));

    let currentAccum = '';
    for (let i = 0; i < tokens.length; i++) {
      if (activeTypingIdRef.current !== targetMsgId) break;

      currentAccum += tokens[i];
      setChatMessages((prev) =>
        prev.map((m) => (m.id === targetMsgId ? { ...m, content: currentAccum } : m))
      );

      // Tự động cuộn xuống dưới cùng sau mỗi từ xuất hiện
      scrollToBottom('auto');

      await new Promise((resolve) => setTimeout(resolve, delay));
    }

    if (activeTypingIdRef.current === targetMsgId) {
      setChatMessages((prev) =>
        prev.map((m) =>
          m.id === targetMsgId ? { ...m, content: fullText, sqlQuery, source } : m
        )
      );
      setTypingMessageId(null);
      activeTypingIdRef.current = null;
      setTimeout(() => scrollToBottom('smooth'), 50);
    }
  };

  const handleResetChat = async () => {
    activeTypingIdRef.current = null;
    setTypingMessageId(null);
    try {
      await api.post('/ai/reset');
      checkAiStatus();
    } catch {
      // bỏ qua nếu lỗi mạng
    }
    setChatMessages([INITIAL_MESSAGE]);
    message.info('Đã làm mới phiên hội thoại AI.');
    setTimeout(() => scrollToBottom('auto'), 50);
  };

  const handleSendMessage = async (customPrompt?: string) => {
    const question = customPrompt || chatInput;
    if (!question.trim() || chatLoading || typingMessageId) return;

    const userMsg: AIChatMessage = {
      id: Date.now().toString(),
      sender: 'user',
      content: question,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    };

    setChatMessages((prev) => [...prev, userMsg]);
    if (!customPrompt) setChatInput('');
    setChatLoading(true);
    setTimeout(() => scrollToBottom('smooth'), 50);

    try {
      // Gửi trực tiếp câu hỏi đến Backend AI Controller (ChatboxManager & Oracle Live Engine)
      const res = await api.post<{ answer: string; sqlQuery?: string; source?: string }>('/ai/chat', {
        Question: question,
        Lang: 'vi',
      });

      const fullAnswer = res.data?.answer || 'Không nhận được câu trả lời từ hệ thống.';
      const isErrorSource = res.data?.source === 'Fallback_Error';
      setIsAiConnected(!isErrorSource);

      const botMsgId = (Date.now() + 1).toString();
      const botReplyPlaceholder: AIChatMessage = {
        id: botMsgId,
        sender: 'assistant',
        content: '',
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        source: res.data?.source || 'HR_Copilot',
      };

      setChatLoading(false);
      setChatMessages((prev) => [...prev, botReplyPlaceholder]);

      // Chạy hiệu ứng chữ chạy từng từ và tự động kéo khung chat xuống
      await streamTypingEffect(botMsgId, fullAnswer, res.data?.sqlQuery, res.data?.source);
    } catch {
      setIsAiConnected(false);
      setChatLoading(false);
      const errorMsgId = (Date.now() + 1).toString();
      const errorReply: AIChatMessage = {
        id: errorMsgId,
        sender: 'assistant',
        content: 'Rất tiếc hiện tại không thể kết nối tới dịch vụ AI. Vui lòng kiểm tra lại kết nối API backend.',
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      };
      setChatMessages((prev) => [...prev, errorReply]);
      setTimeout(() => scrollToBottom('smooth'), 50);
    }
  };

  const isBusy = chatLoading || typingMessageId !== null;

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
                HRMS AI Copilot
              </Text>
              <Tooltip
                title={
                  checkingStatus
                    ? 'Đang kiểm tra kết nối...'
                    : isAiConnected
                    ? 'Đã kết nối máy chủ AI (Trực tuyến)'
                    : 'Chạy offline / Lỗi kết nối AI'
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
                color={isAiConnected ? 'success' : isAiConnected === false ? 'error' : 'default'}
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
                    <span>Đã kết nối &bull; Qwen 2.5 & Oracle RAG</span>
                  </>
                ) : isAiConnected === false ? (
                  <>
                    <CloseCircleFilled style={{ fontSize: 10 }} />
                    <span>Chạy offline (Lỗi kết nối AI)</span>
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
      width={isMobile ? '100%' : 460}
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
        {/* DANH SÁCH TIN NHẮN (TỰ ĐỘNG CUỘN XUỐNG DƯỚI CÙNG) */}
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
            const action = msg.sender === 'assistant' ? getActionForContent(msg.content) : null;
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
                      onClick={() => {
                        navigator.clipboard.writeText(msg.content);
                        message.success('Đã sao chép phản hồi vào bộ nhớ tạm!');
                      }}
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

          {/* DUMMY ELEMENT ĐỂ CUỘN CHÍNH XÁC XUỐNG ĐÁY */}
          <div ref={messagesEndRef} style={{ float: 'left', clear: 'both', height: 1 }} />
        </div>

        {/* CÂU HỎI NHANH (QUICK PROMPTS ĐỒNG BỘ WINFORMS) */}
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
