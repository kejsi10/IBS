import { describe, it, expect } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { render } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ToastProvider } from '@/components/ui/toast';
import { CarrierDetailPage } from './carrier-detail-page';
import '@/i18n';

/** Wraps the carrier detail page in the necessary providers with route params. */
function renderCarrierDetail(id: string) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false, gcTime: 0 }, mutations: { retry: false } },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <MemoryRouter initialEntries={[`/carriers/${id}`]}>
          <Routes>
            <Route path="/carriers/:id" element={<CarrierDetailPage />} />
          </Routes>
        </MemoryRouter>
      </ToastProvider>
    </QueryClientProvider>
  );
}

describe('CarrierDetailPage', () => {
  it('shows loading skeleton initially', () => {
    // Arrange & Act
    renderCarrierDetail('carrier-1');

    // Assert — animate-pulse appears during skeleton loading state
    expect(document.querySelector('.animate-pulse')).toBeInTheDocument();
  });

  it('renders carrier name and info after data loads', async () => {
    // Arrange & Act
    renderCarrierDetail('carrier-1');

    // Assert — wait for carrier heading to render (use role to avoid matching InlineEdit span)
    await waitFor(() =>
      expect(screen.getByRole('heading', { name: 'Acme Insurance' })).toBeInTheDocument()
    );

    expect(screen.getAllByText('ACME').length).toBeGreaterThan(0);
  });

  it('shows products and appetites sections with real data', async () => {
    // Arrange & Act
    renderCarrierDetail('carrier-1');

    // Assert — wait for the page to fully render
    await waitFor(() =>
      expect(screen.getByRole('heading', { name: 'Acme Insurance' })).toBeInTheDocument()
    );

    // Products section heading and mock product row
    expect(screen.getAllByText('Products').length).toBeGreaterThan(0);
    expect(screen.getByText('General Liability Coverage')).toBeInTheDocument();

    // Appetites section heading (with active appetite card)
    expect(screen.getByText('Underwriting Appetites')).toBeInTheDocument();
  });

  it('shows error state for unknown carrier', async () => {
    // Arrange & Act
    renderCarrierDetail('unknown-id');

    // Assert — MSW returns 404 → Axios error → error card shows the HTTP error message
    await waitFor(() =>
      expect(screen.getByText(/request failed with status code 404/i)).toBeInTheDocument()
    );
  });
});
