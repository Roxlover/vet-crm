<template>
  <div class="page">
    <header class="page-header">
      <h1>Bilanço</h1>
      <p class="subtitle">
        Günlük gelir / gider kayıtlarını tut, tarih aralığı seçerek özetini gör.
      </p>
    </header>

    <!-- ÜST ÖZET KUTULARI -->
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
          <button class="btn primary" @click="loadLedger">Yenile</button>
        </div>
      </div>
    </section>

    <!-- YENİ KAYIT FORMU -->
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

    <!-- LİSTE -->
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
import { fetchLedgerRange, createLedgerEntry } from '@/api/ledger'

// === STATE ===
const entries = ref([])          // API'den gelen düz array
const loading = ref(false)
const saving = ref(false)
const error = ref('')

// Tarih aralığı (default: bugün)
const todayIso = new Date().toISOString().slice(0, 10)
const from = ref(todayIso)
const to = ref(todayIso)

// Form state
const form = ref({
  date: todayIso,
  amount: null,
  type: 'income',   // 'income' | 'expense'
  category: '',
  note: '',
})

// === COMPUTED TOPLU HESAPLAR ===
const totalIncome = computed(() =>
  entries.value
    .filter(e => e.isIncome)
    .reduce((sum, e) => sum + Number(e.amount || 0), 0)
)

const totalExpense = computed(() =>
  entries.value
    .filter(e => !e.isIncome)
    .reduce((sum, e) => sum + Number(e.amount || 0), 0)
)

const netTotal = computed(() => totalIncome.value - totalExpense.value)

// === HELPERS ===
function formatAmount(value) {
  return Number(value || 0).toLocaleString('tr-TR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })
}

function setToday() {
  const t = new Date().toISOString().slice(0, 10)
  from.value = t
  to.value = t
  loadLedger()
}

function setThisWeek() {
  const now = new Date()
  const day = now.getDay() || 7 // Pazarı 7 say
  const monday = new Date(now)
  monday.setDate(now.getDate() - (day - 1))
  const sunday = new Date(monday)
  sunday.setDate(monday.getDate() + 6)

  from.value = monday.toISOString().slice(0, 10)
  to.value = sunday.toISOString().slice(0, 10)
  loadLedger()
}

// === API ÇAĞRILARI ===
async function loadLedger() {
  loading.value = true
  error.value = ''
  try {
    const params = { from: from.value, to: to.value }
    console.log('LEDGER RANGE PARAMS >>>', params)

    const data = await fetchLedgerRange(from.value, to.value)
    console.log('LEDGER RANGE OK >>>', data)

    // API düz array döndürdüğü için direkt atıyoruz
    entries.value = Array.isArray(data) ? data : []
  } catch (e) {
    console.error('ledger load error', e)
    error.value = 'Bilanço verileri yüklenirken hata oluştu.'
    entries.value = []
  } finally {
    loading.value = false
  }
}

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
    description: form.value.note || null,
  }

  console.log('LEDGER PAYLOAD >>>', payload)

  saving.value = true
  try {
    const created = await createLedgerEntry(payload)
    console.log('LEDGER OK >>>', created)

    // Aralıkta ise listeye ekle, yoksa sadece reload
    if (created.date >= from.value && created.date <= to.value) {
      entries.value.unshift(created)
    } else {
      await loadLedger()
    }

    // Formu sıfırla (tarihi bugünde bırak)
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

// İlk açılışta bugünün hareketlerini getir
onMounted(() => {
  loadLedger()
})
</script>

<style scoped>
.page {
  padding: 1.5rem;
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
  display: flex;
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

/* LİSTE */
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

/* RESPONSIVE */
@media (max-width: 900px) {
  .cards {
    grid-template-columns: 1fr;
  }

  .controls .date-range {
    align-items: stretch;
  }
}
</style>
