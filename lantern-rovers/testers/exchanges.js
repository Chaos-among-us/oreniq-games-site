export const EXCHANGE_STATUSES = ['planned', 'active', 'completed', 'archived'];

export function validDateKey(value) {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return false;
  const date = new Date(`${value}T00:00:00Z`);
  return !Number.isNaN(date.getTime()) && date.toISOString().slice(0, 10) === value;
}

export function addDays(dateKey, count) {
  if (!validDateKey(dateKey) || !Number.isInteger(count)) throw new Error('Enter a valid calendar date.');
  const date = new Date(`${dateKey}T00:00:00Z`);
  date.setUTCDate(date.getUTCDate() + count);
  return date.toISOString().slice(0, 10);
}

export function dateInZone(now, zone) {
  const parts = new Intl.DateTimeFormat('en-US', { timeZone: zone, year: 'numeric', month: '2-digit', day: '2-digit' }).formatToParts(now);
  const get = type => parts.find(part => part.type === type)?.value;
  return `${get('year')}-${get('month')}-${get('day')}`;
}

export function exchangeDates(exchange) {
  if (!validDateKey(exchange.startDate) || !Number.isInteger(exchange.duration) || exchange.duration < 1 || exchange.duration > 30) return [];
  return Array.from({ length: exchange.duration }, (_, index) => addDays(exchange.startDate, index));
}

export function scheduleSummary(exchange, days, now = new Date()) {
  const dates = exchangeDates(exchange);
  if (!dates.length) return { today: '', due: false, remaining: 0, total: 0, completed: 0, end: '' };
  const today = dateInZone(now, exchange.timeZone);
  const remaining = dates.filter(date => !days[date]?.played).length;
  const overdue = dates.filter(date => date < today && !days[date]?.played).length;
  return { today, due: exchange.status === 'active' && dates.includes(today) && !days[today]?.played, remaining, overdue, total: dates.length, completed: dates.filter(date => days[date]?.played).length, end: dates.at(-1), cleanupDue: addDays(dates.at(-1), 90) <= today };
}

export function safeExchangeUrl(value) {
  const raw = String(value || '').trim();
  if (!raw) return '';
  if (raw.length > 500) throw new Error('Links must be 500 characters or less.');
  let url;
  try { url = new URL(raw); } catch { throw new Error('Enter a complete https:// link.'); }
  if (url.protocol !== 'https:' || !url.hostname || url.username || url.password) throw new Error('Use a complete https:// link without embedded credentials.');
  return url.href;
}

export function validateExchange(input) {
  const text = (value, max, label, required = false) => {
    const clean = String(value || '').trim();
    if (clean.length > max || (required && !clean)) throw new Error(`${label} must be ${required ? `1–${max}` : `at most ${max}`} characters.`);
    return clean;
  };
  const appName = text(input.appName, 100, 'App name', true);
  const developer = text(input.developer, 80, 'Developer');
  const testerUid = text(input.testerUid, 128, 'Linked tester UID');
  const instructions = text(input.instructions, 2000, 'Testing instructions');
  const startDate = String(input.startDate || '');
  if (!validDateKey(startDate)) throw new Error('Choose a valid start date.');
  const timeZone = text(input.timeZone, 80, 'Time zone', true);
  try { new Intl.DateTimeFormat('en-US', { timeZone }); } catch { throw new Error('Choose a valid time zone.'); }
  const duration = Number(input.duration);
  if (!Number.isInteger(duration) || duration < 1 || duration > 30) throw new Error('Duration must be 1 to 30 days.');
  const status = String(input.status || '');
  if (!EXCHANGE_STATUSES.includes(status)) throw new Error('Choose a valid exchange status.');
  return { appName, developer, testerUid, playUrl: safeExchangeUrl(input.playUrl), groupUrl: safeExchangeUrl(input.groupUrl), contactUrl: safeExchangeUrl(input.contactUrl), instructions, startDate, timeZone, duration, status };
}

export function partnerReport(testers, dateReports, testerUid, dates) {
  if (!testerUid) return 'No Lantern tester linked.';
  const tester = testers.find(item => item.uid === testerUid);
  if (!tester) return 'Linked tester record missing or deleted.';
  if (tester.deleting === true) return 'Linked tester deletion in progress.';
  if (tester.telemetryConsent !== true) return 'Activity sharing off.';
  const reports = dateReports.get(testerUid) || {};
  const latest = [...dates].reverse().find(date => reports[date]);
  return latest ? `Latest reported Lantern activity: ${latest} UTC (daily summary, not exact last play).` : 'No report in the latest 14 UTC days; play is unknown.';
}
