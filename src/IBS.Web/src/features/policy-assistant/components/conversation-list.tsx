import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, MessageSquare } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import type { ConversationDto, ConversationStatus } from '../types';

/**
 * Props for ConversationList.
 */
interface ConversationListProps {
  /** List of conversations to display. */
  conversations: ConversationDto[];
  /** Callback when New Chat button is clicked. */
  onNewChat: () => void;
}

/**
 * Returns the badge variant for a given conversation status.
 */
function getStatusVariant(status: ConversationStatus) {
  switch (status) {
    case 'Active':
      return 'default' as const;
    case 'PolicyCreated':
      return 'success' as const;
    case 'Abandoned':
      return 'secondary' as const;
  }
}

/**
 * Sidebar list of past policy assistant conversations.
 * Clicking a conversation navigates to its chat page.
 */
export function ConversationList({ conversations, onNewChat }: ConversationListProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { id: activeId } = useParams<{ id: string }>();

  /** Returns a translated label for a conversation status. */
  function getStatusLabel(status: ConversationStatus): string {
    switch (status) {
      case 'PolicyCreated':
        return t('policyAssistant.status.created');
      case 'Active':
        return t('policyAssistant.status.active');
      case 'Abandoned':
        return t('policyAssistant.status.abandoned');
    }
  }

  return (
    <div className="flex h-full flex-col">
      {/* Header */}
      <div className="flex items-center justify-between p-4 border-b">
        <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wider">
          {t('policyAssistant.conversationList.header')}
        </h2>
        <Button size="icon" variant="ghost" onClick={onNewChat} title={t('policyAssistant.conversationList.newChat')}>
          <Plus className="h-4 w-4" />
        </Button>
      </div>

      {/* List */}
      <div className="flex-1 overflow-y-auto p-2 space-y-1">
        {conversations.length === 0 && (
          <div className="flex flex-col items-center justify-center h-32 text-center px-4">
            <MessageSquare className="h-8 w-8 text-muted-foreground mb-2" />
            <p className="text-sm text-muted-foreground">{t('policyAssistant.conversationList.empty')}</p>
          </div>
        )}

        {conversations.map((conv) => (
          <button
            key={conv.id}
            onClick={() => navigate(`/policy-assistant/${conv.id}`)}
            className={cn(
              'w-full text-left rounded-lg px-3 py-2.5 transition-colors hover:bg-accent',
              activeId === conv.id && 'bg-accent'
            )}
          >
            <div className="flex items-start justify-between gap-2 mb-1">
              <span className="text-sm font-medium leading-tight line-clamp-2 flex-1">
                {conv.title}
              </span>
              <Badge variant={getStatusVariant(conv.status)} size="sm" className="shrink-0 mt-0.5">
                {getStatusLabel(conv.status)}
              </Badge>
            </div>
            <p className="text-xs text-muted-foreground">
              {new Date(conv.updatedAt).toLocaleDateString()}
            </p>
          </button>
        ))}
      </div>
    </div>
  );
}
