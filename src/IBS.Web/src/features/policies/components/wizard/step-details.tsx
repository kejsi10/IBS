import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, type SelectOption } from '@/components/ui/select';
import type { WizardData } from '../../policy-wizard';

/**
 * Props for the StepDetails component.
 */
export interface StepDetailsProps {
  data: Partial<WizardData>;
  onUpdate: (updates: Partial<WizardData>) => void;
}

/**
 * Policy details step of the policy wizard.
 */
export function StepDetails({ data, onUpdate }: StepDetailsProps) {
  const { t } = useTranslation();

  const billingTypeOptions: SelectOption[] = React.useMemo(() => [
    { value: 'DirectBill', label: 'Direct Bill' },
    { value: 'AgencyBill', label: 'Agency Bill' },
  ], []);

  const paymentPlanOptions: SelectOption[] = React.useMemo(() => [
    { value: 'Annual', label: 'Annual (1 payment)' },
    { value: 'SemiAnnual', label: 'Semi-Annual (2 payments)' },
    { value: 'Quarterly', label: 'Quarterly (4 payments)' },
    { value: 'Monthly', label: 'Monthly (12 payments)' },
  ], []);

  // Calculate policy term in days
  const termDays = React.useMemo(() => {
    if (!data.effectiveDate || !data.expirationDate) return null;
    const start = new Date(data.effectiveDate);
    const end = new Date(data.expirationDate);
    return Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24));
  }, [data.effectiveDate, data.expirationDate]);

  // Handle effective date change (auto-update expiration to +1 year)
  const handleEffectiveDateChange = (value: string) => {
    const effectiveDate = new Date(value);
    const expirationDate = new Date(effectiveDate);
    expirationDate.setFullYear(expirationDate.getFullYear() + 1);

    onUpdate({
      effectiveDate: value,
      expirationDate: expirationDate.toISOString().split('T')[0],
    });
  };

  return (
    <div className="space-y-6">
      {/* Policy Number (optional) */}
      <div className="space-y-2">
        <Label htmlFor="policyNumber">Policy Number (Optional)</Label>
        <Input
          id="policyNumber"
          value={data.policyNumber || ''}
          onChange={(e) => onUpdate({ policyNumber: e.target.value })}
          placeholder="Auto-generated if left empty"
        />
        <p className="text-xs text-muted-foreground">
          Leave empty to auto-generate a policy number
        </p>
      </div>

      {/* Dates */}
      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="effectiveDate">{t('policies.filters.effectiveFrom')} *</Label>
          <Input
            id="effectiveDate"
            type="date"
            value={data.effectiveDate || ''}
            onChange={(e) => handleEffectiveDateChange(e.target.value)}
          />
        </div>
        <div className="space-y-2">
          <Label htmlFor="expirationDate">Expiration Date *</Label>
          <Input
            id="expirationDate"
            type="date"
            value={data.expirationDate || ''}
            onChange={(e) => onUpdate({ expirationDate: e.target.value })}
          />
          {termDays !== null && (
            <p className="text-xs text-muted-foreground">
              Policy term: {termDays} days
            </p>
          )}
        </div>
      </div>

      {/* Billing */}
      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <Label htmlFor="billingType">Billing Type *</Label>
          <Select
            id="billingType"
            options={billingTypeOptions}
            value={data.billingType}
            onChange={(value) =>
              onUpdate({ billingType: value as 'DirectBill' | 'AgencyBill' })
            }
            placeholder="Select billing type"
          />
          <p className="text-xs text-muted-foreground">
            {data.billingType === 'DirectBill'
              ? 'Carrier bills the insured directly'
              : 'Agency collects and remits premium'}
          </p>
        </div>
        <div className="space-y-2">
          <Label htmlFor="paymentPlan">Payment Plan *</Label>
          <Select
            id="paymentPlan"
            options={paymentPlanOptions}
            value={data.paymentPlan}
            onChange={(value) =>
              onUpdate({
                paymentPlan: value as 'Annual' | 'SemiAnnual' | 'Quarterly' | 'Monthly',
              })
            }
            placeholder="Select payment plan"
          />
        </div>
      </div>

      {/* Description */}
      <div className="space-y-2">
        <Label htmlFor="description">Description / Notes</Label>
        <Textarea
          id="description"
          value={data.description || ''}
          onChange={(e) => onUpdate({ description: e.target.value })}
          placeholder="Optional notes about this policy..."
          rows={3}
        />
      </div>

      {/* Summary */}
      <div className="rounded-lg border bg-muted/30 p-4">
        <h4 className="mb-3 font-medium">Policy Term Summary</h4>
        <div className="grid gap-4 text-sm sm:grid-cols-2 lg:grid-cols-4">
          <div>
            <p className="text-muted-foreground">Effective</p>
            <p className="font-medium">
              {data.effectiveDate
                ? new Date(data.effectiveDate).toLocaleDateString()
                : '—'}
            </p>
          </div>
          <div>
            <p className="text-muted-foreground">Expiration</p>
            <p className="font-medium">
              {data.expirationDate
                ? new Date(data.expirationDate).toLocaleDateString()
                : '—'}
            </p>
          </div>
          <div>
            <p className="text-muted-foreground">Billing</p>
            <p className="font-medium">
              {data.billingType === 'DirectBill' ? 'Direct Bill' : 'Agency Bill'}
            </p>
          </div>
          <div>
            <p className="text-muted-foreground">Payment</p>
            <p className="font-medium">{data.paymentPlan}</p>
          </div>
        </div>
      </div>
    </div>
  );
}
