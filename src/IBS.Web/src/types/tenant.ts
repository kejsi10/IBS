/**
 * Tenant status options.
 */
export type TenantStatus = 'Active' | 'Suspended' | 'Cancelled';

/**
 * Subscription tier options.
 */
export type SubscriptionTier = 'Basic' | 'Professional' | 'Enterprise';

/**
 * Tenant list item for table views.
 */
export interface TenantListItem {
  id: string;
  name: string;
  subdomain: string;
  status: TenantStatus;
  subscriptionTier: SubscriptionTier;
  createdAt: string;
}

/**
 * Full tenant details including carriers.
 */
export interface TenantDetails {
  id: string;
  name: string;
  subdomain: string;
  status: TenantStatus;
  subscriptionTier: SubscriptionTier;
  defaultCurrency: string;
  carriers: TenantCarrier[];
  createdAt: string;
  updatedAt: string;
}

/**
 * Tenant carrier association.
 */
export interface TenantCarrier {
  carrierId: string;
  agencyCode: string | null;
  commissionRate: number | null;
  isActive: boolean;
}

/**
 * Request to create a new tenant.
 */
export interface CreateTenantRequest {
  name: string;
  subdomain: string;
  subscriptionTier: SubscriptionTier;
}

/**
 * Request to update a tenant.
 */
export interface UpdateTenantRequest {
  name: string;
  settings?: string | null;
}

/**
 * Request to update subscription tier.
 */
export interface UpdateSubscriptionRequest {
  subscriptionTier: SubscriptionTier;
}

/**
 * Request to add a carrier to a tenant.
 */
export interface AddTenantCarrierRequest {
  carrierId: string;
  agencyCode?: string;
  commissionRate?: number;
}

/**
 * Paginated result for tenant list.
 */
export interface TenantPagedResult {
  items: TenantListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
