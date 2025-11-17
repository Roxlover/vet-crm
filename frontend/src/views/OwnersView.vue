<template>
  <div class="page">
    <header class="page-header">
      <div>
        <h1>Hasta Sahipleri</h1>
        <p class="subtitle">
          Kliniğinizde kayıtlı tüm hasta sahiplerini yönetin.
        </p>
      </div>
    </header>

    <section class="grid">
      <!-- Sol: Liste -->
      <div class="card">
        <div class="card-header">
          <h2>Liste</h2>
          <button class="btn btn-sm" @click="loadOwners" :disabled="loading">
            Yenile
          </button>
        </div>

        <div v-if="loading" class="state">
          Yükleniyor...
        </div>
        <div v-else-if="error" class="state state-error">
          {{ error }}
        </div>
        <div v-else-if="owners.length === 0" class="state">
          Henüz hasta sahibi eklenmemiş.
        </div>
        <table v-else class="table">
          <thead>
            <tr>
              <th>İsim</th>
              <th>Telefon</th>
              <th>E-posta</th>
              <th>Adres</th>
              <th>Pet sayısı</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="owner in owners" :key="owner.id">
              <td>{{ owner.fullName }}</td>
              <td>{{ owner.phoneE164 }}</td>
              <td>{{ owner.email || '—' }}</td>
              <td>{{ owner.address || '—' }}</td>
              <td>{{ owner.petCount }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Sağ: Yeni ekleme formu -->
      <div class="card">
        <div class="card-header">
          <h2>Yeni Hasta Sahibi</h2>
        </div>

        <form class="form" @submit.prevent="handleCreate">
          <div class="form-group">
            <label for="fullName">Ad Soyad</label>
            <input
              id="fullName"
              v-model="form.fullName"
              type="text"
              required
              placeholder=""
            />
          </div>

          <div class="form-group">
            <label for="phone">Telefon</label>
            <input
              id="phone"
              v-model="form.phoneE164"
              type="tel"
              required
              placeholder=""
            />
            <small class="hint">
              0 yazmadan, ülke kodu ile birlikte (Türkiye için 90).
            </small>
          </div>

          <div class="form-group">
            <label for="email">E-posta</label>
            <input
              id="email"
              v-model="form.email"
              type="email"
              placeholder="opsiyonel"
            />
          </div>

          <div class="form-group">
            <label for="address">Adres</label>
            <textarea
              id="address"
              v-model="form.address"
              rows="3"
              placeholder="opsiyonel"
            />
          </div>

          <div class="form-actions">
            <button class="btn" type="submit" :disabled="creating">
              {{ creating ? 'Kaydediliyor...' : 'Kaydet' }}
            </button>
          </div>

          <p v-if="formError" class="state state-error">
            {{ formError }}
          </p>
          <p v-if="formSuccess" class="state state-success">
            {{ formSuccess }}
          </p>
        </form>
      </div>
    </section>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { fetchOwners, createOwner } from '../api/owners'

const owners = ref([])
const loading = ref(false)
const error = ref('')

const creating = ref(false)
const formError = ref('')
const formSuccess = ref('')

const form = reactive({
  fullName: '',
  phoneE164: '',
  email: '',
  address: '',
})

async function loadOwners() {
  loading.value = true
  error.value = ''
  try {
    const data = await fetchOwners()
    owners.value = data
  } catch (err) {
    console.error(err)
    error.value = 'Hasta sahipleri yüklenirken bir hata oluştu.'
  } finally {
    loading.value = false
  }
}

async function handleCreate() {
  formError.value = ''
  formSuccess.value = ''
  creating.value = true

  try {
    if (!form.fullName || !form.phoneE164) {
      formError.value = 'Ad soyad ve telefon zorunludur.'
      return
    }

    const payload = {
      fullName: form.fullName,
      phoneE164: form.phoneE164,
      email: form.email || null,
      address: form.address || null,
      kvkkOptIn: true,
    }

    await createOwner(payload)

    formSuccess.value = 'Kayıt başarıyla oluşturuldu.'
    // listeyi tazele
    await loadOwners()

    // formu temizle
    form.fullName = ''
    form.phoneE164 = ''
    form.email = ''
    form.address = ''
  } catch (err) {
    console.error(err)
    formError.value = 'Kayıt oluşturulurken bir hata oluştu.'
  } finally {
    creating.value = false
  }
}

onMounted(() => {
  loadOwners()
})
</script>

<style scoped>
.page {
  padding: 0.5rem 0.5rem 1.5rem;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-bottom: 1rem;
}

.page-header h1 {
  margin: 0;
  font-size: 1.4rem;
}

.subtitle {
  margin: 0.25rem 0 0;
  font-size: 0.85rem;
  color: #6b7280;
}

.grid {
  display: grid;
  grid-template-columns: 2fr 1.2fr;
  gap: 1rem;
}

@media (max-width: 900px) {
  .grid {
    grid-template-columns: 1fr;
  }
}

.card {
  background: #ffffff;
  border-radius: 0.75rem;
  padding: 1rem;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.card-header h2 {
  margin: 0;
  font-size: 1rem;
}

.btn {
  border: none;
  padding: 0.5rem 0.9rem;
  border-radius: 999px;
  background: #eaa0a0;
  color: #5c0022;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
}

.btn:hover {
  filter: brightness(0.95);
}

.btn:disabled {
  opacity: 0.6;
  cursor: default;
}

.btn-sm {
  padding: 0.35rem 0.7rem;
  font-size: 0.8rem;
}

.state {
  font-size: 0.9rem;
  color: #6b7280;
  padding: 0.4rem 0;
}

.state-error {
  color: #b91c1c;
}

.state-success {
  color: #15803d;
}

.table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}

.table th,
.table td {
  padding: 0.4rem 0.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.table th {
  text-align: left;
  font-weight: 600;
  color: #4b5563;
  font-size: 0.8rem;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 0.7rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

label {
  font-size: 0.85rem;
  font-weight: 500;
  color: #374151;
}

input,
textarea {
  border-radius: 0.5rem;
  border: 1px solid #d1d5db;
  padding: 0.45rem 0.6rem;
  font-size: 0.85rem;
  font-family: inherit;
}

input:focus,
textarea:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 1px #22c55e33;
}

.hint {
  font-size: 0.75rem;
  color: #6b7280;
}

.form-actions {
  margin-top: 0.5rem;
}
</style>
