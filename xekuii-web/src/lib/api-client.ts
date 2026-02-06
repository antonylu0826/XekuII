import axios from "axios";
import { toast } from "sonner";

export const apiClient = axios.create({
  baseURL: "/api",
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("xekuii-token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem("xekuii-token");
      window.location.href = "/login";
    } else if (error.response?.status === 403) {
      toast.error("您沒有執行此操作的權限");
    }
    return Promise.reject(error);
  },
);
