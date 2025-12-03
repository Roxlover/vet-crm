<template>
  <div class="page">
    <header class="page-header">
      <h1>Takvim</h1>
      <p class="subtitle">
        Randevuları tarih ve saate göre görüntüleyin.
      </p>
    </header>

    <section class="filters">
      <div class="field">
        <label>Başlangıç</label>
        <input type="date" v-model="from" @change="loadAppointments" />
      </div>
      <div class="field">
        <label>Bitiş</label>
        <input type="date" v-model="to" @change="loadAppointments" />
      </div>
      <button class="btn" @click="todayRange">Bugün</button>
      <button class="btn" @click="thisWeekRange">Bu Hafta</button>
    </section>

    <section class="calendar">
      <div v-if="loading" class="state">Yükleniyor...</div>
      <div v-else-if="appointments.length === 0" class="state">
        Bu aralıkta randevu bulunamadı.
      </div>

      <table v-else class="table">
        <thead>
          <tr>
            <th>Tarih</th>
            <th>Saat</th>
            <th>Hasta</th>
            <th>Sahip</th>
            <th>İşlem</th>
            <th>Doktor</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in appointments" :key="item.visitId">
            <td>{{ formatDate(item.scheduledAt) }}</td>
            <td>{{ formatTime(item.scheduledAt) }}</td>
            <td>{{ item.petName }}</td>
            <td>{{ item.ownerName }}</td>
            <td>{{ item.purpose || '—' }}</td>
            <td>{{ item.doctorName || '—' }}</td>
          </tr>
        </tbody>
      </table>
    </section>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { http } from '@/api/http'
import { useRoute } from 'vue-router'

const route = useRoute()

const from = ref('')
const to = ref('')
const loading = ref(false)
const appointments = ref([])

const loadAppointments = async () => {
  if (!from.value || !to.value) return
  loading.value = true
  try {
    const { data } = await http.get('/calendar/appointments', {
      params: {
        from: from.value,
        to: to.value,
      },
    })
    appointments.value = data
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const todayRange = () => {
  const today = new Date().toISOString().slice(0, 10)
  from.value = today
  to.value = today
  loadAppointments()
}

const thisWeekRange = () => {
  const d = new Date()
  const day = d.getDay() || 7
  const monday = new Date(d)
  monday.setDate(d.getDate() - day + 1)
  const sunday = new Date(monday)
  sunday.setDate(monday.getDate() + 6)

  from.value = monday.toISOString().slice(0, 10)
  to.value = sunday.toISOString().slice(0, 10)
  loadAppointments()
}

const formatDate = (iso) => {
  const d = new Date(iso)
  return d.toLocaleDateString('tr-TR')
}

const formatTime = (iso) => {
  const d = new Date(iso)
  return d.toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })
}

// Dashboard'dan tarih parametresi ile gelmişsek
onMounted(() => {
  const qDate = route.query.date
  if (qDate) {
    from.value = qDate
    to.value = qDate
    loadAppointments()
  } else {
    todayRange()
  }
})
</script>

<style scoped>
.page {
  width: 100%;
  max-width: 1024px;
  margin: 0 auto;
  padding: 1rem; /* her sayfada ihtiyacına göre değiştirirsin */
}

.page-header {
  margin-bottom: 1rem;
}

.subtitle {
  margin: 0.25rem 0 0;
  font-size: 0.85rem;
  color: #6b7280;
}

.filters {
  display: flex;
  gap: 1rem;
  align-items: flex-end;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.85rem;
}

.field input[type='date'] {
  padding: 0.35rem 0.5rem;
  border-radius: 0.5rem;
  border: 1px solid #d1d5db;
}

.btn {
  border: none;
  padding: 0.4rem 0.9rem;
  border-radius: 999px;
  background: #111827;
  color: #fff;
  font-size: 0.8rem;
  cursor: pointer;
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
}

.state {
  font-size: 0.9rem;
  color: #6b7280;
  padding: 0.4rem 0;
}
</style>
