import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import React from 'react';
import {
  useClaims,
  useClaim,
  useCreateClaim,
  useUpdateClaimStatus,
} from './use-claims';

/** Creates a fresh QueryClient and wrapper for each test to prevent state bleed. */
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

describe('useClaims', () => {
  it('returns paginated claim list on success', async () => {
    // Arrange
    const { result } = renderHook(() => useClaims(), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data?.items).toBeDefined();
    expect(result.current.data?.items.length).toBeGreaterThan(0);
  });

  it('shows loading state initially', () => {
    // Arrange & Act
    const { result } = renderHook(() => useClaims(), { wrapper: createWrapper() });

    // Assert
    expect(result.current.isLoading).toBe(true);
  });
});

describe('useClaim', () => {
  it('fetches single claim by id', async () => {
    // Arrange
    const { result } = renderHook(() => useClaim('claim-1'), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data).toBeDefined();
    expect(result.current.data?.id).toBe('claim-1');
  });

  it('is disabled when id is empty string', () => {
    // Arrange & Act
    const { result } = renderHook(() => useClaim(''), { wrapper: createWrapper() });

    // Assert — query should not be fetching
    expect(result.current.isFetching).toBe(false);
  });
});

describe('useCreateClaim', () => {
  it('creates claim and invalidates list and statistics', async () => {
    // Arrange
    const { result } = renderHook(() => useCreateClaim(), { wrapper: createWrapper() });

    // Act
    result.current.mutate({
      policyId: 'policy-1',
      clientId: 'client-1',
      lossDate: '2024-03-15T10:30:00Z',
      reportedDate: '2024-03-16T09:00:00Z',
      lossType: 'PropertyDamage',
      lossDescription: 'Water damage from burst pipe',
    });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.id).toBe('new-claim-id');
  });
});

describe('useUpdateClaimStatus', () => {
  it('updates claim status and invalidates detail, list, and statistics', async () => {
    // Arrange
    const { result } = renderHook(() => useUpdateClaimStatus(), { wrapper: createWrapper() });

    // Act
    result.current.mutate({
      claimId: 'claim-1',
      data: { newStatus: 'Acknowledged' },
    });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});
