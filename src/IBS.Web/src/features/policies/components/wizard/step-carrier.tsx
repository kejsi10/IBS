import * as React from 'react';
import { useTranslation } from 'react-i18next';
import { Autocomplete } from '@/components/common/autocomplete';
import { Badge } from '@/components/ui/badge';
import { useCarriers, useCarrier } from '@/hooks/use-carriers';
import type { WizardData } from '../../policy-wizard';
import type { LineOfBusiness } from '@/types/api';

/**
 * Props for the StepCarrier component.
 */
export interface StepCarrierProps {
  data: Partial<WizardData>;
  onUpdate: (updates: Partial<WizardData>) => void;
}

/**
 * Formats line of business for display.
 */
function formatLob(lob: string): string {
  return lob.replace(/([A-Z])/g, ' $1').trim();
}

/**
 * Carrier and product selection step of the policy wizard.
 */
export function StepCarrier({ data, onUpdate }: StepCarrierProps) {
  const { t } = useTranslation();
  const [carrierSearch, setCarrierSearch] = React.useState('');
  const { data: carriers, isLoading: carriersLoading } = useCarriers();
  const { data: selectedCarrier } = useCarrier(data.carrierId || '');

  // Convert carriers to autocomplete options
  const carrierOptions = React.useMemo(() => {
    let items = carriers ?? [];
    if (carrierSearch) {
      const query = carrierSearch.toLowerCase();
      items = items.filter(
        (c) =>
          c.name.toLowerCase().includes(query) ||
          c.code.toLowerCase().includes(query)
      );
    }
    return items.map((carrier) => ({
      value: carrier.id,
      label: carrier.name,
      description: carrier.amBestRating ? `AM Best: ${carrier.amBestRating}` : undefined,
    }));
  }, [carriers, carrierSearch]);

  // Get products for selected carrier
  const productOptions = React.useMemo(() => {
    return (
      selectedCarrier?.products
        ?.filter((p) => p.isActive)
        .map((product) => ({
          value: product.id,
          label: product.name,
          description: formatLob(product.lineOfBusiness),
          lineOfBusiness: product.lineOfBusiness,
        })) ?? []
    );
  }, [selectedCarrier]);

  const handleCarrierSelect = (carrierId: string | undefined) => {
    const carrier = carriers?.find((c) => c.id === carrierId);
    onUpdate({
      carrierId: carrierId || '',
      carrierName: carrier?.name,
      productId: '', // Reset product when carrier changes
      productName: undefined,
      lineOfBusiness: undefined,
    });
  };

  const handleProductSelect = (productId: string | undefined) => {
    const product = selectedCarrier?.products?.find((p) => p.id === productId);
    onUpdate({
      productId: productId || '',
      productName: product?.name,
      lineOfBusiness: product?.lineOfBusiness as LineOfBusiness,
    });
  };

  return (
    <div className="space-y-6">
      {/* Carrier Selection */}
      <div className="space-y-2">
        <label className="text-sm font-medium">{t('policies.wizard.steps.carrier')}</label>
        <Autocomplete
          value={data.carrierId}
          onChange={handleCarrierSelect}
          options={carrierOptions}
          onSearch={setCarrierSearch}
          isLoading={carriersLoading}
          placeholder={t('carriers.searchPlaceholder')}
          searchPlaceholder={t('carriers.searchPlaceholder')}
          emptyMessage={t('carriers.noCarriers')}
          className="w-full"
        />
      </div>

      {/* Product Selection (only show when carrier selected) */}
      {data.carrierId && (
        <div className="space-y-2">
          <label className="text-sm font-medium">{t('carriers.detail.products.title')}</label>
          {productOptions.length === 0 ? (
            <div className="rounded-lg border border-dashed p-4 text-center text-muted-foreground">
              No active products available for this carrier.
            </div>
          ) : (
            <div className="grid gap-3 sm:grid-cols-2">
              {productOptions.map((product) => (
                <button
                  key={product.value}
                  type="button"
                  onClick={() => handleProductSelect(product.value)}
                  className={`rounded-lg border-2 p-4 text-left transition-colors ${
                    data.productId === product.value
                      ? 'border-primary bg-primary/5'
                      : 'border-border hover:border-primary/50'
                  }`}
                >
                  <div className="font-medium">{product.label}</div>
                  <Badge variant="outline" className="mt-1">
                    {product.description}
                  </Badge>
                </button>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Selected Summary */}
      {data.carrierId && data.productId && (
        <div className="rounded-lg border bg-muted/30 p-4">
          <div className="grid gap-4 sm:grid-cols-2">
            <div>
              <p className="text-sm text-muted-foreground">Carrier</p>
              <p className="font-medium">{data.carrierName}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Product</p>
              <p className="font-medium">{data.productName}</p>
              {data.lineOfBusiness && (
                <Badge variant="outline" className="mt-1">
                  {formatLob(data.lineOfBusiness)}
                </Badge>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
