import { ConfigProvider, Modal } from 'antd';
import type { ModalProps } from 'antd';
import { useAppTheme } from '../../../theme/ThemeContext';
import '../LandingModal.css';

export function LandingModal({ className = '', ...props }: ModalProps) {
  const { isDark } = useAppTheme();
  const palette = isDark
    ? { surface: '#1c2d23', field: '#16251c', text: '#f4eedc', muted: '#c8d3c2', border: '#526658' }
    : { surface: '#f8f5eb', field: '#fffdf6', text: '#26382c', muted: '#55664f', border: '#c3ccbb' };
  return (
    <ConfigProvider theme={{
      token: {
        colorPrimary: '#456650', colorText: palette.text, colorTextSecondary: palette.muted,
        colorTextPlaceholder: palette.muted, colorBgContainer: palette.field,
        colorBorder: palette.border, borderRadius: 10, controlHeightLG: 48,
        fontFamily: "'Plus Jakarta Sans', 'Segoe UI', sans-serif",
      },
      components: {
        Modal: { contentBg: palette.surface, headerBg: palette.surface, titleColor: palette.text },
        Button: { primaryColor: '#fffaf0', primaryShadow: 'none' },
      },
    }}>
      <Modal {...props} centered destroyOnHidden
        className={'landing-modal landing-modal-' + (isDark ? 'dark' : 'light') + ' ' + className}
      />
    </ConfigProvider>
  );
}
