import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { UserSession, LoginRequest } from '../types/auth';
import { authApi } from '../api/authApi';
import { meApi } from '../api/meApi';
import { SecureTokenStorage } from '../utils/storage';
import { setUnauthorizedHandler } from '../api/client';

interface AuthContextType {
  user: UserSession | null;
  token: string | null;
  isLoading: boolean;
  isInitializing: boolean;
  isAuthenticated: boolean;
  login: (credentials: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<UserSession | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isInitializing, setIsInitializing] = useState(true);

  const handleLogout = useCallback(async () => {
    try {
      await authApi.logout();
    } catch (e) {
      console.warn('[AuthContext] Error calling server logout:', e);
    }
    try {
      await SecureTokenStorage.removeToken();
    } catch (e) {
      console.warn('[AuthContext] Error removing token on logout:', e);
    }
    setToken(null);
    setUser(null);
  }, []);

  const refreshUser = useCallback(async () => {
    try {
      const me = await meApi.getMe();
      setUser(me);
    } catch (e) {
      console.warn('[AuthContext] refreshUser error:', e);
      throw e;
    }
  }, []);

  // Initialize session from SecureStore
  useEffect(() => {
    // Register 401 callback
    setUnauthorizedHandler(() => {
      handleLogout();
    });

    const initAuth = async () => {
      try {
        const storedToken = await SecureTokenStorage.getToken();
        if (storedToken) {
          setToken(storedToken);
          try {
            const me = await meApi.getMe();
            setUser(me);
          } catch (apiError: any) {
            // If 401, remove token; if network error, keep token so retry/offline works
            if (apiError.response && apiError.response.status === 401) {
              await SecureTokenStorage.removeToken();
              setToken(null);
              setUser(null);
            } else {
              console.log('[AuthContext] Network or server error during session restore:', apiError.message);
            }
          }
        }
      } catch (e) {
        console.warn('[AuthContext] initAuth error:', e);
      } finally {
        setIsInitializing(false);
      }
    };

    initAuth();
  }, [handleLogout]);

  const login = async (credentials: LoginRequest) => {
    setIsLoading(true);
    try {
      const response = await authApi.login(credentials);
      if (response && response.token) {
        await SecureTokenStorage.saveToken(response.token);
        setToken(response.token);

        // Fetch fresh user profile details
        try {
          const me = await meApi.getMe();
          setUser(me);
        } catch {
          // Fallback to login response user if me fails
          setUser(response.user as UserSession);
        }
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isLoading,
        isInitializing,
        isAuthenticated: !!token && !!user,
        login,
        logout: handleLogout,
        refreshUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
