import { describe, it, expect } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '@/test/utils';
import { ClientsPage } from './clients-page';

describe('ClientsPage', () => {
  it('renders the page heading', async () => {
    // Arrange & Act
    render(<ClientsPage />);

    // Assert
    expect(screen.getByRole('heading', { name: 'Clients' })).toBeInTheDocument();
  });

  it('renders loading skeleton while data is fetching', () => {
    // Arrange & Act
    render(<ClientsPage />);

    // Assert — animate-pulse elements appear during table loading state
    expect(document.querySelector('.animate-pulse')).toBeInTheDocument();
  });

  it('renders client rows after data loads', async () => {
    // Arrange
    render(<ClientsPage />);

    // Act
    await waitFor(() => expect(screen.getByText('John Doe')).toBeInTheDocument());

    // Assert
    expect(screen.getByText('John Doe')).toBeInTheDocument();
    expect(screen.getByText('Acme Corp')).toBeInTheDocument();
  });

  it('clicking Add Client button opens the dialog', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<ClientsPage />);

    // Act
    const addButton = screen.getByRole('button', { name: /add client/i });
    await user.click(addButton);

    // Assert — dialog title should appear
    await waitFor(() =>
      expect(screen.getByText('Add New Client')).toBeInTheDocument()
    );
  });

  it('clicking a client row navigates to the client detail page', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<ClientsPage />);

    // Act — wait for data and click the row
    await waitFor(() => expect(screen.getByText('John Doe')).toBeInTheDocument());
    const row = screen.getByText('John Doe').closest('tr')!;
    await user.click(row);

    // Assert
    expect(window.location.pathname).toBe('/clients/client-1');
  });

  it('deactivate action calls mutation and shows success toast', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<ClientsPage />);
    await waitFor(() => expect(screen.getByText('John Doe')).toBeInTheDocument());

    // Act — open the first row's action menu and click Deactivate
    const menuButtons = screen.getAllByRole('button', { name: /open menu/i });
    await user.click(menuButtons[0]);
    const deactivateItem = await screen.findByText('Deactivate');
    await user.click(deactivateItem);

    // Assert — success toast appears
    await waitFor(() =>
      expect(screen.getByText('Client deactivated')).toBeInTheDocument()
    );
  });
});
