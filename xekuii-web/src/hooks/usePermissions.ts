import { usePermissionsStore } from "@/lib/permissions";

export function useEntityPermissions(entity: string) {
  const can = usePermissionsStore((s) => s.can);
  const loaded = usePermissionsStore((s) => s.loaded);
  return {
    canRead: can(entity, "read"),
    canCreate: can(entity, "create"),
    canUpdate: can(entity, "write"),
    canDelete: can(entity, "delete"),
    loaded,
  };
}
