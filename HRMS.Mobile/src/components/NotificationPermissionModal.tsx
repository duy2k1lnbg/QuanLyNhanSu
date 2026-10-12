import React from 'react';
import {
  Modal,
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  TouchableWithoutFeedback,
} from 'react-native';
import { useTranslation } from 'react-i18next';
import { Ionicons } from '@expo/vector-icons';
import { useTheme } from '../hooks/useTheme';
import { spacing } from '../constants/spacing';
import { typography } from '../constants/typography';

interface Props {
  visible: boolean;
  onAccept: () => void;
  onDismiss: () => void;
}

export const NotificationPermissionModal: React.FC<Props> = ({
  visible,
  onAccept,
  onDismiss,
}) => {
  const { t } = useTranslation();
  const { colors } = useTheme();

  return (
    <Modal
      visible={visible}
      transparent
      animationType="fade"
      onRequestClose={onDismiss}
    >
      <TouchableWithoutFeedback onPress={onDismiss}>
        <View style={styles.backdrop}>
          <TouchableWithoutFeedback>
            <View style={[styles.modalCard, { backgroundColor: colors.surfaceCard, borderColor: colors.border }]}>
              {/* Header Icon */}
              <View style={[styles.iconWrapper, { backgroundColor: 'rgba(30, 64, 175, 0.12)' }]}>
                <Ionicons name="notifications" size={36} color={colors.primary} />
              </View>

              {/* Title & Body */}
              <Text style={[typography.h2, styles.title, { color: colors.text }]}>
                {t('notifications.permissionTitle', 'Bật thông báo HRMS')}
              </Text>

              <Text style={[typography.body, styles.body, { color: colors.textSecondary }]}>
                {t(
                  'notifications.permissionMessage',
                  'Nhận kết quả duyệt đơn và thông báo từ công ty ngay trên điện thoại. Bạn có thể thay đổi lựa chọn trong Cài đặt.'
                )}
              </Text>

              {/* Actions */}
              <View style={styles.buttonGroup}>
                <TouchableOpacity
                  style={[styles.primaryButton, { backgroundColor: colors.primary }]}
                  onPress={onAccept}
                  activeOpacity={0.8}
                >
                  <Ionicons name="notifications-outline" size={18} color="#FFFFFF" style={{ marginRight: 6 }} />
                  <Text style={[typography.bodyBold, { color: '#FFFFFF' }]}>
                    {t('notifications.enableButton', 'Bật thông báo')}
                  </Text>
                </TouchableOpacity>

                <TouchableOpacity
                  style={[styles.secondaryButton, { borderColor: colors.border }]}
                  onPress={onDismiss}
                  activeOpacity={0.7}
                >
                  <Text style={[typography.bodyBold, { color: colors.textSecondary }]}>
                    {t('notifications.laterButton', 'Để sau')}
                  </Text>
                </TouchableOpacity>
              </View>
            </View>
          </TouchableWithoutFeedback>
        </View>
      </TouchableWithoutFeedback>
    </Modal>
  );
};

const styles = StyleSheet.create({
  backdrop: {
    flex: 1,
    backgroundColor: 'rgba(0, 0, 0, 0.55)',
    justifyContent: 'center',
    alignItems: 'center',
    padding: spacing.lg,
  },
  modalCard: {
    width: '100%',
    maxWidth: 380,
    borderRadius: spacing.borderRadiusLg,
    padding: spacing.xl,
    alignItems: 'center',
    borderWidth: 1,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.15,
    shadowRadius: 16,
    elevation: 8,
  },
  iconWrapper: {
    width: 68,
    height: 68,
    borderRadius: 34,
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: spacing.md,
  },
  title: {
    textAlign: 'center',
    marginBottom: spacing.sm,
  },
  body: {
    textAlign: 'center',
    lineHeight: 22,
    marginBottom: spacing.xl,
  },
  buttonGroup: {
    width: '100%',
    gap: spacing.sm,
  },
  primaryButton: {
    height: 48,
    borderRadius: spacing.borderRadiusLg,
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
  },
  secondaryButton: {
    height: 44,
    borderRadius: spacing.borderRadiusLg,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1,
  },
});
