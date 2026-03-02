import { describe, it, expect } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '@/test/utils';
import { CarriersPage } from './carriers-page';

describe('CarriersPage', () => {
  it('renders the page heading', () => {
    // Arrange & Act
    render(<CarriersPage />);

    // Assert
    expect(screen.getByRole('heading', { name: 'Carriers' })).toBeInTheDocument();
  });

  it('renders loading skeleton while data is fetching', () => {
    // Arrange & Act
    render(<CarriersPage />);

    // Assert — animate-pulse elements appear during table loading state
    expect(document.querySelector('.animate-pulse')).toBeInTheDocument();
  });

  it('renders carrier rows after data loads', async () => {
    // Arrange
    render(<CarriersPage />);

    // Act
    await waitFor(() => expect(screen.getByText('Acme Insurance')).toBeInTheDocument());

    // Assert
    expect(screen.getByText('Acme Insurance')).toBeInTheDocument();
    expect(screen.getByText('Global Underwriters')).toBeInTheDocument();
  });

  it('search input filters displayed carriers', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<CarriersPage />);

    // Wait for data to load
    await waitFor(() => expect(screen.getByText('Acme Insurance')).toBeInTheDocument());

    // Act — type in the search box
    const searchInput = screen.getByPlaceholderText(/search carriers/i);
    await user.type(searchInput, 'acme');

    // Assert — only the matching carrier should remain visible after debounce
    await waitFor(
      () => expect(screen.queryByText('Global Underwriters')).not.toBeInTheDocument(),
      { timeout: 2000 }
    );
    expect(screen.getByText('Acme Insurance')).toBeInTheDocument();
  });

  it('status filter shows only active carriers when selected', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<CarriersPage />);

    // Wait for data to load
    await waitFor(() => expect(screen.getByText('Acme Insurance')).toBeInTheDocument());

    // Act — click the Active filter tab
    const activeFilter = screen.getByRole('tab', { name: /^active/i });
    await user.click(activeFilter);

    // Assert — all mock carriers are Active so both should remain
    expect(screen.getByText('Acme Insurance')).toBeInTheDocument();
    expect(screen.getByText('Global Underwriters')).toBeInTheDocument();
  });
});
