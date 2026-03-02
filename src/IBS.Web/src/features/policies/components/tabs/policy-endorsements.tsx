import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Plus, Check, X, FileText } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Badge, type BadgeVariant } from '@/components/ui/badge';
import { useToast } from '@/components/ui/toast';
import {
  useAddEndorsement,
  useApproveEndorsement,
  useIssueEndorsement,
  useRejectEndorsement,
} from '@/hooks/use-policies';
import { addEndorsementSchema, type AddEndorsementFormData } from '@/lib/validations/policy';
import type { Endorsement, Policy } from '@/types/api';

/**
 * Props for the PolicyEndorsementsTab component.
 */
export interface PolicyEndorsementsTabProps {
  policy: Policy;
}

/**
 * Status badge variants.
 */
const statusVariants: Record<string, BadgeVariant> = {
  Pending: 'warning',
  Approved: 'success',
  Issued: 'success',
  Rejected: 'error',
  Cancelled: 'outline',
};

/**
 * Endorsements tab for policy detail page.
 */
export function PolicyEndorsementsTab({ policy }: PolicyEndorsementsTabProps) {
  const { t } = useTranslation();
  const [showAddForm, setShowAddForm] = React.useState(false);
  const { addToast } = useToast();
  const addEndorsement = useAddEndorsement();
  const approveEndorsement = useApproveEndorsement();
  const issueEndorsement = useIssueEndorsement();
  const rejectEndorsement = useRejectEndorsement();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AddEndorsementFormData>({
    resolver: zodResolver(addEndorsementSchema),
    defaultValues: {
      type: '',
      effectiveDate: new Date().toISOString().split('T')[0],
      description: '',
      premiumChange: 0,
    },
  });

  const canAddEndorsement = policy.status === 'Active';

  const handleAddEndorsement = async (data: AddEndorsementFormData) => {
    try {
      await addEndorsement.mutateAsync({
        policyId: policy.id,
        data: {
          type: data.type,
          effectiveDate: data.effectiveDate,
          description: data.description,
          premiumChange: data.premiumChange ?? 0,
        },
      });
      addToast({
        title: t('policies.detail.endorsements.endorsementAdded'),
        description: t('policies.detail.endorsements.endorsementAddedDesc'),
        variant: 'success',
      });
      reset();
      setShowAddForm(false);
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('policies.detail.endorsements.addError'),
        variant: 'error',
      });
    }
  };

  const handleApprove = async (endorsement: Endorsement) => {
    try {
      await approveEndorsement.mutateAsync({
        policyId: policy.id,
        endorsementId: endorsement.id,
      });
      addToast({
        title: t('policies.detail.endorsements.endorsementApproved'),
        description: t('policies.detail.endorsements.endorsementApprovedDesc'),
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('policies.detail.endorsements.approveError'),
        variant: 'error',
      });
    }
  };

  const handleIssue = async (endorsement: Endorsement) => {
    try {
      await issueEndorsement.mutateAsync({
        policyId: policy.id,
        endorsementId: endorsement.id,
      });
      addToast({
        title: t('policies.detail.endorsements.endorsementIssued'),
        description: t('policies.detail.endorsements.endorsementIssuedDesc'),
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('policies.detail.endorsements.issueError'),
        variant: 'error',
      });
    }
  };

  const handleReject = async (endorsement: Endorsement) => {
    try {
      await rejectEndorsement.mutateAsync({
        policyId: policy.id,
        endorsementId: endorsement.id,
        data: { reason: 'Rejected by user' },
      });
      addToast({
        title: t('policies.detail.endorsements.endorsementRejected'),
        description: t('policies.detail.endorsements.endorsementRejectedDesc'),
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('policies.detail.endorsements.rejectError'),
        variant: 'error',
      });
    }
  };

  // Sort endorsements by date (newest first)
  const sortedEndorsements = [...policy.endorsements].sort(
    (a, b) => new Date(b.effectiveDate).getTime() - new Date(a.effectiveDate).getTime()
  );

  return (
    <div className="space-y-6">
      {/* Add Endorsement Form */}
      {canAddEndorsement && showAddForm && (
        <Card>
          <CardHeader>
            <CardTitle>{t('policies.detail.endorsements.addEndorsement')}</CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSubmit(handleAddEndorsement)} className="space-y-4">
              <div className="grid gap-4 sm:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="endorsementType">{t('policies.detail.endorsements.typeLabel')} *</Label>
                  <Input
                    id="endorsementType"
                    {...register('type')}
                    error={errors.type?.message}
                    placeholder={t('policies.detail.endorsements.typePlaceholder')}
                  />
                </div>
                <div className="space-y-2">
                  <Label htmlFor="endorsementDate">{t('policies.detail.endorsements.effectiveDateLabel')} *</Label>
                  <Input
                    id="endorsementDate"
                    type="date"
                    {...register('effectiveDate')}
                    error={errors.effectiveDate?.message}
                  />
                </div>
              </div>
              <div className="space-y-2">
                <Label htmlFor="endorsementDescription">{t('policies.detail.endorsements.description')} *</Label>
                <Textarea
                  id="endorsementDescription"
                  {...register('description')}
                  placeholder={t('policies.detail.endorsements.descriptionPlaceholder')}
                  rows={3}
                />
                {errors.description && (
                  <p className="text-sm text-destructive">{errors.description.message}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="premiumChange">{t('policies.detail.endorsements.premiumChangeLabel')}</Label>
                <Input
                  id="premiumChange"
                  type="number"
                  {...register('premiumChange', { valueAsNumber: true })}
                  placeholder="0"
                />
                <p className="text-xs text-muted-foreground">
                  {t('policies.detail.endorsements.premiumChangeHint')}
                </p>
              </div>
              <div className="flex justify-end gap-3">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => {
                    reset();
                    setShowAddForm(false);
                  }}
                >
                  {t('common.actions.cancel')}
                </Button>
                <Button type="submit" disabled={addEndorsement.isPending}>
                  {addEndorsement.isPending ? t('policies.detail.endorsements.submitting') : t('policies.detail.endorsements.addEndorsement')}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      {/* Endorsements List */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>{t('policies.detail.endorsements.endorsementsTitle')}</CardTitle>
          {canAddEndorsement && !showAddForm && (
            <Button variant="outline" size="sm" onClick={() => setShowAddForm(true)}>
              <Plus className="mr-2 h-4 w-4" />
              {t('policies.detail.endorsements.addEndorsement')}
            </Button>
          )}
        </CardHeader>
        <CardContent>
          {sortedEndorsements.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-8 text-muted-foreground">
              <FileText className="h-12 w-12 mb-2" />
              <p>{t('policies.detail.endorsements.noEndorsements')}</p>
            </div>
          ) : (
            <div className="relative">
              {/* Timeline line */}
              <div className="absolute left-4 top-0 bottom-0 w-0.5 bg-border" />

              <div className="space-y-6">
                {sortedEndorsements.map((endorsement) => (
                  <div key={endorsement.id} className="relative pl-10">
                    {/* Timeline dot */}
                    <div className="absolute left-2.5 top-1 h-3 w-3 rounded-full border-2 border-background bg-primary" />

                    <Card>
                      <CardContent className="p-4">
                        <div className="flex items-start justify-between">
                          <div>
                            <div className="flex items-center gap-2">
                              <span className="font-mono text-sm text-muted-foreground">
                                {endorsement.endorsementNumber}
                              </span>
                              <Badge variant={statusVariants[endorsement.status] || 'secondary'}>
                                {endorsement.status}
                              </Badge>
                            </div>
                            <h4 className="mt-1 font-medium">{endorsement.type}</h4>
                            <p className="mt-1 text-sm text-muted-foreground">
                              {endorsement.description}
                            </p>
                            <div className="mt-2 flex gap-4 text-sm">
                              <span className="text-muted-foreground">
                                {t('policies.detail.endorsements.effectiveLabel')} {new Date(endorsement.effectiveDate).toLocaleDateString()}
                              </span>
                              {endorsement.premiumChange !== 0 && (
                                <span
                                  className={
                                    endorsement.premiumChange > 0
                                      ? 'text-green-600'
                                      : 'text-red-600'
                                  }
                                >
                                  {endorsement.premiumChange > 0 ? '+' : ''}
                                  {new Intl.NumberFormat('en-US', {
                                    style: 'currency',
                                    currency: policy.currency || 'USD',
                                  }).format(endorsement.premiumChange)}
                                </span>
                              )}
                            </div>
                          </div>

                          {/* Actions based on status */}
                          <div className="flex gap-1">
                            {endorsement.status === 'Pending' && (
                              <>
                                <Button
                                  variant="ghost"
                                  size="icon"
                                  className="h-8 w-8"
                                  onClick={() => handleApprove(endorsement)}
                                  title={t('policies.detail.endorsements.approve')}
                                >
                                  <Check className="h-4 w-4 text-green-600" />
                                </Button>
                                <Button
                                  variant="ghost"
                                  size="icon"
                                  className="h-8 w-8"
                                  onClick={() => handleReject(endorsement)}
                                  title={t('policies.detail.endorsements.reject')}
                                >
                                  <X className="h-4 w-4 text-destructive" />
                                </Button>
                              </>
                            )}
                            {endorsement.status === 'Approved' && (
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => handleIssue(endorsement)}
                              >
                                {t('policies.detail.endorsements.issue')}
                              </Button>
                            )}
                          </div>
                        </div>
                      </CardContent>
                    </Card>
                  </div>
                ))}
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
