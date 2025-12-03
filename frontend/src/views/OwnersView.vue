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
              <th>Pet sayısı</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="owner in owners" :key="owner.id">
              <td>{{ owner.fullName }}</td>
              <td>{{ owner.phoneE164 }}</td>
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

          <!-- ... Ad Soyad, Telefon, KVKK vs inputları ... -->

       <!-- Evcil Hayvanları -->
<section class="pets-section">
  <div class="pets-header">
    <h3>Evcil Hayvanları</h3>
  </div>

  <div
    v-for="(pet, index) in form.pets"
    :key="index"
    class="pet-card"
  >
    <div class="pet-card-header">
      <button
        v-if="form.pets.length > 1"
        type="button"
        class="link-button"
        @click="removePetRow(index)"
      >
        Bu peti kaldır
      </button>
    </div>

    <div class="pet-card-grid">
      <div class="field">
        <label>Pet adı</label>
        <input
          v-model="pet.name"
          type="text"
        />
      </div>

      <div class="field">
        <label>Tür</label>
        <input
          v-model="pet.species"
          type="text"
        />
      </div>

      <div class="field field-small">
        <label>Yaşı</label>
        <input
          v-model.number="pet.ageYears"
          type="number"
          min="0"
        />
      </div>
    </div>

    <div class="field">
      <label>Geçmiş   </label>
      <input
        v-model="pet.notes"
        type="text"
      />
    </div>
  </div>

  <button
    type="button"
    class="btn-secondary add-pet-button"
    @click="addPetRow"
  >
    + Pet ekle
  </button>
</section>


          <button type="button" class="btn-primary" @click="handleCreate" :disabled="creating">
            {{ creating ? 'Kaydediliyor…' : 'Kaydet' }}
          </button>

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
import { ref, reactive, onMounted } from 'vue'
import { fetchOwners, createOwner } from '../api/owners'

const owners = ref([])
const loading = ref(false)

const creating = ref(false)
const error = ref('')
const formError = ref('')
const formSuccess = ref('')

const form = reactive({
  fullName: '',
  phoneE164: '',
  kvkkOptIn: true,
  pets: [
    { name: '', species: '', ageYears: null, notes: '' }
  ]
})


// “+ Pet ekle” butonu burayı çağıracak
function addPetRow() {
  form.pets.push({
    name: '',
    species: '',
    ageYears: null,
    notes: ''
  })
}

function removePetRow(index) {
  if (form.pets.length === 1) return
  form.pets.splice(index, 1)
}


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

    const cleanedPets = form.pets
      .filter(p => (p.name && p.name.trim().length > 0))
      .map(p => ({
        name: p.name.trim(),
        species: p.species || null,
        ageYears: p.ageYears ?? null,
        notes: p.notes || null
      }))

    const payload = {
      fullName: form.fullName.trim(),
      phoneE164: form.phoneE164.trim(),
      kvkkOptIn: true,
      pets: cleanedPets
    }

    await createOwner(payload)

    formSuccess.value = 'Kayıt başarıyla oluşturuldu.'
    await loadOwners()

    // formu sıfırla
    form.fullName = ''
    form.phoneE164 = ''
    form.pets = [
      { name: '', species: '', ageYears: null, notes: '' }
    ]
  } catch (err) {
    console.error(err)
    formError.value = 'Kayıt oluşturulurken bir hata oluştu.'
  } finally {
    creating.value = false
  }
}

onMounted(loadOwners)
</script>




<style scoped>
.page {
  width: 100%;
  max-width: 1024px;
  margin: 0 auto;
  padding: 1rem; /* her sayfada ihtiyacına göre değiştirirsin */
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
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.25rem;

  padding: 0.45rem 0.9rem;
  border-radius: 999px;
  border: 1px solid #e5e7eb;

  background: #ffe2ab;
  color: #4c5137;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;

  transition:
    background-color 0.12s ease,
    border-color 0.12s ease,
    box-shadow 0.12s ease,
    transform 0.08s ease;
}

.btn:hover {
  background: #fde1c4;
  border-color: #ebc458;
  box-shadow: 0 4px 10px rgba(187, 208, 255, 0.08);
  transform: translateY(-0.5px);
}

.btn:active {
  transform: translateY(0);
  box-shadow: 0 2px 6px rgba(15, 23, 42, 0.08);
}

.btn:disabled {
  opacity: 0.6;
  cursor: default;
  box-shadow: none;
}

/* küçük buton (Yenile gibi) */
.btn-sm {
  padding: 0.3rem 0.7rem;
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
.form-row {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem;
}
.form-row-inline {
  display: flex;
  gap: 12px;   
}

.form-row-inline .form-group {
  flex: 1; 
}

 @media (max-width: 640px) {
  .form-row-inline {
    flex-direction: column;
  }
} 

.form-field-small {
  max-width: 120px;
}

.pet-row {
  margin-bottom: 1rem;
  padding: 0.75rem;
  border-radius: 0.5rem;
  background: #f9fafb;
}

.pet-separator {
  border: none;
  border-top: 1px dashed #e5e7eb;
  margin: 0.5rem 0 0;
}

.btn-outline.small,
.btn-link.small {
  font-size: 0.8rem;
  padding: 0.25rem 0.5rem;
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

.pets-section {
  margin-top: 1.5rem;
  padding: 1.25rem;
  background: #f9fafb;
  border-radius: 0.75rem;
  border: 1px dashed #e5e7eb;
}

.pets-header {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  margin-bottom: 0.75rem;
}

.pets-header h3 {
  margin: 0;
  font-size: 1rem;
}

.pets-subtitle {
  margin: 0;
  font-size: 0.8rem;
  color: #6b7280;
}

.pet-card {
  background: #ffffff;
  border-radius: 0.75rem;
  padding: 0.75rem 0.9rem;
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.06);
  margin-bottom: 0.75rem;
}

.pet-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.pet-tag {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 24px;
  height: 24px;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
  background: #dbeafe;
  color: #1d4ed8;
}

.link-button {
  border: none;
  background: transparent;
  font-size: 0.75rem;
  color: #dc2626;
  cursor: pointer;
  padding: 0;
}

.link-button:hover {
  text-decoration: underline;
}

.pet-card-grid {
  display: grid;
  grid-template-columns: 2fr 2fr 1fr;
  gap: 0.75rem;
  margin-bottom: 0.5rem;
}

.field-small input {
  max-width: 140px;
}

.add-pet-button {
  margin-top: 0.25rem;
  width: 100%;
  justify-content: center;
}

.btn-secondary {
  border-radius: 999px;
  border: 1px solid #d1d5db;
  background: #ffffff;
  padding: 0.45rem 0.9rem;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
}

.btn-secondary:hover {
  background: #f3f4f6;
}

@media (max-width: 768px) {
  .pet-card-grid {
    grid-template-columns: 1fr;
  }
}


.btn-primary {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.3rem;

  border: none;
  border-radius: 999px;

  padding: 0.55rem 1.2rem;
  font-size: 0.9rem;
  font-weight: 600;

  background: linear-gradient(135deg, #ffe2ab, #b93e19);
  color: #ffffff;

  cursor: pointer;
  box-shadow: 0 8px 20px rgba(22, 163, 74, 0.35);
  transition:
    transform 0.1s ease,
    box-shadow 0.1s ease,
    filter 0.1s ease;
}

.btn-primary:hover {
  filter: brightness(1.03);
  box-shadow: 0 10px 25px rgba(22, 163, 74, 0.45);
  transform: translateY(-1px);
}

.btn-primary:active {
  transform: translateY(0);
  box-shadow: 0 4px 12px rgba(22, 163, 74, 0.3);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: default;
  box-shadow: none;
}

</style>
