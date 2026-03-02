import { cn } from '@/lib/utils';
import type { ChatMessageDto } from '../types';

/**
 * Props for ChatMessage.
 */
interface ChatMessageProps {
  /** The message data to render. */
  message: ChatMessageDto;
}

/**
 * Formats an ISO timestamp to a readable time string.
 */
function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' });
}

/**
 * Individual message bubble for the chat window.
 * User messages align right; assistant messages align left.
 * PolicyExtraction messages render in italic style.
 */
export function ChatMessage({ message }: ChatMessageProps) {
  const isUser = message.role === 'user';
  const isExtraction = message.messageType === 'PolicyExtraction';

  return (
    <div className={cn('flex w-full', isUser ? 'justify-end' : 'justify-start')}>
      <div className={cn('max-w-[80%] space-y-1', isUser ? 'items-end' : 'items-start')}>
        <div
          className={cn(
            'rounded-2xl px-4 py-2.5 text-sm',
            isUser
              ? 'bg-blue-600 text-white rounded-br-sm'
              : 'bg-gray-100 text-gray-900 rounded-bl-sm',
            isExtraction && 'italic opacity-90'
          )}
        >
          <p className="whitespace-pre-wrap break-words">{message.content}</p>
        </div>
        <p className={cn('text-xs text-muted-foreground px-1', isUser ? 'text-right' : 'text-left')}>
          {formatTime(message.createdAt)}
        </p>
      </div>
    </div>
  );
}
