/* FlowOps Portal — shared JS */

// ── Dark / Light mode ─────────────────────────────────────
const dmTgl = document.getElementById('dm-tgl');
let dark = localStorage.getItem('fo-dark') !== 'false';

function applyTheme() {
  document.getElementById('app')?.classList.toggle('lm', !dark);
  if (dmTgl) dmTgl.classList.toggle('on', dark);
}
applyTheme();
dmTgl?.parentElement.addEventListener('click', () => {
  dark = !dark;
  localStorage.setItem('fo-dark', dark);
  applyTheme();
});

// ── Refresh button ────────────────────────────────────────
function refreshPage(btn) {
  const i = btn.querySelector('i');
  if (i) { i.style.animation = 'spin .5s linear'; setTimeout(() => i.style.animation = '', 500); }
  location.reload();
}

// ── Toast notifications ───────────────────────────────────
function showToast(type, title, msg) {
  const area = document.getElementById('toasts');
  if (!area) return;
  const cfg = {
    ok: { bg: 'var(--okb)', c: 'var(--okt)', ic: 'ti-check' },
    er: { bg: 'var(--erb)', c: 'var(--ert)', ic: 'ti-x' },
    in: { bg: 'var(--inb)', c: 'var(--int)', ic: 'ti-info-circle' },
    wa: { bg: 'var(--wab)', c: 'var(--wat)', ic: 'ti-alert-triangle' }
  };
  const s = cfg[type] || cfg.in;
  const t = document.createElement('div');
  t.className = 'toast';
  t.innerHTML = `<div class="toast-ic" style="background:${s.bg};color:${s.c}"><i class="ti ${s.ic}"></i></div><div><div style="font-weight:500;font-size:13px">${title}</div><div style="font-size:11px;color:var(--t2);margin-top:1px">${msg}</div></div>`;
  area.appendChild(t);
  setTimeout(() => { t.classList.add('fade'); setTimeout(() => t.remove(), 400); }, 3200);
}

// ── Live alert badge ──────────────────────────────────────
async function pollAlerts() {
  try {
    const r = await fetch('/api/monitor/alerts');
    if (!r.ok) return;
    const d = await r.json();
    const badge = document.getElementById('alert-count');
    if (!badge) return;
    const count = d.critical ?? 0;
    badge.textContent = count;
    badge.style.display = count > 0 ? 'inline-flex' : 'none';
  } catch { }
}
pollAlerts();
setInterval(pollAlerts, 60_000);

// ── ArgoCD force sync ─────────────────────────────────────
async function forceSync(appName) {
  showToast('in', 'Syncing', `Force-syncing ${appName}…`);
  try {
    const r = await fetch(`/api/argocd/sync/${appName}`, { method: 'POST' });
    const d = await r.json();
    d.success
      ? showToast('ok', 'Sync complete', `${appName} synced successfully`)
      : showToast('er', 'Sync failed', `${appName} could not sync`);
  } catch { showToast('er', 'Error', 'Network request failed'); }
}

async function forceSyncAll() {
  showToast('in', 'Syncing all', 'Force-syncing all applications…');
  try {
    const r = await fetch('/api/argocd/sync-all', { method: 'POST' });
    const d = await r.json();
    d.success
      ? showToast('ok', 'All synced', 'All applications synced successfully')
      : showToast('wa', 'Partial sync', 'Some applications failed to sync');
  } catch { showToast('er', 'Error', 'Network request failed'); }
}

// ── Deployment status poller ──────────────────────────────
function pollDeploymentStatus(deploymentId) {
  const interval = setInterval(async () => {
    try {
      const r = await fetch(`/api/deploy/${deploymentId}/status`);
      const d = await r.json();
      updatePipelineUI(d.status);
      if (['Success', 'Failed', 'RolledBack'].includes(d.status)) {
        clearInterval(interval);
        const type = d.status === 'Success' ? 'ok' : 'er';
        showToast(type, `Deployment ${d.status}`,
          d.status === 'Success'
            ? `Completed in ${Math.round(d.duration ?? 0)}s`
            : d.errorMessage ?? 'Check logs for details');
      }
    } catch { clearInterval(interval); }
  }, 3000);
}

function updatePipelineUI(status) {
  const stepsEl = document.querySelectorAll('.stn');
  const current = { Pending: 0, Running: 2, Success: 5, Failed: -1, RolledBack: -1 }[status] ?? 0;
  stepsEl.forEach((el, i) => {
    el.classList.remove('done', 'run');
    if (current < 0) return;
    if (i < current - 1)     el.classList.add('done');
    else if (i === current - 1) el.classList.add('run');
  });
}

// ── Toggle switches ───────────────────────────────────────
document.querySelectorAll('.tsw').forEach(sw =>
  sw.addEventListener('click', () => sw.classList.toggle('on'))
);
