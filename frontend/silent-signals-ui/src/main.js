
const API_BASE = 'https://localhost:5038'; // غيّري المنفذ حسب ما يظهر عند تشغيل الـAPI

const t = {
  ar: {
    appTitle: 'لغات بلا صوت | Silent Signals',
    converterTitle: 'المحوّل',
    systemLabel: 'النظام',
    textLabel: 'النص',
    convertNow: 'حوّل الآن',
    result: 'النتيجة',
    morse: 'مورس',
    sign: 'إشارات',
  },
  en: {
    appTitle: 'Silent Signals',
    converterTitle: 'Converter',
    systemLabel: 'System',
    textLabel: 'Text',
    convertNow: 'Convert',
    result: 'Result',
    morse: 'Morse',
    sign: 'Sign',
  }
};

const ui = {
  title: document.getElementById('app-title'),
  converterTitle: document.getElementById('converter-title'),
  systemLabel: document.getElementById('system-label'),
  textLabel: document.getElementById('text-label'),
  convertBtn: document.getElementById('convert-btn'),
  resultTitle: document.getElementById('result-title'),
  system: document.getElementById('system'),
  text: document.getElementById('text'),
  output: document.getElementById('output'),
  langAR: document.getElementById('lang-ar'),
  langEN: document.getElementById('lang-en')
};

let lang = localStorage.getItem('lang') || 'ar';
function applyLang() {
  const d = t[lang];
  ui.title.textContent = d.appTitle;
  ui.converterTitle.textContent = d.converterTitle;
  ui.systemLabel.textContent = d.systemLabel;
  ui.textLabel.textContent = d.textLabel;
  ui.convertBtn.textContent = d.convertNow;
  ui.resultTitle.textContent = d.result;
  ui.system.options[0].textContent = d.morse;
  ui.system.options[1].textContent = d.sign;
  document.documentElement.lang = lang;
  document.documentElement.dir = (lang === 'ar') ? 'rtl' : 'ltr';
}
applyLang();
ui.langAR.addEventListener('click', () => { lang = 'ar'; localStorage.setItem('lang','ar'); applyLang(); });
ui.langEN.addEventListener('click', () => { lang = 'en'; localStorage.setItem('lang','en'); applyLang(); });

async function convertTextToMorse(text) {
  const res = await fetch(`${API_BASE}/api/convert/text-to-morse`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ Text: text })
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return await res.json();
}

async function convertTextToSign(text) {
  const res = await fetch(`${API_BASE}/api/convert/text-to-sign`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ Text: text })
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return await res.json();
}

function renderMorse(response) {
  ui.output.innerHTML = '';
  response.Patterns.forEach(p => {
    const span = document.createElement('span');
    span.className = 'badge';
    span.textContent = p;
    ui.output.appendChild(span);
  });
}

function renderSign(response) {
  ui.output.innerHTML = '';
  response.Sequence.forEach(item => {
    const div = document.createElement('div');
    div.className = 'sign-card';
    if (item.ImageUrl) {
      const img = document.createElement('img');
      img.src = item.ImageUrl;
      img.alt = item.Token;
      div.appendChild(img);
    } else {
      const badge = document.createElement('span');
      badge.className = 'badge' + (item.Unknown ? ' unknown' : '');
      badge.textContent = item.Token;
      div.appendChild(badge);
    }
    const lbl = document.createElement('div');
    lbl.className = 'label';
    lbl.textContent = item.TokenType + ' • ' + item.Token;
    div.appendChild(lbl);
    ui.output.appendChild(div);
  });
}

ui.convertBtn.addEventListener('click', async () => {
  ui.output.innerHTML = '';
  const sys = ui.system.value;
  const text = ui.text.value.trim();
  if (!text) {
    const s = document.createElement('span');
    s.className = 'badge unknown';
    s.textContent = (lang === 'ar') ? 'النص مطلوب' : 'Text is required';
    ui.output.appendChild(s);
    return;
  }
  try {
    if (sys === 'morse') {
      const r = await convertTextToMorse(text);
      renderMorse(r);
    } else {
      const r = await convertTextToSign(text);
      renderSign(r);
    }
  } catch (err) {
    const s = document.createElement('span');
    s.className = 'badge unknown';
    s.textContent = (lang === 'ar') ? `خطأ: ${err.message}` : `Error: ${err.message}`;
    ui.output.appendChild(s);
  }
});
