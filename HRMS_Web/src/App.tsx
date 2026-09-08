import { useState } from 'react';
import {
  Layout,
  Menu,
  Typography,
  Card,
  Row,
  Col,
  Statistic,
  Table,
  Tag,
  Avatar,
  Space,
  Button,
  Input,
  Drawer,
  Badge,
  Tooltip,
  theme,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import {
  DashboardOutlined,
  TeamOutlined,
  DollarOutlined,
  CalendarOutlined,
  FileTextOutlined,
  RobotOutlined,
  BellOutlined,
  UserOutlined,
  SendOutlined,
  SearchOutlined,
  PlusOutlined,
  CheckCircleOutlined,
} from '@ant-design/icons';
import {
  ResponsiveContainer,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip as RechartsTooltip,
  Legend,
  PieChart,
  Pie,
  Cell,
} from 'recharts';
import type { NhanVienDTO, DashboardLuongDTO, DashboardPhongBanDTO, AIChatMessage } from './types/hrms';

const { Header, Content, Sider } = Layout;
const { Title, Text } = Typography;

// Dữ liệu mẫu khớp với DTO C#
const sampleLuongData: DashboardLuongDTO[] = [
  { KyCong: 'T10/2025', TongLuong: 1720000000 },
  { KyCong: 'T11/2025', TongLuong: 1780000000 },
  { KyCong: 'T12/2025', TongLuong: 2150000000 },
  { KyCong: 'T01/2026', TongLuong: 1820000000 },
  { KyCong: 'T02/2026', TongLuong: 1890000000 },
  { KyCong: 'T03/2026', TongLuong: 1950000000 },
];

const samplePhongBanData: DashboardPhongBanDTO[] = [
  { PhongBan: 'Phòng Kỹ thuật', SoLuong: 45 },
  { PhongBan: 'Phòng Kinh doanh', SoLuong: 38 },
  { PhongBan: 'Phòng Kế toán', SoLuong: 18 },
  { PhongBan: 'Phòng Nhân sự', SoLuong: 12 },
  { PhongBan: 'Phòng Marketing', SoLuong: 19 },
  { PhongBan: 'Ban Giám đốc', SoLuong: 10 },
];

const PIE_COLORS = ['#1677ff', '#52c41a', '#fa8c16', '#722ed1', '#13c2c2', '#eb2f96'];

const sampleNhanVien: NhanVienDTO[] = [
  {
    MANV: 101,
    HOTEN: 'Nguyễn Văn An',
    GIOITINH: 'Nam',
    TENPB: 'Phòng Kỹ thuật',
    TENCV: 'Trưởng phòng IT',
    DIENTHOAI: '0988123456',
    TRANGTHAI: true,
  },
  {
    MANV: 102,
    HOTEN: 'Trần Thị Mai',
    GIOITINH: 'Nữ',
    TENPB: 'Phòng Nhân sự',
    TENCV: 'Chuyên viên Tuyển dụng',
    DIENTHOAI: '0912345678',
    TRANGTHAI: true,
  },
  {
    MANV: 103,
    HOTEN: 'Lê Hoàng Nam',
    GIOITINH: 'Nam',
    TENPB: 'Phòng Kinh doanh',
    TENCV: 'Trưởng nhóm Bán hàng',
    DIENTHOAI: '0977654321',
    TRANGTHAI: true,
  },
  {
    MANV: 104,
    HOTEN: 'Phạm Hồng Nhung',
    GIOITINH: 'Nữ',
    TENPB: 'Phòng Kế toán',
    TENCV: 'Kế toán Tiền lương',
    DIENTHOAI: '0933456789',
    TRANGTHAI: true,
  },
  {
    MANV: 105,
    HOTEN: 'Vũ Đức Thịnh',
    GIOITINH: 'Nam',
    TENPB: 'Phòng Kỹ thuật',
    TENCV: 'Kỹ sư Phần mềm',
    DIENTHOAI: '0966987123',
    TRANGTHAI: true,
  },
];

export function App() {
  const [collapsed, setCollapsed] = useState(false);
  const [currentMenu, setCurrentMenu] = useState('dashboard');
  const [aiDrawerVisible, setAiDrawerVisible] = useState(false);
  const [chatInput, setChatInput] = useState('');
  const [chatMessages, setChatMessages] = useState<AIChatMessage[]>([
    {
      id: '1',
      sender: 'assistant',
      content: 'Xin chào! Tôi là Trợ lý AI HRMS Copilot (kết nối trực tiếp với lõi Ollama & Oracle HR). Bạn cần tra cứu luật lao động, chính sách nhân sự hay số liệu phòng ban nào?',
      timestamp: 'Vừa xong',
    },
  ]);

  const {
    token: { colorBgContainer, borderRadiusLG },
  } = theme.useToken();

  const handleSendMessage = () => {
    if (!chatInput.trim()) return;

    const userMsg: AIChatMessage = {
      id: Date.now().toString(),
      sender: 'user',
      content: chatInput,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
    };

    setChatMessages((prev) => [...prev, userMsg]);
    const currentQuestion = chatInput;
    setChatInput('');

    // Phản hồi giả lập AI Copilot (Sẵn sàng kết nối qua HRMS_API)
    setTimeout(() => {
      let botReply = `Tôi đã nhận được câu hỏi: "${currentQuestion}". API Backend HRMS_API đã được kết nối với Oracle DB và mô hình Qwen 2.5 sẵn sàng phân tích dữ liệu chuyên sâu cho bạn!`;
      if (currentQuestion.toLowerCase().includes('lương')) {
        botReply = 'Theo quy định hiện hành, kỳ công tính từ ngày 1 đến ngày cuối tháng. Lương OT ngày thường tính 150%, ngày nghỉ 200%, ngày lễ tết 300% lương cơ bản.';
      } else if (currentQuestion.toLowerCase().includes('nhân sự') || currentQuestion.toLowerCase().includes('kỹ thuật')) {
        botReply = 'Hiện tại Phòng Kỹ thuật có 45 nhân sự, gồm 1 Trưởng phòng, 2 Phó phòng và 42 Kỹ sư phần mềm/hệ thống.';
      }

      setChatMessages((prev) => [
        ...prev,
        {
          id: (Date.now() + 1).toString(),
          sender: 'assistant',
          content: botReply,
          timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
        },
      ]);
    }, 600);
  };

  const columns: ColumnsType<NhanVienDTO> = [
    {
      title: 'Mã NV',
      dataIndex: 'MANV',
      key: 'MANV',
      width: 90,
      render: (id: number) => <Tag color="blue">#{id}</Tag>,
    },
    {
      title: 'Họ và tên',
      dataIndex: 'HOTEN',
      key: 'HOTEN',
      render: (text: string, record) => (
        <Space>
          <Avatar style={{ backgroundColor: record.GIOITINH === 'Nam' ? '#1677ff' : '#eb2f96' }}>
            {text.charAt(0)}
          </Avatar>
          <Text strong>{text}</Text>
        </Space>
      ),
    },
    {
      title: 'Giới tính',
      dataIndex: 'GIOITINH',
      key: 'GIOITINH',
      width: 100,
    },
    {
      title: 'Phòng ban',
      dataIndex: 'TENPB',
      key: 'TENPB',
    },
    {
      title: 'Chức vụ',
      dataIndex: 'TENCV',
      key: 'TENCV',
    },
    {
      title: 'Điện thoại',
      dataIndex: 'DIENTHOAI',
      key: 'DIENTHOAI',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'TRANGTHAI',
      key: 'TRANGTHAI',
      width: 130,
      render: (status: boolean) => (
        <Tag icon={<CheckCircleOutlined />} color={status ? 'success' : 'default'}>
          {status ? 'Đang làm việc' : 'Đã nghỉ việc'}
        </Tag>
      ),
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      {/* SIDEBAR NAVIGATION */}
      <Sider
        collapsible
        collapsed={collapsed}
        onCollapse={(val) => setCollapsed(val)}
        width={250}
        theme="dark"
        style={{
          boxShadow: '2px 0 8px 0 rgba(29,35,41,.05)',
        }}
      >
        <div
          style={{
            height: 56,
            margin: '12px 16px',
            background: 'linear-gradient(135deg, #1677ff 0%, #0958d9 100%)',
            borderRadius: 8,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#fff',
            fontWeight: 'bold',
            fontSize: collapsed ? '14px' : '17px',
            letterSpacing: '0.5px',
          }}
        >
          {collapsed ? 'HR' : '⚡ HRMS PORTAL'}
        </div>

        <Menu
          theme="dark"
          selectedKeys={[currentMenu]}
          mode="inline"
          onClick={(e) => {
            if (e.key === 'aichat') {
              setAiDrawerVisible(true);
            } else {
              setCurrentMenu(e.key);
            }
          }}
          items={[
            { key: 'dashboard', icon: <DashboardOutlined />, label: 'Bảng điều khiển' },
            { key: 'nhanvien', icon: <TeamOutlined />, label: 'Quản lý Nhân sự' },
            { key: 'chamcong', icon: <CalendarOutlined />, label: 'Chấm công & Ca làm' },
            { key: 'bangluong', icon: <DollarOutlined />, label: 'Tính lương & Thuế' },
            { key: 'hopdong', icon: <FileTextOutlined />, label: 'Hợp đồng lao động' },
            {
              key: 'aichat',
              icon: <RobotOutlined style={{ color: '#52c41a' }} />,
              label: (
                <span>
                  AI Copilot <Tag color="purple" style={{ marginLeft: 4, fontSize: 10 }}>Ollama</Tag>
                </span>
              ),
            },
          ]}
        />
      </Sider>

      <Layout>
        {/* TOP HEADER */}
        <Header
          style={{
            padding: '0 24px',
            background: colorBgContainer,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: '1px solid #f0f0f0',
            position: 'sticky',
            top: 0,
            zIndex: 10,
          }}
        >
          <Title level={4} style={{ margin: 0, color: '#1f1f1f', fontWeight: 600 }}>
            {currentMenu === 'dashboard' && '📊 TỔNG QUAN HỆ THỐNG NHÂN SỰ'}
            {currentMenu === 'nhanvien' && '👥 QUẢN LÝ HỒ SƠ NHÂN VIÊN'}
            {currentMenu === 'chamcong' && '🕒 CHẤM CÔNG & QUẢN LÝ CA'}
            {currentMenu === 'bangluong' && '💰 BẢNG LƯƠNG & QUỸ LƯƠNG'}
            {currentMenu === 'hopdong' && '📜 HỢP ĐỒNG LAO ĐỘNG'}
          </Title>

          <Space orientation="horizontal" size="middle">
            <Button
              type="primary"
              icon={<RobotOutlined />}
              onClick={() => setAiDrawerVisible(true)}
              style={{
                background: 'linear-gradient(135deg, #722ed1 0%, #1677ff 100%)',
                border: 'none',
              }}
            >
              Hỏi AI Copilot
            </Button>

            <Tooltip title="Thông báo hệ thống">
              <Badge count={3} size="small">
                <Button shape="circle" icon={<BellOutlined />} />
              </Badge>
            </Tooltip>

            <Space style={{ marginLeft: 8 }}>
              <Avatar style={{ backgroundColor: '#1677ff' }} icon={<UserOutlined />} />
              <div style={{ display: 'flex', flexDirection: 'column', lineHeight: 1.2 }}>
                <Text strong style={{ fontSize: 13 }}>Admin HR</Text>
                <Text type="secondary" style={{ fontSize: 11 }}>Quản trị viên</Text>
              </div>
            </Space>
          </Space>
        </Header>

        {/* MAIN CONTENT AREA */}
        <Content style={{ margin: '20px 24px', minHeight: 400 }}>
          {currentMenu === 'dashboard' && (
            <Space direction="vertical" size="large" style={{ width: '100%' }}>
              {/* 4 KPI CARDS */}
              <Row gutter={[16, 16]}>
                <Col xs={24} sm={12} lg={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}>
                    <Statistic
                      title={<Text strong type="secondary">Tổng số Nhân viên</Text>}
                      value={142}
                      prefix={<TeamOutlined style={{ color: '#1677ff', marginRight: 8 }} />}
                      suffix={<Tag color="blue" style={{ marginLeft: 8 }}>+4 mới</Tag>}
                      valueStyle={{ color: '#1677ff', fontWeight: 700 }}
                    />
                  </Card>
                </Col>

                <Col xs={24} sm={12} lg={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}>
                    <Statistic
                      title={<Text strong type="secondary">Đi làm hôm nay</Text>}
                      value={138}
                      suffix={<span style={{ fontSize: 14, color: '#8c8c8c' }}>/ 142 (97.2%)</span>}
                      prefix={<CalendarOutlined style={{ color: '#52c41a', marginRight: 8 }} />}
                      valueStyle={{ color: '#52c41a', fontWeight: 700 }}
                    />
                  </Card>
                </Col>

                <Col xs={24} sm={12} lg={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}>
                    <Statistic
                      title={<Text strong type="secondary">Tổng Quỹ Lương Tháng</Text>}
                      value={1950000000}
                      prefix={<DollarOutlined style={{ color: '#fa8c16', marginRight: 8 }} />}
                      formatter={(v) => `${Number(v).toLocaleString('vi-VN')} đ`}
                      valueStyle={{ color: '#fa8c16', fontWeight: 700, fontSize: '1.3rem' }}
                    />
                  </Card>
                </Col>

                <Col xs={24} sm={12} lg={6}>
                  <Card bordered={false} style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}>
                    <Statistic
                      title={<Text strong type="secondary">AI HRMS Copilot</Text>}
                      value="Online"
                      prefix={<RobotOutlined style={{ color: '#722ed1', marginRight: 8 }} />}
                      suffix={<Tag color="purple">Qwen 2.5 RAG</Tag>}
                      valueStyle={{ color: '#722ed1', fontWeight: 700 }}
                    />
                  </Card>
                </Col>
              </Row>

              {/* CHARTS ROW */}
              <Row gutter={[16, 16]}>
                <Col xs={24} lg={15}>
                  <Card
                    title="📈 Biến động Quỹ lương 6 tháng qua (VNĐ)"
                    bordered={false}
                    style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}
                  >
                    <div style={{ width: '100%', height: 300 }}>
                      <ResponsiveContainer>
                        <BarChart data={sampleLuongData} margin={{ top: 10, right: 30, left: 20, bottom: 5 }}>
                          <XAxis dataKey="KyCong" />
                          <YAxis tickFormatter={(val) => `${val / 1000000}M`} />
                          <RechartsTooltip
                            formatter={(val: any) => [`${Number(val).toLocaleString('vi-VN')} đ`, 'Quỹ Lương']}
                          />
                          <Legend />
                          <Bar dataKey="TongLuong" fill="#1677ff" name="Tổng chi lương" radius={[6, 6, 0, 0]} />
                        </BarChart>
                      </ResponsiveContainer>
                    </div>
                  </Card>
                </Col>

                <Col xs={24} lg={9}>
                  <Card
                    title="🏢 Phân bổ Nhân sự theo Phòng ban"
                    bordered={false}
                    style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}
                  >
                    <div style={{ width: '100%', height: 300 }}>
                      <ResponsiveContainer>
                        <PieChart>
                          <Pie
                            data={samplePhongBanData}
                            dataKey="SoLuong"
                            nameKey="PhongBan"
                            cx="50%"
                            cy="50%"
                            outerRadius={95}
                            label={({ name, percent }) => `${name} (${((percent || 0) * 100).toFixed(0)}%)`}
                          >
                            {samplePhongBanData.map((_, index) => (
                              <Cell key={`cell-${index}`} fill={PIE_COLORS[index % PIE_COLORS.length]} />
                            ))}
                          </Pie>
                          <RechartsTooltip />
                        </PieChart>
                      </ResponsiveContainer>
                    </div>
                  </Card>
                </Col>
              </Row>

              {/* RECENT EMPLOYEES TABLE */}
              <Card
                title="👥 Danh sách Nhân sự Gần đây"
                extra={
                  <Button type="link" onClick={() => setCurrentMenu('nhanvien')}>
                    Xem tất cả &gt;
                  </Button>
                }
                bordered={false}
                style={{ borderRadius: borderRadiusLG, boxShadow: '0 1px 4px rgba(0,0,0,0.05)' }}
              >
                <Table
                  columns={columns}
                  dataSource={sampleNhanVien}
                  rowKey="MANV"
                  pagination={false}
                  size="middle"
                />
              </Card>
            </Space>
          )}

          {currentMenu === 'nhanvien' && (
            <Card
              title="Danh sách Hồ sơ Nhân viên"
              extra={
                <Space>
                  <Input
                    placeholder="Tìm theo tên, mã NV..."
                    prefix={<SearchOutlined />}
                    style={{ width: 250 }}
                  />
                  <Button type="primary" icon={<PlusOutlined />}>
                    Thêm nhân viên mới
                  </Button>
                </Space>
              }
              bordered={false}
              style={{ borderRadius: borderRadiusLG }}
            >
              <Table
                columns={columns}
                dataSource={sampleNhanVien}
                rowKey="MANV"
                pagination={{ pageSize: 5 }}
              />
            </Card>
          )}

          {['chamcong', 'bangluong', 'hopdong'].includes(currentMenu) && (
            <Card style={{ borderRadius: borderRadiusLG, textAlign: 'center', padding: 60 }}>
              <Title level={4}>Tính năng đang kết nối dữ liệu từ backend HRMS_API</Title>
              <Text type="secondary">
                Dữ liệu sẽ được tự động đồng bộ từ Database Oracle qua các API Controller tương ứng.
              </Text>
            </Card>
          )}
        </Content>
      </Layout>

      {/* AI COPILOT CHAT DRAWER */}
      <Drawer
        title={
          <Space>
            <Avatar style={{ backgroundColor: '#722ed1' }} icon={<RobotOutlined />} />
            <div>
              <Text strong>AI HRMS Copilot</Text>
              <div>
                <Tag color="green" style={{ fontSize: 10 }}>Ollama • Qwen 2.5 RAG</Tag>
              </div>
            </div>
          </Space>
        }
        placement="right"
        width={420}
        onClose={() => setAiDrawerVisible(false)}
        open={aiDrawerVisible}
      >
        <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
          {/* Messages list */}
          <div style={{ flex: 1, overflowY: 'auto', paddingRight: 4, display: 'flex', flexDirection: 'column', gap: 12 }}>
            {chatMessages.map((msg) => (
              <div
                key={msg.id}
                style={{
                  alignSelf: msg.sender === 'user' ? 'flex-end' : 'flex-start',
                  maxWidth: '85%',
                  background: msg.sender === 'user' ? '#1677ff' : '#f0f2f5',
                  color: msg.sender === 'user' ? '#fff' : '#1f1f1f',
                  padding: '10px 14px',
                  borderRadius: msg.sender === 'user' ? '12px 12px 2px 12px' : '12px 12px 12px 2px',
                  fontSize: '13.5px',
                  lineHeight: '1.4',
                }}
              >
                <div>{msg.content}</div>
                <div
                  style={{
                    fontSize: '10px',
                    color: msg.sender === 'user' ? 'rgba(255,255,255,0.7)' : '#8c8c8c',
                    marginTop: 4,
                    textAlign: 'right',
                  }}
                >
                  {msg.timestamp}
                </div>
              </div>
            ))}
          </div>

          {/* Prompt Suggestions */}
          <div style={{ margin: '12px 0' }}>
            <Text type="secondary" style={{ fontSize: 11 }}>Gợi ý nhanh:</Text>
            <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap', marginTop: 4 }}>
              <Tag
                style={{ cursor: 'pointer' }}
                onClick={() => setChatInput('Quy định tính tiền làm thêm giờ (OT)?')}
              >
                💰 Quy định làm thêm giờ
              </Tag>
              <Tag
                style={{ cursor: 'pointer' }}
                onClick={() => setChatInput('Phòng Kỹ thuật hiện có bao nhiêu nhân viên?')}
              >
                👥 Nhân sự Phòng Kỹ thuật
              </Tag>
            </div>
          </div>

          {/* Input & Send */}
          <div style={{ display: 'flex', gap: 8, borderTop: '1px solid #f0f0f0', paddingTop: 12 }}>
            <Input.TextArea
              rows={2}
              value={chatInput}
              onChange={(e) => setChatInput(e.target.value)}
              onPressEnter={(e) => {
                if (!e.shiftKey) {
                  e.preventDefault();
                  handleSendMessage();
                }
              }}
              placeholder="Nhập câu hỏi cho AI Copilot..."
            />
            <Button
              type="primary"
              icon={<SendOutlined />}
              onClick={handleSendMessage}
              style={{ height: 'auto', background: '#722ed1' }}
            />
          </div>
        </div>
      </Drawer>
    </Layout>
  );
}

export default App;
