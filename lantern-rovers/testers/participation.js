export function utcDateKeys(now = new Date(), count = 14) {
  const end = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate()));
  return Array.from({ length: count }, (_, index) => {
    const day = new Date(end);
    day.setUTCDate(end.getUTCDate() - (count - index - 1));
    return day.toISOString().slice(0, 10);
  });
}

export function qualifies(d = {}) {
  return Number(d.activeSeconds) >= 120 &&
    (Number(d.kills) > 0 || Number(d.nodes) > 0 || Number(d.lanterns) > 0);
}

export function dayStatus(report, sharing = true) {
  if (!sharing) return 'sharing-off';
  if (!report) return 'unknown';
  return qualifies(report) ? 'qualifying' : 'activity';
}

export function statusLabel(status) {
  return ({
    qualifying: 'Qualifying reported play',
    activity: 'Reported activity below threshold',
    unknown: 'No report; play is unknown',
    'sharing-off': 'Activity sharing off',
  })[status] || 'No report; play is unknown';
}

export function cohortState(t = {}) {
  if (t.deleting === true) return 'deleting';
  if (t.status !== 'approved') return 'pending-review';
  if (t.accessProvisioned !== true) return 'pending-access';
  if (t.telemetryConsent !== true) return 'sharing-off';
  return 'reporting';
}

export function cohortStateLabel(state) {
  return ({ deleting: 'Deletion in progress', 'pending-review': 'Pending application review',
    'pending-access': 'Play access setup pending', 'sharing-off': 'Activity sharing off',
    reporting: 'Approved · access marked added · sharing on' })[state] || 'Status unknown';
}

export function receiptDate(report) {
  try {
    const value = report?.updatedAt;
    const date = typeof value?.toDate === 'function' ? value.toDate() :
      typeof value === 'string' ? new Date(value) : null;
    return date && Number.isFinite(date.getTime()) ? date : null;
  } catch { return null; }
}

export function latestReportReceipt(days = {}) {
  let latest = null;
  for (const report of Object.values(days)) {
    const date = receiptDate(report);
    if (date && (!latest || date > latest)) latest = date;
  }
  return latest;
}

export function reportDetail(report) {
  if (!report) return 'No report; play is unknown';
  const received = receiptDate(report);
  return `${Number(report.activeSeconds) || 0} active seconds · ${Number(report.kills) || 0} kills · ${Number(report.nodes) || 0} nodes · ${Number(report.lanterns) || 0} lantern waves · ${Number(report.completedRuns) || 0} completed runs · build ${report.version || 'unknown'} · received ${received ? received.toISOString() : 'time unknown'}`;
}

export function dailyAttention(testers, dateReports, date) {
  const counts = { reporting: 0, qualifying: 0, activity: 0, unknown: 0,
    'pending-review': 0, 'pending-access': 0, 'sharing-off': 0, deleting: 0 };
  for (const t of testers) {
    const state = cohortState(t);
    counts[state]++;
    if (state === 'reporting') counts[dayStatus(dateReports.get(t.uid)?.[date], true)]++;
  }
  return counts;
}

export function participationCsv(testers, dates) {
  const quote = value => {
    let text = String(value ?? '');
    // Display names are untrusted: prevent spreadsheet formula execution.
    if (/^[\s]*[=+@-]/.test(text) || /^[\t\r\n]/.test(text)) text = "'" + text;
    return `"${text.replaceAll('"', '""')}"`;
  };
  const rows = [['Tester', 'Application status', 'Activity sharing', 'Cohort status', 'Latest received UTC (queried window)', ...dates]];
  for (const tester of testers) {
    rows.push([
      tester.alias,
      tester.status,
      tester.telemetryConsent ? 'On' : 'Off',
      cohortStateLabel(cohortState(tester)),
      latestReportReceipt(tester.days)?.toISOString() || 'Unknown',
      ...dates.map(date => statusLabel(dayStatus(tester.days?.[date], tester.telemetryConsent))),
    ]);
  }
  return rows.map(row => row.map(quote).join(',')).join('\r\n');
}
