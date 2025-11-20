<template>
  <div class="page">
    <header class="page-header">
      <h1>Genel Bakış</h1>
      <p class="subtitle">
        Bugünkü, yarınki ve geciken hatırlatmaları özet halinde görün.
      </p>
    </header>

    <!-- 3 KUTU -->
    <section class="cards">
      <div
        class="card kpi today"
        :class="{ active: activeFilter === 'today' }"
        @click="loadList('today')"
      >
        <h2>Bugün</h2>
        <p class="value">{{ summary?.pendingToday ?? 0 }}</p>
        <p class="label">hatırlatma</p>
      </div>

      <div
        class="card kpi tomorrow"
        :class="{ active: activeFilter === 'tomorrow' }"
        @click="loadList('tomorrow')"
      >
        <h2>Yarın</h2>
        <p class="value">{{ summary?.pendingTomorrow ?? 0 }}</p>
        <p class="label">hatırlatma</p>
      </div>

      <div
        class="card kpi overdue"
        :class="{ active: activeFilter === 'overdue' }"
        @click="loadList('overdue')"
      >
        <h2>Geciken</h2>
        <p class="value">{{ summary?.overdue ?? 0 }}</p>
        <p class="label">henüz işlenmemiş</p>
      </div>
      <div
        class="card kpi done"
        :class="{ active: activeFilter === 'done' }"
        @click="loadList('done')"
      >
        <h2>İşlem Yapıldı</h2>
        <p class="value">{{ summary?.completed ?? 0 }}</p>
        <p class="label">tamamlanan</p>
      </div>
    </section>

    <!-- AŞAĞIDAKİ TABLO -->
    <section class="card upcoming">
      <div class="card-header">
        <h2>{{ titleForFilter() }}</h2>
      </div>

      <div v-if="listLoading" class="state">Yükleniyor...</div>

      <div
        v-else-if="!reminderList || reminderList.length === 0"
        class="state"
      >
        Kayıt bulunamadı.
      </div>

      <table v-else class="table">
        <thead>
          <tr>
            <th>Hatırlatma</th>
            <th>Randevu</th>
            <th>Hasta</th>
            <th>Sahip</th>
            <th>İşlem</th>
          </tr>
        </thead>
        <tbody>
          <tr
              v-for="item in reminderList"
              :key="item.id"
              @click="openVisit(item)"
              class="clickable-row"
            >
              <td>{{ formatDate(item.reminderDate) }}</td>
              <td>{{ formatDate(item.appointmentDate) }}</td>
              <td>{{ item.petName }}</td>
              <td>{{ item.ownerName }}</td>
              <td class="procedure-cell">{{ item.procedures }}</td>
          </tr>

        </tbody>
      </table>
    </section>
  </div>

<div v-if="showDetail" class="modal-backdrop" @click.self="closeDetail">
  <div class="modal">
    <button class="close" @click="closeDetail">×</button>

    <div v-if="detailLoading" class="state">Yükleniyor...</div>
    <div v-else-if="!selectedVisit" class="state">
      Kayıt bulunamadı.
    </div>
    <div v-else class="detail-body">
      <h3>{{ selectedVisit.petName }} – {{ selectedVisit.ownerName }}</h3>
      <p><strong>Yapılan işlem tarihi:</strong> {{ selectedVisit.performedAt }}</p>
      <p><strong>Ne zaman gelecek? :</strong> {{ formatDateTime(selectedVisit.nextDate) }}</p>
      <p><strong>Ne için gelecek? :</strong> {{ selectedVisit.purpose || '—' }}</p>
      <p><strong>İşlem(ler):</strong> {{ selectedVisit.procedures || '—' }}</p>
      <p><strong>Tutar:</strong> {{ selectedVisit.amountTl ?? '—' }} TL</p>
      <p><strong>Hasta sahibine not:</strong> {{ selectedVisit.notes || '—' }}</p>

      <div v-if="selectedVisit.imageUrl" class="image-box">
        <img :src="selectedVisit.imageUrl" alt="Ziyaret görseli" />
      </div>
      <div class="status-row">
        <p><strong>İşlem durumu:</strong> Bu işlem işleme alındı mı?</p>
      </div>
      <div class="actions-row">
        <button class="btn-fail" @click="markReminder(false)">✗ Yapılmadı</button>
        <button class="btn-success" @click="markReminder(true)">✓ Yapıldı</button>
      </div>
    </div>
  </div>
</div>



</template>

<script setup>
import { onMounted, ref } from 'vue'
import { fetchReminderSummary, fetchReminders, fetchVisitDetail } from '../api/dashboard'
import { http } from '@/api/http'


const loading = ref(false)
const error = ref('')
const summary = ref(null)
const visitDetail = ref(null)
const showDetailModal = ref(false)
const showDetail = ref(false)
const detailLoading = ref(false)
const selectedVisit = ref(null)

const listLoading = ref(false)
const reminderList = ref([])
const activeFilter = ref('upcoming')
const selectedReminderId = ref(null) 

onMounted(async () => {
  await loadSummary()
  await loadList('upcoming')
})

async function openVisit(item) {
  showDetail.value = true
  detailLoading.value = true
  selectedVisit.value = null
  selectedReminderId.value = item.id   // YENİ: bu reminder'ın id'sini saklıyoruz

  try {
    selectedVisit.value = await fetchVisitDetail(item.visitId)
  } catch (e) {
    console.error(e)
  } finally {
    detailLoading.value = false
  }
}

async function markReminder(completed) {
  if (!selectedReminderId.value) return

  try {
    await http.patch(`/reminders/${selectedReminderId.value}/status`, {
      completed,
      markAsOverdue: !completed,
    })

    // Özet ve listeyi tazele
    await loadSummary()
    await loadList(completed ? 'done' : 'overdue')

    // Modalı kapat
    closeDetail()
  } catch (e) {
    console.error(e)
  }
}


async function openVisitDetail(item) {
  try {
    detailLoading.value = true
    showDetailModal.value = true
    visitDetail.value = await fetchVisitDetail(item.visitId)
  } finally {
    detailLoading.value = false
  }
}
function formatDateTime(dt) {
  if (!dt) return '—'
  // backend'den ISO string geliyorsa:
  const d = new Date(dt)
  return d.toLocaleDateString('tr-TR')
}

function closeDetail() {
  showDetail.value = false
  selectedVisit.value = null
}

async function loadSummary() {
  loading.value = true
  error.value = ''
  try {
    const result = await fetchReminderSummary()
    console.log('SUMMARY FROM API >>>', result) // 🔴 DEBUG satırı
    summary.value = result
  } catch (e) {
    console.error(e)
    error.value = 'Hatırlatma özeti yüklenirken bir hata oluştu.'
  } finally {
    loading.value = false
  }
}

async function loadList(filter) {
  activeFilter.value = filter
  listLoading.value = true
  try {
    reminderList.value = await fetchReminders(filter)
  } catch (e) {
    console.error(e)
  } finally {
    listLoading.value = false
  }
}

function formatDate(dateOnlyString) {
  if (!dateOnlyString) return '—'
  const [y, m, d] = dateOnlyString.split('-')
  return `${d}.${m}.${y}`
}

function titleForFilter() {
  switch (activeFilter.value) {
    case 'today':
      return 'Bugünkü hatırlatmalar'
    case 'tomorrow':
      return 'Yarının hatırlatmaları'
    case 'overdue':
      return 'Geciken hatırlatmalar'
    case 'done':
      return 'Tamamlanan işlemler'
    default:
      return 'Yaklaşan hatırlatmalar'
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
  margin: 0.25rem 0 0;
  font-size: 0.85rem;
  color: #6b7280;
}

.cards {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

@media (max-width: 900px) {
  .cards {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 600px) {
  .cards {
    grid-template-columns: 1fr;
  }
}


.card {
  background: #ffffff;
  border-radius: 0.75rem;
  padding: 1rem;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);
}

.kpi {
  text-align: center;
  cursor: pointer;
  transition: transform 0.1s ease, box-shadow 0.1s ease;
}

.kpi:hover {
  transform: translateY(-2px);
  box-shadow: 0 14px 40px rgba(15, 23, 42, 0.12);
}

.kpi.active {
  outline: 2px solid #4ade80;
}

.kpi h2 {
  margin: 0;
  font-size: 0.9rem;
  color: #4b5563;
}

.kpi .value {
  margin: 0.25rem 0 0;
  font-size: 2rem;
  font-weight: 700;
}

.kpi .label {
  margin: 0;
  font-size: 0.8rem;
  color: #6b7280;
}

.today {
  border-top: 3px solid #22c55e;
}

.tomorrow {
  border-top: 3px solid #0ea5e9;
}

.overdue {
  border-top: 3px solid #f97316;
}

.card-header {
  margin-bottom: 0.5rem;
}

.card-header h2 {
  margin: 0;
  font-size: 1rem;
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

.procedure-cell {
  max-width: 320px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}


.clickable-row {
  cursor: pointer;
}
.clickable-row:hover {
  background: #f9fafb;
}

.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
}

.modal {
  background: white;
  border-radius: 0.75rem;
  padding: 1.25rem 1.5rem;
  max-width: 600px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
}
.status-row {
  margin-top: 0.75rem;
  font-size: 0.85rem;
}

.actions-row {
  margin-top: 0.5rem;
  display: flex;
  gap: 0.5rem;
}

.btn-success,
.btn-fail {
  border: none;
  border-radius: 999px;
  padding: 0.35rem 0.9rem;
  font-size: 0.8rem;
  cursor: pointer;
  font-weight: 600;
}

.btn-success {
  background: #dcfce7;
  color: #166534;
}

.btn-success:hover {
  background: #bbf7d0;
}

.btn-fail {
  background: #fee2e2;
  color: #b91c1c;
}

.btn-fail:hover {
  background: #fecaca;
}


.modal .close {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  border: none;
  background: transparent;
  font-size: 1.5rem;
  cursor: pointer;
}

.image-box {
  margin-top: 0.75rem;
}
.image-box img {
  max-width: 100%;
  border-radius: 0.5rem;
}

</style>
