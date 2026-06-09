<template>
  <div class="client-visits">
    <header class="view-header">
      <h1>Sağlık Geçmişi 📋</h1>
      <p>Dostlarınızın kliniğimizdeki muayene geçmişini, uygulanan tedavileri ve hekim notlarını inceleyin.</p>
    </header>

    <!-- Yükleniyor Durumu -->
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Muayene geçmişi yükleniyor...</p>
    </div>

    <!-- Hata Durumu -->
    <div v-else-if="error" class="error-container">
      <span class="error-icon">⚠️</span>
      <p>{{ error }}</p>
      <button class="client-btn" @click="fetchData">Yeniden Dene</button>
    </div>

    <div v-else>
      <!-- Arama ve Filtreleme Filtresi -->
      <div class="filter-card client-card">
        <label for="petFilter">🩺 Hayvana Göre Filtrele</label>
        <select id="petFilter" v-model="selectedPetFilter" class="filter-select">
          <option value="">Tüm Dostlarım</option>
          <option v-for="name in uniquePetNames" :key="name" :value="name">
            {{ name }}
          </option>
        </select>
      </div>

      <!-- Boş Geçmiş Durumu -->
      <div v-if="filteredVisits.length === 0" class="empty-state client-card">
        <span class="empty-icon">📂</span>
        <h3>Ziyaret Kaydı Bulunmadı</h3>
        <p>Seçilen filtreye uygun klinik muayene veya işlem kaydı bulunmamaktadır.</p>
      </div>

      <!-- Muayene Timeline -->
      <div v-else class="visits-timeline">
        <div v-for="visit in filteredVisits" :key="visit.id" class="visit-card client-card">
          <!-- Ziyaret Kart Başlığı -->
          <div class="visit-card-header">
            <div class="visit-main-meta">
              <span class="visit-date">{{ formatDateTime(visit.performedAt) }}</span>
              <span class="pet-badge">{{ visit.petName }}</span>
            </div>
            <div v-if="visit.doctorName" class="doctor-badge">
              👨‍⚕️ {{ visit.doctorName }}
            </div>
          </div>

          <!-- Uygulanan İşlemler -->
          <div class="visit-body">
            <div class="visit-section">
              <span class="section-label">⚙️ Uygulanan İşlem / Tedavi</span>
              <p class="section-content font-semibold">{{ visit.procedures || 'Genel Kontrol' }}</p>
            </div>

            <!-- Veteriner Notu (Önceki Hekim Notu) -->
            <div v-if="visit.clientNotes" class="visit-section notes-block">
              <span class="section-label text-warning-dark">📝 Veteriner Notu</span>
              <p class="section-content text-warning-dark">{{ visit.clientNotes }}</p>
            </div>
            
            <div v-if="visit.collectedAmountTl !== null" class="visit-section">
              <span class="section-label">💰 Ödenen Ücret</span>
              <p class="section-content font-bold text-success">{{ visit.collectedAmountTl }} TL</p>
            </div>

            <!-- Ekli Dosyalar/Görseller -->
            <div v-if="visit.images && visit.images.length > 0" class="visit-section">
              <span class="section-label">📸 İşlem Görselleri</span>
              <div class="visit-gallery">
                <div v-for="(img, idx) in visit.images" :key="idx" class="gallery-item">
                  <img
                    :src="normalizeMediaUrl(img)"
                    alt="Muayene görseli"
                    @click="openImage(normalizeMediaUrl(img))"
                    title="Görseli büyütmek için tıklayın"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { http, API_BASE } from '@/api/http'

const loading = ref(true)
const error = ref('')

const visits = ref([])
const selectedPetFilter = ref('')

async function fetchData() {
  loading.value = true
  error.value = ''

  try {
    const res = await http.get('/clientportal/visits')
    visits.value = res.data || []
  } catch (err) {
    console.error('[CLIENT PORTAL VISITS FETCH ERROR]', err)
    error.value = 'Geçmiş klinik kayıtlarınız yüklenirken bir sorun oluştu.'
  } finally {
    loading.value = false
  }
}

const uniquePetNames = computed(() => {
  const names = visits.value.map(v => v.petName).filter(Boolean)
  return [...new Set(names)]
})

const filteredVisits = computed(() => {
  if (!selectedPetFilter.value) return visits.value
  return visits.value.filter(
    v => String(v.petName).toLowerCase() === String(selectedPetFilter.value).toLowerCase()
  )
})

function formatDateTime(isoString) {
  if (!isoString) return ''
  try {
    const date = new Date(isoString)
    return date.toLocaleString('tr-TR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch {
    return isoString
  }
}

function normalizeMediaUrl(rawUrl) {
  if (!rawUrl) return ''
  if (rawUrl.startsWith('http')) return rawUrl
  const base = API_BASE.endsWith('/') ? API_BASE.slice(0, -1) : API_BASE
  const path = rawUrl.startsWith('/') ? rawUrl : `/${rawUrl}`
  return `${base}${path}`
}

function openImage(url) {
  window.open(url, '_blank')
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.client-visits {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.view-header {
  margin-bottom: 2rem;
  padding: 0 0.5rem;
}

.view-header h1 {
  font-family: 'Outfit', sans-serif;
  font-size: 1.75rem;
  font-weight: 800;
  color: #111827;
  margin: 0 0 0.5rem;
  letter-spacing: -0.04em;
}

.view-header p {
  font-size: 0.95rem;
  color: #047857;
  margin: 0;
  line-height: 1.5;
}

.loading-container, .error-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 50vh;
  text-align: center;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid rgba(5, 150, 105, 0.1);
  border-radius: 50%;
  border-top-color: #059669;
  animation: spin 0.8s linear infinite;
  margin-bottom: 1rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.error-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
}

.filter-card {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 1.25rem;
}

.filter-card label {
  font-size: 0.85rem;
  font-weight: 700;
  color: #374151;
}

.filter-select {
  width: 100%;
  padding: 0.85rem 1rem;
  border: 1px solid #cbd5e1;
  border-radius: 16px;
  background: white;
  font-size: 0.95rem;
  font-weight: 600;
  outline: none;
  color: #1f2937;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.02);
  transition: border-color 0.2s;
}

.filter-select:focus {
  border-color: #059669;
}

.empty-state {
  text-align: center;
  padding: 3rem 1.5rem;
}

.empty-icon {
  font-size: 3rem;
  display: block;
  margin-bottom: 1rem;
}

.empty-state h3 {
  font-size: 1.25rem;
  font-weight: 800;
  margin: 0 0 0.5rem;
  color: #111827;
}

.empty-state p {
  font-size: 0.95rem;
  color: #6b7280;
  margin: 0;
  line-height: 1.5;
}

.visits-timeline {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.visit-card {
  padding: 0;
  overflow: hidden;
}

.visit-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem;
  background: rgba(249, 250, 251, 0.4);
  border-bottom: 1px solid rgba(22, 101, 52, 0.08);
}

.visit-main-meta {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.visit-date {
  font-family: 'Outfit', sans-serif;
  font-size: 0.95rem;
  font-weight: 800;
  color: #1f2937;
}

.pet-badge {
  background-color: #ecfdf5;
  color: #047857;
  padding: 0.2rem 0.6rem;
  border-radius: 8px;
  font-size: 0.75rem;
  font-weight: 700;
  border: 1px solid #a7f3d0;
}

.doctor-badge {
  font-size: 0.8rem;
  font-weight: 700;
  color: #4b5563;
}

.visit-body {
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.visit-section {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.section-label {
  font-size: 0.75rem;
  font-weight: 700;
  color: #6b7280;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.section-content {
  font-size: 0.95rem;
  color: #1f2937;
  margin: 0;
  line-height: 1.5;
}

.font-semibold {
  font-weight: 600;
}

.notes-block {
  background: #fffbeb;
  border: 1px solid #fde68a;
  border-radius: 18px;
  padding: 1rem;
}

.text-warning-dark {
  color: #78350f;
}

.visit-gallery {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-top: 0.25rem;
}

.gallery-item {
  width: 90px;
  height: 90px;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #e5e7eb;
  cursor: pointer;
  transition: transform 0.2s;
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
}

.gallery-item:hover {
  transform: scale(1.05);
}

.gallery-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
</style>
