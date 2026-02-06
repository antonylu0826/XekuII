import { useAuthStore } from "@/lib/auth";

export function useAuth() {
  const { isAuthenticated, login, logout } = useAuthStore();
  return { isAuthenticated, login, logout };
}
