export const formatCurrency = (amount: number | null | undefined, locale: string = 'vi'): string => {
  if (amount == null || isNaN(amount)) return '0 ₫';

  try {
    if (locale === 'ja') {
      return new Intl.NumberFormat('ja-JP', { style: 'currency', currency: 'VND' }).format(amount);
    }
    if (locale === 'en') {
      return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'VND' }).format(amount);
    }
    if (locale === 'zh-CN' || locale === 'zh') {
      return new Intl.NumberFormat('zh-CN', { style: 'currency', currency: 'VND' }).format(amount);
    }
    if (locale === 'ko') {
      return new Intl.NumberFormat('ko-KR', { style: 'currency', currency: 'VND' }).format(amount);
    }
    // Mặc định chuẩn tiếng Việt
    return new Intl.NumberFormat('vi-VN').format(amount) + ' ₫';
  } catch {
    return amount.toLocaleString() + ' ₫';
  }
};

export const formatDateByLocale = (dateStr: string | null | undefined, locale: string = 'vi'): string => {
  if (!dateStr || dateStr.trim() === '' || dateStr === 'N/A' || dateStr === 'Chưa ghi nhận' || dateStr === 'Chưa cập nhật') {
    return dateStr || 'Chưa cập nhật';
  }

  try {
    // Trường hợp đã là dạng dd/MM/yyyy
    const dmyRegex = /^(\d{1,2})\/(\d{1,2})\/(\d{4})/;
    const dmyMatch = dateStr.match(dmyRegex);
    if (dmyMatch) {
      const day = dmyMatch[1].padStart(2, '0');
      const month = dmyMatch[2].padStart(2, '0');
      const year = dmyMatch[3];

      if (locale === 'ja' || locale === 'zh-CN' || locale === 'zh') {
        return `${year}/${month}/${day}`;
      }
      if (locale === 'ko') {
        return `${year}.${month}.${day}`;
      }
      if (locale === 'en') {
        return `${month}/${day}/${year}`;
      }
      return `${day}/${month}/${year}`;
    }

    const d = new Date(dateStr);
    if (isNaN(d.getTime())) return dateStr;

    const day = d.getDate().toString().padStart(2, '0');
    const month = (d.getMonth() + 1).toString().padStart(2, '0');
    const year = d.getFullYear();

    if (locale === 'ja' || locale === 'zh-CN' || locale === 'zh') {
      return `${year}/${month}/${day}`;
    }
    if (locale === 'ko') {
      return `${year}.${month}.${day}`;
    }
    if (locale === 'en') {
      return `${month}/${day}/${year}`;
    }
    return `${day}/${month}/${year}`;
  } catch {
    return dateStr;
  }
};

export const formatDate = formatDateByLocale;
