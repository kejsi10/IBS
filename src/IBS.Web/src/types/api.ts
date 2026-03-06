// ============================================
// Common Types
// ============================================

/**
 * Generic paginated result wrapper for list endpoints.
 */
export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Standard API error response.
 */
export interface ApiError {
  error: string;
}

// ============================================
// Auth Types
// ============================================

/**
 * Login request payload.
 */
export interface LoginRequest {
  email: string;
  password: string;
  tenantId?: string;
}

/**
 * Login response with tokens and user info.
 */
export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  userId: string;
  tenantId: string;
  email: string;
  fullName: string;
  roles: string[];
}

/**
 * User profile information.
 */
export interface User {
  id: string;
  tenantId: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  title?: string;
  phoneNumber?: string;
  isActive: boolean;
  lastLoginAt?: string;
  roles: string[];
}

// ============================================
// Carrier Types
// ============================================

/**
 * Carrier status options.
 */
export type CarrierStatus = 'Active' | 'Inactive' | 'Suspended';

/**
 * Summary information for carrier list views.
 */
export interface CarrierSummary {
  id: string;
  code: string;
  name: string;
  status: CarrierStatus;
  amBestRating?: string;
}

/**
 * Full carrier details including products and appetites.
 */
export interface Carrier extends CarrierSummary {
  legalName?: string;
  naicCode?: string;
  websiteUrl?: string;
  apiEndpoint?: string;
  notes?: string;
  products: Product[];
  appetites: Appetite[];
  createdAt: string;
  updatedAt?: string;
}

/**
 * Insurance product offered by a carrier.
 */
export interface Product {
  id: string;
  code: string;
  name: string;
  lineOfBusiness: LineOfBusiness;
  description?: string;
  minimumPremium?: number;
  effectiveDate?: string;
  expirationDate?: string;
  isActive: boolean;
}

/**
 * Carrier appetite/underwriting criteria.
 */
export interface Appetite {
  id: string;
  lineOfBusiness: LineOfBusiness;
  states: string;
  minYearsInBusiness?: number;
  maxYearsInBusiness?: number;
  minAnnualRevenue?: number;
  maxAnnualRevenue?: number;
  acceptedIndustries?: string;
  excludedIndustries?: string;
  isActive: boolean;
}

/**
 * Request payload for creating a new carrier.
 */
export interface CreateCarrierRequest {
  name: string;
  code: string;
  legalName?: string;
  amBestRating?: string;
  naicCode?: string;
  websiteUrl?: string;
  notes?: string;
}

/**
 * Request payload for updating a carrier.
 */
export interface UpdateCarrierRequest {
  name: string;
  legalName?: string;
  amBestRating?: string;
  naicCode?: string;
  websiteUrl?: string;
  apiEndpoint?: string;
  notes?: string;
}

/**
 * Request payload for adding a product to a carrier.
 */
export interface AddProductRequest {
  code: string;
  name: string;
  lineOfBusiness: LineOfBusiness;
  description?: string;
  minimumPremium?: number;
  effectiveDate?: string;
  expirationDate?: string;
}

/**
 * Request payload for adding appetite to a carrier.
 */
export interface AddAppetiteRequest {
  lineOfBusiness: LineOfBusiness;
  states: string;
  minYearsInBusiness?: number;
  maxYearsInBusiness?: number;
  minAnnualRevenue?: number;
  maxAnnualRevenue?: number;
  acceptedIndustries?: string;
  excludedIndustries?: string;
}

// ============================================
// Client Types
// ============================================

/**
 * Client type: Individual person or Business entity.
 */
export type ClientType = 'Individual' | 'Business';

/**
 * Client status options.
 */
export type ClientStatus = 'Active' | 'Inactive';

/**
 * Address type options.
 */
export type AddressType = 'Mailing' | 'Physical' | 'Billing';

/**
 * Summary information for client list views.
 */
export interface ClientSummary {
  id: string;
  displayName: string;
  clientType: ClientType;
  email?: string;
  phone?: string;
  status: ClientStatus;
  createdAt: string;
}

/**
 * Full client details with contacts and addresses.
 */
export interface Client {
  id: string;
  clientType: ClientType;
  status: ClientStatus;
  // Individual fields
  firstName?: string;
  lastName?: string;
  middleName?: string;
  suffix?: string;
  dateOfBirth?: string;
  // Business fields
  businessName?: string;
  businessType?: string;
  dbaName?: string;
  industry?: string;
  yearEstablished?: number;
  numberOfEmployees?: number;
  annualRevenue?: number;
  website?: string;
  // Common fields
  email?: string;
  phone?: string;
  // Relations
  contacts: Contact[];
  addresses: Address[];
  createdAt: string;
  updatedAt?: string;
}

/**
 * Contact person for a business client.
 */
export interface Contact {
  id: string;
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  title?: string;
  isPrimary: boolean;
}

/**
 * Physical/mailing address.
 */
export interface Address {
  id: string;
  type: AddressType;
  line1: string;
  line2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  isPrimary: boolean;
}

/**
 * Request payload for creating an individual client.
 */
export interface CreateIndividualClientRequest {
  firstName: string;
  lastName: string;
  middleName?: string;
  suffix?: string;
  dateOfBirth?: string;
  email?: string;
  phone?: string;
}

/**
 * Request payload for creating a business client.
 */
export interface CreateBusinessClientRequest {
  businessName: string;
  businessType: string;
  dbaName?: string;
  industry?: string;
  yearEstablished?: number;
  numberOfEmployees?: number;
  annualRevenue?: number;
  website?: string;
  email?: string;
  phone?: string;
}

/**
 * Request payload for updating a client.
 */
export interface UpdateClientRequest {
  // Individual fields
  firstName?: string;
  lastName?: string;
  middleName?: string;
  suffix?: string;
  dateOfBirth?: string;
  // Business fields
  businessName?: string;
  businessType?: string;
  dbaName?: string;
  industry?: string;
  yearEstablished?: number;
  numberOfEmployees?: number;
  annualRevenue?: number;
  website?: string;
  // Common fields
  email?: string;
  phone?: string;
}

/**
 * Request payload for adding a contact to a client.
 */
export interface AddContactRequest {
  firstName: string;
  lastName: string;
  email?: string;
  phone?: string;
  title?: string;
  isPrimary?: boolean;
}

/**
 * Request payload for adding an address to a client.
 */
export interface AddAddressRequest {
  type: AddressType;
  line1: string;
  line2?: string;
  city: string;
  state: string;
  postalCode: string;
  country?: string;
  isPrimary?: boolean;
}

// ============================================
// Policy Types
// ============================================

/**
 * Line of business/insurance type.
 */
export type LineOfBusiness =
  | 'PersonalAuto'
  | 'Homeowners'
  | 'Renters'
  | 'PersonalUmbrella'
  | 'Life'
  | 'Health'
  | 'GeneralLiability'
  | 'CommercialProperty'
  | 'WorkersCompensation'
  | 'CommercialAuto'
  | 'ProfessionalLiability'
  | 'DirectorsAndOfficers'
  | 'CyberLiability'
  | 'BusinessOwnersPolicy'
  | 'CommercialUmbrella'
  | 'InlandMarine'
  | 'Surety';

/**
 * Policy lifecycle status.
 */
export type PolicyStatus =
  | 'Draft'
  | 'Bound'
  | 'Active'
  | 'PendingCancellation'
  | 'Cancelled'
  | 'Expired'
  | 'PendingRenewal'
  | 'Renewed'
  | 'NonRenewed';

/**
 * Billing type options.
 */
export type BillingType = 'DirectBill' | 'AgencyBill';

/**
 * Payment plan options.
 */
export type PaymentPlan = 'Annual' | 'SemiAnnual' | 'Quarterly' | 'Monthly';

/**
 * Cancellation type options.
 */
export type CancellationType =
  | 'InsuredRequest'
  | 'CarrierUnderwriting'
  | 'NonPayment'
  | 'FlatCancel'
  | 'Misrepresentation';

/**
 * Endorsement status options.
 */
export type EndorsementStatus = 'Pending' | 'Approved' | 'Issued' | 'Rejected' | 'Cancelled';

/**
 * Summary information for policy list views.
 */
export interface PolicySummary {
  id: string;
  policyNumber: string;
  clientId: string;
  clientName?: string;
  carrierId: string;
  carrierName?: string;
  lineOfBusiness: string;
  policyType: string;
  status: string;
  effectiveDate: string;
  expirationDate: string;
  totalPremium: number;
  currency: string;
  createdAt: string;
}

/**
 * Full policy details with coverages and endorsements.
 */
export interface Policy extends PolicySummary {
  billingType: string;
  paymentPlan: string;
  carrierPolicyNumber?: string;
  notes?: string;
  boundAt?: string;
  cancellationDate?: string;
  cancellationReason?: string;
  reinstatementDate?: string;
  reinstatementReason?: string;
  coverages: Coverage[];
  endorsements: Endorsement[];
  updatedAt?: string;
}

/**
 * Policy coverage details.
 */
export interface Coverage {
  id: string;
  code: string;
  name: string;
  description?: string;
  limitAmount?: number;
  perOccurrenceLimit?: number;
  aggregateLimit?: number;
  deductibleAmount?: number;
  premiumAmount: number;
  isOptional: boolean;
  isActive: boolean;
}

/**
 * Policy endorsement/amendment.
 */
export interface Endorsement {
  id: string;
  endorsementNumber: string;
  effectiveDate: string;
  type: string;
  description: string;
  premiumChange: number;
  status: string;
  processedAt?: string;
  notes?: string;
}

/**
 * Request payload for creating a new policy.
 */
export interface CreatePolicyRequest {
  clientId: string;
  carrierId: string;
  lineOfBusiness: LineOfBusiness;
  policyType: string;
  effectiveDate: string;
  expirationDate: string;
  billingType?: BillingType;
  paymentPlan?: PaymentPlan;
  quoteId?: string;
  notes?: string;
}

/**
 * Request payload for cancelling a policy.
 */
export interface CancelPolicyRequest {
  cancellationDate: string;
  reason: string;
  cancellationType: CancellationType;
}

/**
 * Request payload for reinstating a cancelled policy.
 */
export interface ReinstatePolicyRequest {
  reason: string;
}

/**
 * Request payload for adding coverage to a policy.
 */
export interface AddCoverageRequest {
  code: string;
  name: string;
  premiumAmount: number;
  description?: string;
  limitAmount?: number;
  perOccurrenceLimit?: number;
  aggregateLimit?: number;
  deductibleAmount?: number;
  isOptional?: boolean;
}

/**
 * Request payload for updating coverage.
 */
export interface UpdateCoverageRequest {
  name: string;
  premiumAmount: number;
  description?: string;
  limitAmount?: number;
  perOccurrenceLimit?: number;
  aggregateLimit?: number;
  deductibleAmount?: number;
}

/**
 * Request payload for adding an endorsement.
 */
export interface AddEndorsementRequest {
  effectiveDate: string;
  type: string;
  description: string;
  premiumChange: number;
  notes?: string;
}

/**
 * Request payload for rejecting an endorsement.
 */
export interface RejectEndorsementRequest {
  reason: string;
}

// ============================================
// Quote Types
// ============================================

/**
 * Quote lifecycle status.
 */
export type QuoteStatus =
  | 'Draft'
  | 'Submitted'
  | 'Quoted'
  | 'Accepted'
  | 'Expired'
  | 'Cancelled';

/**
 * Quote carrier response status.
 */
export type QuoteCarrierStatus =
  | 'Pending'
  | 'Quoted'
  | 'Declined'
  | 'Expired';

/**
 * Summary information for quote list views.
 */
export interface QuoteSummary {
  id: string;
  clientId: string;
  clientName?: string;
  lineOfBusiness: string;
  effectiveDate: string;
  expirationDate: string;
  status: string;
  expiresAt: string;
  carrierCount: number;
  responseCount: number;
  lowestPremium?: number;
  createdAt: string;
}

/**
 * Full quote details with carrier responses.
 */
export interface QuoteDetail {
  id: string;
  clientId: string;
  clientName?: string;
  lineOfBusiness: string;
  effectiveDate: string;
  expirationDate: string;
  status: string;
  expiresAt: string;
  acceptedCarrierId?: string;
  policyId?: string;
  notes?: string;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
  carriers: QuoteCarrier[];
}

/**
 * Carrier participation in a quote.
 */
export interface QuoteCarrier {
  id: string;
  carrierId: string;
  carrierName?: string;
  status: string;
  premiumAmount?: number;
  premiumCurrency?: string;
  declinationReason?: string;
  conditions?: string;
  proposedCoverages?: string;
  respondedAt?: string;
  expiresAt?: string;
}

/**
 * Request payload for creating a new quote.
 */
export interface CreateQuoteRequest {
  clientId: string;
  lineOfBusiness: LineOfBusiness;
  effectiveDate: string;
  expirationDate: string;
  notes?: string;
  expiresAt?: string;
}

/**
 * Request payload for adding a carrier to a quote.
 */
export interface AddCarrierToQuoteRequest {
  carrierId: string;
}

/**
 * Request payload for recording a carrier's response.
 */
export interface RecordQuoteResponseRequest {
  isQuoted: boolean;
  premiumAmount?: number;
  premiumCurrency?: string;
  conditions?: string;
  proposedCoverages?: string;
  carrierExpiresAt?: string;
  declinationReason?: string;
}

/**
 * Quote list filtering parameters.
 */
export interface QuoteFilters extends PaginationParams {
  search?: string;
  clientId?: string;
  status?: QuoteStatus;
  lineOfBusiness?: LineOfBusiness;
}

/**
 * Quote summary dashboard statistics.
 */
export interface QuoteSummaryStats {
  totalQuotes: number;
  draftCount: number;
  submittedCount: number;
  quotedCount: number;
  acceptedCount: number;
  expiredCount: number;
  cancelledCount: number;
  averageQuotedPremium?: number;
}

// ============================================
// Claims Types
// ============================================

/**
 * Claim lifecycle status.
 */
export type ClaimStatus =
  | 'FNOL'
  | 'Acknowledged'
  | 'Assigned'
  | 'UnderInvestigation'
  | 'Evaluation'
  | 'Approved'
  | 'Denied'
  | 'Settlement'
  | 'Closed';

/**
 * Loss type classification.
 */
export type LossType =
  | 'PropertyDamage'
  | 'Liability'
  | 'WorkersComp'
  | 'Auto'
  | 'Professional'
  | 'Cyber'
  | 'NaturalDisaster'
  | 'TheftFraud'
  | 'BodilyInjury'
  | 'Other';

/**
 * Payment lifecycle status.
 */
export type PaymentStatus = 'Authorized' | 'Issued' | 'Voided';

/**
 * Summary information for claim list views.
 */
export interface ClaimListItem {
  id: string;
  claimNumber: string;
  policyId: string;
  clientId: string;
  clientName?: string;
  policyNumber?: string;
  status: ClaimStatus;
  lossDate: string;
  reportedDate: string;
  lossType: string;
  lossDescription: string;
  lossAmount?: number;
  lossCurrency?: string;
  claimAmount?: number;
  claimAmountCurrency?: string;
  assignedAdjusterId?: string;
  createdAt: string;
}

/**
 * Full claim details with notes, reserves, and payments.
 */
export interface ClaimDetail {
  id: string;
  claimNumber: string;
  policyId: string;
  clientId: string;
  clientName?: string;
  policyNumber?: string;
  status: ClaimStatus;
  lossDate: string;
  reportedDate: string;
  lossType: string;
  lossDescription: string;
  lossAmount?: number;
  lossCurrency?: string;
  claimAmount?: number;
  claimAmountCurrency?: string;
  assignedAdjusterId?: string;
  denialReason?: string;
  closedAt?: string;
  closureReason?: string;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
  notes: ClaimNote[];
  reserves: ClaimReserve[];
  payments: ClaimPayment[];
}

/**
 * Claim note entry.
 */
export interface ClaimNote {
  id: string;
  content: string;
  authorBy: string;
  isInternal: boolean;
  createdAt: string;
}

/**
 * Claim reserve entry.
 */
export interface ClaimReserve {
  id: string;
  reserveType: string;
  amount: number;
  currency: string;
  setBy: string;
  setAt: string;
  notes?: string;
}

/**
 * Claim payment entry.
 */
export interface ClaimPayment {
  id: string;
  paymentType: string;
  amount: number;
  currency: string;
  payeeName: string;
  paymentDate: string;
  checkNumber?: string;
  status: PaymentStatus;
  authorizedBy: string;
  authorizedAt: string;
  issuedAt?: string;
  voidedAt?: string;
  voidReason?: string;
}

/**
 * Claim statistics for dashboard.
 */
export interface ClaimStatistics {
  totalClaims: number;
  fnolCount: number;
  acknowledgedCount: number;
  assignedCount: number;
  underInvestigationCount: number;
  evaluationCount: number;
  approvedCount: number;
  deniedCount: number;
  settlementCount: number;
  closedCount: number;
  totalReserves: number;
  totalPayments: number;
}

/**
 * Request payload for creating a new claim (FNOL).
 */
export interface CreateClaimRequest {
  policyId: string;
  clientId: string;
  lossDate: string;
  reportedDate: string;
  lossType: LossType;
  lossDescription: string;
  estimatedLossAmount?: number;
  estimatedLossCurrency?: string;
}

/**
 * Request payload for updating claim status.
 */
export interface UpdateClaimStatusRequest {
  newStatus: ClaimStatus;
  claimAmount?: number;
  claimAmountCurrency?: string;
  denialReason?: string;
  closureReason?: string;
  adjusterId?: string;
}

/**
 * Request payload for adding a note to a claim.
 */
export interface AddClaimNoteRequest {
  content: string;
  isInternal?: boolean;
}

/**
 * Request payload for setting a reserve.
 */
export interface SetReserveRequest {
  reserveType: string;
  amount: number;
  currency?: string;
  notes?: string;
}

/**
 * Request payload for authorizing a payment.
 */
export interface AuthorizePaymentRequest {
  paymentType: string;
  amount: number;
  currency: string;
  payeeName: string;
  paymentDate: string;
  checkNumber?: string;
}

/**
 * Request payload for voiding a payment.
 */
export interface VoidPaymentRequest {
  reason: string;
}

/**
 * Claim list filtering parameters.
 */
export interface ClaimFilters extends PaginationParams {
  search?: string;
  status?: ClaimStatus;
  policyId?: string;
  clientId?: string;
  lossType?: LossType;
  lossDateFrom?: string;
  lossDateTo?: string;
}

// ============================================
// User Management Types
// ============================================

/**
 * Summary information for user list views.
 */
export interface UserSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  isActive: boolean;
  lastLoginAt?: string;
  roles: string[];
}

/**
 * Full user details for management views.
 */
export interface UserDetail extends UserSummary {
  tenantId: string;
  title?: string;
  phoneNumber?: string;
  createdAt: string;
  updatedAt?: string;
}

/**
 * Role assigned to a user.
 */
export interface UserRole {
  roleId: string;
  roleName: string;
  assignedAt: string;
}

/**
 * Summary information for role list views.
 */
export interface RoleSummary {
  id: string;
  name: string;
  description?: string;
  isSystemRole: boolean;
  userCount: number;
}

/**
 * Full role details with permissions.
 */
export interface RoleDetail extends RoleSummary {
  permissions: Permission[];
  createdAt: string;
  updatedAt?: string;
}

/**
 * Permission assigned to a role.
 */
export interface Permission {
  id: string;
  name: string;
  description?: string;
  module: string;
}

/**
 * Request payload for creating a new user (via registration).
 */
export interface CreateUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  title?: string;
  phoneNumber?: string;
}

/**
 * Request payload for updating a user's profile.
 */
export interface UpdateUserProfileRequest {
  firstName: string;
  lastName: string;
  title?: string;
  phoneNumber?: string;
}

/**
 * Request payload for assigning a role to a user.
 */
export interface AssignRoleRequest {
  roleId: string;
}

/**
 * Request payload for creating a new role.
 */
export interface CreateRoleRequest {
  name: string;
  description?: string;
}

/**
 * Request payload for updating a role.
 */
export interface UpdateRoleRequest {
  name: string;
  description?: string;
}

/**
 * Request payload for granting a permission to a role.
 */
export interface GrantPermissionRequest {
  permissionId: string;
}

// ============================================
// Query Parameters
// ============================================

/**
 * Common pagination and sorting parameters.
 */
export interface PaginationParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

/**
 * Current policy information for renewal comparison.
 */
export interface CurrentPolicyInfo {
  policyNumber: string;
  carrierName?: string;
  totalPremium: number;
  currency: string;
  effectiveDate: string;
  expirationDate: string;
  coverages: string[];
}

/**
 * A single renewal carrier offer for comparison.
 */
export interface RenewalOfferDto {
  quoteId: string;
  quoteCarrierId: string;
  carrierId: string;
  carrierName?: string;
  premiumAmount?: number;
  conditions?: string;
  proposedCoverages?: string;
  status: string;
  premiumDifference?: number;
  premiumDifferencePercent?: number;
}

/**
 * Renewal comparison result.
 */
export interface RenewalComparisonDto {
  currentPolicy: CurrentPolicyInfo;
  renewalOffers: RenewalOfferDto[];
}

/**
 * Policy history event types.
 */
export type PolicyHistoryEventType =
  | 'Created'
  | 'Bound'
  | 'Activated'
  | 'Cancelled'
  | 'Reinstated'
  | 'Renewed'
  | 'Expired'
  | 'NonRenewed'
  | 'CoverageAdded'
  | 'CoverageModified'
  | 'CoverageRemoved'
  | 'EndorsementAdded'
  | 'EndorsementApproved'
  | 'EndorsementIssued'
  | 'EndorsementRejected'
  | 'PremiumChanged';

/**
 * A single entry in a policy's audit history.
 */
export interface PolicyHistoryItem {
  id: string;
  eventType: PolicyHistoryEventType;
  description: string;
  changesJson?: string;
  userId?: string;
  timestamp: string;
}

/**
 * Paginated result of policy history entries.
 */
export interface PolicyHistoryResult {
  items: PolicyHistoryItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

/**
 * Policy list filtering parameters.
 */
export interface PolicyFilters extends PaginationParams {
  search?: string;
  clientId?: string;
  carrierId?: string;
  status?: PolicyStatus;
  lineOfBusiness?: LineOfBusiness;
  effectiveDateFrom?: string;
  effectiveDateTo?: string;
  expirationDateFrom?: string;
  expirationDateTo?: string;
}

/**
 * Client list filtering parameters.
 */
export interface ClientFilters extends PaginationParams {
  search?: string;
  type?: ClientType;
  status?: ClientStatus;
}

/**
 * Carrier list filtering parameters.
 */
export interface CarrierFilters extends PaginationParams {
  search?: string;
  status?: CarrierStatus;
}

/**
 * User list filtering parameters.
 */
export interface UserFilters extends PaginationParams {
  search?: string;
  isActive?: boolean;
}

// ============================================
// Commission Types
// ============================================

/** Statement status values */
export type StatementStatus = 'Received' | 'Reconciling' | 'Reconciled' | 'Disputed' | 'Paid';

/** Transaction type values */
export type TransactionType = 'NewBusiness' | 'Renewal' | 'Endorsement' | 'Cancellation' | 'Chargeback';

/** Commission schedule list item */
export interface CommissionScheduleListItem {
  id: string;
  carrierId: string;
  carrierName: string;
  lineOfBusiness: string;
  newBusinessRate: number;
  renewalRate: number;
  effectiveFrom: string;
  effectiveTo: string | null;
  isActive: boolean;
  createdAt: string;
}

/** Commission schedule detail */
export interface CommissionScheduleDetail extends CommissionScheduleListItem {
  updatedAt: string;
}

/** Commission statement list item */
export interface CommissionStatementListItem {
  id: string;
  carrierId: string;
  carrierName: string;
  statementNumber: string;
  periodMonth: number;
  periodYear: number;
  statementDate: string;
  status: string;
  totalPremium: number;
  totalPremiumCurrency: string;
  totalCommission: number;
  totalCommissionCurrency: string;
  lineItemCount: number;
  reconciledCount: number;
  disputedCount: number;
  receivedAt: string;
  createdAt: string;
}

/** Commission line item */
export interface CommissionLineItem {
  id: string;
  policyId: string | null;
  policyNumber: string;
  insuredName: string;
  lineOfBusiness: string;
  effectiveDate: string;
  transactionType: string;
  grossPremium: number;
  grossPremiumCurrency: string;
  commissionRate: number;
  commissionAmount: number;
  commissionAmountCurrency: string;
  isReconciled: boolean;
  reconciledAt: string | null;
  disputeReason: string | null;
}

/** Producer split */
export interface ProducerSplit {
  id: string;
  lineItemId: string;
  producerName: string;
  producerId: string;
  splitPercentage: number;
  splitAmount: number;
  splitAmountCurrency: string;
}

/** Commission statement detail with line items and splits */
export interface CommissionStatementDetail {
  id: string;
  carrierId: string;
  carrierName: string;
  statementNumber: string;
  periodMonth: number;
  periodYear: number;
  statementDate: string;
  status: string;
  totalPremium: number;
  totalPremiumCurrency: string;
  totalCommission: number;
  totalCommissionCurrency: string;
  receivedAt: string;
  createdAt: string;
  updatedAt: string;
  lineItems: CommissionLineItem[];
  producerSplits: ProducerSplit[];
}

/** Commission statistics */
export interface CommissionStatistics {
  totalStatements: number;
  receivedStatements: number;
  reconcilingStatements: number;
  reconciledStatements: number;
  paidStatements: number;
  disputedStatements: number;
  totalCommissionAmount: number;
  totalPaidAmount: number;
  totalDisputedAmount: number;
  statementsByStatus: Record<string, number>;
  commissionByCarrier: Record<string, number>;
}

/** Commission summary report entry */
export interface CommissionSummaryEntry {
  carrierId: string;
  carrierName: string;
  periodMonth: number;
  periodYear: number;
  statementCount: number;
  totalPremium: number;
  totalCommission: number;
  totalPaid: number;
  currency: string;
}

/** Producer report entry */
export interface ProducerReportEntry {
  producerId: string;
  producerName: string;
  periodMonth: number;
  periodYear: number;
  lineItemCount: number;
  totalSplitAmount: number;
  averageSplitPercentage: number;
  currency: string;
}

/** Request to create a commission schedule */
export interface CreateScheduleRequest {
  carrierId: string;
  carrierName: string;
  lineOfBusiness: string;
  newBusinessRate: number;
  renewalRate: number;
  effectiveFrom: string;
  effectiveTo?: string;
}

/** Request to update a commission schedule */
export interface UpdateScheduleRequest {
  carrierName: string;
  lineOfBusiness: string;
  newBusinessRate: number;
  renewalRate: number;
  effectiveFrom: string;
  effectiveTo?: string;
}

/** Request to create a commission statement */
export interface CreateStatementRequest {
  carrierId: string;
  carrierName: string;
  statementNumber: string;
  periodMonth: number;
  periodYear: number;
  statementDate: string;
  totalPremium: number;
  totalPremiumCurrency: string;
  totalCommission: number;
  totalCommissionCurrency: string;
}

/** Request to add a line item */
export interface AddLineItemRequest {
  policyNumber: string;
  insuredName: string;
  lineOfBusiness: string;
  effectiveDate: string;
  transactionType: TransactionType;
  grossPremium: number;
  grossPremiumCurrency: string;
  commissionRate: number;
  commissionAmount: number;
  commissionAmountCurrency: string;
  policyId?: string;
}

/** Request to dispute a line item */
export interface DisputeLineItemRequest {
  reason: string;
}

/** Request to add a producer split */
export interface AddProducerSplitRequest {
  producerName: string;
  producerId: string;
  splitPercentage: number;
  splitAmount: number;
  splitAmountCurrency: string;
}

/** Request to update statement status */
export interface UpdateStatementStatusRequest {
  newStatus: StatementStatus;
}

/** Commission schedule filtering parameters */
export interface ScheduleFilters extends PaginationParams {
  search?: string;
  carrierId?: string;
  lineOfBusiness?: string;
  isActive?: boolean;
}

/** Commission statement filtering parameters */
export interface StatementFilters extends PaginationParams {
  search?: string;
  carrierId?: string;
  status?: StatementStatus;
  periodMonth?: number;
  periodYear?: number;
}

// ============================================
// Response Types
// ============================================

/**
 * Generic response for create operations returning an ID.
 */
export interface CreateResponse {
  id: string;
}

// ============================================
// Documents Types
// ============================================

/** Entity types that a document can be linked to. */
export type DocumentEntityType = 'Policy' | 'Client' | 'Claim' | 'Carrier' | 'Quote' | 'General';

/** Document category types. */
export type DocumentCategory = 'Policy' | 'Endorsement' | 'COI' | 'ClaimReport' | 'KYC' | 'Invoice' | 'Proposal' | 'Other';

/** Document template types. */
export type TemplateType = 'CertificateOfInsurance' | 'PolicySummary' | 'Proposal';

/** Summary of a document in a list. */
export interface DocumentListItem {
  id: string;
  entityType: DocumentEntityType;
  entityId?: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  category: DocumentCategory;
  version: number;
  isArchived: boolean;
  uploadedBy: string;
  uploadedAt: string;
}

/** Full document details. */
export interface DocumentDetail {
  id: string;
  tenantId: string;
  entityType: DocumentEntityType;
  entityId?: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  blobKey: string;
  category: DocumentCategory;
  version: number;
  isArchived: boolean;
  uploadedBy: string;
  description?: string;
  uploadedAt: string;
}

/** Summary of a document template in a list. */
export interface DocumentTemplateListItem {
  id: string;
  name: string;
  description: string;
  templateType: TemplateType;
  isActive: boolean;
  version: number;
  createdBy: string;
}

/** Full document template details. */
export interface DocumentTemplateDetail {
  id: string;
  tenantId: string;
  name: string;
  description: string;
  templateType: TemplateType;
  content: string;
  isActive: boolean;
  version: number;
  createdBy: string;
}

/** Filters for document list. */
export interface DocumentFilters extends PaginationParams {
  searchTerm?: string;
  category?: DocumentCategory;
  entityType?: DocumentEntityType;
  entityId?: string;
  includeArchived?: boolean;
}

/** Filters for document template list. */
export interface DocumentTemplateFilters extends PaginationParams {
  searchTerm?: string;
  templateType?: TemplateType;
  isActive?: boolean;
}

/** Request to create a document template. */
export interface CreateDocumentTemplateRequest {
  name: string;
  description?: string;
  templateType: TemplateType;
  content: string;
}

/** Request to update a document template. */
export interface UpdateDocumentTemplateRequest {
  name: string;
  description?: string;
  content: string;
}

/** Request to generate a COI. */
export interface GenerateCOIRequest {
  templateId: string;
  policyId: string;
}

/** Request to generate a proposal PDF from a quote. */
export interface GenerateProposalRequest {
  templateId: string;
  quoteId: string;
}

/** Result of an AI-powered PDF template import. */
export interface ImportTemplateResult {
  /** The generated HTML/Handlebars template content. */
  generatedContent: string;
  /** A suggested name for the template, derived from the file name. */
  suggestedName: string;
}

/** Request body for AI template editing. */
export interface AIEditTemplateRequest {
  /** Natural language instruction describing the desired modification. */
  instruction: string;
}

/** Result of an AI template edit, containing both versions for comparison. */
export interface AIEditTemplateResult {
  /** Template content before the AI edit. */
  originalContent: string;
  /** Template content after the AI edit. */
  modifiedContent: string;
}
