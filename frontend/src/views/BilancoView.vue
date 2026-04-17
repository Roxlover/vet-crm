<template>
  <div class="page">
    <header class="page-header">
      <div class="header-left">
        <h1>Bilanço</h1>
      </div>

      <div class="header-right" v-if="canAccessBilanco">
        <div class="segmented" role="tablist" aria-label="Rapor periyodu">
          <button
            class="seg-btn"
            :class="{ active: period === 'day' }"
            type="button"
            @click="setPeriod('day')"
          >
            Gün
          </button>
          <button
            class="seg-btn"
            :class="{ active: period === 'week' }"
            type="button"
            @click="setPeriod('week')"
          >
            Hafta
          </button>
          <button
            class="seg-btn"
            :class="{ active: period === 'month' }"
            type="button"
            @click="setPeriod('month')"
          >
            Ay
          </button>
          <button
            class="seg-btn"
            :class="{ active: period === 'year' }"
            type="button"
            @click="setPeriod('year')"
          >
            Yıl
          </button>
        </div>
      </div>
    </header>

    <div v-if="!canAccessBilanco" class="card state">
      Bu sayfaya erişim yetkiniz yok.
    </div>

    <template v-else>
      <!-- FILTER BAR -->
      <section class="card filter-card">
        <div class="filter-grid">
          <div class="filter-col">
            <div class="label">Başlangıç</div>
            <input class="input" type="date" v-model="from" />
          </div>

          <div class="filter-col">
            <div class="label">Bitiş</div>
            <input class="input" type="date" v-model="to" />
          </div>

          <div class="filter-col grow">
            <div class="label">Hızlı</div>
            <div class="quick-row">
              <button class="btn" type="button" @click="setToday">Bugün</button>
              <button class="btn" type="button" @click="setThisWeek">Bu Hafta</button>
              <button class="btn" type="button" @click="setThisMonth">Bu Ay</button>
              <button class="btn" type="button" @click="setThisYear">Bu Yıl</button>
            </div>
          </div>

          <div class="filter-col end">
            <div class="label">&nbsp;</div>
            <button class="btn primary" type="button" @click="loadLedger" :disabled="loading">
              {{ loading ? 'Yükleniyor...' : 'Getir' }}
            </button>
          </div>
        </div>

        <p v-if="error" class="state state-error">{{ error }}</p>
      </section>

      <!-- VISIT SUMMARY KPI -->
      <section class="kpi-grid">
        <div class="card kpi">
          <div class="kpi-title">Ziyaret Toplam</div>
          <div class="kpi-value">{{ fmtMoney(summaryTotalAmount) }}</div>
          <div class="kpi-sub">Seçili aralıktaki toplam tutar</div>
        </div>

        <div class="card kpi">
          <div class="kpi-title">Tahsil</div>
          <div class="kpi-value">{{ fmtMoney(summaryTotalCollected) }}</div>
          <div class="kpi-sub">Alınan toplam</div>
        </div>

        <div class="card kpi">
          <div class="kpi-title">Veresiye</div>
          <div class="kpi-value">{{ fmtMoney(summaryTotalCredit) }}</div>
          <div class="kpi-sub">Kalan tutar</div>
        </div>

        <div class="card kpi">
          <div class="kpi-title">Ziyaret</div>
          <div class="kpi-value">{{ summaryVisitCount }}</div>
          <div class="kpi-sub">Seçili aralıktaki ziyaret sayısı</div>
        </div>
      </section>

      <!-- BY USER / DOCTOR GROUPS -->
      <section class="card section-card">
        <div class="section-head">
          <div>
            <h2 class="h2">Yapılan İşlemler (Kullanıcıya Göre)</h2>
            <div class="muted">({{ from }} – {{ to }})</div>
          </div>

          <div class="mini">
            <span class="pill">Toplam: {{ fmtMoney(summaryTotalAmount) }}</span>
            <span class="pill">Tahsil: {{ fmtMoney(summaryTotalCollected) }}</span>
            <span class="pill">Veresiye: {{ fmtMoney(summaryTotalCredit) }}</span>
          </div>
        </div>

        <div v-if="!visitUserGroups || !visitUserGroups.length" class="state">
          Bu aralıkta ziyaret verisi yok.
        </div>

        <div v-else class="group-grid">
          <article
            v-for="group in visitUserGroups"
            :key="groupKey(group)"
            class="card group-card"
          >
            <div class="group-head">
              <div>
                <div class="group-name">
                  {{ group.fullName || group.FullName || group.username || group.Username || '—' }}
                </div>
                <div class="muted small">
                  {{ (group.summary || group.Summary)?.visitCount ?? (group.summary || group.Summary)?.VisitCount ?? 0 }}
                  ziyaret
                </div>
              </div>

              <div class="group-kpis">
                <div class="gk">
                  <div class="gk-l">Toplam</div>
                  <div class="gk-v">{{ fmtMoney(groupTotalAmount(group)) }}</div>
                </div>
                <div class="gk">
                  <div class="gk-l">Tahsil</div>
                  <div class="gk-v">{{ fmtMoney(groupTotalCollected(group)) }}</div>
                </div>
                <div class="gk">
                  <div class="gk-l">Veresiye</div>
                  <div class="gk-v">{{ fmtMoney(groupTotalCredit(group)) }}</div>
                </div>
              </div>
            </div>

            <div class="divider"></div>

            <div v-if="!(group.items || group.Items)?.length" class="state">
              Bu kullanıcı için kayıt yok.
            </div>

            <div v-else class="items">
              <div
                v-for="r in (group.items || group.Items)"
                :key="r.visitId || r.VisitId"
                class="item-row"
              >
                <div class="item-left">
                  <div class="item-top">
                    <div class="item-date">{{ fmtDate(r.performedAt || r.PerformedAt) }}</div>
                    <div class="item-names">
                      <span class="strong">{{ r.petName || r.PetName }}</span>
                      <span class="dot">•</span>
                      <span>{{ r.ownerName || r.OwnerName }}</span>
                    </div>
                  </div>

                  <!-- Purpose/Procedures/Notes: backend ekleyince otomatik görünür -->
                  <div
                    v-if="(r.purpose || r.Purpose || r.procedures || r.Procedures || r.notes || r.Notes)"
                    class="item-meta"
                  >
                    <div v-if="(r.purpose || r.Purpose)" class="meta-line">
                      <span class="meta-k">Amaç:</span>
                      <span class="meta-v">{{ r.purpose || r.Purpose }}</span>
                    </div>
                    <div v-if="(r.procedures || r.Procedures)" class="meta-line">
                      <span class="meta-k">İşlem:</span>
                      <span class="meta-v">{{ r.procedures || r.Procedures }}</span>
                    </div>
                    <div v-if="(r.notes || r.Notes)" class="meta-line">
                      <span class="meta-k">Not:</span>
                      <span class="meta-v">{{ r.notes || r.Notes }}</span>
                    </div>
                  </div>
                </div>

                <div class="item-right">
                  <div class="money-col">
                    <div class="mrow">
                      <span class="mkey">Toplam</span>
                      <span class="mval">{{ fmtMoney(r.totalAmount ?? r.TotalAmount) }}</span>
                    </div>
                    <div class="mrow">
                      <span class="mkey">Tahsil</span>
                      <span class="mval">{{ fmtMoney(r.collectedAmount ?? r.CollectedAmount) }}</span>
                    </div>
                    <div class="mrow">
                      <span class="mkey">Veresiye</span>
                      <span class="mval">{{ fmtMoney(r.creditAmount ?? r.CreditAmount) }}</span>
                    </div>
                  </div>

                  <div class="muted tiny" v-if="r.createdByName || r.CreatedByName || r.createdByUsername || r.CreatedByUsername">
                    Ekleyen: {{ r.createdByName || r.CreatedByName || r.createdByUsername || r.CreatedByUsername }}
                  </div>
                </div>
              </div>
            </div>
          </article>
        </div>
      </section>

      <!-- MANUAL LEDGER (income/expense) -->
      <section class="card section-card">
        <div class="section-head">
          <div>
            <h2 class="h2">Manuel Gelir / Gider</h2>
            <div class="muted">Elle eklenen kayıtlar ayrı tutulur ve devam eder.</div>
          </div>

          <div class="mini">
            <span class="pill">Gelir: {{ fmtMoney(totalIncome) }}</span>
            <span class="pill">Gider: {{ fmtMoney(totalExpense) }}</span>
            <span class="pill">Net: {{ fmtMoney(netTotal) }}</span>
          </div>
        </div>

        <div class="manual-grid">
          <!-- Create Form -->
          <div class="card inner">
            <h3 class="h3">Yeni Kayıt Ekle</h3>

            <form @submit.prevent="submitEntry" class="form">
              <div class="form-row">
                <div class="field">
                  <div class="label">Tarih</div>
                  <input class="input" type="date" v-model="form.date" required />
                </div>

                <div class="field">
                  <div class="label">Tutar (TL)</div>
                  <input class="input" type="number" step="0.01" v-model.number="form.amount" required />
                </div>

                <div class="field">
                  <div class="label">Tür</div>
                  <div class="radio">
                    <label><input type="radio" value="income" v-model="form.type" /> Gelir</label>
                    <label><input type="radio" value="expense" v-model="form.type" /> Gider</label>
                  </div>
                </div>
              </div>

              <div class="form-row">
                <div class="field grow">
                  <div class="label">Kategori</div>
                  <input class="input" type="text" v-model="form.category" placeholder="Örn: Kira, Mama, Muayene..." />
                </div>
                <div class="field grow">
                  <div class="label">Not</div>
                  <input class="input" type="text" v-model="form.note" placeholder="İsteğe bağlı" />
                </div>
              </div>

              <div class="actions">
                <button class="btn primary" type="submit" :disabled="saving">
                  {{ saving ? 'Kaydediliyor...' : 'Kaydet' }}
                </button>
              </div>
            </form>
          </div>

          <!-- List -->
          <div class="card inner">
            <h3 class="h3">Kayıtlar</h3>

            <div v-if="loading" class="state">Yükleniyor...</div>
            <div v-else-if="entries.length === 0" class="state">Bu aralıkta kayıt yok.</div>

            <div v-else class="table-wrap">
              <table class="table">
                <thead>
                  <tr>
                    <th>Tarih</th>
                    <th>Tür</th>
                    <th>Kategori</th>
                    <th>Not</th>
                    <th class="right">Tutar</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="e in entries" :key="e.id">
                    <td>{{ e.date }}</td>
                    <td>
                      <span class="badge" :class="e.isIncome ? 'b-income' : 'b-expense'">
                        {{ e.isIncome ? 'Gelir' : 'Gider' }}
                      </span>
                    </td>
                    <td>{{ e.category || '—' }}</td>
                    <td class="muted">{{ e.note || '—' }}</td>
                    <td class="right">{{ fmtMoney(e.amount) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </section>
    </template>
  </div>
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

// AUTH
const user = computed(() => getUser() || null)
const canAccessBilanco = computed(() => {
  const u = user.value || {}
  const role = String(u.role || '').trim().toLowerCase()
  const username = String(u.username || '').trim().toLowerCase()
  return role === 'admin' || username === 'bullboss'
})

// STATE
const entries = ref([])
const visitUserGroups = ref([])
const summary = ref(null)

const loading = ref(false)
const saving = ref(false)
const error = ref('')

// Period (day/week/month/year) – sadece UI/kolay seçim, backend aynı from/to ile çalışır
const period = ref('month')

// DATE
function toIsoDate(date) {
  return date.toISOString().slice(0, 10)
}
const today = new Date()
const from = ref(toIsoDate(today))
const to = ref(toIsoDate(today))

// Manual form
const form = ref({
  date: toIsoDate(today),
  amount: null,
  type: 'income',
  category: '',
  note: '',
})

// Computed manual totals
const totalIncome = computed(() =>
  (entries.value || []).filter(e => e?.isIncome).reduce((s, e) => s + Number(e?.amount || 0), 0),
)
const totalExpense = computed(() =>
  (entries.value || []).filter(e => !e?.isIncome).reduce((s, e) => s + Number(e?.amount || 0), 0),
)
const netTotal = computed(() => totalIncome.value - totalExpense.value)

// Summary helpers (visit based)
const summaryTotalAmount = computed(() => Number((summary.value?.totalAmount ?? summary.value?.TotalAmount) || 0))
const summaryTotalCollected = computed(() => Number((summary.value?.totalCollected ?? summary.value?.TotalCollected) || 0))
const summaryTotalCredit = computed(() => Number((summary.value?.totalCredit ?? summary.value?.TotalCredit) || 0))
const summaryVisitCount = computed(() => Number((summary.value?.visitCount ?? summary.value?.VisitCount) || 0))

function fmtMoney(value) {
  const n = Number(value ?? 0)
  return `${n.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} TL`
}

function fmtDate(iso) {
  try {
    return new Date(iso).toLocaleString('tr-TR')
  } catch {
    return String(iso || '')
  }
}

function groupKey(group) {
  return group.username || group.Username || group.userId || group.UserId || JSON.stringify(group)
}
function groupSummary(group) {
  return (group.summary || group.Summary) || {}
}
function groupTotalAmount(group) {
  const s = groupSummary(group)
  return Number((s.totalAmount ?? s.TotalAmount) || 0)
}
function groupTotalCollected(group) {
  const s = groupSummary(group)
  return Number((s.totalCollected ?? s.TotalCollected) || 0)
}
function groupTotalCredit(group) {
  const s = groupSummary(group)
  return Number((s.totalCredit ?? s.TotalCredit) || 0)
}

// Period setters
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
  const day = now.getDay() || 7 // Pazar 0 -> 7
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

// Fetch
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
    console.error('loadLedger hata', e)
    error.value = 'Bilanço verileri yüklenirken bir hata oluştu.'
  } finally {
    loading.value = false
  }
}

// Create manual entry
async function submitEntry() {
  if (!canAccessBilanco.value) return
  if (!form.value.date || !form.value.amount) {
    alert('Tarih ve tutar zorunlu.')
    return
  }

  const payload = {
    date: form.value.date,
    amount: Number(form.value.amount),
    isIncome: form.value.type === 'income',
    category: form.value.category || null,
    note: form.value.note || null,
  }

  saving.value = true
  try {
    const created = await createLedgerEntry(payload)
    if (created?.date && created.date >= from.value && created.date <= to.value) {
      entries.value.unshift(created)
    }
    form.value.amount = null
    form.value.category = ''
    form.value.note = ''
    form.value.type = 'income'
  } catch (e) {
    console.error('create ledger entry error', e)
    alert('Kayıt eklenirken bir hata oluştu.')
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  if (!canAccessBilanco.value) {
    router.push({ name: 'dashboard' })
    return
  }
  // default: month
  setThisMonth()
})
</script>

<style scoped>
/* Light background, dark text */
.page {
  width: 100%;
  max-width: 1200px;
  margin: 0 auto;
  padding: 1rem;
  color: #111827;
}

.page-header {
  display: flex;
  gap: 1rem;
  align-items: flex-start;
  justify-content: space-between;
  margin-bottom: 1rem;
}
.page {
  overflow-x: hidden;
}

.manual-grid {
  width: 100%;
  max-width: 100%;
}

.manual-grid > .card.inner {
  min-width: 0;      /* GRID için kritik */
  max-width: 100%;
}

.manual-grid .input,
.manual-grid input,
.manual-grid textarea,
.manual-grid select {
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
}

.table-wrap {
  width: 100%;
  max-width: 100%;
  overflow-x: auto;
  overflow-y: hidden;
  -webkit-overflow-scrolling: touch;
}

.table-wrap .table {
  width: 100%;
  border-collapse: collapse;
}

@media (max-width: 700px) {
  .table-wrap .table {
    min-width: 640px; /* kolonlar sıkışmasın, scroll çalışsın */
  }

  .table-wrap .table th,
  .table-wrap .table td {
    white-space: nowrap;
  }

  .table-wrap .table td:nth-child(3),
  .table-wrap .table td:nth-child(4) {
    white-space: normal;
    word-break: break-word;
  }
}

.table {
  width: 100%;
  max-width: 100%;
}

@media (max-width: 700px) {
  .page {
    padding: 0.75rem;
  }

  .field-row {
    flex-direction: column;
  }

  .field,
  .flex-2,
  .flex-3 {
    width: 100%;
    flex: 1 1 auto;
    min-width: 0;
  }

  .field input {
    width: 100%;
    box-sizing: border-box;
  }


}
.table-wrap .table {
  table-layout: auto;
}

@media (max-width: 700px) {
  .controls .date-range {
    flex-direction: column;
    align-items: stretch;
  }

  .buttons {
    flex-wrap: wrap;
  }

  .btn {
    width: 100%;
  }
}

.header-left h1 {
  margin: 0;
  font-size: 1.4rem;
  letter-spacing: -0.02em;
}

.subtitle {
  margin: 0.35rem 0 0;
  font-size: 0.9rem;
  color: #374151;
  max-width: 70ch;
}

.card {
  background: #ffffff;
  border-radius: 0.9rem;
  padding: 1rem;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);
  border: 1px solid rgba(17, 24, 39, 0.06);
}

.state {
  color: #374151;
  font-size: 0.95rem;
}
.state-error {
  color: #b91c1c;
}

/* Segmented */
.segmented {
  display: inline-flex;
  background: #f3f4f6;
  border-radius: 999px;
  padding: 0.2rem;
  border: 1px solid rgba(17, 24, 39, 0.08);
}
.seg-btn {
  border: 0;
  background: transparent;
  padding: 0.45rem 0.85rem;
  border-radius: 999px;
  font-size: 0.85rem;
  color: #111827;
  cursor: pointer;
}
.seg-btn.active {
  background: #111827;
  color: #ffffff;
}

/* Filter */
.filter-card {
  margin-bottom: 1rem;
}
.filter-grid {
  display: grid;
  grid-template-columns: 170px 170px 1fr 140px;
  gap: 0.75rem;
  align-items: end;
}
.label {
  font-size: 0.78rem;
  font-weight: 700;
  color: #374151;
  margin-bottom: 0.35rem;
}
.input {
  width: 100%;
  border-radius: 0.65rem;
  border: 1px solid rgba(17, 24, 39, 0.18);
  padding: 0.5rem 0.6rem;
  font-size: 0.9rem;
  color: #111827;
  background: #ffffff;
}
.quick-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}
.btn {
  border: 1px solid rgba(17, 24, 39, 0.12);
  background: #f3f4f6;
  color: #111827;
  padding: 0.5rem 0.85rem;
  border-radius: 999px;
  font-size: 0.85rem;
  cursor: pointer;
}
.btn.primary {
  background: #111827;
  color: #ffffff;
  border-color: #111827;
}
.filter-col.end {
  display: flex;
  justify-content: flex-end;
}

/* KPI */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 0.85rem;
  margin-bottom: 1rem;
}
.kpi-title {
  font-size: 0.85rem;
  color: #374151;
  font-weight: 700;
}
.kpi-value {
  margin-top: 0.35rem;
  font-size: 1.5rem;
  font-weight: 800;
  letter-spacing: -0.02em;
}
.kpi-sub {
  margin-top: 0.25rem;
  font-size: 0.8rem;
  color: #6b7280;
}

/* Sections */
.section-card {
  margin-bottom: 1rem;
}
.section-head {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: flex-start;
  margin-bottom: 0.75rem;
}
.h2 {
  margin: 0;
  font-size: 1.05rem;
}
.h3 {
  margin: 0 0 0.75rem;
  font-size: 1rem;
}
.muted {
  color: #6b7280;
}
.small { font-size: 0.85rem; }
.tiny { font-size: 0.78rem; }
.strong { font-weight: 800; }
.dot { margin: 0 0.35rem; color: #9ca3af; }

.mini {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  justify-content: flex-end;
}
.pill {
  background: #f3f4f6;
  border: 1px solid rgba(17, 24, 39, 0.08);
  padding: 0.35rem 0.65rem;
  border-radius: 999px;
  font-size: 0.82rem;
  color: #111827;
}

/* Groups */
.group-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.85rem;
}
.group-card {
  padding: 0.9rem;
}
.group-head {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: flex-start;
}
.group-name {
  font-size: 1rem;
  font-weight: 900;
  color: #111827;
}
.group-kpis {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.5rem;
}
.gk {
  background: #f9fafb;
  border: 1px solid rgba(17, 24, 39, 0.06);
  border-radius: 0.75rem;
  padding: 0.55rem 0.6rem;
  min-width: 110px;
  text-align: right;
}
.gk-l {
  font-size: 0.72rem;
  color: #6b7280;
  font-weight: 700;
}
.gk-v {
  margin-top: 0.15rem;
  font-size: 0.92rem;
  font-weight: 900;
  color: #111827;
}
.divider {
  height: 1px;
  background: rgba(17, 24, 39, 0.08);
  margin: 0.75rem 0;
}

/* Items list */
.items {
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}
.item-row {
  display: grid;
  grid-template-columns: 1fr 280px;
  gap: 0.85rem;
  padding: 0.7rem 0.75rem;
  border-radius: 0.85rem;
  background: #f9fafb;
  border: 1px solid rgba(17, 24, 39, 0.06);
}
.item-date {
  font-weight: 900;
  color: #111827;
  font-size: 0.9rem;
}
.item-names {
  margin-top: 0.25rem;
  font-size: 0.88rem;
  color: #111827;
}
.item-meta {
  margin-top: 0.45rem;
  display: grid;
  gap: 0.25rem;
}
.meta-line {
  display: grid;
  grid-template-columns: 60px 1fr;
  gap: 0.5rem;
  font-size: 0.82rem;
  color: #374151;
}
.meta-k {
  color: #6b7280;
  font-weight: 800;
}
.meta-v {
  color: #111827;
  font-weight: 600;
}

.money-col {
  display: grid;
  gap: 0.25rem;
}
.mrow {
  display: flex;
  justify-content: space-between;
  gap: 0.75rem;
  font-size: 0.85rem;
}
.mkey {
  color: #6b7280;
  font-weight: 800;
}
.mval {
  color: #111827;
  font-weight: 900;
  white-space: nowrap;
}

/* Manual grid */
.manual-grid {
  display: grid;
  grid-template-columns: 1fr 1.2fr;
  gap: 0.85rem;
}
.inner {
  background: #ffffff;
}
.form {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}
.form-row {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
}
.field {
  flex: 1;
  min-width: 180px;
}
.field.grow { flex: 2; min-width: 220px; }
.radio {
  display: flex;
  gap: 0.75rem;
  font-size: 0.9rem;
  color: #111827;
}
.actions {
  display: flex;
  justify-content: flex-end;
}

.table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.9rem;
}
.table th,
.table td {
  padding: 0.5rem 0.55rem;
  border-bottom: 1px solid rgba(17, 24, 39, 0.08);
}
.table th {
  text-align: left;
  color: #374151;
  font-size: 0.82rem;
  font-weight: 900;
  background: #f9fafb;
}
.right { text-align: right; }

.badge {
  display: inline-block;
  padding: 0.2rem 0.55rem;
  border-radius: 999px;
  font-size: 0.78rem;
  font-weight: 900;
  border: 1px solid rgba(17, 24, 39, 0.08);
}
.b-income { background: #dcfce7; color: #14532d; }
.b-expense { background: #fee2e2; color: #7f1d1d; }

/* Responsive */
@media (max-width: 980px) {
  .filter-grid {
    grid-template-columns: 1fr 1fr;
  }
  .filter-col.grow { grid-column: 1 / -1; }
  .filter-col.end { grid-column: 1 / -1; justify-content: flex-start; }
  .kpi-grid { grid-template-columns: 1fr 1fr; }
  .group-grid { grid-template-columns: 1fr; }
  .manual-grid { grid-template-columns: 1fr; }
  .item-row { grid-template-columns: 1fr; }
  .group-kpis { grid-template-columns: 1fr; }
  .gk { text-align: left; min-width: 0; }
}
</style>
