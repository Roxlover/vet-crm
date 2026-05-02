<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '../api/auth'

const router = useRouter()
const loading = ref(false)
const errorMessage = ref('')

const form = reactive({
  username: '',
  password: '',
})

async function handleLogin() {
  if (!form.username || !form.password) {
    errorMessage.value = 'Lütfen kullanıcı adı ve şifre girin.'
    return
  }

  loading.value = true
  errorMessage.value = ''

  try {
    await login(form.username, form.password)
    router.push('/')
  } catch (err) {
    console.error(err)
    errorMessage.value =
      err.response?.data || 'Giriş yapılamadı. Bilgilerinizi kontrol edin.'
  } finally {
    loading.value = false
  }
}

function devBypass() {
  if (window.location.hostname === 'localhost') {
    console.log('🌌 Geliştirici Modu: Gizli giriş yapıldı.');
    localStorage.setItem('vetcrm_token', 'dev-preview-token');
    localStorage.setItem('vetcrm_user', JSON.stringify({ username: 'Geliştirici', role: 'Admin' }));
    router.push('/');
  }
}

const starStyle = (n) => {
  const size = Math.random() * 2 + 1 + 'px'
  return {
    top: Math.random() * 100 + '%',
    left: Math.random() * 100 + '%',
    width: size,
    height: size,
    animationDelay: Math.random() * 5 + 's',
    animationDuration: Math.random() * 3 + 2 + 's'
  }
}
</script>

<template>
  <div class="login-page">
    <!-- 🌌 YILDIZLI GECE ARKA PLAN -->
    <div class="stars-container">
      <div class="star" v-for="n in 80" :key="n" :style="starStyle(n)"></div>
    </div>

    <div class="login-container">
      <div class="glass-card">
        <div class="login-header">
          <div class="logo-wrapper" @dblclick="devBypass">
            <img src="../logo.png" alt="Logo" class="logo-img" />
          </div>
          <h1>Vet-CRM</h1>
          <p class="subtitle">Klinik Yönetim Sistemine Hoş Geldiniz</p>
        </div>

        <form @submit.prevent="handleLogin" class="login-form">
          <div class="form-group">
            <label>Kullanıcı Adı</label>
            <div class="input-wrapper">
              <span class="input-icon">👤</span>
              <input
                v-model="form.username"
                type="text"
                placeholder="Kullanıcı adınız"
                autocomplete="username"
                required
              />
            </div>
          </div>

          <div class="form-group">
            <label>Şifre</label>
            <div class="input-wrapper">
              <span class="input-icon">🔒</span>
              <input
                v-model="form.password"
                type="password"
                placeholder="••••••••"
                autocomplete="current-password"
                required
              />
            </div>
          </div>

          <transition name="fade">
            <div v-if="errorMessage" class="error-alert">
              {{ errorMessage }}
            </div>
          </transition>

          <button class="login-btn" type="submit" :disabled="loading">
            <span v-if="!loading">Giriş Yap</span>
            <div v-else class="loader"></div>
          </button>
        </form>

        <div class="login-footer">
          <p>© 2026 Roxlover Vet-CRM. Tüm hakları saklıdır.</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: radial-gradient(circle at bottom, #1e293b, #0f172a, #020617);
  padding: 2rem;
  overflow: hidden;
  position: relative;
}

/* 🌌 STARS ANIMATION */
.stars-container {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.star {
  position: absolute;
  background: white;
  border-radius: 50%;
  opacity: 0.5;
  box-shadow: 0 0 12px 1px white;
  animation: twinkle linear infinite;
}

@keyframes twinkle {
  0%, 100% { opacity: 0.3; transform: scale(1); }
  50% { opacity: 0.8; transform: scale(1.3); }
}

/* LOGIN CONTAINER */
.login-container {
  width: 100%;
  max-width: 440px;
  position: relative;
  z-index: 10;
  animation: slideUp 0.6s cubic-bezier(0.16, 1, 0.3, 1);
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(40px); }
  to { opacity: 1; transform: translateY(0); }
}

.glass-card {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  padding: 3rem;
  border-radius: 32px;
  border: 1px solid rgba(255, 255, 255, 0.2);
  box-shadow: 0 25px 60px -15px rgba(0, 0, 0, 0.4);
  text-align: center;
}

.logo-wrapper {
  margin-bottom: 1.5rem;
  display: inline-block;
}

.logo-img {
  width: 90px;
  height: 90px;
  border-radius: 22px;
  box-shadow: 0 10px 25px rgba(99, 102, 241, 0.2);
  transition: transform 0.3s ease;
}

.logo-wrapper:hover .logo-img {
  transform: scale(1.05) rotate(5deg);
}

h1 {
  font-size: 2.25rem;
  font-weight: 800;
  color: #0f172a;
  letter-spacing: -0.05em;
  margin-bottom: 0.5rem;
}

.subtitle {
  color: #64748b;
  font-size: 1rem;
  margin-bottom: 2.5rem;
  font-weight: 500;
}

/* FORM STYLES */
.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  text-align: left;
}

.form-group label {
  display: block;
  font-size: 0.85rem;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 0.6rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 1.25rem;
  font-size: 1rem;
}

.login-form input {
  width: 100%;
  padding: 1rem 1.25rem 1rem 3rem;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  background: #f8fafc;
  font-size: 1rem;
  color: #0f172a;
  transition: all 0.25s ease;
  font-family: inherit;
}

.login-form input:focus {
  outline: none;
  border-color: #6366f1;
  background: #ffffff;
  box-shadow: 0 0 0 4px rgba(99, 102, 241, 0.1);
}

.login-btn {
  width: 100%;
  padding: 1.1rem;
  background: #6366f1;
  color: white;
  border: none;
  border-radius: 16px;
  font-size: 1.1rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.3s ease;
  margin-top: 1rem;
  display: flex;
  justify-content: center;
  align-items: center;
  box-shadow: 0 10px 20px -5px rgba(99, 102, 241, 0.4);
}

.login-btn:hover:not(:disabled) {
  background: #4f46e5;
  transform: translateY(-2px);
  box-shadow: 0 15px 25px -5px rgba(99, 102, 241, 0.5);
}

.login-btn:active {
  transform: translateY(0);
}

.login-btn:disabled {
  opacity: 0.8;
  cursor: not-allowed;
}

/* ERROR ALERT */
.error-alert {
  padding: 1rem;
  background: #fef2f2;
  color: #ef4444;
  border-radius: 12px;
  font-size: 0.9rem;
  font-weight: 600;
  border: 1px solid #fee2e2;
  margin-bottom: 0.5rem;
}

.login-footer {
  margin-top: 3rem;
  padding-top: 2rem;
  border-top: 1px solid #f1f5f9;
}

.login-footer p {
  font-size: 0.8rem;
  color: #94a3b8;
  font-weight: 500;
}

/* LOADER */
.loader {
  width: 24px;
  height: 24px;
  border: 3px solid rgba(255,255,255,0.3);
  border-radius: 50%;
  border-top-color: #fff;
  animation: spin 1s ease-in-out infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.fade-enter-active, .fade-leave-active { transition: opacity 0.3s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }

@media (max-width: 480px) {
  .glass-card { padding: 2rem; }
  h1 { font-size: 1.75rem; }
}
</style>
