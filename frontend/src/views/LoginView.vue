<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '@/api/auth'

const router = useRouter()

const form = ref({
  username: '',
  password: '',
})

const loading = ref(false)
const errorMessage = ref('')

async function handleSubmit() {
  errorMessage.value = ''

  if (!form.value.username || !form.value.password) {
    errorMessage.value = 'Kullanıcı adı ve şifre zorunludur.'
    return
  }

  const credentials = {
    username: form.value.username.trim(),
    password: form.value.password,
  }

  console.log('[LOGIN][FORM]', credentials)

  try {
    loading.value = true

    const data = await login(credentials)

    console.log('[LOGIN][OK]', data)

    await router.push({ name: 'dashboard' })
  } catch (err) {
    console.error('[LOGIN][ERROR]', err)
    errorMessage.value = 'Kullanıcı adı veya şifre hatalı.'
  } finally {
    loading.value = false
  }
}
</script>



<template>
  <div class="login-page">
    <div class="login-card">
      <div class="login-header">
        <h1 class="login-title">e-BullVet</h1>
        <p class="login-subtitle">Kullanıcı adı ve şifrenizle giriş yapın</p>
      </div>

      <form class="login-form" @submit.prevent="handleSubmit">
        <label class="field">
          <span class="field-label">Kullanıcı adı</span>
          <input
            v-model="form.username"
            type="text"
            autocomplete="username"
            class="field-input"
          />
        </label>

        <label class="field">
          <span class="field-label">Şifre</span>
          <input
            v-model="form.password"
            type="password"
            autocomplete="current-password"
            class="field-input"
          />
        </label>

        <p v-if="errorMessage" class="error-text">
          {{ errorMessage }}
        </p>

        <button class="login-button" type="submit" :disabled="loading">
          {{ loading ? 'Giriş yapılıyor...' : 'Giriş yap' }}
        </button>
      </form>
    </div>
  </div>
</template>

<style scoped>
/* Sayfa: login kartını ortalamak için */
.login-page {
  width: 100%;
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;

  /* 🌌 YILDIZLI GECE ARKA PLAN */
  background-color: #020617;
  background-image:
    radial-gradient(1.5px 1.5px at 10px 10px, #ffffff 0, transparent 55%),
    radial-gradient(1.5px 1.5px at 30px 40px, #ffffff 0, transparent 55%),
    radial-gradient(1.5px 1.5px at 50px 20px, #ffffff 0, transparent 55%),
    radial-gradient(1.5px 1.5px at 70px 50px, #ffffff 0, transparent 55%);
  background-size: 80px 80px;
  background-repeat: repeat;
}


/* Kart */
.login-card {
  width: 100%;
  max-width: 420px;
  padding: 24px 20px;
  border-radius: 16px;
  background: #f9fafb;
  box-shadow: 0 20px 40px rgba(15, 23, 42, 0.4);
}

/* Başlık */
.login-header {
  margin-bottom: 20px;
}

.login-title {
  margin: 0 0 4px;
  font-size: 28px;
  font-weight: 700;
  color: #020617;
}

.login-subtitle {
  margin: 0;
  font-size: 14px;
  color: #6b7280;
}

/* Form alanları */
.login-form {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.field-label {
  font-size: 13px;
  font-weight: 500;
  color: #111827;
}

.field-input {
  height: 40px;
  border-radius: 10px;
  border: 1px solid #d1d5db;
  padding: 0 12px;
  font-size: 14px;
  outline: none;
  background: white;
}

.field-input:focus {
  border-color: #111827;
  box-shadow: 0 0 0 1px #11182733;
}

/* Hata yazısı */
.error-text {
  margin: 4px 0 0;
  font-size: 12px;
  color: #dc2626;
}

/* Buton */
.login-button {
  margin-top: 8px;
  width: 100%;
  height: 42px;
  border-radius: 999px;
  border: none;
  font-size: 15px;
  font-weight: 600;
  color: white;
  background: #020617;
  cursor: pointer;
  transition: filter 0.15s ease, transform 0.12s ease;
}

.login-button:hover:not(:disabled) {
  filter: brightness(1.05);
  transform: translateY(-1px);
}

.login-button:active:not(:disabled) {
  transform: translateY(0);
}

.login-button:disabled {
  opacity: 0.6;
  cursor: default;
}

/* Küçük ekranlarda padding ve radius ayarı */
@media (max-width: 480px) {
  .login-card {
    padding: 20px 16px;
    border-radius: 16px;
    box-shadow: 0 10px 30px rgba(15, 23, 42, 0.4);
  }

  .login-title {
    font-size: 24px;
  }
}
</style>
