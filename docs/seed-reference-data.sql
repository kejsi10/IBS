-- PolicyAssistant Reference Documents Seed Data
-- Run this after the main seed-data.sql script
-- These documents are used by the AI Policy Assistant for RAG (retrieval-augmented generation)

BEGIN TRANSACTION;

-- Clear existing seeded documents (idempotent)
DELETE FROM PolicyAssistantDocumentChunks WHERE ReferenceDocumentId IN (
    SELECT Id FROM PolicyAssistantReferenceDocuments WHERE Source = 'System Seed Data'
);
DELETE FROM PolicyAssistantReferenceDocuments WHERE Source = 'System Seed Data';

-- =====================================================
-- REGULATIONS (Category = 0)
-- =====================================================

-- 1. CA Personal Auto Minimum Requirements
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A1000001-0000-0000-0000-000000000001',
    NULL,
    'CA Personal Auto Minimum Coverage Requirements',
    0,  -- Regulation
    0,  -- PersonalAuto
    'CA',
    N'CALIFORNIA PERSONAL AUTOMOBILE INSURANCE MINIMUM REQUIREMENTS

California Vehicle Code Section 16056 mandates that all motor vehicles registered and operated in the state carry minimum liability insurance. The California Department of Insurance enforces these minimums and requires proof of financial responsibility at all times.

MINIMUM LIABILITY LIMITS (15/30/5):
California requires minimum bodily injury liability coverage of $15,000 per person injured in any one accident and $30,000 for all persons injured in any one accident. Property damage liability minimum is $5,000 per accident. These are referred to as the 15/30/5 limits. As of January 1, 2025, limits increase to $30,000/$60,000/$15,000 per Senate Bill 1107.

UNINSURED MOTORIST COVERAGE:
Insurers are required to offer Uninsured Motorist Bodily Injury (UMBI) and Uninsured Motorist Property Damage (UMPD) coverage. Uninsured motorist limits must be offered at the same limits as the liability coverage. The insured may reject UM/UIM in writing. UMPD carries a $250 deductible unless waived.

UNDERINSURED MOTORIST COVERAGE:
Underinsured Motorist coverage (UIM) must be offered at matching limits. UIM pays when the at-fault driver''s liability limits are insufficient to cover the insured''s damages. UIM stacks on top of the at-fault driver''s coverage.

MEDICAL PAYMENTS COVERAGE (MedPay):
Medical Payments coverage is optional in California. When offered, standard limits range from $1,000 to $100,000. MedPay applies regardless of fault and covers reasonable medical expenses incurred within 3 years of the accident.

POLICY TERM REQUIREMENTS:
The minimum policy effective period is 6 months (180 days). Annual policies (365 days) are also standard. Policies must be issued to California-licensed drivers operating vehicles garaged in California. Out-of-state garaging requires disclosure and may affect rating.

RATING FACTORS:
California Proposition 103 requires that premiums be primarily based on the insured''s driving safety record, annual miles driven, and years of driving experience. Vehicle type, garaging zip code, and other secondary factors may be used but cannot override the primary factors. Gender may not be used as a rating factor in California.

CANCELLATION AND NON-RENEWAL:
Policies may only be cancelled mid-term for non-payment of premium, fraud or material misrepresentation, or the insured''s driver''s license being suspended or revoked. Non-renewal requires 75 days'' written notice to the insured. New policies in the first 60 days may be cancelled with 20 days'' notice for any reason.

GOOD DRIVER DISCOUNT:
California law requires a minimum 20% Good Driver Discount for insureds who have held a license for 3 or more years and have no more than one violation point in the preceding 3 years and no at-fault accidents.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 2. TX Personal Auto Minimum Requirements
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A1000002-0000-0000-0000-000000000001',
    NULL,
    'TX Personal Auto Minimum Coverage Requirements',
    0,  -- Regulation
    0,  -- PersonalAuto
    'TX',
    N'TEXAS PERSONAL AUTOMOBILE INSURANCE MINIMUM REQUIREMENTS

The Texas Transportation Code Chapter 601 (Motor Vehicle Safety Responsibility Act) establishes financial responsibility requirements for all motor vehicle operators in Texas. The Texas Department of Insurance (TDI) regulates automobile insurance and sets minimum standards.

MINIMUM LIABILITY LIMITS (30/60/25):
Texas requires minimum bodily injury liability of $30,000 per person / $60,000 per accident and property damage liability of $25,000 per accident. These are commonly abbreviated as 30/60/25. All passenger vehicles must maintain this coverage to demonstrate financial responsibility.

PERSONAL INJURY PROTECTION (PIP):
Texas requires insurers to offer Personal Injury Protection (PIP) coverage. The default minimum offering is $2,500 per person, but higher limits are available. PIP must be accepted or rejected in writing by the named insured. PIP covers medical expenses, lost wages (80% of gross income), and essential services, regardless of fault. If the insured does not reject PIP in writing, it must be included in the policy.

UNINSURED/UNDERINSURED MOTORIST COVERAGE (UM/UIM):
Texas law requires insurers to offer Uninsured Motorist Bodily Injury (UMBI) and Underinsured Motorist Bodily Injury (UIMBI) coverage. These coverages must be offered at limits matching the liability coverage selected. The insured must reject these coverages in writing if they choose not to purchase them. Uninsured Motorist Property Damage (UMPD) is also available with a $250 deductible.

POLICY TERMS:
Texas personal auto policies are typically written for 6-month or 12-month terms. The Texas Department of Insurance does not mandate a minimum policy period for private passenger auto, but standard market practice is 6 months. Non-standard or high-risk policies may be written for shorter terms.

TEXAS PERSONAL AUTOMOBILE POLICY (PAP) FORM:
The standard Texas Personal Automobile Policy uses ISO forms adapted for Texas requirements. Texas requires specific endorsements for UM/UIM rejection, PIP rejection, and named driver exclusions. Policies must clearly disclose all exclusions in bold or otherwise conspicuous text.

PROOF OF FINANCIAL RESPONSIBILITY:
All Texas drivers must carry proof of liability insurance (or other approved financial responsibility) in their vehicles at all times. Electronic proof of insurance is accepted via smartphone. TexasSure is the state''s insurance verification system used by law enforcement.

CANCELLATION REQUIREMENTS:
Mid-term cancellation for reasons other than non-payment or fraud requires 10 days'' advance written notice. Non-renewal requires 30 days'' advance written notice. New business in the first 60 days may be cancelled with 10 days'' notice for any underwriting reason.

SR-22 FILING:
Texas requires SR-22 (Certificate of Financial Responsibility) filings for high-risk drivers including those convicted of DWI, driving without insurance, or accumulating excessive points. The filing period is typically 2 years.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 3. NY Personal Auto Minimum Requirements
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A1000003-0000-0000-0000-000000000001',
    NULL,
    'NY Personal Auto Minimum Coverage Requirements',
    0,  -- Regulation
    0,  -- PersonalAuto
    'NY',
    N'NEW YORK PERSONAL AUTOMOBILE INSURANCE MINIMUM REQUIREMENTS

New York Vehicle and Traffic Law Article 6 and New York Insurance Law Article 51 (Comprehensive Motor Vehicle Insurance Reparations Act, commonly called the No-Fault Law) govern automobile insurance requirements in New York State. The New York Department of Financial Services (NYDFS) regulates all automobile insurance policies.

MINIMUM LIABILITY LIMITS:
New York mandates minimum bodily injury liability coverage of $25,000 per person / $50,000 per accident for injury, and $50,000 per person / $100,000 per accident in case of death. Property damage liability minimum is $10,000 per accident. These limits are often expressed as 25/50/10 for injury or 50/100/10 for death.

NO-FAULT / PERSONAL INJURY PROTECTION (PIP):
New York is a no-fault state. Personal Injury Protection (PIP) is mandatory at a minimum of $50,000 per person per accident. PIP (called Basic Economic Loss in New York) covers: all necessary medical treatment, 80% of lost earnings up to $2,000/month for up to 3 years, and $25/day for other reasonable and necessary expenses for up to 1 year. Serious injury threshold must be met to sue for non-economic damages (pain and suffering).

SUPPLEMENTAL SPOUSAL LIABILITY:
New York requires that policies automatically include Supplemental Spousal Liability (SSL) unless affirmatively rejected by the named insured in writing. SSL extends bodily injury liability coverage to a spouse who is injured while a passenger in the insured vehicle.

UNINSURED MOTORIST COVERAGE (UM):
Uninsured Motorist Bodily Injury (UMBI) coverage is mandatory in New York at the same limits as the selected bodily injury liability limits. The minimum required UMBI is $25,000/$50,000. Underinsured Motorist coverage (UIM) is not separately required but may be purchased above the required UM limits via a SUM (Supplemental Uninsured/Underinsured Motorist) endorsement.

CANCELLATION NOTICE REQUIREMENTS:
New York has strict cancellation requirements. Mid-term cancellation notices must be provided at least 20 days in advance (15 days for non-payment of premium). Non-renewal notices must be provided at least 60 days prior to policy expiration. Cancellation notices must state the specific reason for cancellation. Policies cannot be cancelled solely because the insured was involved in an accident unless fraud is established.

ASSIGNED RISK PLAN:
The New York Automobile Insurance Plan (NYAIP) is the assigned risk mechanism for drivers who cannot obtain coverage in the voluntary market. All licensed insurers in New York must participate. Premiums in the assigned risk plan are typically significantly higher than voluntary market premiums.

COMPULSORY INSURANCE ENFORCEMENT:
New York has one of the nation''s strictest compulsory insurance enforcement systems. Uninsured vehicle registration results in a $750 civil penalty plus license/registration suspension. Driving without insurance carries criminal penalties. New York uses an electronic insurance verification system to identify uninsured vehicles.

RATING RESTRICTIONS:
New York restricts the use of certain rating factors. Credit score cannot be used as a sole basis for non-renewal. Territorial rating is permitted but must be filed and approved by NYDFS. Premium changes require 45 days'' notice to policyholders and NYDFS filing.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 4. FL Homeowners Minimum Requirements
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A1000004-0000-0000-0000-000000000001',
    NULL,
    'FL Homeowners Insurance Requirements and Regulations',
    0,  -- Regulation
    2,  -- Homeowners
    'FL',
    N'FLORIDA HOMEOWNERS INSURANCE REQUIREMENTS AND REGULATIONS

Florida Statutes Chapter 627 governs property and casualty insurance in Florida, including homeowners insurance. The Florida Office of Insurance Regulation (OIR) oversees the market. Florida''s unique geography creates specific requirements not found in other states, particularly regarding windstorm and hurricane coverage.

DWELLING COVERAGE REQUIREMENTS:
Florida homeowners policies must cover the dwelling at a minimum of 100% of the replacement cost value (RCV) for the structure. Agreed Value or Replacement Cost Value settlement is standard; Actual Cash Value (ACV) settlement for the dwelling is restricted. Insurers must offer RCV coverage for personal property if RCV is provided on the dwelling.

WINDSTORM COVERAGE AND WIND-ONLY POLICIES:
In coastal counties and areas east of I-95 or I-75 (the wind-exposed areas), windstorm coverage may be excluded from the homeowners policy and must be obtained separately through the Florida Market Assistance Plan (FMAP) or Citizens Property Insurance Corporation. The Florida Hurricane Catastrophe Fund (FHCF) reinsures insurers for catastrophic hurricane losses. Policies in coastal areas must clearly disclose whether wind is included or excluded.

HURRICANE DEDUCTIBLE:
Florida law requires a separate hurricane deductible rather than a flat dollar deductible for hurricane losses. The hurricane deductible is expressed as a percentage of the Coverage A (Dwelling) limit, typically 2% or 5% but may be as high as 10% for high-value homes or coastal locations. The hurricane deductible applies per hurricane season, not per storm (once triggered, it applies to all subsequent hurricane losses that season). Calendar year hurricane deductible is an alternative available in some policies.

CITIZENS PROPERTY INSURANCE CORPORATION:
Citizens is the state-created insurer of last resort for property owners who cannot obtain coverage in the private market at reasonable rates. Citizens policies are subject to rate caps (limited annual increases), but rates must trend toward actuarially sound levels over time. Citizens depopulation efforts regularly move policies to private market carriers (takeout program). Citizens policyholders may receive an offer from a private carrier and have limited time to accept or remain with Citizens.

FLOOD INSURANCE:
Homeowners policies in Florida do not cover flood. Flood insurance is provided by the National Flood Insurance Program (NFIP) or private flood insurers. For properties in FEMA-designated Special Flood Hazard Areas (SFHA, flood zones A or V), flood insurance is mandatory for federally backed mortgages. Florida has a robust private flood insurance market due to state legislation encouraging non-NFIP alternatives.

SINKHOLE COVERAGE:
Florida law requires insurers to offer sinkhole coverage. Catastrophic ground cover collapse is covered under standard homeowners policies. Non-catastrophic sinkhole damage requires a separate sinkhole endorsement. Sinkhole claims are subject to strict reporting timelines and engineering investigation requirements.

ROOF COVERAGE RULES:
Recent Florida legislation (SB 2-D, 2022) restricts roof claims. Policies may include roof age schedules that reduce claims payments for older roofs. Roofs over 15 years old may be subject to inspection requirements. Cosmetic roof damage exclusions are permitted. AOB (Assignment of Benefits) for roof claims is significantly restricted.

POLICY CANCELLATION:
Florida restricts mid-term cancellations. Policies in force more than 90 days may only be cancelled for non-payment, fraud, or material change in risk. Non-renewal requires 120 days'' advance notice. Insurers must provide 45 days'' notice for mid-term non-renewal triggered by underwriting reasons. The OIR must approve any withdrawal of a carrier from the Florida market.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 5. CA General Liability Requirements
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A1000005-0000-0000-0000-000000000001',
    NULL,
    'CA Commercial General Liability Requirements',
    0,  -- Regulation
    4,  -- GeneralLiability
    'CA',
    N'CALIFORNIA COMMERCIAL GENERAL LIABILITY INSURANCE REQUIREMENTS

California does not mandate general liability insurance by statute for most businesses, but contractual requirements, licensing requirements, and practical business necessity make CGL coverage essential. The California Department of Insurance regulates general liability forms and rates filed for use in the state.

STANDARD MINIMUM LIMITS:
The industry standard minimum for commercial general liability in California is $1,000,000 per occurrence / $2,000,000 general aggregate. For most commercial leases, landlords require a minimum of $1,000,000 per occurrence with the landlord listed as an additional insured. Higher limits are required by contract for many commercial operations.

CONTRACTOR LICENSING AND INSURANCE REQUIREMENTS:
Under California Business and Professions Code Section 7071.5, contractors licensed by the Contractors State License Board (CSLB) must maintain a contractor''s license bond and are strongly encouraged to carry liability insurance. General contractors are required by most project owners and general contractors to maintain CGL with minimum $1,000,000/$2,000,000 limits. Subcontractors typically must be named on the general contractor''s policy as additional insureds or carry their own policy.

ADDITIONAL INSURED REQUIREMENTS:
California contracts routinely require that the policyholder add specified parties as additional insureds on a primary and non-contributory basis. ISO Additional Insured endorsement CG 20 10 (ongoing operations) and CG 20 37 (completed operations) are standard. The additional insured''s own negligence may be covered (completed operations additional insured endorsements). Certificates of Insurance must reflect additional insured status; the certificate alone does not confer coverage.

CLAIMS-MADE VS. OCCURRENCE:
General liability policies are typically written on an occurrence basis, meaning coverage applies to bodily injury or property damage that occurs during the policy period, regardless of when the claim is made. Claims-made policies (less common for CGL) cover claims first made during the policy period. Claims-made policies require extended reporting periods (tail coverage) of at least 1 year and must include a retroactive date.

PROFESSIONAL LIABILITY EXCLUSION:
Standard CGL policies exclude professional liability (errors and omissions). Businesses providing professional services (architects, engineers, IT consultants, financial advisors) must obtain separate professional liability coverage. Some endorsements restore limited professional liability coverage for incidental professional services.

EMPLOYMENT PRACTICES LIABILITY:
Employment Practices Liability (EPLI) is not part of standard CGL but is commonly required or recommended for California employers due to the state''s robust employee protection laws (FEHA, CFRA, CCPA). California has among the highest rates of employment-related litigation in the country.

CONSTRUCTION DEFECT CLAIMS:
California Civil Code Section 895-945.5 (SB 800, Right to Repair Act) governs construction defect claims for new residential construction. Builders must comply with pre-litigation dispute resolution procedures. CGL policies for contractors must carefully address the "your work" exclusion and the "your product" exclusion, as construction defect claims may be limited by these standard exclusions. Wrap-up programs (OCIPs and CCIPs) are common for large construction projects in California.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 6. TX Workers Compensation Requirements
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A1000006-0000-0000-0000-000000000001',
    NULL,
    'TX Workers Compensation Requirements and Regulations',
    0,  -- Regulation
    6,  -- WorkersCompensation
    'TX',
    N'TEXAS WORKERS COMPENSATION INSURANCE REQUIREMENTS AND REGULATIONS

Texas is unique among U.S. states in that workers compensation insurance is not mandatory for most private employers. Texas Labor Code Chapter 406 governs workers compensation. The Texas Department of Insurance Division of Workers'' Compensation (TDI-DWC) administers the system.

SUBSCRIBER VS. NON-SUBSCRIBER STATUS:
Employers who purchase workers compensation insurance are called "subscribers." Employers who elect not to purchase workers compensation are called "non-subscribers." Non-subscribers lose certain common law defenses (contributory negligence, fellow servant rule, assumption of risk) and are exposed to tort liability for workplace injuries. Non-subscribers must notify their employees in writing and must notify TDI of their non-subscriber status annually.

MANDATORY COVERAGE FOR CERTAIN EMPLOYERS:
While not universally required, workers compensation is mandatory for: (1) Employers with state government contracts over $100,000; (2) Motor carriers; (3) Employers in certain regulated industries. Construction contractors working on state projects must provide proof of workers compensation coverage.

STANDARD POLICY STRUCTURE (PART 1 AND PART 2):
Texas workers compensation policies follow the NCCI Workers Compensation and Employers Liability Insurance Policy form. Part 1 (Workers Compensation) provides statutory benefits per Texas law — this is an unlimited-benefit policy (no cap on medical or indemnity). Part 2 (Employers Liability) provides coverage for employer liability claims not covered by the workers compensation statute, with standard minimum limits of $100,000 per occurrence / $500,000 policy aggregate / $100,000 per disease per employee.

BENEFITS UNDER TEXAS WORKERS COMPENSATION:
Statutory benefits include: Medical benefits (all reasonable and necessary medical treatment with no dollar cap, subject to workers comp fee schedule). Income benefits (Temporary Income Benefits = 70% of average weekly wage up to state maximum; Impairment Income Benefits for permanent impairment; Supplemental Income Benefits). Death benefits for surviving spouse and dependents. Burial benefits up to $10,000. Benefits are calculated based on the employee''s average weekly wage in the 13 weeks before the injury.

EXPERIENCE MODIFICATION FACTOR:
Texas uses the NCCI experience rating plan. The Experience Modification Factor (EMR or X-Mod) compares an employer''s actual loss history to expected losses for their industry. An EMR of 1.00 is average. EMR below 1.00 results in premium credits; above 1.00 results in surcharges. The EMR is calculated annually and applied to the manual premium. Employers with three or more years of payroll data are subject to experience rating.

PAYROLL AND CLASS CODES:
Workers compensation premiums are calculated based on payroll (per $100 of remuneration) multiplied by class code rates. NCCI assigns class codes based on the employer''s business operations. Common Texas class codes include: 8810 (Clerical Office), 5537 (HVAC contractors), 8742 (Outside Sales), 3632 (Machine Shops). Each class code has an assigned rate reflecting the relative hazard of that occupation.

POLICY REPORTING AND COMPLIANCE:
Policies must be filed with TDI-DWC. Subscribers must post a notice to employees informing them of workers compensation coverage. Employers must report injuries to TDI-DWC and to the insurance carrier. First Report of Injury (FROI) must be filed within 8 days of the employer''s knowledge of an injury. Medical treatment must be directed through a certified workers compensation health care network if the employer participates in one.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- =====================================================
-- VALIDATION RULES (Category = 2)
-- =====================================================

-- 7. Effective Date Validation Rules
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A2000001-0000-0000-0000-000000000001',
    NULL,
    'Policy Effective Date Validation Rules',
    2,  -- ValidationRule
    NULL,
    NULL,
    N'POLICY EFFECTIVE DATE VALIDATION RULES

These rules govern acceptable policy effective and expiration dates across all lines of business. All dates are validated at time of policy entry, binding, and issuance.

PAST EFFECTIVE DATES:
Policy effective dates must not be more than 30 calendar days in the past at the time of binding. Backdating a policy beyond 30 days requires supervisor approval with documented business justification. Backdating beyond 48 hours requires supervisor approval in all cases. Policies may not be backdated to cover a loss that has already occurred — this constitutes fraud. Any backdating must be disclosed on the policy application and confirmed with the carrier underwriter.

FUTURE EFFECTIVE DATES:
Policy effective dates cannot be more than 90 calendar days in the future without prior underwriting approval. Future-dated policies beyond 90 days require the underwriter to confirm acceptability of the risk at the future date. Rate lock is not guaranteed for future-dated policies beyond 30 days. The carrier may require re-rating if market conditions or the insured''s risk profile changes between submission and the effective date.

EXPIRATION DATE RULES:
The policy expiration date must always be after the effective date. The minimum policy duration is 1 day (binders and short-rate policies). Standard policy terms by line of business: Personal Auto — 6 months or 12 months standard. Homeowners — 12 months standard. Commercial Lines — 12 months standard, 3-year policies available for some lines. Workers Compensation — 12 months standard. Life Insurance — no fixed expiration (permanent) or defined term (term life).

STANDARD POLICY TERMS:
6-month policies: exactly 182 days (6 calendar months). 12-month policies: exactly 365 days (12 calendar months; 366 days in a leap year). 3-year policies: exactly 1,095 days (3 calendar years, accounting for leap years). Non-standard terms (e.g., 9 months, 18 months) require underwriting approval and may be subject to pro-rata or short-rate premium calculations.

SHORT-RATE AND PRO-RATA CANCELLATION:
When a policy is cancelled mid-term by the insured, the return premium is typically calculated on a short-rate basis (insured receives less than pro-rata for the unused term, reflecting the insurer''s expense loading). When cancelled by the insurer (except for fraud), the return premium is calculated on a pro-rata basis (exact proportion of unused term). Short-rate tables or standard factors per line of business apply.

BINDERS:
Insurance binders are temporary evidence of coverage valid for up to 90 days (varies by state). Binders must be converted to a formal policy before expiration. Binders cannot be renewed — a new binder or policy must be issued. The binder effective date follows the same rules as policy effective dates (no more than 30 days past, 90 days future without approval).

RENEWAL TIMING:
Policy renewal offers must be delivered to the insured no later than the renewal period required by the applicable state law (typically 30–60 days before expiration). If the renewal offer is not delivered on time, the existing policy may need to be extended at current terms until proper notice is given. Renewal policies automatically use the expiration date of the prior term as the new effective date to avoid gaps in coverage.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 8. Premium Range Validation Rules
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A2000002-0000-0000-0000-000000000001',
    NULL,
    'Policy Premium Range Validation Rules',
    2,  -- ValidationRule
    NULL,
    NULL,
    N'POLICY PREMIUM RANGE VALIDATION RULES

These rules define acceptable premium ranges by line of business and trigger underwriting review for anomalous premiums. Premiums outside these ranges are not automatically declined but require additional underwriting scrutiny.

PERSONAL AUTO PREMIUM RANGES:
Annual premiums below $200 for personal auto indicate potential under-rating and require underwriting review. This threshold accounts for minimum state liability-only coverage for low-risk drivers in rural areas. Annual premiums above $50,000 for personal auto are highly unusual and require senior underwriter sign-off. Standard voluntary market premiums for personal auto range from $500 to $5,000 annually depending on coverage, driver history, and vehicle. Premiums must not be negative. Premium components must sum to the total policy premium.

HOMEOWNERS PREMIUM RANGES:
Annual homeowners premiums below $300 suggest the dwelling value may be understated or the policy is a shell. Premiums below $300 require confirmation of Coverage A (dwelling replacement cost value). Annual homeowners premiums above $100,000 indicate either a very high-value home (Coverage A above $5,000,000) or a rating error, and require underwriting review. Standard homeowners premiums range from $800 to $8,000 annually for typical residential properties.

COMMERCIAL GENERAL LIABILITY PREMIUM RANGES:
CGL annual premiums below $500 suggest under-reporting of exposure (payroll or revenue basis) or very limited operations. Premiums below $500 require documentation of the exposure base and annual revenues. Annual premiums above $500,000 require senior underwriter approval and may require reinsurance treaty review. Standard CGL premiums range from $1,500 to $50,000 for small-to-mid-size businesses.

WORKERS COMPENSATION PREMIUM RANGES:
Workers compensation premiums must be calculated using approved state bureau or NCCI class code rates applied to reported payroll. The minimum annual premium for workers compensation is typically $500 (varies by carrier and state). There is no maximum premium threshold, as large employer payrolls can produce very large premiums. Premium estimates must reconcile to audited payroll at policy expiration; audit adjustments apply. Premium must be allocated across class codes in proportion to payroll by classification.

PROFESSIONAL LIABILITY PREMIUM RANGES:
E&O / Professional Liability annual premiums below $500 are unusual outside of sole practitioners. Premiums below $500 require documentation of the professional services rendered and revenue. Premiums above $250,000 require facultative reinsurance consideration. Standard professional liability premiums range from $1,500 to $30,000 for small professional firms.

PREMIUM ALLOCATION RULES:
The sum of all individual coverage premiums must equal the total policy premium within $1.00 rounding tolerance. Premium taxes, surcharges, and fees must be itemized separately from the base premium. Minimum earned premium applies in most lines: typically 25% of annual premium for standard policies. Fully earned premium at inception applies to some short-term and specialty coverages (event insurance, single-trip commercial auto).

ENDORSEMENT PREMIUM ADJUSTMENTS:
Mid-term endorsement premiums are calculated on a pro-rata basis for the remaining policy term. Premium credits (reductions) for mid-term endorsements are applied as return premium. Endorsements that increase exposure (additional vehicles, increased limits, additional locations) generate additional premium. Endorsements that reduce exposure generate return premium on pro-rata basis unless the policy is on a short-rate cancellation schedule.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 9. Coverage Limits Validation Rules
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A2000003-0000-0000-0000-000000000001',
    NULL,
    'Coverage Limits Validation Rules',
    2,  -- ValidationRule
    NULL,
    NULL,
    N'COVERAGE LIMITS VALIDATION RULES

These rules define acceptable coverage limit structures across all lines of business and flag limit combinations that are technically invalid, contractually improper, or indicate potential rating errors.

BODILY INJURY LIABILITY LIMIT STRUCTURE:
Bodily injury (BI) liability limits are expressed as per-person / per-occurrence (split limits) or as a combined single limit (CSL). Per-person limits must always be less than or equal to per-occurrence limits (e.g., 100/300 is valid; 300/100 is invalid). Per-person BI limits must meet or exceed the state mandatory minimum (e.g., $15,000 in CA, $25,000 in NY, $30,000 in TX). Combined Single Limit (CSL) auto liability minimum is typically $300,000 where split limits are not required. CSL policies must document that the limit applies to combined BI and PD.

PROPERTY DAMAGE LIABILITY LIMITS:
Property damage (PD) limits must meet or exceed the state mandatory minimum ($5,000 CA, $10,000 NY, $25,000 TX). PD limits have no maximum imposed by regulation but excess limits above $500,000 for personal auto are unusual and may suggest a rating error.

UMBRELLA AND EXCESS LIABILITY:
Umbrella and excess liability policies require underlying liability limits to be maintained at specific minimum levels, typically: Personal Auto BI/PD at $250,000/$500,000 minimum, Homeowners Personal Liability at $300,000 minimum, CGL at $1,000,000/$2,000,000 minimum. If underlying coverage falls below required minimums, the umbrella policy may become primary for the gap. Umbrella policies must list all required underlying policies. Umbrella aggregate limits must be at least equal to the umbrella per-occurrence limit.

AGGREGATE VS. PER-OCCURRENCE LIMITS:
For Commercial General Liability and Professional Liability, the aggregate limit must be at least equal to the per-occurrence (per-claim) limit. Common structures: $1M/$2M (per occurrence / aggregate), $2M/$4M, $5M/$10M. An aggregate limit lower than the per-occurrence limit is technically valid (aggregate could be exhausted by a single large claim) but unusual and requires underwriting approval. Products-Completed Operations aggregate is typically equal to the general aggregate.

DEDUCTIBLE LIMITS:
Deductibles must not exceed 10% of the coverage limit for standard admitted market policies. Example: A $500,000 dwelling cannot have a deductible greater than $50,000. Exceptions apply for hurricane/wind deductibles (percentage deductibles are standard in coastal states). High deductible programs for commercial lines (large deductible workers compensation, CGL large deductible) allow higher deductibles with collateral requirements.

PROFESSIONAL LIABILITY RETROACTIVE DATES:
For claims-made policies, the retroactive date must not be later than the first date the insured began providing professional services. The retroactive date determines the earliest date from which incidents can give rise to covered claims. Advancing (moving forward) a retroactive date eliminates prior acts coverage and must be done only with explicit insured consent. Full prior acts coverage has a retroactive date equal to the inception of the first claims-made policy in the continuous coverage chain.

INLAND MARINE AND SCHEDULED PROPERTY:
Scheduled property items must have an agreed value or stated amount that reflects reasonable market value. Blanket property limits must be sufficient to cover the total value of all items subject to the blanket. Coinsurance clauses (where applicable) require the limit of insurance to equal at least 80% (or specified percentage) of the property''s total value. Coinsurance penalty applies when the limit is below the required percentage at time of loss.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 10. Required Coverages by Line of Business
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A2000004-0000-0000-0000-000000000001',
    NULL,
    'Required Coverages by Line of Business',
    2,  -- ValidationRule
    NULL,
    NULL,
    N'REQUIRED COVERAGES BY LINE OF BUSINESS

This document defines mandatory coverage components that must be present on policies by line of business. Policies missing required coverages are considered incomplete and should not be bound without resolution.

PERSONAL AUTO (LineOfBusiness = PersonalAuto):
Required: Bodily Injury Liability and Property Damage Liability (collectively, third-party liability) are mandatory on all personal auto policies in all states. These are the only federally non-negotiable coverages. Required if financed: Comprehensive and Collision coverage are required by the lienholder/lender for any vehicle with an active loan or lease. Required by state: Uninsured Motorist (NY), Personal Injury Protection/No-Fault (NY, FL, MI, NJ, PA, HI, KY, MN, ND, UT). Optional but standard: Medical Payments (MedPay), Rental Reimbursement, Roadside Assistance, Gap Coverage.

HOMEOWNERS (LineOfBusiness = Homeowners):
Required on HO-3 (the standard form): Coverage A — Dwelling (always required; must cover the structure at replacement cost). Coverage E — Personal Liability (always required; protects against third-party bodily injury/property damage claims). Optional but standard: Coverage B — Other Structures (10% of Coverage A by default, can be increased or excluded). Coverage C — Personal Property (typically 50-70% of Coverage A). Coverage D — Loss of Use (Additional Living Expenses, typically 20-30% of Coverage A). Coverage F — Medical Payments to Others ($1,000–$5,000, standard). Required separately: Flood insurance (if in SFHA), Earthquake insurance (CA, WA, OR and seismic zones), Windstorm (separate policy in some FL counties).

COMMERCIAL GENERAL LIABILITY (LineOfBusiness = GeneralLiability):
Required under standard ISO CGL form (CG 00 01): Section I — Coverages: Coverage A (Bodily Injury and Property Damage), Coverage B (Personal and Advertising Injury), Coverage C (Medical Payments). Required: Products and Completed Operations Aggregate (included in standard form). Required: Fire Damage Legal Liability (Damage to Premises Rented to You, minimum $50,000). Optional (not included): Pollution Liability, Professional Liability, Cyber Liability, EPLI — these require separate policies or endorsements.

WORKERS COMPENSATION (LineOfBusiness = WorkersCompensation):
Required by standard NCCI/state policy form: Part 1 — Workers Compensation (statutory benefits; no dollar limit). Part 2 — Employers Liability (always included; standard limits $100,000/$500,000/$100,000 but should be increased). Not included in standard WC: Stop-Gap Employers Liability (for monopolistic fund states: ND, OH, WA, WY). USL&H (U.S. Longshore and Harbor Workers'' Compensation Act) endorsement required if employees work on navigable waters. Federal Employers Liability Act (FELA) coverage for railroad employees (separate policy).

PROFESSIONAL LIABILITY / E&O (LineOfBusiness = ProfessionalLiability):
Required under standard claims-made form: Claims-made insuring agreement (specified policy period for claims). Retroactive date (must be established). Defense costs (either within limits or outside/supplementary). Extended Reporting Period (ERP/tail) option must be offered. Required disclosure: Claims-made policies must include a clear explanation of the claims-made trigger and the ERP provision. Not included: Cyber liability, EPLI, Directors & Officers — separate policies required.

UMBRELLA (LineOfBusiness = Umbrella):
Required: Scheduled underlying policies (auto, homeowners/CGL, employers liability must be listed). Required: Follow-form provision for covered losses in excess of underlying limits. Required: Drop-down provision for losses where underlying is exhausted or uncollectible. Not included: Coverage that the underlying policies specifically exclude (pollution, professional liability, cyber) unless specifically endorsed.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- =====================================================
-- SAMPLE POLICIES (Category = 1)
-- =====================================================

-- 11. Sample Personal Auto Policy - Standard
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A3000001-0000-0000-0000-000000000001',
    NULL,
    'Sample Personal Auto Policy - Standard Coverage',
    1,  -- SamplePolicy
    0,  -- PersonalAuto
    NULL,
    N'SAMPLE PERSONAL AUTO POLICY - STANDARD COVERAGE

DECLARATIONS PAGE
Policy Number: PAP-2024-001234
Named Insured: John Smith
Co-Insured: Jane Smith (spouse)
Address: 456 Oak Avenue, Sacramento, CA 95814
Policy Period: January 1, 2024 to January 1, 2025 (12 months)
Carrier: Acme Insurance Company
Agent: West Coast Insurance Brokers, License #0A12345

COVERED VEHICLES:
Vehicle 1: 2020 Toyota Camry SE, VIN 4T1BF1FK5LU123456, Garaging Zip 95814
Vehicle 2: 2019 Honda CR-V EX, VIN 7FARW2H57KE123456, Garaging Zip 95814

COVERAGES AND PREMIUMS — VEHICLE 1 (2020 Toyota Camry):
Bodily Injury Liability: $100,000 per person / $300,000 per accident, Premium: $450.00/year
Property Damage Liability: $100,000 per accident, Premium: $200.00/year
Uninsured Motorist BI: $100,000 per person / $300,000 per accident, Premium: $75.00/year
Uninsured Motorist PD: $3,500 limit / $250 deductible, Premium: $30.00/year
Medical Payments: $5,000 per person, Premium: $45.00/year
Comprehensive: $500 deductible, ACV settlement, Premium: $180.00/year
Collision: $500 deductible, ACV settlement, Premium: $350.00/year
Rental Reimbursement: $30/day, 30 days maximum, Premium: $25.00/year
Roadside Assistance: Standard plan, Premium: $15.00/year
Vehicle 1 Subtotal: $1,370.00/year

COVERAGES AND PREMIUMS — VEHICLE 2 (2019 Honda CR-V):
Bodily Injury Liability: $100,000/$300,000, Premium: $380.00/year
Property Damage Liability: $100,000, Premium: $175.00/year
Uninsured Motorist BI: $100,000/$300,000, Premium: $65.00/year
Uninsured Motorist PD: $3,500/$250 deductible, Premium: $28.00/year
Medical Payments: $5,000, Premium: $40.00/year
Comprehensive: $500 deductible, Premium: $220.00/year
Collision: $500 deductible, Premium: $420.00/year
Rental Reimbursement: $30/day, 30 days, Premium: $25.00/year
Roadside Assistance: Premium: $15.00/year
Vehicle 2 Subtotal: $1,368.00/year

DISCOUNTS APPLIED:
Multi-vehicle discount: -$180.00
Good driver discount (both insureds): -$275.00
Anti-theft device discount: -$40.00

TOTAL ANNUAL PREMIUM: $2,243.00
FEES: Policy fee $25.00, State surcharge $18.50

BILLING: Direct Bill — Semi-Annual Payment Plan
First Installment Due: January 1, 2024 — $1,143.25
Second Installment Due: July 1, 2024 — $1,143.25

LIENHOLDER: First National Bank, P.O. Box 1000, Sacramento, CA 95801 (Vehicle 1)
LIENHOLDER: Toyota Financial Services, P.O. Box 5000, Torrance, CA 90503 (Vehicle 2)

NOTES: Both vehicles must maintain comprehensive and collision coverage while lienholder interests exist. Loss payable clause applies to both lienholders.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 12. Sample Commercial General Liability Policy
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A3000002-0000-0000-0000-000000000001',
    NULL,
    'Sample Commercial General Liability Policy - Standard CGL',
    1,  -- SamplePolicy
    4,  -- GeneralLiability
    NULL,
    N'SAMPLE COMMERCIAL GENERAL LIABILITY POLICY - STANDARD CGL

DECLARATIONS PAGE
Policy Number: CGL-2024-005678
Named Insured: ABC Construction LLC
DBA: ABC General Contracting
Business Address: 789 Industrial Blvd, Houston, TX 77001
Business Type: General Contractor — Residential and Light Commercial
FEIN: 74-1234567
Policy Period: March 1, 2024 to March 1, 2025 (12 months)
Carrier: Builders Insurance Group, A.M. Best Rating: A (Excellent)
Agent: Texas Commercial Insurance Brokers

COVERAGE LIMITS:
Each Occurrence Limit: $1,000,000
General Aggregate Limit: $2,000,000
Products and Completed Operations Aggregate: $2,000,000
Personal and Advertising Injury Limit: $1,000,000
Damage to Premises Rented to You: $100,000 (any one premises)
Medical Payments Limit: $5,000 (any one person)

DEDUCTIBLE: $2,500 per occurrence (applies to property damage only)

PREMIUM CALCULATION:
Exposure Basis: Payroll
Annual Payroll — Direct Employees: $850,000
Annual Payroll — Subcontracted Work (rated at 15%): $2,000,000 (subcontracted)
Effective Subcontracted Payroll for Rating: $300,000

Classification 91340 — Contractors — Subcontracted Work (basis: subcontracted payroll):
Rate per $100: $12.50, Payroll: $300,000, Premium: $37,500
Classification 91585 — General Contractor — Residential (basis: direct payroll):
Rate per $100: $18.75, Payroll: $850,000, Premium: $159,375
Subtotal Manual Premium: $196,875
Experience Modification Factor: 0.88 (credit mod — favorable loss history)
Modified Premium: $173,250
Schedule Rating: -10% (safety program, loss control investment)
Net Premium after Schedule Rating: $155,925
Minimum Premium: $5,000 (N/A — premium exceeds minimum)
Annual Premium: $4,500 (Note: premium above represents full program; this sample reflects typical small contractor segment)

ENDORSEMENTS:
CG 20 10 04 13 — Additional Insured, Owners Lessees or Contractors (Ongoing Ops)
CG 20 37 04 13 — Additional Insured, Owners Lessees or Contractors (Completed Ops)
CG 25 04 05 09 — Designated Ongoing Operations
Contractors Special Conditions Endorsement
Texas Changes Endorsement

SUBCONTRACTOR REQUIREMENTS (per policy conditions):
All subcontractors must carry minimum CGL limits of $1,000,000/$2,000,000. Named insured must obtain certificates of insurance from all subcontractors prior to commencement of work. Failure to obtain subcontractor certificates may result in an uninsured subcontractors charge at audit.

AUDIT BASIS: Annual payroll audit. Audit performed 60 days after policy expiration. Audit adjustment premium (additional or return) billed/credited within 30 days of audit completion.

BILLING: Agency Bill, Annual Payment Plan — $4,500 due at inception.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 13. Sample Homeowners Policy - HO-3
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A3000003-0000-0000-0000-000000000001',
    NULL,
    'Sample Homeowners Policy HO-3 - Standard Form',
    1,  -- SamplePolicy
    2,  -- Homeowners
    NULL,
    N'SAMPLE HOMEOWNERS POLICY HO-3 - STANDARD FORM

DECLARATIONS PAGE
Policy Number: HO-2024-009876
Named Insured: Mary Johnson
Co-Insured: Robert Johnson (spouse)
Insured Location: 123 Main Street, Austin, TX 78701
Policy Period: April 15, 2024 to April 15, 2025 (12 months)
Policy Form: HO 00 03 10 00 (Special Form — Open Perils on Dwelling, Named Perils on Contents)
Carrier: Lone Star Property Insurance Company
Agent: Austin Independent Insurance Agency

PROPERTY DESCRIPTION:
Construction: Frame / Masonry veneer
Year Built: 2005
Square Footage: 2,450 sq ft
Number of Stories: 2
Roof Material: Architectural shingles, Installed 2018
Protection Class: 3
Distance to Fire Station: 1.2 miles

SECTION I — PROPERTY COVERAGES:
Coverage A — Dwelling: $350,000 replacement cost value (RCV)
Coverage B — Other Structures: $35,000 (10% of Coverage A)
Coverage C — Personal Property: $175,000 (50% of Coverage A), Replacement Cost Value
Coverage D — Loss of Use (Additional Living Expenses): $70,000 (20% of Coverage A)

SECTION II — LIABILITY COVERAGES:
Coverage E — Personal Liability: $300,000 per occurrence
Coverage F — Medical Payments to Others: $5,000 per person

DEDUCTIBLES:
All Peril (AOP) Deductible: $1,000 (applies to all covered losses except wind/hail and hurricane)
Wind/Hail Deductible: $3,500 (flat dollar, per occurrence)
Hurricane Deductible: $7,000 (2% of Coverage A = 2% x $350,000)
Note: Hurricane deductible applies separately from wind/hail deductible; the higher of the two applies during a named storm.

PREMIUM SUMMARY:
Base Dwelling Premium (Coverage A): $1,420.00
Contents RCV Endorsement (Coverage C at RCV vs ACV): $200.00
Personal Liability Endorsement (increased from $100,000 to $300,000): $50.00
Medical Payments to Others: $30.00
Water Backup and Sump Overflow Endorsement: $75.00
Equipment Breakdown Endorsement: $45.00
Identity Theft Protection Endorsement: $30.00
Animal Liability Exclusion Endorsement: -$20.00 (one dog, excluded)
Protective Device Credit (monitored alarm): -$75.00
Claims-Free Discount (5+ years): -$140.00
Paid-in-Full Discount: -$115.00
TOTAL ANNUAL PREMIUM: $1,500.00

MORTGAGEE:
First National Mortgage Corp
P.O. Box 10000, Dallas TX 75201
Loan Number: 78954321
Mortgagee clause: Standard — ISAOA ATIMA

ADDITIONAL INTERESTS:
Homeowners Association: Oakwood HOA, 100 Community Drive, Austin TX 78701

SCHEDULED PERSONAL PROPERTY (Jewelry Floater — HO 04 61):
Item 1: Diamond engagement ring, Agreed Value $8,500, Premium: $85.00/year
Item 2: Rolex watch, Agreed Value $5,000, Premium: $50.00/year
(Scheduled items not subject to AOP deductible)

KEY EXCLUSIONS (summary — refer to policy form for complete list):
Flood damage (separate NFIP or private flood policy required)
Earthquake damage (endorsement available)
Mold, unless resulting from a covered water loss
Wear and tear, deterioration
Business pursuits (home-based business may require endorsement)
Intentional acts
War and nuclear hazard',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 14. Sample Professional Liability (E&O) Policy
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A3000004-0000-0000-0000-000000000001',
    NULL,
    'Sample Professional Liability (E&O) Policy - Financial Advisors',
    1,  -- SamplePolicy
    5,  -- ProfessionalLiability
    NULL,
    N'SAMPLE PROFESSIONAL LIABILITY / ERRORS & OMISSIONS POLICY — FINANCIAL ADVISORS

DECLARATIONS PAGE
Policy Number: PL-2024-003321
Named Insured: Smith Financial Advisors LLC
Named Insured 2: John A. Smith, CFP (individual coverage included)
Principal Address: 500 Financial Center Drive, Suite 200, Chicago, IL 60601
Professional Services: Financial Planning, Investment Advisory Services, Retirement Planning
Annual Revenue: $1,200,000
Number of Licensed Professionals: 8 (6 advisors, 2 registered representatives)
Regulatory Status: SEC-Registered Investment Adviser (RIA), CRD #123456
Policy Period: January 1, 2024 to January 1, 2025
Policy Form: Claims-Made and Reported
Carrier: Professional Indemnity Insurance Co., A.M. Best: A+
Agent: National Professional Lines Group

COVERAGE DETAILS:
Each Claim Limit: $1,000,000
Annual Aggregate Limit: $2,000,000
Retroactive Date: January 1, 2018 (continuous coverage since 2018; prior acts covered back to retroactive date)
Defense Costs: Outside the limits (Supplementary Payments — defense does not erode the coverage limit)
Deductible: $5,000 per claim, $15,000 annual aggregate deductible
Deductible applies to: Defense costs and indemnity payments combined
Consent to Settle: Hammer clause at 80/20 (insurer pays 80% of excess if insured refuses reasonable settlement)

ANNUAL PREMIUM: $3,200.00
Premium Basis: Revenue ($1,200,000 annual revenue) + Professional headcount (8)
Rate per $1,000 revenue: $2.25
Headcount loading: $200 per professional above 3 = $1,000
Total Premium: $2,700 + $500 credits = $3,200

EXTENDED REPORTING PERIOD (TAIL) OPTIONS:
60-day automatic ERP: Included at no charge (applies automatically upon policy cancellation or non-renewal)
1-year optional ERP: 75% of expiring annual premium ($2,400)
2-year optional ERP: 125% of expiring annual premium ($4,000)
3-year optional ERP: 175% of expiring annual premium ($5,600)
Unlimited ERP: Available upon retirement, death, or disability at 200% of expiring premium ($6,400)
Note: Optional ERP must be purchased within 60 days of policy expiration or cancellation.

COVERED PROFESSIONAL SERVICES:
Investment advisory services and portfolio management
Financial planning and retirement planning advice
Tax planning (not tax preparation)
Estate planning advice (not legal services)
529 and education savings plan recommendations
401(k) and retirement plan consulting

EXCLUSIONS (Key — refer to full policy form for complete list):
Intentional, dishonest, fraudulent, or criminal acts (coverage afforded to innocent insureds)
Bodily injury and property damage (CGL coverage required)
Employment practices liability (EPLI policy required)
Theft, conversion, or misappropriation of funds (crime coverage required)
Guarantees of investment performance
Services not within the definition of Professional Services
Prior known circumstances as of policy inception (subject to prior knowledge exclusion)
ERISA violations as a plan fiduciary (separate fiduciary liability policy required)

IMPORTANT CLAIMS-MADE CONDITIONS:
Claims must be first made and reported during the policy period (or ERP). Circumstances that may give rise to a claim should be reported to the insurer as soon as practicable even if no formal claim has been made. Reporting a circumstance during the policy period preserves coverage under the ERP if a formal claim arises later. Failure to report known circumstances before policy expiration may result in claim denial.

REGULATORY PROCEEDINGS COVERAGE:
This policy includes coverage for regulatory investigations by the SEC, FINRA, state securities regulators, and other governmental bodies, subject to: $250,000 sublimit per regulatory proceeding, $500,000 aggregate for all regulatory proceedings.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

-- 15. Sample Workers Compensation Policy
INSERT INTO PolicyAssistantReferenceDocuments
    (Id, TenantId, Title, Category, LineOfBusiness, State, Content, Source, CreatedAt, UpdatedAt)
VALUES (
    'A3000005-0000-0000-0000-000000000001',
    NULL,
    'Sample Workers Compensation Policy - Manufacturing',
    1,  -- SamplePolicy
    6,  -- WorkersCompensation
    NULL,
    N'SAMPLE WORKERS COMPENSATION AND EMPLOYERS LIABILITY POLICY — MANUFACTURING

DECLARATIONS PAGE
Policy Number: WC-2024-007890
Named Insured: Delta Manufacturing Inc.
Principal Address: 2200 Industrial Parkway, San Antonio, TX 78219
FEIN: 74-9876543
Business Type: Precision Machine Shop, Custom Metal Fabrication
Policy Period: July 1, 2024 to July 1, 2025 (12 months)
Policy Form: WC 00 00 00 C (Standard NCCI Workers Compensation and Employers Liability Policy)
Carrier: Texas Mutual Insurance Company (largest WC carrier in Texas)
Agent: Alamo Commercial Insurance Agency

PART 1 — WORKERS COMPENSATION INSURANCE:
State: Texas
Coverage: Statutory — all benefits required by the Texas Workers'' Compensation Act apply. This is an unlimited-benefit policy; there are no dollar caps on medical benefits or maximum indemnity periods for most injury categories under Texas law. The policy provides coverage for all employees of the insured operating within the state of Texas.

PART 2 — EMPLOYERS LIABILITY INSURANCE:
Each Accident Limit: $500,000 (increased from statutory minimum $100,000)
Disease — Policy Limit: $500,000
Disease — Each Employee Limit: $500,000
Jurisdictional Territory: Texas (with All States endorsement WC 00 03 26 for employees temporarily working in other states)

CLASSIFICATION AND PAYROLL:
Class Code 3632 — Machine Shops (not otherwise classified): Annual Payroll $1,200,000, Rate per $100: $6.85, Manual Premium: $82,200
Class Code 8742 — Salespersons (outside), drivers: Annual Payroll $150,000, Rate per $100: $1.20, Manual Premium: $1,800
Class Code 8810 — Clerical Office Employees: Annual Payroll $250,000, Rate per $100: $0.28, Manual Premium: $700
Class Code 8742 — Executive Officers (if included): Annual Payroll $400,000 (4 officers x $100,000 average), Rate: $1.20, Manual Premium: $4,800
Total Estimated Annual Payroll: $2,000,000
Total Manual Premium: $89,500

EXPERIENCE MODIFICATION FACTOR (EMR): 0.95 (favorable — below average loss experience)
Modified Premium: $89,500 x 0.95 = $85,025

SCHEDULE RATING:
Safety Program Credit: -5% (documented safety committee, OSHA 300 logs reviewed)
Experience Rating Adjustment: Standard
Net Premium After Schedule Rating: $80,774

MINIMUM PREMIUM: $500 (N/A — premium exceeds minimum)
ESTIMATED ANNUAL PREMIUM: $18,000
Note: The $18,000 reflects small employer tier; larger manufacturers with this payroll structure would be subject to the full calculated premium above. This sample uses a simplified illustration.

PREMIUM PAYMENT AND AUDIT:
Billing: Agency Bill, Annual Payment — $18,000 due at policy inception
Deposit Premium: $18,000 (based on estimated payroll)
AUDIT: Policy is subject to annual payroll audit. Audit performed within 60 days of policy expiration. Premium adjusted based on actual audited payroll vs. estimated payroll. Additional premium or return premium billed/credited within 30 days.

ENDORSEMENTS:
WC 00 03 13 — Voluntary Compensation and Employers Liability Coverage Endorsement
WC 00 03 26 — Other States Insurance (all states except monopolistic fund states)
TX 00 01 — Texas Changes Endorsement (required for all TX WC policies)
WC 00 04 19 — Premium Discount Endorsement (if premium qualifies — typically $5,000+ annual premium)

NEW HIRE REPORTING REQUIREMENT:
Named insured must report all new hires to the State of Texas New Hire Reporting Program within 20 days of hire date. Named insured must also notify the insurance carrier of significant changes in payroll or operations that may affect premium by more than 25%.

LOSS CONTROL SERVICES:
Texas Mutual provides complimentary loss control services including: on-site safety inspections, OSHA compliance review, return-to-work program assistance, and accident investigation support. Loss control compliance may affect renewal premium and schedule rating credits.',
    'System Seed Data',
    GETUTCDATE(),
    GETUTCDATE()
);

COMMIT;
