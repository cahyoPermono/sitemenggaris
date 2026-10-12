export type SchemaType = 'SOT_PRODUCTION' | 'SOT_LPUMA';

export interface ConversionResponse {
  id: string;
  originalFileName: string;
  customFileName: string;
  schemaType: SchemaType;
  status: 'Berhasil' | 'Gagal';
  fileSizeBytes: number;
  rowCount: number;
  createdAt: string;
  xmlContent: string;
  errorMessage?: string;
  downloadUrl: string;
}

export interface HistoryItem {
  id: string;
  createdAt: string;
  displayFileName: string;
  originalFileName: string;
  schemaType: SchemaType;
  status: 'Berhasil' | 'Gagal';
  fileSizeBytes: number;
  rowCount: number;
  errorMessage?: string;
  downloadUrl: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
