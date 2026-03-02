import { useState } from "react";
import { useTranslation } from "react-i18next";
import { formatCurrency } from "@/lib/format";
import { useCommissionSummaryReport, useProducerReport } from "@/hooks/use-commissions";

/**
 * Reports page with Commission Summary and Producer Report tabs.
 */
export function ReportsPage() {
  const { t, i18n } = useTranslation();
  const [activeTab, setActiveTab] = useState<"summary" | "producer">("summary");
  const [summaryYear, setSummaryYear] = useState<number>(new Date().getFullYear());
  const [producerYear, setProducerYear] = useState<number>(new Date().getFullYear());

  const { data: summaryData, isLoading: summaryLoading } = useCommissionSummaryReport(
    undefined, undefined, summaryYear
  );
  const { data: producerData, isLoading: producerLoading } = useProducerReport(
    undefined, undefined, producerYear
  );

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">{t('reports.title')}</h1>
        <p className="text-muted-foreground">{t('reports.subtitle')}</p>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 border-b">
        <button
          onClick={() => setActiveTab("summary")}
          className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
            activeTab === "summary"
              ? "border-primary text-primary"
              : "border-transparent text-muted-foreground hover:text-foreground"
          }`}
        >
          {t('reports.tabs.commissionSummary')}
        </button>
        <button
          onClick={() => setActiveTab("producer")}
          className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
            activeTab === "producer"
              ? "border-primary text-primary"
              : "border-transparent text-muted-foreground hover:text-foreground"
          }`}
        >
          {t('reports.tabs.producerReport')}
        </button>
      </div>

      {/* Commission Summary Tab */}
      {activeTab === "summary" && (
        <div className="space-y-4">
          <div className="flex items-center gap-4">
            <label className="text-sm font-medium">{t('reports.year')}:</label>
            <select
              value={summaryYear}
              onChange={(e) => setSummaryYear(Number(e.target.value))}
              className="rounded-md border px-3 py-1.5 text-sm"
            >
              {[2024, 2025, 2026, 2027].map((y) => (
                <option key={y} value={y}>{y}</option>
              ))}
            </select>
          </div>

          {summaryLoading ? (
            <div className="py-12 text-center text-muted-foreground">{t('reports.loading')}</div>
          ) : !summaryData?.length ? (
            <div className="rounded-lg border border-dashed p-12 text-center text-muted-foreground">
              {t('reports.noCommissionData', { year: summaryYear })}
            </div>
          ) : (
            <div className="rounded-md border">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b bg-muted/50">
                    <th className="px-4 py-3 text-left font-medium">{t('reports.columns.carrier')}</th>
                    <th className="px-4 py-3 text-left font-medium">{t('reports.columns.period')}</th>
                    <th className="px-4 py-3 text-right font-medium">{t('reports.columns.statements')}</th>
                    <th className="px-4 py-3 text-right font-medium">{t('reports.columns.totalPremium')}</th>
                    <th className="px-4 py-3 text-right font-medium">{t('reports.columns.totalCommission')}</th>
                    <th className="px-4 py-3 text-right font-medium">{t('reports.columns.totalPaid')}</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryData.map((entry, idx) => (
                    <tr key={idx} className="border-b last:border-0">
                      <td className="px-4 py-3 font-medium">{entry.carrierName}</td>
                      <td className="px-4 py-3">
                        {t(`commissions.months.${entry.periodMonth}`)} {entry.periodYear}
                      </td>
                      <td className="px-4 py-3 text-right">{entry.statementCount}</td>
                      <td className="px-4 py-3 text-right">{formatCurrency(entry.totalPremium, entry.currency, i18n.language)}</td>
                      <td className="px-4 py-3 text-right">{formatCurrency(entry.totalCommission, entry.currency, i18n.language)}</td>
                      <td className="px-4 py-3 text-right">{formatCurrency(entry.totalPaid, entry.currency, i18n.language)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr className="bg-muted/50 font-medium">
                    <td className="px-4 py-3" colSpan={2}>{t('reports.columns.totals')}</td>
                    <td className="px-4 py-3 text-right">{summaryData.reduce((s, e) => s + e.statementCount, 0)}</td>
                    <td className="px-4 py-3 text-right">{formatCurrency(summaryData.reduce((s, e) => s + e.totalPremium, 0), 'USD', i18n.language)}</td>
                    <td className="px-4 py-3 text-right">{formatCurrency(summaryData.reduce((s, e) => s + e.totalCommission, 0), 'USD', i18n.language)}</td>
                    <td className="px-4 py-3 text-right">{formatCurrency(summaryData.reduce((s, e) => s + e.totalPaid, 0), 'USD', i18n.language)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          )}
        </div>
      )}

      {/* Producer Report Tab */}
      {activeTab === "producer" && (
        <div className="space-y-4">
          <div className="flex items-center gap-4">
            <label className="text-sm font-medium">{t('reports.year')}:</label>
            <select
              value={producerYear}
              onChange={(e) => setProducerYear(Number(e.target.value))}
              className="rounded-md border px-3 py-1.5 text-sm"
            >
              {[2024, 2025, 2026, 2027].map((y) => (
                <option key={y} value={y}>{y}</option>
              ))}
            </select>
          </div>

          {producerLoading ? (
            <div className="py-12 text-center text-muted-foreground">{t('reports.loading')}</div>
          ) : !producerData?.length ? (
            <div className="rounded-lg border border-dashed p-12 text-center text-muted-foreground">
              {t('reports.noProducerData', { year: producerYear })}
            </div>
          ) : (
            <div className="rounded-md border">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b bg-muted/50">
                    <th className="px-4 py-3 text-left font-medium">{t('reports.columns.producer')}</th>
                    <th className="px-4 py-3 text-left font-medium">{t('reports.columns.period')}</th>
                    <th className="px-4 py-3 text-right font-medium">{t('reports.columns.lineItems')}</th>
                    <th className="px-4 py-3 text-right font-medium">{t('reports.columns.avgSplit')}</th>
                    <th className="px-4 py-3 text-right font-medium">{t('reports.columns.totalSplit')}</th>
                  </tr>
                </thead>
                <tbody>
                  {producerData.map((entry, idx) => (
                    <tr key={idx} className="border-b last:border-0">
                      <td className="px-4 py-3 font-medium">{entry.producerName}</td>
                      <td className="px-4 py-3">
                        {t(`commissions.months.${entry.periodMonth}`)} {entry.periodYear}
                      </td>
                      <td className="px-4 py-3 text-right">{entry.lineItemCount}</td>
                      <td className="px-4 py-3 text-right">{entry.averageSplitPercentage.toFixed(1)}%</td>
                      <td className="px-4 py-3 text-right">{formatCurrency(entry.totalSplitAmount, entry.currency, i18n.language)}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot>
                  <tr className="bg-muted/50 font-medium">
                    <td className="px-4 py-3" colSpan={2}>{t('reports.columns.totals')}</td>
                    <td className="px-4 py-3 text-right">{producerData.reduce((s, e) => s + e.lineItemCount, 0)}</td>
                    <td className="px-4 py-3 text-right">-</td>
                    <td className="px-4 py-3 text-right">{formatCurrency(producerData.reduce((s, e) => s + e.totalSplitAmount, 0), 'USD', i18n.language)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
