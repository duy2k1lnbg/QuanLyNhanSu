import i18n from '../i18n/i18n';

export const mapApiError = (error: any): string => {
  if (!error) return i18n.t('errors.unknown');

  // Lỗi mạng hoặc server không phản hồi
  if (error.code === 'ECONNABORTED' || error.message?.includes('timeout')) {
    return i18n.t('errors.timeout');
  }

  if (!error.response) {
    return i18n.t('errors.networkUnavailable');
  }

  const status = error.response.status;
  const data = error.response.data;

  // Lấy message từ server nếu là thông điệp thân thiện
  if (data) {
    if (typeof data === 'string' && !data.includes('<') && !data.includes('Exception') && !data.includes('ORA-')) {
      return data;
    }
    if (typeof data === 'object') {
      if (data.message && typeof data.message === 'string' && !data.message.includes('Exception') && !data.message.includes('ORA-')) {
        return data.message;
      }
      if (data.Message && typeof data.Message === 'string' && !data.Message.includes('Exception') && !data.Message.includes('ORA-')) {
        return data.Message;
      }
    }
  }

  switch (status) {
    case 400:
      return i18n.t('errors.badRequest');
    case 401:
      return i18n.t('errors.unauthorized');
    case 403:
      return i18n.t('errors.forbidden');
    case 404:
      return i18n.t('errors.notFound');
    case 408:
      return i18n.t('errors.timeout');
    case 500:
    case 502:
    case 503:
      return i18n.t('errors.serverError');
    default:
      return i18n.t('errors.unknown');
  }
};
