
const API_BASE = 'https://localhost:5038'; // <-- غيّري المنفذ حسب تشغيل الـAPI

const t = {
  ar: { appTitle:'لغات بلا صوت | Silent Signals', converterTitle:'المحوّل', systemLabel:'النظام', textLabel:'النص', convertNow:'حوّل الآن', result:'النتيجة', morse:'مورس', sign:'إشارات' },
  en: { appTitle:'Silent Signals', converterTitle:'Converter', systemLabel:'System', textLabel:'Text', convertNow:'Convert', result:'Result', morse:'Morse', sign:'Sign' }
};

const ui = {
  title: document.getElementById('app-title'), converterTitle: document.getElementById('converter-title'),
  systemLabel: document.getElementById('system-label'), textLabel: document.getElementById('text-label'),
  convertBtn: document.getElementById('convert-btn'), resultTitle: document.getElementById('result-title'),
  system: document.getElementById('system'), text: document.getElementById('text'), output: document.getElementById('output'),
  langAR: document.getElementById('lang-ar'), langEN: document.getElementById('lang-en'),
  btnRegister: document.getElementById('btn-register'), btnLogin: document.getElementById('btn-login'), btnLogout: document.getElementById('btn-logout'),
  profileCard: document.getElementById('profile-card'), profileInfo: document.getElementById('profile-info'), historyList: document.getElementById('history-list')
};

let lang = localStorage.getItem('lang') || 'ar';
function applyLang() {
  const d = t[lang];
  ui.title.textContent = d.appTitle; ui.converterTitle.textContent = d.converterTitle;
  ui.systemLabel.textContent = d.systemLabel; ui.textLabel.textContent = d.textLabel;
  ui.convertBtn.textContent = d.convertNow; ui.resultTitle.textContent = d.result;
  ui.system.options[0].textContent = d.morse; ui.system.options[1].textContent = d.sign;
  document.documentElement.lang = lang; document.documentElement.dir = (lang === 'ar') ? 'rtl' : 'ltr';
}
applyLang();
ui.langAR.addEventListener('click', () => { lang='ar'; localStorage.setItem('lang','ar'); applyLang(); });
ui.langEN.addEventListener('click', () => { lang='en'; localStorage.setItem('lang','en'); applyLang(); });

// ===== Token management =====
let token = localStorage.getItem('token') || null;
function setToken(t) { token = t; if (t) localStorage.setItem('token', t); else localStorage.removeItem('token'); ui.btnLogout.style.display = t ? 'inline-block' : 'none'; ui.profileCard.hidden = !t; }

// ===== Auth APIs =====
async function authRegister(email, password, displayName, preferredLanguage='ar') {
  const res = await fetch(`${API_BASE}/api/auth/register`, { method:'POST', headers:{ 'Content-Type':'application/json' }, body: JSON.stringify({ Email:email, Password:password, DisplayName:displayName, PreferredLanguage:preferredLanguage }) });
  if (!res.ok) throw new Error(`HTTP ${res.status}`); const data = await res.json(); setToken(data.Token); localStorage.setItem('displayName', data.DisplayName || email); localStorage.setItem('preferredLanguage', data.PreferredLanguage || 'ar'); return data;
}
async function authLogin(email, password) {
  const res = await fetch(`${API_BASE}/api/auth/login`, { method:'POST', headers:{ 'Content-Type':'application/json' }, body: JSON.stringify({ Email:email, Password:password }) });
  if (!res.ok) throw new Error(`HTTP ${res.status}`); const data = await res.json(); setToken(data.Token); localStorage.setItem('displayName', data.DisplayName || email); localStorage.setItem('preferredLanguage', data.PreferredLanguage || 'ar'); return data;
}
async function authMe() {
  if (!token) throw new Error('No token');
  const res = await fetch(`${API_BASE}/api/auth/me`, { headers:{ Authorization:`Bearer ${token}` } });
  if (!res.ok) throw new Error(`HTTP ${res.status}`); return await res.json();
}
async function fetchMyHistory() {
  if (!token) throw new Error('No token');
  const res = await fetch(`${API_BASE}/api/history/my`, { headers:{ Authorization:`Bearer ${token}` } });
  if (!res.ok) throw new Error(`HTTP ${res.status}`); return await res.json();
}

// ===== Converter APIs =====
async function convertTextToMorse(text) {
  const res = await fetch(`${API_BASE}/api/convert/text-to-morse`, {
    method:'POST',
    headers:{ 'Content-Type':'application/json', ...(token ? { Authorization:`Bearer ${token}` } : {}) },
    body: JSON.stringify({ Text:text })
  }); if (!res.ok) throw new Error(`HTTP ${res.status}`); return await res.json();
}
async function convertTextToSign(text) {
  const res = await fetch(`${API_BASE}/api/convert/text-to-sign`, {
    method:'POST',
    headers:{ 'Content-Type':'application/json', ...(token ? { Authorization:`Bearer ${token}` } : {}) },
    body: JSON.stringify({ Text:text })
  }); if (!res.ok) throw new Error(`HTTP ${res.status}`); return await res.json();
}

// ===== Renderers =====
function renderMorse(response) {
  ui.output.innerHTML = '';
  response.Patterns.forEach(p => { const s=document.createElement('span'); s.className='badge'; s.textContent=p; ui.output.appendChild(s); });
}
function renderSign(response) {
  ui.output.innerHTML = '';
  response.Sequence.forEach(item => {
    const d=document.createElement('div'); d.className='sign-card';
    if (item.ImageUrl) { const img=document.createElement('img'); img.src=item.ImageUrl; img.alt=item.Token; d.appendChild(img); }
    else { const b=document.createElement('span'); b.className='badge' + (item.Unknown ? ' unknown' : ''); b.textContent=item.Token; d.appendChild(b); }
    const l=document.createElement('div'); l.className='label'; l.textContent=item.TokenType + ' • ' + item.Token; d.appendChild(l);
    ui.output.appendChild(d);
  });
}

// ===== UI events =====
ui.convertBtn.addEventListener('click', async () => {
  ui.output.innerHTML = ''; const sys=ui.system.value; const text=ui.text.value.trim();
  if (!text) { const s=document.createElement('span'); s.className='badge unknown'; s.textContent=(lang==='ar') ? 'النص مطلوب' : 'Text is required'; ui.output.appendChild(s); return; }
  try { if (sys==='morse') { const r=await convertTextToMorse(text); renderMorse(r); } else { const r=await convertTextToSign(text); renderSign(r); } }
  catch(err) { const s=document.createElement('span'); s.className='badge unknown'; s.textContent=(lang==='ar') ? `خطأ: ${err.message}` : `Error: ${err.message}`; ui.output.appendChild(s); }
});
ui.btnRegister.addEventListener('click', async () => {
  const email=prompt('Email:'); const pass=prompt('Password (min 6):'); const name=prompt('Display Name (اختياري):')||''; const plang=prompt('Preferred Language (ar/en):')||'ar';
  try { await authRegister(email, pass, name, plang); alert('Registered & logged in'); await refreshProfile(); } catch(e){ alert(`Error: ${e.message}`); }
});
ui.btnLogin.addEventListener('click', async () => {
  const email=prompt('Email:'); const pass=prompt('Password:'); try { await authLogin(email, pass); alert('Logged in'); await refreshProfile(); } catch(e){ alert(`Error: ${e.message}`); }
});
ui.btnLogout.addEventListener('click', () => { setToken(null); alert('Logged out'); ui.profileInfo.innerHTML=''; ui.historyList.innerHTML=''; });

async function refreshProfile() {
  if (!token) { ui.profileCard.hidden = true; return; }
  try {
    const me = await authMe(); ui.profileCard.hidden = false;
    ui.profileInfo.innerHTML = `
      <p><strong>Email:</strong> ${me.email}</p>
      <p><strong>Display Name:</strong> ${me.displayName}</p>
      <p><strong>Preferred Language:</strong> ${me.preferredLanguage}</p>
      <p><strong>Created:</strong> ${new Date(me.createdAt).toLocaleString()}</p>
    `;
    const hist = await fetchMyHistory(); ui.historyList.innerHTML='';
    hist.forEach(h => { const div=document.createElement('div'); div.className='sign-card';
      div.innerHTML = `
        <div class="label"><strong>${h.targetSystem}</strong> • ${new Date(h.createdAt).toLocaleString()}</div>
        <div class="label">Text: ${h.sourceText}</div>
      `;
      ui.historyList.appendChild(div);
    });
  } catch(e){ alert(`Error: ${e.message}`); }
}

// init
setToken(token);
refreshProfile().catch(() => {});
