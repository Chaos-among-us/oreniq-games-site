// Referral codes identify the inviter without exposing an email or Firebase UID.
export async function referralCode(uid) {
  if (!uid) return '';
  const bytes = await crypto.subtle.digest('SHA-256', new TextEncoder().encode('lantern-rovers-referral-v1:' + uid));
  return Array.from(new Uint8Array(bytes)).map(b => b.toString(16).padStart(2, '0')).join('').slice(0,32);
}
export function referralBadge(credits) { return ['No badge yet','Trail Friend','Grove Guide','Camp Host'][Math.max(0,Math.min(3,Number(credits)||0))]; }
export function referralAward(inviter, recruit, expectedCode) {
  if (!inviter || !recruit || inviter.uid === recruit.uid) throw new Error('A referral must be a different person.');
  if (inviter.deleting || recruit.deleting || inviter.status !== 'approved' || recruit.status !== 'approved' || recruit.accessProvisioned !== true) throw new Error('Both accounts must be approved; the new tester also needs Play access.');
  if (!/^[0-9a-f]{32}$/.test(expectedCode) || recruit.referredBy !== expectedCode) throw new Error('The application does not match this invitation.');
  if (recruit.referralRewarded === true) throw new Error('This referral has already been credited.');
  const count = inviter.referralCredits ?? 0;
  if (!Number.isInteger(count) || count < 0 || count >= 3) throw new Error('The inviter has already earned all three badges.');
  return {referralCredits:count+1};
}
