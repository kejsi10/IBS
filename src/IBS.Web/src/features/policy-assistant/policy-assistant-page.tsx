import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { MessageSquare, Plus, Loader2 } from 'lucide-react';
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
import { ModeSelector } from './components/mode-selector';
import { useConversations, useCreateConversation } from './hooks/use-policy-assistant';
import type { ConversationMode } from './types';

/**
 * Main Policy Assistant page at /policy-assistant.
 * Shows a sidebar of conversations and prompts the user to start a new one.
 */
export function PolicyAssistantPage() {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const { data: conversations = [], isLoading } = useConversations();
  const createConversation = useCreateConversation();

  const [dialogOpen, setDialogOpen] = React.useState(false);
  const [mode, setMode] = React.useState<ConversationMode>('Guided');
  const [lineOfBusiness, setLineOfBusiness] = React.useState('');

  const handleCreate = async () => {
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

  return (
    <div className="flex h-[calc(100vh-4rem)] -m-6">
      {/* Sidebar */}
      <aside className="w-64 border-r bg-card shrink-0">
        {isLoading ? (
          <div className="flex items-center justify-center h-32">
            <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
          </div>
        ) : (
          <ConversationList conversations={conversations} onNewChat={() => setDialogOpen(true)} />
        )}
      </aside>

      {/* Main area — empty state */}
      <main className="flex-1 flex flex-col items-center justify-center gap-6 p-8">
        <div className="text-center space-y-3 max-w-sm">
          <div className="flex items-center justify-center w-16 h-16 rounded-full bg-primary/10 mx-auto">
            <MessageSquare className="h-8 w-8 text-primary" />
          </div>
          <h1 className="text-2xl font-bold tracking-tight">{t('policyAssistant.title')}</h1>
          <p className="text-muted-foreground text-sm">
            {t('policyAssistant.description')}
          </p>
          <Button onClick={() => setDialogOpen(true)} className="mt-2">
            <Plus className="mr-2 h-4 w-4" />
            {t('policyAssistant.startNewChat')}
          </Button>
        </div>
      </main>

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
              <label className="text-sm font-medium" htmlFor="lob">
                {t('policyAssistant.newConversation.lineOfBusiness')}{' '}
                <span className="text-muted-foreground">({t('common.form.optional').toLowerCase()})</span>
              </label>
              <input
                id="lob"
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
            <Button onClick={handleCreate} disabled={createConversation.isPending}>
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
