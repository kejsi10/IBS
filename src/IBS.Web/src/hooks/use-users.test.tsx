import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import React from 'react';
import {
  useUsers,
  useUser,
  useActivateUser,
  useDeactivateUser,
} from './use-users';

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

describe('useUsers', () => {
  it('returns paginated users list on success', async () => {
    // Arrange
    const { result } = renderHook(() => useUsers(), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data?.items).toBeDefined();
    expect(result.current.data?.items.length).toBeGreaterThan(0);
  });

  it('shows loading state initially', () => {
    // Arrange & Act
    const { result } = renderHook(() => useUsers(), { wrapper: createWrapper() });

    // Assert
    expect(result.current.isLoading).toBe(true);
  });
});

describe('useUser', () => {
  it('fetches single user by id', async () => {
    // Arrange
    const { result } = renderHook(() => useUser('user-1'), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data).toBeDefined();
    expect(result.current.data?.id).toBe('user-1');
  });

  it('is disabled when id is empty string', () => {
    // Arrange & Act
    const { result } = renderHook(() => useUser(''), { wrapper: createWrapper() });

    // Assert — query should not be fetching
    expect(result.current.isFetching).toBe(false);
  });
});

describe('useActivateUser', () => {
  it('posts activate and succeeds', async () => {
    // Arrange
    const { result } = renderHook(() => useActivateUser(), { wrapper: createWrapper() });

    // Act
    result.current.mutate('user-3');

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});

describe('useDeactivateUser', () => {
  it('posts deactivate and succeeds', async () => {
    // Arrange
    const { result } = renderHook(() => useDeactivateUser(), { wrapper: createWrapper() });

    // Act
    result.current.mutate('user-1');

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});
