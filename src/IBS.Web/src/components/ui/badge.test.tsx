import { describe, it, expect } from 'vitest';
import { render, screen } from '@/test/utils';
import { Badge, StatusBadge } from './badge';

describe('Badge', () => {
  it('renders with default variant', () => {
    // Arrange & Act
    render(<Badge>Default Badge</Badge>);

    // Assert
    expect(screen.getByText('Default Badge')).toBeInTheDocument();
  });

  it('renders with success variant', () => {
    // Arrange & Act
    render(<Badge variant="success">Success</Badge>);

    // Assert
    const badge = screen.getByText('Success');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-green-100');
  });

  it('renders with error variant', () => {
    // Arrange & Act
    render(<Badge variant="error">Error</Badge>);

    // Assert
    const badge = screen.getByText('Error');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-red-100');
  });

  it('renders with different sizes', () => {
    // Arrange & Act
    const { rerender } = render(<Badge size="sm">Small</Badge>);

    // Assert
    expect(screen.getByText('Small')).toHaveClass('text-[10px]');

    rerender(<Badge size="lg">Large</Badge>);
    expect(screen.getByText('Large')).toHaveClass('text-sm');
  });

  it('accepts custom className', () => {
    // Arrange & Act
    render(<Badge className="custom-class">Custom</Badge>);

    // Assert
    expect(screen.getByText('Custom')).toHaveClass('custom-class');
  });
});

describe('StatusBadge', () => {
  it('renders active status with success variant', () => {
    // Arrange & Act
    render(<StatusBadge status="active" />);

    // Assert
    const badge = screen.getByText('Active');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-green-100');
  });

  it('renders pending status with warning variant', () => {
    // Arrange & Act
    render(<StatusBadge status="pending" />);

    // Assert
    const badge = screen.getByText('Pending');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-yellow-100');
  });

  it('renders cancelled status with error variant', () => {
    // Arrange & Act
    render(<StatusBadge status="cancelled" />);

    // Assert
    const badge = screen.getByText('Cancelled');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-red-100');
  });

  it('allows custom children to override label', () => {
    // Arrange & Act
    render(<StatusBadge status="active">Custom Label</StatusBadge>);

    // Assert
    expect(screen.getByText('Custom Label')).toBeInTheDocument();
    expect(screen.queryByText('Active')).not.toBeInTheDocument();
  });
});
