import * as React from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { ArrowLeft, ArrowRight, Check, Plus, X, Search } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { NativeSelect } from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Autocomplete } from '@/components/common/autocomplete';
import { useToast } from '@/components/ui/toast';
import { useClients } from '@/hooks/use-clients';
import { useCarriers } from '@/hooks/use-carriers';
import { useCreateQuote, useAddCarrierToQuote } from '@/hooks';
import type { LineOfBusiness } from '@/types/api';

/** Wizard step definition. */
interface WizardStep {
  id: string;
  title: string;
  description: string;
}

/** Wizard form data. */
interface QuoteWizardData {
  clientId: string;
  clientName?: string;
  lineOfBusiness: string;
  effectiveDate: string;
  expirationDate: string;
  notes?: string;
  selectedCarriers: Array<{ id: string; name: string }>;
}

/**
 * Multi-step quote creation wizard.
 */
export function QuoteWizard() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const [currentStep, setCurrentStep] = React.useState(0);

  /**
   * Line of business options — depends on t so defined inside component via useMemo.
   */
  const LOB_OPTIONS = React.useMemo(() => [
    { value: '', label: t('quotes.wizard.lob.select') },
    { value: 'PersonalAuto', label: t('quotes.wizard.lob.personalAuto') },
    { value: 'Homeowners', label: t('quotes.wizard.lob.homeowners') },
    { value: 'Renters', label: t('quotes.wizard.lob.renters') },
    { value: 'PersonalUmbrella', label: t('quotes.wizard.lob.personalUmbrella') },
    { value: 'Life', label: t('quotes.wizard.lob.life') },
    { value: 'Health', label: t('quotes.wizard.lob.health') },
    { value: 'GeneralLiability', label: t('quotes.wizard.lob.gl') },
    { value: 'CommercialProperty', label: t('quotes.wizard.lob.commercialProperty') },
    { value: 'WorkersCompensation', label: t('quotes.wizard.lob.workersCompensation') },
    { value: 'CommercialAuto', label: t('quotes.wizard.lob.auto') },
    { value: 'ProfessionalLiability', label: t('quotes.wizard.lob.professionalLiability') },
    { value: 'DirectorsAndOfficers', label: t('quotes.wizard.lob.directorsAndOfficers') },
    { value: 'CyberLiability', label: t('quotes.wizard.lob.cyber') },
    { value: 'BusinessOwnersPolicy', label: t('quotes.wizard.lob.businessOwnersPolicy') },
    { value: 'CommercialUmbrella', label: t('quotes.wizard.lob.commercialUmbrella') },
    { value: 'InlandMarine', label: t('quotes.wizard.lob.inlandMarine') },
    { value: 'Surety', label: t('quotes.wizard.lob.surety') },
  ], [t]);

  /** Wizard step labels — depends on t so defined inside component via useMemo. */
  const steps: WizardStep[] = React.useMemo(() => [
    { id: 'client', title: t('quotes.wizard.steps.client'), description: t('quotes.wizard.steps.client') },
    { id: 'details', title: t('quotes.wizard.steps.details'), description: t('quotes.wizard.steps.details') },
    { id: 'carriers', title: t('quotes.wizard.steps.carriers'), description: t('quotes.wizard.steps.carriers') },
    { id: 'review', title: t('quotes.wizard.steps.review'), description: t('quotes.wizard.steps.review') },
  ], [t]);

  const [data, setData] = React.useState<QuoteWizardData>(() => {
    const tomorrow = new Date(Date.now() + 86400000).toISOString().split('T')[0];
    const nextYear = new Date(Date.now() + 365 * 86400000).toISOString().split('T')[0];
    return {
      clientId: searchParams.get('clientId') || '',
      lineOfBusiness: '',
      effectiveDate: tomorrow,
      expirationDate: nextYear,
      notes: '',
      selectedCarriers: [],
    };
  });

  const createQuote = useCreateQuote();
  const addCarrier = useAddCarrierToQuote();

  /** Updates wizard data with partial changes. */
  const updateData = (updates: Partial<QuoteWizardData>) => {
    setData((prev) => ({ ...prev, ...updates }));
  };

  /** Whether the current step allows proceeding. */
  const canProceed = React.useMemo(() => {
    switch (currentStep) {
      case 0: return !!data.clientId;
      case 1: return !!data.lineOfBusiness && !!data.effectiveDate && !!data.expirationDate;
      case 2: return data.selectedCarriers.length > 0;
      case 3: return true;
      default: return false;
    }
  }, [currentStep, data]);

  const handleNext = () => {
    if (currentStep < steps.length - 1) {
      setCurrentStep(currentStep + 1);
    }
  };

  const handleBack = () => {
    if (currentStep > 0) {
      setCurrentStep(currentStep - 1);
    }
  };

  /** Creates the quote and adds carriers. */
  const handleCreate = async () => {
    try {
      const result = await createQuote.mutateAsync({
        clientId: data.clientId,
        lineOfBusiness: data.lineOfBusiness as LineOfBusiness,
        effectiveDate: data.effectiveDate,
        expirationDate: data.expirationDate,
        notes: data.notes || undefined,
      });

      // Add carriers sequentially
      for (const carrier of data.selectedCarriers) {
        await addCarrier.mutateAsync({
          quoteId: result.id,
          data: { carrierId: carrier.id },
        });
      }

      addToast({
        title: 'Quote created',
        description: `Quote created with ${data.selectedCarriers.length} carrier(s).`,
        variant: 'success',
      });

      navigate(`/quotes/${result.id}`);
    } catch (error) {
      addToast({
        title: 'Error',
        description: error instanceof Error ? error.message : 'Failed to create quote',
        variant: 'error',
      });
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <Button variant="ghost" onClick={() => navigate('/quotes')}>
        <ArrowLeft className="mr-2 h-4 w-4" />
        {t('quotes.detail.backToQuotes')}
      </Button>

      {/* Progress Steps */}
      <div className="flex items-center justify-center">
        {steps.map((step, index) => (
          <div key={step.id} className="flex items-center">
            <button
              type="button"
              onClick={() => index < currentStep && setCurrentStep(index)}
              disabled={index > currentStep}
              className={`flex h-8 w-8 items-center justify-center rounded-full border-2 text-sm font-medium transition-colors ${
                index < currentStep
                  ? 'border-primary bg-primary text-primary-foreground'
                  : index === currentStep
                  ? 'border-primary bg-primary/10 text-primary'
                  : 'border-muted text-muted-foreground'
              }`}
            >
              {index < currentStep ? <Check className="h-4 w-4" /> : index + 1}
            </button>
            {index < steps.length - 1 && (
              <div
                className={`mx-2 h-0.5 w-8 sm:w-16 ${
                  index < currentStep ? 'bg-primary' : 'bg-muted'
                }`}
              />
            )}
          </div>
        ))}
      </div>

      {/* Step Content */}
      <Card>
        <CardHeader>
          <CardTitle>{steps[currentStep].title}</CardTitle>
          <p className="text-sm text-muted-foreground">
            {steps[currentStep].description}
          </p>
        </CardHeader>
        <CardContent>
          {currentStep === 0 && <StepClient data={data} onUpdate={updateData} />}
          {currentStep === 1 && <StepDetails data={data} onUpdate={updateData} lobOptions={LOB_OPTIONS} />}
          {currentStep === 2 && <StepCarriers data={data} onUpdate={updateData} />}
          {currentStep === 3 && <StepReview data={data} onEdit={setCurrentStep} lobOptions={LOB_OPTIONS} />}
        </CardContent>
        <CardFooter className="flex justify-between">
          <Button
            variant="outline"
            onClick={handleBack}
            disabled={currentStep === 0}
          >
            <ArrowLeft className="mr-2 h-4 w-4" />
            {t('common.actions.back')}
          </Button>

          {currentStep < steps.length - 1 ? (
            <Button onClick={handleNext} disabled={!canProceed}>
              Next
              <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          ) : (
            <Button
              onClick={handleCreate}
              disabled={createQuote.isPending || addCarrier.isPending}
            >
              {createQuote.isPending || addCarrier.isPending
                ? t('common.form.creating')
                : t('quotes.wizard.title')}
            </Button>
          )}
        </CardFooter>
      </Card>
    </div>
  );
}

// ========================================
// Step Components
// ========================================

/**
 * Client selection step.
 */
function StepClient({
  data,
  onUpdate,
}: {
  data: QuoteWizardData;
  onUpdate: (updates: Partial<QuoteWizardData>) => void;
}) {
  const { t } = useTranslation();
  const [search, setSearch] = React.useState('');
  const { data: clients, isLoading } = useClients({ search, status: 'Active' });

  const options = React.useMemo(() => {
    return (
      clients?.items?.map((client) => ({
        value: client.id,
        label: client.displayName,
        description: client.email || client.phone || undefined,
      })) ?? []
    );
  }, [clients]);

  const handleSelect = (clientId: string | undefined) => {
    const client = clients?.items?.find((c) => c.id === clientId);
    onUpdate({
      clientId: clientId || '',
      clientName: client?.displayName,
    });
  };

  return (
    <div className="space-y-6">
      <div className="space-y-2">
        <label className="text-sm font-medium">{t('quotes.wizard.steps.client')}</label>
        <Autocomplete
          value={data.clientId}
          onChange={handleSelect}
          options={options}
          onSearch={setSearch}
          isLoading={isLoading}
          placeholder="Search by name, email, or phone..."
          searchPlaceholder="Type to search clients..."
          emptyMessage="No clients found."
          className="w-full"
        />
      </div>

      {data.clientId && data.clientName && (
        <div className="rounded-lg border bg-muted/30 p-4">
          <p className="text-sm text-muted-foreground">Selected Client</p>
          <p className="text-lg font-medium">{data.clientName}</p>
        </div>
      )}
    </div>
  );
}

/**
 * Quote details step (LOB, dates, notes).
 */
function StepDetails({
  data,
  onUpdate,
  lobOptions,
}: {
  data: QuoteWizardData;
  onUpdate: (updates: Partial<QuoteWizardData>) => void;
  lobOptions: Array<{ value: string; label: string }>;
}) {
  const { t } = useTranslation();

  return (
    <div className="space-y-4">
      <div className="space-y-2">
        <label className="text-sm font-medium">{t('quotes.columns.lineOfBusiness')} *</label>
        <NativeSelect
          options={lobOptions}
          value={data.lineOfBusiness}
          onChange={(e) => onUpdate({ lineOfBusiness: e.target.value })}
        />
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="space-y-2">
          <label className="text-sm font-medium">{t('quotes.columns.effectiveDate')} *</label>
          <Input
            type="date"
            value={data.effectiveDate}
            onChange={(e) => onUpdate({ effectiveDate: e.target.value })}
          />
        </div>
        <div className="space-y-2">
          <label className="text-sm font-medium">{t('quotes.columns.expirationDate')} *</label>
          <Input
            type="date"
            value={data.expirationDate}
            onChange={(e) => onUpdate({ expirationDate: e.target.value })}
          />
        </div>
      </div>

      <div className="space-y-2">
        <label className="text-sm font-medium">{t('quotes.detail.notes')}</label>
        <Textarea
          value={data.notes || ''}
          onChange={(e) => onUpdate({ notes: e.target.value })}
          placeholder="Additional notes or requirements..."
          rows={4}
        />
      </div>
    </div>
  );
}

/**
 * Carrier selection step.
 */
function StepCarriers({
  data,
  onUpdate,
}: {
  data: QuoteWizardData;
  onUpdate: (updates: Partial<QuoteWizardData>) => void;
}) {
  const { t } = useTranslation();
  const [search, setSearch] = React.useState('');
  const { data: carriers, isLoading } = useCarriers();

  /** Filtered carriers based on search. */
  const filteredCarriers = React.useMemo(() => {
    const allCarriers = Array.isArray(carriers) ? carriers : [];
    if (!search) return allCarriers;
    const lower = search.toLowerCase();
    return allCarriers.filter(
      (c) =>
        c.name.toLowerCase().includes(lower) ||
        c.code.toLowerCase().includes(lower)
    );
  }, [carriers, search]);

  /** Whether a carrier is already selected. */
  const isSelected = (carrierId: string) =>
    data.selectedCarriers.some((c) => c.id === carrierId);

  /** Adds a carrier to the selection. */
  const handleAdd = (carrierId: string, carrierName: string) => {
    if (!isSelected(carrierId)) {
      onUpdate({
        selectedCarriers: [...data.selectedCarriers, { id: carrierId, name: carrierName }],
      });
    }
  };

  /** Removes a carrier from the selection. */
  const handleRemove = (carrierId: string) => {
    onUpdate({
      selectedCarriers: data.selectedCarriers.filter((c) => c.id !== carrierId),
    });
  };

  return (
    <div className="space-y-4">
      {/* Selected Carriers */}
      {data.selectedCarriers.length > 0 && (
        <div className="space-y-2">
          <label className="text-sm font-medium">
            {t('quotes.wizard.steps.carriers')} ({data.selectedCarriers.length})
          </label>
          <div className="flex flex-wrap gap-2">
            {data.selectedCarriers.map((carrier) => (
              <Badge key={carrier.id} variant="secondary" className="gap-1 pr-1">
                {carrier.name}
                <button
                  type="button"
                  onClick={() => handleRemove(carrier.id)}
                  className="ml-1 rounded-full hover:bg-muted"
                >
                  <X className="h-3 w-3" />
                </button>
              </Badge>
            ))}
          </div>
        </div>
      )}

      {/* Search */}
      <div className="relative">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          placeholder={t('carriers.searchPlaceholder')}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="pl-9"
        />
      </div>

      {/* Carrier List */}
      <div className="max-h-64 overflow-y-auto rounded-md border">
        {isLoading && (
          <div className="p-4 space-y-2">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </div>
        )}
        {!isLoading && filteredCarriers.length === 0 && (
          <p className="p-4 text-center text-sm text-muted-foreground">
            {t('carriers.noCarriers')}
          </p>
        )}
        {!isLoading &&
          filteredCarriers.map((carrier) => (
            <button
              key={carrier.id}
              type="button"
              disabled={isSelected(carrier.id)}
              onClick={() => handleAdd(carrier.id, carrier.name)}
              className={`flex w-full items-center justify-between px-4 py-3 text-left text-sm transition-colors hover:bg-muted/50 border-b last:border-b-0 ${
                isSelected(carrier.id) ? 'bg-muted/30 text-muted-foreground' : ''
              }`}
            >
              <div>
                <p className="font-medium">{carrier.name}</p>
                <p className="text-xs text-muted-foreground">{carrier.code}</p>
              </div>
              {isSelected(carrier.id) ? (
                <Check className="h-4 w-4 text-primary" />
              ) : (
                <Plus className="h-4 w-4 text-muted-foreground" />
              )}
            </button>
          ))}
      </div>
    </div>
  );
}

/**
 * Review step showing all wizard data.
 */
function StepReview({
  data,
  onEdit,
  lobOptions,
}: {
  data: QuoteWizardData;
  onEdit: (step: number) => void;
  lobOptions: Array<{ value: string; label: string }>;
}) {
  const { t } = useTranslation();
  const lobLabel =
    lobOptions.find((o) => o.value === data.lineOfBusiness)?.label || data.lineOfBusiness;

  return (
    <div className="space-y-4">
      {/* Client */}
      <div className="flex items-center justify-between rounded-lg border p-4">
        <div>
          <p className="text-sm text-muted-foreground">{t('quotes.wizard.steps.client')}</p>
          <p className="font-medium">{data.clientName || data.clientId}</p>
        </div>
        <Button variant="ghost" size="sm" onClick={() => onEdit(0)}>
          {t('common.actions.edit')}
        </Button>
      </div>

      {/* Details */}
      <div className="flex items-center justify-between rounded-lg border p-4">
        <div className="space-y-1">
          <p className="text-sm text-muted-foreground">{t('quotes.wizard.steps.details')}</p>
          <p className="font-medium">{lobLabel}</p>
          <p className="text-sm text-muted-foreground">
            {new Date(data.effectiveDate).toLocaleDateString()} -{' '}
            {new Date(data.expirationDate).toLocaleDateString()}
          </p>
          {data.notes && (
            <p className="text-sm text-muted-foreground">{t('quotes.detail.notes')}: {data.notes}</p>
          )}
        </div>
        <Button variant="ghost" size="sm" onClick={() => onEdit(1)}>
          {t('common.actions.edit')}
        </Button>
      </div>

      {/* Carriers */}
      <div className="flex items-center justify-between rounded-lg border p-4">
        <div>
          <p className="text-sm text-muted-foreground">
            {t('quotes.wizard.steps.carriers')} ({data.selectedCarriers.length})
          </p>
          <div className="flex flex-wrap gap-1 mt-1">
            {data.selectedCarriers.map((carrier) => (
              <Badge key={carrier.id} variant="secondary">
                {carrier.name}
              </Badge>
            ))}
          </div>
        </div>
        <Button variant="ghost" size="sm" onClick={() => onEdit(2)}>
          {t('common.actions.edit')}
        </Button>
      </div>
    </div>
  );
}
