<template>
  <div class="client-pets">
    <header class="view-header">
      <h1>Dostlarım 🐾</h1>
      <p>Kayıtlı evcil hayvanlarınızın aşı takvimini ve sağlık kartlarını buradan yönetebilirsiniz.</p>
    </header>

    <!-- Yükleniyor Durumu -->
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Dostlarınızın bilgileri yükleniyor...</p>
    </div>

    <!-- Hata Durumu -->
    <div v-else-if="error" class="error-container">
      <span class="error-icon">⚠️</span>
      <p>{{ error }}</p>
      <button class="client-btn" @click="fetchData">Yeniden Dene</button>
    </div>

    <div v-else>
      <!-- Boş Durum -->
      <div v-if="pets.length === 0" class="empty-state client-card">
        <span class="empty-icon">🐈</span>
        <h3>Henüz Kayıtlı Dostunuz Yok</h3>
        <p>Sistemde kayıtlı evcil hayvanınız bulunamadı. Lütfen ekleme yapılması için veteriner hekiminizle görüşün.</p>
      </div>

      <!-- Evcil Hayvan Listesi -->
      <div v-else class="pets-list">
        <div v-for="pet in pets" :key="pet.id" class="pet-card client-card">
          <!-- Pet Genel Kartı -->
          <div class="pet-card-header" @click="toggleExpand(pet.id)">
            <div class="pet-avatar-wrap">
              <span class="pet-emoji">{{ getPetEmoji(pet.species) }}</span>
            </div>
            <div class="pet-main-info">
              <h3>{{ pet.name }}</h3>
              <span class="breed-badge">{{ pet.species }} <span v-if="pet.breed">| {{ pet.breed }}</span></span>
              <p class="age-text">{{ formatAge(pet.ageYears, pet.ageMonths) }}</p>
            </div>
            <button class="expand-arrow" :class="{ rotated: expandedPetId === pet.id }">
              ▼
            </button>
          </div>

          <!-- Genişletilmiş Sağlık Karnesi / Aşı Takvimi Bölümü -->
          <div v-if="expandedPetId === pet.id" class="pet-card-details">
            <!-- Hakkında Notları -->
            <div v-if="pet.notes" class="pet-notes-section">
              <strong>Hekim Notları:</strong>
              <p>{{ pet.notes }}</p>
            </div>

            <!-- Aşı & Kontrol Karnesi -->
            <div class="vaccine-timeline-section">
              <h4>📋 Aşı ve Kontrol Takvimi</h4>

              <div v-if="getPetReminders(pet.name).length === 0" class="no-vaccine-hint">
                Bu pet için planlanmış aşı/kontrol bulunmamaktadır.
              </div>
              <div v-else class="timeline">
                <div v-for="rem in getPetReminders(pet.name)" :key="rem.id" class="timeline-item" :class="{ completed: rem.isCompleted }">
                  <div class="timeline-dot-wrap">
                    <span class="timeline-dot"></span>
                  </div>
                  <div class="timeline-content">
                    <div class="timeline-header-wrap">
                      <span class="timeline-purpose">{{ rem.purpose || 'Rutin Sağlık Kontrolü' }}</span>
                      <span class="timeline-status-badge" :class="rem.isCompleted ? 'completed' : 'pending'">
                        {{ rem.isCompleted ? 'Yapıldı' : 'Bekliyor' }}
                      </span>
                    </div>
                    <span class="timeline-date">Tarih: {{ formatDate(rem.dueDate) }}</span>
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
import { ref, onMounted } from 'vue'
import { http } from '@/api/http'

const loading = ref(true)
const error = ref('')

const pets = ref([])
const reminders = ref([])
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
    
    // Auto-expand first pet if available
    if (pets.value.length > 0) {
      expandedPetId.value = pets.value[0].id
    }
  } catch (err) {
    console.error('[CLIENT PORTAL PETS FETCH ERROR]', err)
    error.value = 'Hayvan bilgileriniz yüklenirken bir sorun oluştu.'
  } finally {
    loading.value = false
  }
}

function toggleExpand(id) {
  if (expandedPetId.value === id) {
    expandedPetId.value = null
  } else {
    expandedPetId.value = id
  }
}

function getPetEmoji(species) {
  const s = String(species || '').toLowerCase()
  if (s.includes('kedi') || s.includes('cat')) return '🐱'
  if (s.includes('köpek') || s.includes('dog')) return '🐶'
  if (s.includes('kuş') || s.includes('bird')) return '🦜'
  if (s.includes('tavşan') || s.includes('rabbit')) return '🐰'
  return '🐾'
}

function formatAge(years, months) {
  const parts = []
  if (years && years > 0) parts.push(`${years} Yıl`)
  if (months && months > 0) parts.push(`${months} Ay`)
  return parts.length > 0 ? parts.join(', ') : 'Yeni doğmuş'
}

function formatDate(dateOnlyString) {
  if (!dateOnlyString) return ''
  try {
    const [year, month, day] = dateOnlyString.split('-')
    return `${day}.${month}.${year}`
  } catch {
    return dateOnlyString
  }
}

function getPetReminders(petName) {
  if (!petName) return []
  return reminders.value.filter(
    r => String(r.petName).toLowerCase() === String(petName).toLowerCase()
  )
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.client-pets {
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

.pets-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.pet-card {
  padding: 0;
  overflow: hidden;
  transition: all 0.3s ease;
}

.pet-card-header {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  padding: 1.25rem;
  cursor: pointer;
  user-select: none;
}

.pet-avatar-wrap {
  width: 54px;
  height: 54px;
  background: #ecfdf5;
  border-radius: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid #a7f3d0;
}

.pet-emoji {
  font-size: 2rem;
}

.pet-main-info {
  flex: 1;
}

.pet-main-info h3 {
  font-family: 'Outfit', sans-serif;
  font-size: 1.2rem;
  font-weight: 800;
  margin: 0 0 0.25rem;
  color: #111827;
  letter-spacing: -0.02em;
}

.breed-badge {
  font-size: 0.75rem;
  color: #4b5563;
  font-weight: 600;
  background: #f3f4f6;
  padding: 0.2rem 0.5rem;
  border-radius: 8px;
}

.age-text {
  font-size: 0.8rem;
  color: #059669;
  font-weight: 700;
  margin: 0.4rem 0 0;
}

.expand-arrow {
  background: none;
  border: none;
  font-size: 0.8rem;
  color: #9ca3af;
  cursor: pointer;
  transition: transform 0.3s ease;
}

.expand-arrow.rotated {
  transform: rotate(180deg);
}

.pet-card-details {
  border-top: 1px solid rgba(22, 101, 52, 0.1);
  background: rgba(249, 250, 251, 0.5);
  padding: 1.25rem;
  animation: slideDown 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes slideDown {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}

.pet-notes-section {
  background: #fffbeb;
  border: 1px solid #fde68a;
  border-radius: 16px;
  padding: 1rem;
  margin-bottom: 1.25rem;
  font-size: 0.9rem;
  color: #78350f;
  line-height: 1.5;
}

.pet-notes-section strong {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 700;
}

.pet-notes-section p {
  margin: 0;
}

.vaccine-timeline-section h4 {
  font-family: 'Outfit', sans-serif;
  font-size: 1.05rem;
  font-weight: 800;
  color: #1f2937;
  margin: 0 0 1rem;
}

.no-vaccine-hint {
  font-size: 0.85rem;
  color: #6b7280;
  text-align: center;
  padding: 1.5rem;
  background: white;
  border-radius: 16px;
  border: 1px dashed #e5e7eb;
}

.timeline {
  display: flex;
  flex-direction: column;
  position: relative;
  padding-left: 1.5rem;
}

.timeline::before {
  content: '';
  position: absolute;
  left: 5px;
  top: 8px;
  bottom: 8px;
  width: 2px;
  background: #cbd5e1;
}

.timeline-item {
  position: relative;
  margin-bottom: 1.25rem;
}

.timeline-item:last-child {
  margin-bottom: 0;
}

.timeline-dot-wrap {
  position: absolute;
  left: -20px;
  top: 4px;
}

.timeline-dot {
  display: block;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #94a3b8;
  border: 3px solid white;
  box-shadow: 0 0 0 1px #cbd5e1;
  transition: all 0.3s ease;
}

.timeline-item.completed .timeline-dot {
  background: #10b981;
  box-shadow: 0 0 0 1px #a7f3d0;
}

.timeline-content {
  background: white;
  border: 1px solid #e5e7eb;
  border-radius: 16px;
  padding: 0.85rem 1rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
}

.timeline-header-wrap {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.25rem;
}

.timeline-purpose {
  font-size: 0.9rem;
  font-weight: 700;
  color: #1f2937;
}

.timeline-status-badge {
  font-size: 0.7rem;
  font-weight: 700;
  padding: 0.15rem 0.5rem;
  border-radius: 9999px;
}

.timeline-status-badge.completed {
  background: #d1fae5;
  color: #065f46;
}

.timeline-status-badge.pending {
  background: #fef3c7;
  color: #92400e;
}

.timeline-date {
  font-size: 0.75rem;
  color: #6b7280;
  font-weight: 600;
}
</style>
