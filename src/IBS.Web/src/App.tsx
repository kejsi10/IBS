import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useAuthStore } from "./stores/auth";
import { Layout } from "./components/common/layout";
import { ToastProvider } from "./components/ui/toast";
import { LoginPage } from "./features/auth/login-page";
import { DashboardPage } from "./features/dashboard/dashboard-page";
import { ClientsPage } from "./features/clients/clients-page";
import { ClientDetailPage } from "./features/clients/client-detail-page";
import { CarriersPage } from "./features/carriers/carriers-page";
import { CarrierDetailPage } from "./features/carriers/carrier-detail-page";
import { PoliciesPage } from "./features/policies/policies-page";
import { PolicyDetailPage } from "./features/policies/policy-detail-page";
import { PolicyWizard } from "./features/policies/policy-wizard";
import { SettingsLayout } from "./features/settings/settings-layout";
import { UsersPage } from "./features/settings/users/users-page";
import { UserDetailPage } from "./features/settings/users/user-detail-page";
import { RolesPage } from "./features/settings/roles/roles-page";
import { RoleDetailPage } from "./features/settings/roles/role-detail-page";
import { QuotesPage } from "./features/quotes/quotes-page";
import { QuoteDetailPage } from "./features/quotes/quote-detail-page";
import { QuoteWizard } from "./features/quotes/quote-wizard";
import { ClaimsPage } from "./features/claims/claims-page";
import { ClaimDetailPage } from "./features/claims/claim-detail-page";
import { FNOLWizard } from "./features/claims/fnol-wizard";
import { CommissionsPage } from "./features/commissions/commissions-page";
import { StatementDetailPage } from "./features/commissions/statement-detail-page";
import { ReportsPage } from "./features/reports/reports-page";
import DocumentsPage from "./features/documents/documents-page";
import TemplatesPage from "./features/documents/templates/templates-page";
import { AuditPage } from "./features/settings/audit/audit-page";
import { TenantsPage } from "./features/settings/tenants/tenants-page";
import { TenantDetailPage } from "./features/settings/tenants/tenant-detail-page";
import { PolicyAssistantPage } from "./features/policy-assistant/policy-assistant-page";
import { ChatPage } from "./features/policy-assistant/chat-page";

/**
 * TanStack Query client configuration
 */
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: 1,
    },
  },
});

/**
 * Protected route wrapper that redirects to login if not authenticated
 */
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
}

/**
 * Main application component with routing and providers
 */
function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <BrowserRouter>
          <Routes>
            {/* Public routes */}
            <Route path="/login" element={<LoginPage />} />

            {/* Protected routes with layout */}
            <Route
              path="/"
              element={
                <ProtectedRoute>
                  <Layout />
                </ProtectedRoute>
              }
            >
              <Route index element={<DashboardPage />} />
              <Route path="clients" element={<ClientsPage />} />
              <Route path="clients/:id" element={<ClientDetailPage />} />
              <Route path="carriers" element={<CarriersPage />} />
              <Route path="carriers/:id" element={<CarrierDetailPage />} />
              <Route path="policies" element={<PoliciesPage />} />
              <Route path="policies/new" element={<PolicyWizard />} />
              <Route path="policies/:id" element={<PolicyDetailPage />} />
              <Route path="claims" element={<ClaimsPage />} />
              <Route path="claims/new" element={<FNOLWizard />} />
              <Route path="claims/:id" element={<ClaimDetailPage />} />
              <Route path="commissions" element={<CommissionsPage />} />
              <Route path="commissions/statements/:id" element={<StatementDetailPage />} />
              <Route path="quotes" element={<QuotesPage />} />
              <Route path="quotes/new" element={<QuoteWizard />} />
              <Route path="quotes/:id" element={<QuoteDetailPage />} />
              <Route path="reports" element={<ReportsPage />} />
              <Route path="documents" element={<DocumentsPage />} />
              <Route path="documents/templates" element={<TemplatesPage />} />
              <Route path="policy-assistant" element={<PolicyAssistantPage />} />
              <Route path="policy-assistant/:id" element={<ChatPage />} />
              <Route path="settings" element={<SettingsLayout />}>
                <Route index element={<Navigate to="users" replace />} />
                <Route path="users" element={<UsersPage />} />
                <Route path="users/:id" element={<UserDetailPage />} />
                <Route path="roles" element={<RolesPage />} />
                <Route path="roles/:id" element={<RoleDetailPage />} />
                <Route path="audit" element={<AuditPage />} />
                <Route path="tenants" element={<TenantsPage />} />
                <Route path="tenants/:id" element={<TenantDetailPage />} />
              </Route>
            </Route>

            {/* Catch-all redirect */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </BrowserRouter>
      </ToastProvider>
    </QueryClientProvider>
  );
}

export default App;
