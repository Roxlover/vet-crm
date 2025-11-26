<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { login } from '../api/auth'   // ✅ DİKKAT: ../api/auth

const router = useRouter()

const username = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

async function onSubmit() {
  error.value = ''
  loading.value = true

  try {
    const user = await login({
      username: username.value,
      password: password.value,
    })

    console.log('Giriş başarılı, kullanıcı:', user)

    router.push({ name: 'dashboard' })
  } catch (e) {
    console.error(e)
    error.value = 'Kullanıcı adı veya şifre hatalı.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <h1 class="title">BullVet</h1>
      <p class="subtitle">Kullanıcı adı ve şifrenizle giriş yapın</p>

      <form @submit.prevent="onSubmit">
        <div class="field">
          <label for="username">Kullanıcı adı</label>
          <input
            id="username"
            v-model="username"
            type="text"
            autocomplete="username"
            required
          />
        </div>

        <div class="field">
          <label for="password">Şifre</label>
          <input
            id="password"
            v-model="password"
            type="password"
            autocomplete="current-password"
            required
          />
        </div>

        <p v-if="error" class="error">{{ error }}</p>

        <button class="btn-login" type="submit" :disabled="loading">
          <span v-if="loading">Giriş yapılıyor...</span>
          <span v-else>Giriş yap</span>
        </button>
      </form>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #0f172a;
  padding: 1.5rem;
}

.login-card {
  width: 100%;
  max-width: 360px;
  background: #ffffff;
  border-radius: 0.9rem;
  padding: 1.5rem 1.75rem;
  box-shadow: 0 20px 60px rgba(15, 23, 42, 0.35);
}

.title {
  margin: 0;
  font-size: 1.4rem;
  font-weight: 700;
  color: #111827;
}

.subtitle {
  margin: 0.2rem 0 1.2rem;
  font-size: 0.9rem;
  color: #6b7280;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  margin-bottom: 0.9rem;
  font-size: 0.85rem;
}

.field label {
  font-weight: 600;
  color: #374151;
}

.field input {
  border-radius: 0.55rem;
  border: 1px solid #d1d5db;
  padding: 0.45rem 0.6rem;
  font-size: 0.9rem;
  outline: none;
}

.field input:focus {
  border-color: #2563eb;
  box-shadow: 0 0 0 1px rgba(37, 99, 235, 0.2);
}

.btn-login {
  width: 100%;
  border: none;
  border-radius: 999px;
  padding: 0.55rem 0.9rem;
  margin-top: 0.4rem;
  background: #111827;
  color: #ffffff;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
}

.btn-login:disabled {
  opacity: 0.7;
  cursor: default;
}

.error {
  margin: 0.25rem 0 0;
  font-size: 0.8rem;
  color: #dc2626;
}
</style>
