import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { FileText, Loader2 } from 'lucide-react';
import { Autocomplete } from '@/components/common/autocomplete';
import { Button } from '@/components/ui/button';
import { DatePicker } from '@/components/ui/date-picker';
import { Select } from '@/components/ui/select';
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
import { useClients } from '@/hooks/use-clients';
import { useCarriers } from '@/hooks/use-carriers';
import type { LineOfBusiness } from '@/types/api';
import { useCreatePolicyFromConversation } from '../hooks/use-policy-assistant';
import type { PolicyExtractionResult } from '../types';

/**
 * Props for CreatePolicyButton.
 */
interface CreatePolicyButtonProps {
  /** ID of the conversation to create a policy from. */
  conversationId: string;
  /** Latest extraction result — used to pre-populate dialog fields. */
  extraction: PolicyExtractionResult;
}

/** All valid LineOfBusiness enum values. */
const LOB_VALUES: LineOfBusiness[] = [
  'PersonalAuto', 'Homeowners', 'Renters', 'PersonalUmbrella', 'Life', 'Health',
  'GeneralLiability', 'CommercialProperty', 'WorkersCompensation', 'CommercialAuto',
  'ProfessionalLiability', 'DirectorsAndOfficers', 'CyberLiability', 'BusinessOwnersPolicy',
  'CommercialUmbrella', 'InlandMarine', 'Surety',
];

/** Human-readable labels for each LineOfBusiness value. */
const LOB_LABELS: Record<LineOfBusiness, string> = {
  PersonalAuto: 'Personal Auto',
  Homeowners: 'Homeowners',
  Renters: 'Renters',
  PersonalUmbrella: 'Personal Umbrella',
  Life: 'Life',
  Health: 'Health',
  GeneralLiability: 'General Liability',
  CommercialProperty: 'Commercial Property',
  WorkersCompensation: 'Workers Compensation',
  CommercialAuto: 'Commercial Auto',
  ProfessionalLiability: 'Professional Liability (E&O)',
  DirectorsAndOfficers: 'Directors & Officers (D&O)',
  CyberLiability: 'Cyber Liability',
  BusinessOwnersPolicy: 'Business Owners Policy (BOP)',
  CommercialUmbrella: 'Commercial Umbrella',
  InlandMarine: 'Inland Marine',
  Surety: 'Surety Bonds',
};

const LOB_OPTIONS = LOB_VALUES.map((v) => ({ value: v, label: LOB_LABELS[v] }));

const BILLING_TYPE_OPTIONS = [
  { value: 'DirectBill', label: 'Direct Bill' },
  { value: 'Agency', label: 'Agency Bill' },
];

const PAYMENT_PLAN_OPTIONS = [
  { value: 'Annual', label: 'Annual' },
  { value: 'SemiAnnual', label: 'Semi-Annual' },
  { value: 'Quarterly', label: 'Quarterly' },
  { value: 'Monthly', label: 'Monthly' },
];

/**
 * Normalizes an AI-extracted line-of-business string to the nearest valid enum value.
 * Handles cases like "Homeowners Policy" → "Homeowners".
 */
function normalizeLob(value: string | null | undefined): LineOfBusiness | undefined {
  if (!value) return undefined;
  const lower = value.toLowerCase();
  const exact = LOB_VALUES.find((v) => v.toLowerCase() === lower);
  if (exact) return exact;
  // Enum name contained in extracted string (e.g. "Homeowners" in "Homeowners Policy")
  const contained = LOB_VALUES.find((v) => lower.includes(v.toLowerCase()));
  return contained;
}

/**
 * Normalizes an extracted billing type string to a canonical enum value.
 */
function normalizeBillingType(value: string | null | undefined): string {
  if (!value) return 'DirectBill';
  const normalized = value.replace(/\s/g, '');
  return BILLING_TYPE_OPTIONS.find((o) => o.value.toLowerCase() === normalized.toLowerCase())?.value ?? 'DirectBill';
}

/**
 * Normalizes an extracted payment plan string to a canonical enum value.
 */
function normalizePaymentPlan(value: string | null | undefined): string {
  if (!value) return 'Annual';
  const normalized = value.replace(/[\s-]/g, '');
  return PAYMENT_PLAN_OPTIONS.find((o) => o.value.toLowerCase() === normalized.toLowerCase())?.value ?? 'Annual';
}

/**
 * Button that opens a dialog to review and confirm policy creation from a conversation.
 * All fields are pre-populated from the AI extraction and can be corrected before submitting.
 */
export function CreatePolicyButton({ conversationId, extraction }: CreatePolicyButtonProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const createPolicy = useCreatePolicyFromConversation();
  const [open, setOpen] = React.useState(false);

  // Client / Carrier autocomplete state
  const [clientId, setClientId] = React.useState<string | undefined>(undefined);
  const [carrierId, setCarrierId] = React.useState<string | undefined>(undefined);
  const [clientSearch, setClientSearch] = React.useState('');
  const [carrierSearch, setCarrierSearch] = React.useState('');

  // Policy fields — pre-populated from extraction
  const [lineOfBusiness, setLineOfBusiness] = React.useState<string | undefined>(undefined);
  const [effectiveDate, setEffectiveDate] = React.useState('');
  const [expirationDate, setExpirationDate] = React.useState('');
  const [billingType, setBillingType] = React.useState('DirectBill');
  const [paymentPlan, setPaymentPlan] = React.useState('Annual');

  const { data: clients, isLoading: clientsLoading } = useClients({ search: clientSearch, status: 'Active' });
  const { data: carriers, isLoading: carriersLoading } = useCarriers();

  const clientOptions = React.useMemo(
    () => clients?.items?.map((c) => ({ value: c.id, label: c.displayName, description: c.email || c.phone || undefined })) ?? [],
    [clients]
  );

  const carrierOptions = React.useMemo(() => {
    let items = carriers ?? [];
    if (carrierSearch) {
      const q = carrierSearch.toLowerCase();
      items = items.filter((c) => c.name.toLowerCase().includes(q) || c.code.toLowerCase().includes(q));
    }
    return items.map((c) => ({
      value: c.id,
      label: c.name,
      description: c.amBestRating ? `AM Best: ${c.amBestRating}` : undefined,
    }));
  }, [carriers, carrierSearch]);

  const handleOpenChange = (next: boolean) => {
    setOpen(next);
    if (next) {
      // Pre-populate from extraction when opening
      setClientId(undefined);
      setCarrierId(undefined);
      setClientSearch(extraction.clientName ?? '');
      setCarrierSearch(extraction.carrierName ?? '');
      setLineOfBusiness(normalizeLob(extraction.lineOfBusiness));
      setEffectiveDate(extraction.effectiveDate ?? '');
      setExpirationDate(extraction.expirationDate ?? '');
      setBillingType(normalizeBillingType(extraction.billingType));
      setPaymentPlan(normalizePaymentPlan(extraction.paymentPlan));
    } else {
      setClientId(undefined);
      setCarrierId(undefined);
      setClientSearch('');
      setCarrierSearch('');
      setLineOfBusiness(undefined);
      setEffectiveDate('');
      setExpirationDate('');
      setBillingType('DirectBill');
      setPaymentPlan('Annual');
    }
  };

  const handleCreate = async () => {
    if (!clientId || !carrierId || !lineOfBusiness || !effectiveDate || !expirationDate) {
      addToast({ title: t('common.form.required'), description: t('policyAssistant.createPolicy.requiredDescription'), variant: 'error' });
      return;
    }

    try {
      const result = await createPolicy.mutateAsync({
        conversationId,
        clientId,
        carrierId,
        overrides: { lineOfBusiness, effectiveDate, expirationDate, billingType, paymentPlan },
      });
      addToast({ title: t('policyAssistant.createPolicy.successTitle'), description: t('policyAssistant.createPolicy.successDescription'), variant: 'success' });
      setOpen(false);
      navigate(`/policies/${result.policyId}`);
    } catch (err) {
      addToast({ title: t('common.toast.error'), description: getErrorMessage(err), variant: 'error' });
    }
  };

  return (
    <>
      <Button
        className="w-full"
        disabled={!extraction.isComplete}
        onClick={() => handleOpenChange(true)}
        title={!extraction.isComplete ? t('policyAssistant.createPolicy.buttonDisabledTitle') : undefined}
      >
        <FileText className="mr-2 h-4 w-4" />
        {t('policyAssistant.createPolicy.button')}
      </Button>

      <Dialog open={open} onOpenChange={handleOpenChange}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>{t('policyAssistant.createPolicy.dialogTitle')}</DialogTitle>
            <DialogDescription>
              {t('policyAssistant.createPolicy.dialogDescription')}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-2">
            {/* Client */}
            <div className="space-y-1.5">
              <label className="text-sm font-medium">
                {t('policyAssistant.createPolicy.client')}
              </label>
              <Autocomplete
                value={clientId}
                onChange={setClientId}
                options={clientOptions}
                onSearch={setClientSearch}
                isLoading={clientsLoading}
                placeholder={t('policyAssistant.createPolicy.clientPlaceholder')}
                searchPlaceholder={t('clients.searchPlaceholder')}
                emptyMessage={t('clients.noClients')}
                className="w-full"
              />
            </div>

            {/* Carrier */}
            <div className="space-y-1.5">
              <label className="text-sm font-medium">
                {t('policyAssistant.createPolicy.carrier')}
              </label>
              <Autocomplete
                value={carrierId}
                onChange={setCarrierId}
                options={carrierOptions}
                onSearch={setCarrierSearch}
                isLoading={carriersLoading}
                placeholder={t('policyAssistant.createPolicy.carrierPlaceholder')}
                searchPlaceholder={t('carriers.searchPlaceholder')}
                emptyMessage={t('carriers.noCarriers')}
                className="w-full"
              />
            </div>

            {/* Line of Business */}
            <div className="space-y-1.5">
              <label className="text-sm font-medium">
                {t('policyAssistant.createPolicy.lineOfBusiness')}
              </label>
              <Select
                options={LOB_OPTIONS}
                value={lineOfBusiness}
                onChange={setLineOfBusiness}
                placeholder={t('policyAssistant.createPolicy.lineOfBusinessPlaceholder')}
                className="w-full"
              />
            </div>

            {/* Dates */}
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1.5">
                <label className="text-sm font-medium">
                  {t('policyAssistant.createPolicy.effectiveDate')}
                </label>
                <DatePicker value={effectiveDate} onChange={setEffectiveDate} />
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium">
                  {t('policyAssistant.createPolicy.expirationDate')}
                </label>
                <DatePicker value={expirationDate} onChange={setExpirationDate} minDate={effectiveDate} />
              </div>
            </div>

            {/* Billing Type + Payment Plan */}
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1.5">
                <label className="text-sm font-medium">
                  {t('policyAssistant.createPolicy.billingType')}
                </label>
                <Select
                  options={BILLING_TYPE_OPTIONS}
                  value={billingType}
                  onChange={setBillingType}
                  className="w-full"
                />
              </div>
              <div className="space-y-1.5">
                <label className="text-sm font-medium">
                  {t('policyAssistant.createPolicy.paymentPlan')}
                </label>
                <Select
                  options={PAYMENT_PLAN_OPTIONS}
                  value={paymentPlan}
                  onChange={setPaymentPlan}
                  className="w-full"
                />
              </div>
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => handleOpenChange(false)} disabled={createPolicy.isPending}>
              {t('common.actions.cancel')}
            </Button>
            <Button onClick={handleCreate} disabled={createPolicy.isPending}>
              {createPolicy.isPending ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  {t('common.form.creating')}
                </>
              ) : (
                t('policyAssistant.createPolicy.button')
              )}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
