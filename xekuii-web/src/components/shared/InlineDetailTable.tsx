import { useState } from "react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { ConfirmDialog } from "@/components/shared/ConfirmDialog";
import { EntityForm, type FieldConfig } from "@/components/shared/EntityForm";
import { Plus, Pencil, Trash2 } from "lucide-react";
import type { FieldValues } from "react-hook-form";

export interface InlineDetailItem {
  _localId: string;
  _status: "existing" | "new" | "modified" | "deleted";
  _serverId?: string;
  [key: string]: unknown;
}

export interface InlineDetailColumn {
  key: string;
  header: string;
}

interface InlineDetailTableProps {
  title: string;
  items: InlineDetailItem[];
  onItemsChange: (items: InlineDetailItem[]) => void;
  columns: InlineDetailColumn[];
  fieldConfigs: FieldConfig[];
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  schema: any;
  defaultValues: Record<string, unknown>;
}

export function InlineDetailTable({
  title,
  items,
  onItemsChange,
  columns,
  fieldConfigs,
  schema,
  defaultValues,
}: InlineDetailTableProps) {
  const [addOpen, setAddOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<string | null>(null);

  const visibleItems = items.filter((i) => i._status !== "deleted");

  function handleAdd(data: FieldValues) {
    const newItem: InlineDetailItem = {
      ...data,
      _localId: crypto.randomUUID(),
      _status: "new",
    };
    onItemsChange([...items, newItem]);
    setAddOpen(false);
  }

  function handleEdit(data: FieldValues) {
    if (!editingId) return;
    onItemsChange(
      items.map((item) => {
        if (item._localId !== editingId) return item;
        return {
          ...item,
          ...data,
          _status: item._status === "existing" ? "modified" : item._status,
        } as InlineDetailItem;
      }),
    );
    setEditingId(null);
  }

  function handleDelete() {
    if (!deleteTarget) return;
    const target = items.find((i) => i._localId === deleteTarget);
    if (!target) return;

    if (target._status === "new") {
      // New items can be removed entirely
      onItemsChange(items.filter((i) => i._localId !== deleteTarget));
    } else {
      // Existing items are marked as deleted
      onItemsChange(
        items.map((i) =>
          i._localId === deleteTarget
            ? { ...i, _status: "deleted" as const }
            : i,
        ),
      );
    }
    setDeleteTarget(null);
  }

  const editingItem = editingId
    ? items.find((i) => i._localId === editingId)
    : null;

  // Build default values for edit form from the editing item
  function getEditDefaults(): Record<string, unknown> {
    if (!editingItem) return defaultValues;
    const defaults: Record<string, unknown> = {};
    for (const cfg of fieldConfigs) {
      defaults[cfg.name] = editingItem[cfg.name] ?? defaultValues[cfg.name];
    }
    return defaults;
  }

  return (
    <div className="mt-6 border-t pt-4">
      <div className="flex items-center justify-between mb-3">
        <h3 className="text-lg font-semibold">{title}</h3>
        <Button
          type="button"
          size="sm"
          variant="outline"
          onClick={() => setAddOpen(true)}
        >
          <Plus className="mr-2 h-4 w-4" /> Add
        </Button>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            {columns.map((col) => (
              <TableHead key={col.key}>{col.header}</TableHead>
            ))}
            <TableHead className="w-[80px]"></TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {visibleItems.length > 0 ? (
            visibleItems.map((item) => (
              <TableRow key={item._localId}>
                {columns.map((col) => (
                  <TableCell key={col.key}>
                    {String(item[col.key] ?? "-")}
                  </TableCell>
                ))}
                <TableCell>
                  <div className="flex items-center gap-1">
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8"
                      onClick={() => setEditingId(item._localId)}
                    >
                      <Pencil className="h-4 w-4" />
                    </Button>
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-destructive"
                      onClick={() => setDeleteTarget(item._localId)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </TableCell>
              </TableRow>
            ))
          ) : (
            <TableRow>
              <TableCell
                colSpan={columns.length + 1}
                className="text-center text-muted-foreground"
              >
                No items
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      {/* Add Dialog */}
      <Dialog open={addOpen} onOpenChange={setAddOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add {title}</DialogTitle>
          </DialogHeader>
          <EntityForm
            schema={schema}
            fields={fieldConfigs}
            defaultValues={defaultValues}
            onSubmit={handleAdd}
            submitLabel="Add"
            onCancel={() => setAddOpen(false)}
          />
        </DialogContent>
      </Dialog>

      {/* Edit Dialog */}
      <Dialog
        open={!!editingId}
        onOpenChange={(open) => !open && setEditingId(null)}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Edit {title}</DialogTitle>
          </DialogHeader>
          {editingItem && (
            <EntityForm
              key={editingId}
              schema={schema}
              fields={fieldConfigs}
              defaultValues={getEditDefaults()}
              onSubmit={handleEdit}
              submitLabel="Update"
              onCancel={() => setEditingId(null)}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Delete Confirm */}
      <ConfirmDialog
        open={!!deleteTarget}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        title="Delete Item"
        description="Are you sure you want to delete this item?"
        onConfirm={handleDelete}
      />
    </div>
  );
}
