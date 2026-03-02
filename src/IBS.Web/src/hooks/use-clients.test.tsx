import { describe, it, expect } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter } from 'react-router-dom';
import React from 'react';
import {
  useClients,
  useClient,
  useCreateIndividualClient,
  useCreateBusinessClient,
  useDeactivateClient,
  useReactivateClient,
} from './use-clients';

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

describe('useClients', () => {
  it('returns paginated client list on success', async () => {
    // Arrange
    const { result } = renderHook(() => useClients(), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data?.items).toBeDefined();
    expect(result.current.data?.items.length).toBeGreaterThan(0);
  });

  it('shows loading state initially', () => {
    // Arrange & Act
    const { result } = renderHook(() => useClients(), { wrapper: createWrapper() });

    // Assert
    expect(result.current.isLoading).toBe(true);
  });

  it('passes filters to the query key', async () => {
    // Arrange
    const filters = { search: 'John' };
    const { result } = renderHook(() => useClients(filters), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data?.items).toBeDefined();
  });
});

describe('useClient', () => {
  it('fetches single client by id', async () => {
    // Arrange
    const { result } = renderHook(() => useClient('client-1'), { wrapper: createWrapper() });

    // Act
    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data).toBeDefined();
    expect(result.current.data?.id).toBe('client-1');
  });

  it('is disabled when id is empty string', () => {
    // Arrange & Act
    const { result } = renderHook(() => useClient(''), { wrapper: createWrapper() });

    // Assert — query should not be fetching
    expect(result.current.isFetching).toBe(false);
  });
});

describe('useCreateIndividualClient', () => {
  it('posts individual client and returns new id', async () => {
    // Arrange
    const { result } = renderHook(() => useCreateIndividualClient(), { wrapper: createWrapper() });

    // Act
    result.current.mutate({ firstName: 'Jane', lastName: 'Smith', email: 'jane@example.com' });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.id).toBe('new-individual-client-id');
  });
});

describe('useCreateBusinessClient', () => {
  it('posts business client and returns new id', async () => {
    // Arrange
    const { result } = renderHook(() => useCreateBusinessClient(), { wrapper: createWrapper() });

    // Act
    result.current.mutate({
      businessName: 'New Corp',
      businessType: 'Corporation',
      email: 'contact@newcorp.com',
    });

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data?.id).toBe('new-business-client-id');
  });
});

describe('useDeactivateClient', () => {
  it('posts deactivate and succeeds', async () => {
    // Arrange
    const { result } = renderHook(() => useDeactivateClient(), { wrapper: createWrapper() });

    // Act
    result.current.mutate('client-1');

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});

describe('useReactivateClient', () => {
  it('posts reactivate and succeeds', async () => {
    // Arrange
    const { result } = renderHook(() => useReactivateClient(), { wrapper: createWrapper() });

    // Act
    result.current.mutate('client-1');

    // Assert
    await waitFor(() => expect(result.current.isSuccess).toBe(true));
  });
});
