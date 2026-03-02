import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, ArrowRight, FileWarning } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useToast } from '@/components/ui/toast';
import { Autocomplete, type AutocompleteOption } from '@/components/common/autocomplete';
import { getErrorMessage } from '@/lib/api';
import { useCreateClaim } from '@/hooks';
import { useClients, useClient } from '@/hooks/use-clients';
import { usePolicies } from '@/hooks/use-policies';
import { useDebounce } from '@/hooks/use-debounce';
import { formatCurrency } from '@/lib/format';
import type { LossType } from '@/types/api';

/**
 * Multi-step wizard for filing a new claim (First Notice of Loss).
 */
export function FNOLWizard() {
  const navigate = useNavigate();
  const { addToast } = useToast();
  const { t, i18n } = useTranslation();
  const createClaim = useCreateClaim();
  const [step, setStep] = React.useState(1);

  /** Loss type options, translated. */
  const LOSS_TYPE_OPTIONS: { value: LossType; label: string }[] = React.useMemo(() => [
    { value: 'PropertyDamage', label: t('claims.lossType.propertyDamage') },
    { value: 'Liability', label: t('claims.lossType.liability') },
    { value: 'WorkersComp', label: t('claims.lossType.workersComp') },
    { value: 'Auto', label: t('claims.lossType.auto') },
    { value: 'Professional', label: t('claims.lossType.professional') },
    { value: 'Cyber', label: t('claims.lossType.cyber') },
    { value: 'NaturalDisaster', label: t('claims.lossType.naturalDisaster') },
    { value: 'TheftFraud', label: t('claims.lossType.theftFraud') },
    { value: 'BodilyInjury', label: t('claims.lossType.bodilyInjury') },
    { value: 'Other', label: t('claims.lossType.other') },
  ], [t]);

  const [formData, setFormData] = React.useState({
    policyId: '',
    clientId: '',
    lossDate: '',
    reportedDate: new Date().toISOString().split('T')[0],
    lossType: 'PropertyDamage' as LossType,
    lossDescription: '',
    estimatedLossAmount: '',
    estimatedLossCurrency: 'USD',
  });

  // Selected policy kept in state so its label is resolvable after search clears
  const [selectedPolicyOption, setSelectedPolicyOption] = React.useState<AutocompleteOption | null>(null);

  // Search state for policy autocomplete
  const [policySearch, setPolicySearch] = React.useState('');
  const debouncedPolicySearch = useDebounce(policySearch, 300);
  const { data: policiesData, isFetching: policiesLoading } = usePolicies(
    debouncedPolicySearch ? { search: debouncedPolicySearch } : undefined
  );

  // Search state for client autocomplete
  const [clientSearch, setClientSearch] = React.useState('');
  const debouncedClientSearch = useDebounce(clientSearch, 300);
  const { data: clientsData, isFetching: clientsLoading } = useClients(
    debouncedClientSearch ? { search: debouncedClientSearch } : undefined
  );

  // Always fetch the selected client by ID — guarantees the real displayName is shown
  // regardless of whether the client was chosen directly or auto-filled from a policy.
  const { data: selectedClientData } = useClient(formData.clientId);

  /** Policy options for autocomplete — always includes the selected option so its label is shown. */
  const policyOptions = React.useMemo(() => {
    const searchResults: AutocompleteOption[] =
      debouncedPolicySearch && policiesData?.items
        ? policiesData.items.map((p) => ({
            value: p.id,
            label: p.policyNumber,
            description: `${p.clientName ?? ''} · ${p.status}`,
          }))
        : [];
    // Inject selected option so Autocomplete can resolve its label after search clears
    if (selectedPolicyOption && !searchResults.find((o) => o.value === selectedPolicyOption.value)) {
      return [selectedPolicyOption, ...searchResults];
    }
    return searchResults;
  }, [policiesData, debouncedPolicySearch, selectedPolicyOption]);

  /** Derives a display name from the full Client detail object. */
  const selectedClientLabel = React.useMemo(() => {
    if (!selectedClientData) return '';
    if (selectedClientData.clientType === 'Business') {
      return selectedClientData.businessName ?? '';
    }
    return [selectedClientData.firstName, selectedClientData.lastName].filter(Boolean).join(' ');
  }, [selectedClientData]);

  /** Client options for autocomplete — always includes the selected client so its name is shown. */
  const clientOptions = React.useMemo(() => {
    const searchResults: AutocompleteOption[] =
      debouncedClientSearch && clientsData?.items
        ? clientsData.items.map((c) => ({
            value: c.id,
            label: c.displayName,
            description: c.email,
          }))
        : [];
    // Inject the selected client (from API) so label resolves correctly after search clears
    if (selectedClientData && !searchResults.find((o) => o.value === selectedClientData.id)) {
      return [
        { value: selectedClientData.id, label: selectedClientLabel, description: selectedClientData.email },
        ...searchResults,
      ];
    }
    return searchResults;
  }, [clientsData, debouncedClientSearch, selectedClientData, selectedClientLabel]);

  /** Updates a single form field. */
  const updateField = (field: string, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  /** Handles policy selection from autocomplete. */
  const handlePolicySelect = (policyId: string | undefined) => {
    if (!policyId) {
      updateField('policyId', '');
      setSelectedPolicyOption(null);
      return;
    }
    updateField('policyId', policyId);
    setSelectedPolicyOption(policyOptions.find((o) => o.value === policyId) ?? null);
    // Auto-fill client ID — useClient will resolve the display name via API
    const policy = policiesData?.items?.find((p) => p.id === policyId);
    if (policy?.clientId) {
      updateField('clientId', policy.clientId);
    }
  };

  /** Handles client selection from autocomplete. */
  const handleClientSelect = (clientId: string | undefined) => {
    updateField('clientId', clientId ?? '');
  };

  /** Validates step 1. */
  const canProceedFromStep1 = formData.policyId && formData.clientId;

  /** Validates step 2. */
  const canProceedFromStep2 = formData.lossDate && formData.reportedDate && formData.lossDescription.length >= 10;

  /** Submits the claim. */
  const handleSubmit = async () => {
    try {
      const result = await createClaim.mutateAsync({
        policyId: formData.policyId,
        clientId: formData.clientId,
        lossDate: new Date(formData.lossDate).toISOString(),
        reportedDate: new Date(formData.reportedDate).toISOString(),
        lossType: formData.lossType,
        lossDescription: formData.lossDescription,
        estimatedLossAmount: formData.estimatedLossAmount ? Number(formData.estimatedLossAmount) : undefined,
        estimatedLossCurrency: formData.estimatedLossCurrency || 'USD',
      });
      addToast({
        title: t('claims.fnol.toast.filed'),
        description: t('claims.fnol.toast.filedDesc'),
        variant: 'success',
      });
      navigate(`/claims/${result.id}`);
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: getErrorMessage(err),
        variant: 'error',
      });
    }
  };

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" onClick={() => navigate('/claims')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('common.actions.back')}
        </Button>
        <div>
          <h1 className="text-2xl font-bold flex items-center gap-2">
            <FileWarning className="h-6 w-6" />
            {t('claims.fnol.title')}
          </h1>
          <p className="text-muted-foreground">{t('claims.fnol.subtitle')}</p>
        </div>
      </div>

      {/* Step Indicator */}
      <div className="flex items-center gap-2">
        {[1, 2, 3].map((s) => (
          <React.Fragment key={s}>
            <div className={`flex h-8 w-8 items-center justify-center rounded-full text-sm font-medium ${
              s === step ? 'bg-primary text-primary-foreground' :
              s < step ? 'bg-primary/20 text-primary' :
              'bg-muted text-muted-foreground'
            }`}>
              {s}
            </div>
            {s < 3 && <div className={`h-0.5 flex-1 ${s < step ? 'bg-primary' : 'bg-muted'}`} />}
          </React.Fragment>
        ))}
      </div>

      {/* Step 1: Policy & Client */}
      {step === 1 && (
        <Card>
          <CardHeader>
            <CardTitle>{t('claims.fnol.steps.policyAndClient')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">{t('claims.fnol.policySearch')}</label>
              <Autocomplete
                value={formData.policyId}
                onChange={handlePolicySelect}
                options={policyOptions}
                onSearch={setPolicySearch}
                placeholder={t('claims.fnol.policySearchPlaceholder')}
                searchPlaceholder={t('claims.fnol.policySearchPlaceholder')}
                isLoading={policiesLoading}
                emptyMessage={debouncedPolicySearch ? t('claims.fnol.noResults') : t('claims.fnol.typeToSearch')}
              />
              <p className="mt-1 text-xs text-muted-foreground">{t('claims.fnol.policyIdHint')}</p>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('claims.fnol.clientSearch')}</label>
              <Autocomplete
                value={formData.clientId}
                onChange={handleClientSelect}
                options={clientOptions}
                onSearch={setClientSearch}
                placeholder={t('claims.fnol.clientSearchPlaceholder')}
                searchPlaceholder={t('claims.fnol.clientSearchPlaceholder')}
                isLoading={clientsLoading}
                emptyMessage={debouncedClientSearch ? t('claims.fnol.noResults') : t('claims.fnol.typeToSearch')}
              />
              <p className="mt-1 text-xs text-muted-foreground">{t('claims.fnol.clientIdHint')}</p>
            </div>
            <div className="flex justify-end">
              <Button onClick={() => setStep(2)} disabled={!canProceedFromStep1}>
                {t('claims.fnol.next')}
                <ArrowRight className="ml-2 h-4 w-4" />
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Step 2: Loss Details */}
      {step === 2 && (
        <Card>
          <CardHeader>
            <CardTitle>{t('claims.fnol.steps.incident')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-1">
                  {t('claims.fnol.lossDate')} <span className="text-destructive">*</span>
                </label>
                <Input
                  type="date"
                  value={formData.lossDate}
                  onChange={(e) => updateField('lossDate', e.target.value)}
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">
                  {t('claims.fnol.reportedDate')} <span className="text-destructive">*</span>
                </label>
                <Input
                  type="date"
                  value={formData.reportedDate}
                  onChange={(e) => updateField('reportedDate', e.target.value)}
                />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('claims.fnol.lossType')}</label>
              <select
                value={formData.lossType}
                onChange={(e) => updateField('lossType', e.target.value)}
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
              >
                {LOSS_TYPE_OPTIONS.map((opt) => (
                  <option key={opt.value} value={opt.value}>{opt.label}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">
                {t('claims.fnol.lossDescription')} <span className="text-destructive">*</span>
              </label>
              <Textarea
                value={formData.lossDescription}
                onChange={(e) => updateField('lossDescription', e.target.value)}
                rows={4}
                placeholder={t('claims.fnol.lossDescriptionPlaceholder')}
              />
              <p className={`mt-1 text-xs ${formData.lossDescription.length < 10 ? 'text-destructive' : 'text-muted-foreground'}`}>
                {formData.lossDescription.length < 10
                  ? t('claims.fnol.charMinHint', { remaining: 10 - formData.lossDescription.length })
                  : t('claims.fnol.charCount', { count: formData.lossDescription.length })}
              </p>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium mb-1">
                  {t('claims.fnol.estimatedLossAmount')}
                  <span className="ml-1 text-xs font-normal text-muted-foreground">({t('claims.fnol.optional')})</span>
                </label>
                <Input
                  type="number"
                  step="0.01"
                  value={formData.estimatedLossAmount}
                  onChange={(e) => updateField('estimatedLossAmount', e.target.value)}
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">{t('claims.fnol.currency')}</label>
                <Input
                  value={formData.estimatedLossCurrency}
                  onChange={(e) => updateField('estimatedLossCurrency', e.target.value)}
                />
              </div>
            </div>
            <div className="flex justify-between">
              <Button variant="outline" onClick={() => setStep(1)}>
                <ArrowLeft className="mr-2 h-4 w-4" />
                {t('common.actions.back')}
              </Button>
              <Button onClick={() => setStep(3)} disabled={!canProceedFromStep2}>
                {t('claims.fnol.next')}
                <ArrowRight className="ml-2 h-4 w-4" />
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Step 3: Review & File */}
      {step === 3 && (
        <Card>
          <CardHeader>
            <CardTitle>{t('claims.fnol.steps.details')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="rounded-lg bg-muted p-4 space-y-3">
              <div className="grid grid-cols-2 gap-y-2 text-sm">
                <span className="text-muted-foreground">{t('claims.fnol.reviewPolicy')}</span>
                <span className="font-medium">{selectedPolicyOption?.label ?? formData.policyId}</span>
                <span className="text-muted-foreground">{t('claims.fnol.reviewClient')}</span>
                <span className="font-medium">{selectedClientLabel || formData.clientId}</span>
                <span className="text-muted-foreground">{t('claims.fnol.reviewLossDate')}</span>
                <span className="font-medium">{formData.lossDate}</span>
                <span className="text-muted-foreground">{t('claims.fnol.reviewReportedDate')}</span>
                <span className="font-medium">{formData.reportedDate}</span>
                <span className="text-muted-foreground">{t('claims.fnol.reviewLossType')}</span>
                <span className="font-medium">
                  {LOSS_TYPE_OPTIONS.find((o) => o.value === formData.lossType)?.label || formData.lossType}
                </span>
                {formData.estimatedLossAmount && (
                  <>
                    <span className="text-muted-foreground">{t('claims.fnol.reviewEstimatedLoss')}</span>
                    <span className="font-medium">
                      {formatCurrency(Number(formData.estimatedLossAmount), formData.estimatedLossCurrency || 'USD', i18n.language)}
                    </span>
                  </>
                )}
              </div>
              <div>
                <p className="text-sm text-muted-foreground mb-1">{t('claims.fnol.reviewDescription')}</p>
                <p className="text-sm">{formData.lossDescription}</p>
              </div>
            </div>

            <div className="flex justify-between">
              <Button variant="outline" onClick={() => setStep(2)}>
                <ArrowLeft className="mr-2 h-4 w-4" />
                {t('common.actions.back')}
              </Button>
              <Button onClick={handleSubmit} disabled={createClaim.isPending}>
                <FileWarning className="mr-2 h-4 w-4" />
                {t('claims.fnol.submit')}
              </Button>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
