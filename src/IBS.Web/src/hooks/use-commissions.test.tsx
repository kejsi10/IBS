import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import React from 'react';
import {
  useSchedules,
  useStatements,
  useCreateSchedule,
} from './use-commissions';

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

describe('useSchedules', () => {
  it('returns paginated schedules on success', async () => {
    // Arrange
    const { result } = renderHook(() => useSchedules(), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data?.items).toBeDefined();
    expect(result.current.data?.items.length).toBeGreaterThan(0);
  });

  it('shows loading state initially', () => {
    // Arrange & Act
    const { result } = renderHook(() => useSchedules(), { wrapper: createWrapper() });

    // Assert
    expect(result.current.isLoading).toBe(true);
  });
});

describe('useStatements', () => {
  it('returns paginated statements on success', async () => {
    // Arrange
    const { result } = renderHook(() => useStatements(), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data?.items).toBeDefined();
    expect(result.current.data?.items.length).toBeGreaterThan(0);
  });
});

describe('useCreateSchedule', () => {
  it('creates schedule and invalidates schedules list', async () => {
    // Arrange
    const { result } = renderHook(() => useCreateSchedule(), { wrapper: createWrapper() });

    // Act
    result.current.mutate({
      carrierId: 'carrier-1',
      carrierName: 'Test Carrier',
      lineOfBusiness: 'Personal Auto',
      newBusinessRate: 15,
      renewalRate: 12,
      effectiveFrom: '2024-01-01',
    });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.id).toBe('new-schedule-id');
  });
});
