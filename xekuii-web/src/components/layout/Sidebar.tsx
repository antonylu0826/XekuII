import { Link, useLocation } from "@tanstack/react-router";
import { cn } from "@/lib/utils";
import type { NavItem } from "@/lib/types";
import {
  Package,
  Folder,
  ShoppingCart,
  FileText,
  Users,
  BarChart,
  Settings,
  Calendar,
  CheckSquare,
  StickyNote,
  File,
  LayoutDashboard,
} from "lucide-react";
import type { LucideIcon } from "lucide-react";

const iconMap: Record<string, LucideIcon> = {
  package: Package,
  folder: Folder,
  "shopping-cart": ShoppingCart,
  "file-text": FileText,
  users: Users,
  "bar-chart": BarChart,
  settings: Settings,
  calendar: Calendar,
  "check-square": CheckSquare,
  "sticky-note": StickyNote,
  file: File,
  dashboard: LayoutDashboard,
};

function getIcon(name: string): LucideIcon {
  return iconMap[name] ?? File;
}

interface SidebarProps {
  navItems: NavItem[];
}

export function Sidebar({ navItems }: SidebarProps) {
  const location = useLocation();

  return (
    <aside className="flex h-full w-60 flex-col border-r bg-sidebar text-sidebar-foreground">
      <div className="flex h-14 items-center border-b px-4">
        <Link to="/" className="flex items-center gap-2 font-semibold">
          <Package className="h-5 w-5" />
          <span>XekuII</span>
        </Link>
      </div>
      <nav className="flex-1 overflow-y-auto p-2">
        <div className="mb-2">
          <Link
            to="/"
            className={cn(
              "flex items-center gap-3 rounded-md px-3 py-2 text-sm transition-colors",
              location.pathname === "/"
                ? "bg-sidebar-accent text-sidebar-accent-foreground"
                : "text-sidebar-foreground/70 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
            )}
          >
            <LayoutDashboard className="h-4 w-4" />
            Dashboard
          </Link>
        </div>
        {navItems.length > 0 && (
          <>
            <div className="mb-1 mt-4 px-3 text-xs font-medium text-sidebar-foreground/50">
              Entities
            </div>
            {navItems.map((item) => {
              const Icon = getIcon(item.icon);
              const isActive = location.pathname.startsWith(item.path);
              return (
                <Link
                  key={item.path}
                  to={item.path}
                  className={cn(
                    "flex items-center gap-3 rounded-md px-3 py-2 text-sm transition-colors",
                    isActive
                      ? "bg-sidebar-accent text-sidebar-accent-foreground"
                      : "text-sidebar-foreground/70 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
                  )}
                >
                  <Icon className="h-4 w-4" />
                  {item.label}
                </Link>
              );
            })}
          </>
        )}
      </nav>
    </aside>
  );
}
