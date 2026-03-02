import { describe, it, expect } from 'vitest';

describe('Test Setup', () => {
  it('should run a basic test', () => {
    // Arrange
    const value = 1 + 1;

    // Act & Assert
    expect(value).toBe(2);
  });

  it('should have access to DOM matchers', () => {
    // Arrange
    const element = document.createElement('div');
    element.textContent = 'Hello, World!';

    // Act
    document.body.appendChild(element);

    // Assert
    expect(element).toBeInTheDocument();
    expect(element).toHaveTextContent('Hello, World!');

    // Cleanup
    element.remove();
  });
});
