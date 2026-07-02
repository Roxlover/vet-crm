<template>
  <div class="client-login-container">
    <div class="glass-login-card">
      <div class="brand-section">
        <div class="logo-wrapper">
          <span class="logo-emoji">🐾</span>
        </div>
        <h1 class="portal-title">BullVet</h1>
        <p class="portal-subtitle">Müşteri Portalı Girişi</p>
      </div>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="input-group">
          <label for="phone">Telefon Numarası</label>
          <div class="input-wrapper">
            <span class="input-icon">📞</span>
            <input
              id="phone"
              v-model="phone"
              type="tel"
              placeholder="5xx xxx xx xx"
              required
              :disabled="loading"
            />
          </div>
          <small class="helper-text">Sistemde kayıtlı olan numaranızı girin.</small>
        </div>

        <div class="input-group">
          <label for="password">Şifre</label>
          <div class="input-wrapper">
            <span class="input-icon">🔒</span>
            <input
              id="password"
              v-model="password"
              type="password"
              placeholder="••••••"
              required
              :disabled="loading"
            />
          </div>
          <small class="helper-text">Veteriner hekiminizin belirlediği şifre.</small>
        </div>

        <div v-if="error" class="error-banner">
          <span class="error-icon">⚠️</span>
          <span class="error-msg">{{ error }}</span>
        </div>

        <button type="submit" class="submit-btn" :disabled="loading">
          <span v-if="loading" class="spinner"></span>
          <span v-else>Giriş Yap</span>
        </button>
      </form>

      <footer class="login-footer">
        <p>Giriş yapamıyorsanız lütfen kliniğiniz ile iletişime geçerek şifre tanımlatın.</p>
      </footer>
    </div>

    <!-- HATA BİLGİLENDİRME MODALI -->
    <div v-if="showErrorModal" class="modal-overlay" @click="showErrorModal = false">
      <div class="modal-card" @click.stop>
        <div class="modal-header">
          <span class="modal-title-icon">⚠️</span>
          <h2>Giriş Başarısız</h2>
        </div>
        <div class="modal-body">
          <p class="main-error-msg">{{ error }}</p>
          <div v-if="debugDetails" class="debug-section">
            <span class="debug-title">Sistem / Hata Detayı:</span>
            <pre class="debug-content">{{ debugDetails }}</pre>
          </div>
        </div>
        <div class="modal-footer">
          <button class="modal-close-btn" @click="showErrorModal = false">Kapat</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { http } from '@/api/http'
import { saveAuth } from '@/utils/auth'

const router = useRouter()
const phone = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')
const showErrorModal = ref(false)
const debugDetails = ref('')

async function handleLogin() {
  error.value = ''
  debugDetails.value = ''
  loading.value = true

  try {
    // Önceki tüm eski oturum kalıntılarını temizle
    localStorage.removeItem('vetcrm_token')
    localStorage.removeItem('vetcrm_user')

    const res = await http.post('/auth/client-login', {
      phone: phone.value,
      password: password.value,
    })

    const data = res.data
    
    // JWT Token'ı ve kullanıcı bilgilerini doğrudan diske yazıyoruz
    localStorage.setItem('vetcrm_token', data.token)
    localStorage.setItem('vetcrm_user', JSON.stringify({
      token: data.token,
      role: 'client',
      id: data.id,
      username: data.fullName,
      phone: data.phone,
    }))

    router.push('/client/dashboard')
  } catch (err) {
    console.error('[CLIENT LOGIN ERROR]', err)
    showErrorModal.value = true

    if (err.response) {
      // Sunucu yanıt verdi ama 4xx/5xx döndü
      const apiData = err.response.data
      error.value = typeof apiData === 'string' ? apiData : (apiData?.message || 'Telefon numarası veya şifre hatalı.')
      debugDetails.value = `Durum Kodu: ${err.response.status}\nDetay: ${JSON.stringify(apiData, null, 2)}`
    } else if (err.request) {
      // İstek yapıldı ama sunucudan yanıt alınamadı (CORS, ağ kesintisi vb.)
      error.value = 'Sunucuya ulaşılamadı. Lütfen internet bağlantınızı veya API durumunu kontrol edin.'
      debugDetails.value = `İstek Hatası: Sunucu yanıt vermedi. CORS engeli olabilir.\nAPI URL: ${http.defaults.baseURL}/auth/client-login`
    } else {
      error.value = 'Giriş işlemi başlatılırken beklenmedik bir hata oluştu.'
      debugDetails.value = err.message
    }
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.client-login-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #064e3b 0%, #065f46 50%, #047857 100%);
  padding: 1.5rem;
  font-family: 'Inter', sans-serif;
  box-sizing: border-box;
}

.glass-login-card {
  width: 100%;
  max-width: 420px;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid rgba(255, 255, 255, 0.18);
  border-radius: 32px;
  padding: 2.5rem 2rem;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.25);
  color: white;
  animation: slideUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.brand-section {
  text-align: center;
  margin-bottom: 2.5rem;
}

.logo-wrapper {
  width: 70px;
  height: 70px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 22px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
  box-shadow: 0 8px 32px rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.25);
  animation: pulse 3s infinite ease-in-out;
}

@keyframes pulse {
  0%, 100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.05);
  }
}

.logo-emoji {
  font-size: 2rem;
}

.portal-title {
  font-family: 'Outfit', sans-serif;
  font-size: 2.25rem;
  font-weight: 800;
  margin: 0;
  letter-spacing: -0.05em;
  background: linear-gradient(to right, #ffffff, #a7f3d0);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.portal-subtitle {
  font-size: 0.95rem;
  font-weight: 500;
  color: #a7f3d0;
  margin: 0.5rem 0 0;
  letter-spacing: 0.05em;
  text-transform: uppercase;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.input-group label {
  font-size: 0.85rem;
  font-weight: 600;
  color: #d1fae5;
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 1.25rem;
  font-size: 1.1rem;
  pointer-events: none;
  opacity: 0.8;
}

.input-wrapper input {
  width: 100%;
  padding: 1rem 1.25rem 1rem 3rem;
  background: rgba(255, 255, 255, 0.15);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 16px;
  color: white;
  font-size: 1rem;
  outline: none;
  transition: all 0.3s ease;
  font-family: inherit;
}

.input-wrapper input::placeholder {
  color: rgba(255, 255, 255, 0.5);
}

.input-wrapper input:focus {
  background: rgba(255, 255, 255, 0.25);
  border-color: #34d399;
  box-shadow: 0 0 0 4px rgba(52, 211, 153, 0.2);
}

.helper-text {
  font-size: 0.75rem;
  color: rgba(209, 250, 229, 0.7);
}

.error-banner {
  background: rgba(239, 68, 68, 0.2);
  border: 1px solid rgba(239, 68, 68, 0.4);
  border-radius: 14px;
  padding: 0.75rem 1rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  animation: shake 0.4s ease;
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-4px); }
  75% { transform: translateX(4px); }
}

.error-icon {
  font-size: 1.2rem;
}

.error-msg {
  font-size: 0.875rem;
  font-weight: 500;
  color: #fca5a5;
}

.submit-btn {
  background: linear-gradient(135deg, #34d399 0%, #059669 100%);
  color: white;
  border: none;
  padding: 1.1rem;
  border-radius: 16px;
  font-weight: 700;
  font-size: 1.1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: 0.5rem;
  box-shadow: 0 8px 24px rgba(5, 150, 105, 0.3);
}

.submit-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 28px rgba(5, 150, 105, 0.4);
}

.submit-btn:active {
  transform: translateY(0);
}

.submit-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  transform: none;
}

.spinner {
  width: 20px;
  height: 20px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.login-footer {
  margin-top: 2rem;
  text-align: center;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  padding-top: 1.5rem;
}

.login-footer p {
  font-size: 0.8rem;
  color: rgba(209, 250, 229, 0.6);
  line-height: 1.4;
  margin: 0;
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 1.5rem;
  animation: fadeInModal 0.25s ease-out;
}

@keyframes fadeInModal {
  from { opacity: 0; }
  to { opacity: 1; }
}

.modal-card {
  background: #ffffff;
  border-radius: 28px;
  width: 100%;
  max-width: 440px;
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.3);
  padding: 2rem;
  color: #1f2937;
  transform: scale(0.95);
  animation: scaleUpModal 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.1) forwards;
}

@keyframes scaleUpModal {
  to { transform: scale(1); }
}

.modal-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1.25rem;
}

.modal-title-icon {
  font-size: 2rem;
}

.modal-header h2 {
  font-family: 'Outfit', sans-serif;
  font-size: 1.5rem;
  font-weight: 800;
  color: #991b1b;
  margin: 0;
}

.modal-body {
  margin-bottom: 1.5rem;
}

.main-error-msg {
  font-size: 1.05rem;
  font-weight: 600;
  color: #374151;
  line-height: 1.5;
  margin: 0 0 1rem;
}

.debug-section {
  background: #f3f4f6;
  border-radius: 12px;
  padding: 0.85rem;
  border: 1px solid #e5e7eb;
}

.debug-title {
  display: block;
  font-size: 0.72rem;
  font-weight: 800;
  color: #6b7280;
  text-transform: uppercase;
  margin-bottom: 0.35rem;
}

.debug-content {
  margin: 0;
  font-family: 'Courier New', Courier, monospace;
  font-size: 0.75rem;
  white-space: pre-wrap;
  word-break: break-all;
  color: #374151;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
}

.modal-close-btn {
  background: #e5e7eb;
  color: #374151;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 12px;
  font-weight: 700;
  font-size: 0.95rem;
  cursor: pointer;
  transition: all 0.2s;
}

.modal-close-btn:hover {
  background: #d1d5db;
}

.modal-close-btn:active {
  transform: scale(0.97);
}
</style>
