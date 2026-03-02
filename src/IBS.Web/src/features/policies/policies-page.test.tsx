import { describe, it, expect } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '@/test/utils';
import { PoliciesPage } from './policies-page';

describe('PoliciesPage', () => {
  it('renders the page heading', () => {
    // Arrange & Act
    render(<PoliciesPage />);

    // Assert
    expect(screen.getByRole('heading', { name: 'Policies' })).toBeInTheDocument();
  });

  it('renders loading skeleton while data is fetching', () => {
    // Arrange & Act
    render(<PoliciesPage />);

    // Assert — animate-pulse elements appear during table loading state
    expect(document.querySelector('.animate-pulse')).toBeInTheDocument();
  });

  it('renders policy rows after data loads', async () => {
    // Arrange
    render(<PoliciesPage />);

    // Act
    await waitFor(() => expect(screen.getByText('POL-2024-001')).toBeInTheDocument());

    // Assert
    expect(screen.getByText('POL-2024-001')).toBeInTheDocument();
    expect(screen.getByText('POL-2024-002')).toBeInTheDocument();
  });

  it('clicking New Policy button navigates to /policies/new', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<PoliciesPage />);

    // Act
    const newPolicyButton = screen.getByRole('button', { name: /new policy/i });
    await user.click(newPolicyButton);

    // Assert
    expect(window.location.pathname).toBe('/policies/new');
  });

  it('renders the policy dashboard metrics section', async () => {
    // Arrange
    render(<PoliciesPage />);

    // Act
    await waitFor(() => expect(screen.getByText('Active Policies')).toBeInTheDocument());

    // Assert — dashboard stat cards are present
    expect(screen.getByText('Active Policies')).toBeInTheDocument();
    expect(screen.getByText('Expiring Soon')).toBeInTheDocument();
  });
});
