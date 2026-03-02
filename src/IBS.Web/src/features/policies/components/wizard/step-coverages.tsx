import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import type { WizardData } from '../../policy-wizard';

/**
 * Props for the StepCoverages component.
 */
export interface StepCoveragesProps {
  data: Partial<WizardData>;
  onUpdate: (updates: Partial<WizardData>) => void;
}

/**
 * Coverage item type.
 */
interface Coverage {
  code: string;
  name: string;
  premium: number;
  limit?: number;
  deductible?: number;
}

/**
 * Common coverage templates.
 */
const coverageTemplates: Coverage[] = [
  { code: 'GL', name: 'General Liability', premium: 1000, limit: 1000000, deductible: 1000 },
  { code: 'PROP', name: 'Property', premium: 500, limit: 500000, deductible: 2500 },
  { code: 'BI', name: 'Business Interruption', premium: 300, limit: 100000 },
  { code: 'CYBER', name: 'Cyber Liability', premium: 750, limit: 250000, deductible: 5000 },
  { code: 'EPL', name: 'Employment Practices', premium: 600, limit: 500000, deductible: 10000 },
];

/**
 * Coverages step of the policy wizard.
 */
export function StepCoverages({ data, onUpdate }: StepCoveragesProps) {
  const { t } = useTranslation();
  const [showAdd, setShowAdd] = React.useState(false);
  const [newCoverage, setNewCoverage] = React.useState<Coverage>({
    code: '',
    name: '',
    premium: 0,
    limit: undefined,
    deductible: undefined,
  });

  const coverages = data.coverages || [];

  const handleAddTemplate = (template: Coverage) => {
    if (!coverages.some((c) => c.code === template.code)) {
      onUpdate({ coverages: [...coverages, { ...template }] });
    }
  };

  const handleAddCustom = () => {
    if (newCoverage.code && newCoverage.name && newCoverage.premium > 0) {
      onUpdate({ coverages: [...coverages, { ...newCoverage }] });
      setNewCoverage({ code: '', name: '', premium: 0 });
      setShowAdd(false);
    }
  };

  const handleRemove = (code: string) => {
    onUpdate({ coverages: coverages.filter((c) => c.code !== code) });
  };

  const handleUpdate = (code: string, field: keyof Coverage, value: string | number) => {
    onUpdate({
      coverages: coverages.map((c) =>
        c.code === code ? { ...c, [field]: value } : c
      ),
    });
  };

  const totalPremium = coverages.reduce((sum, c) => sum + c.premium, 0);

  return (
    <div className="space-y-6">
      {/* Quick Add Templates */}
      <div className="space-y-2">
        <Label>{t('policies.detail.coverages.addCoverage')}</Label>
        <div className="flex flex-wrap gap-2">
          {coverageTemplates.map((template) => {
            const isAdded = coverages.some((c) => c.code === template.code);
            return (
              <Button
                key={template.code}
                variant={isAdded ? 'secondary' : 'outline'}
                size="sm"
                onClick={() => handleAddTemplate(template)}
                disabled={isAdded}
              >
                {isAdded ? '✓ ' : '+ '}
                {template.name}
              </Button>
            );
          })}
        </div>
      </div>

      {/* Added Coverages */}
      {coverages.length > 0 && (
        <div className="space-y-3">
          <Label>{t('policies.detail.tabs.coverages')}</Label>
          {coverages.map((coverage) => (
            <Card key={coverage.code}>
              <CardContent className="p-4">
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center gap-2">
                      <span className="font-mono text-xs text-muted-foreground">
                        {coverage.code}
                      </span>
                      <span className="font-medium">{coverage.name}</span>
                    </div>
                    <div className="mt-2 grid gap-4 sm:grid-cols-3">
                      <div className="space-y-1">
                        <Label className="text-xs">Premium</Label>
                        <Input
                          type="number"
                          value={coverage.premium}
                          onChange={(e) =>
                            handleUpdate(coverage.code, 'premium', Number(e.target.value))
                          }
                          className="h-8"
                        />
                      </div>
                      <div className="space-y-1">
                        <Label className="text-xs">Limit</Label>
                        <Input
                          type="number"
                          value={coverage.limit || ''}
                          onChange={(e) =>
                            handleUpdate(
                              coverage.code,
                              'limit',
                              e.target.value ? Number(e.target.value) : 0
                            )
                          }
                          className="h-8"
                          placeholder="—"
                        />
                      </div>
                      <div className="space-y-1">
                        <Label className="text-xs">Deductible</Label>
                        <Input
                          type="number"
                          value={coverage.deductible || ''}
                          onChange={(e) =>
                            handleUpdate(
                              coverage.code,
                              'deductible',
                              e.target.value ? Number(e.target.value) : 0
                            )
                          }
                          className="h-8"
                          placeholder="—"
                        />
                      </div>
                    </div>
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    className="h-8 w-8 text-destructive"
                    onClick={() => handleRemove(coverage.code)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {/* Custom Coverage Form */}
      {showAdd ? (
        <Card>
          <CardContent className="p-4">
            <h4 className="mb-3 font-medium">Add Custom Coverage</h4>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="space-y-2">
                <Label htmlFor="coverageCode">Code</Label>
                <Input
                  id="coverageCode"
                  value={newCoverage.code}
                  onChange={(e) =>
                    setNewCoverage({ ...newCoverage, code: e.target.value.toUpperCase() })
                  }
                  placeholder="e.g., CUSTOM1"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="coverageName">Name</Label>
                <Input
                  id="coverageName"
                  value={newCoverage.name}
                  onChange={(e) => setNewCoverage({ ...newCoverage, name: e.target.value })}
                  placeholder="Coverage name"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="coveragePremium">Premium</Label>
                <Input
                  id="coveragePremium"
                  type="number"
                  value={newCoverage.premium || ''}
                  onChange={(e) =>
                    setNewCoverage({ ...newCoverage, premium: Number(e.target.value) })
                  }
                  placeholder="0"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="coverageLimit">Limit (Optional)</Label>
                <Input
                  id="coverageLimit"
                  type="number"
                  value={newCoverage.limit || ''}
                  onChange={(e) =>
                    setNewCoverage({
                      ...newCoverage,
                      limit: e.target.value ? Number(e.target.value) : undefined,
                    })
                  }
                  placeholder="—"
                />
              </div>
            </div>
            <div className="mt-4 flex justify-end gap-2">
              <Button variant="outline" size="sm" onClick={() => setShowAdd(false)}>
                {t('common.actions.cancel')}
              </Button>
              <Button size="sm" onClick={handleAddCustom}>
                {t('policies.detail.coverages.addCoverage')}
              </Button>
            </div>
          </CardContent>
        </Card>
      ) : (
        <Button variant="outline" onClick={() => setShowAdd(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Add Custom Coverage
        </Button>
      )}

      {/* Total Premium */}
      <div className="rounded-lg border bg-muted/30 p-4">
        <div className="flex items-center justify-between">
          <span className="font-medium">Total Premium</span>
          <span className="text-xl font-bold">
            {new Intl.NumberFormat('en-US', {
              style: 'currency',
              currency: 'USD',
            }).format(totalPremium)}
          </span>
        </div>
      </div>
    </div>
  );
}
