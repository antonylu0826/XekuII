import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { LayoutDashboard } from "lucide-react";

export function DashboardPage() {
  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold">Dashboard</h1>
      <Card>
        <CardHeader>
          <div className="flex items-center gap-2">
            <LayoutDashboard className="h-5 w-5" />
            <CardTitle>Welcome to XekuII</CardTitle>
          </div>
          <CardDescription>
            Define entities in YAML, generate code, and manage your data.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            Use the sidebar to navigate to entity pages. Entity pages are
            auto-generated from YAML definitions.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
