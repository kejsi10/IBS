import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { ChatMessage } from './chat-message';
import type { ChatMessageDto } from '../types';

/**
 * Props for ChatWindow.
 */
interface ChatWindowProps {
  /** Messages to render. */
  messages: ChatMessageDto[];
}

/**
 * Scrollable chat window that displays all messages and auto-scrolls to the bottom on new messages.
 */
export function ChatWindow({ messages }: ChatWindowProps) {
  const { t } = useTranslation();
  const bottomRef = React.useRef<HTMLDivElement>(null);

  // Auto-scroll to bottom whenever the message list changes.
  React.useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  return (
    <div className="flex-1 overflow-y-auto p-4 space-y-4">
      {messages.length === 0 && (
        <div className="flex items-center justify-center h-full">
          <p className="text-sm text-muted-foreground">
            {t('policyAssistant.chat.emptyState')}
          </p>
        </div>
      )}

      {messages.map((message) => (
        <ChatMessage key={message.id} message={message} />
      ))}

      {/* Sentinel element for auto-scroll */}
      <div ref={bottomRef} />
    </div>
  );
}
