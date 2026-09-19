export const lightColors = {
  primary: '#1E40AF',       // Deep Royal Blue
  primaryDark: '#1E3A8A',
  primaryLight: '#3B82F6',
  accent: '#F59E0B',        // Warm Amber/Gold
  background: '#F8FAFC',    // Slate 50
  surface: '#FFFFFF',
  surfaceCard: '#FFFFFF',
  text: '#0F172A',          // Slate 900
  textSecondary: '#64748B', // Slate 500
  textMuted: '#94A3B8',     // Slate 400
  border: '#E2E8F0',        // Slate 200
  divider: '#F1F5F9',
  success: '#10B981',       // Emerald
  warning: '#F59E0B',
  danger: '#EF4444',        // Red
  info: '#0EA5E9',
  cardShadow: 'rgba(15, 23, 42, 0.06)',
};

export const darkColors = {
  primary: '#3B82F6',
  primaryDark: '#2563EB',
  primaryLight: '#60A5FA',
  accent: '#FBBF24',
  background: '#0B1329',    // Deep Midnight Blue
  surface: '#1E293B',       // Slate 800
  surfaceCard: '#1A233A',
  text: '#F8FAFC',          // Slate 50
  textSecondary: '#94A3B8', // Slate 400
  textMuted: '#64748B',
  border: '#334155',        // Slate 700
  divider: '#1E293B',
  success: '#34D399',
  warning: '#FBBF24',
  danger: '#F87171',
  info: '#38BDF8',
  cardShadow: 'rgba(0, 0, 0, 0.3)',
};

export type ThemeColors = typeof lightColors;
