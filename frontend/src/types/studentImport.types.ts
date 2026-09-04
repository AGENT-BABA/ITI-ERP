export interface StudentImportRow {
  rowNumber: number;
  applicationIdDisplay: string;
  candidateName: string;
  gender: string;
  dob: string;
  mobileNo: string;
  allottedCategory: string;
  allottedRound: string;
  admittedDateTime: string;
}

export interface StudentImportValidationError {
  rowNumber: number;
  field: string;
  errorMessage: string;
}

export interface StudentImportPreview {
  totalRows: number;
  validRows: number;
  invalidRows: number;
  rows: StudentImportRow[];
  errors: StudentImportValidationError[];
  fileName?: string;
  tradeId: string;
  tradeName: string;
  instituteName: string;
  sessionYear: string;
  batchId?: string;
  batchName?: string;
}

export interface StudentImportResult {
  success: boolean;
  studentsImported: number;
  message: string;
  errors: string[];
}

export interface StudentImportHistory {
  id: string;
  fileName: string;
  importedByName: string;
  importDate: string;
  numberOfStudentsImported: number;
  status: number;
  validationErrors?: string;
  tradeName: string;
}
