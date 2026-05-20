<template>
  <div class="client-dashboard">
    <!-- Yükleniyor Durumu -->
    <div v-if="loading" class="loading-container">
      <div class="spinner"></div>
      <p>Verileriniz yükleniyor...</p>
    </div>

    <!-- Hata Durumu -->
    <div v-else-if="error" class="error-container">
      <span class="error-icon">⚠️</span>
      <p>{{ error }}</p>
      <button class="client-btn" @click="fetchData">Yeniden Dene</button>
    </div>

    <!-- Ana Panel İçeriği -->
    <div v-else class="portal-content">
      <!-- Hoşgeldiniz & Karşılama Kartı -->
      <div class="welcome-card client-card">
        <div class="welcome-text">
          <span class="wave">👋</span>
          <h1>Merhaba, {{ profile?.fullName }}!</h1>
          <p>Dostlarınızın tüm sağlık durumunu buradan kolayca takip edebilirsiniz.</p>
        </div>
      </div>

      <!-- Kasa / Bakiye Kartı -->
      <div class="balance-card client-card" :class="{ 'has-debt': profile?.outstandingBalance > 0 }">
        <div class="balance-header">
          <span class="card-icon">💳</span>
          <h3>Hesap Bakiyesi</h3>
        </div>
        <div class="balance-body">
          <div class="amount">
            {{ formatMoney(profile?.outstandingBalance) }}
          </div>
          <p v-if="profile?.outstandingBalance > 0" class="balance-warning">
            Ödenmemiş bakiyeniz bulunmaktadır. Ödeme detayları için klinik ile iletişime geçebilirsiniz.
          </p>
          <p v-else class="balance-success">
            Ödenmemiş borcunuz bulunmamaktadır. Teşekkür ederiz! 🎉
          </p>
        </div>
      </div>

      <!-- Sevimli Dostlarım (Pet Listesi) -->
      <div class="section-title-wrap">
        <h2>Sevimli Dostlarım ({{ pets.length }})</h2>
        <RouterLink to="/client/pets" class="see-all-link">Tümünü Gör</RouterLink>
      </div>

      <div class="pets-mini-grid">
        <div v-for="pet in pets.slice(0, 3)" :key="pet.id" class="pet-summary-card client-card">
          <div class="pet-avatar-wrap">
            <span class="pet-emoji">{{ getPetEmoji(pet.species) }}</span>
          </div>
          <div class="pet-meta">
            <h4>{{ pet.name }}</h4>
            <span class="breed-badge">{{ pet.species }} <span v-if="pet.breed">| {{ pet.breed }}</span></span>
            <p class="age-text">{{ formatAge(pet.ageYears, pet.ageMonths) }}</p>
          </div>
        </div>
      </div>

      <!-- Yaklaşan Hatırlatıcılar & Aşılar -->
      <div class="section-title-wrap mt-2">
        <h2>Yaklaşan Aşı ve Kontroller</h2>
      </div>

      <div class="reminders-list">
        <div v-if="activeReminders.length === 0" class="empty-state client-card">
          <span class="empty-icon">📅</span>
          <p>Yakın zamanda planlanmış aşı veya randevunuz bulunmamaktadır.</p>
        </div>
        <div v-else v-for="rem in activeReminders" :key="rem.id" class="reminder-card client-card">
          <div class="reminder-badge" :class="getReminderBadgeClass(rem.dueDate)">
            <span>{{ formatDate(rem.dueDate) }}</span>
          </div>
          <div class="reminder-details">
            <strong>{{ rem.petName }}</strong>
            <p>{{ rem.purpose || 'Aşı / Rutin Kontrol Ziyareti' }}</p>
          </div>
        </div>
      </div>

      <!-- Hızlı İletişim & Yol Tarifi -->
      <div class="section-title-wrap mt-2">
        <h2>Kliniğimize Ulaşın</h2>
      </div>

      <div class="contact-grid">
        <a href="tel:+905555555555" class="contact-action-card client-card">
          <span class="contact-icon">📞</span>
          <strong>Hemen Ara</strong>
          <p>Sorularınız veya randevu talepleriniz için</p>
        </a>

        <a href="https://maps.google.com/?q=BullVet+Veteriner+Kliniği" target="_blank" class="contact-action-card client-card">
          <span class="contact-icon">📍</span>
          <strong>Yol Tarifi Al</strong>
          <p>Kliniğimize en hızlı rota</p>
        </a>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { http } from '@/api/http'

const loading = ref(true)
const error = ref('')

const profile = ref(null)
const pets = ref([])
const reminders = ref([])

const activeReminders = computed(() => {
  return reminders.value
    .filter(r => !r.isCompleted)
    .slice(0, 5) // limit to nearest 5
})

async function fetchData() {
  loading.value = true
  error.value = ''

  try {
    const [profileRes, petsRes, remindersRes] = await Promise.all([
      http.get('/clientportal/profile'),
      http.get('/clientportal/pets'),
      http.get('/clientportal/reminders'),
    ])

    profile.value = profileRes.data
    pets.value = petsRes.data
    reminders.value = remindersRes.data
  } catch (err) {
    console.error('[CLIENT PORTAL FETCH ERROR]', err)
    error.value = 'Bilgileriniz yüklenirken bir sorun oluştu. Lütfen oturumunuzu kontrol edip tekrar deneyin.'
  } finally {
    loading.value = false
  }
}

function formatMoney(val) {
  return `${Number(val || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ₺`
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

function getReminderBadgeClass(dateStr) {
  try {
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    const [y, m, d] = dateStr.split('-')
    const due = new Date(y, m - 1, d)
    due.setHours(0, 0, 0, 0)

    const diffDays = Math.ceil((due - today) / (1000 * 60 * 60 * 24))
    if (diffDays < 0) return 'badge-expired'
    if (diffDays <= 3) return 'badge-urgent'
    return 'badge-normal'
  } catch {
    return 'badge-normal'
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.client-dashboard {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
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

.welcome-card {
  background: linear-gradient(135deg, rgba(5, 150, 105, 0.15) 0%, rgba(4, 120, 87, 0.05) 100%);
  border: 1px solid rgba(5, 150, 105, 0.15);
}

.welcome-text h1 {
  font-family: 'Outfit', sans-serif;
  font-size: 1.75rem;
  font-weight: 800;
  margin: 0.5rem 0;
  color: #065f46;
  letter-spacing: -0.04em;
}

.welcome-text p {
  font-size: 0.95rem;
  margin: 0;
  color: #047857;
  line-height: 1.5;
}

.wave {
  font-size: 1.5rem;
  display: inline-block;
  animation: wave 2.5s infinite;
  transform-origin: 70% 70%;
}

@keyframes wave {
  0%, 100% { transform: rotate(0deg); }
  10% { transform: rotate(14deg); }
  20% { transform: rotate(-8deg); }
  30% { transform: rotate(14deg); }
  40% { transform: rotate(-4deg); }
  50% { transform: rotate(10deg); }
  60% { transform: rotate(0deg); }
}

.balance-card {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  border-left: 6px solid #10b981;
}

.balance-card.has-debt {
  border-left-color: #f59e0b;
}

.balance-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.balance-header h3 {
  font-size: 1rem;
  font-weight: 700;
  margin: 0;
  color: #374151;
}

.balance-body .amount {
  font-family: 'Outfit', sans-serif;
  font-size: 2.25rem;
  font-weight: 800;
  color: #111827;
  letter-spacing: -0.03em;
  margin-bottom: 0.5rem;
}

.balance-warning {
  font-size: 0.85rem;
  color: #b45309;
  margin: 0;
  line-height: 1.4;
}

.balance-success {
  font-size: 0.85rem;
  color: #047857;
  margin: 0;
  line-height: 1.4;
}

.section-title-wrap {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: 1.5rem 0.5rem 0.75rem;
}

.section-title-wrap h2 {
  font-family: 'Outfit', sans-serif;
  font-size: 1.25rem;
  font-weight: 800;
  color: #111827;
  margin: 0;
}

.see-all-link {
  font-size: 0.875rem;
  font-weight: 700;
  color: #059669;
  text-decoration: none;
  transition: opacity 0.2s;
}

.see-all-link:hover {
  opacity: 0.8;
}

.pets-mini-grid {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.pet-summary-card {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  padding: 1.25rem;
  margin-bottom: 0;
}

.pet-avatar-wrap {
  width: 50px;
  height: 50px;
  background: #ecfdf5;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid #a7f3d0;
}

.pet-emoji {
  font-size: 1.75rem;
}

.pet-meta h4 {
  font-size: 1.1rem;
  font-weight: 700;
  margin: 0 0 0.25rem;
  color: #111827;
}

.breed-badge {
  font-size: 0.75rem;
  color: #6b7280;
  font-weight: 600;
}

.age-text {
  font-size: 0.8rem;
  color: #059669;
  font-weight: 700;
  margin: 0.25rem 0 0;
}

.reminders-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.empty-state {
  text-align: center;
  padding: 2rem 1.5rem;
}

.empty-icon {
  font-size: 2rem;
  display: block;
  margin-bottom: 0.5rem;
}

.empty-state p {
  font-size: 0.9rem;
  color: #6b7280;
  margin: 0;
}

.reminder-card {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  padding: 1rem 1.25rem;
  margin-bottom: 0;
}

.reminder-badge {
  padding: 0.5rem 0.75rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 700;
  text-align: center;
  min-width: 80px;
}

.badge-normal {
  background: #ecfdf5;
  color: #047857;
  border: 1px solid #a7f3d0;
}

.badge-urgent {
  background: #fffbeb;
  color: #b45309;
  border: 1px solid #fde68a;
  animation: pulseBg 2s infinite ease-in-out;
}

.badge-expired {
  background: #fef2f2;
  color: #b91c1c;
  border: 1px solid #fca5a5;
}

@keyframes pulseBg {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.03); }
}

.reminder-details strong {
  font-size: 0.95rem;
  color: #111827;
}

.reminder-details p {
  font-size: 0.85rem;
  color: #4b5563;
  margin: 0.25rem 0 0;
}

.contact-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.contact-action-card {
  text-decoration: none;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 1.5rem 1rem;
}

.contact-icon {
  font-size: 2rem;
}

.contact-action-card strong {
  font-size: 1.1rem;
  color: #065f46;
  font-weight: 800;
}

.contact-action-card p {
  font-size: 0.75rem;
  color: #6b7280;
  margin: 0;
  line-height: 1.3;
}

.mt-2 {
  margin-top: 2rem;
}
</style>
