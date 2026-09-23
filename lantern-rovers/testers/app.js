import { initializeApp } from 'https://www.gstatic.com/firebasejs/11.10.0/firebase-app.js';
import { connectAuthEmulator, createUserWithEmailAndPassword, sendEmailVerification, sendPasswordResetEmail, signInWithEmailAndPassword, getAuth, EmailAuthProvider, reauthenticateWithCredential, signOut, onAuthStateChanged, deleteUser } from 'https://www.gstatic.com/firebasejs/11.10.0/firebase-auth.js';
import { connectFirestoreEmulator, getFirestore, doc, getDoc, setDoc, updateDoc, deleteDoc, collection, query, orderBy, getDocs, limit, serverTimestamp, writeBatch, runTransaction, onSnapshot, where, documentId } from 'https://www.gstatic.com/firebasejs/11.10.0/firebase-firestore.js';
import { firebaseConfig, useEmulators } from './firebase-config.js';
import { referralCode, referralBadge, referralAward } from './referrals.js?v=0118';
import { utcDateKeys, qualifies, dayStatus, statusLabel, participationCsv } from './participation.js?v=0119';
import { dateInZone, exchangeDates, scheduleSummary, safeExchangeUrl, validateExchange, partnerReport } from './exchanges.js?v=0120';

const $ = id => document.getElementById(id);
const show = id => $(id).classList.remove('hidden');
const hide = id => $(id).classList.add('hidden');
const message = (id, text, error=false) => { const el=$(id); el.textContent=text; el.style.color=error?'#9a4139':'#356344'; };
const INVITE_URL = 'https://chaos-among-us.github.io/oreniq-games-site/lantern-rovers/testers/?join=1';
const configured = firebaseConfig.apiKey && !firebaseConfig.apiKey.startsWith('REPLACE_') && firebaseConfig.projectId && !firebaseConfig.projectId.startsWith('REPLACE_');
let ownInviteUrl=INVITE_URL;
let app, auth, db, currentUser, profile, selectedUid='', unsubscribeMessages=null, mode='signin', authEpoch=0, ownerMessagesRequest=0;
let participationView={testers:[],dateReports:new Map(),dates:[]}, ownerLoadPromise=null, ownerLoadSession='', ownerLastLoadedAt=null;
let exchangeView={items:[],days:new Map(),openId:'',editId:'',busy:false},exchangeLoadRequest=0;

if(new URLSearchParams(location.search).get('join')==='1'){
  $('authHeading').textContent='You’re invited to test Lantern Rovers.';
  $('authIntro').textContent='Apply for the Android test and share feedback if you choose.';
  show('inviteDetails');
}

const referralFromLink=new URLSearchParams(location.hash.slice(1)).get('ref')||'';
if(/^[0-9a-f]{32}$/.test(referralFromLink)) sessionStorage.setItem('lanternReferral',referralFromLink);
$('referralCode').value=sessionStorage.getItem('lanternReferral')||'';

function emptyAll(){['auth','enroll','tester','owner','setup','resendVerification','inviteCard'].forEach(hide);}
function clearPrivate(){ownInviteUrl=INVITE_URL;$('inviteLink').value=ownInviteUrl;for(const key of Object.keys(topicViews))topicViews[key]={items:[],id:'',subject:''};participationView={testers:[],dateReports:new Map(),dates:[]};ownerLastLoadedAt=null;$('refreshOwnerDashboard').disabled=true;$('participationExport').disabled=true;$('ownerRefreshStatus').textContent='';$('ownerRefreshStatus').style.color='';profile=null;selectedUid='';ownerMessagesRequest++;$('threadTitle').textContent='Select a tester';$('ownerTopic').replaceChildren();$('replyText').value='';['messages','ownerMessages','roster','activityRows'].forEach(id=>$(id).replaceChildren());hide('replyForm');clearExchangePrivate();}
function sameSession(epoch,uid){return epoch===authEpoch&&auth?.currentUser?.uid===uid;}
function errorText(e){
  const codes={ 'auth/email-already-in-use':'That email already has an account. Sign in instead.', 'auth/invalid-credential':'Email or password was not recognized.', 'auth/weak-password':'Choose a password with at least 8 characters.', 'auth/too-many-requests':'Too many attempts. Please wait and try again.', 'auth/network-request-failed':'Connection failed. Check your internet and retry.', 'permission-denied':'This action is not allowed by the current account or project rules.' };
  return codes[e.code] || (e.code?.includes('permission-denied')?'Access denied by project rules.':e.message || 'Something went wrong. Please retry.');
}
function esc(s){return String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));}

if (!configured) { show('setup'); show('auth'); $('authSubmit').disabled=true; $('authMessage').textContent='Authentication is unavailable until Firebase project values are supplied.'; }
else {
  app=initializeApp(firebaseConfig); auth=getAuth(app); db=getFirestore(app);
  if(useEmulators){connectAuthEmulator(auth,'http://127.0.0.1:9099');connectFirestoreEmulator(db,'127.0.0.1',8080);}
  onAuthStateChanged(auth, async user=>{
    const epoch=++authEpoch;currentUser=user; emptyAll(); if(unsubscribeMessages){unsubscribeMessages();unsubscribeMessages=null;}clearPrivate();
    $('identity').textContent=user?.email || '';
    if(!user){show('auth');return;}
    if(!user.emailVerified){show('auth');show('resendVerification');message('authMessage','Verify your email using the link we sent, then return and sign in again.');return;}
    try{
      const ownerSnap=await getDoc(doc(db,'owners',user.uid));
      if(!sameSession(epoch,user.uid))return;
      if(ownerSnap.exists()){show('owner');show('inviteCard');await loadOwner();return;}
      const ref=doc(db,'testers',user.uid), snap=await getDoc(ref);
      if(!sameSession(epoch,user.uid))return;
      const marker=await getDoc(doc(db,'deletedAccounts',user.uid));
      if(!sameSession(epoch,user.uid))return;
      if(!snap.exists()){
        show('enroll');
        if(marker.exists()){hide('enrollForm');message('enrollMessage','Account deletion is in progress. Use Delete my sign-in account instead to finish.');}
        else{show('enrollForm');message('enrollMessage','');}
        return;
      }
      profile={...snap.data(),deleting:snap.data().deleting===true||marker.exists()}; show('tester'); await loadTester();
    }catch(e){if(sameSession(epoch,user.uid)){emptyAll();clearPrivate();show('auth');message('authMessage',errorText(e),true);}}
  });
}

document.querySelectorAll('.tab').forEach(b=>b.addEventListener('click',()=>{mode=b.dataset.mode;document.querySelectorAll('.tab').forEach(x=>x.classList.toggle('active',x===b));$('authSubmit').textContent=mode==='signup'?'Create account':'Sign in';$('password').autocomplete=mode==='signup'?'new-password':'current-password';$('authAdultRow').classList.toggle('hidden',mode!=='signup');$('authSignupNotice').classList.toggle('hidden',mode!=='signup');$('authAdult').required=mode==='signup';message('authMessage','');}));
if(new URLSearchParams(location.search).get('join')==='1')document.querySelector('[data-mode="signup"]').click();
$('authForm').addEventListener('submit',async e=>{e.preventDefault();if(!auth)return;const email=$('email').value.trim(),password=$('password').value;try{if(mode==='signup'){if(!$('authAdult').checked){message('authMessage','Connected tester accounts are for adults 18 and older.',true);return;}const cred=await createUserWithEmailAndPassword(auth,email,password);await sendEmailVerification(cred.user);await signOut(auth);message('authMessage','Check your email for a verification link, then return here to sign in.');}else{await signInWithEmailAndPassword(auth,email,password);}}catch(err){message('authMessage',errorText(err),true);}});
$('reset').addEventListener('click',async()=>{if(!auth)return;const email=$('email').value.trim();if(!email){message('authMessage','Enter your email above first.');return;}try{await sendPasswordResetEmail(auth,email);message('authMessage','If this address has an account, a reset link is on its way.');}catch(e){message('authMessage',errorText(e),true);}});
$('resendVerification').addEventListener('click',async()=>{if(!currentUser)return;try{await sendEmailVerification(currentUser);message('authMessage','Verification email sent. Check your inbox and spam folder.');}catch(e){message('authMessage',errorText(e),true);}});

$('enrollForm').addEventListener('submit',async e=>{e.preventDefault();try{const uid=currentUser.uid;const alias=$('alias').value.trim();await setDoc(doc(db,'testers',uid),{email:currentUser.email,alias,status:'pending',adultConfirmed:$('adult').checked,telemetryConsent:$('telemetry').checked,referredBy:$('referralCode').value.trim().toLowerCase(),referralCredits:0,referralRewarded:false,privacyVersion:'2026-09-22-v2',createdAt:serverTimestamp(),accessProvisioned:false,deleting:false});profile={alias,status:'pending',telemetryConsent:$('telemetry').checked,deleting:false};emptyAll();show('tester');sessionStorage.removeItem('lanternReferral');await loadTester();}catch(err){message('enrollMessage',errorText(err),true);}});

async function loadTester(){
  const epoch=authEpoch,uid=currentUser.uid;
  hide('pendingNote');
  if(profile.deleting===true){hide('testerStatusTools');hide('inviteCard');$('hello').textContent=`Welcome, ${profile.alias}`;$('statusText').textContent='Account deletion is in progress. Retry to finish removing your data and sign-in account.';$('statusBadge').textContent='Deleting';$('messageForm').querySelector('button').disabled=true;$('consentToggle').disabled=true;$('activitySummary').textContent='Activity sharing is stopped.';return;}
  show('testerStatusTools');show('inviteCard');
  const code=await referralCode(uid);if(!sameSession(epoch,uid))return;ownInviteUrl=profile.status==='approved'?INVITE_URL+'#ref='+code:INVITE_URL;$('inviteLink').value=ownInviteUrl;
  $('referralBadge').textContent=referralBadge(profile.referralCredits)+' · '+(profile.referralCredits||0)+'/3 verified referrals';
  $('consentToggle').disabled=false;
  $('hello').textContent=`Welcome, ${profile.alias}`;$('statusText').textContent=profile.status==='approved'?(profile.accessProvisioned===true?'Your application is approved, and the owner marked that they added your email to Play testing. Check Play for availability; this hub cannot confirm access or guarantee installation.':'Your tester application is approved. The owner handles any Google Play access setup separately.'):'Your application is pending owner review.';
  $('statusBadge').textContent=profile.status==='approved'?'Approved':'Pending';$('statusBadge').classList.toggle('ok',profile.status==='approved');if(profile.status==='pending')show('pendingNote');
  $('consentToggle').checked=profile.telemetryConsent===true;$('messageText').disabled=profile.status!=='approved';$('messageForm').querySelector('button').disabled=profile.status!=='approved';
  $('messageNotice').textContent=profile.status==='approved'?'Only you and the test owner can read this conversation.':'The inbox becomes available after approval. For application questions, use the support email on the privacy page.';
  $('activitySummary').textContent=profile.telemetryConsent?'Daily summaries are enabled.':'Daily summaries are off; no activity summary should be sent.';
  if(profile.status==='approved'){
    unsubscribeMessages=onSnapshot(query(collection(db,'testers',uid,'messages'),orderBy('createdAt')),snap=>{if(sameSession(epoch,uid))selectTopics('messages',snap.docs.map(d=>d.data()));},err=>{if(sameSession(epoch,uid))message('messageNotice',errorText(err),true);});
  } else {$('messages').innerHTML='<p class="muted small">No conversation yet.</p>';}
  if(profile.telemetryConsent){const days=await getDocs(query(collection(db,'testers',uid,'days'),orderBy('updatedAt','desc'),limit(14)));if(!sameSession(epoch,uid))return;const qualified=days.docs.map(d=>d.data()).filter(qualifies).length;$('activitySummary').textContent+=` ${qualified} qualifying reported day(s) in the most recent ${days.size} summaries.`;}
}
async function refreshTesterStatus(){
  if(!currentUser)return;
  const epoch=authEpoch,uid=currentUser.uid;
  $('refreshTesterStatus').disabled=true;message('refreshStatusMessage','Checking for an update…');
  try{
    const snap=await getDoc(doc(db,'testers',uid));
    if(!sameSession(epoch,uid))return;
    if(!snap.exists()){message('refreshStatusMessage','No active application was found. Sign out and sign in again if this seems wrong.',true);return;}
    const data=snap.data();profile={...data,deleting:data.deleting===true};
    await loadTester();message('refreshStatusMessage','Status refreshed just now.');
  }catch(e){message('refreshStatusMessage',errorText(e),true);}
  finally{$('refreshTesterStatus').disabled=false;}
}
$('refreshTesterStatus').addEventListener('click',refreshTesterStatus);
const topicViews={messages:{items:[],id:'',subject:''},ownerMessages:{items:[],id:'',subject:''}};
function topicFields(target){const v=topicViews[target];return v.id?{threadId:v.id,subject:v.subject}:{};}
function selectTopics(target,items){
  const v=topicViews[target];v.items=items;
  const select=$(target==='messages'?'testerTopic':'ownerTopic');
  const topics=new Map([['','General feedback']]);
  for(const m of items) if(m.threadId) topics.set(m.threadId,m.subject||'Feedback');
  if(v.id&&!topics.has(v.id))topics.set(v.id,v.subject);
  select.replaceChildren();for(const [id,subject] of topics){const o=document.createElement('option');o.value=id;o.textContent=subject;select.append(o);}
  select.value=v.id;v.subject=topics.get(v.id)||'';
  renderMessages(target,items.filter(m=>(m.threadId||'')===v.id));
}
$('testerTopic').addEventListener('change',()=>{const v=topicViews.messages;v.id=$('testerTopic').value;v.subject=$('testerTopic').selectedOptions[0].textContent;selectTopics('messages',v.items);});
$('ownerTopic').addEventListener('change',()=>{const v=topicViews.ownerMessages;v.id=$('ownerTopic').value;v.subject=$('ownerTopic').selectedOptions[0].textContent;selectTopics('ownerMessages',v.items);});
$('newTopic').addEventListener('click',()=>{const subject=prompt('Conversation topic (up to 80 characters)');if(subject===null)return;const clean=subject.trim();if(!clean||clean.length>80){message('messageNotice','Enter a topic of 1 to 80 characters.',true);return;}const v=topicViews.messages;v.id=crypto.randomUUID().replaceAll('-','');v.subject=clean;selectTopics('messages',v.items);$('messageText').focus();});
function renderMessages(target,items,uid){const box=$(target);box.innerHTML='';if(!items.length){box.innerHTML='<p class="muted small">No messages yet.</p>';return;}for(const m of items){const div=document.createElement('div');div.className='bubble '+((m.role===(target==='ownerMessages'?'studio':'tester'))?'mine':'');const when=m.createdAt?.toDate?.().toLocaleString()||'Sending…';div.textContent=m.text;const meta=document.createElement('small');meta.textContent=`${m.role==='tester'?(target==='ownerMessages'?'Tester':'You'):'Oreniq games'} · ${when}${m.build?' · '+m.build:''}`;div.append(meta);box.append(div);}box.scrollTop=box.scrollHeight;}
$('messageForm').addEventListener('submit',async e=>{e.preventDefault();if(!profile||profile.status!=='approved')return;const text=$('messageText').value.trim();if(!text)return;try{const id=crypto.randomUUID();await setDoc(doc(db,'testers',currentUser.uid,'messages',id),{role:'tester',text,...topicFields('messages'),createdAt:serverTimestamp(),build:$('build').value.trim().slice(0,64),clientId:id});$('messageText').value='';message('messageNotice','Message sent securely.');}catch(err){message('messageNotice',errorText(err),true);}});
$('consentToggle').addEventListener('change',async()=>{try{await updateDoc(doc(db,'testers',currentUser.uid),{telemetryConsent:$('consentToggle').checked});profile.telemetryConsent=$('consentToggle').checked;$('activitySummary').textContent=profile.telemetryConsent?'Daily summaries are enabled.':'Daily summaries are off; no activity summary should be sent.';}catch(e){$('consentToggle').checked=!$('consentToggle').checked;message('activitySummary',errorText(e),true);}});

function formatOwnerRefreshTime(date){return date?.toLocaleString()||'';}
function setOwnerRefreshStatus(text,error=false){const el=$('ownerRefreshStatus');el.textContent=text;el.style.color=error?'#9a4139':'';}
async function loadOwner(){
  const epoch=authEpoch,uid=currentUser.uid,sessionKey=`${epoch}:${uid}`;
  if(ownerLoadPromise&&ownerLoadSession===sessionKey)return ownerLoadPromise;
  const run=(async()=>{
    const refresh=$('refreshOwnerDashboard');refresh.disabled=true;
    setOwnerRefreshStatus(ownerLastLoadedAt?`Refreshing… Last successful refresh: ${formatOwnerRefreshTime(ownerLastLoadedAt)}`:'Loading dashboard…');
    try{
      await loadOwnerData(epoch,uid);
      if(!sameSession(epoch,uid))return false;
      try{await loadExchanges(epoch,uid);}catch(e){if(sameSession(epoch,uid))exchangeMessage(`Exchange refresh failed: ${errorText(e)}. Retry with Refresh dashboard.`,true);throw e;}
      if(!sameSession(epoch,uid))return false;
      ownerLastLoadedAt=new Date();$('participationExport').disabled=false;
      setOwnerRefreshStatus(`Last successful refresh: ${formatOwnerRefreshTime(ownerLastLoadedAt)}`);
      return true;
    }catch(e){
      if(sameSession(epoch,uid))setOwnerRefreshStatus(`Refresh failed: ${errorText(e)}. ${ownerLastLoadedAt?`Last complete refresh: ${formatOwnerRefreshTime(ownerLastLoadedAt)}; current sections may be incomplete.`:'No successful refresh yet.'}`,true);
      return false;
    }finally{
      if(ownerLoadPromise===run){ownerLoadPromise=null;ownerLoadSession='';}
      if(sameSession(epoch,uid))refresh.disabled=false;
    }
  })();
  ownerLoadPromise=run;ownerLoadSession=sessionKey;return run;
}
async function loadOwnerData(epoch,uid){
  const snap=await getDocs(query(collection(db,'testers'),orderBy('createdAt','desc')));const testers=snap.docs.map(d=>({uid:d.id,...d.data()}));
  if(!sameSession(epoch,uid))return;
  const selected=testers.find(t=>t.uid===selectedUid);
  if(selectedUid&&(!selected||selected.deleting===true||selected.status!=='approved'))clearSelectedTester();
  $('pendingCount').textContent=testers.filter(t=>t.status==='pending'&&t.deleting!==true).length;$('approvedCount').textContent=testers.filter(t=>isPlayEligible(t)).length;
  const roster=$('roster');roster.innerHTML='';for(const t of testers){const row=document.createElement('div');row.className='personhead';const b=document.createElement('button');b.className='person'+(selectedUid===t.uid?' selected':'');b.dataset.uid=t.uid;const accessLabel=t.status==='approved'?(t.accessProvisioned===true?'Play access added':'Play access setup still needed'):'';b.innerHTML=`<strong>${esc(t.alias)}</strong><small>${esc(t.email)} · ${esc(t.status)} · ${esc(accessLabel|| (t.telemetryConsent?'activity on':'activity off'))}${t.deleting===true?' · deletion in progress':''}</small>`;b.onclick=()=>{selectTester(t).catch(e=>message('globalMessage',errorText(e),true));};row.append(b);
    if(t.deleting!==true){const action=document.createElement('button');action.className='secondary';action.textContent=t.status==='approved'?'Revoke':'Approve';action.onclick=async()=>{try{const update={status:t.status==='approved'?'pending':'approved'};if(t.status==='approved')update.accessProvisioned=false;await updateDoc(doc(db,'testers',t.uid),update);await loadOwner();}catch(e){message('globalMessage',errorText(e),true);}};row.append(action);}
    if(t.status==='approved'&&t.deleting!==true){const access=document.createElement('button');access.className=t.accessProvisioned===true?'link':'secondary';access.textContent=t.accessProvisioned===true?'Reset Play access status':'Confirm Play access added';access.title=t.accessProvisioned===true?'Reset the hub status only; this does not remove Play membership':'Confirm you manually added this email to Play testing';access.onclick=async()=>{const value=t.accessProvisioned!==true;if(value&&!confirm('Confirm only after you have manually added this email to Play testing. This hub status does not grant Play access.'))return;if(!value&&!confirm('Reset the hub status only? This does not remove the tester from Google Play.'))return;try{await updateDoc(doc(db,'testers',t.uid),{accessProvisioned:value});await loadOwner();}catch(e){message('globalMessage',errorText(e),true);}};row.append(access);}
    if(t.referredBy){const info=document.createElement('small');info.className='referral-note';info.textContent=t.referralRewarded?'Referral credited':'Invited tester · awaiting verification';row.append(info);
      if(!t.referralRewarded&&t.status==='approved'&&t.accessProvisioned&&!t.deleting){const reward=document.createElement('button');reward.className='secondary';reward.textContent='Verify referral';reward.onclick=()=>creditReferral(t.uid,testers);row.append(reward);}}
    roster.append(row);}
  if(selectedUid)await selectTester(selected);
  if(!sameSession(epoch,uid))return;
  const dates=utcDateKeys();const dateReports=new Map();let qualifiedTotal=0;
  for(const t of testers){const reports={};if(t.telemetryConsent){const ds=await getDocs(query(collection(db,'testers',t.uid,'days'),where(documentId(),'>=',dates[0]),where(documentId(),'<=',dates.at(-1))));if(!sameSession(epoch,uid))return;for(const d of ds.docs)reports[d.id]=d.data();}dateReports.set(t.uid,reports);qualifiedTotal+=dates.filter(date=>dayStatus(reports[date],t.telemetryConsent)==='qualifying').length;}
  participationView={testers,dateReports,dates};renderParticipation(testers,dateReports,dates);
  $('qualifiedCount').textContent=qualifiedTotal;
}
$('refreshOwnerDashboard').addEventListener('click',()=>{if(!exchangeView.busy)loadOwner();});
function renderParticipation(testers,dateReports,dates){
  const root=$('activityRows');root.replaceChildren();
  const grid=document.createElement('div');grid.className='participation-grid';grid.style.setProperty('--day-count',dates.length);
  const corner=document.createElement('div');corner.className='participation-head tester-head';corner.textContent='Tester';grid.append(corner);
  for(const date of dates){const head=document.createElement('div');head.className='participation-head';head.textContent=date.slice(5).replace('-','/');head.title=`${date} UTC`;grid.append(head);}
  const filter=$('participationFilter').value, search=$('participationSearch').value.trim().toLocaleLowerCase();
  for(const t of testers){const days=dateReports.get(t.uid)||{};const statuses=dates.map(date=>dayStatus(days[date],t.telemetryConsent));
    if(search&&!`${t.alias} ${t.email}`.toLocaleLowerCase().includes(search))continue;
    if(filter!=='all'&&!statuses.includes(filter))continue;
    const name=document.createElement('div');name.className='participation-name';name.textContent=t.alias;name.title=`${t.alias} · ${t.status} · Sharing ${t.telemetryConsent?'on':'off'}`;grid.append(name);
    dates.forEach((date,index)=>{const status=statuses[index];const cell=document.createElement('div');cell.className=`day-cell ${status}`;cell.textContent=({qualifying:'✓',activity:'•',unknown:'—','sharing-off':'Off'})[status];cell.title=`${date} UTC · ${statusLabel(status)}`;cell.setAttribute('aria-label',cell.title);grid.append(cell);});
  }
  root.append(grid);
}
function refreshParticipationView(){if(currentUser&&$('owner').classList.contains('hidden')===false)renderParticipation(participationView.testers,participationView.dateReports,participationView.dates);}
$('participationFilter').addEventListener('change',refreshParticipationView);$('participationSearch').addEventListener('input',()=>{clearTimeout(window.participationSearchTimer);window.participationSearchTimer=setTimeout(refreshParticipationView,150);});
$('participationExport').addEventListener('click',()=>{try{const out=participationView.testers.map(t=>({...t,days:participationView.dateReports.get(t.uid)||{}}));download('tester-participation-last-14-days.csv',participationCsv(out,participationView.dates),'text/csv;charset=utf-8');}catch(e){message('globalMessage',errorText(e),true);}});
function clearSelectedTester(){selectedUid='';ownerMessagesRequest++;topicViews.ownerMessages={items:[],id:'',subject:''};$('threadTitle').textContent='Select a tester';$('ownerTopic').replaceChildren();$('ownerMessages').replaceChildren();$('replyText').value='';hide('replyForm');}
async function selectTester(t){
  const epoch=authEpoch,uid=currentUser.uid,request=++ownerMessagesRequest;
  if(selectedUid!==t.uid){topicViews.ownerMessages={items:[],id:'',subject:''};$('replyText').value='';$('ownerMessages').replaceChildren();}
  selectedUid=t.uid;$('threadTitle').textContent=`${t.alias} · ${t.status}`;show('replyForm');
  let msgs;
  try{msgs=await getDocs(query(collection(db,'testers',t.uid,'messages'),orderBy('createdAt')));}
  catch(e){if(!sameSession(epoch,uid)||selectedUid!==t.uid||request!==ownerMessagesRequest)return;throw e;}
  if(!sameSession(epoch,uid)||selectedUid!==t.uid||request!==ownerMessagesRequest)return;
  selectTopics('ownerMessages',msgs.docs.map(d=>d.data()));
  document.querySelectorAll('.person').forEach(el=>el.classList.toggle('selected',el.dataset.uid===t.uid));
}
$('replyForm').addEventListener('submit',async e=>{e.preventDefault();if(!selectedUid||!currentUser)return;const epoch=authEpoch,uid=currentUser.uid,targetUid=selectedUid,draft=$('replyText').value,text=draft.trim();if(!text)return;try{const id=crypto.randomUUID();await setDoc(doc(db,'testers',targetUid,'messages',id),{role:'studio',text,...topicFields('ownerMessages'),createdAt:serverTimestamp(),build:$('replyBuild').value.trim().slice(0,64),clientId:id});if(!sameSession(epoch,uid)||selectedUid!==targetUid)return;if($('replyText').value===draft)$('replyText').value='';await selectTester({uid:targetUid,alias:$('threadTitle').textContent.split(' · ')[0],status:$('threadTitle').textContent.split(' · ')[1]||''});}catch(err){if(sameSession(epoch,uid)&&selectedUid===targetUid)message('globalMessage',errorText(err),true);}});
async function creditReferral(recruitUid,testers){
  try{
    const recruit=testers.find(t=>t.uid===recruitUid);let inviter=null;
    for(const t of testers){if(await referralCode(t.uid)===recruit.referredBy){inviter=t;break;}}
    if(!inviter)throw new Error('Inviter not found. No reward was granted.');
    const code=await referralCode(inviter.uid);referralAward(inviter,recruit,code);
    if(!confirm('Award '+inviter.alias+' a tester badge? Confirm this is a genuine different person, has Play access and has tried an expedition. You can confirm play through a private conversation; activity sharing is not required. Never require a rating, review, purchase or daily streak.'))return;
    await runTransaction(db,async tx=>{
      const ir=doc(db,'testers',inviter.uid),rr=doc(db,'testers',recruitUid);
      const [i,r,im,rm]=await Promise.all([tx.get(ir),tx.get(rr),tx.get(doc(db,'deletedAccounts',inviter.uid)),tx.get(doc(db,'deletedAccounts',recruitUid))]);
      if(!i.exists()||!r.exists()||im.exists()||rm.exists())throw new Error('Account unavailable or being deleted.');
      const award=referralAward({uid:i.id,...i.data()},{uid:r.id,...r.data()},code);
      tx.update(ir,award);tx.update(rr,{referralRewarded:true});
    });message('globalMessage','Referral credited once. The inviter’s badge updates when their account refreshes.');await loadOwner();
  }catch(e){message('globalMessage',errorText(e),true);}
}
function isPlayEligible(t){return t.status==='approved'&&t.deleting!==true&&Boolean(t.email);}
function isAwaitingPlayAccess(t){return isPlayEligible(t)&&t.accessProvisioned!==true;}
async function getOwnerTesters(){const snap=await getDocs(collection(db,'testers'));return snap.docs.map(d=>({uid:d.id,...d.data()}));}
$('playExport').addEventListener('click',async()=>{try{const testers=await getOwnerTesters();const emails=testers.filter(isPlayEligible).map(t=>t.email);download('tester-play-email-list.csv',emails.join('\r\n'),'text/csv;charset=utf-8');}catch(e){message('globalMessage',errorText(e),true);}});
async function copyText(text){if(navigator.clipboard?.writeText){await navigator.clipboard.writeText(text);return;}const input=document.createElement('textarea');input.value=text;input.style.position='fixed';input.style.opacity='0';document.body.append(input);input.select();const copied=document.execCommand('copy');input.remove();if(!copied)throw new Error('Clipboard access is unavailable.');}
async function copyInvite(){await copyText(ownInviteUrl);message('inviteMessage','Invite link copied.');}
$('copyInvite').addEventListener('click',async()=>{try{await copyInvite();}catch(e){message('inviteMessage',errorText(e),true);}});
$('shareInvite').addEventListener('click',async()=>{try{if(navigator.share)await navigator.share({title:'Lantern Rovers tester application',text:'Apply for the adult Android test. Use your Google Play email; feedback is voluntary.'+(ownInviteUrl!==INVITE_URL?' I may earn a cosmetic tester badge after you join through my invitation and the owner verifies your first expedition.':''),url:ownInviteUrl});else await copyInvite();message('inviteMessage','Invite link ready to share.');}catch(e){if(e.name!=='AbortError')message('inviteMessage',errorText(e),true);}});
$('copyAwaitingEmails').addEventListener('click',async()=>{try{const testers=await getOwnerTesters();const emails=testers.filter(isAwaitingPlayAccess).map(t=>t.email);if(!emails.length){message('awaitingMessage','No approved testers are waiting for access.');return;}await copyText(emails.join('\n'));message('awaitingMessage',`${emails.length} approved email(s) copied for manual Play setup.`);}catch(e){message('awaitingMessage',errorText(e),true);}});
function csv(v){return '"'+String(v??'').replaceAll('"','""')+'"';}
function download(name,text,type='text/plain;charset=utf-8'){const a=document.createElement('a');a.href=URL.createObjectURL(new Blob([text],{type}));a.download=name;a.click();URL.revokeObjectURL(a.href);}
$('exportMine').addEventListener('click',async()=>{try{const [m,d]=await Promise.all([getDocs(collection(db,'testers',currentUser.uid,'messages')),getDocs(collection(db,'testers',currentUser.uid,'days'))]);const out={profile:{alias:profile.alias,status:profile.status,telemetryConsent:profile.telemetryConsent,referredBy:profile.referredBy||'',referralCredits:profile.referralCredits||0,referralRewarded:profile.referralRewarded===true},messages:m.docs.map(x=>({id:x.id,...x.data()})),days:d.docs.map(x=>({date:x.id,...x.data()}))};download('my-tester-data.json',JSON.stringify(out,null,2));}catch(e){message('globalMessage',errorText(e),true);}});
async function deleteCollection(path){while(true){const snap=await getDocs(query(collection(db,path),limit(250)));if(snap.empty)break;const batch=writeBatch(db);snap.docs.forEach(d=>batch.delete(d.ref));await batch.commit();}}
async function ensureDeletionMarker(uid){
  const marker=doc(db,'deletedAccounts',uid);
  if((await getDoc(marker)).exists())return;
  try{await setDoc(marker,{deletedAt:serverTimestamp()});}
  catch(e){if(!(await getDoc(marker)).exists())throw e;}
}
async function deleteMyAccount(){
  if(!currentUser||!confirm('Permanently delete your hub profile, messages, activity summaries and sign-in account? A UID-only deletion marker is retained for up to 90 days to prevent late writes. This cannot be undone.'))return;
  const user=currentUser,uid=user.uid;
  try{
    if(user.providerData.some(p=>p.providerId==='password')){const password=prompt('Re-enter your password to confirm account deletion.');if(password===null)return;await reauthenticateWithCredential(user,EmailAuthProvider.credential(user.email,password));}
    await ensureDeletionMarker(uid);
    const base=`testers/${uid}`,ref=doc(db,base),snap=await getDoc(ref);
    if(snap.exists()&&snap.data().deleting!==true)await updateDoc(ref,{deleting:true});
    await deleteCollection(`${base}/messages`);await deleteCollection(`${base}/days`);
    if(snap.exists())await deleteDoc(ref);
    await deleteUser(user);
    message('globalMessage','Your hub records and sign-in account were deleted. A UID-only deletion marker remains for up to 90 days.');
  }catch(e){message('globalMessage',`Deletion is incomplete. Sign in and use Delete my account and data again to finish: ${errorText(e)}`,true);}
}
$('deleteAccount').addEventListener('click',deleteMyAccount);
$('deleteSignInOnly').addEventListener('click',deleteMyAccount);
function exchangeMessage(text,error=false){message('exchangeMessage',text,error);}
function clearExchangePrivate(){
  exchangeLoadRequest++;
  exchangeView={items:[],days:new Map(),openId:'',editId:'',busy:false};
  $('exchangeList').replaceChildren();$('exchangeForm').reset();hide('exchangeForm');$('exportExchanges').disabled=true;exchangeMessage('');
  setOwnerTab('overview');
}
function setOwnerTab(name){
  const exchanges=name==='exchanges';
  $('ownerOverview').classList.toggle('hidden',exchanges);$('ownerExchanges').classList.toggle('hidden',!exchanges);
  for(const [id,active] of [['overviewTab',!exchanges],['exchangesTab',exchanges]]){$(id).classList.toggle('active',active);$(id).setAttribute('aria-selected',String(active));}
}
$('overviewTab').addEventListener('click',()=>setOwnerTab('overview'));
$('exchangesTab').addEventListener('click',()=>setOwnerTab('exchanges'));
for(const [id,name,other] of [['overviewTab','overview','exchangesTab'],['exchangesTab','exchanges','overviewTab']])$(id).addEventListener('keydown',event=>{if(event.key==='ArrowLeft'||event.key==='ArrowRight'){event.preventDefault();setOwnerTab(name==='overview'?'exchanges':'overview');$(other).focus();}});
function exchangePath(uid,id){return doc(db,'owners',uid,'exchanges',id);}
async function readExchangeDays(epoch,uid,id,entry){
  const dates=exchangeDates(entry);if(!dates.length)return {};
  const snap=await getDocs(query(collection(db,'owners',uid,'exchanges',id,'days'),where(documentId(),'>=',dates[0]),where(documentId(),'<=',dates.at(-1))));
  if(!sameSession(epoch,uid))return null;
  return Object.fromEntries(snap.docs.map(day=>[day.id,day.data()]));
}
async function loadExchanges(epoch=authEpoch,uid=currentUser?.uid){
  if(!uid||!sameSession(epoch,uid))return;
  const request=++exchangeLoadRequest;
  const snap=await getDocs(collection(db,'owners',uid,'exchanges'));
  if(!sameSession(epoch,uid)||request!==exchangeLoadRequest)return;
  const items=snap.docs.map(item=>({id:item.id,...item.data()}));
  const days=new Map();
  for(const item of items){const found=await readExchangeDays(epoch,uid,item.id,item);if(found===null||request!==exchangeLoadRequest)return;days.set(item.id,found);}
  if(!sameSession(epoch,uid)||request!==exchangeLoadRequest)return;
  exchangeView.items=items;exchangeView.days=days;
  $('exportExchanges').disabled=false;
  renderExchangeList();
}
function exchangeNode(tag,className,text){const node=document.createElement(tag);if(className)node.className=className;if(text!==undefined)node.textContent=text;return node;}
function exchangeButton(label,kind,handler){const button=exchangeNode('button',kind,label);button.type='button';button.addEventListener('click',handler);return button;}
function updateExchangeCardSummary(card,item,days){
  const summary=scheduleSummary(item,days),badge=card.querySelector('.exchange-badges strong'),line=card.querySelector('.exchange-summary');
  badge.className=summary.due?'exchange-due':'exchange-progress';badge.textContent=`${summary.due?'Due today · ':''}${summary.remaining} day${summary.remaining===1?'':'s'} remaining`;
  line.textContent=`${summary.completed} of ${summary.total} manual play checkmarks${summary.overdue?` · ${summary.overdue} past day${summary.overdue===1?'':'s'} unchecked`:''}. ${summary.today} is today in this exchange’s saved time zone.`;
}
function renderExchangeList(){
  const root=$('exchangeList');root.replaceChildren();
  const items=[...exchangeView.items].sort((a,b)=>{const aa=a.status==='archived'?1:0,bb=b.status==='archived'?1:0;return aa-bb||String(a.startDate).localeCompare(String(b.startDate))||String(a.appName).localeCompare(String(b.appName));});
  if(!items.length){root.append(exchangeNode('p','muted','No test exchanges yet. Add an app to start a private daily checklist.'));return;}
  for(const item of items){
    const days=exchangeView.days.get(item.id)||{},summary=scheduleSummary(item,days),card=exchangeNode('article','card exchange-card');
    const top=exchangeNode('div','exchange-card-top'),heading=exchangeNode('div','');heading.append(exchangeNode('h3','',item.appName||'Untitled app'),exchangeNode('p','small muted',`${item.developer||'Developer not listed'} · ${item.status||'planned'} · ${item.startDate} to ${summary.end} · ${item.timeZone}`));top.append(heading);
    const badges=exchangeNode('div','exchange-badges');badges.append(exchangeNode('strong',summary.due?'exchange-due':'exchange-progress',`${summary.due?'Due today · ':''}${summary.remaining} day${summary.remaining===1?'':'s'} remaining`));top.append(badges);card.append(top);
    card.append(exchangeNode('p','small exchange-summary'));
    updateExchangeCardSummary(card,item,days);
    if(summary.cleanupDue)card.append(exchangeNode('p','exchange-warning','The 90-day cleanup deadline has arrived. Delete this exchange and its daily records now.'));
    const report=partnerReport(participationView.testers,participationView.dateReports,item.testerUid,participationView.dates);
    card.append(exchangeNode('p','small muted',`Lantern partner: ${report} Refreshed with the owner dashboard.`));
    const actions=exchangeNode('div','exchange-actions');actions.append(exchangeButton(exchangeView.openId===item.id?'Hide checklist':'Open checklist','secondary',()=>{exchangeView.openId=exchangeView.openId===item.id?'':item.id;renderExchangeList();}));actions.append(exchangeButton('Edit','secondary',()=>openExchangeForm(item)));const archive=exchangeButton(item.status==='archived'?'Archived':'Archive','secondary',()=>setExchangeStatus(item,'archived'));archive.disabled=item.status==='archived';actions.append(archive);actions.append(exchangeButton('Delete','danger',()=>deleteExchange(item)));card.append(actions);
    if(exchangeView.openId===item.id)card.append(renderExchangeDetail(item,days,card));
    root.append(card);
  }
}
function renderExchangeDetail(item,days,card){
  const detail=exchangeNode('div','exchange-detail');
  const links=exchangeNode('div','exchange-links');for(const [label,value] of [['Google Play',item.playUrl],['Group',item.groupUrl],['Reddit / contact',item.contactUrl]]){try{const safe=safeExchangeUrl(value);if(!safe)continue;const a=exchangeNode('a','',label);a.href=safe;a.target='_blank';a.rel='noopener noreferrer';links.append(a);}catch{/* Legacy unsafe links are not rendered. */}}
  if(links.childElementCount)detail.append(links);
  if(item.instructions)detail.append(exchangeNode('p','exchange-instructions',item.instructions));
  detail.append(exchangeNode('p','small muted','Play and feedback checkmarks are your manual record. Save each day separately. Partner activity above is an optional report about Lantern Rovers only.'));
  const list=exchangeNode('div','exchange-days');
  for(const date of exchangeDates(item)){
    const state=days[date]||{},row=exchangeNode('div','exchange-day');
    const head=exchangeNode('div','exchange-day-head');head.append(exchangeNode('strong','',date));if(date===dateInZone(new Date(),item.timeZone))head.append(exchangeNode('span','exchange-today','Today'));row.append(head);
    const played=exchangeNode('input');played.type='checkbox';played.checked=state.played===true;const playedLabel=exchangeNode('label','check');playedLabel.append(played,exchangeNode('span','','Played'));row.append(playedLabel);
    const feedback=exchangeNode('input');feedback.type='checkbox';feedback.checked=state.feedback===true;const feedbackLabel=exchangeNode('label','check');feedbackLabel.append(feedback,exchangeNode('span','','Feedback sent'));row.append(feedbackLabel);
    const note=exchangeNode('textarea');note.maxLength=1000;note.value=state.note||'';note.placeholder='Private note for this day';note.setAttribute('aria-label',`Note for ${date}`);row.append(note);
    const save=exchangeButton('Save day','secondary',async()=>{const epoch=authEpoch,uid=currentUser?.uid;if(!uid||exchangeView.busy||ownerLoadPromise)return;const payload={played:played.checked,feedback:feedback.checked,note:note.value.trim()};if(payload.note.length>1000){exchangeMessage('Daily note must be 1000 characters or less.',true);return;}exchangeView.busy=true;played.disabled=feedback.disabled=note.disabled=save.disabled=true;exchangeMessage('Saving day…');try{await setDoc(doc(db,'owners',uid,'exchanges',item.id,'days',date),{...payload,updatedAt:serverTimestamp()});if(!sameSession(epoch,uid))return;days[date]=payload;const live=exchangeView.days.get(item.id);if(live)live[date]=payload;updateExchangeCardSummary(card,item,days);exchangeMessage(`${date} saved.`);}catch(e){if(sameSession(epoch,uid))exchangeMessage(`Day was not saved: ${errorText(e)}. Retry Save day.`,true);}finally{if(sameSession(epoch,uid)){exchangeView.busy=false;played.disabled=feedback.disabled=note.disabled=save.disabled=false;}}});row.append(save);list.append(row);
  }
  detail.append(list);return detail;
}
function populateExchangeTesters(value=''){
  const select=$('exchangeTesterUid');select.replaceChildren();const none=exchangeNode('option','','None');none.value='';select.append(none);
  for(const tester of participationView.testers.filter(t=>t.deleting!==true)){const option=exchangeNode('option','',`${tester.alias||'Tester'} · ${tester.uid.slice(0,8)}`);option.value=tester.uid;select.append(option);}
  if(value&&!participationView.testers.some(t=>t.uid===value&&t.deleting!==true)){const option=exchangeNode('option','',`Missing or deleting tester · ${value.slice(0,8)}`);option.value=value;select.append(option);}
  select.value=value;
}
function openExchangeForm(item=null){
  exchangeView.editId=item?.id||'';$('exchangeForm').reset();
  $('exchangeFormTitle').textContent=item?'Edit exchange':'Add exchange';
  const zone=item?.timeZone||Intl.DateTimeFormat().resolvedOptions().timeZone||'UTC';
  const fields={exchangeAppName:item?.appName||'',exchangeDeveloper:item?.developer||'',exchangePlayUrl:item?.playUrl||'',exchangeGroupUrl:item?.groupUrl||'',exchangeContactUrl:item?.contactUrl||'',exchangeStartDate:item?.startDate||dateInZone(new Date(),zone),exchangeDuration:item?.duration||14,exchangeTimeZone:zone,exchangeStatus:item?.status||'planned',exchangeInstructions:item?.instructions||''};
  for(const [id,value] of Object.entries(fields))$(id).value=value;
  populateExchangeTesters(item?.testerUid||'');show('exchangeForm');setOwnerTab('exchanges');$('exchangeForm').scrollIntoView({block:'start',behavior:'smooth'});$('exchangeAppName').focus();
}
$('newExchange').addEventListener('click',()=>openExchangeForm());
$('cancelExchange').addEventListener('click',()=>{hide('exchangeForm');exchangeView.editId='';});
function exchangeFormValue(){return validateExchange({appName:$('exchangeAppName').value,developer:$('exchangeDeveloper').value,testerUid:$('exchangeTesterUid').value,playUrl:$('exchangePlayUrl').value,groupUrl:$('exchangeGroupUrl').value,contactUrl:$('exchangeContactUrl').value,startDate:$('exchangeStartDate').value,duration:$('exchangeDuration').value,timeZone:$('exchangeTimeZone').value,status:$('exchangeStatus').value,instructions:$('exchangeInstructions').value});}
$('exchangeForm').addEventListener('submit',async event=>{
  event.preventDefault();const epoch=authEpoch,uid=currentUser?.uid;if(!uid||exchangeView.busy)return;
  let payload;try{payload=exchangeFormValue();}catch(e){exchangeMessage(e.message,true);return;}
  const id=exchangeView.editId||crypto.randomUUID(),existing=exchangeView.items.find(item=>item.id===id);
  const activeDates=new Set(exchangeDates(payload));
  const staleDates=existing?Object.keys(exchangeView.days.get(id)||{}).filter(date=>!activeDates.has(date)):[];
  if(staleDates.length&&!confirm(`Changing this schedule will delete ${staleDates.length} saved daily record(s) outside the new dates. Continue?`))return;
  exchangeView.busy=true;$('saveExchange').disabled=true;exchangeMessage('Saving exchange…');
  try{
    if(existing){const batch=writeBatch(db);batch.update(exchangePath(uid,id),{...payload,updatedAt:serverTimestamp()});for(const date of staleDates)batch.delete(doc(db,'owners',uid,'exchanges',id,'days',date));await batch.commit();}
    else await setDoc(exchangePath(uid,id),{...payload,createdAt:serverTimestamp(),updatedAt:serverTimestamp()});
    if(!sameSession(epoch,uid))return;
    exchangeView.openId=id;exchangeView.editId='';hide('exchangeForm');
    await loadExchanges(epoch,uid);if(sameSession(epoch,uid))exchangeMessage('Exchange saved.');
  }catch(e){if(sameSession(epoch,uid))exchangeMessage(`Exchange could not be saved or refreshed: ${errorText(e)}. Retry or refresh.`,true);}
  finally{if(sameSession(epoch,uid)){exchangeView.busy=false;$('saveExchange').disabled=false;}}
});
async function setExchangeStatus(item,status){
  const epoch=authEpoch,uid=currentUser?.uid;if(!uid||exchangeView.busy)return;
  exchangeView.busy=true;exchangeMessage('Updating exchange…');
  try{await updateDoc(exchangePath(uid,item.id),{status,updatedAt:serverTimestamp()});if(!sameSession(epoch,uid))return;await loadExchanges(epoch,uid);if(sameSession(epoch,uid))exchangeMessage('Exchange archived.');}
  catch(e){if(sameSession(epoch,uid))exchangeMessage(`Archive failed: ${errorText(e)}. Retry.`,true);}finally{if(sameSession(epoch,uid))exchangeView.busy=false;}
}
async function deleteExchange(item){
  if(!confirm(`Delete ${item.appName} and all its daily checklist records? This cannot be undone. Download your exchanges first if you need a copy.`))return;
  const epoch=authEpoch,uid=currentUser?.uid;if(!uid||exchangeView.busy)return;
  exchangeView.busy=true;exchangeMessage('Deleting exchange and daily records…');
  try{
    const path=`owners/${uid}/exchanges/${item.id}/days`;
    while(true){const snap=await getDocs(query(collection(db,path),limit(30)));if(!sameSession(epoch,uid))return;if(snap.empty)break;const batch=writeBatch(db);for(const day of snap.docs)batch.delete(day.ref);await batch.commit();}
    await deleteDoc(exchangePath(uid,item.id));if(!sameSession(epoch,uid))return;
    exchangeView.openId='';await loadExchanges(epoch,uid);if(sameSession(epoch,uid))exchangeMessage('Exchange and daily records deleted.');
  }catch(e){if(sameSession(epoch,uid))exchangeMessage(`Deletion is incomplete: ${errorText(e)}. Retry Delete to finish.`,true);}finally{if(sameSession(epoch,uid))exchangeView.busy=false;}
}
$('exportExchanges').addEventListener('click',()=>{
  if(!currentUser||$('owner').classList.contains('hidden'))return;
  const out=exchangeView.items.map(item=>({id:item.id,appName:item.appName,developer:item.developer,testerUid:item.testerUid,playUrl:item.playUrl,groupUrl:item.groupUrl,contactUrl:item.contactUrl,instructions:item.instructions,startDate:item.startDate,timeZone:item.timeZone,duration:item.duration,status:item.status,days:Object.fromEntries(Object.entries(exchangeView.days.get(item.id)||{}).map(([date,day])=>[date,{played:day.played===true,feedback:day.feedback===true,note:day.note||''}]))}));
  download('my-test-exchanges.json',JSON.stringify({exportedAt:new Date().toISOString(),exchanges:out},null,2),'application/json;charset=utf-8');exchangeMessage('Exchange data downloaded.');
});
$('identity').addEventListener('click',async()=>{if(currentUser&&confirm('Sign out?'))await signOut(auth);});
