<template>
  <div class="layout">
    <aside class="sidebar">
      <div class="logo">
  <span class="logo-mark">
    <!-- Buraya logonu ekle -->
    <img src="./logo.png" alt="BullVet Logo" />


  </span>
  <div class="logo-text">
    <div class="title">BullVet</div>
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
          to="/pets"
          class="nav-item"
          :class="{ active: $route.name === 'pets' }"
        >
          🐾 <span>Hastalar</span>
        </RouterLink>

        <RouterLink
          to="/visits"
          class="nav-item"
          :class="{ active: $route.name === 'visits' }"
        >
          📋 <span>Ziyaretler</span>
        </RouterLink>
      </nav>
    </aside>

    <main class="main">
      <header class="topbar">
        <div class="topbar-title">
          BullVet – Veteriner Aşı Takip Paneli
        </div>
      </header>

      <section class="content">
        <RouterView />
      </section>
    </main>
  </div>
</template>

<script setup>
import { RouterLink, RouterView } from 'vue-router'
</script>
