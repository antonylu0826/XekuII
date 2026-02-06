import type { ReactNode } from "react";

export interface RouteComponent {
  path: string;
  component: () => ReactNode;
}
