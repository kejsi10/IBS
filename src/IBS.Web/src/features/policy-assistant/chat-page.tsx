import * as React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from '@/components/ui/dialog';
import { useToast } from '@/components/ui/toast';
import { getErrorMessage } from '@/lib/api';
import { ConversationList } from './components/conversation-list';
import { ChatWindow } from './components/chat-window';
import { ChatInput } from './components/chat-input';
import { PolicyPreview } from './components/policy-preview';
import { ValidationResults } from './components/validation-results';
import { CreatePolicyButton } from './components/create-policy-button';
import { ModeSelector } from './components/mode-selector';
import {
  useConversations,
  useConversation,
  useSendMessage,
  useCreateConversation,
} from './hooks/use-policy-assistant';
import type { ChatMessageDto, PolicyExtractionResult, PolicyValidationResult, ConversationMode } from './types';

/**
 * Individual chat page at /policy-assistant/:id.
 * Three-column layout: conversation list sidebar, chat window, policy preview panel.
 */
export function ChatPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();

  const { data: conversations = [] } = useConversations();
  const { data: conversation, isLoading } = useConversation(id);
  const sendMessage = useSendMessage();
  const createConversation = useCreateConversation();

  // Local state for optimistic message updates during sending
  const [localMessages, setLocalMessages] = React.useState<ChatMessageDto[]>([]);
  const [localExtraction, setLocalExtraction] = React.useState<PolicyExtractionResult | undefined>();
  const [localValidation, setLocalValidation] = React.useState<PolicyValidationResult | undefined>();

  // Sync local state from server data when the conversation loads
  React.useEffect(() => {
    if (conversation) {
      setLocalMessages(conversation.messages);
      setLocalExtraction(conversation.extractedData);
      setLocalValidation(undefined);
    }
  }, [conversation]);

  // New chat dialog
  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [mode, setMode] = React.useState<ConversationMode>('Guided');
  const [lineOfBusiness, setLineOfBusiness] = React.useState('');

  const handleSend = async (content: string) => {
    if (!id) return;

    // Optimistic user message (temporary ID)
    const tempUserMsg: ChatMessageDto = {
      id: `temp-user-${Date.now()}`,
      role: 'user',
      content,
      messageType: 'Chat',
      createdAt: new Date().toISOString(),
    };
    setLocalMessages((prev) => [...prev, tempUserMsg]);

    try {
      const result = await sendMessage.mutateAsync({ conversationId: id, content });

      // Keep the optimistic user message, add the assistant reply from the response
      const assistantMsg: ChatMessageDto = {
        id: result.messageId,
        role: 'assistant',
        content: result.content,
        messageType: 'Chat',
        createdAt: new Date().toISOString(),
      };
      setLocalMessages((prev) => [
        ...prev.filter((m) => m.id !== tempUserMsg.id),
        { ...tempUserMsg, id: `user-${Date.now()}` },
        assistantMsg,
      ]);

      if (result.extraction) {
        setLocalExtraction(result.extraction);
      }
      if (result.validation) {
        setLocalValidation(result.validation);
      }
    } catch (err) {
      // Remove optimistic message on failure
      setLocalMessages((prev) => prev.filter((m) => m.id !== tempUserMsg.id));
      addToast({ title: t('common.toast.error'), description: getErrorMessage(err), variant: 'error' });
    }
  };

  const handleNewChat = async () => {
    setDialogOpen(true);
  };

  const handleCreateConversation = async () => {
    try {
      const conv = await createConversation.mutateAsync({
        mode,
        lineOfBusiness: lineOfBusiness.trim() || undefined,
      });
      setDialogOpen(false);
      setLineOfBusiness('');
      setMode('Guided');
      navigate(`/policy-assistant/${conv.id}`);
    } catch (err) {
      addToast({ title: t('common.toast.error'), description: getErrorMessage(err), variant: 'error' });
    }
  };

  const isSending = sendMessage.isPending;
  const showRightPanel = !!localExtraction;

  return (
    <div className="flex h-[calc(100vh-4rem)] -m-6">
      {/* Left sidebar — conversation list */}
      <aside className="w-56 border-r bg-card shrink-0">
        <ConversationList conversations={conversations} onNewChat={handleNewChat} />
      </aside>

      {/* Center — chat */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Conversation header */}
        {conversation && (
          <div className="flex items-center justify-between px-4 py-3 border-b bg-background shrink-0">
            <div className="min-w-0">
              <h2 className="text-sm font-semibold truncate">{conversation.title}</h2>
              <p className="text-xs text-muted-foreground">
                {conversation.mode} · {conversation.status}
              </p>
            </div>
          </div>
        )}

        {/* Loading state */}
        {isLoading && (
          <div className="flex-1 flex items-center justify-center">
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
          </div>
        )}

        {/* Chat messages */}
        {!isLoading && (
          <>
            <ChatWindow messages={localMessages} />
            <ChatInput
              onSend={handleSend}
              disabled={isSending || conversation?.status !== 'Active'}
              placeholder={
                conversation?.status !== 'Active'
                  ? t('policyAssistant.chat.placeholderClosed')
                  : t('policyAssistant.chat.placeholder')
              }
            />
          </>
        )}
      </div>

      {/* Right panel — policy preview + validation + create button */}
      {showRightPanel && localExtraction && (
        <aside className="w-72 border-l bg-card shrink-0 overflow-y-auto">
          <div className="p-4 space-y-4">
            <PolicyPreview extraction={localExtraction} />

            {localValidation && <ValidationResults validation={localValidation} />}

            {id && conversation?.status === 'Active' && (
              <CreatePolicyButton conversationId={id} extraction={localExtraction} />
            )}
          </div>
        </aside>
      )}

      {/* New Chat Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t('policyAssistant.newConversation.title')}</DialogTitle>
            <DialogDescription>
              {t('policyAssistant.newConversation.description')}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-2">
            <ModeSelector value={mode} onChange={setMode} />

            <div className="space-y-1.5">
              <label className="text-sm font-medium" htmlFor="lob-chat">
                {t('policyAssistant.newConversation.lineOfBusiness')}{' '}
                <span className="text-muted-foreground">({t('common.form.optional').toLowerCase()})</span>
              </label>
              <input
                id="lob-chat"
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring"
                placeholder={t('policyAssistant.newConversation.lobPlaceholder')}
                value={lineOfBusiness}
                onChange={(e) => setLineOfBusiness(e.target.value)}
              />
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setDialogOpen(false)} disabled={createConversation.isPending}>
              {t('common.actions.cancel')}
            </Button>
            <Button onClick={handleCreateConversation} disabled={createConversation.isPending}>
              {createConversation.isPending ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  {t('common.form.creating')}
                </>
              ) : (
                t('policyAssistant.newConversation.startChat')
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
