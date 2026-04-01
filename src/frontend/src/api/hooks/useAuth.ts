import { useQuery, useMutation } from '@tanstack/react-query';
import { apiFetch } from '../client';
import { queryClient } from '../../lib/query-client';

export interface UserInfo {
  id: string;
  email: string;
  displayName: string;
}

export interface AuthResponse {
  token: string;
  user: UserInfo;
}

export function useLogin() {
  return useMutation({
    mutationFn: (data: { email: string; password: string }) =>
      apiFetch<AuthResponse>('/auth/login', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
    onSuccess: (data) => {
      queryClient.clear();
      localStorage.setItem('auth_token', data.token);
    },
  });
}

export function useRegister() {
  return useMutation({
    mutationFn: (data: { email: string; password: string; displayName: string }) =>
      apiFetch<AuthResponse>('/auth/register', {
        method: 'POST',
        body: JSON.stringify(data),
      }),
    onSuccess: (data) => {
      queryClient.clear();
      localStorage.setItem('auth_token', data.token);
    },
  });
}

export function useCurrentUser() {
  const token = localStorage.getItem('auth_token');
  return useQuery({
    queryKey: ['auth', 'me'],
    queryFn: () => apiFetch<UserInfo>('/auth/me'),
    enabled: !!token,
    retry: false,
  });
}
