import { describe, it, expect } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '@/test/utils';
import { ClientDialog } from './client-dialog';

/** Helper to render the ClientDialog in open state. */
const renderOpenDialog = () =>
  render(<ClientDialog open={true} onOpenChange={() => {}} />);

describe('ClientDialog', () => {
  it('renders type selection step initially', () => {
    // Arrange & Act
    renderOpenDialog();

    // Assert — both type buttons are visible
    expect(screen.getByText('Add New Client')).toBeInTheDocument();
    expect(screen.getByText('Individual')).toBeInTheDocument();
    expect(screen.getByText('Business')).toBeInTheDocument();
  });

  it('clicking Individual advances to the form step', async () => {
    // Arrange
    const user = userEvent.setup();
    renderOpenDialog();

    // Act
    await user.click(screen.getByText('Individual'));

    // Assert — individual form fields appear
    expect(await screen.findByLabelText(/first name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/last name/i)).toBeInTheDocument();
  });

  it('clicking Business advances to the form step', async () => {
    // Arrange
    const user = userEvent.setup();
    renderOpenDialog();

    // Act
    await user.click(screen.getByText('Business'));

    // Assert — business form fields appear
    expect(await screen.findByLabelText(/legal business name/i)).toBeInTheDocument();
  });

  it('back button returns to type selection', async () => {
    // Arrange
    const user = userEvent.setup();
    renderOpenDialog();
    await user.click(screen.getByText('Individual'));
    await screen.findByLabelText(/first name/i);

    // Act
    await user.click(screen.getByText(/← back to client type/i));

    // Assert — type selection is shown again
    expect(screen.getByText('Add New Client')).toBeInTheDocument();
    expect(screen.getByText('Individual')).toBeInTheDocument();
  });

  it('submitting empty individual form shows validation errors', async () => {
    // Arrange
    const user = userEvent.setup();
    renderOpenDialog();
    await user.click(screen.getByText('Individual'));
    await screen.findByLabelText(/first name/i);

    // Act — submit without filling fields
    await user.click(screen.getByRole('button', { name: /create client/i }));

    // Assert — validation errors appear
    await waitFor(() =>
      expect(screen.getByText(/first name is required/i)).toBeInTheDocument()
    );
  });

  it('successful individual create calls mutation and shows success toast', async () => {
    // Arrange
    const user = userEvent.setup();
    renderOpenDialog();
    await user.click(screen.getByText('Individual'));
    await screen.findByLabelText(/first name/i);

    // Act — fill and submit the form
    await user.type(screen.getByLabelText(/first name/i), 'Jane');
    await user.type(screen.getByLabelText(/last name/i), 'Smith');
    await user.type(screen.getByLabelText(/email/i), 'jane@example.com');
    await user.click(screen.getByRole('button', { name: /create client/i }));

    // Assert — success toast appears
    await waitFor(() =>
      expect(screen.getByText('Client created')).toBeInTheDocument()
    );
  });
});
