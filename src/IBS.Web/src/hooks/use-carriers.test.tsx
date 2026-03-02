import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import React from 'react';
import {
  useCarriers,
  useCarrier,
  useCreateCarrier,
  useDeactivateCarrier,
} from './use-carriers';

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

describe('useCarriers', () => {
  it('returns carrier array on success', async () => {
    // Arrange
    const { result } = renderHook(() => useCarriers(), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(Array.isArray(result.current.data)).toBe(true);
    expect(result.current.data!.length).toBeGreaterThan(0);
  });

  it('shows loading state initially', () => {
    // Arrange & Act
    const { result } = renderHook(() => useCarriers(), { wrapper: createWrapper() });

    // Assert
    expect(result.current.isLoading).toBe(true);
  });
});

describe('useCarrier', () => {
  it('fetches single carrier by id', async () => {
    // Arrange
    const { result } = renderHook(() => useCarrier('carrier-1'), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data).toBeDefined();
    expect(result.current.data?.id).toBe('carrier-1');
  });

  it('is disabled when id is empty string', () => {
    // Arrange & Act
    const { result } = renderHook(() => useCarrier(''), { wrapper: createWrapper() });

    // Assert — query should not be fetching
    expect(result.current.isFetching).toBe(false);
  });
});

describe('useCreateCarrier', () => {
  it('creates carrier and returns new id', async () => {
    // Arrange
    const { result } = renderHook(() => useCreateCarrier(), { wrapper: createWrapper() });

    // Act
    result.current.mutate({ name: 'New Carrier', code: 'NEW' });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.id).toBe('new-carrier-id');
  });
});

describe('useDeactivateCarrier', () => {
  it('posts deactivate and succeeds', async () => {
    // Arrange
    const { result } = renderHook(() => useDeactivateCarrier(), { wrapper: createWrapper() });

    // Act
    result.current.mutate({ id: 'carrier-1' });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});
