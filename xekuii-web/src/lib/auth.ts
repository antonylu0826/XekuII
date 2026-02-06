import { create } from "zustand";
import { apiClient } from "./api-client";

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
  },

  logout: () => {
    localStorage.removeItem("xekuii-token");
    set({ token: null, isAuthenticated: false });
  },
}));
