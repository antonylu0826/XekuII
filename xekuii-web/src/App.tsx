import { useEffect, useMemo } from "react";
import {
  createRouter,
  createRoute,
  createRootRoute,
  RouterProvider,
  redirect,
  Outlet,
} from "@tanstack/react-router";
import { QueryClientProvider } from "@tanstack/react-query";
import { TooltipProvider } from "@/components/ui/tooltip";
import { queryClient } from "@/lib/query-client";
import { useAuthStore } from "@/lib/auth";
import { usePermissionsStore } from "@/lib/permissions";
import { AppLayout } from "@/components/layout/AppLayout";
import { LoginPage } from "@/pages/LoginPage";
import { DashboardPage } from "@/pages/DashboardPage";
import { NotFoundPage } from "@/pages/NotFoundPage";
import { generatedNavItems } from "@/generated/navigation.generated";
import { generatedRoutes } from "@/generated/routes.generated";

function isAuthenticated() {
  return !!localStorage.getItem("xekuii-token");
}

const rootRoute = createRootRoute({
  component: Outlet,
});

const loginRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/login",
  component: LoginPage,
  beforeLoad: () => {
    if (isAuthenticated()) {
      throw redirect({ to: "/" });
    }
  },
});

const layoutRoute = createRoute({
  getParentRoute: () => rootRoute,
  id: "layout",
  component: () => <AppLayout navItems={generatedNavItems} />,
  beforeLoad: () => {
    if (!isAuthenticated()) {
      throw redirect({ to: "/login" });
    }
  },
});

const dashboardRoute = createRoute({
  getParentRoute: () => layoutRoute,
  path: "/",
  component: DashboardPage,
});

function buildRouter() {
  const entityRoutes = generatedRoutes.map((r) =>
    createRoute({
      getParentRoute: () => layoutRoute,
      path: r.path,
      component: r.component,
    }),
  );

  const routeTree = rootRoute.addChildren([
    loginRoute,
    layoutRoute.addChildren([dashboardRoute, ...entityRoutes]),
  ]);

  return createRouter({
    routeTree,
    defaultNotFoundComponent: NotFoundPage,
  });
}

export default function App() {
  const initialize = useAuthStore((s) => s.initialize);

  useEffect(() => {
    initialize();
    if (localStorage.getItem("xekuii-token")) {
      usePermissionsStore.getState().fetchPermissions();
    }
  }, [initialize]);

  const router = useMemo(() => buildRouter(), []);

  return (
    <QueryClientProvider client={queryClient}>
      <TooltipProvider>
        <RouterProvider router={router} />
      </TooltipProvider>
    </QueryClientProvider>
  );
}
