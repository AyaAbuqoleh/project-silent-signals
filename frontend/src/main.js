
// ===== إعدادات عامة =====
const API_BASE = "http://localhost:5038"; // عدّلي المنفذ إذا تغيّر
let token = localStorage.getItem('token') || null;

// ===== عناصر DOM =====
const btnRegister = document.getElementById('btn-register');
const regEmail = document.getElementById('reg-email');
const regPass = document.getElementById('reg-pass');
const regName = document.getElementById('reg-name');
const regLang = document.getElementById('reg-lang');

const btnLogin = document.getElementById('btn-login');
const btnLogout = document.getElementById('btn-logout');
const loginEmail = document.getElementById('login-email');
const loginPass = document.getElementById('login-pass');

const profileCard = document.getElementById('profile-card');
const adminCard = document.getElementById('admin-card');

const meId = document.getElementById('me-id');
const meEmail = document.getElementById('me-email');
const meDisplay = document.getElementById('me-display');
const meLang = document.getElementById('me-lang');

const elDisplayName = document.getElementById('profile-displayname');
const elLang = document.getElementById('profile-lang');
const btnUpdateProfile = document.getElementById('btn-profile-update');

const elMorseChar = document.getElementById('morse-char');
const elMorsePattern = document.getElementById('morse-pattern');
const btnMorseAdd = document.getElementById('btn-morse-add');
const listMorse = document.getElementById('morse-list');

const elSignType = document.getElementById('sign-type');
const elSignToken = document.getElementById('sign-token');
const elSignImage = document.getElementById('sign-image');
const btnSignAdd = document.getElementById('btn-sign-add');
const listSign = document.getElementById('sign-list');

const elLessonTopic = document.getElementById('lesson-topic');
const elLessonTitleAR = document.getElementById('lesson-title-ar');
const elLessonTitleEN = document.getElementById('lesson-title-en');
const elLessonBodyAR = document.getElementById('lesson-body-ar');
const elLessonBodyEN = document.getElementById('lesson-body-en');
const btnLessonAdd = document.getElementById('btn-lesson-add');
const listLesson = document.getElementById('lesson-list');

const toastEl = document.getElementById('toast');

// ===== أدوات مساعدة =====
function showToast(msg, kind = 'info') {
  toastEl.textContent = msg;
  toastEl.className = `toast ${kind}`;
  toastEl.hidden = false;
  setTimeout(() => { toastEl.hidden = true; }, 3000);
}

function parseJwt(t) {
  try { return JSON.parse(atob(t.split('.')[1])); } catch { return null; }
}

function isAdmin() {
  if (!token) return false;
  const p = parseJwt(token);
  if (!p) return false;
  const roles = ([]).concat(
    p['role'] || p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || []
  );
  return roles.includes('Admin');
}

function setToken(t) {
  token = t;
  if (t) localStorage.setItem('token', t); else localStorage.removeItem('token');
  btnLogout.style.display = t ? 'inline-block' : 'none';
  profileCard.hidden = !t;
  adminCard.hidden = !(t && isAdmin());
}

// ===== Fetch Helpers =====
async function apiFetch(url, options = {}) {
  const res = await fetch(`${API_BASE}${url}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(options.headers || {})
    }
  });
  if (!res.ok) {
    const text = await res.text().catch(()=> '');
    throw new Error(`HTTP ${res.status} ${text}`);
  }
  return res.status === 204 ? null : await res.json();
}

async function authFetch(url, options = {}) {
  if (!token) throw new Error('No token');
  const res = await fetch(`${API_BASE}${url}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
      ...(options.headers || {})
    }
  });
  if (!res.ok) {
    const text = await res.text().catch(()=> '');
    throw new Error(`HTTP ${res.status} ${text}`);
  }
  return res.status === 204 ? null : await res.json();
}

// ===== Auth Events =====
btnRegister?.addEventListener('click', async () => {
  try {
    const payload = {
      email: regEmail.value?.trim(),
      password: regPass.value,
      displayName: regName.value?.trim() || null,
      preferredLanguage: regLang.value || 'ar'
    };
    const data = await apiFetch('/api/auth/register', { method: 'POST', body: JSON.stringify(payload) });
    setToken(data.token);
    showToast('تم إنشاء الحساب وتسجيل الدخول ✅', 'ok');
    await refreshProfile();
    await refreshAdmin();
  } catch (err) {
    console.error(err);
    showToast(`فشل التسجيل: ${err.message}`, 'err');
  }
});

btnLogin?.addEventListener('click', async () => {
  try {
    const payload = {
      email: loginEmail.value?.trim(),
      password: loginPass.value
    };
    const data = await apiFetch('/api/auth/login', { method: 'POST', body: JSON.stringify(payload) });
    setToken(data.token);
    showToast('تم تسجيل الدخول ✅', 'ok');
    await refreshProfile();
    await refreshAdmin();
  } catch (err) {
    console.error(err);
    showToast(`فشل الدخول: ${err.message}`, 'err');
  }
});

btnLogout?.addEventListener('click', () => {
  setToken(null);
  showToast('تم تسجيل الخروج', 'info');
});

// ===== Profile =====
async function refreshProfile() {
  if (!token) { profileCard.hidden = true; return; }
  try {
    const me = await authFetch('/api/auth/me', { method: 'GET' });
    meId.textContent = me.userId || '';
    meEmail.textContent = me.email || '';
    meDisplay.textContent = me.displayName || '';
    meLang.textContent = me.preferredLanguage || '';
    elDisplayName.value = me.displayName || '';
    elLang.value = (me.preferredLanguage || 'ar');
    profileCard.hidden = false;
  } catch (err) {
    console.error(err);
    profileCard.hidden = true;
  }
}

btnUpdateProfile?.addEventListener('click', async () => {
  try {
    const payload = {
      displayName: elDisplayName.value?.trim() || null,
      preferredLanguage: elLang.value || null
    };
    await authFetch('/api/auth/me', { method: 'PUT', body: JSON.stringify(payload) });
    showToast('تم تحديث الملف الشخصي ✅', 'ok');
    await refreshProfile();
  } catch (err) {
    console.error(err);
    showToast(`فشل التحديث: ${err.message}`, 'err');
  }
});

// ===== Admin (CRUD) =====
async function adminFetch(url, options = {}) {
  return await authFetch(url, options);
}

// Morse
async function loadMorse() {
  const items = await adminFetch('/api/admin/morse', { method: 'GET' });
  listMorse.innerHTML = '';
  items.forEach(m => {
    const row = document.createElement('div');
    row.className = 'row';
    row.innerHTML = `
      <span class="badge">${m.char} = ${m.pattern}</span>
      <button class="danger" data-id="${m.id}">حذف</button>
    `;
    row.querySelector('button').addEventListener('click', async () => {
      try {
        await adminFetch(`/api/admin/morse/${m.id}`, { method: 'DELETE' });
        await loadMorse();
        showToast('تم حذف مورس', 'ok');
      } catch (err) {
        console.error(err);
        showToast(`فشل حذف: ${err.message}`, 'err');
      }
    });
    listMorse.appendChild(row);
  });
}

btnMorseAdd?.addEventListener('click', async () => {
  try {
    await adminFetch('/api/admin/morse', {
      method: 'POST',
      body: JSON.stringify({ Char: elMorseChar.value, Pattern: elMorsePattern.value })
    });
    elMorseChar.value = ''; elMorsePattern.value = '';
    await loadMorse();
    showToast('تم إضافة مورس ✅', 'ok');
  } catch (err) {
    console.error(err);
    showToast(`فشل الإضافة: ${err.message}`, 'err');
  }
});

// Signs
async function loadSigns() {
  const items = await adminFetch('/api/admin/signs', { method: 'GET' });
  listSign.innerHTML = '';
  items.forEach(s => {
    const row = document.createElement('div');
    row.className = 'row';
    row.innerHTML = `
      <span class="badge">${s.tokenType}:${s.token}</span>
      <button class="danger" data-id="${s.id}">حذف</button>
    `;
    row.querySelector('button').addEventListener('click', async () => {
      try {
        await adminFetch(`/api/admin/signs/${s.id}`, { method: 'DELETE' });
        await loadSigns();
        showToast('تم حذف الإشارة', 'ok');
      } catch (err) {
        console.error(err);
        showToast(`فشل الحذف: ${err.message}`, 'err');
      }
    });
    listSign.appendChild(row);
  });
}

btnSignAdd?.addEventListener('click', async () => {
  try {
    await adminFetch('/api/admin/signs', {
      method: 'POST',
      body: JSON.stringify({
        TokenType: elSignType.value,
        Token: elSignToken.value,
        ImageUrl: elSignImage.value || null,
        DescriptionAR: null,
        DescriptionEN: null
      })
    });
    elSignToken.value = ''; elSignImage.value = '';
    await loadSigns();
    showToast('تم إضافة الإشارة ✅', 'ok');
  } catch (err) {
    console.error(err);
    showToast(`فشل الإضافة: ${err.message}`, 'err');
  }
});

// Lessons
async function loadLessons() {
  const items = await adminFetch('/api/admin/lessons', { method: 'GET' });
  listLesson.innerHTML = '';
  items.forEach(l => {
    const row = document.createElement('div');
    row.className = 'row';
    row.innerHTML = `
      <span class="badge">${l.topic} • ${l.titleAR}</span>
      <button class="danger" data-id="${l.id}">حذف</button>
    `;
    row.querySelector('button').addEventListener('click', async () => {
      try {
        await adminFetch(`/api/admin/lessons/${l.id}`, { method: 'DELETE' });
        await loadLessons();
        showToast('تم حذف الدرس', 'ok');
      } catch (err) {
        console.error(err);
        showToast(`فشل الحذف: ${err.message}`, 'err');
      }
    });
    listLesson.appendChild(row);
  });
}

btnLessonAdd?.addEventListener('click', async () => {
  try {
    await adminFetch('/api/admin/lessons', {
      method: 'POST',
      body: JSON.stringify({
        Topic: elLessonTopic.value,
        TitleAR: elLessonTitleAR.value,
        TitleEN: elLessonTitleEN.value,
        BodyAR: elLessonBodyAR.value,
        BodyEN: elLessonBodyEN.value,
        MediaUrl: null
      })
    });
    elLessonTitleAR.value = ''; elLessonTitleEN.value = '';
    elLessonBodyAR.value = '';  elLessonBodyEN.value = '';
    await loadLessons();
    showToast('تم إضافة الدرس ✅', 'ok');
  } catch (err) {
    console.error(err);
    showToast(`فشل الإضافة: ${err.message}`, 'err');
  }
});

// ===== Admin refresh =====
async function refreshAdmin() {
  try {
    adminCard.hidden = !(token && isAdmin());
    if (token && isAdmin()) {
      await Promise.all([loadMorse(), loadSigns(), loadLessons()]);
    }
  } catch (err) {
    console.error(err);
    adminCard.hidden = true;
  }
}

// ===== تشغيل أولي =====
(async () => {
  if (token) {
    btnLogout.style.display = 'inline-block';
    await refreshProfile().catch(()=>{});
    await refreshAdmin().catch(()=>{});
  }
})();
