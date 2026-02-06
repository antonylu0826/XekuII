import { create } from "zustand";
import { apiClient } from "./api-client";
import { usePermissionsStore } from "./permissions";

interface AuthState {
  token: string | null;
  isAuthenticated: boolean;
  login: (userName: string, password: string) => Promise<void>;
  logout: () => void;
  initialize: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  token: null,
  isAuthenticated: false,

  initialize: () => {
    const token = localStorage.getItem("xekuii-token");
    if (token) {
      set({ token, isAuthenticated: true });
    }
  },

  login: async (userName: string, password: string) => {
    const response = await apiClient.post("/Authentication/Authenticate", {
      userName,
      password,
    });
    const token = response.data.token ?? response.data;
    localStorage.setItem("xekuii-token", token);
    set({ token, isAuthenticated: true });
    await usePermissionsStore.getState().fetchPermissions();
  },

  logout: () => {
    localStorage.removeItem("xekuii-token");
    set({ token: null, isAuthenticated: false });
    usePermissionsStore.getState().clear();
    window.location.href = "/login";
  },
}));
