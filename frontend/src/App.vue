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
        <div class="logo">
          <span class="logo-mark">
            <img src="./logo.png" alt="BullVet Logo" />
          </span>
          <div class="logo-text">
            <div class="title">e-Bull Vet</div>
            <div class="subtitle">Klinik Paneli</div>
          </div>
        </div>
        <nav class="nav">
          <RouterLink
            to="/"
            class="nav-item"
            :class="{ active: route.name === 'dashboard' }"
          >
            🏠 <span>Dashboard</span>
          </RouterLink>

          <RouterLink
            to="/owners"
            class="nav-item"
            :class="{ active: route.name === 'owners' }"
          >
            👤 <span>Hasta Sahipleri</span>
          </RouterLink>

          <RouterLink
            to="/visits"
            class="nav-item"
            :class="{ active: route.name === 'visits' }"
          >
            📋 <span>Ziyaretler</span>
          </RouterLink>

          <RouterLink 
            to="/pets" 
            class="nav-item" 
             :class="{ active: route.name === 'pets' }"
           >
            🐕🐈 <span>Hastalar</span>
          </RouterLink>
          
         <RouterLink
  v-if="canSeeBilanco"
  to="/bilanco"
  class="nav-item"
  :class="{ active: route.name === 'Bilanco' }"
>
  💰 <span>Bilanço</span>
</RouterLink>

        </nav>
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
          <!-- MOBİL: HAMBURGER BUTON -->
          <button
            v-if="isMobile"
            class="topbar-menu-btn"
            type="button"
            @click="toggleSidebar"
          >
            ☰
          </button>

          <div class="topbar-title">
            e-Bull Vet – Veteriner Aşı Takip Paneli
          </div>

          <!-- SAĞ ÜST ZİL -->
          <div class="notif-wrapper">
            <button
              class="notif-bell"
              type="button"
              @click="togglePanel"
            >
              🔔
              <span v-if="unreadCount > 0" class="badge">
                {{ unreadCount }}
              </span>
            </button>

            <div v-if="open" class="notif-panel">
              <div
                v-if="notifications.length === 0"
                class="notif-empty"
              >
                Bildirim yok.
              </div>

              <div
                v-for="n in notifications"
                :key="n.id"
                class="notif-item"
              >
                <div class="notif-message">
                  {{ n.message }}
                </div>
                <div class="notif-time">
                  {{ new Date(n.createdAt).toLocaleString('tr-TR') }}
                </div>
              </div>
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

<style scoped>
.app-root {
  min-height: 100vh;
}

/* GENEL LAYOUT */
.layout {
  display: flex;
  min-height: 100vh;
}

/* SIDEBAR */
.sidebar {
  width: 260px;
  background-color: #020617;
  background-image: radial-gradient(
      1.5px 1.5px at 10px 10px,
      #ffffff 0,
      transparent 55%
    ),
    radial-gradient(
      1.5px 1.5px at 30px 40px,
      #ffffff 0,
      transparent 55%
    ),
    radial-gradient(
      1.5px 1.5px at 50px 20px,
      #ffffff 0,
      transparent 55%
    ),
    radial-gradient(
      1.5px 1.5px at 70px 50px,
      #ffffff 0,
      transparent 55%
    );
  background-size: 80px 80px;
  color: #e5e7eb;
  padding: 1.25rem 1rem;
  display: flex;
  flex-direction: column;
}

.logo {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
}

.logo-mark img {
  width: 36px;
  height: 36px;
  border-radius: 999px;
}

.logo-text .title {
  font-weight: 700;
  font-size: 1.1rem;
}

.logo-text .subtitle {
  font-size: 0.75rem;
  color: #9ca3af;
}

.nav {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.55rem 0.75rem;
  border-radius: 0.6rem;
  font-size: 0.9rem;
  color: #e5e7eb;
  text-decoration: none;
  opacity: 0.85;
}

.nav-item span {
  flex: 1;
}

.nav-item:hover {
  background: rgba(15, 23, 42, 0.9);
  opacity: 1;
}

.nav-item.active {
  background: #f97316;
  color: #111827;
}

.main {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: #f3f4f6;

  min-width: 0;         /* içerik ne kadar geniş olursa olsun main büyümesin */
  overflow-x: hidden;   /* emniyet kilidi: main sağa taşmasın */
}

/* içerik */
.content {
  padding: 1.25rem;
  max-width: 1200px;
  width: 100%;
  margin: 0 auto 1.25rem;
}

@media (max-width: 768px) {
  .main {
    padding: 0;               /* dış padding yok */
  }

  .topbar {
    padding: 0.75rem 1rem;    /* hamburger + başlık için */
  }

  .content {
    padding: 0 1rem 1.5rem;   /* Dashboard .page ile aynı hizaya gelsin */
    margin-top: 0.5rem;
  }
}


.topbar {
  height: 56px;
  padding: 0 1.25rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #ffffff;
  border-bottom: 1px solid #e5e7eb;
}

.topbar-title {
  font-size: 0.95rem;
  font-weight: 500;
  color: #111827;
}

/* hamburger */
.topbar-menu-btn {
  border: none;
  background: transparent;
  font-size: 1.5rem;
  margin-right: 0.75rem;
  cursor: pointer;
}


/* BİLDİRİM ZİLİ */
.notif-wrapper {
  position: relative;
}

.notif-bell {
  border: none;
  background: #f9fafb;
  border-radius: 999px;
  padding: 0.4rem 0.75rem;
  cursor: pointer;
  font-size: 0.95rem;
  position: relative;
}

.badge {
  position: absolute;
  top: -4px;
  right: -4px;
  background: #ef4444;
  color: #fff;
  border-radius: 999px;
  font-size: 0.65rem;
  padding: 0 0.3rem;
  min-width: 16px;
  text-align: center;
}

.notif-panel {
  position: absolute;
  right: 0;
  margin-top: 0.4rem;
  width: 260px;
  max-height: 320px;
  overflow-y: auto;
  background: #ffffff;
  border-radius: 0.75rem;
  box-shadow: 0 18px 45px rgba(15, 23, 42, 0.25);
  padding: 0.5rem 0.75rem;
  z-index: 30;
}

.notif-empty {
  font-size: 0.8rem;
  color: #6b7280;
  padding: 0.25rem 0;
}

.notif-item {
  padding: 0.35rem 0.25rem;
  border-bottom: 1px solid #f3f4f6;
  font-size: 0.8rem;
}

.notif-item:last-child {
  border-bottom: none;
}

.notif-message {
  margin-bottom: 0.15rem;
}

.notif-time {
  font-size: 0.7rem;
  color: #9ca3af;
}

/* MOBİL DAVRANIŞ */
.sidebar-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.55);
  z-index: 35;
}

@media (max-width: 768px) {
  .layout {
    min-height: 100vh;
  }

  .sidebar {
    position: fixed;
    inset: 0 auto 0 0;
    width: 260px;
    transform: translateX(-100%);
    transition: transform 0.25s ease-out;
    z-index: 40;
    background-color: #020617;
    background-image: radial-gradient(
      1.5px 1.5px at 10px 10px,
      #ffffff 0,
      transparent 55%
    ),
    radial-gradient(
      1.5px 1.5px at 30px 40px,
      #ffffff 0,
      transparent 55%
    ),
    radial-gradient(
      1.5px 1.5px at 50px 20px,
      #ffffff 0,
      transparent 55%
    ),
    radial-gradient(
      1.5px 1.5px at 70px 50px,
      #ffffff 0,
      transparent 55%
    );
  color: #e5e7eb;
  }

  .sidebar--mobile-open {
    transform: translateX(0);
  }

  .content {
    padding: 0.85rem;
  }

  .topbar {
    padding: 0 0.85rem;
  }

  .topbar-title {
    font-size: 0.9rem;
  }
}

@media (max-width: 480px) {
  .content {
    padding: 0.75rem 0.65rem 1rem;
  }

  .topbar-title {
    font-size: 0.85rem;
  }

  .notif-bell {
    padding: 0.3rem 0.6rem;
  }
}
</style>
