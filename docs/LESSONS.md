# IBS — Lessons Learned

Patterns and rules captured after corrections, to prevent repeating mistakes.

---

## Frontend Testing

### 1. MSW v2 Handler URLs Must Match the Axios baseURL Used in Tests

**Mistake:** Handlers used absolute URLs (`https://localhost:7001/api/v1/...`) but in the test environment `VITE_API_URL` is not set, so `axios` uses a relative base URL (`/api/v1`). MSW never matched the requests.

**Rule:** Always use relative URL patterns in MSW handlers (e.g., `/api/v1/clients`). MSW v2 relative patterns match any origin, so they work regardless of the host axios targets.

```ts
// ✅ Correct
const API_URL = '/api/v1';
http.get(`${API_URL}/clients`, handler)

// ❌ Wrong — won't match in jsdom/test env
const API_URL = 'https://localhost:7001/api/v1';
```

---

### 2. MSW Handler Response Shape Must Match the Backend DTO, Not the Frontend Type

**Mistake:** Handlers returned the mapped frontend shape (e.g., `{ items: mockPolicies }`) but services expect the raw backend shape (e.g., `{ policies: [...] }`) and map it themselves. When `mapPolicyListResult(dto)` called `dto.policies.map(...)` and `dto.policies` was `undefined`, it threw a `TypeError`, causing the query to error and `isSuccess` to never be `true`.

**Rule:** Read the service's `Backend*Result` interface to know what shape the handler must return. Match the backend DTO keys exactly.

```ts
// policies.service.ts declares:
interface BackendPolicyListResult {
  policies: PolicySummary[];   // ← handler must return this key
  totalCount: number;
  pageNumber: number;          // ← NOT 'page'
  ...
}

// ✅ Correct handler
return HttpResponse.json({ policies: mockPolicies, totalCount: 2, pageNumber: 1, ... });

// ❌ Wrong — service's mapPolicyListResult() reads dto.policies, finds undefined, throws
return HttpResponse.json({ items: mockPolicies, ... });
```

Services in this project that map backend → frontend shapes:
- `policiesService` — `BackendPolicyListResult.policies` → `PaginatedResult.items`

---

## Backend / EF Core

### 3. `AuditInterceptor` Secondary Save — Must Call `AcceptAllChanges()` First

**Mistake:** `AuditInterceptor.SavedChangesAsync` fires a secondary `context.SaveChangesAsync()` to insert `AuditLog` rows. EF Core calls `AcceptAllChanges()` *after* `SavedChangesAsync` returns, not before. So when the secondary save runs, previously-saved entities (e.g., `Conversation`, `ChatMessageEntity`) are still in `Modified`/`Added` state with stale `RowVersion` original snapshots. The secondary save re-generates `UPDATE … WHERE RowVersion = @staleValue`, finds 0 rows, and throws `DbUpdateConcurrencyException`.

Setting `AutoDetectChangesEnabled = false` (the original fix) only prevents `DetectChanges()` from re-scanning properties; it does **not** reset entity states that are already `Modified`/`Added`.

**Rule:** Always call `context.ChangeTracker.AcceptAllChanges()` *before* the secondary `SaveChangesAsync` inside `SavedChangesAsync` interceptors. This resets every previously-saved entity to `Unchanged` with the correct post-save `RowVersion`, so only the newly added `AuditLog` entries are written in the secondary call.

```csharp
// In SavedChangesAsync interceptor — CORRECT order:
eventData.Context.ChangeTracker.AcceptAllChanges();   // ← reset prior entities first
eventData.Context.ChangeTracker.AutoDetectChangesEnabled = false;
await eventData.Context.SaveChangesAsync(cancellationToken);
```
- `quotesService` — `BackendQuoteListResult.quotes` → `PaginatedResult.items`
- `claimsService` — `BackendClaimListResult.claims` → `PaginatedResult.items`
- `commissionsService` — `BackendScheduleListResult.schedules` / `BackendStatementListResult.statements` → `PaginatedResult.items`
- `clientsService` — returns `response.data` directly (no mapping), so use `PaginatedResult` shape

---

### 3. Test Assertions Must Use the Mapped Frontend Property, Not the Backend DTO Property

**Mistake:** After fixing handler shapes, test assertions still read `data?.quotes` or `data?.claims`. But since services map those to `PaginatedResult.items`, the hook returns `data.items`, not `data.quotes`.

**Rule:** Hook data reflects what the *service returns*, not what the *handler returns*. Always check the service's return type.

```ts
// ✅ Correct — service maps to PaginatedResult<T> with .items
expect(result.current.data?.items).toBeDefined();

// ❌ Wrong — .quotes is the backend field, not in PaginatedResult
expect(result.current.data?.quotes).toBeDefined();
```

---

### 4. Add ToastProvider to the Shared Test Wrapper

**Mistake:** Page components using `useToast()` throw `"useToast must be used within a ToastProvider"` when `ToastProvider` is absent from the test wrapper.

**Rule:** Any test wrapper (`src/test/utils.tsx`) that renders full page components must include `<ToastProvider>`.

```tsx
// src/test/utils.tsx
import { ToastProvider } from '@/components/ui/toast';

function AllProviders({ children }: WrapperProps) {
  return (
    <QueryClientProvider client={createTestQueryClient()}>
      <BrowserRouter>
        <ToastProvider>{children}</ToastProvider>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
```

---

### 5. QuickFilters Renders role="tab", Not role="button"

**Mistake:** Used `getByRole('button', { name: /^active/i })` to find a QuickFilters pill. The component renders `<button role="tab">` inside a `role="tablist"` container.

**Rule:** When testing `QuickFilters`, query with `getByRole('tab', { name: ... })`.

```ts
// ✅ Correct
const activeFilter = screen.getByRole('tab', { name: /^active/i });

// ❌ Wrong — element has explicit role="tab", overriding the implicit button role
const activeFilter = screen.getByRole('button', { name: /^active/i });
```

---

### 6. Add MSW Handlers for All Mutation Endpoints Before Running Page Tests

**Mistake:** Page component tests that trigger mutations (deactivate, reactivate, create) failed with `onUnhandledRequest: 'error'` because the handlers for `POST /clients/:id/deactivate` etc. were missing.

**Rule:** Before adding page component tests, audit all mutations the component can trigger and ensure every endpoint has a handler. Add 204/201 stubs to `handlers.ts` even if the test doesn't assert the mutation itself.

---

### 7. MSW Static Path Handlers Must Be Registered Before Wildcard Handlers

**Mistake (potential):** Registering `GET /policies/:id` before `GET /policies/expiring` causes requests to `/policies/expiring` to match `:id` with `id='expiring'`, returning 404.

**Rule:** In `handlers.ts`, always declare specific/static paths before wildcard (`:param`) paths at the same URL depth.

```ts
// ✅ Correct order
http.get(`${API_URL}/policies/expiring`, handler),       // static first
http.get(`${API_URL}/policies/due-for-renewal`, handler), // static first
http.get(`${API_URL}/policies/:id`, handler),             // wildcard last

// ❌ Wrong — :id catches 'expiring' and 'due-for-renewal'
http.get(`${API_URL}/policies/:id`, handler),
http.get(`${API_URL}/policies/expiring`, handler),
```
