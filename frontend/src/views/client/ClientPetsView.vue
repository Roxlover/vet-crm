<template>
  <div class="client-pets">
    <!-- Sticky Header -->
    <div class="page-header">
      <h1>Dostlarım</h1>
      <p>Sağlık kartları ve aşı takvimleri</p>
    </div>

    <!-- Yükleniyor -->
    <div v-if="loading" class="loading-state">
      <div class="paw-loader"><span>🐾</span></div>
      <p>Dostlarınız yükleniyor...</p>
    </div>

    <!-- Hata -->
    <div v-else-if="error" class="error-state">
      <div class="error-icon-wrap">⚠️</div>
      <p>{{ error }}</p>
      <button class="client-btn" @click="fetchData">Tekrar Dene</button>
    </div>

    <div v-else>
      <!-- Boş Durum -->
      <div v-if="pets.length === 0" class="empty-state-card">
        <div class="empty-graphic">🐈</div>
        <h3>Henüz Kayıtlı Dostunuz Yok</h3>
        <p>Veteriner hekiminizden hayvanınızı sisteme eklemesini isteyin.</p>
      </div>

      <!-- Pet Listesi -->
      <div v-else class="pets-list">
        <div
          v-for="pet in pets"
          :key="pet.id"
          class="pet-card"
          :class="{ expanded: expandedPetId === pet.id }"
        >
          <!-- Pet Başlığı (tıklanabilir) -->
          <div class="pet-card-trigger" @click="toggleExpand(pet.id)">
            <!-- Avatar -->
            <div class="pet-card-avatar" :style="{ background: getPetGradient(pet.species) }">
              <span>{{ getPetEmoji(pet.species) }}</span>
            </div>

            <!-- Bilgi -->
            <div class="pet-card-info">
              <h3>{{ pet.name }}</h3>
              <div class="pet-tags">
                <span class="pet-tag">{{ pet.species }}</span>
                <span v-if="pet.breed" class="pet-tag">{{ pet.breed }}</span>
              </div>
              <p class="pet-age">{{ formatAge(pet.ageYears, pet.ageMonths) }}</p>
            </div>

            <!-- Sağlık Durumu & Ok -->
            <div class="pet-card-right">
              <span class="health-dot" :class="getHealthClass(pet)"></span>
              <div class="expand-btn" :class="{ rotated: expandedPetId === pet.id }">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                  <polyline points="6 9 12 15 18 9"></polyline>
                </svg>
              </div>
            </div>
          </div>

          <!-- Genişletilmiş Alan -->
          <Transition name="slide-down">
            <div v-if="expandedPetId === pet.id" class="pet-detail-panel">
              <!-- Sağlık Özeti -->
              <div class="detail-chips">
                <div class="detail-chip chip-green">
                  <span class="chip-icon">🏥</span>
                  <div>
                    <span class="chip-label">Muayene</span>
                    <span class="chip-val">{{ getPetVisitCount(pet.name) }}</span>
                  </div>
                </div>
                <div class="detail-chip chip-purple">
                  <span class="chip-icon">💉</span>
                  <div>
                    <span class="chip-label">Aşı</span>
                    <span class="chip-val">{{ getPetReminderCount(pet.name) }}</span>
                  </div>
                </div>
              </div>

              <!-- Veteriner Notu -->
              <div v-if="pet.clientNotes" class="vet-note">
                <div class="vet-note-header">
                  <span>📝</span>
                  <strong>Veteriner Notu</strong>
                </div>
                <p>{{ pet.clientNotes }}</p>
              </div>

              <!-- Aşı Takvimi -->
              <div class="vaccine-section">
                <h4 class="vaccine-section-title">Aşı & Kontrol Takvimi</h4>

                <div v-if="getPetReminders(pet.name).length === 0" class="no-vaccine">
                  <span>✅</span> Planlanmış aşı bulunmuyor.
                </div>

                <div v-else class="timeline">
                  <div
                    v-for="rem in getPetReminders(pet.name)"
                    :key="rem.id"
                    class="timeline-item"
                    :class="{ 'tl-done': rem.isCompleted, 'tl-pending': !rem.isCompleted }"
                  >
                    <div class="tl-dot-col">
                      <div class="tl-dot">
                        <span v-if="rem.isCompleted">✓</span>
                      </div>
                      <div class="tl-line"></div>
                    </div>
                    <div class="tl-content">
                      <div class="tl-row">
                        <span class="tl-purpose">{{ rem.purpose || 'Rutin Kontrol' }}</span>
                        <span class="tl-badge" :class="rem.isCompleted ? 'badge-done' : 'badge-wait'">
                          {{ rem.isCompleted ? 'Yapıldı' : 'Bekliyor' }}
                        </span>
                      </div>
                      <span class="tl-date">📅 {{ formatDate(rem.dueDate) }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </Transition>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { http } from '@/api/http'

const loading = ref(true)
const error = ref('')
const pets = ref([])
const reminders = ref([])
const visits = ref([])
const expandedPetId = ref(null)

async function fetchData() {
  loading.value = true
  error.value = ''
  try {
    const [petsRes, remindersRes] = await Promise.all([
      http.get('/clientportal/pets'),
      http.get('/clientportal/reminders'),
    ])
    pets.value = petsRes.data
    reminders.value = remindersRes.data

    try {
      const visitsRes = await http.get('/clientportal/visits')
      visits.value = visitsRes.data || []
    } catch { visits.value = [] }

    if (pets.value.length > 0) expandedPetId.value = pets.value[0].id
  } catch (err) {
    error.value = 'Hayvan bilgileri yüklenirken bir hata oluştu.'
  } finally {
    loading.value = false
  }
}

function toggleExpand(id) {
  expandedPetId.value = expandedPetId.value === id ? null : id
}

function getPetEmoji(species) {
  const s = String(species || '').toLowerCase()
  if (s.includes('kedi') || s.includes('cat')) return '🐱'
  if (s.includes('köpek') || s.includes('dog')) return '🐶'
  if (s.includes('kuş') || s.includes('bird')) return '🦜'
  if (s.includes('tavşan') || s.includes('rabbit')) return '🐰'
  if (s.includes('hamster')) return '🐹'
  return '🐾'
}

function getPetGradient(species) {
  const s = String(species || '').toLowerCase()
  if (s.includes('kedi') || s.includes('cat')) return 'linear-gradient(135deg, #f3e8ff, #ddd6fe)'
  if (s.includes('köpek') || s.includes('dog')) return 'linear-gradient(135deg, #fef3c7, #fde68a)'
  if (s.includes('kuş') || s.includes('bird')) return 'linear-gradient(135deg, #dbeafe, #bfdbfe)'
  if (s.includes('tavşan') || s.includes('rabbit')) return 'linear-gradient(135deg, #fce7f3, #fbcfe8)'
  return 'linear-gradient(135deg, #d1fae5, #a7f3d0)'
}

function getHealthClass(pet) {
  const hasActive = reminders.value.some(r => !r.isCompleted &&
    String(r.petName).toLowerCase() === String(pet.name).toLowerCase())
  return hasActive ? 'dot-active' : 'dot-clear'
}

function formatAge(years, months) {
  const parts = []
  if (years && years > 0) parts.push(`${years} yıl`)
  if (months && months > 0) parts.push(`${months} ay`)
  return parts.length > 0 ? parts.join(' ') : 'Yeni doğmuş'
}

function formatDate(dateOnlyString) {
  if (!dateOnlyString) return ''
  try {
    const [year, month, day] = dateOnlyString.split('-')
    return `${day}.${month}.${year}`
  } catch { return dateOnlyString }
}

function getPetReminders(petName) {
  if (!petName) return []
  return reminders.value.filter(r =>
    String(r.petName).toLowerCase() === String(petName).toLowerCase()
  )
}

function getPetReminderCount(petName) {
  return getPetReminders(petName).length
}

function getPetVisitCount(petName) {
  if (!petName) return 0
  return visits.value.filter(v =>
    String(v.petName).toLowerCase() === String(petName).toLowerCase()
  ).length
}

onMounted(() => fetchData())
</script>

<style scoped>
.client-pets {
  animation: fadeUp 0.4s ease-out both;
}

@keyframes fadeUp {
  from { opacity: 0; transform: translateY(14px); }
  to   { opacity: 1; transform: translateY(0); }
}

/* ── Header ── */
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
.page-header p {
  font-size: 0.9rem;
  color: #059669;
  margin: 0;
  font-weight: 500;
}

/* ── Loading ── */
.loading-state {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center; min-height: 55vh; gap: 1rem; color: #047857;
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

/* ── Error ── */
.error-state {
  display: flex; flex-direction: column; align-items: center;
  justify-content: center; min-height: 55vh; gap: 1rem; text-align: center;
}
.error-icon-wrap { font-size: 3rem; }

/* ── Empty ── */
.empty-state-card {
  background: rgba(255,255,255,0.85);
  backdrop-filter: blur(12px);
  border-radius: 28px;
  padding: 3rem 2rem;
  text-align: center;
  border: 1px solid rgba(255,255,255,0.6);
  box-shadow: 0 8px 24px rgba(4, 120, 87, 0.07);
}
.empty-graphic { font-size: 4rem; margin-bottom: 1rem; display: block; }
.empty-state-card h3 { font-family: 'Outfit', sans-serif; font-size: 1.2rem; font-weight: 800; color: #111827; margin: 0 0 0.5rem; }
.empty-state-card p { font-size: 0.9rem; color: #6b7280; margin: 0; line-height: 1.5; }

/* ── Pets List ── */
.pets-list { display: flex; flex-direction: column; gap: 1rem; }

.pet-card {
  background: rgba(255,255,255,0.85);
  backdrop-filter: blur(16px);
  border-radius: 24px;
  border: 1px solid rgba(255,255,255,0.6);
  box-shadow: 0 6px 20px rgba(4, 120, 87, 0.06);
  overflow: hidden;
  transition: box-shadow 0.3s;
}
.pet-card.expanded {
  box-shadow: 0 12px 36px rgba(4, 120, 87, 0.14);
}

.pet-card-trigger {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1.25rem;
  cursor: pointer;
  user-select: none;
  transition: background 0.2s;
}
.pet-card-trigger:active { background: rgba(5, 150, 105, 0.04); }

.pet-card-avatar {
  width: 60px;
  height: 60px;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  flex-shrink: 0;
  box-shadow: 0 4px 12px rgba(0,0,0,0.08);
}

.pet-card-info { flex: 1; }
.pet-card-info h3 {
  font-family: 'Outfit', sans-serif;
  font-size: 1.15rem;
  font-weight: 800;
  color: #111827;
  margin: 0 0 0.4rem;
  letter-spacing: -0.02em;
}
.pet-tags { display: flex; flex-wrap: wrap; gap: 0.3rem; margin-bottom: 0.35rem; }
.pet-tag {
  font-size: 0.68rem;
  font-weight: 700;
  color: #059669;
  background: rgba(5, 150, 105, 0.1);
  padding: 0.15rem 0.5rem;
  border-radius: 9999px;
}
.pet-age { font-size: 0.78rem; color: #6b7280; margin: 0; font-weight: 600; }

.pet-card-right {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  flex-shrink: 0;
}

.health-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
}
.dot-active { background: #f59e0b; box-shadow: 0 0 6px rgba(245, 158, 11, 0.5); }
.dot-clear { background: #10b981; box-shadow: 0 0 6px rgba(16, 185, 129, 0.4); }

.expand-btn {
  color: #9ca3af;
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex; align-items: center;
}
.expand-btn.rotated { transform: rotate(180deg); color: #059669; }

/* ── Detail Panel ── */
.slide-down-enter-active { transition: all 0.35s cubic-bezier(0.16, 1, 0.3, 1); }
.slide-down-leave-active { transition: all 0.25s ease; }
.slide-down-enter-from { opacity: 0; transform: translateY(-10px); }
.slide-down-leave-to   { opacity: 0; transform: translateY(-8px); }

.pet-detail-panel {
  border-top: 1px solid rgba(5, 150, 105, 0.1);
  padding: 1.25rem;
  background: rgba(240, 253, 244, 0.4);
}

.detail-chips {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
  margin-bottom: 1.1rem;
}
.detail-chip {
  border-radius: 16px;
  padding: 0.9rem 1rem;
  display: flex;
  align-items: center;
  gap: 0.7rem;
}
.chip-green { background: rgba(16, 185, 129, 0.1); border: 1px solid rgba(16, 185, 129, 0.2); }
.chip-purple { background: rgba(139, 92, 246, 0.1); border: 1px solid rgba(139, 92, 246, 0.2); }
.chip-icon { font-size: 1.3rem; }
.chip-label { display: block; font-size: 0.68rem; color: #6b7280; font-weight: 600; text-transform: uppercase; letter-spacing: 0.04em; }
.chip-val {
  display: block;
  font-family: 'Outfit', sans-serif;
  font-size: 1.4rem;
  font-weight: 900;
  color: #111827;
  line-height: 1;
}
.chip-green .chip-val { color: #047857; }
.chip-purple .chip-val { color: #7c3aed; }

/* Vet Note */
.vet-note {
  background: #fffbeb;
  border: 1px solid #fde68a;
  border-radius: 16px;
  padding: 1rem;
  margin-bottom: 1.1rem;
}
.vet-note-header {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin-bottom: 0.5rem;
}
.vet-note-header strong { font-size: 0.85rem; color: #78350f; }
.vet-note p { font-size: 0.88rem; color: #92400e; margin: 0; line-height: 1.5; }

/* Vaccine section */
.vaccine-section-title {
  font-family: 'Outfit', sans-serif;
  font-size: 1rem;
  font-weight: 800;
  color: #1f2937;
  margin: 0 0 1rem;
}
.no-vaccine {
  font-size: 0.85rem;
  color: #6b7280;
  background: white;
  border-radius: 14px;
  padding: 1rem;
  text-align: center;
  border: 1px dashed #e5e7eb;
}

/* Timeline */
.timeline { display: flex; flex-direction: column; gap: 0; }

.timeline-item {
  display: flex;
  gap: 0.85rem;
  position: relative;
}

.tl-dot-col {
  display: flex;
  flex-direction: column;
  align-items: center;
  flex-shrink: 0;
  padding-top: 2px;
}
.tl-dot {
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: #e5e7eb;
  border: 2px solid white;
  box-shadow: 0 0 0 2px #e5e7eb;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.65rem;
  font-weight: 900;
  color: white;
  flex-shrink: 0;
  transition: all 0.3s;
}
.tl-done .tl-dot {
  background: #10b981;
  box-shadow: 0 0 0 2px rgba(16, 185, 129, 0.3);
}
.tl-pending .tl-dot {
  background: #f59e0b;
  box-shadow: 0 0 0 2px rgba(245, 158, 11, 0.25);
}
.tl-line {
  width: 2px;
  flex: 1;
  background: #e5e7eb;
  margin: 4px 0;
  min-height: 12px;
}
.timeline-item:last-child .tl-line { display: none; }

.tl-content {
  flex: 1;
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 14px;
  padding: 0.75rem 1rem;
  margin-bottom: 0.75rem;
  box-shadow: 0 2px 6px rgba(0,0,0,0.025);
}
.tl-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.3rem;
}
.tl-purpose { font-size: 0.88rem; font-weight: 700; color: #1f2937; }
.tl-badge {
  font-size: 0.65rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 9999px;
}
.badge-done { background: #d1fae5; color: #065f46; }
.badge-wait { background: #fef3c7; color: #92400e; }
.tl-date { font-size: 0.75rem; color: #9ca3af; font-weight: 600; }
</style>
