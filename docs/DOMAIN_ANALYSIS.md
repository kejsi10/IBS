# Insurance Broker System - Domain Analysis

## Executive Summary

This document provides a comprehensive domain analysis of the Insurance Broker System (IBS), breaking down the system into bounded contexts, aggregate roots, features, and their relationships following Domain-Driven Design (DDD) principles.

---

## Bounded Contexts Overview

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                              INSURANCE BROKER SYSTEM                                 │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                      │
│  ┌────────────────────┐    ┌────────────────────┐    ┌────────────────────┐        │
│  │  IDENTITY CONTEXT  │    │  TENANT CONTEXT    │    │  CLIENT CONTEXT    │        │
│  │    [IMPLEMENTED]   │    │   [IMPLEMENTED]    │    │   [IMPLEMENTED]    │        │
│  │   (+ Frontend ✅)  │    │  (no frontend UI)  │    │   (+ Frontend ✅)  │        │
│  │                    │    │                    │    │                    │        │
│  │  • User            │    │  • Tenant          │    │  • Client          │        │
│  │  • Role            │    │  • TenantCarrier   │    │  • Contact         │        │
│  │  • Permission      │    │  • Settings        │    │  • Address         │        │
│  │  • RefreshToken    │    │                    │    │  • Communication   │        │
│  └─────────┬──────────┘    └─────────┬──────────┘    └─────────┬──────────┘        │
│            │                         │                         │                    │
│            └─────────────────────────┼─────────────────────────┘                    │
│                                      │                                              │
│  ┌───────────────────────────────────┴────────────────────────────────────────┐    │
│  │                           POLICY CONTEXT                                    │    │
│  │                            [IMPLEMENTED]                                    │    │
│  │                                                                             │    │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐            │    │
│  │  │  Underwriting   │  │    Policy       │  │    Renewal      │            │    │
│  │  │   Subdomain     │  │   Subdomain     │  │   Subdomain     │            │    │
│  │  │  [IMPLEMENTED]  │  │  [IMPLEMENTED]  │  │  [IMPLEMENTED]  │            │    │
│  │  │  • Quote        │  │  • Policy       │  │  via Policy     │            │    │
│  │  │  • QuoteCarrier │  │  • Coverage     │  │  aggregate      │            │    │
│  │  │                 │  │  • Endorsement  │  │                 │            │    │
│  │  │                 │  │  • Premium      │  │                 │            │    │
│  │  └─────────────────┘  └─────────────────┘  └─────────────────┘            │    │
│  └────────────────────────────────────────────────────────────────────────────┘    │
│                                      │                                              │
│  ┌───────────────────────────────────┴────────────────────────────────────────┐    │
│  │                           CLAIMS CONTEXT                                    │    │
│  │                            [IMPLEMENTED]                                    │    │
│  │                                                                             │    │
│  │  • Claim (Aggregate Root)           • ClaimPayment                         │    │
│  │  • ClaimParty                       • ClaimNote                            │    │
│  │  • Reserve                          • Subrogation                          │    │
│  └────────────────────────────────────────────────────────────────────────────┘    │
│                                      │                                              │
│  ┌────────────────────┐    ┌─────────┴──────────┐    ┌────────────────────┐        │
│  │ COMMISSION CONTEXT │    │  DOCUMENT CONTEXT  │    │  CARRIER CONTEXT   │        │
│  │   [IMPLEMENTED]    │    │  [IMPLEMENTED]     │    │   [IMPLEMENTED]    │        │
│  │                    │    │                    │    │                    │        │
│  │  • CommSchedule    │    │  • Document        │    │  • Carrier         │        │
│  │  • Statement       │    │  • Template        │    │  • Product         │        │
│  │  • LineItem        │    │  • Certificate     │    │  • Appetite        │        │
│  │  • ProducerSplit   │    │                    │    │                    │        │
│  └────────────────────┘    └────────────────────┘    └────────────────────┘        │
│                                                                                      │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

---

## Bounded Contexts Detail

### 1. Identity Context ✅ IMPLEMENTED

**Purpose:** Manages user authentication, authorization, roles, and permissions.

**Aggregate Roots:**
| Aggregate | Entities | Value Objects |
|-----------|----------|---------------|
| **User** | UserRole, RefreshToken | Email, PasswordHash, UserStatus |
| **Role** | RolePermission | - |
| **Permission** | - | - |

**Features:**
- F1.1: User registration with email confirmation
- F1.2: Login with JWT + refresh token
- F1.3: Password reset workflow
- F1.4: Account lockout after failed attempts
- F1.5: Role assignment and management
- F1.6: Permission-based authorization
- F1.7: Multi-tenant user isolation

**Domain Events:**
- `UserRegisteredEvent`
- `UserActivatedEvent`
- `UserDeactivatedEvent`
- `RoleAssignedEvent`
- `PermissionGrantedEvent`

---

### 2. Tenant Context ✅ IMPLEMENTED

**Purpose:** Manages multi-tenancy, subscriptions, and tenant-carrier relationships.

**Aggregate Roots:**
| Aggregate | Entities | Value Objects |
|-----------|----------|---------------|
| **Tenant** | TenantCarrier | Subdomain, TenantStatus, SubscriptionTier |

**Features:**
- F2.1: Tenant registration with unique subdomain
- F2.2: Subscription tier management (Basic, Professional, Enterprise)
- F2.3: Tenant suspension and cancellation
- F2.4: Carrier association per tenant
- F2.5: Agency code and commission rate per carrier
- F2.6: Tenant settings (JSON configuration)

**Domain Events:**
- `TenantRegisteredEvent`
- `TenantSuspendedEvent`
- `TenantActivatedEvent`
- `TenantCancelledEvent`

**Invariants:**
- Subdomain must be unique across all tenants
- Cannot suspend a cancelled tenant
- Cannot activate a cancelled tenant

---

### 3. Client Context ✅ IMPLEMENTED

**Purpose:** Manages client relationships, contacts, addresses, and communications (CRM).

**Aggregate Roots:**
| Aggregate | Entities | Value Objects |
|-----------|----------|---------------|
| **Client** | Contact, Address, Communication | ClientType, ClientStatus, PersonName, BusinessInfo, EmailAddress, PhoneNumber, AddressType, CommunicationType |

**Features:**
- F3.1: Individual client management (person)
- F3.2: Business client management (company)
- F3.3: Multiple contacts per client (with primary designation)
- F3.4: Multiple addresses per client (mailing, physical, billing)
- F3.5: Communication logging (calls, emails, notes)
- F3.6: Client search and filtering
- F3.7: Client deactivation (soft delete)

**Domain Events:**
- `ClientRegisteredEvent`
- `ClientUpdatedEvent`
- `ClientDeactivatedEvent`
- `ContactAddedEvent`
- `ContactRemovedEvent`
- `AddressAddedEvent`
- `CommunicationLoggedEvent`

**Invariants:**
- Individual clients must have PersonName
- Business clients must have BusinessInfo
- Only one primary contact allowed
- Only one primary address per type

---

### 4. Carrier Context ✅ IMPLEMENTED

**Purpose:** Manages insurance carriers, products, appetite rules, and rating engine integrations.

**Aggregate Roots:**
| Aggregate | Entities | Value Objects |
|-----------|----------|---------------|
| **Carrier** | Product, Appetite | CarrierCode, CarrierStatus, AmBestRating, LineOfBusiness |

**Features:**
- F4.1: Carrier profile management ✅
- F4.2: Product catalog per carrier ✅
- F4.3: Appetite rules (what risks they accept) ✅
- F4.4: Rating engine configuration 🔲
- F4.5: API credential management 🔲
- F4.6: Commission schedule templates 🔲
- F4.7: Carrier performance tracking 🔲

**Domain Events:**
- `CarrierCreatedEvent` ✅
- `CarrierDeactivatedEvent` ✅
- `ProductAddedEvent` ✅
- `AppetiteAddedEvent` ✅

**Invariants:**
- Carrier code must be unique
- Cannot delete carrier with active policies
- API credentials must be encrypted

---

### 5. Policy Context ✅ IMPLEMENTED

**Purpose:** Full lifecycle management of insurance policies from quote to expiration.

**Subdomains:**

#### 5a. Underwriting Subdomain ✅ IMPLEMENTED
**Aggregate Roots:**
| Aggregate | Entities | Value Objects |
|-----------|----------|---------------|
| **Quote** | QuoteCarrier | QuoteStatus, QuoteCarrierStatus |

**Features:**
- F5.1: Quote request creation ✅
- F5.2: Multi-carrier quote submission ✅
- F5.3: Quote comparison ✅
- F5.4: Accept quote → bind policy ✅
- F5.5: Quote cancellation / expiration handling ✅
- F5.6: Proposal generation (PDF) 🔲

#### 5b. Policy Subdomain ✅ IMPLEMENTED
**Aggregate Roots:**
| Aggregate | Entities | Value Objects |
|-----------|----------|---------------|
| **Policy** | Coverage, Endorsement | PolicyNumber, PolicyStatus, EffectivePeriod, Money, LineOfBusiness, BillingType, PaymentPlan |

**Features:**
- F5.7: Policy binding from quote ✅
- F5.8: Policy issuance (activation) ✅
- F5.9: Coverage management (add, update, remove) ✅
- F5.10: Endorsement processing (add, approve, issue, reject) ✅
- F5.11: Policy cancellation ✅
- F5.12: Policy reinstatement 🔲
- F5.13: Certificate of Insurance generation 🔲
- F5.14: Policy version history 🔲

#### 5c. Renewal Subdomain ✅ IMPLEMENTED
**Aggregate Roots:**
| Aggregate | Entities | Value Objects |
|-----------|----------|---------------|
| **Policy** (renewal via CreateRenewal) | - | - |

**Features:**
- F5.15: Renewal initiation (query policies due for renewal) ✅
- F5.16: Renewal policy creation (copies coverages) ✅
- F5.17: Renewal offer comparison 🔲
- F5.18: Renewal binding ✅
- F5.19: Non-renewal tracking ✅

**Domain Events (Policy Context) - Implemented:**
- `PolicyCreatedEvent` ✅
- `PolicyBoundEvent` ✅
- `PolicyActivatedEvent` ✅
- `PolicyCancelledEvent` ✅
- `PolicyExpiredEvent` ✅
- `PolicyRenewedEvent` ✅
- `CoverageAddedEvent` ✅
- `CoverageModifiedEvent` ✅
- `CoverageRemovedEvent` ✅
- `EndorsementAddedEvent` ✅
- `EndorsementApprovedEvent` ✅
- `PolicyPremiumChangedEvent` ✅

**Commands Implemented:**
- CreatePolicy, BindPolicy, ActivatePolicy, CancelPolicy, RenewPolicy
- AddCoverage, UpdateCoverage, RemoveCoverage
- AddEndorsement, ApproveEndorsement, IssueEndorsement, RejectEndorsement

**Queries Implemented:**
- GetPolicyById, GetPolicies (filtered/paginated)
- GetPoliciesByClient, GetExpiringPolicies, GetPoliciesDueForRenewal

**Invariants:**
- Policy number must be unique within tenant
- Effective date must be before expiration date
- Cannot cancel an already cancelled policy
- Cannot add endorsement to expired policy
- Total premium must equal sum of coverage premiums

---

### 6. Claims Context ✅ IMPLEMENTED

**Purpose:** Track and manage insurance claims from First Notice of Loss (FNOL) to settlement.

**Aggregate Roots:**
| Aggregate | Entities | Value Objects |
|-----------|----------|---------------|
| **Claim** | ClaimNote, Reserve, ClaimPayment | ClaimNumber, ClaimStatus, Money, LossType, PaymentStatus |

**Features:**
- F6.1: First Notice of Loss (FNOL) intake ✅
- F6.2: Claim workflow (status transitions) ✅
- F6.3: Claim notes ✅
- F6.4: Reserve management ✅
- F6.5: Payment authorization and issuance ✅
- F6.6: Payment void ✅
- F6.7: Claim statistics query ✅
- F6.8: Subrogation tracking 🔲
- F6.9: Claim party management (claimant, witnesses) 🔲
- F6.10: Litigation tracking 🔲

**Claim Workflow States:**
```
FNOL → Acknowledged → Assigned → Under Investigation → Evaluation →
     → Approved → Settlement → Closed
     → Denied → Closed
     → Reopened (from Closed)
```

**Domain Events:**
- `ClaimReportedEvent` (FNOL)
- `ClaimAcknowledgedEvent`
- `ClaimAssignedEvent`
- `ClaimInvestigationStartedEvent`
- `ReserveSetEvent`
- `ReserveAdjustedEvent`
- `ClaimEvaluatedEvent`
- `ClaimApprovedEvent`
- `ClaimDeniedEvent`
- `PaymentAuthorizedEvent`
- `PaymentIssuedEvent`
- `SubrogationInitiatedEvent`
- `ClaimReopenedEvent`
- `ClaimClosedEvent`

**Invariants:**
- Claim number must be unique within tenant
- Loss date cannot be in the future
- Loss date must be within policy effective period
- Total paid cannot exceed reserve without approval
- Cannot close claim with pending payments

---

### 7. Commission Context ✅ IMPLEMENTED

**Purpose:** Track commissions, reconcile carrier statements, and manage producer splits.

**Aggregate Roots:**
| Aggregate | Entities | Value Objects |
|-----------|----------|---------------|
| **CommissionSchedule** | - | Rate, EffectivePeriod |
| **CommissionStatement** | CommissionLineItem, ProducerSplit | Money, Period, StatementStatus, TransactionType |

**Features:**
- F7.1: Commission schedule management (per carrier/product) ✅
- F7.2: Statement creation and management ✅
- F7.3: Line item reconciliation ✅
- F7.4: Dispute tracking ✅
- F7.5: Producer split assignment ✅
- F7.6: Statement status workflow ✅
- F7.7: Commission statistics ✅
- F7.8: Commission summary report ✅
- F7.9: Producer report ✅
- F7.10: Chargeback tracking 🔲

**Domain Events:**
- `CommissionScheduleCreatedEvent` ✅
- `CommissionScheduleDeactivatedEvent` ✅
- `CommissionStatementCreatedEvent` ✅
- `LineItemAddedEvent` ✅
- `LineItemReconciledEvent` ✅
- `LineItemDisputedEvent` ✅
- `ProducerSplitAddedEvent` ✅

**Invariants:**
- Statement period must not overlap for same carrier
- Line item commission = premium × rate
- Producer splits must total 100%
- New business rate applies for first term only

---

### 8. Document Context ✅ IMPLEMENTED

**Purpose:** Centralized document storage, templates, and certificate generation.

**Aggregate Roots:**
| Aggregate | Entities | Value Objects |
|-----------|----------|---------------|
| **Document** | - | DocumentCategory, DocumentEntityType |
| **DocumentTemplate** | - | TemplateType |

**Features:**
- F8.1: Document upload via Azure Blob Storage (Azurite) ✅
- F8.2: Document categorization and entity linking (policy, client, claim, carrier) ✅
- F8.3: Document versioning and archiving ✅
- F8.4: Handlebars template management (create, update, activate, deactivate) ✅
- F8.5: Certificate of Insurance (COI) generation via QuestPDF ✅
- F8.6: Proposal document generation 🔲
- F8.7: E-signature integration 🔲
- F8.8: Retention policy enforcement 🔲
- F8.9: OCR text extraction 🔲

**Domain Events:**
- `DocumentUploadedEvent` ✅
- `DocumentArchivedEvent` ✅
- `DocumentTemplateCreatedEvent` ✅
- `DocumentTemplateUpdatedEvent` ✅

**Invariants:**
- Document must belong to an entity (client, policy, claim, carrier, or general)
- File size limits per subscription tier
- Supported file types only

---

## Context Mapping

```
┌──────────────┐                                      ┌──────────────┐
│   Identity   │ ──── Shared Kernel ────────────────▶ │    Tenant    │
│   Context    │                                      │   Context    │
└──────────────┘                                      └──────────────┘
       │                                                     │
       │ Customer/Supplier                                   │
       ▼                                                     ▼
┌──────────────┐         Upstream/Downstream         ┌──────────────┐
│    Client    │ ◀────────────────────────────────── │    Policy    │
│   Context    │                                     │   Context    │
└──────────────┘                                     └──────────────┘
       │                                                     │
       │                                                     │
       │              ┌──────────────┐                       │
       └─────────────▶│    Claims    │◀──────────────────────┘
                      │   Context    │
                      └──────────────┘
                             │
                             ▼
                      ┌──────────────┐
                      │  Commission  │
                      │   Context    │
                      └──────────────┘

┌──────────────┐                                     ┌──────────────┐
│   Document   │ ◀──── Conformist ──────────────────▶│   Carrier    │
│   Context    │       (all contexts use docs)       │   Context    │
└──────────────┘                                     └──────────────┘
                                                            │
                                                            ▼
                                                     ┌──────────────┐
                                                     │   External   │
                                                     │  Carrier API │
                                                     │   (ACL)      │
                                                     └──────────────┘
```

**Relationship Types:**
- **Shared Kernel:** Identity ↔ Tenant (shared user/tenant concepts)
- **Customer/Supplier:** Client → Policy, Policy → Claims
- **Conformist:** All contexts → Document (standard document handling)
- **Anti-Corruption Layer (ACL):** Carrier → External Carrier APIs

---

## Integration Events (Cross-Context)

| Event | Publisher | Subscribers | Purpose |
|-------|-----------|-------------|---------|
| `PolicyBoundEvent` | Policy | Commission, Document | Trigger commission setup, generate docs |
| `PolicyCancelledEvent` | Policy | Claims, Commission | Close related claims, adjust commissions |
| `ClaimReportedEvent` | Claims | Policy | Update policy loss history |
| `ClaimClosedEvent` | Claims | Commission | Finalize commission impact |
| `ClientUpdatedEvent` | Client | Policy, Claims | Sync client info |

---

## Feature Priority Matrix

### Phase 1: Core Foundation (MVP)
| # | Feature | Context | Priority | Complexity |
|---|---------|---------|----------|------------|
| 1 | User authentication & authorization | Identity | P0 | Medium |
| 2 | Tenant management | Tenant | P0 | Medium |
| 3 | Client CRUD | Client | P0 | Medium |
| 4 | Carrier setup | Carrier | P0 | Low |
| 5 | Basic policy creation | Policy | P0 | High |

### Phase 2: Quote to Bind
| # | Feature | Context | Priority | Complexity |
|---|---------|---------|----------|------------|
| 6 | Quote request | Policy | P1 | Medium |
| 7 | Multi-carrier submission | Policy | P1 | High |
| 8 | Quote comparison | Policy | P1 | Medium |
| 9 | Policy binding | Policy | P1 | Medium |
| 10 | Proposal generation | Document | P1 | Medium |

### Phase 3: Policy Lifecycle
| # | Feature | Context | Priority | Complexity |
|---|---------|---------|----------|------------|
| 11 | Endorsement processing | Policy | P2 | High |
| 12 | Renewal workflow | Policy | P2 | High |
| 13 | Cancellation handling | Policy | P2 | Medium |
| 14 | COI generation | Document | P2 | Medium |

### Phase 4: Claims
| # | Feature | Context | Priority | Complexity |
|---|---------|---------|----------|------------|
| 15 | FNOL intake | Claims | P2 | Medium |
| 16 | Claim workflow | Claims | P2 | High |
| 17 | Reserve management | Claims | P2 | Medium |
| 18 | Payment processing | Claims | P2 | High |

### Phase 5: Commissions & Reporting
| # | Feature | Context | Priority | Complexity |
|---|---------|---------|----------|------------|
| 19 | Commission tracking | Commission | P3 | High |
| 20 | Statement reconciliation | Commission | P3 | High |
| 21 | Production reports | Reporting | P3 | Medium |
| 22 | Loss ratio analysis | Reporting | P3 | Medium |

---

## Domain Services

| Context | Domain Service | Responsibility |
|---------|----------------|----------------|
| Policy | `PremiumCalculationService` | Calculate total premium from coverages |
| Policy | `RenewalService` | Manage renewal workflow and reminders |
| Policy | `QuoteComparisonService` | Compare quotes across carriers |
| Claims | `ReserveCalculationService` | Calculate and validate reserves |
| Claims | `ClaimWorkflowService` | Manage claim state transitions |
| Commission | `CommissionReconciliationService` | Match statements to policies |
| Document | `CertificateGenerationService` | Generate COIs from policies |

---

## Implementation Status

> **Last audited: 2026-02-20**

| Context | Domain | Application | Infrastructure | API | Frontend | Tests | Status |
|---------|--------|-------------|----------------|-----|----------|-------|--------|
| Identity | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 95% |
| Tenant | ✅ | ✅ | ✅ | ✅ | 🔲 | ✅ | 90% |
| Client | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 95% |
| Carrier | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 90% |
| Policy | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 90% |
| Quote | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 90% |
| Claims | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 85% |
| Commission | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 90% |
| Document | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | 85% |

**Legend:** ✅ Complete | ⏳ Partial | 🔲 Not Started

---

## Next Steps

1. ~~**Implement Carrier Context**~~ ✅ Complete
2. ~~**Implement Policy Context**~~ ✅ Complete (Policy + Quotes subdomains)
3. ~~**Implement Claims Context**~~ ✅ Complete
4. ~~**Implement Commission Context**~~ ✅ Complete
5. ~~**Implement Frontend UI Modules**~~ ✅ Complete (Clients, Carriers, Policies, Quotes, Claims, Commissions, Dashboard, Settings, Reports, Documents)
6. ~~**Write Frontend Tests**~~ ✅ Complete (hook tests and page tests for all feature modules)
7. ~~**Implement Document Context**~~ ✅ Complete (document storage, templates, COI generation via QuestPDF)
