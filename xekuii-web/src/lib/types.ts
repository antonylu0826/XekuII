export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface QueryParams {
  page?: number;
  pageSize?: number;
  sort?: string;
  sortDir?: "asc" | "desc";
  search?: string;
  [key: string]: unknown;
}

export interface NavItem {
  label: string;
  path: string;
  icon: string;
  entity?: string;
}
