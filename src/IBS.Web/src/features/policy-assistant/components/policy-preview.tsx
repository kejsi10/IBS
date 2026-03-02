import { useTranslation } from 'react-i18next';
import { CheckCircle2 } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import type { PolicyExtractionResult } from '../types';

/**
 * Props for PolicyPreview.
 */
interface PolicyPreviewProps {
  /** The latest extracted policy data. */
  extraction: PolicyExtractionResult;
}

/**
 * Renders a single detail row if the value is present.
 */
function DetailRow({ label, value }: { label: string; value?: string }) {
  if (!value) return null;
  return (
    <div className="flex items-start justify-between gap-2 py-1">
      <span className="text-xs text-muted-foreground shrink-0">{label}</span>
      <span className="text-xs font-medium text-right">{value}</span>
    </div>
  );
}

/**
 * Card showing live extracted policy data from the AI conversation.
 * Only renders when there is at least some extracted data.
 */
export function PolicyPreview({ extraction }: PolicyPreviewProps) {
  const { t } = useTranslation();
  const hasAnyData =
    extraction.clientName ||
    extraction.carrierName ||
    extraction.lineOfBusiness ||
    extraction.policyType ||
    extraction.effectiveDate ||
    extraction.expirationDate ||
    extraction.billingType ||
    extraction.paymentPlan ||
    extraction.coverages.length > 0;

  if (!hasAnyData) return null;

  return (
    <Card>
      <CardHeader className="pb-2">
        <div className="flex items-center justify-between">
          <CardTitle className="text-sm font-semibold">{t('policyAssistant.policyPreview.title')}</CardTitle>
          {extraction.isComplete && (
            <div className="flex items-center gap-1 text-green-600">
              <CheckCircle2 className="h-4 w-4" />
              <span className="text-xs font-medium">{t('policyAssistant.policyPreview.complete')}</span>
            </div>
          )}
        </div>
      </CardHeader>

      <CardContent className="space-y-3">
        {/* Policy details */}
        <div className="divide-y divide-border">
          <DetailRow label={t('policyAssistant.policyPreview.client')} value={extraction.clientName} />
          <DetailRow label={t('policyAssistant.policyPreview.carrier')} value={extraction.carrierName} />
          <DetailRow label={t('policyAssistant.policyPreview.lineOfBusiness')} value={extraction.lineOfBusiness} />
          <DetailRow label={t('policyAssistant.policyPreview.policyType')} value={extraction.policyType} />
          <DetailRow label={t('policyAssistant.policyPreview.effectiveDate')} value={extraction.effectiveDate} />
          <DetailRow label={t('policyAssistant.policyPreview.expirationDate')} value={extraction.expirationDate} />
          <DetailRow label={t('policyAssistant.policyPreview.billingType')} value={extraction.billingType} />
          <DetailRow label={t('policyAssistant.policyPreview.paymentPlan')} value={extraction.paymentPlan} />
        </div>

        {/* Coverages table */}
        {extraction.coverages.length > 0 && (
          <div>
            <p className="text-xs font-semibold text-muted-foreground mb-2 uppercase tracking-wider">
              {t('policyAssistant.policyPreview.coverages')}
            </p>
            <div className="overflow-x-auto">
              <table className="w-full text-xs">
                <thead>
                  <tr className="border-b">
                    <th className="text-left py-1 pr-2 text-muted-foreground font-medium">{t('policyAssistant.policyPreview.coverageName')}</th>
                    <th className="text-right py-1 pr-2 text-muted-foreground font-medium">{t('policyAssistant.policyPreview.coveragePremium')}</th>
                    <th className="text-right py-1 pr-2 text-muted-foreground font-medium">{t('policyAssistant.policyPreview.coverageLimit')}</th>
                    <th className="text-right py-1 text-muted-foreground font-medium">{t('policyAssistant.policyPreview.coverageDeductible')}</th>
                  </tr>
                </thead>
                <tbody>
                  {extraction.coverages.map((cov, i) => (
                    <tr key={i} className="border-b last:border-0">
                      <td className="py-1 pr-2">{cov.name || cov.code || '—'}</td>
                      <td className="py-1 pr-2 text-right">{cov.premium || '—'}</td>
                      <td className="py-1 pr-2 text-right">{cov.limit || '—'}</td>
                      <td className="py-1 text-right">{cov.deductible || '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* Missing fields */}
        {extraction.missingFields.length > 0 && (
          <div>
            <p className="text-xs font-semibold text-amber-600 mb-1">{t('policyAssistant.policyPreview.missingFields')}</p>
            <ul className="space-y-0.5">
              {extraction.missingFields.map((field) => (
                <li key={field} className="text-xs text-amber-700 flex items-center gap-1">
                  <span className="inline-block w-1 h-1 rounded-full bg-amber-500 shrink-0" />
                  {field}
                </li>
              ))}
            </ul>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
