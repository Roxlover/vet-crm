<template>
  <div class="client-dashboard">
    <!-- Yükleniyor Durumu -->
    <div v-if="loading" class="loading-state">
      <div class="paw-loader">
        <span>🐾</span>
      </div>
      <p>Bilgileriniz yükleniyor...</p>
    </div>

    <!-- Hata Durumu -->
    <div v-else-if="error" class="error-state">
      <div class="error-icon-wrap">⚠️</div>
      <p>{{ error }}</p>
      <button class="client-btn" @click="fetchData">Tekrar Dene</button>
    </div>

    <!-- Ana İçerik -->
    <div v-else class="dashboard-content">

      <!-- Hero Karşılama Bölümü -->
      <div class="hero-section">
        <div class="hero-bg-orb orb-1"></div>
        <div class="hero-bg-orb orb-2"></div>
        <div class="hero-avatar">
          <span class="wave-emoji">👋</span>
        </div>
        <div class="hero-text">
          <p class="hero-greeting">Merhaba,</p>
          <h1 class="hero-name">{{ profile?.fullName?.split(' ')[0] || 'Değerli Üyemiz' }}</h1>
          <p class="hero-sub">Dostlarınız sizi bekliyor 🐾</p>
        </div>
      </div>

      <!-- Özet Sayaçlar -->
      <div class="stats-row">
        <div class="stat-pill stat-pets">
          <span class="stat-num">{{ pets.length }}</span>
          <span class="stat-label">Dostum</span>
        </div>
        <div class="stat-pill stat-visits">
          <span class="stat-num">{{ totalVisits }}</span>
          <span class="stat-label">Muayene</span>
        </div>
        <div class="stat-pill stat-reminders">
          <span class="stat-num">{{ activeReminders.length }}</span>
          <span class="stat-label">Hatırlatıcı</span>
        </div>
      </div>

      <!-- Bakiye Kartı -->
      <div class="balance-card" :class="{ 'debt-mode': profile?.outstandingBalance > 0 }">
        <div class="balance-card-inner">
          <div class="balance-left">
            <div class="balance-label">
              <span class="balance-icon">{{ profile?.outstandingBalance > 0 ? '💳' : '✅' }}</span>
              Hesap Bakiyesi
            </div>
            <div class="balance-amount">
              {{ formatMoney(profile?.outstandingBalance) }}
            </div>
            <p class="balance-status" :class="profile?.outstandingBalance > 0 ? 'status-warn' : 'status-ok'">
              {{ profile?.outstandingBalance > 0 ? 'Bekleyen ödemeniz var' : 'Hesabınız temiz 🎉' }}
            </p>
          </div>
          <div class="balance-deco">
            <div class="balance-ring ring-1"></div>
            <div class="balance-ring ring-2"></div>
          </div>
        </div>
      </div>

      <!-- Sevimli Dostlarım -->
      <div class="section-block">
        <div class="section-header">
          <h2 class="section-title">Sevimli Dostlarım</h2>
          <RouterLink to="/client/pets" class="section-link">Tümü →</RouterLink>
        </div>

        <div v-if="pets.length === 0" class="empty-mini">
          <span>🐈</span> Kayıtlı hayvanınız bulunamadı.
        </div>

        <div v-else class="pets-scroll-row">
          <div v-for="pet in pets.slice(0, 4)" :key="pet.id" class="pet-bubble">
            <div class="pet-bubble-avatar" :style="{ background: getPetGradient(pet.species) }">
              <span class="pet-bubble-emoji">{{ getPetEmoji(pet.species) }}</span>
            </div>
            <span class="pet-bubble-name">{{ pet.name }}</span>
            <span class="pet-bubble-breed">{{ pet.species }}</span>
          </div>
        </div>
      </div>

      <!-- Yaklaşan Hatırlatıcılar -->
      <div class="section-block">
        <div class="section-header">
          <h2 class="section-title">Yaklaşan Kontroller</h2>
          <span class="reminder-count-badge" v-if="activeReminders.length > 0">{{ activeReminders.length }}</span>
        </div>

        <div v-if="activeReminders.length === 0" class="empty-reminder-card">
          <span class="empty-reminder-icon">📅</span>
          <p>Yakın zamanda planlanmış randevu veya aşı bulunmuyor.</p>
        </div>

        <div v-else class="reminders-stack">
          <div
            v-for="rem in activeReminders"
            :key="rem.id"
            class="reminder-item"
            :class="getReminderClass(rem.dueDate)"
          >
            <div class="reminder-left">
              <div class="reminder-icon-wrap">
                <span>{{ getReminderEmoji(rem.dueDate) }}</span>
              </div>
            </div>
            <div class="reminder-body">
              <strong>{{ rem.petName }}</strong>
              <p>{{ rem.purpose || 'Aşı / Rutin Kontrol' }}</p>
            </div>
            <div class="reminder-date-wrap">
              <span class="reminder-date-text">{{ formatDate(rem.dueDate) }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Hızlı Erişim -->
      <div class="section-block">
        <h2 class="section-title" style="margin-bottom: 1rem;">Bize Ulaşın</h2>
        <div class="quick-actions">
          <a href="tel:+905555555555" class="quick-action-card qa-call">
            <div class="qa-icon">📞</div>
            <div class="qa-text">
              <strong>Hemen Ara</strong>
              <span>Randevu & Destek</span>
            </div>
            <span class="qa-arrow">→</span>
          </a>
          <a href="https://maps.google.com/?q=BullVet+Veteriner+Kliniği" target="_blank" class="quick-action-card qa-map">
            <div class="qa-icon">📍</div>
            <div class="qa-text">
              <strong>Yol Tarifi</strong>
              <span>Kliniğimize git</span>
            </div>
            <span class="qa-arrow">→</span>
          </a>
        </div>
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
const totalVisits = ref(0)

const activeReminders = computed(() =>
  reminders.value.filter(r => !r.isCompleted).slice(0, 5)
)

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

    try {
      const visitsRes = await http.get('/clientportal/visits')
      totalVisits.value = (visitsRes.data || []).length
    } catch { totalVisits.value = 0 }

  } catch (err) {
    error.value = 'Bilgileriniz yüklenirken bir hata oluştu.'
  } finally {
    loading.value = false
  }
}

function formatMoney(val) {
  return `${Number(val || 0).toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺`
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
  if (s.includes('kedi') || s.includes('cat')) return 'linear-gradient(135deg, #f3e8ff, #e9d5ff)'
  if (s.includes('köpek') || s.includes('dog')) return 'linear-gradient(135deg, #fef3c7, #fde68a)'
  if (s.includes('kuş') || s.includes('bird')) return 'linear-gradient(135deg, #dbeafe, #bfdbfe)'
  if (s.includes('tavşan') || s.includes('rabbit')) return 'linear-gradient(135deg, #fce7f3, #fbcfe8)'
  return 'linear-gradient(135deg, #d1fae5, #a7f3d0)'
}

function formatDate(dateOnlyString) {
  if (!dateOnlyString) return ''
  try {
    const [year, month, day] = dateOnlyString.split('-')
    return `${day}.${month}.${year}`
  } catch { return dateOnlyString }
}

function getReminderClass(dateStr) {
  try {
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    const [y, m, d] = dateStr.split('-')
    const due = new Date(y, m - 1, d)
    const diffDays = Math.ceil((due - today) / (1000 * 60 * 60 * 24))
    if (diffDays < 0) return 'rem-expired'
    if (diffDays <= 3) return 'rem-urgent'
    return 'rem-normal'
  } catch { return 'rem-normal' }
}

function getReminderEmoji(dateStr) {
  try {
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    const [y, m, d] = dateStr.split('-')
    const due = new Date(y, m - 1, d)
    const diffDays = Math.ceil((due - today) / (1000 * 60 * 60 * 24))
    if (diffDays < 0) return '⏰'
    if (diffDays <= 3) return '🔴'
    return '💉'
  } catch { return '📅' }
}

onMounted(() => fetchData())
</script>

<style scoped>
.client-dashboard {
  animation: fadeUp 0.4s ease-out both;
}

@keyframes fadeUp {
  from { opacity: 0; transform: translateY(14px); }
  to { opacity: 1; transform: translateY(0); }
}

/* ── Loading ── */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
  gap: 1rem;
  color: #047857;
}
.paw-loader span {
  font-size: 2.5rem;
  display: block;
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
  justify-content: center; min-height: 60vh; gap: 1rem; text-align: center;
}
.error-icon-wrap { font-size: 3rem; }

/* ── Hero ── */
.hero-section {
  position: relative;
  background: linear-gradient(135deg, #064e3b 0%, #065f46 55%, #047857 100%);
  border-radius: 28px;
  padding: 2rem 1.75rem 2rem;
  margin-bottom: 1.25rem;
  overflow: hidden;
  display: flex;
  align-items: center;
  gap: 1.25rem;
}
.hero-bg-orb {
  position: absolute;
  border-radius: 50%;
  opacity: 0.12;
  background: white;
  pointer-events: none;
}
.orb-1 { width: 160px; height: 160px; top: -60px; right: -40px; }
.orb-2 { width: 90px; height: 90px; bottom: -30px; right: 60px; }

.hero-avatar {
  width: 60px;
  height: 60px;
  background: rgba(255, 255, 255, 0.18);
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  border: 1px solid rgba(255, 255, 255, 0.25);
}
.wave-emoji {
  font-size: 1.8rem;
  display: inline-block;
  animation: waveHand 2.5s infinite;
  transform-origin: 70% 70%;
}
@keyframes waveHand {
  0%, 100% { transform: rotate(0deg); }
  10% { transform: rotate(14deg); }
  20% { transform: rotate(-8deg); }
  30% { transform: rotate(14deg); }
  40% { transform: rotate(-4deg); }
  50% { transform: rotate(10deg); }
  60% { transform: rotate(0deg); }
}

.hero-text { flex: 1; }
.hero-greeting { font-size: 0.85rem; color: rgba(167, 243, 208, 0.85); margin: 0; font-weight: 500; }
.hero-name {
  font-family: 'Outfit', sans-serif;
  font-size: 1.85rem;
  font-weight: 900;
  color: white;
  margin: 0.1rem 0 0.3rem;
  letter-spacing: -0.04em;
  line-height: 1.1;
}
.hero-sub { font-size: 0.85rem; color: #a7f3d0; margin: 0; font-weight: 600; }

/* ── Stat Pills ── */
.stats-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0.75rem;
  margin-bottom: 1.25rem;
}
.stat-pill {
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(12px);
  border-radius: 20px;
  padding: 1.1rem 0.75rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.2rem;
  border: 1px solid rgba(255, 255, 255, 0.6);
  box-shadow: 0 4px 16px rgba(4, 120, 87, 0.07);
  transition: transform 0.2s;
}
.stat-pill:active { transform: scale(0.95); }
.stat-num {
  font-family: 'Outfit', sans-serif;
  font-size: 1.7rem;
  font-weight: 900;
  color: #064e3b;
  line-height: 1;
}
.stat-label { font-size: 0.7rem; font-weight: 700; color: #6b7280; text-transform: uppercase; letter-spacing: 0.04em; }

.stat-pets .stat-num { color: #7c3aed; }
.stat-visits .stat-num { color: #0369a1; }
.stat-reminders .stat-num { color: #b45309; }

/* ── Balance Card ── */
.balance-card {
  border-radius: 24px;
  margin-bottom: 1.25rem;
  background: linear-gradient(135deg, #059669 0%, #047857 100%);
  box-shadow: 0 12px 32px -8px rgba(5, 150, 105, 0.4);
  overflow: hidden;
  position: relative;
  transition: transform 0.2s;
}
.balance-card:active { transform: scale(0.98); }
.balance-card.debt-mode {
  background: linear-gradient(135deg, #d97706 0%, #b45309 100%);
  box-shadow: 0 12px 32px -8px rgba(217, 119, 6, 0.4);
}
.balance-card-inner {
  padding: 1.75rem 1.5rem;
  position: relative;
  z-index: 1;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.balance-label {
  font-size: 0.8rem;
  color: rgba(255,255,255,0.75);
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 0.4rem;
  margin-bottom: 0.5rem;
}
.balance-icon { font-size: 1rem; }
.balance-amount {
  font-family: 'Outfit', sans-serif;
  font-size: 2.1rem;
  font-weight: 900;
  color: white;
  letter-spacing: -0.04em;
  line-height: 1;
  margin-bottom: 0.4rem;
}
.balance-status { font-size: 0.78rem; color: rgba(255,255,255,0.8); margin: 0; font-weight: 600; }
.balance-deco { position: relative; width: 60px; flex-shrink: 0; }
.balance-ring {
  position: absolute;
  border-radius: 50%;
  border: 2px solid rgba(255,255,255,0.15);
}
.ring-1 { width: 70px; height: 70px; top: -20px; right: -10px; }
.ring-2 { width: 45px; height: 45px; top: 5px; right: 10px; }

/* ── Section ── */
.section-block { margin-bottom: 1.5rem; }
.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}
.section-title {
  font-family: 'Outfit', sans-serif;
  font-size: 1.15rem;
  font-weight: 800;
  color: #064e3b;
  margin: 0;
  letter-spacing: -0.02em;
}
.section-link {
  font-size: 0.85rem;
  font-weight: 700;
  color: #059669;
  text-decoration: none;
  background: rgba(5, 150, 105, 0.1);
  padding: 0.3rem 0.75rem;
  border-radius: 9999px;
  transition: background 0.2s;
}
.section-link:hover { background: rgba(5, 150, 105, 0.2); }

/* ── Pets Scroll Row ── */
.pets-scroll-row {
  display: flex;
  gap: 0.85rem;
  overflow-x: auto;
  padding-bottom: 0.5rem;
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none;
}
.pets-scroll-row::-webkit-scrollbar { display: none; }

.pet-bubble {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.4rem;
  flex-shrink: 0;
  cursor: pointer;
  transition: transform 0.2s;
}
.pet-bubble:active { transform: scale(0.93); }

.pet-bubble-avatar {
  width: 72px;
  height: 72px;
  border-radius: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 6px 18px rgba(0,0,0,0.08);
  border: 2px solid rgba(255,255,255,0.9);
}
.pet-bubble-emoji { font-size: 2.1rem; }
.pet-bubble-name { font-size: 0.8rem; font-weight: 700; color: #064e3b; }
.pet-bubble-breed { font-size: 0.68rem; color: #6b7280; font-weight: 500; }

.empty-mini {
  background: rgba(255,255,255,0.7);
  border-radius: 18px;
  padding: 1.25rem;
  font-size: 0.9rem;
  color: #6b7280;
  text-align: center;
  border: 1px dashed #d1d5db;
}

/* ── Reminders ── */
.reminder-count-badge {
  background: linear-gradient(135deg, #f59e0b, #d97706);
  color: white;
  font-size: 0.7rem;
  font-weight: 800;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.reminders-stack { display: flex; flex-direction: column; gap: 0.75rem; }
.empty-reminder-card {
  background: rgba(255,255,255,0.8);
  border-radius: 20px;
  padding: 2rem 1.5rem;
  text-align: center;
  border: 1px dashed #d1d5db;
}
.empty-reminder-icon { font-size: 2rem; display: block; margin-bottom: 0.5rem; }
.empty-reminder-card p { font-size: 0.88rem; color: #6b7280; margin: 0; }

.reminder-item {
  background: rgba(255,255,255,0.85);
  backdrop-filter: blur(12px);
  border-radius: 18px;
  padding: 1rem 1.1rem;
  display: flex;
  align-items: center;
  gap: 0.9rem;
  border: 1px solid rgba(255,255,255,0.6);
  box-shadow: 0 4px 14px rgba(4, 120, 87, 0.05);
  transition: transform 0.2s;
}
.reminder-item:active { transform: scale(0.97); }
.reminder-item.rem-urgent { border-left: 4px solid #f59e0b; }
.reminder-item.rem-expired { border-left: 4px solid #ef4444; opacity: 0.8; }
.reminder-item.rem-normal { border-left: 4px solid #10b981; }

.reminder-icon-wrap {
  width: 40px;
  height: 40px;
  border-radius: 13px;
  background: rgba(5, 150, 105, 0.08);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.25rem;
  flex-shrink: 0;
}
.rem-urgent .reminder-icon-wrap { background: rgba(245, 158, 11, 0.1); }
.rem-expired .reminder-icon-wrap { background: rgba(239, 68, 68, 0.1); }

.reminder-body { flex: 1; }
.reminder-body strong { font-size: 0.95rem; font-weight: 700; color: #111827; display: block; }
.reminder-body p { font-size: 0.8rem; color: #6b7280; margin: 0.2rem 0 0; }

.reminder-date-wrap { flex-shrink: 0; }
.reminder-date-text {
  font-size: 0.75rem;
  font-weight: 700;
  color: #059669;
  background: rgba(5, 150, 105, 0.1);
  padding: 0.25rem 0.6rem;
  border-radius: 8px;
}
.rem-urgent .reminder-date-text { color: #b45309; background: rgba(245, 158, 11, 0.1); }
.rem-expired .reminder-date-text { color: #b91c1c; background: rgba(239, 68, 68, 0.1); }

/* ── Quick Actions ── */
.quick-actions { display: flex; flex-direction: column; gap: 0.75rem; }
.quick-action-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  background: rgba(255,255,255,0.85);
  backdrop-filter: blur(12px);
  border-radius: 18px;
  padding: 1.1rem 1.25rem;
  text-decoration: none;
  border: 1px solid rgba(255,255,255,0.6);
  box-shadow: 0 4px 14px rgba(4, 120, 87, 0.05);
  transition: transform 0.2s, box-shadow 0.2s;
}
.quick-action-card:active { transform: scale(0.97); }
.qa-icon { font-size: 1.75rem; flex-shrink: 0; }
.qa-text { flex: 1; }
.qa-text strong { display: block; font-size: 0.95rem; font-weight: 700; color: #064e3b; }
.qa-text span { font-size: 0.78rem; color: #6b7280; font-weight: 500; }
.qa-arrow { font-size: 1.1rem; color: #059669; font-weight: 700; }

.qa-call { border-left: 4px solid #059669; }
.qa-map { border-left: 4px solid #0284c7; }
</style>
