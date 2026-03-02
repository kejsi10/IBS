import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '@/test/utils';
import { Header } from './header';
import { LoginPage } from '@/features/auth/login-page';
import i18n from '@/i18n';

describe('Header — language switcher', () => {
  beforeEach(() => {
    // Ensure each test starts in English
    i18n.changeLanguage('en');
    localStorage.removeItem('ibs-language');
  });

  afterEach(() => {
    // Restore English so other test suites are not affected
    i18n.changeLanguage('en');
    localStorage.removeItem('ibs-language');
  });

  it('shows "PL" toggle button when current language is English', () => {
    // Arrange & Act
    render(<Header />);

    // Assert — the button label is the language to switch TO
    expect(screen.getByRole('button', { name: 'PL' })).toBeInTheDocument();
  });

  it('switches to Polish when the "PL" button is clicked', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<Header />);

    // Act
    await user.click(screen.getByRole('button', { name: 'PL' }));

    // Assert — button now shows "EN" (target to switch back)
    expect(screen.getByRole('button', { name: 'EN' })).toBeInTheDocument();
    expect(i18n.language).toBe('pl');
  });

  it('persists the selected language in localStorage', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<Header />);

    // Act
    await user.click(screen.getByRole('button', { name: 'PL' }));

    // Assert
    expect(localStorage.getItem('ibs-language')).toBe('pl');
  });

  it('switches back to English when the "EN" button is clicked', async () => {
    // Arrange — start in Polish
    const user = userEvent.setup();
    i18n.changeLanguage('pl');
    render(<Header />);

    // Act
    await user.click(screen.getByRole('button', { name: 'EN' }));

    // Assert
    expect(screen.getByRole('button', { name: 'PL' })).toBeInTheDocument();
    expect(i18n.language).toBe('en');
  });
});

describe('Polish smoke test — LoginPage renders in Polish', () => {
  beforeEach(() => {
    i18n.changeLanguage('pl');
  });

  afterEach(() => {
    i18n.changeLanguage('en');
    localStorage.removeItem('ibs-language');
  });

  it('renders the sign-in button in Polish', () => {
    // Arrange & Act
    render(<LoginPage />);

    // Assert — "Zaloguj się" is the Polish translation of "Sign in"
    expect(screen.getByRole('button', { name: /zaloguj się/i })).toBeInTheDocument();
  });

  it('renders the page title in Polish', () => {
    // Arrange & Act
    render(<LoginPage />);

    // Assert — "Witamy ponownie" is the Polish translation of "Welcome back"
    expect(screen.getByText('Witamy ponownie')).toBeInTheDocument();
  });
});
