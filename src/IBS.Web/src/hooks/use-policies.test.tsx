import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import React from 'react';
import {
  usePolicies,
  usePolicy,
  useCreatePolicy,
  useBindPolicy,
  useActivatePolicy,
} from './use-policies';

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

describe('usePolicies', () => {
  it('returns paginated policy list on success', async () => {
    // Arrange
    const { result } = renderHook(() => usePolicies(), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data?.items).toBeDefined();
    expect(result.current.data?.items.length).toBeGreaterThan(0);
  });

  it('shows loading state initially', () => {
    // Arrange & Act
    const { result } = renderHook(() => usePolicies(), { wrapper: createWrapper() });

    // Assert
    expect(result.current.isLoading).toBe(true);
  });
});

describe('usePolicy', () => {
  it('fetches single policy by id', async () => {
    // Arrange
    const { result } = renderHook(() => usePolicy('policy-1'), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data).toBeDefined();
    expect(result.current.data?.id).toBe('policy-1');
  });

  it('is disabled when id is empty string', () => {
    // Arrange & Act
    const { result } = renderHook(() => usePolicy(''), { wrapper: createWrapper() });

    // Assert — query should not be fetching
    expect(result.current.isFetching).toBe(false);
  });
});

describe('useCreatePolicy', () => {
  it('creates policy and returns new id', async () => {
    // Arrange
    const { result } = renderHook(() => useCreatePolicy(), { wrapper: createWrapper() });

    // Act
    result.current.mutate({
      clientId: 'client-1',
      carrierId: 'carrier-1',
      lineOfBusiness: 'PersonalAuto',
      policyType: 'Auto Insurance',
      effectiveDate: '2024-01-01',
      expirationDate: '2025-01-01',
      billingType: 'DirectBill',
      paymentPlan: 'Annual',
    });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.id).toBe('new-policy-id');
  });
});

describe('useBindPolicy', () => {
  it('posts bind and succeeds', async () => {
    // Arrange
    const { result } = renderHook(() => useBindPolicy(), { wrapper: createWrapper() });

    // Act
    result.current.mutate('policy-1');

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});

describe('useActivatePolicy', () => {
  it('posts activate and succeeds', async () => {
    // Arrange
    const { result } = renderHook(() => useActivatePolicy(), { wrapper: createWrapper() });

    // Act
    result.current.mutate('policy-1');

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});
