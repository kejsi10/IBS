import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import React from 'react';
import {
  useDocuments,
  useDocument,
  useUploadDocument,
  useDeleteDocument,
  useDocumentTemplates,
  useGenerateCOI,
} from './use-documents';

/** Creates a fresh QueryClient and wrapper for each test. */
const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: 0 },
      mutations: { retry: false },
    },
  });
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>{children}</BrowserRouter>
    </QueryClientProvider>
  );
};

describe('useDocuments', () => {
  it('returns paginated documents on success', async () => {
    // Arrange
    const { result } = renderHook(() => useDocuments(), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data?.items).toBeDefined();
    expect(result.current.data?.items.length).toBeGreaterThan(0);
  });

  it('shows loading state initially', () => {
    // Arrange & Act
    const { result } = renderHook(() => useDocuments(), { wrapper: createWrapper() });

    // Assert
    expect(result.current.isLoading).toBe(true);
  });
});

describe('useDocument', () => {
  it('returns document detail on success', async () => {
    // Arrange
    const { result } = renderHook(
      () => useDocument('00000000-0000-0000-0000-000000000010'),
      { wrapper: createWrapper() }
    );

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data?.id).toBe('00000000-0000-0000-0000-000000000010');
    expect(result.current.data?.fileName).toBe('policy-document.pdf');
  });

  it('does not fetch when id is empty', () => {
    // Arrange & Act
    const { result } = renderHook(() => useDocument(''), { wrapper: createWrapper() });

    // Assert
    expect(result.current.fetchStatus).toBe('idle');
  });
});

describe('useUploadDocument', () => {
  it('returns mutation function', () => {
    // Arrange & Act
    const { result } = renderHook(() => useUploadDocument(), { wrapper: createWrapper() });

    // Assert
    expect(result.current.mutateAsync).toBeDefined();
    expect(result.current.isPending).toBe(false);
  });
});

describe('useDeleteDocument', () => {
  it('returns mutation function', () => {
    // Arrange & Act
    const { result } = renderHook(() => useDeleteDocument(), { wrapper: createWrapper() });

    // Assert
    expect(result.current.mutateAsync).toBeDefined();
    expect(result.current.isPending).toBe(false);
  });
});

describe('useDocumentTemplates', () => {
  it('returns paginated templates on success', async () => {
    // Arrange
    const { result } = renderHook(() => useDocumentTemplates(), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data?.items).toBeDefined();
    expect(result.current.data?.items.length).toBeGreaterThan(0);
    expect(result.current.data?.items[0].templateType).toBe('CertificateOfInsurance');
  });
});

describe('useGenerateCOI', () => {
  it('returns mutation function', () => {
    // Arrange & Act
    const { result } = renderHook(() => useGenerateCOI(), { wrapper: createWrapper() });

    // Assert
    expect(result.current.mutateAsync).toBeDefined();
    expect(result.current.isPending).toBe(false);
  });
});
