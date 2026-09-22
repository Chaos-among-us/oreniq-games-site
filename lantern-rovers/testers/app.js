import { initializeApp } from 'https://www.gstatic.com/firebasejs/11.10.0/firebase-app.js';
import { connectAuthEmulator, createUserWithEmailAndPassword, sendEmailVerification, sendPasswordResetEmail, signInWithEmailAndPassword, getAuth, EmailAuthProvider, reauthenticateWithCredential, signOut, onAuthStateChanged, deleteUser } from 'https://www.gstatic.com/firebasejs/11.10.0/firebase-auth.js';
import { connectFirestoreEmulator, getFirestore, doc, getDoc, setDoc, updateDoc, deleteDoc, collection, query, orderBy, getDocs, limit, serverTimestamp, writeBatch, onSnapshot } from 'https://www.gstatic.com/firebasejs/11.10.0/firebase-firestore.js';
import { firebaseConfig, useEmulators } from './firebase-config.js';

const $ = id => document.getElementById(id);
const show = id => $(id).classList.remove('hidden');
const hide = id => $(id).classList.add('hidden');
const message = (id, text, error=false) => { const el=$(id); el.textContent=text; el.style.color=error?'#9a4139':'#356344'; };
const configured = firebaseConfig.apiKey && !firebaseConfig.apiKey.startsWith('REPLACE_') && firebaseConfig.projectId && !firebaseConfig.projectId.startsWith('REPLACE_');
let app, auth, db, currentUser, profile, selectedUid='', unsubscribeMessages=null, mode='signin', authEpoch=0;

function emptyAll(){['auth','enroll','tester','owner','setup','resendVerification'].forEach(hide);}
function clearPrivate(){for(const key of Object.keys(topicViews))topicViews[key]={items:[],id:'',subject:''};profile=null;selectedUid='';['messages','ownerMessages','roster','activityRows'].forEach(id=>$(id).replaceChildren());hide('replyForm');}
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
      if(ownerSnap.exists()){show('owner');await loadOwner();return;}
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
$('authForm').addEventListener('submit',async e=>{e.preventDefault();if(!auth)return;const email=$('email').value.trim(),password=$('password').value;try{if(mode==='signup'){if(!$('authAdult').checked){message('authMessage','Connected tester accounts are for adults 18 and older.',true);return;}const cred=await createUserWithEmailAndPassword(auth,email,password);await sendEmailVerification(cred.user);await signOut(auth);message('authMessage','Check your email for a verification link, then return here to sign in.');}else{await signInWithEmailAndPassword(auth,email,password);}}catch(err){message('authMessage',errorText(err),true);}});
$('reset').addEventListener('click',async()=>{if(!auth)return;const email=$('email').value.trim();if(!email){message('authMessage','Enter your email above first.');return;}try{await sendPasswordResetEmail(auth,email);message('authMessage','If this address has an account, a reset link is on its way.');}catch(e){message('authMessage',errorText(e),true);}});
$('resendVerification').addEventListener('click',async()=>{if(!currentUser)return;try{await sendEmailVerification(currentUser);message('authMessage','Verification email sent. Check your inbox and spam folder.');}catch(e){message('authMessage',errorText(e),true);}});

$('enrollForm').addEventListener('submit',async e=>{e.preventDefault();try{const uid=currentUser.uid;const alias=$('alias').value.trim();await setDoc(doc(db,'testers',uid),{email:currentUser.email,alias,status:'pending',adultConfirmed:$('adult').checked,telemetryConsent:$('telemetry').checked,privacyVersion:'2026-09-22-v1',createdAt:serverTimestamp(),accessProvisioned:false,deleting:false});profile={alias,status:'pending',telemetryConsent:$('telemetry').checked,deleting:false};emptyAll();show('tester');await loadTester();}catch(err){message('enrollMessage',errorText(err),true);}});

async function loadTester(){
  const epoch=authEpoch,uid=currentUser.uid;
  hide('pendingNote');
  if(profile.deleting===true){$('hello').textContent=`Welcome, ${profile.alias}`;$('statusText').textContent='Account deletion is in progress. Retry to finish removing your data and sign-in account.';$('statusBadge').textContent='Deleting';$('messageForm').querySelector('button').disabled=true;$('consentToggle').disabled=true;$('activitySummary').textContent='Activity sharing is stopped.';return;}
  $('consentToggle').disabled=false;
  $('hello').textContent=`Welcome, ${profile.alias}`;$('statusText').textContent=profile.status==='approved'?'Your tester application is approved.': 'Your application is pending owner review.';
  $('statusBadge').textContent=profile.status==='approved'?'Approved':'Pending';$('statusBadge').classList.toggle('ok',profile.status==='approved');if(profile.status==='pending')show('pendingNote');
  $('consentToggle').checked=profile.telemetryConsent===true;$('messageText').disabled=profile.status!=='approved';$('messageForm').querySelector('button').disabled=profile.status!=='approved';
  $('messageNotice').textContent=profile.status==='approved'?'Only you and the test owner can read this conversation.':'The inbox becomes available after approval. For application questions, use the support email on the privacy page.';
  $('activitySummary').textContent=profile.telemetryConsent?'Daily summaries are enabled.':'Daily summaries are off; no activity summary should be sent.';
  if(profile.status==='approved'){
    unsubscribeMessages=onSnapshot(query(collection(db,'testers',uid,'messages'),orderBy('createdAt')),snap=>{if(sameSession(epoch,uid))selectTopics('messages',snap.docs.map(d=>d.data()));},err=>{if(sameSession(epoch,uid))message('messageNotice',errorText(err),true);});
  } else {$('messages').innerHTML='<p class="muted small">No conversation yet.</p>';}
  if(profile.telemetryConsent){const days=await getDocs(query(collection(db,'testers',uid,'days'),orderBy('updatedAt','desc'),limit(14)));if(!sameSession(epoch,uid))return;const qualified=days.docs.map(d=>d.data()).filter(qualifies).length;$('activitySummary').textContent+=` ${qualified} qualifying reported day(s) in the most recent ${days.size} summaries.`;}
}
function qualifies(d){return d.activeSeconds>=120&&(d.kills>0||d.nodes>0||d.lanterns>0);}
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

async function loadOwner(){
  const epoch=authEpoch,uid=currentUser.uid;
  const snap=await getDocs(query(collection(db,'testers'),orderBy('createdAt','desc')));const testers=snap.docs.map(d=>({uid:d.id,...d.data()}));
  if(!sameSession(epoch,uid))return;
  $('pendingCount').textContent=testers.filter(t=>t.status==='pending').length;$('approvedCount').textContent=testers.filter(t=>t.status==='approved').length;
  const roster=$('roster');roster.innerHTML='';for(const t of testers){const row=document.createElement('div');row.className='personhead';const b=document.createElement('button');b.className='person'+(selectedUid===t.uid?' selected':'');b.innerHTML=`<strong>${esc(t.alias)}</strong><small>${esc(t.email)} · ${esc(t.status)} · ${t.telemetryConsent?'activity on':'activity off'}</small>`;b.onclick=()=>selectTester(t);const action=document.createElement('button');action.className='secondary';action.textContent=t.status==='approved'?'Revoke':'Approve';action.onclick=async()=>{try{await updateDoc(doc(db,'testers',t.uid),{status:t.status==='approved'?'pending':'approved',accessProvisioned:t.status!=='approved'});await loadOwner();}catch(e){message('globalMessage',errorText(e),true);}};row.append(b,action);roster.append(row);}
  const activityBody=$('activityRows');activityBody.innerHTML='';let qualifiedTotal=0;const now=new Date();const since=new Date(now);since.setUTCDate(since.getUTCDate()-13);const minId=since.toISOString().slice(0,10),maxId=now.toISOString().slice(0,10);
  for(const t of testers){let all=[];if(t.telemetryConsent){const ds=await getDocs(query(collection(db,'testers',t.uid,'days'),orderBy('updatedAt','desc'),limit(30)));if(!sameSession(epoch,uid))return;all=ds.docs.filter(d=>d.id>=minId&&d.id<=maxId).map(d=>d.data());}
    const q=all.filter(qualifies).length;qualifiedTotal+=q;const last=all.map(d=>d.updatedAt?.toDate?.()).filter(Boolean).sort((a,b)=>b-a)[0];const tr=document.createElement('tr');for(const val of [t.alias,`${all.length}/14`,`${q}/14`,last?last.toLocaleString():'No report',t.telemetryConsent?'Opted in':'Off']){const td=document.createElement('td');td.textContent=val;tr.append(td);}activityBody.append(tr);}
  $('qualifiedCount').textContent=qualifiedTotal;
}
async function selectTester(t){const epoch=authEpoch,uid=currentUser.uid;if(selectedUid!==t.uid){topicViews.ownerMessages={items:[],id:'',subject:''};}selectedUid=t.uid;$('threadTitle').textContent=`${t.alias} · ${t.status}`;$('ownerMessages').innerHTML='';show('replyForm');const msgs=await getDocs(query(collection(db,'testers',t.uid,'messages'),orderBy('createdAt')));if(!sameSession(epoch,uid))return;selectTopics('ownerMessages',msgs.docs.map(d=>d.data()));document.querySelectorAll('.person').forEach(el=>el.classList.remove('selected'));}
$('replyForm').addEventListener('submit',async e=>{e.preventDefault();if(!selectedUid)return;const text=$('replyText').value.trim();if(!text)return;try{const id=crypto.randomUUID();await setDoc(doc(db,'testers',selectedUid,'messages',id),{role:'studio',text,...topicFields('ownerMessages'),createdAt:serverTimestamp(),build:$('replyBuild').value.trim().slice(0,64),clientId:id});$('replyText').value='';await selectTester({uid:selectedUid,alias:$('threadTitle').textContent.split(' · ')[0],status:$('threadTitle').textContent.split(' · ')[1]||''});}catch(err){message('globalMessage',errorText(err),true);}});
$('playExport').addEventListener('click',async()=>{const snap=await getDocs(collection(db,'testers'));const rows=[['email','status','access_provisioned'],...snap.docs.map(d=>{const x=d.data();return[x.email,x.status,String(x.accessProvisioned)]})];download('tester-play-email-list.csv',rows.map(r=>r.map(csv).join(',')).join('\r\n'));});
function csv(v){return '"'+String(v??'').replaceAll('"','""')+'"';}
function download(name,text){const a=document.createElement('a');a.href=URL.createObjectURL(new Blob([text],{type:'text/plain;charset=utf-8'}));a.download=name;a.click();URL.revokeObjectURL(a.href);}
$('exportMine').addEventListener('click',async()=>{try{const [m,d]=await Promise.all([getDocs(collection(db,'testers',currentUser.uid,'messages')),getDocs(collection(db,'testers',currentUser.uid,'days'))]);const out={profile:{alias:profile.alias,status:profile.status,telemetryConsent:profile.telemetryConsent},messages:m.docs.map(x=>({id:x.id,...x.data()})),days:d.docs.map(x=>({date:x.id,...x.data()}))};download('my-tester-data.json',JSON.stringify(out,null,2));}catch(e){message('globalMessage',errorText(e),true);}});
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
$('identity').addEventListener('click',async()=>{if(currentUser&&confirm('Sign out?'))await signOut(auth);});
