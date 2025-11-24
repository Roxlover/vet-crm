<template>
  <div class="page">
    <header class="page-header">
      <h1>Bilanço</h1>
      <p class="subtitle">
        Günlük gelir / gider kayıtlarını tut, seçtiğin tarih aralığında özetini gör.
      </p>
    </header>

    <!-- TARİH FİLTRELERİ -->
    <section class="filters card">
      <div class="date-row">
        <div class="field">
          <label>Başlangıç</label>
          <input type="date" v-model="fromDate" @change="loadLedger" />
        </div>
        <div class="field">
          <label>Bitiş</label>
          <input type="date" v-model="toDate" @change="loadLedger" />
        </div>
        <div class="quick-buttons">
          <button @click="setToday">Bugün</button>
          <button @click="setThisWeek">Bu Hafta</button>
          <button @click="setThisMonth">Bu Ay</button>
        </div>
      </div>
    </section>

    <!-- ÖZET KARTLAR -->
    <section class="cards">
      <div class="card kpi income">
        <h2>Gelir</h2>
        <p class="value">{{ formatMoney(summary.totalIncome) }}</p>
      </div>
      <div class="card kpi expense">
        <h2>Gider</h2>
        <p class="value">{{ formatMoney(summary.totalExpense) }}</p>
      </div>
      <div class="card kpi net">
        <h2>Net</h2>
        <p class="value">{{ formatMoney(summary.net) }}</p>
      </div>
    </section>

    <div class="layout">
      <!-- KAYIT FORMU -->
      <section class="card form-card">
        <h2>Yeni Kayıt</h2>

        <div class="field">
          <label>Tarih</label>
          <input type="date" v-model="form.date" />
        </div>

        <div class="field">
          <label>Tür</label>
          <div class="inline">
            <label><input type="radio" value="income" v-model="form.type" /> Gelir</label>
            <label><input type="radio" value="expense" v-model="form.type" /> Gider</label>
          </div>
        </div>

        <div class="field">
          <label>Tutar (TL)</label>
          <input type="number" step="0.01" v-model.number="form.amount" />
        </div>

        <div class="field">
          <label>Kategori</label>
          <input type="text" v-model="form.category" placeholder="Örn: Muayene, Mama Satışı, Kira..." />
        </div>

        <div class="field">
          <label>Açıklama</label>
          <textarea rows="2" v-model="form.description"></textarea>
        </div>

        <div class="actions">
          <button @click="resetForm">Temizle</button>
          <button class="primary" @click="submitEntry">Kaydet</button>
        </div>
      </section>

      <!-- LİSTE -->
      <section class="card table-card">
        <h2>Kayıtlar</h2>

        <div v-if="loading" class="state">Yükleniyor...</div>
        <div v-else-if="items.length === 0" class="state">
          Bu aralıkta kayıt yok.
        </div>

        <table v-else class="table">
          <thead>
            <tr>
              <th>Tarih</th>
              <th>Tür</th>
              <th>Kategori</th>
              <th>Tutar</th>
              <th>Açıklama</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in items" :key="item.id">
              <td>{{ formatDate(item.date) }}</td>
              <td>
                <span :class="item.isIncome ? 'tag-income' : 'tag-expense'">
                  {{ item.isIncome ? 'Gelir' : 'Gider' }}
                </span>
              </td>
              <td>{{ item.category }}</td>
              <td>{{ formatMoney(item.amount) }}</td>
              <td>{{ item.description || '—' }}</td>
            </tr>
          </tbody>
        </table>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { fetchLedgerRange, createLedgerEntry } from '@/api/ledger'

const fromDate = ref('')
const toDate = ref('')
const loading = ref(false)
const items = ref([])

const summary = ref({
  totalIncome: 0,
  totalExpense: 0,
  net: 0,
})

const form = ref({
  date: '',
  type: 'income', // income | expense
  amount: null,
  category: '',
  description: '',
})

onMounted(() => {
  setToday()
})

function todayIso() {
  return new Date().toISOString().slice(0, 10)
}

function setToday() {
  const t = todayIso()
  fromDate.value = t
  toDate.value = t
  form.value.date = t
  loadLedger()
}

function setThisWeek() {
  const now = new Date()
  const day = now.getDay() || 7 // pazar=0 → 7
  const diff = day - 1
  const start = new Date(now)
  start.setDate(now.getDate() - diff)

  fromDate.value = start.toISOString().slice(0, 10)
  toDate.value = todayIso()
  loadLedger()
}

function setThisMonth() {
  const now = new Date()
  const start = new Date(now.getFullYear(), now.getMonth(), 1)
  fromDate.value = start.toISOString().slice(0, 10)
  toDate.value = todayIso()
  loadLedger()
}

async function loadLedger() {
  if (!fromDate.value || !toDate.value) return
  loading.value = true
  try {
    const data = await fetchLedgerRange(fromDate.value, toDate.value)
    items.value = data.items
    summary.value = {
      totalIncome: data.totalIncome,
      totalExpense: data.totalExpense,
      net: data.net,
    }
  } catch (e) {
    console.error('ledger load error', e)
  } finally {
    loading.value = false
  }
}

function formatDate(dateOnly) {
  if (!dateOnly) return '—'
  const [y, m, d] = dateOnly.split('-')
  return `${d}.${m}.${y}`
}

function formatMoney(v) {
  return new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency: 'TRY',
    minimumFractionDigits: 2,
  }).format(v || 0)
}

function resetForm() {
  form.value = {
    date: form.value.date || todayIso(),
    type: 'income',
    amount: null,
    category: '',
    description: '',
  }
}

async function submitEntry() {
  if (!form.value.date || !form.value.amount || !form.value.category) {
    alert('Tarih, tutar ve kategori zorunlu.')
    return
  }

  const payload = {
    date: form.value.date,
    amount: form.value.amount,
    isIncome: form.value.type === 'income',
    category: form.value.category,
    description: form.value.description,
    createdByUserId: 1, // şimdilik Ahmet
  }

  try {
    await createLedgerEntry(payload)
    // form tarihine göre seçili aralık içindeyse listede gözükecek
    await loadLedger()
    resetForm()
  } catch (e) {
    console.error('create ledger entry error', e)
  }
}
</script>

<style scoped>
.page {
  padding: 1.5rem;
}

.page-header {
  margin-bottom: 1rem;
}

.subtitle {
  margin-top: 0.25rem;
  font-size: 0.85rem;
  color: #6b7280;
}

.card {
  background: #ffffff;
  border-radius: 0.75rem;
  padding: 1rem;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);
}

.filters {
  margin-bottom: 1rem;
}

.date-row {
  display: flex;
  gap: 1rem;
  align-items: flex-end;
  flex-wrap: wrap;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.85rem;
}

.field label {
  font-weight: 600;
}

.field input,
.field textarea {
  border-radius: 0.5rem;
  border: 1px solid #d1d5db;
  padding: 0.35rem 0.5rem;
  font-size: 0.85rem;
}

.quick-buttons {
  display: flex;
  gap: 0.5rem;
}

.quick-buttons button {
  border: none;
  padding: 0.4rem 0.8rem;
  border-radius: 999px;
  background: #e5e7eb;
  font-size: 0.8rem;
  cursor: pointer;
}

.quick-buttons button:hover {
  background: #d1d5db;
}

.cards {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
  margin: 1rem 0;
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

.income {
  border-top: 3px solid #16a34a;
}

.expense {
  border-top: 3px solid #dc2626;
}

.net {
  border-top: 3px solid #0ea5e9;
}

.layout {
  display: grid;
  grid-template-columns: minmax(0, 280px) minmax(0, 1fr);
  gap: 1rem;
}

@media (max-width: 900px) {
  .cards {
    grid-template-columns: 1fr;
  }
  .layout {
    grid-template-columns: 1fr;
  }
}

.inline {
  display: flex;
  gap: 1rem;
  font-size: 0.8rem;
}

.actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 0.75rem;
}

.actions button {
  border: none;
  padding: 0.35rem 0.8rem;
  border-radius: 999px;
  font-size: 0.8rem;
  cursor: pointer;
}

.actions .primary {
  background: #111827;
  color: #fff;
}

.state {
  padding: 0.5rem 0;
  font-size: 0.85rem;
  color: #6b7280;
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

.tag-income,
.tag-expense {
  padding: 0.1rem 0.5rem;
  border-radius: 999px;
  font-size: 0.75rem;
}

.tag-income {
  background: #dcfce7;
  color: #166534;
}

.tag-expense {
  background: #fee2e2;
  color: #b91c1c;
}
</style>
