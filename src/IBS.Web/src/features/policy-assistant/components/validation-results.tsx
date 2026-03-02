import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { CheckCircle2, AlertCircle, AlertTriangle, Info } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { cn } from '@/lib/utils';
import type { PolicyValidationResult, ValidationIssue } from '../types';

/**
 * Props for ValidationResults.
 */
interface ValidationResultsProps {
  /** The validation result data to display. */
  validation: PolicyValidationResult;
}

/**
 * Returns icon and color classes based on issue severity.
 */
function getSeverityStyle(severity: string): { icon: React.ReactNode; classes: string } {
  const lower = severity.toLowerCase();
  if (lower === 'error' || lower === 'critical') {
    return {
      icon: <AlertCircle className="h-3.5 w-3.5 shrink-0" />,
      classes: 'text-red-700 bg-red-50 border-red-200',
    };
  }
  if (lower === 'warning') {
    return {
      icon: <AlertTriangle className="h-3.5 w-3.5 shrink-0" />,
      classes: 'text-amber-700 bg-amber-50 border-amber-200',
    };
  }
  return {
    icon: <Info className="h-3.5 w-3.5 shrink-0" />,
    classes: 'text-blue-700 bg-blue-50 border-blue-200',
  };
}

/**
 * Row rendering a single validation issue.
 */
function IssueRow({ issue }: { issue: ValidationIssue }) {
  const { icon, classes } = getSeverityStyle(issue.severity);

  return (
    <div className={cn('flex items-start gap-2 rounded-md border px-3 py-2 text-xs', classes)}>
      {icon}
      <div className="min-w-0">
        <p className="font-medium">{issue.field}</p>
        <p className="opacity-80">{issue.description}</p>
      </div>
    </div>
  );
}

/**
 * Panel showing policy validation results.
 * Shows a green banner when valid, or a list of issues and warnings when not.
 */
export function ValidationResults({ validation }: ValidationResultsProps) {
  const { t } = useTranslation();

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-sm font-semibold">{t('policyAssistant.validation.title')}</CardTitle>
      </CardHeader>

      <CardContent className="space-y-3">
        {/* Overall status banner */}
        {validation.isValid ? (
          <div className="flex items-center gap-2 rounded-md bg-green-50 border border-green-200 px-3 py-2 text-green-700">
            <CheckCircle2 className="h-4 w-4 shrink-0" />
            <span className="text-xs font-medium">{t('policyAssistant.validation.compliant')}</span>
          </div>
        ) : (
          <div className="flex items-center gap-2 rounded-md bg-red-50 border border-red-200 px-3 py-2 text-red-700">
            <AlertCircle className="h-4 w-4 shrink-0" />
            <span className="text-xs font-medium">{t('policyAssistant.validation.failed')}</span>
          </div>
        )}

        {/* Summary text */}
        {validation.summary && (
          <p className="text-xs text-muted-foreground">{validation.summary}</p>
        )}

        {/* Issues */}
        {validation.issues.length > 0 && (
          <div className="space-y-1.5">
            <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">{t('policyAssistant.validation.issues')}</p>
            {validation.issues.map((issue, i) => (
              <IssueRow key={i} issue={issue} />
            ))}
          </div>
        )}

        {/* Warnings */}
        {validation.warnings.length > 0 && (
          <div className="space-y-1.5">
            <p className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">{t('policyAssistant.validation.warnings')}</p>
            {validation.warnings.map((warning, i) => (
              <div
                key={i}
                className="flex items-start gap-2 rounded-md border px-3 py-2 text-xs text-amber-700 bg-amber-50 border-amber-200"
              >
                <AlertTriangle className="h-3.5 w-3.5 shrink-0 mt-0.5" />
                <p>{warning}</p>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
