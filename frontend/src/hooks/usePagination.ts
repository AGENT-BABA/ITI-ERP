import { useState, useCallback } from 'react';

interface PaginationState {
  page: number;
  pageSize: number;
  searchTerm: string;
}

interface UsePaginationReturn {
  pagination: PaginationState;
  setSearchTerm: (term: string) => void;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
}

export function usePagination(defaultPageSize = 10): UsePaginationReturn {
  const [pagination, setPagination] = useState<PaginationState>({
    page: 0,
    pageSize: defaultPageSize,
    searchTerm: '',
  });

  const setSearchTerm = useCallback((term: string) => {
    setPagination((prev) => ({ ...prev, searchTerm: term, page: 0 }));
  }, []);

  const setPage = useCallback((page: number) => {
    setPagination((prev) => ({ ...prev, page }));
  }, []);

  const setPageSize = useCallback((pageSize: number) => {
    setPagination((prev) => ({ ...prev, pageSize, page: 0 }));
  }, []);

  return { pagination, setSearchTerm, setPage, setPageSize };
}
