import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/api-client";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

interface ReferenceSelectProps {
  endpoint: string;
  value: string | null;
  onChange: (value: string | null) => void;
  placeholder?: string;
  disabled?: boolean;
  labelField?: string;
}

interface RefOption {
  oid: string;
  [key: string]: unknown;
}

export function ReferenceSelect({
  endpoint,
  value,
  onChange,
  placeholder = "Select...",
  disabled = false,
  labelField,
}: ReferenceSelectProps) {
  const { data: options = [], isLoading } = useQuery({
    queryKey: ["ref", endpoint],
    queryFn: async () => {
      const res = await apiClient.get<RefOption[]>(endpoint);
      return res.data;
    },
  });

  function getLabel(item: RefOption): string {
    if (labelField && item[labelField]) return String(item[labelField]);
    if (item.name) return String(item.name);
    if (item.code) return String(item.code);
    return String(item.oid);
  }

  return (
    <Select
      value={value ?? ""}
      onValueChange={(v) => onChange(v === "" ? null : v)}
      disabled={disabled || isLoading}
    >
      <SelectTrigger className="w-full">
        <SelectValue placeholder={isLoading ? "Loading..." : placeholder} />
      </SelectTrigger>
      <SelectContent>
        {options.map((item) => (
          <SelectItem key={item.oid} value={item.oid}>
            {getLabel(item)}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
