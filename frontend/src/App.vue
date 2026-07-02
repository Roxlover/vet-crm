<template>
  <div class="app-root">
    <!-- LOGIN / AUTH SAYFALARI: tam ekran, sidebar yok -->
    <RouterView v-if="isAuthRoute" />

    <!-- MÜŞTERİ PORTALI TEMA & YERLEŞİM (Emerald Green / Glassmorphism) -->
    <div v-else-if="isClientRoute" class="client-layout">
      <!-- ANA İÇERİK -->
      <main class="client-main">
        <section class="client-content">
          <RouterView />
        </section>
      </main>

      <!-- MÜŞTERİ ALT NAVİGASYON (Premium Zümrüt Yeşili) -->
      <nav class="client-bottom-nav">
        <RouterLink to="/client/dashboard" class="client-nav-item" :class="{ active: route.name === 'client-dashboard' }">
          <span class="client-nav-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"></path>
              <polyline points="9 22 9 12 15 12 15 22"></polyline>
            </svg>
          </span>
          <span class="client-nav-label">Özet</span>
        </RouterLink>
        <RouterLink to="/client/pets" class="client-nav-item" :class="{ active: route.name === 'client-pets' }">
          <span class="client-nav-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path>
            </svg>
          </span>
          <span class="client-nav-label">Dostlarım</span>
        </RouterLink>
        <RouterLink to="/client/visits" class="client-nav-item" :class="{ active: route.name === 'client-visits' }">
          <span class="client-nav-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
              <line x1="16" y1="2" x2="16" y2="6"></line>
              <line x1="8" y1="2" x2="8" y2="6"></line>
              <line x1="3" y1="10" x2="21" y2="10"></line>
            </svg>
          </span>
          <span class="client-nav-label">Geçmiş</span>
        </RouterLink>
        <button class="client-nav-item logout-btn" @click="handleClientLogout">
          <span class="client-nav-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"></path>
              <polyline points="16 17 21 12 16 7"></polyline>
              <line x1="21" y1="12" x2="9" y2="12"></line>
            </svg>
          </span>
          <span class="client-nav-label">Çıkış</span>
        </button>
      </nav>
    </div>

    <!-- DİĞER SAYFALAR: sidebar + içerik (Veteriner CRM) -->
    <div v-else class="layout">
      <!-- SOL MENÜ -->
      <aside
        class="sidebar"
        :class="{ 'sidebar--mobile-open': sidebarOpen }"
      >
        <div class="sidebar-header">
          <div class="logo">
            <span class="logo-mark">
              <img src="./logo.png" alt="BullVet Logo" />
            </span>
            <div class="logo-text">
              <div class="title">BullVet</div>
              <div class="subtitle">Klinik Yönetimi</div>
            </div>
          </div>
        </div>

        <nav class="nav-links" @click="sidebarOpen = false">
          <RouterLink to="/dashboard" class="nav-item" :class="{ active: route.name === 'dashboard' }">
            <span>Dashboard</span>
          </RouterLink>

          <RouterLink to="/owners" class="nav-item" :class="{ active: route.name === 'owners' }">
            <span>Sahipler</span>
          </RouterLink>

          <RouterLink to="/visits" class="nav-item" :class="{ active: route.name === 'visits' }">
            <span>Ziyaretler</span>
          </RouterLink>

          <RouterLink to="/pets" class="nav-item" :class="{ active: route.name === 'pets' }">
            <span>Hastalar</span>
          </RouterLink>
          
          <RouterLink v-if="canSeeBilanco" to="/bilanco" class="nav-item" :class="{ active: route.name === 'Bilanco' }">
            <span>Bilanço</span>
          </RouterLink>
        </nav>

        <div class="sidebar-footer" v-if="rawUser">
          <div class="user-brief">
            <div class="avatar">{{ (rawUser.username || 'V').charAt(0).toUpperCase() }}</div>
            <div class="user-meta">
              <span class="name">{{ rawUser.username }}</span>
              <span class="role">{{ canSeeBilanco ? 'Yönetici' : 'Personel' }}</span>
            </div>
          </div>
        </div>
      </aside>

      <!-- MOBİLDE KARARAN ARKA PLAN -->
      <div
        v-if="sidebarOpen && isMobile"
        class="sidebar-backdrop"
        @click="sidebarOpen = false"
      ></div>

      <!-- ANA ALAN -->
      <main class="main">
        <header class="topbar">
          <div class="topbar-left">
            <button v-if="isMobile" class="topbar-menu-btn" @click="toggleSidebar">
              <div class="hamburger" :class="{ 'is-active': sidebarOpen }">
                <span></span>
                <span></span>
                <span></span>
              </div>
            </button>
            <div class="search-pill">
              <input type="text" placeholder="Hızlı hasta veya sahip ara..." />
            </div>
          </div>

          <div class="topbar-right">
            <div class="notif-wrapper">
              <button class="notif-btn" @click="togglePanel">
                <span class="btn-icon">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path>
                    <path d="M13.73 21a2 2 0 0 1-3.46 0"></path>
                  </svg>
                </span>
                <span v-if="unreadCount > 0" class="badge">{{ unreadCount }}</span>
              </button>
              
              <div v-if="open" class="notif-panel">
                <div class="panel-header">Bildirimler</div>
                <div v-if="notifications.length === 0" class="notif-empty">Henüz bir bildirim yok.</div>
                <div v-for="n in notifications" :key="n.id" class="notif-item">
                  <div class="notif-message">{{ n.message }}</div>
                  <div class="notif-time">{{ new Date(n.createdAt).toLocaleDateString('tr-TR') }}</div>
                </div>
              </div>
            </div>

            <div class="top-user-pill" v-if="rawUser">
              <div class="avatar-mini">{{ (rawUser.username || 'V').charAt(0).toUpperCase() }}</div>
              <span class="user-name-mini">{{ rawUser.username }}</span>
            </div>
          </div>
        </header>

        <section class="content">
          <RouterView />
        </section>
      </main>

      <!-- MOBİL ALT NAVİGASYON (Sadece Mobilde Görünür) -->
      <nav v-if="isMobile && !isAuthRoute" class="bottom-nav">
        <RouterLink to="/dashboard" class="bottom-nav-item" :class="{ active: route.name === 'dashboard' }">
          <span class="nav-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
              <line x1="3" y1="9" x2="21" y2="9"></line>
              <line x1="9" y1="21" x2="9" y2="9"></line>
            </svg>
          </span>
          <span class="nav-label">Özet</span>
        </RouterLink>
        <RouterLink to="/owners" class="bottom-nav-item" :class="{ active: route.name === 'owners' }">
          <span class="nav-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
              <circle cx="9" cy="7" r="4"></circle>
              <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
              <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
            </svg>
          </span>
          <span class="nav-label">Sahipler</span>
        </RouterLink>
        <RouterLink to="/visits" class="bottom-nav-item" :class="{ active: route.name === 'visits' }">
          <span class="nav-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M21 11.5a8.38 8.38 0 0 1-.9 3.8 8.5 8.5 0 1 1-7.6-10.6 8.38 8.38 0 0 1 3.9.9"></path>
              <polyline points="16 2 16 6 20 6"></polyline>
            </svg>
          </span>
          <span class="nav-label">Ziyaretler</span>
        </RouterLink>
        <RouterLink to="/pets" class="bottom-nav-item" :class="{ active: route.name === 'pets' }">
          <span class="nav-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M10 5.172a4 4 0 0 1 5.656 5.656L10 16.485l-5.656-5.656a4 4 0 0 1 5.656-5.656z"></path>
            </svg>
          </span>
          <span class="nav-label">Hastalar</span>
        </RouterLink>
        <RouterLink v-if="canSeeBilanco" to="/bilanco" class="bottom-nav-item" :class="{ active: route.name === 'Bilanco' }">
          <span class="nav-icon">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="12" y1="1" x2="12" y2="23"></line>
              <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"></path>
            </svg>
          </span>
          <span class="nav-label">Kasa</span>
        </RouterLink>
      </nav>
    </div>
  </div>
</template>

<script setup>
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import {
  onMounted,
  onBeforeUnmount,
  ref,
  computed,
  watch,
} from 'vue'
import {
  fetchNotifications,
  markNotificationsRead,
} from '@/api/notifications'
import '@/style.css'
import { getUser } from '@/utils/auth'
import { Capacitor } from '@capacitor/core'
import { PushNotifications } from '@capacitor/push-notifications'


const rawUser = computed(() => getUser() || null)

const canSeeBilanco = computed(() => {
  const u = rawUser.value || {}
  const role = String(u.role || '').trim().toLowerCase()
  const username = String(u.username || '').trim().toLowerCase()
  return role === 'admin' || username === 'bullboss' 
})



const route = useRoute()

// bildirimler
const notifications = ref([])
const unreadCount = ref(0)
const open = ref(false)
let notifIntervalId = null

async function loadNotifications() {
  const token = localStorage.getItem('vetcrm_token')
  if (!token) return

  try {
    const data = await fetchNotifications()
    notifications.value = data || []
    unreadCount.value = notifications.value.filter(
      (n) => !n.isRead,
    ).length
  } catch (e) {
    if (e.response && e.response.status === 401) return
    console.error('notif error', e)
  }
}

async function togglePanel() {
  open.value = !open.value

  if (open.value && unreadCount.value > 0) {
    try {
      await markNotificationsRead()
      notifications.value = notifications.value.map((n) => ({
        ...n,
        isRead: true,
      }))
      unreadCount.value = 0
    } catch (e) {
      console.error('markNotificationsRead error', e)
    }
  }
}

// layout / responsive
const sidebarOpen = ref(false)
const isMobile = ref(false)

function handleResize() {
  isMobile.value = window.innerWidth < 768
  // desktop'a geçince menüyü kapat
  if (!isMobile.value) {
    sidebarOpen.value = false
  }
}

function toggleSidebar() {
  sidebarOpen.value = !sidebarOpen.value
}

const router = useRouter()

const isAuthRoute = computed(() =>
  ['login', 'client-login', 'welcome'].includes(route.name),
)

const isClientRoute = computed(() =>
  route.path.startsWith('/client'),
)

function handleClientLogout() {
  localStorage.removeItem('vetcrm_token')
  localStorage.removeItem('vetcrm_user')
  router.push('/client/login')
}

// Sayfa değiştiğinde (link tıklandığında) mobilde menüyü kapat
watch(
  () => route.path,
  () => {
    if (isMobile.value) {
      sidebarOpen.value = false
    }
  }
)

async function initPushNotifications() {
  if (!Capacitor.isNativePlatform()) return

  try {
    let permStatus = await PushNotifications.checkPermissions()
    if (permStatus.receive === 'prompt') {
      permStatus = await PushNotifications.requestPermissions()
    }

    if (permStatus.receive !== 'granted') {
      console.warn('Push notification permission denied')
      return
    }

    await PushNotifications.register()

    PushNotifications.addListener('registration', (token) => {
      console.log('Push registration success, token: ' + token.value)
      localStorage.setItem('fcm_token', token.value)
    })

    PushNotifications.addListener('registrationError', (error) => {
      console.error('Push registration error: ', JSON.stringify(error))
    })

    PushNotifications.addListener('pushNotificationReceived', (notification) => {
      console.log('Push received: ', JSON.stringify(notification))
      loadNotifications()
    })

    PushNotifications.addListener('pushNotificationActionPerformed', (notification) => {
      console.log('Push action performed: ', JSON.stringify(notification))
    })
  } catch (error) {
    console.error('Push init error', error)
  }
}

onMounted(() => {
  handleResize()
  window.addEventListener('resize', handleResize)

  loadNotifications()
  notifIntervalId = setInterval(loadNotifications, 60000)

  initPushNotifications()
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  if (notifIntervalId) clearInterval(notifIntervalId)
})
</script>

<style>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&family=Outfit:wght@500;600;700&display=swap');

.app-root {
  min-height: 100vh;
  background-color: var(--bg-main);
}

.layout {
  display: flex;
  min-height: 100vh;
}

/* SIDEBAR - MODERN WHITE */
.sidebar {
  width: 300px;
  background-color: #ffffff;
  border-right: 1px solid #f1f5f9;
  display: flex;
  flex-direction: column;
  padding: 2.5rem 1.5rem;
  transition: var(--transition);
  z-index: 100;
}

.sidebar-header {
  margin-bottom: 3rem;
}

.logo {
  display: flex;
  align-items: center;
  gap: 1.25rem;
}

.logo-mark {
  width: 50px;
  height: 50px;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  border-radius: 14px;
  background: #ffffff;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.08);
}

.logo-mark img {
  width: 100%;
  height: 100%;
  object-fit: contain;
}

.logo-text .title {
  font-size: 1.6rem;
  font-weight: 900;
  color: var(--text-main);
  letter-spacing: -0.04em;
  line-height: 1;
}

.logo-text .subtitle {
  font-size: 0.8rem;
  color: var(--text-muted);
  font-weight: 700;
  margin-top: 4px;
}

.nav-links {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  flex: 1;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 1.1rem;
  padding: 1rem 1.25rem;
  border-radius: 14px;
  color: var(--text-muted);
  font-weight: 700;
  font-size: 0.95rem;
  text-decoration: none;
  transition: var(--transition);
}

.nav-item:hover {
  background-color: var(--primary-light);
  color: var(--primary);
  transform: translateX(6px);
}

.nav-item.active {
  background-color: var(--primary);
  color: #ffffff;
  box-shadow: 0 10px 20px -5px rgba(79, 70, 229, 0.3);
}

.nav-icon {
  font-size: 1.25rem;
}

.sidebar-footer {
  margin-top: 2rem;
  padding-top: 1.5rem;
  border-top: 1px solid #f1f5f9;
}

.user-brief {
  display: flex;
  align-items: center;
  gap: 0.85rem;
}

.user-meta {
  display: flex;
  flex-direction: column;
}

.user-meta .name {
  font-weight: 800;
  color: var(--text-main);
  font-size: 0.9rem;
}

.user-meta .role {
  font-size: 0.7rem;
  color: var(--primary);
  font-weight: 700;
}

/* TOPBAR REFINEMENT */
.topbar {
  height: calc(85px + env(safe-area-inset-top));
  padding-top: env(safe-area-inset-top);
  background-color: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(20px);
  padding-left: 2rem;
  padding-right: 2rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #f1f5f9;
  position: sticky;
  top: 0;
  z-index: 90;
}

.search-pill {
  display: flex;
  align-items: center;
  background: #f8fafc;
  border: 1px solid #f1f5f9;
  border-radius: 14px;
  padding: 0.7rem 1.25rem;
  width: 100%;
  max-width: 400px;
  transition: var(--transition);
}

.search-pill:focus-within {
  background: #ffffff;
  border-color: var(--primary-light);
  box-shadow: var(--shadow-sm);
}

.search-pill input {
  border: none;
  background: transparent;
  margin-left: 0.85rem;
  font-size: 0.95rem;
  width: 100%;
  outline: none;
  font-weight: 600;
}

.topbar-right {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.notif-btn {
  background: #ffffff;
  border: 1px solid #f1f5f9;
  width: 46px;
  height: 46px;
  border-radius: 12px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  transition: var(--transition);
}

.notif-btn:hover {
  background: #f8fafc;
  transform: translateY(-1px);
}

.top-user-pill {
  display: flex;
  align-items: center;
  gap: 0.6rem;
}

.avatar-mini {
  width: 32px;
  height: 32px;
  background: var(--primary-light);
  color: var(--primary);
  border-radius: 10px;
  display: flex;
  justify-content: center;
  font-weight: 800;
  font-size: 0.8rem;
}

.user-name-mini {
  font-weight: 700;
  font-size: 0.9rem;
  color: var(--text-main);
}

.notif-panel {
  position: absolute;
  right: 0;
  top: 100%;
  margin-top: 1.25rem;
  width: 360px;
  background: #ffffff;
  border-radius: 24px;
  box-shadow: var(--shadow-lg);
  border: 1px solid #f1f5f9;
  padding: 1.5rem;
  z-index: 1000;
  animation: fadeIn 0.3s ease-out;
}

.panel-header {
  font-weight: 800;
  font-size: 1.1rem;
  margin-bottom: 1rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #f1f5f9;
}

.main {
  flex: 1;
  min-width: 0;
  width: 100%;
  overflow-x: hidden;
}

.content {
  padding: 2rem;
  width: 100%;
  max-width: 100vw;
  overflow-x: hidden;
}

/* RESPONSIVE */
@media (max-width: 768px) {
  .topbar { 
    padding-left: 1rem; 
    padding-right: 1rem; 
    height: calc(70px + env(safe-area-inset-top)); 
  }
  .search-pill { display: none; }
  .user-name-mini { display: none; }
  .content { padding: 1rem; }
  
  .layout {
    display: block;
  }

  .sidebar {
    width: 280px;
    position: fixed;
    left: -280px;
    top: 0;
    bottom: 0;
    box-shadow: 20px 0 50px rgba(0,0,0,0.1);
  }
  
  .sidebar--mobile-open {
    left: 0;
  }

  .topbar-menu-btn {
  background: none;
  border: none;
  padding: 8px;
  color: #64748b;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 8px;
  transition: all 0.2s;
}

.topbar-menu-btn:hover {
  background: #f1f5f9;
  color: #0f172a;
}

  .topbar-left {
    display: flex;
    align-items: center;
    gap: 0.75rem;
  }

  h1 { font-size: 1.75rem !important; }
  h2 { font-size: 1.5rem !important; }
  h3 { font-size: 1.25rem !important; }

  /* BOTTOM NAV STYLES */
  .bottom-nav {
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    min-height: 70px;
    background: #ffffff;
    display: flex;
    justify-content: space-around;
    align-items: center;
    border-top: 1px solid #f1f5f9;
    box-shadow: 0 -5px 20px rgba(0,0,0,0.05);
    z-index: 1000;
    padding-bottom: env(safe-area-inset-bottom);
    padding-top: 0.5rem;
  }

  .bottom-nav-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 4px;
    color: var(--text-muted);
    text-decoration: none;
    flex: 1;
    transition: var(--transition);
  }

  .bottom-nav-item .nav-icon {
    font-size: 1.4rem;
  }

  .bottom-nav-item .nav-label {
    font-size: 0.7rem;
    font-weight: 700;
  }

  .bottom-nav-item.active {
    color: var(--primary);
  }

  .main {
    padding-bottom: calc(85px + env(safe-area-inset-bottom)); /* Alt menü için boşluk */
  }

  /* HAMBURGER ANIMATION */
  .hamburger {
    width: 24px;
    height: 18px;
    position: relative;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
  }

  .hamburger span {
    display: block;
    width: 100%;
    height: 2px;
    background: currentColor;
    border-radius: 2px;
    transition: all 0.3s cubic-bezier(0.645, 0.045, 0.355, 1);
  }

  .hamburger.is-active span:nth-child(1) {
    transform: translateY(8px) rotate(45deg);
  }
  .hamburger.is-active span:nth-child(2) {
    opacity: 0;
  }
  .hamburger.is-active span:nth-child(3) {
    transform: translateY(-8px) rotate(-45deg);
  }
}

/* --- CLIENT PORTAL STYLES ( Emerald Green & Glassmorphism ) --- */
.client-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background:
    radial-gradient(ellipse at 10% 0%, rgba(167, 243, 208, 0.45) 0%, transparent 55%),
    radial-gradient(ellipse at 90% 100%, rgba(110, 231, 183, 0.3) 0%, transparent 55%),
    linear-gradient(160deg, #ecfdf5 0%, #f0fdf4 40%, #f9fafb 100%);
  color: #166534;
  font-family: 'Inter', sans-serif;
  padding-bottom: calc(90px + env(safe-area-inset-bottom));
  overflow-x: hidden;
}

.client-main {
  flex: 1;
  width: 100%;
  max-width: 600px;
  margin: 0 auto;
  padding: 1.5rem 1rem;
  box-sizing: border-box;
}

.client-content {
  width: 100%;
}

/* Premium Client Bottom Nav */
.client-bottom-nav {
  position: fixed;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 100%;
  max-width: 600px;
  min-height: 68px;
  background: rgba(255, 255, 255, 0.92);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border-top: 1px solid rgba(22, 101, 52, 0.08);
  display: flex;
  justify-content: space-around;
  align-items: center;
  box-shadow: 0 -10px 40px rgba(6, 78, 59, 0.1);
  z-index: 1000;
  padding-bottom: env(safe-area-inset-bottom);
  padding-top: 0.5rem;
}

.client-nav-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3px;
  color: #9ca3af;
  text-decoration: none;
  background: none;
  border: none;
  cursor: pointer;
  padding: 8px 14px;
  border-radius: 16px;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  flex: 1;
  position: relative;
}

.client-nav-item.logout-btn {
  font-family: inherit;
}

.client-nav-item .client-nav-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 26px;
  height: 26px;
  color: #9ca3af;
  transition: all 0.25s ease;
}

.client-nav-item .client-nav-label {
  font-size: 0.68rem;
  font-weight: 600;
  transition: all 0.25s ease;
  letter-spacing: 0.01em;
}

.client-nav-item:active {
  transform: scale(0.93);
}

.client-nav-item.active {
  color: #059669;
}

.client-nav-item.active .client-nav-icon {
  color: #059669;
  filter: drop-shadow(0 2px 4px rgba(5, 150, 105, 0.3));
}

.client-nav-item.active::before {
  content: '';
  position: absolute;
  top: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 28px;
  height: 3px;
  background: linear-gradient(90deg, #059669, #047857);
  border-radius: 0 0 4px 4px;
}

.client-nav-item.active .client-nav-label {
  color: #047857;
  font-weight: 800;
}

/* Global Custom Styles for Client Dashboard & Views */
.client-card {
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.5);
  border-radius: 24px;
  padding: 1.5rem;
  box-shadow: 0 10px 30px -10px rgba(4, 120, 87, 0.1);
  margin-bottom: 1.25rem;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.client-card:active {
  transform: scale(0.98);
}

.client-btn {
  background: linear-gradient(135deg, #059669 0%, #047857 100%);
  color: white;
  border: none;
  padding: 1rem 1.5rem;
  border-radius: 16px;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  width: 100%;
  box-shadow: 0 8px 20px -6px rgba(4, 120, 87, 0.4);
  transition: all 0.3s ease;
}

.client-btn:active {
  transform: scale(0.97);
  box-shadow: 0 4px 10px -3px rgba(4, 120, 87, 0.3);
}

.client-badge-success {
  background-color: #d1fae5;
  color: #065f46;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 700;
}

.client-badge-warning {
  background-color: #fef3c7;
  color: #92400e;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 700;
}

.client-badge-danger {
  background-color: #fee2e2;
  color: #991b1b;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 700;
}
</style>
