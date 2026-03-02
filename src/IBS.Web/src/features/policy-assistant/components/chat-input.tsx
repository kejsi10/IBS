import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { Send } from 'lucide-react';
import { Button } from '@/components/ui/button';

/**
 * Props for ChatInput.
 */
interface ChatInputProps {
  /** Callback invoked with the message text when the user sends. */
  onSend: (message: string) => void;
  /** Disables the input and send button when true. */
  disabled?: boolean;
  /** Placeholder text for the textarea. */
  placeholder?: string;
}

/**
 * Textarea + Send button for composing chat messages.
 * Enter sends the message; Shift+Enter inserts a newline.
 */
export function ChatInput({
  onSend,
  disabled = false,
  placeholder = 'Type a message...',
}: ChatInputProps) {
  const { t } = useTranslation();
  const [value, setValue] = React.useState('');

  const handleSend = () => {
    const trimmed = value.trim();
    if (!trimmed || disabled) return;
    onSend(trimmed);
    setValue('');
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <div className="flex items-end gap-2 border-t p-4 bg-background">
      <textarea
        className="flex-1 resize-none rounded-lg border border-input bg-background px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50 min-h-[44px] max-h-[120px]"
        value={value}
        onChange={(e) => setValue(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder={placeholder}
        disabled={disabled}
        rows={1}
      />
      <Button
        onClick={handleSend}
        disabled={disabled || !value.trim()}
        size="icon"
        className="shrink-0"
        title={t('policyAssistant.chat.sendMessage')}
      >
        <Send className="h-4 w-4" />
      </Button>
    </div>
  );
}
