import type { ReactNode } from "react";
import { useForm, type DefaultValues, type FieldValues, type Path } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";

export interface FieldConfig {
  name: string;
  label: string;
  type: "text" | "number" | "boolean" | "textarea" | "select" | "date" | "custom";
  placeholder?: string;
  render?: (field: { value: unknown; onChange: (v: unknown) => void }, methods: { setValue: (name: string, value: unknown) => void; getValues: (name: string) => unknown }) => ReactNode;
}

export interface FormRowConfig {
  fields: string[];
}

interface EntityFormProps {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  schema: any;
  defaultValues: DefaultValues<FieldValues>;
  fields: FieldConfig[];
  layout?: FormRowConfig[];
  onSubmit: (data: FieldValues) => void | Promise<void>;
  loading?: boolean;
  submitLabel?: string;
  onCancel?: () => void;
  children?: ReactNode;
}

export function EntityForm({
  schema,
  defaultValues,
  fields,
  layout,
  onSubmit,
  loading = false,
  submitLabel = "Save",
  onCancel,
  children,
}: EntityFormProps) {
  const form = useForm({
    resolver: zodResolver(schema),
    defaultValues,
  });

  // Watch for changes to trigger re-renders for custom/calculated fields
  form.watch();

  const fieldMap = new Map(fields.map((f) => [f.name, f]));

  function renderField(config: FieldConfig) {
    return (
      <FormField
        key={config.name}
        control={form.control}
        name={config.name as Path<FieldValues>}
        render={({ field }) => (
          <FormItem className="flex-1">
            <FormLabel>{config.label}</FormLabel>
            <FormControl>
              {config.render ? (
                <div>{config.render({ value: field.value, onChange: field.onChange }, {
                  setValue: (name, val) => form.setValue(name as Path<FieldValues>, val, { shouldValidate: true, shouldDirty: true }),
                  getValues: (name) => form.getValues(name as Path<FieldValues>)
                })}</div>
              ) : config.type === "boolean" ? (
                <div className="flex items-center gap-2 pt-1">
                  <Switch
                    checked={field.value as boolean}
                    onCheckedChange={field.onChange}
                  />
                </div>
              ) : config.type === "textarea" ? (
                <Textarea
                  {...field}
                  value={field.value as string}
                  placeholder={config.placeholder}
                />
              ) : config.type === "number" ? (
                <Input
                  type="number"
                  step="any"
                  {...field}
                  value={field.value as number}
                  onChange={(e) => field.onChange(e.target.valueAsNumber || 0)}
                  placeholder={config.placeholder}
                />
              ) : config.type === "date" ? (
                <Input
                  type="datetime-local"
                  {...field}
                  value={field.value as string}
                  placeholder={config.placeholder}
                />
              ) : (
                <Input
                  {...field}
                  value={field.value as string}
                  placeholder={config.placeholder}
                />
              )}
            </FormControl>
            <FormMessage />
          </FormItem>
        )}
      />
    );
  }

  const rows = layout ?? fields.map((f) => ({ fields: [f.name] }));

  return (
    <Form {...form}>
      <form onSubmit={(e) => { e.stopPropagation(); form.handleSubmit(onSubmit)(e); }} className="space-y-4">
        {rows.map((row, i) => (
          <div key={i} className="flex gap-4">
            {row.fields.map((name) => {
              const cfg = fieldMap.get(name);
              return cfg ? renderField(cfg) : null;
            })}
          </div>
        ))}
        {children}
        <div className="flex gap-2 pt-4">
          <Button type="submit" disabled={loading}>
            {loading ? "Saving..." : submitLabel}
          </Button>
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
        </div>
      </form>
    </Form>
  );
}
