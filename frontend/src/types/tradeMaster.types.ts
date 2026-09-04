export interface TradeMasterRow {
  rowNumber: number;
  tradeCode: string;
  tradeName: string;
  totalSeats: number;
  durationMonths: number;
}

export interface TradeMasterValidationError {
  rowNumber: number;
  field: string;
  errorMessage: string;
}

export interface TradeMasterPreview {
  totalRows: number;
  validRows: number;
  invalidRows: number;
  rows: TradeMasterRow[];
  errors: TradeMasterValidationError[];
  fileName?: string;
}

export interface TradeMasterImportResult {
  success: boolean;
  tradesImported: number;
  message: string;
  errors: string[];
}

export interface TradeMasterHistory {
  id: string;
  fileName: string;
  importedByName: string;
  importDate: string;
  numberOfTradesImported: number;
  status: number;
  validationErrors?: string;
}
