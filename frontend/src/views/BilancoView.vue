<template>
  <div class="page">
    <header class="page-header">
      <h1>Bilanço</h1>
      <p class="subtitle">
        Günlük gelir / gider kayıtlarını tut, tarih aralığı seçerek özetini gör.
      </p>
    </header>

    <!-- ÜST ÖZET KUTULARI (manuel gelir/gider) -->
    <section class="cards">
      <div class="card kpi income">
        <h2>Toplam Gelir</h2>
        <p class="value">{{ formatAmount(totalIncome) }} TL</p>
      </div>
      <div class="card kpi expense">
        <h2>Toplam Gider</h2>
        <p class="value">{{ formatAmount(totalExpense) }} TL</p>
      </div>
      <div class="card kpi net">
        <h2>Net</h2>
        <p class="value">{{ formatAmount(netTotal) }} TL</p>
      </div>
    </section>

    <!-- FİLTRE + TARİH ARALIĞI -->
    <section class="card controls">
      <div class="date-range">
        <div class="field">
          <label>Başlangıç</label>
          <input type="date" v-model="from" />
        </div>
        <div class="field">
          <label>Bitiş</label>
          <input type="date" v-model="to" />
        </div>
        <div class="buttons">
          <button class="btn" @click="setToday">Bugün</button>
          <button class="btn" @click="setThisWeek">Bu Hafta</button>
          <button class="btn primary" @click="loadLedger">Getir</button>
        </div>
      </div>
    </section>

    <!-- YENİ KAYIT FORMU (manuel gelir/gider) -->
    <section class="card form-card">
      <h2>Yeni Kayıt Ekle</h2>

      <form @submit.prevent="submitEntry" class="ledger-form">
        <div class="field-row">
          <div class="field">
            <label>Tarih</label>
            <input type="date" v-model="form.date" required />
          </div>

          <div class="field">
            <label>Tutar (TL)</label>
            <input
              type="number"
              step="0.01"
              v-model.number="form.amount"
              required
            />
          </div>

          <div class="field">
            <label>Tür</label>
            <div class="radio-row">
              <label>
                <input
                  type="radio"
                  value="income"
                  v-model="form.type"
                />
                Gelir
              </label>
              <label>
                <input
                  type="radio"
                  value="expense"
                  v-model="form.type"
                />
                Gider
              </label>
            </div>
          </div>
        </div>

        <div class="field-row">
          <div class="field flex-2">
            <label>Kategori</label>
            <input
              type="text"
              v-model="form.category"
              placeholder="Örn: Muayene, Mama, Kira..."
            />
          </div>

          <div class="field flex-3">
            <label>Not</label>
            <input
              type="text"
              v-model="form.note"
              placeholder="İsteğe bağlı açıklama"
            />
          </div>
        </div>

        <div class="actions-row">
          <button
            type="submit"
            class="btn primary"
            :disabled="saving"
          >
            Kaydet
          </button>
        </div>
      </form>
    </section>

    <!-- ZİYARETLERDEN GELEN ÖZET -->
    <div v-if="summary" class="cards summary-cards">
      <div class="card summary-card">
        <h3>Ziyaret Toplam Tutar</h3>
        <p class="summary-subtitle">Ziyaret kartlarındaki işlem tutarları</p>
        <p class="summary-value">
          {{ summary.totalAmount.toLocaleString('tr-TR') }} TL
        </p>
      </div>
      <div class="card summary-card">
        <h3>Ziyaret Tahsil</h3>
        <p class="summary-subtitle">“Ne kadar alındı” alanlarının toplamı</p>
        <p class="summary-value">
          {{ summary.totalCollected.toLocaleString('tr-TR') }} TL
        </p>
      </div>
      <div class="card summary-card">
        <h3>Ziyaret Veresiye</h3>
        <p class="summary-subtitle">Veresiye kalan tutar</p>
        <p class="summary-value">
          {{ summary.totalCredit.toLocaleString('tr-TR') }} TL
        </p>
      </div>
      <div class="card summary-card">
        <h3>Ziyaret Sayısı</h3>
        <p class="summary-subtitle">Seçili aralıktaki toplam ziyaret</p>
        <p class="summary-value">
          {{ summary.visitCount }}
        </p>
      </div>
    </div>

    <!-- ZİYARETLERDEN GELİR DETAY TABLOLARI – EKLEYEN KULLANICIYA GÖRE -->
    <section v-if="isBullBoss" class="doctor-grid">
      <div
        v-for="group in visitUserGroups"
        :key="group.username"
        class="card detail-card doctor-card"
      >
        <div class="card-header">
          <div>
            <h2>{{ group.fullName }}</h2>
            <span class="small">({{ from }} – {{ to }})</span>
          </div>
          <p class="doctor-subtitle">Ziyaretlerden Oluşan Gelirler</p>
        </div>

        <div class="doctor-summary-row">
          <div class="summary-pill">
            Toplam: {{ formatAmount(group.summary.totalAmount) }} TL
          </div>
          <div class="summary-pill">
            Tahsil: {{ formatAmount(group.summary.totalCollected) }} TL
          </div>
          <div class="summary-pill">
            Veresiye: {{ formatAmount(group.summary.totalCredit) }} TL
          </div>
          <div class="summary-pill">
            Ziyaret: {{ group.summary.visitCount }}
          </div>
        </div>

        <table
          v-if="group.items && group.items.length"
          class="table detail-table"
        >
          <thead>
            <tr>
              <th>Tarih</th>
              <th>Hasta</th>
              <th>Sahip</th>
              <th class="amount">Toplam</th>
              <th class="amount">Alınan</th>
              <th class="amount">Veresiye</th>
              <th>Ekleyen</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in group.items" :key="r.visitId">
              <td data-label="Tarih">
                {{ new Date(r.performedAt).toLocaleDateString('tr-TR') }}
              </td>
              <td data-label="Hasta">
                {{ r.petName }}
              </td>
              <td data-label="Sahip">
                {{ r.ownerName }}
              </td>
              <td class="amount" data-label="Toplam">
                {{ r.totalAmount.toLocaleString('tr-TR') }} TL
              </td>
              <td class="amount" data-label="Alınan">
                {{ r.collectedAmount.toLocaleString('tr-TR') }} TL
              </td>
              <td class="amount" data-label="Veresiye">
                {{ r.creditAmount.toLocaleString('tr-TR') }} TL
              </td>
              <td data-label="Ekleyen">
                {{ r.createdByName || r.createdByUsername }}
              </td>
            </tr>
          </tbody>
        </table>

        <div v-else class="state">
          Bu aralıkta kayıt yok.
        </div>
      </div>
    </section>

    <!-- MANUEL GELİR/GİDER LİSTESİ -->
    <section class="card list-card">
      <div class="card-header">
        <h2>
          Kayıtlar
          <span class="small">
            ({{ from }} – {{ to }})
          </span>
        </h2>
      </div>

      <div v-if="loading" class="state">Yükleniyor...</div>
      <div v-else-if="entries.length === 0" class="state">
        Bu aralıkta kayıt yok.
      </div>

      <table v-else class="table">
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
              <span
                class="badge"
                :class="e.isIncome ? 'badge-income' : 'badge-expense'"
              >
                {{ e.isIncome ? 'Gelir' : 'Gider' }}
              </span>
            </td>
            <td>{{ e.category || '—' }}</td>
            <td>{{ e.note || '—' }}</td>
            <td class="right">
              {{ formatAmount(e.amount) }} TL
            </td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getUser } from '@/utils/auth'
import {
  fetchLedgerSummary,
  fetchLedgerItems,
  createLedgerEntry,
  fetchLedgerRange,
} from '@/api/ledger'

const router = useRouter()

const rawUser = getUser()
const isBullBoss = computed(() => rawUser?.username === 'BullBoss')

// --- GENEL STATE ---
const entries = ref([])
const loading = ref(false)
const saving = ref(false)
const error = ref('')

// Ziyaret özeti (tüm ziyaretler)
const summary = ref(null)

// Tüm ziyaret satırları (ekleyen kullanıcıya göre gruplayacağız)
const allVisitItems = ref([])

// --- TARİH ARALIĞI ---
function toIsoDate(date) {
  return date.toISOString().slice(0, 10)
}

const today = new Date()
const from = ref(toIsoDate(today))
const to = ref(toIsoDate(today))

// FORM STATE (manuel gelir/gider)
const form = ref({
  date: toIsoDate(today),
  amount: null,
  type: 'income',
  category: '',
  note: '',
})

// === COMPUTED TOPLAM GELİR / GİDER (manuel ledger kayıtlarından) ===
const totalIncome = computed(() =>
  entries.value
    .filter(e => e.isIncome)
    .reduce((sum, e) => sum + Number(e.amount || 0), 0),
)

const totalExpense = computed(() =>
  entries.value
    .filter(e => !e.isIncome)
    .reduce((sum, e) => sum + Number(e.amount || 0), 0),
)

const netTotal = computed(() => totalIncome.value - totalExpense.value)

// === ZİYARETLERİ EKLEYEN KULLANICIYA GÖRE GRUPLA ===
const visitUserGroups = computed(() => {
  const map = new Map()

  for (const row of allVisitItems.value) {
    const key = row.createdByUsername || 'unknown'
    const fullName = row.createdByName || row.createdByUsername || 'Bilinmiyor'

    if (!map.has(key)) {
      map.set(key, {
        username: key,
        fullName,
        items: [],
      })
    }
    map.get(key).items.push(row)
  }

  const result = []

  for (const group of map.values()) {
    let totalAmount = 0
    let totalCollected = 0
    let totalCredit = 0

    for (const v of group.items) {
      totalAmount += v.totalAmount || 0
      totalCollected += v.collectedAmount || 0
      totalCredit += v.creditAmount || 0
    }

    result.push({
      ...group,
      summary: {
        totalAmount,
        totalCollected,
        totalCredit,
        visitCount: group.items.length,
      },
    })
  }

  // İsimlere göre sırala
  return result.sort((a, b) =>
    a.fullName.localeCompare(b.fullName, 'tr'),
  )
})

// === HELPERLAR ===
function formatAmount(value) {
  return Number(value || 0).toLocaleString('tr-TR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })
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

// === BACKEND'TEN VERİ ÇEKME ===
async function loadLedger() {
  // Sadece BullBoss bu sayfayı kullanacağı için,
  // yine de güvenlik için kontrol.
  if (!isBullBoss.value) {
    loading.value = false
    return
  }

  loading.value = true
  error.value = ''

  try {
    const [summaryRes, visitItems, rangeData] = await Promise.all([
      fetchLedgerSummary(from.value, to.value),
      fetchLedgerItems(from.value, to.value),
      fetchLedgerRange(from.value, to.value),
    ])

    summary.value = summaryRes
    allVisitItems.value = visitItems
    entries.value = rangeData
  } catch (e) {
    console.error('loadLedger hata', e)
    error.value = 'Bilanço verileri yüklenirken bir hata oluştu.'
  } finally {
    loading.value = false
  }
}

// === YENİ MANUEL GELİR/GİDER KAYDI ===
async function submitEntry() {
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

    if (created.date >= from.value && created.date <= to.value) {
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

// === SAYFA AÇILIRKEN ===
onMounted(() => {
  // Sadece BullBoss bu sayfayı görebilsin
  if (!isBullBoss.value) {
    router.push({ name: 'dashboard' })
    return
  }

  const now = new Date()
  const start = new Date(now.getFullYear(), now.getMonth(), 1)
  const end = new Date(now.getFullYear(), now.getMonth() + 1, 0)

  from.value = toIsoDate(start)
  to.value = toIsoDate(end)

  loadLedger()
})
</script>

<style scoped>
.page {
  width: 100%;
  max-width: 1024px;
  margin: 0 auto;
  padding: 1rem;
}

.page-header {
  margin-bottom: 1rem;
}

.subtitle {
  margin: 0.25rem 0 0;
  font-size: 0.85rem;
  color: #6b7280;
}

/* KARTLAR */
.cards {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.card {
  background: #ffffff;
  border-radius: 0.75rem;
  padding: 1rem;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);
}

.kpi {
  text-align: center;
}

.kpi h2 {
  margin: 0;
  font-size: 0.9rem;
  color: #4b5563;
}

.kpi .value {
  margin-top: 0.25rem;
  font-size: 1.6rem;
  font-weight: 700;
}

.kpi.income {
  border-top: 3px solid #22c55e;
}

.kpi.expense {
  border-top: 3px solid #ef4444;
}

.kpi.net {
  border-top: 3px solid #0ea5e9;
}

/* KONTROLLER */
.controls .date-range {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  align-items: flex-end;
}

.field {
  display: flex
;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.85rem;
}

.field label {
  font-weight: 600;
  color: #4b5563;
}

.field input {
  border-radius: 0.5rem;
  border: 1px solid #d1d5db;
  padding: 0.35rem 0.5rem;
  font-size: 0.85rem;
}

.buttons {
  display: flex;
  gap: 0.5rem;
}

.btn {
  border: none;
  padding: 0.35rem 0.9rem;
  border-radius: 999px;
  font-size: 0.8rem;
  cursor: pointer;
  background: #e5e7eb;
}

.btn.primary {
  background: #111827;
  color: #fff;
}

/* FORM */
.form-card h2 {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}

.ledger-form {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.field-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.flex-2 {
  flex: 2;
}

.flex-3 {
  flex: 3;
}

.radio-row {
  display: flex;
  gap: 0.75rem;
  font-size: 0.8rem;
}

.actions-row {
  display: flex;
  justify-content: flex-end;
}

/* ORTA ÖZET VE DETAY */
.summary-cards {
  margin-top: 0.75rem;
  margin-bottom: 0.75rem;
}

.detail-card {
  margin-top: 0.5rem;
}

/* ORTAK TABLO STİLLERİ */
.table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}

.table th,
.table td {
  padding: 0.35rem 0.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.table th {
  text-align: left;
  font-weight: 600;
  color: #4b5563;
  font-size: 0.8rem;
}

.table td.right,
.table th.right {
  text-align: right;
}

.detail-table thead tr {
  background: #f3f4f6;
}

.detail-table th.amount,
.detail-table td.amount {
  text-align: right;
  white-space: nowrap;
}

.detail-table tbody tr:nth-child(odd) {
  background: #f9fafb;
}

/* LİSTE KARTI */
.list-card {
  margin-top: 1rem;
}

.card-header {
  margin-bottom: 0.5rem;
  display: flex;
  justify-content: space-between;
}

.card-header .small {
  font-size: 0.8rem;
  color: #6b7280;
}

.state {
  font-size: 0.9rem;
  color: #6b7280;
  padding: 0.4rem 0;
}

/* BADGE’LER */
.badge {
  display: inline-block;
  padding: 0.1rem 0.4rem;
  border-radius: 999px;
  font-size: 0.7rem;
}

.badge-income {
  background: #dcfce7;
  color: #166534;
}

.badge-expense {
  background: #fee2e2;
  color: #b91c1c;
}

/* DOKTOR/KULLANICI TABLOLARI GRIDİ */
.doctor-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
  margin-top: 0.5rem;
}

.doctor-card {
  padding: 0.9rem;
}

.doctor-subtitle {
  margin: 0;
  font-size: 0.8rem;
  color: #6b7280;
}

.doctor-summary-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
  margin-bottom: 0.5rem;
  margin-top: 0.25rem;
}

.summary-pill {
  font-size: 0.75rem;
  padding: 0.15rem 0.6rem;
  border-radius: 999px;
  background: #f3f4f6;
  color: #374151;
}

/* RESPONSIVE */
@media (max-width: 900px) {
  .cards {
    grid-template-columns: 1fr;
  }

  .controls .date-range {
    align-items: stretch;
  }

  .doctor-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 700px) {
  .detail-card {
    padding: 0.75rem;
  }

  .detail-table {
    border: 0;
  }

  .detail-table thead {
    display: none;
  }

  .detail-table tbody tr {
    display: block;
    margin-bottom: 0.75rem;
    padding: 0.75rem 0.85rem;
    border-radius: 0.75rem;
    background: #f9fafb;
    box-shadow: 0 8px 20px rgba(15, 23, 42, 0.06);
  }

  .detail-table tbody tr:last-child {
    margin-bottom: 0;
  }

  .detail-table td {
    display: flex;
    justify-content: space-between;
    align-items: baseline;
    border-bottom: none;
    padding: 0.15rem 0;
    font-size: 0.8rem;
  }

  .detail-table td::before {
    content: attr(data-label);
    font-weight: 600;
    color: #6b7280;
    margin-right: 0.75rem;
  }

  .detail-table td.amount {
    font-variant-numeric: tabular-nums;
  }
}
</style>
