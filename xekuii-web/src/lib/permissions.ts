import { create } from "zustand";
import { apiClient } from "./api-client";

export interface EntityPermissions {
  read: boolean;
  create: boolean;
  write: boolean;
  delete: boolean;
  navigate: boolean;
}

interface PermissionsState {
  permissions: Record<string, EntityPermissions>;
  loaded: boolean;
  fetchPermissions: () => Promise<void>;
  clear: () => void;
  can: (entity: string, action: keyof EntityPermissions) => boolean;
}

export const usePermissionsStore = create<PermissionsState>((set, get) => ({
  permissions: {},
  loaded: false,

  fetchPermissions: async () => {
    try {
      const response = await apiClient.get<Record<string, EntityPermissions>>(
        "/Permissions/my-permissions",
      );
      set({ permissions: response.data, loaded: true });
    } catch {
      set({ permissions: {}, loaded: true });
    }
  },

  clear: () => set({ permissions: {}, loaded: false }),

  can: (entity, action) => {
    const permissions = get().permissions;
    // Try exact, then Try PascalCase (common in backend)
    const perms = permissions[entity] || permissions[entity.charAt(0).toUpperCase() + entity.slice(1)];
    if (!perms) return false;
    return perms[action] ?? false;
  },
}));
