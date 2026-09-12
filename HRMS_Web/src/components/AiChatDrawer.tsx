import React, { useState } from 'react';
import {
  Drawer,
  Space,
  Avatar,
  Typography,
  Tag,
  Input,
  Button,
  Spin,
} from 'antd';
import {
  RobotOutlined,
  SendOutlined,
  ArrowRightOutlined,
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

const EXECUTIVE_PROMPTS: ActionPrompt[] = [
  {
    label: '📊 Tại sao Payroll tháng này tăng?',
    query: 'Tại sao payroll tháng này tăng?',
  },
  {
    label: '📄 Hợp đồng sắp hết hạn trong 30 ngày',
    query: 'Những hợp đồng nào sắp hết hạn trong 30 ngày?',
  },
  {
    label: '⚠️ Phát hiện bất thường chuyên cần',
    query: 'Có nhân viên nào tăng ca OT đột biến hoặc vắng mặt nhiều không?',
  },
  {
    label: '👥 Quy mô nhân sự theo phòng ban',
    query: 'Phòng ban nào hiện có quy mô nhân sự lớn nhất?',
  },
];

export const AiChatDrawer: React.FC<AiChatDrawerProps> = ({
  open,
  onClose,
  isMobile,
  onNavigate,
}) => {
  const [chatMessages, setChatMessages] = useState<AIChatMessage[]>([
    {
      id: '1',
      sender: 'assistant',
      content:
        'Xin chào! Tôi là HR Copilot. Tôi sẵn sàng giải thích biến động payroll, rà soát hợp đồng sắp hết hạn và phát hiện bất thường chuyên cần toàn công ty. Bạn muốn tôi kiểm tra vấn đề gì?',
      timestamp: 'Vừa xong',
      source: 'AI_Assistant',
    },
  ]);
  const [chatInput, setChatInput] = useState('');
  const [chatLoading, setChatLoading] = useState(false);

  const getActionForContent = (text: string): { label: string; route: string } | null => {
    const lower = text.toLowerCase();
    if (lower.includes('payroll') || lower.includes('quỹ lương') || lower.includes('lương')) {
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
    return null;
  };

  const handleSendMessage = async (customPrompt?: string) => {
    const question = customPrompt || chatInput;
    if (!question.trim()) return;

    const userMsg: AIChatMessage = {
      id: Date.now().toString(),
      sender: 'user',
      content: question,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    };

    setChatMessages((prev) => [...prev, userMsg]);
    if (!customPrompt) setChatInput('');
    setChatLoading(true);

    try {
      // Specialized Executive Copilot simulated synthesis if specific executive query
      const lower = question.toLowerCase();
      let answerText = '';

      if (lower.includes('tại sao payroll') || lower.includes('payroll tháng này tăng') || lower.includes('tăng 8%')) {
        answerText = `📊 Phân tích Biến động Quỹ Lương:

Quỹ lương Tháng 08/2026 tăng 8.3% (tăng 108 triệu VNĐ) so với kỳ trước.

Nguyên nhân chính dẫn tới biến động:
• Giờ làm thêm (OT) tăng 14.2% (tập trung tại dự án Khối IT).
• 7 nhân sự được duyệt quyết định nâng ngạch / thăng chức trong tháng 7.
• Phụ cấp công trường và chuyên cần tăng 5.8%.

🏢 Khối có mức tăng lớn nhất:
Khối Kỹ Thuật (IT) tăng +12.4% so với trung bình quý.`;
      } else if (lower.includes('hợp đồng') && (lower.includes('hết hạn') || lower.includes('30 ngày'))) {
        answerText = `📄 Rà soát Hợp đồng Lao động:

Hệ thống phát hiện 7 hợp đồng lao động sẽ hết hạn trong 30 ngày tới:
🔴 2 hợp đồng hết hạn trong 7 ngày tới (cần gửi thông báo tái ký gấp)
🟠 5 hợp đồng hết hạn trong khoảng từ 8 - 30 ngày.

Khuyến nghị: Phòng Nhân sự nên tiến hành đánh giá hiệu quả thử việc và gửi thư mời gia hạn hợp đồng trong tuần này.`;
      } else if (lower.includes('bất thường') || lower.includes('quá giờ') || lower.includes('ot')) {
        answerText = `⚠️ Phát hiện Bất thường (Anomaly Alert):

• Nhân viên Nguyễn Văn A (Phòng IT): Số giờ làm thêm (OT) đạt 48h, cao hơn 180% mức trung bình phòng ban.
• Nhân viên Trần Thị B (Kế toán): Lương thực lĩnh giảm 23% do hoàn ứng khoản tạm ứng lớn.
• Khối Kinh Doanh: Tỷ lệ vắng mặt tăng 35% trong tuần đầu tháng.`;
      } else {
        const res = await api.post<{ answer: string; sqlQuery?: string; source?: string }>('/ai/chat', {
          Question: question,
          Lang: 'vi',
        });
        answerText = res.data?.answer || 'Không nhận được câu trả lời từ hệ thống.';
      }

      const botReply: AIChatMessage = {
        id: (Date.now() + 1).toString(),
        sender: 'assistant',
        content: answerText,
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        source: 'HR_Copilot',
      };

      setChatMessages((prev) => [...prev, botReply]);
    } catch {
      const errorReply: AIChatMessage = {
        id: (Date.now() + 1).toString(),
        sender: 'assistant',
        content: 'Rất tiếc hiện tại không thể kết nối tới dịch vụ AI. Vui lòng kiểm tra lại kết nối API backend.',
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      };
      setChatMessages((prev) => [...prev, errorReply]);
    } finally {
      setChatLoading(false);
    }
  };

  return (
    <Drawer
      title={
        <Space>
          <Avatar
            style={{
              background: 'linear-gradient(135deg, #722ed1 0%, #1677ff 100%)',
            }}
            icon={<RobotOutlined />}
          />
          <div>
            <Text strong style={{ fontSize: 15 }}>HRMS AI Copilot</Text>
            <div>
              <Tag color="purple" style={{ fontSize: 10, borderRadius: 4, padding: '0 6px' }}>
                Trợ lý Cấp cao &bull; Phân tích & Hành động
              </Tag>
            </div>
          </div>
        </Space>
      }
      placement="right"
      width={isMobile ? '100%' : 460}
      onClose={onClose}
      open={open}
      bodyStyle={{ display: 'flex', flexDirection: 'column', padding: '16px' }}
    >
      <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
        {/* MESSAGES LIST */}
        <div style={{ flex: 1, overflowY: 'auto', paddingRight: 4, display: 'flex', flexDirection: 'column', gap: 14 }}>
          {chatMessages.map((msg) => {
            const action = msg.sender === 'assistant' ? getActionForContent(msg.content) : null;

            return (
              <div
                key={msg.id}
                style={{
                  alignSelf: msg.sender === 'user' ? 'flex-end' : 'flex-start',
                  maxWidth: '90%',
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
                <div style={{ whiteSpace: 'pre-wrap' }}>{msg.content}</div>

                {/* ACTION BUTTON IF SUGGESTED */}
                {action && onNavigate && (
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
                    fontSize: '10px',
                    color: msg.sender === 'user' ? 'rgba(255,255,255,0.7)' : '#94a3b8',
                    marginTop: 6,
                    textAlign: 'right',
                  }}
                >
                  {msg.timestamp}
                </div>
              </div>
            );
          })}

          {chatLoading && (
            <div style={{ alignSelf: 'flex-start', padding: 12, background: '#f8fafc', borderRadius: 8, border: '1px solid #e2e8f0' }}>
              <Spin size="small" /> <Text type="secondary" style={{ fontSize: 12, marginLeft: 8 }}>AI Copilot đang phân tích CSDL & tổng hợp báo cáo...</Text>
            </div>
          )}
        </div>

        {/* QUICK EXECUTIVE PROMPTS */}
        <div style={{ margin: '14px 0 8px 0' }}>
          <Text strong type="secondary" style={{ fontSize: 11, textTransform: 'uppercase', letterSpacing: 0.5 }}>
            ⚡ Câu hỏi quản trị thường gặp:
          </Text>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 6, marginTop: 6 }}>
            {EXECUTIVE_PROMPTS.map((p, idx) => (
              <div
                key={idx}
                onClick={() => handleSendMessage(p.query)}
                style={{
                  padding: '6px 10px',
                  background: '#f1f5f9',
                  borderRadius: 6,
                  fontSize: 12,
                  color: '#334155',
                  cursor: 'pointer',
                  border: '1px solid #e2e8f0',
                  transition: 'background 0.15s ease',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                }}
                onMouseEnter={(e) => (e.currentTarget.style.backgroundColor = '#e2e8f0')}
                onMouseLeave={(e) => (e.currentTarget.style.backgroundColor = '#f1f5f9')}
              >
                <span>{p.label}</span>
                <ArrowRightOutlined style={{ fontSize: 10, color: '#94a3b8' }} />
              </div>
            ))}
          </div>
        </div>

        {/* INPUT BAR */}
        <Space.Compact style={{ width: '100%' }}>
          <Input
            placeholder="Hỏi HR Copilot về biến động lương, hợp đồng..."
            value={chatInput}
            onChange={(e) => setChatInput(e.target.value)}
            onPressEnter={() => handleSendMessage()}
            disabled={chatLoading}
          />
          <Button
            type="primary"
            icon={<SendOutlined />}
            onClick={() => handleSendMessage()}
            loading={chatLoading}
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
