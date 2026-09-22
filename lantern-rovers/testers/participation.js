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

export function participationCsv(testers, dates) {
  const quote = value => {
    let text = String(value ?? '');
    // Display names are untrusted: prevent spreadsheet formula execution.
    if (/^[\s]*[=+@-]/.test(text) || /^[\t\r\n]/.test(text)) text = "'" + text;
    return `"${text.replaceAll('"', '""')}"`;
  };
  const rows = [['Tester', 'Application status', 'Activity sharing', ...dates]];
  for (const tester of testers) {
    rows.push([
      tester.alias,
      tester.status,
      tester.telemetryConsent ? 'On' : 'Off',
      ...dates.map(date => statusLabel(dayStatus(tester.days?.[date], tester.telemetryConsent))),
    ]);
  }
  return rows.map(row => row.map(quote).join(',')).join('\r\n');
}
