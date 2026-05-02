<template>
  <main class="page-bilanco">
    <header class="page-header">
      <div class="header-content">
        <h1>Bilanço & Muhasebe</h1>
        <p class="subtitle">Klinik gelir-gider dengesini ve tahsilat durumlarını izleyin.</p>
      </div>

      <div class="header-actions" v-if="canAccessBilanco">
        <div class="segmented-control">
          <button
            v-for="p in ['day', 'week', 'month', 'year']"
            :key="p"
            class="seg-btn"
            :class="{ active: period === p }"
            @click="setPeriod(p)"
          >
            {{ {day: 'Gün', week: 'Hafta', month: 'Ay', year: 'Yıl'}[p] }}
          </button>
        </div>
      </div>
    </header>

    <div v-if="!canAccessBilanco" class="access-denied">
      <div class="empty-icon">🔒</div>
      <h3>Yetki Gereklidir</h3>
      <p>Bu sayfaya erişmek için yönetici yetkisine sahip olmanız gerekmektedir.</p>
    </div>

    <template v-else>
      <!-- FILTER BAR -->
      <section class="filters-bar">
        <div class="filter-grid">
          <div class="filter-col">
            <span class="label">Başlangıç</span>
            <input class="modern-input" type="date" v-model="from" />
          </div>

          <div class="filter-col">
            <span class="label">Bitiş</span>
            <input class="modern-input" type="date" v-model="to" />
          </div>

          <div class="filter-col actions">
            <button class="btn btn-primary" @click="loadLedger" :disabled="loading">
              {{ loading ? 'Yükleniyor...' : 'Verileri Güncelle' }}
            </button>
          </div>
        </div>
        <p v-if="error" class="state state-error">{{ error }}</p>
      </section>

      <!-- KPI CARDS -->
      <section class="stats-grid">
        <div class="stat-card sky">
          <div class="stat-icon">💰</div>
          <div class="stat-info">
            <span class="stat-label">Toplam Ciro</span>
            <div class="stat-value">{{ fmtMoney(summaryTotalAmount) }}</div>
          </div>
        </div>

        <div class="stat-card emerald">
          <div class="stat-icon">✅</div>
          <div class="stat-info">
            <span class="stat-label">Tahsil Edilen</span>
            <div class="stat-value">{{ fmtMoney(summaryTotalCollected) }}</div>
          </div>
        </div>

        <div class="stat-card rose">
          <div class="stat-icon">⏳</div>
          <div class="stat-info">
            <span class="stat-label">Toplam Veresiye</span>
            <div class="stat-value danger">{{ fmtMoney(summaryTotalCredit) }}</div>
          </div>
        </div>

        <div class="stat-card amber">
          <div class="stat-icon">📋</div>
          <div class="stat-info">
            <span class="stat-label">İşlem Sayısı</span>
            <div class="stat-value">{{ summaryVisitCount }} Adet</div>
          </div>
        </div>
      </section>

      <div class="ledger-layout">
        <!-- Sol: Kullanıcı Bazlı Rapor -->
        <div class="report-section">
          <h3 class="section-title">Kullanıcı Bazlı İşlemler</h3>
          
          <div v-if="!visitUserGroups || !visitUserGroups.length" class="empty-state">
            Bu aralıkta veri bulunamadı.
          </div>

          <div v-else class="user-groups">
            <article v-for="group in visitUserGroups" :key="groupKey(group)" class="user-card">
              <div class="user-header">
                <div class="user-main">
                  <span class="username">{{ group.fullName || group.username || '—' }}</span>
                  <span class="visit-count">{{ groupSummary(group).visitCount || 0 }} Ziyaret</span>
                </div>
                <div class="user-totals">
                  <div class="total-row">
                    <span>Ciro:</span>
                    <span class="val">{{ fmtMoney(groupTotalAmount(group)) }}</span>
                  </div>
                  <div class="total-row">
                    <span class="danger">Veresiye:</span>
                    <span class="val danger">{{ fmtMoney(groupTotalCredit(group)) }}</span>
                  </div>
                </div>
              </div>
            </article>
          </div>
        </div>

        <!-- Sağ: Manuel Gelir/Gider -->
        <div class="manual-section">
          <div class="manual-card">
            <div class="card-header">
              <h3>Manuel Kayıt Ekle</h3>
              <div class="net-summary">
                <span class="pill income">Gelir: {{ fmtMoney(totalIncome) }}</span>
                <span class="pill expense">Gider: {{ fmtMoney(totalExpense) }}</span>
              </div>
            </div>

            <form @submit.prevent="submitEntry" class="manual-form">
              <div class="form-grid">
                <div class="form-group">
                  <label>Tarih</label>
                  <input type="date" v-model="form.date" required />
                </div>
                <div class="form-group">
                  <label>Tutar</label>
                  <input type="number" step="0.01" v-model.number="form.amount" placeholder="0.00" required />
                </div>
                <div class="form-group">
                  <label>Tür</label>
                  <div class="type-pill-group">
                    <button type="button" class="type-pill" :class="{ active: form.type === 'income' }" @click="form.type = 'income'">Gelir</button>
                    <button type="button" class="type-pill" :class="{ active: form.type === 'expense' }" @click="form.type = 'expense'">Gider</button>
                  </div>
                </div>
              </div>
              <div class="form-group">
                <label>Kategori / Açıklama</label>
                <input type="text" v-model="form.category" placeholder="Kira, Fatura, İlaç vb." />
              </div>
              <button class="btn btn-primary" type="submit" :disabled="saving">
                {{ saving ? 'Kaydediliyor...' : 'Kayıt Ekle' }}
              </button>
            </form>

            <div class="ledger-table-wrapper">
              <table class="premium-table">
                <thead>
                  <tr>
                    <th>Tarih</th>
                    <th>Kategori</th>
                    <th class="right">Tutar</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="e in entries" :key="e.id">
                    <td>{{ e.date }}</td>
                    <td>
                      <span class="cat">{{ e.category || '—' }}</span>
                      <span class="badge" :class="e.isIncome ? 'income' : 'expense'">{{ e.isIncome ? 'Gelir' : 'Gider' }}</span>
                    </td>
                    <td class="right font-bold" :class="e.isIncome ? 'text-success' : 'text-danger'">
                      {{ e.isIncome ? '+' : '-' }}{{ fmtMoney(e.amount) }}
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </template>
  </main>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getUser } from '@/utils/auth'
import {
  fetchLedgerSummary,
  fetchLedgerByUser,
  createLedgerEntry,
  fetchLedgerRange,
} from '@/api/ledger'

const router = useRouter()

const user = computed(() => getUser() || null)
const canAccessBilanco = computed(() => {
  const u = user.value || {}
  const role = String(u.role || '').trim().toLowerCase()
  const username = String(u.username || '').trim().toLowerCase()
  return role === 'admin' || username === 'bullboss'
})

const entries = ref([])
const visitUserGroups = ref([])
const summary = ref(null)
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const period = ref('month')

function toIsoDate(date) {
  return date.toISOString().slice(0, 10)
}
const todayDate = new Date()
const from = ref(toIsoDate(todayDate))
const to = ref(toIsoDate(todayDate))

const form = ref({
  date: toIsoDate(todayDate),
  amount: null,
  type: 'income',
  category: '',
  note: '',
})

const totalIncome = computed(() =>
  (entries.value || []).filter(e => e?.isIncome).reduce((s, e) => s + Number(e?.amount || 0), 0),
)
const totalExpense = computed(() =>
  (entries.value || []).filter(e => !e?.isIncome).reduce((s, e) => s + Number(e?.amount || 0), 0),
)
const netTotal = computed(() => totalIncome.value - totalExpense.value)

const summaryTotalAmount = computed(() => Number((summary.value?.totalAmount ?? summary.value?.TotalAmount) || 0))
const summaryTotalCollected = computed(() => Number((summary.value?.totalCollected ?? summary.value?.TotalCollected) || 0))
const summaryTotalCredit = computed(() => Number((summary.value?.totalCredit ?? summary.value?.TotalCredit) || 0))
const summaryVisitCount = computed(() => Number((summary.value?.visitCount ?? summary.value?.VisitCount) || 0))

function fmtMoney(value) {
  const n = Number(value ?? 0)
  return `${n.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₺`
}

function groupKey(group) {
  return group.username || group.Username || group.userId || group.UserId || Math.random()
}
function groupSummary(group) {
  return (group.summary || group.Summary) || {}
}
function groupTotalAmount(group) {
  const s = groupSummary(group)
  return Number((s.totalAmount ?? s.TotalAmount) || 0)
}
function groupTotalCredit(group) {
  const s = groupSummary(group)
  return Number((s.totalCredit ?? s.TotalCredit) || 0)
}

function setPeriod(p) {
  period.value = p
  if (p === 'day') return setToday()
  if (p === 'week') return setThisWeek()
  if (p === 'month') return setThisMonth()
  if (p === 'year') return setThisYear()
}

function setToday() {
  const d = new Date()
  const iso = toIsoDate(d)
  from.value = iso
  to.value = iso
  loadLedger()
}

function setThisWeek() {
  const now = new Date()
  const day = now.getDay() || 7
  const monday = new Date(now)
  monday.setDate(now.getDate() - (day - 1))
  const sunday = new Date(monday)
  sunday.setDate(monday.getDate() + 6)
  from.value = toIsoDate(monday)
  to.value = toIsoDate(sunday)
  loadLedger()
}

function setThisMonth() {
  const now = new Date()
  const start = new Date(now.getFullYear(), now.getMonth(), 1)
  const end = new Date(now.getFullYear(), now.getMonth() + 1, 0)
  from.value = toIsoDate(start)
  to.value = toIsoDate(end)
  loadLedger()
}

function setThisYear() {
  const now = new Date()
  const start = new Date(now.getFullYear(), 0, 1)
  const end = new Date(now.getFullYear(), 11, 31)
  from.value = toIsoDate(start)
  to.value = toIsoDate(end)
  loadLedger()
}

async function loadLedger() {
  if (!canAccessBilanco.value) return
  loading.value = true
  error.value = ''
  try {
    const [sumRes, rangeRes, byUserRes] = await Promise.all([
      fetchLedgerSummary(from.value, to.value),
      fetchLedgerRange(from.value, to.value),
      fetchLedgerByUser(from.value, to.value),
    ])
    summary.value = sumRes || null
    entries.value = Array.isArray(rangeRes) ? rangeRes : (rangeRes?.items ?? [])
    visitUserGroups.value = Array.isArray(byUserRes) ? byUserRes : []
  } catch (e) {
    error.value = 'Veriler yüklenemedi.'
  } finally {
    loading.value = false
  }
}

async function submitEntry() {
  if (!canAccessBilanco.value || !form.value.date || !form.value.amount) return
  const payload = {
    date: form.value.date,
    amount: Number(form.value.amount),
    isIncome: form.value.type === 'income',
    category: form.value.category || null,
  }
  saving.value = true
  try {
    const created = await createLedgerEntry(payload)
    if (created?.date && created.date >= from.value && created.date <= to.value) {
      entries.value.unshift(created)
    }
    form.value.amount = null
    form.value.category = ''
  } catch (e) {
    alert('Hata oluştu.')
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  if (!canAccessBilanco.value) {
    router.push('/')
    return
  }
  setThisMonth()
})
</script>

<style scoped>
.page-bilanco {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
}

.page-header h1 {
  font-size: 2.25rem;
  letter-spacing: -0.05em;
  font-weight: 800;
}

.subtitle {
  color: var(--text-muted);
  font-size: 1.1rem;
}

/* SEGMENTED CONTROL */
.segmented-control {
  display: flex;
  background: rgba(241, 245, 249, 0.8);
  backdrop-filter: blur(8px);
  padding: 4px;
  border-radius: 14px;
  border: 1px solid #f1f5f9;
}

.seg-btn {
  padding: 0.6rem 1.25rem;
  border: none;
  background: transparent;
  border-radius: 10px;
  font-size: 0.9rem;
  font-weight: 700;
  color: var(--text-muted);
  cursor: pointer;
  transition: var(--transition);
}

.seg-btn.active {
  background: #ffffff;
  color: var(--primary);
  box-shadow: var(--shadow-sm);
}

/* FILTERS */
.filters-bar {
  background: #ffffff;
  padding: 1.5rem;
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-sm);
  border: 1px solid #f1f5f9;
  margin-bottom: 2.5rem;
}

.filter-grid {
  display: flex;
  gap: 1.5rem;
  align-items: flex-end;
}

.filter-col {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.filter-col .label {
  font-size: 0.8rem;
  font-weight: 800;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.modern-input {
  padding: 0.8rem 1rem;
  border-radius: 12px;
  border: 1px solid #f1f5f9;
  background: #f8fafc;
  font-size: 0.95rem;
  transition: var(--transition);
}

.modern-input:focus {
  border-color: var(--primary);
  background: #ffffff;
  outline: none;
}

/* STATS GRID */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1.5rem;
  margin-bottom: 3rem;
}

.stat-card {
  padding: 1.75rem;
  border-radius: var(--radius-xl);
  background: #ffffff;
  border: 1px solid #f1f5f9;
  display: flex;
  align-items: center;
  gap: 1.5rem;
  box-shadow: var(--shadow-sm);
}

.stat-icon {
  width: 56px;
  height: 56px;
  border-radius: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
}

.stat-card.sky .stat-icon { background: #e0f2fe; }
.stat-card.emerald .stat-icon { background: #dcfce7; }
.stat-card.rose .stat-icon { background: #ffe4e6; }
.stat-card.amber .stat-icon { background: #fef3c7; }

.stat-label {
  display: block;
  font-size: 0.75rem;
  font-weight: 800;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 0.25rem;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 800;
  color: var(--text-main);
  letter-spacing: -0.02em;
}

.stat-value.danger { color: var(--danger); }

/* LAYOUT */
.ledger-layout {
  display: grid;
  grid-template-columns: 1fr 450px;
  gap: 2.5rem;
  align-items: start;
}

.section-title {
  font-size: 1.5rem;
  font-weight: 800;
  margin-bottom: 1.5rem;
  letter-spacing: -0.03em;
}

.user-groups {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.user-card {
  background: #ffffff;
  padding: 1.5rem;
  border-radius: var(--radius-lg);
  border: 1px solid #f1f5f9;
  box-shadow: var(--shadow-sm);
}

.user-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.user-main .username {
  display: block;
  font-weight: 800;
  font-size: 1.1rem;
  color: var(--text-main);
}

.user-main .visit-count {
  font-size: 0.85rem;
  color: var(--primary);
  font-weight: 700;
}

.user-totals {
  text-align: right;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.total-row {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text-muted);
}

.total-row .val {
  font-weight: 800;
  color: var(--text-main);
  margin-left: 0.5rem;
}

.total-row .val.danger { color: var(--danger); }

/* MANUAL SECTION */
.manual-card {
  background: #ffffff;
  border-radius: var(--radius-xl);
  padding: 2.5rem;
  box-shadow: var(--shadow-lg);
  border: 1px solid #f1f5f9;
  position: sticky;
  top: 2rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.net-summary {
  display: flex;
  gap: 0.5rem;
}

.pill {
  padding: 0.4rem 0.75rem;
  border-radius: 8px;
  font-size: 0.8rem;
  font-weight: 800;
}

.pill.income { background: #dcfce7; color: #166534; }
.pill.expense { background: #fee2e2; color: #991b1b; }

.manual-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
  margin-bottom: 2.5rem;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
}

.form-group label {
  display: block;
  font-size: 0.75rem;
  font-weight: 800;
  color: var(--text-muted);
  text-transform: uppercase;
  margin-bottom: 0.5rem;
}

.form-group input {
  width: 100%;
  padding: 0.8rem;
  border-radius: 12px;
  border: 1px solid #f1f5f9;
  background: #f8fafc;
}

.type-pill-group {
  display: flex;
  gap: 4px;
  background: #f1f5f9;
  padding: 4px;
  border-radius: 10px;
}

.type-pill {
  flex: 1;
  padding: 0.5rem;
  border: none;
  background: transparent;
  border-radius: 8px;
  font-size: 0.8rem;
  font-weight: 700;
  cursor: pointer;
}

.type-pill.active {
  background: #ffffff;
  color: var(--primary);
  box-shadow: var(--shadow-sm);
}

/* PREMIUM TABLE */
.premium-table {
  width: 100%;
  border-collapse: collapse;
}

.premium-table th {
  text-align: left;
  padding: 1rem 0;
  font-size: 0.75rem;
  font-weight: 800;
  color: var(--text-muted);
  text-transform: uppercase;
  border-bottom: 2px solid #f1f5f9;
}

.premium-table td {
  padding: 1rem 0;
  border-bottom: 1px solid #f1f5f9;
  font-size: 0.9rem;
}

.premium-table .cat { font-weight: 700; color: var(--text-main); }
.premium-table .badge {
  font-size: 0.7rem;
  padding: 2px 6px;
  border-radius: 6px;
  margin-left: 0.5rem;
}

.text-success { color: var(--success) !important; font-weight: 800; }
.text-danger { color: var(--danger) !important; font-weight: 800; }

.access-denied {
  text-align: center;
  padding: 5rem;
  background: #ffffff;
  border-radius: var(--radius-xl);
  border: 1px dashed #e2e8f0;
}

@media (max-width: 1024px) {
  .ledger-layout { grid-template-columns: 1fr; }
  .manual-card { position: static; }
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
    margin-bottom: 1.5rem;
  }

  .page-header h1 {
    font-size: 1.75rem !important;
    white-space: normal;
  }

  .segmented-control {
    width: 100%;
    display: flex;
    overflow-x: auto;
  }

  .seg-btn {
    flex: 1;
    padding: 0.5rem 0.4rem;
    font-size: 0.75rem;
    white-space: nowrap;
  }

  .filters-bar {
    padding: 1rem;
  }

  .filter-grid {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }

  .stats-grid {
    grid-template-columns: 1fr;
    gap: 0.75rem;
  }

  .stat-card {
    padding: 1rem;
    gap: 1rem;
  }

  .stat-icon {
    width: 44px;
    height: 44px;
    font-size: 1.25rem;
  }

  .stat-value {
    font-size: 1.25rem;
  }

  .form-grid {
    grid-template-columns: 1fr;
  }

  .user-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .user-totals {
    text-align: left;
    width: 100%;
    border-top: 1px solid #f1f5f9;
    padding-top: 0.75rem;
  }

  .ledger-table-wrapper {
    overflow-x: auto;
    margin: 0 -1rem;
    padding: 0 1rem;
  }

  .premium-table {
    min-width: 400px;
  }

  .manual-card {
    padding: 1.25rem;
  }

  .card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
    margin-bottom: 1.5rem;
  }

  .net-summary {
    width: 100%;
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }

  .pill {
    width: 100%;
    text-align: center;
    white-space: nowrap;
    font-size: 0.75rem;
    padding: 0.5rem;
  }
}

/* Very small screens */
@media (max-width: 380px) {
  .stat-card {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .stat-info {
    width: 100%;
  }

  .card-header h3 {
    font-size: 1.1rem;
  }
}
</style>
