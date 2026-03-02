import * as React from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, ArrowRight, Check } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { useToast } from '@/components/ui/toast';
import { getErrorMessage } from '@/lib/api';
import { StepClient } from './components/wizard/step-client';
import { StepCarrier } from './components/wizard/step-carrier';
import { StepDetails } from './components/wizard/step-details';
import { StepCoverages } from './components/wizard/step-coverages';
import { StepReview } from './components/wizard/step-review';
import { useCreatePolicy, useBindPolicy } from '@/hooks/use-policies';
import { policiesService } from '@/services';
import type { CreatePolicyRequest, LineOfBusiness } from '@/types/api';

/** Wizard step definition */
interface WizardStep {
  id: string;
  title: string;
  description: string;
}

/** Wizard form data */
export interface WizardData {
  clientId: string;
  clientName?: string;
  carrierId: string;
  carrierName?: string;
  productId: string;
  productName?: string;
  lineOfBusiness?: LineOfBusiness;
  policyNumber?: string;
  effectiveDate: string;
  expirationDate: string;
  billingType: 'DirectBill' | 'AgencyBill';
  paymentPlan: 'Annual' | 'SemiAnnual' | 'Quarterly' | 'Monthly';
  description?: string;
  coverages: Array<{
    code: string;
    name: string;
    premium: number;
    limit?: number;
    deductible?: number;
  }>;
}

/**
 * Multi-step policy creation wizard.
 */
export function PolicyWizard() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { t } = useTranslation();
  const { addToast } = useToast();
  const [currentStep, setCurrentStep] = React.useState(0);

  const steps: WizardStep[] = React.useMemo(() => [
    { id: 'client', title: t('policies.wizard.steps.client'), description: t('policies.wizard.client.selectedClient') },
    { id: 'carrier', title: t('policies.wizard.steps.carrier'), description: t('policies.wizard.carrier.carrier') },
    { id: 'details', title: t('policies.wizard.steps.details'), description: t('policies.wizard.details.termSummary') },
    { id: 'coverages', title: t('policies.wizard.steps.coverages'), description: t('policies.wizard.coverages.addCustom') },
    { id: 'review', title: t('policies.wizard.steps.review'), description: t('policies.wizard.review.createPolicy') },
  ], [t]);

  const [data, setData] = React.useState<Partial<WizardData>>({
    clientId: searchParams.get('clientId') || '',
    effectiveDate: new Date(Date.now() + 86400000).toISOString().split('T')[0],
    expirationDate: new Date(Date.now() + 365 * 86400000).toISOString().split('T')[0],
    billingType: 'DirectBill',
    paymentPlan: 'Annual',
    coverages: [],
  });

  const createPolicy = useCreatePolicy();
  const bindPolicy = useBindPolicy();

  const updateData = (updates: Partial<WizardData>) => {
    setData((prev) => ({ ...prev, ...updates }));
  };

  const canProceed = React.useMemo(() => {
    switch (currentStep) {
      case 0: return !!data.clientId;
      case 1: return !!data.carrierId && !!data.productId;
      case 2: return !!data.effectiveDate && !!data.expirationDate;
      case 3: return true;
      case 4: return true;
      default: return false;
    }
  }, [currentStep, data]);

  const hasCoverages = (data.coverages?.length ?? 0) > 0;

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

  const handleCreate = async (andBind = false) => {
    try {
      const request: CreatePolicyRequest = {
        clientId: data.clientId!,
        carrierId: data.carrierId!,
        lineOfBusiness: data.lineOfBusiness || 'GeneralLiability',
        policyType: data.productName || 'Standard',
        effectiveDate: data.effectiveDate!,
        expirationDate: data.expirationDate!,
        billingType: data.billingType,
        paymentPlan: data.paymentPlan,
        notes: data.description,
      };

      const result = await createPolicy.mutateAsync(request);

      // Add coverages collected in the wizard before binding
      if ((data.coverages?.length ?? 0) > 0) {
        for (const coverage of data.coverages!) {
          await policiesService.addCoverage(result.id, {
            code: coverage.code,
            name: coverage.name,
            premiumAmount: coverage.premium,
            limitAmount: coverage.limit,
            deductibleAmount: coverage.deductible,
          });
        }
      }

      if (andBind) {
        await bindPolicy.mutateAsync(result.id);
        addToast({
          title: t('policies.wizard.toast.createdAndBound'),
          description: t('policies.wizard.toast.createdAndBoundDesc'),
          variant: 'success',
        });
      } else {
        addToast({
          title: t('policies.wizard.toast.created'),
          description: t('policies.wizard.toast.createdDesc'),
          variant: 'success',
        });
      }

      navigate(`/policies/${result.id}`);
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: getErrorMessage(error),
        variant: 'error',
      });
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <Button variant="ghost" onClick={() => navigate('/policies')}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            {t('policies.detail.backToPolicies')}
          </Button>
        </div>
      </div>

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
        </CardHeader>
        <CardContent>
          {currentStep === 0 && (
            <StepClient data={data} onUpdate={updateData} />
          )}
          {currentStep === 1 && (
            <StepCarrier data={data} onUpdate={updateData} />
          )}
          {currentStep === 2 && (
            <StepDetails data={data} onUpdate={updateData} />
          )}
          {currentStep === 3 && (
            <StepCoverages data={data} onUpdate={updateData} />
          )}
          {currentStep === 4 && (
            <StepReview data={data} onEdit={setCurrentStep} />
          )}
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
              {t('policies.wizard.next')}
              <ArrowRight className="ml-2 h-4 w-4" />
            </Button>
          ) : (
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                onClick={() => handleCreate(false)}
                disabled={createPolicy.isPending}
              >
                {t('policies.wizard.createDraft')}
              </Button>
              <div className="flex flex-col items-end gap-1">
                <Button
                  onClick={() => handleCreate(true)}
                  disabled={createPolicy.isPending || bindPolicy.isPending || !hasCoverages}
                >
                  {t('policies.wizard.createAndBind')}
                </Button>
                {!hasCoverages && (
                  <p className="text-xs text-muted-foreground">{t('policies.wizard.createAndBindHint')}</p>
                )}
              </div>
            </div>
          )}
        </CardFooter>
      </Card>
    </div>
  );
}
