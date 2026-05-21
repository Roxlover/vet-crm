<template>
  <div class="welcome-container">
    <!-- Arka plan dekoratif elementler -->
    <div class="bg-orb bg-orb-1"></div>
    <div class="bg-orb bg-orb-2"></div>
    <div class="bg-orb bg-orb-3"></div>

    <div class="welcome-card">
      <!-- Logo & Marka -->
      <div class="brand">
        <div class="logo-ring">
          <img src="../logo.png" alt="BullVet Logo" class="brand-logo" />
        </div>
        <h1 class="brand-name">BullVet</h1>
        <p class="brand-tagline">Veteriner Klinik Yönetim Sistemi</p>
      </div>

      <!-- Hoşgeldiniz Metni -->
      <div class="welcome-text">
        <h2>Hoş Geldiniz </h2>
        <p>Lütfen giriş türünüzü seçin</p>
      </div>

      <!-- Giriş Seçenekleri -->
      <div class="login-options">
        <!-- Veteriner Girişi -->
        <RouterLink to="/login" class="option-card vet-card">
          <div class="option-icon-wrap">
            <span class="option-icon"></span>
          </div>
          <div class="option-text">
            <strong>Veteriner / Personel</strong>
            <span>Klinik yönetim paneline giriş</span>
          </div>
          <span class="option-arrow">›</span>
        </RouterLink>

        <!-- Müşteri Girişi -->
        <RouterLink to="/client/login" class="option-card client-card">
          <div class="option-icon-wrap client-icon-wrap">
            <span class="option-icon"></span>
          </div>
          <div class="option-text">
            <strong>Hayvan Sahibi</strong>
            <span>Dostlarımın sağlık bilgilerini görüntüle</span>
          </div>
          <span class="option-arrow">›</span>
        </RouterLink>
      </div>

      <!-- Alt Bilgi -->
      <footer class="welcome-footer">
        <p>BullVet &copy; {{ new Date().getFullYear() }} · Tüm hakları saklıdır</p>
      </footer>
    </div>
  </div>
</template>

<script setup>
import { RouterLink } from 'vue-router'
</script>

<style scoped>
.welcome-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #0f172a 0%, #1e293b 50%, #0f172a 100%);
  padding: 1.5rem;
  position: relative;
  overflow: hidden;
  font-family: 'Inter', sans-serif;
}

/* Dekoratif arka plan topları */
.bg-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  opacity: 0.15;
  pointer-events: none;
}

.bg-orb-1 {
  width: 400px;
  height: 400px;
  background: #4f46e5;
  top: -100px;
  left: -100px;
  animation: float1 8s infinite ease-in-out;
}

.bg-orb-2 {
  width: 300px;
  height: 300px;
  background: #059669;
  bottom: -50px;
  right: -50px;
  animation: float2 10s infinite ease-in-out;
}

.bg-orb-3 {
  width: 200px;
  height: 200px;
  background: #7c3aed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  animation: float3 12s infinite ease-in-out;
}

@keyframes float1 {
  0%, 100% { transform: translate(0, 0) scale(1); }
  50% { transform: translate(30px, 20px) scale(1.1); }
}

@keyframes float2 {
  0%, 100% { transform: translate(0, 0) scale(1); }
  50% { transform: translate(-20px, -30px) scale(1.05); }
}

@keyframes float3 {
  0%, 100% { transform: translate(-50%, -50%) scale(1); }
  50% { transform: translate(-50%, -60%) scale(1.15); }
}

/* Ana kart */
.welcome-card {
  width: 100%;
  max-width: 440px;
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 32px;
  padding: 2.5rem 2rem;
  box-shadow: 0 25px 50px rgba(0, 0, 0, 0.5);
  color: white;
  animation: slideUp 0.7s cubic-bezier(0.16, 1, 0.3, 1) forwards;
  position: relative;
  z-index: 10;
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(40px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Marka alanı */
.brand {
  text-align: center;
  margin-bottom: 2rem;
}

.logo-ring {
  width: 80px;
  height: 80px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
  border: 1px solid rgba(255, 255, 255, 0.15);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
  overflow: hidden;
  animation: logoBounce 2.5s infinite ease-in-out;
}

@keyframes logoBounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-4px); }
}

.brand-logo {
  width: 100%;
  height: 100%;
  object-fit: contain;
  border-radius: 20px;
}

.brand-name {
  font-family: 'Outfit', sans-serif;
  font-size: 2.5rem;
  font-weight: 900;
  margin: 0;
  letter-spacing: -0.05em;
  background: linear-gradient(to right, #ffffff, #a5b4fc);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.brand-tagline {
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.4);
  margin: 0.25rem 0 0;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

/* Hoşgeldiniz metni */
.welcome-text {
  text-align: center;
  margin-bottom: 1.75rem;
}

.welcome-text h2 {
  font-family: 'Outfit', sans-serif;
  font-size: 1.4rem;
  font-weight: 700;
  margin: 0 0 0.25rem;
  color: rgba(255, 255, 255, 0.9);
}

.welcome-text p {
  font-size: 0.9rem;
  color: rgba(255, 255, 255, 0.45);
  margin: 0;
}

/* Giriş seçenekleri */
.login-options {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.option-card {
  display: flex;
  align-items: center;
  gap: 1.25rem;
  padding: 1.25rem 1.5rem;
  border-radius: 20px;
  text-decoration: none;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: 1px solid rgba(255, 255, 255, 0.08);
  cursor: pointer;
}

/* Veteriner kartı - mor/indigo tonları */
.vet-card {
  background: rgba(79, 70, 229, 0.15);
  border-color: rgba(99, 102, 241, 0.3);
}

.vet-card:hover {
  background: rgba(79, 70, 229, 0.3);
  border-color: rgba(99, 102, 241, 0.6);
  transform: translateY(-3px) scale(1.01);
  box-shadow: 0 12px 32px rgba(79, 70, 229, 0.25);
}

/* Müşteri kartı - zümrüt yeşili */
.client-card {
  background: rgba(5, 150, 105, 0.15);
  border-color: rgba(16, 185, 129, 0.3);
}

.client-card:hover {
  background: rgba(5, 150, 105, 0.3);
  border-color: rgba(16, 185, 129, 0.6);
  transform: translateY(-3px) scale(1.01);
  box-shadow: 0 12px 32px rgba(5, 150, 105, 0.25);
}

.option-icon-wrap {
  width: 52px;
  height: 52px;
  background: rgba(79, 70, 229, 0.2);
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  border: 1px solid rgba(99, 102, 241, 0.2);
  transition: all 0.3s ease;
}

.client-icon-wrap {
  background: rgba(5, 150, 105, 0.2);
  border-color: rgba(16, 185, 129, 0.2);
}

.option-icon {
  font-size: 1.6rem;
  line-height: 1;
}

.option-text {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.option-text strong {
  font-size: 1.05rem;
  font-weight: 700;
  color: rgba(255, 255, 255, 0.95);
}

.option-text span {
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.45);
  font-weight: 500;
}

.option-arrow {
  font-size: 1.5rem;
  color: rgba(255, 255, 255, 0.3);
  transition: all 0.3s ease;
  line-height: 1;
}

.vet-card:hover .option-arrow {
  color: rgba(99, 102, 241, 0.8);
  transform: translateX(4px);
}

.client-card:hover .option-arrow {
  color: rgba(16, 185, 129, 0.8);
  transform: translateX(4px);
}

.vet-card:hover .option-icon-wrap {
  background: rgba(79, 70, 229, 0.35);
  transform: scale(1.08);
}

.client-card:hover .client-icon-wrap {
  background: rgba(5, 150, 105, 0.35);
  transform: scale(1.08);
}

/* Alt bilgi */
.welcome-footer {
  margin-top: 2rem;
  text-align: center;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
  padding-top: 1.5rem;
}

.welcome-footer p {
  font-size: 0.75rem;
  color: rgba(255, 255, 255, 0.2);
  margin: 0;
}

/* Mobil uyum */
@media (max-width: 480px) {
  .welcome-card {
    border-radius: 24px;
    padding: 2rem 1.5rem;
  }

  .brand-name {
    font-size: 2rem;
  }

  .option-card {
    padding: 1rem 1.25rem;
  }
}
</style>
