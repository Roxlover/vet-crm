<template>
  <div class="layout">
    <aside class="sidebar">
      <div class="logo">
    <span class="logo-mark">
      <!-- Buraya logonu ekle -->
      <img src="./logo.png" alt="BullVet Logo" />


    </span>
    <div class="logo-text">
      <div class="title">NL</div>
      <div class="subtitle">Klinik Paneli</div>
    </div>
  </div>


      <nav class="nav">
        <RouterLink
          to="/"
          class="nav-item"
          :class="{ active: $route.name === 'dashboard' }"
        >
          🏠 <span>Dashboard</span>
        </RouterLink>

        <RouterLink
          to="/owners"
          class="nav-item"
          :class="{ active: $route.name === 'owners' }"
        >
          👤 <span>Hasta Sahipleri</span>
        </RouterLink>


        <RouterLink
          to="/visits"
          class="nav-item"
          :class="{ active: $route.name === 'visits' }"
        >
          📋 <span>Ziyaretler</span>
        </RouterLink>
        <RouterLink
          to="/bilanco"
          class="nav-item"
          :class="{ active: $route.name === 'Bilanco' }"
        >
          💰 <span>Bilanço</span>
        </RouterLink>
      </nav>
    </aside>

    <main class="main">
      <header class="topbar">
        <div class="topbar-title">
          NL – Veteriner Aşı Takip Paneli
        </div>
      </header>

      <section class="content">
        <RouterView />
      </section>
    </main>
  
  </div>
   <div class="notif-wrapper">
    <button class="notif-bell" @click="openPanel">
      🔔
      <span v-if="unreadCount > 0" class="badge">{{ unreadCount }}</span>
    </button>

    <div v-if="open" class="notif-panel">
      <div v-if="notifications.length === 0" class="notif-empty">
        Bildirim yok.
      </div>
      <div
        v-for="n in notifications"
        :key="n.id"
        class="notif-item"
      >
        <div class="notif-message">{{ n.message }}</div>
        <div class="notif-time">
          {{ new Date(n.createdAt).toLocaleString('tr-TR') }}
        </div>
      </div>
    </div>
  </div>
</template>


<script setup>
import { RouterLink, RouterView } from 'vue-router'
import { onMounted, ref } from 'vue'
import { fetchNotifications, markNotificationsRead } from '@/api/notifications'

const notifications = ref([])
const unreadCount = ref(0)
const open = ref(false)

async function loadNotifications() {
  const token = localStorage.getItem('vetcrm_token')
  if (!token) return // login yoksa hiç deneme

  try {
    const data = await fetchNotifications()
    notifications.value = data
    unreadCount.value = data.filter(n => !n.isRead).length
  } catch (e) {
    if (e.response && e.response.status === 401) {
      return // token bozuksa da sessiz geç
    }
    console.error('notif error', e)
  }
}



async function openPanel() {
  open.value = !open.value
  if (open.value && unreadCount.value > 0) {
    await markNotificationsRead()
    unreadCount.value = 0
  }
}

onMounted(() => {
  loadNotifications()
  // istersen her 60 sn'de bir yenile:
  setInterval(loadNotifications, 60000)
})
</script>
