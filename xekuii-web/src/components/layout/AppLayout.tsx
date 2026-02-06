import { Outlet } from "@tanstack/react-router";
import { Sidebar } from "./Sidebar";
import { Header } from "./Header";
import type { NavItem } from "@/lib/types";

interface AppLayoutProps {
  navItems: NavItem[];
}

export function AppLayout({ navItems }: AppLayoutProps) {
  return (
    <div className="flex h-screen overflow-hidden">
      <Sidebar navItems={navItems} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Header />
        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
