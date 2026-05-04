<template>
  <div class="app-root">
    <!-- LOGIN / AUTH SAYFALARI: tam ekran, sidebar yok -->
    <RouterView v-if="isAuthRoute" />

    <!-- DİĞER SAYFALAR: sidebar + içerik -->
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

        <nav class="nav-links">
          <RouterLink to="/" class="nav-item" :class="{ active: route.name === 'dashboard' }">
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
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <line x1="3" y1="12" x2="21" y2="12"></line>
                <line x1="3" y1="6" x2="21" y2="6"></line>
                <line x1="3" y1="18" x2="21" y2="18"></line>
              </svg>
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
    </div>
  </div>
</template>

<script setup>
import { RouterLink, RouterView, useRoute } from 'vue-router'
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

const isAuthRoute = computed(() =>
  ['login'].includes(route.name),
)

// Sayfa değiştiğinde (link tıklandığında) mobilde menüyü kapat
watch(
  () => route.path,
  () => {
    if (isMobile.value) {
      sidebarOpen.value = false
    }
  }
)

onMounted(() => {
  handleResize()
  window.addEventListener('resize', handleResize)

  loadNotifications()
  notifIntervalId = setInterval(loadNotifications, 60000)
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
  height: 85px;
  background-color: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(20px);
  padding: 0 2rem;
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
}

.content {
  padding: 2rem;
  width: 100%;
  max-width: 100%;
}

/* RESPONSIVE */
@media (max-width: 768px) {
  .topbar { padding: 0 1rem; height: 70px; }
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
}
</style>
