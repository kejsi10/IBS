import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { commissionsService } from '@/services/commissions.service';
import type {
  ScheduleFilters,
  StatementFilters,
  CreateScheduleRequest,
  UpdateScheduleRequest,
  CreateStatementRequest,
  AddLineItemRequest,
  DisputeLineItemRequest,
  AddProducerSplitRequest,
  UpdateStatementStatusRequest,
} from '@/types/api';

/** Query key factory for commissions. */
export const commissionKeys = {
  all: ['commissions'] as const,
  schedules: () => [...commissionKeys.all, 'schedules'] as const,
  scheduleList: (filters?: ScheduleFilters) => [...commissionKeys.schedules(), 'list', filters] as const,
  scheduleDetail: (id: string) => [...commissionKeys.schedules(), 'detail', id] as const,
  statements: () => [...commissionKeys.all, 'statements'] as const,
  statementList: (filters?: StatementFilters) => [...commissionKeys.statements(), 'list', filters] as const,
  statementDetail: (id: string) => [...commissionKeys.statements(), 'detail', id] as const,
  statistics: () => [...commissionKeys.all, 'statistics'] as const,
  summaryReport: (carrierId?: string, month?: number, year?: number) => [...commissionKeys.all, 'summaryReport', carrierId, month, year] as const,
  producerReport: (producerId?: string, month?: number, year?: number) => [...commissionKeys.all, 'producerReport', producerId, month, year] as const,
};

// ── Schedule Hooks ──

/** Hook to fetch paginated commission schedules. */
export function useSchedules(filters?: ScheduleFilters) {
  return useQuery({
    queryKey: commissionKeys.scheduleList(filters),
    queryFn: () => commissionsService.getSchedules(filters),
  });
}

/** Hook to fetch a single schedule by ID. */
export function useSchedule(id: string) {
  return useQuery({
    queryKey: commissionKeys.scheduleDetail(id),
    queryFn: () => commissionsService.getScheduleById(id),
    enabled: !!id,
  });
}

/** Hook to create a commission schedule. */
export function useCreateSchedule() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateScheduleRequest) => commissionsService.createSchedule(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: commissionKeys.schedules() });
    },
  });
}

/** Hook to update a commission schedule. */
export function useUpdateSchedule() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateScheduleRequest }) => commissionsService.updateSchedule(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: commissionKeys.schedules() });
    },
  });
}

/** Hook to deactivate a commission schedule. */
export function useDeactivateSchedule() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => commissionsService.deactivateSchedule(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: commissionKeys.schedules() });
    },
  });
}

// ── Statement Hooks ──

/** Hook to fetch paginated commission statements. */
export function useStatements(filters?: StatementFilters) {
  return useQuery({
    queryKey: commissionKeys.statementList(filters),
    queryFn: () => commissionsService.getStatements(filters),
  });
}

/** Hook to fetch a single statement by ID with line items and splits. */
export function useStatement(id: string) {
  return useQuery({
    queryKey: commissionKeys.statementDetail(id),
    queryFn: () => commissionsService.getStatementById(id),
    enabled: !!id,
  });
}

/** Hook to create a commission statement. */
export function useCreateStatement() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateStatementRequest) => commissionsService.createStatement(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: commissionKeys.statements() });
      queryClient.invalidateQueries({ queryKey: commissionKeys.statistics() });
    },
  });
}

/** Hook to add a line item to a statement. */
export function useAddLineItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ statementId, data }: { statementId: string; data: AddLineItemRequest }) =>
      commissionsService.addLineItem(statementId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: commissionKeys.statementDetail(variables.statementId) });
      queryClient.invalidateQueries({ queryKey: commissionKeys.statements() });
    },
  });
}

/** Hook to reconcile a line item. */
export function useReconcileLineItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ statementId, lineItemId }: { statementId: string; lineItemId: string }) =>
      commissionsService.reconcileLineItem(statementId, lineItemId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: commissionKeys.statementDetail(variables.statementId) });
    },
  });
}

/** Hook to dispute a line item. */
export function useDisputeLineItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ statementId, lineItemId, data }: { statementId: string; lineItemId: string; data: DisputeLineItemRequest }) =>
      commissionsService.disputeLineItem(statementId, lineItemId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: commissionKeys.statementDetail(variables.statementId) });
    },
  });
}

/** Hook to add a producer split. */
export function useAddProducerSplit() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ statementId, lineItemId, data }: { statementId: string; lineItemId: string; data: AddProducerSplitRequest }) =>
      commissionsService.addProducerSplit(statementId, lineItemId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: commissionKeys.statementDetail(variables.statementId) });
    },
  });
}

/** Hook to update statement status. */
export function useUpdateStatementStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ statementId, data }: { statementId: string; data: UpdateStatementStatusRequest }) =>
      commissionsService.updateStatementStatus(statementId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: commissionKeys.statementDetail(variables.statementId) });
      queryClient.invalidateQueries({ queryKey: commissionKeys.statements() });
      queryClient.invalidateQueries({ queryKey: commissionKeys.statistics() });
    },
  });
}

// ── Statistics & Report Hooks ──

/** Hook to fetch commission statistics. */
export function useCommissionStatistics() {
  return useQuery({
    queryKey: commissionKeys.statistics(),
    queryFn: () => commissionsService.getStatistics(),
  });
}

/** Hook to fetch commission summary report. */
export function useCommissionSummaryReport(carrierId?: string, periodMonth?: number, periodYear?: number) {
  return useQuery({
    queryKey: commissionKeys.summaryReport(carrierId, periodMonth, periodYear),
    queryFn: () => commissionsService.getSummaryReport(carrierId, periodMonth, periodYear),
  });
}

/** Hook to fetch producer report. */
export function useProducerReport(producerId?: string, periodMonth?: number, periodYear?: number) {
  return useQuery({
    queryKey: commissionKeys.producerReport(producerId, periodMonth, periodYear),
    queryFn: () => commissionsService.getProducerReport(producerId, periodMonth, periodYear),
  });
}
