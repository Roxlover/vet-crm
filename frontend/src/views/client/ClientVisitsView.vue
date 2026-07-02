<template>
  <div class="client-visits">

    <!-- Page Header -->
    <div class="page-header">
      <h1>Sağlık Geçmişi</h1>
      <p>Tüm klinik muayeneleri ve tedaviler</p>
    </div>

    <!-- Yükleniyor -->
    <div v-if="loading" class="loading-state">
      <div class="paw-loader"><span>🐾</span></div>
      <p>Muayene kayıtları yükleniyor...</p>
    </div>

    <!-- Hata -->
    <div v-else-if="error" class="error-state">
      <div class="error-icon-wrap">⚠️</div>
      <p>{{ error }}</p>
      <button class="client-btn" @click="fetchData">Tekrar Dene</button>
    </div>

    <div v-else>
      <!-- Özet Sayaç Bandı -->
      <div class="summary-band">
        <div class="summary-item">
          <span class="summary-num">{{ visits.length }}</span>
          <span class="summary-label">Toplam Ziyaret</span>
        </div>
        <div class="summary-divider"></div>
        <div class="summary-item">
          <span class="summary-num">{{ uniquePetNames.length }}</span>
          <span class="summary-label">Hayvan</span>
        </div>
        <div class="summary-divider"></div>
        <div class="summary-item">
          <span class="summary-num">{{ lastVisitDate }}</span>
          <span class="summary-label">Son Ziyaret</span>
        </div>
      </div>

      <!-- Pet Filtre Tabları -->
      <div class="pet-filter-tabs">
        <button
          class="filter-tab"
          :class="{ active: selectedPetFilter === '' }"
          @click="selectedPetFilter = ''"
        >
          Tümü
        </button>
        <button
          v-for="name in uniquePetNames"
          :key="name"
          class="filter-tab"
          :class="{ active: selectedPetFilter === name }"
          @click="selectedPetFilter = name"
        >
          {{ getPetEmoji(getSpeciesForPet(name)) }} {{ name }}
        </button>
      </div>

      <!-- Boş Durum -->
      <div v-if="filteredVisits.length === 0" class="empty-state-card">
        <div class="empty-graphic">📂</div>
        <h3>Kayıt Bulunamadı</h3>
        <p>Seçilen filtre için muayene kaydı bulunmuyor.</p>
      </div>

      <!-- Ziyaret Listesi -->
      <div v-else class="visits-feed">
        <div
          v-for="(visit, idx) in filteredVisits"
          :key="visit.id"
          class="visit-card"
          :class="{ 'card-staggered': true }"
          :style="{ animationDelay: `${idx * 0.05}s` }"
        >
          <!-- Kart Başlığı -->
          <div class="visit-card-head">
            <div class="visit-head-left">
              <div class="visit-date-badge">
                <span class="vdb-day">{{ getDay(visit.performedAt) }}</span>
                <span class="vdb-mon">{{ getMonth(visit.performedAt) }}</span>
              </div>
              <div class="visit-head-meta">
                <span class="visit-year">{{ getYear(visit.performedAt) }}</span>
                <div class="visit-pet-pill">
                  <span>{{ getPetEmoji(getSpeciesForPet(visit.petName)) }}</span>
                  {{ visit.petName }}
                </div>
              </div>
            </div>
            <div v-if="visit.doctorName" class="doctor-pill">
              <span>👨‍⚕️</span> {{ visit.doctorName }}
            </div>
          </div>

          <!-- Kart Gövdesi -->
          <div class="visit-card-body">

            <!-- İşlem -->
            <div class="info-row">
              <span class="info-label">⚙️ Uygulanan İşlem</span>
              <p class="info-value bold-val">{{ visit.procedures || 'Genel Kontrol' }}</p>
            </div>

            <!-- Tanı -->
            <div v-if="visit.diseaseName" class="diagnosis-block">
              <div class="diag-header">
                <span class="diag-icon">🦠</span>
                <span class="diag-title">Tanı</span>
                <span class="diag-status-pill">{{ visit.diagnosisStatus }}</span>
              </div>
              <p class="diag-name">{{ visit.diseaseName }}</p>
            </div>

            <!-- Veteriner Notu -->
            <div v-if="visit.clientNotes" class="vet-note-block">
              <div class="vnb-header">
                <span>📝</span>
                <strong>Veteriner Notu</strong>
              </div>
              <p>{{ visit.clientNotes }}</p>
            </div>

            <!-- Ödeme -->
            <div v-if="visit.collectedAmountTl !== null && visit.collectedAmountTl !== undefined" class="payment-row">
              <span class="payment-label">💰 Ödenen Ücret</span>
              <span class="payment-amount">{{ visit.collectedAmountTl }} ₺</span>
            </div>

            <!-- Görseller -->
            <div v-if="visit.images && visit.images.length > 0" class="gallery-section">
              <span class="info-label">📸 Görseller</span>
              <div class="gallery-grid">
                <div
                  v-for="(img, imgIdx) in visit.images"
                  :key="imgIdx"
                  class="gallery-thumb"
                  @click="openImage(normalizeMediaUrl(img))"
                >
                  <img :src="normalizeMediaUrl(img)" alt="Muayene görseli" />
                  <div class="gallery-overlay">
                    <span>🔍</span>
                  </div>
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
    error.value = 'Muayene kayıtları yüklenirken bir hata oluştu.'
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
  return visits.value.filter(v =>
    String(v.petName).toLowerCase() === String(selectedPetFilter.value).toLowerCase()
  )
})

const lastVisitDate = computed(() => {
  if (!visits.value.length) return '—'
  const latest = visits.value[0]
  return getDay(latest.performedAt) + '.' + getMonthNum(latest.performedAt)
})

// Pet species cache (from visit data — we infer species from name if possible)
const petSpeciesMap = {}
function getSpeciesForPet(petName) {
  return petSpeciesMap[petName] || ''
}

function getPetEmoji(species) {
  const s = String(species || '').toLowerCase()
  if (s.includes('kedi') || s.includes('cat')) return '🐱'
  if (s.includes('köpek') || s.includes('dog')) return '🐶'
  if (s.includes('kuş') || s.includes('bird')) return '🦜'
  if (s.includes('tavşan') || s.includes('rabbit')) return '🐰'
  return '🐾'
}

function getDay(isoString) {
  if (!isoString) return '--'
  try { return new Date(isoString).getDate().toString().padStart(2, '0') }
  catch { return '--' }
}
function getMonthNum(isoString) {
  if (!isoString) return '--'
  try { return (new Date(isoString).getMonth() + 1).toString().padStart(2, '0') }
  catch { return '--' }
}
function getMonth(isoString) {
  if (!isoString) return '--'
  try {
    return new Date(isoString).toLocaleDateString('tr-TR', { month: 'short' }).replace('.', '')
  } catch { return '--' }
}
function getYear(isoString) {
  if (!isoString) return ''
  try { return new Date(isoString).getFullYear() }
  catch { return '' }
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

onMounted(() => fetchData())
</script>

<style scoped>
.client-visits {
  animation: fadeUp 0.4s ease-out both;
}
@keyframes fadeUp {
  from { opacity: 0; transform: translateY(14px); }
  to   { opacity: 1; transform: translateY(0); }
}

/* ── Page Header ── */
.page-header {
  margin-bottom: 1.5rem;
  padding: 0 0.25rem;
}
.page-header h1 {
  font-family: 'Outfit', sans-serif;
  font-size: 1.85rem;
  font-weight: 900;
  color: #064e3b;
  margin: 0 0 0.25rem;
  letter-spacing: -0.04em;
}
.page-header p { font-size: 0.9rem; color: #059669; margin: 0; font-weight: 500; }

/* ── Loading ── */
.loading-state {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center; min-height: 55vh; gap: 1rem;
}
.paw-loader span {
  font-size: 2.5rem; display: block;
  animation: pawBounce 1s ease-in-out infinite alternate;
}
@keyframes pawBounce {
  from { transform: scale(0.8) rotate(-10deg); }
  to   { transform: scale(1.15) rotate(10deg); }
}
.loading-state p { font-weight: 600; font-size: 0.95rem; color: #059669; margin: 0; }
.error-state {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center; min-height: 55vh; gap: 1rem; text-align: center;
}
.error-icon-wrap { font-size: 3rem; }

/* ── Summary Band ── */
.summary-band {
  background: linear-gradient(135deg, #064e3b 0%, #065f46 100%);
  border-radius: 22px;
  padding: 1.25rem 1.5rem;
  display: flex;
  align-items: center;
  justify-content: space-around;
  margin-bottom: 1.25rem;
  box-shadow: 0 10px 28px rgba(6, 78, 59, 0.3);
}
.summary-item { display: flex; flex-direction: column; align-items: center; gap: 0.2rem; }
.summary-num {
  font-family: 'Outfit', sans-serif;
  font-size: 1.6rem;
  font-weight: 900;
  color: white;
  line-height: 1;
}
.summary-label { font-size: 0.7rem; color: rgba(167, 243, 208, 0.85); font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; }
.summary-divider { width: 1px; height: 36px; background: rgba(255,255,255,0.15); }

/* ── Filter Tabs ── */
.pet-filter-tabs {
  display: flex;
  gap: 0.5rem;
  overflow-x: auto;
  padding-bottom: 0.5rem;
  scrollbar-width: none;
  margin-bottom: 1.25rem;
  -webkit-overflow-scrolling: touch;
}
.pet-filter-tabs::-webkit-scrollbar { display: none; }
.filter-tab {
  flex-shrink: 0;
  padding: 0.5rem 1rem;
  border-radius: 9999px;
  border: 1.5px solid rgba(5, 150, 105, 0.2);
  background: rgba(255,255,255,0.8);
  color: #6b7280;
  font-size: 0.82rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.25s ease;
  font-family: inherit;
}
.filter-tab.active {
  background: linear-gradient(135deg, #059669, #047857);
  color: white;
  border-color: transparent;
  box-shadow: 0 4px 14px rgba(5, 150, 105, 0.3);
}

/* ── Empty ── */
.empty-state-card {
  background: rgba(255,255,255,0.85);
  backdrop-filter: blur(12px);
  border-radius: 24px;
  padding: 3rem 2rem;
  text-align: center;
  border: 1px solid rgba(255,255,255,0.6);
}
.empty-graphic { font-size: 3.5rem; display: block; margin-bottom: 1rem; }
.empty-state-card h3 { font-family: 'Outfit', sans-serif; font-size: 1.1rem; font-weight: 800; color: #111827; margin: 0 0 0.5rem; }
.empty-state-card p { font-size: 0.88rem; color: #6b7280; margin: 0; }

/* ── Visits Feed ── */
.visits-feed { display: flex; flex-direction: column; gap: 1rem; }

.visit-card {
  background: rgba(255,255,255,0.88);
  backdrop-filter: blur(16px);
  border-radius: 24px;
  border: 1px solid rgba(255,255,255,0.6);
  box-shadow: 0 6px 24px rgba(4, 120, 87, 0.07);
  overflow: hidden;
  animation: cardFadeIn 0.4s ease-out both;
}
@keyframes cardFadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to   { opacity: 1; transform: translateY(0); }
}

/* ── Card Head ── */
.visit-card-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.1rem 1.25rem;
  background: rgba(240, 253, 244, 0.6);
  border-bottom: 1px solid rgba(5, 150, 105, 0.08);
}
.visit-head-left { display: flex; align-items: center; gap: 0.9rem; }

.visit-date-badge {
  width: 48px;
  height: 52px;
  background: linear-gradient(135deg, #059669, #047857);
  border-radius: 14px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 10px rgba(5, 150, 105, 0.35);
  flex-shrink: 0;
}
.vdb-day { font-family: 'Outfit', sans-serif; font-size: 1.25rem; font-weight: 900; color: white; line-height: 1; }
.vdb-mon { font-size: 0.6rem; color: rgba(255,255,255,0.8); font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; }

.visit-year { font-size: 0.72rem; color: #9ca3af; font-weight: 600; display: block; margin-bottom: 0.25rem; }

.visit-pet-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  background: rgba(5, 150, 105, 0.1);
  border: 1px solid rgba(5, 150, 105, 0.2);
  color: #047857;
  font-size: 0.8rem;
  font-weight: 700;
  padding: 0.2rem 0.65rem;
  border-radius: 9999px;
}

.doctor-pill {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  font-size: 0.75rem;
  color: #4b5563;
  font-weight: 700;
  background: rgba(107, 114, 128, 0.08);
  padding: 0.3rem 0.7rem;
  border-radius: 9999px;
}

/* ── Card Body ── */
.visit-card-body {
  padding: 1.25rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.info-row { display: flex; flex-direction: column; gap: 0.3rem; }
.info-label { font-size: 0.72rem; font-weight: 700; color: #9ca3af; text-transform: uppercase; letter-spacing: 0.05em; }
.info-value { font-size: 0.92rem; color: #1f2937; margin: 0; line-height: 1.45; }
.bold-val { font-weight: 700; }

/* Diagnosis */
.diagnosis-block {
  background: linear-gradient(135deg, rgba(238, 242, 255, 0.8), rgba(224, 231, 255, 0.5));
  border: 1px solid rgba(199, 210, 254, 0.7);
  border-radius: 16px;
  padding: 1rem;
}
.diag-header {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin-bottom: 0.4rem;
}
.diag-icon { font-size: 1rem; }
.diag-title { font-size: 0.75rem; font-weight: 700; color: #4338ca; text-transform: uppercase; letter-spacing: 0.05em; flex: 1; }
.diag-status-pill {
  font-size: 0.65rem;
  font-weight: 700;
  background: rgba(99, 102, 241, 0.15);
  color: #4338ca;
  padding: 0.15rem 0.5rem;
  border-radius: 9999px;
}
.diag-name { font-size: 0.95rem; font-weight: 700; color: #3730a3; margin: 0; }

/* Vet Note Block */
.vet-note-block {
  background: #fffbeb;
  border: 1px solid #fde68a;
  border-radius: 16px;
  padding: 1rem;
}
.vnb-header { display: flex; align-items: center; gap: 0.4rem; margin-bottom: 0.4rem; }
.vnb-header strong { font-size: 0.82rem; color: #78350f; }
.vet-note-block p { font-size: 0.88rem; color: #92400e; margin: 0; line-height: 1.45; }

/* Payment Row */
.payment-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: rgba(16, 185, 129, 0.06);
  border: 1px solid rgba(16, 185, 129, 0.15);
  border-radius: 14px;
  padding: 0.75rem 1rem;
}
.payment-label { font-size: 0.8rem; font-weight: 700; color: #059669; }
.payment-amount {
  font-family: 'Outfit', sans-serif;
  font-size: 1.1rem;
  font-weight: 900;
  color: #047857;
}

/* Gallery */
.gallery-section { display: flex; flex-direction: column; gap: 0.5rem; }
.gallery-grid { display: flex; flex-wrap: wrap; gap: 0.6rem; }
.gallery-thumb {
  width: 80px;
  height: 80px;
  border-radius: 14px;
  overflow: hidden;
  position: relative;
  cursor: pointer;
  transition: transform 0.2s;
  box-shadow: 0 4px 10px rgba(0,0,0,0.1);
}
.gallery-thumb:active { transform: scale(0.93); }
.gallery-thumb img { width: 100%; height: 100%; object-fit: cover; display: block; }
.gallery-overlay {
  position: absolute;
  inset: 0;
  background: rgba(0,0,0,0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: opacity 0.2s;
  font-size: 1.25rem;
}
.gallery-thumb:hover .gallery-overlay { opacity: 1; }
</style>
