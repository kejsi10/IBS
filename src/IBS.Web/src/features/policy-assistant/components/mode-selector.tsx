import { useTranslation } from 'react-i18next';
import { cn } from '@/lib/utils';
import type { ConversationMode } from '../types';

/**
 * Props for ModeSelector.
 */
interface ModeSelectorProps {
  /** Currently selected mode. */
  value: ConversationMode;
  /** Callback invoked when the mode changes. */
  onChange: (mode: ConversationMode) => void;
}

/**
 * Toggle between Guided and Freeform conversation modes.
 * Only shown during conversation creation.
 */
export function ModeSelector({ value, onChange }: ModeSelectorProps) {
  const { t } = useTranslation();

  const modes: { value: ConversationMode; label: string; description: string }[] = [
    {
      value: 'Guided',
      label: t('policyAssistant.modeSelector.guided'),
      description: t('policyAssistant.modeSelector.guidedDescription'),
    },
    {
      value: 'Freeform',
      label: t('policyAssistant.modeSelector.freeform'),
      description: t('policyAssistant.modeSelector.freeformDescription'),
    },
  ];

  return (
    <div className="space-y-2">
      <p className="text-sm font-medium">{t('policyAssistant.modeSelector.label')}</p>
      <div className="grid grid-cols-2 gap-2">
        {modes.map((mode) => (
          <button
            key={mode.value}
            type="button"
            onClick={() => onChange(mode.value)}
            className={cn(
              'flex flex-col items-start rounded-lg border px-4 py-3 text-left transition-colors',
              value === mode.value
                ? 'border-primary bg-primary/5 text-primary'
                : 'border-input text-muted-foreground hover:bg-accent hover:text-accent-foreground'
            )}
          >
            <span className="text-sm font-medium">{mode.label}</span>
            <span className="text-xs mt-0.5 opacity-80">{mode.description}</span>
          </button>
        ))}
      </div>
    </div>
  );
}
