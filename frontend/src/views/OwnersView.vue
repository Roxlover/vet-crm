<template>
  <div class="page">
    <header class="page-header">
      <div>
        <h1>Hasta Sahipleri</h1>
        <p class="subtitle">Kliniğinizde kayıtlı tüm hasta sahiplerini yönetin.</p>
      </div>
    </header>

    <section class="grid">
      <!-- Sol: Liste -->
      <div class="card">
        <div class="card-header">
          <h2>Liste</h2>
          <div class="header-actions">
            <input 
              v-model="searchQuery" 
              type="text" 
              placeholder="Ara (İsim veya Telefon)..." 
              class="search-input"
              @input="handleSearch"
            />
            <button class="btn btn-sm" @click="loadOwners" :disabled="loading">
              Yenile
            </button>
          </div>
        </div>

        <div v-if="loading" class="state">Yükleniyor...</div>
        <div v-else-if="error" class="state state-error">{{ error }}</div>
        <div v-else-if="owners.length === 0" class="state">
          Henüz hasta sahibi eklenmemiş.
        </div>

        <table v-else class="table">
          <thead>
            <tr>
              <th>İsim</th>
              <th>Telefon</th>
              <th>Pet sayısı</th>
              <th class="actions-col"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="owner in owners" :key="owner.id">
              <td>{{ owner.fullName }}</td>
              <td>{{ owner.phoneE164 }}</td>
              <td>{{ owner.petCount }}</td>
              <td class="actions">
                <button class="btn btn-sm" type="button" @click="openOwnerDetail(owner.id)">
                  Detay
                </button>
              </td>
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
            <input id="fullName" v-model="form.fullName" type="text" required />
          </div>

          <div class="form-group">
            <label for="phone">Telefon</label>
            <input id="phone" v-model="form.phoneE164" type="tel" required />
            <small class="hint">0 yazmadan, ülke kodu ile birlikte (Türkiye için 90).</small>
          </div>

          <section class="pets-section">
            <div class="pets-header">
              <h3>Evcil Hayvanları</h3>
            </div>

            <div v-for="(pet, index) in form.pets" :key="index" class="pet-card">
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
    <input v-model="pet.name" type="text" />
  </div>

  <div class="field">
    <label>Tür</label>
    <input v-model="pet.species" type="text" />
  </div>

  <div class="field field-small">
    <label>Yıl</label>
    <input v-model.number="pet.ageYears" type="number" min="0" />
  </div>

  <div class="field field-small">
    <label>Ay</label>
    <input v-model.number="pet.ageMonths" type="number" min="0" max="11" />
  </div>
</div>
 
              <div class="field">
                <label>Geçmiş</label>
                <input v-model="pet.notes" type="text" />
              </div>
            </div>

            <button type="button" class="btn-secondary add-pet-button" @click="addPetRow">
              + Pet ekle
            </button>
          </section>

          <div class="form-actions">
            <button class="btn" type="submit" :disabled="creating">
              {{ creating ? 'Kaydediliyor...' : 'Kaydet' }}
            </button>
          </div>

          <p v-if="formError" class="state state-error">{{ formError }}</p>
          <p v-if="formSuccess" class="state state-success">{{ formSuccess }}</p>
        </form>
      </div>
    </section>

    <!-- Owner Detail Modal -->
    <div v-if="showDetailModal" class="modal-backdrop" @click.self="closeOwnerDetail">
      <div class="modal">
        <div class="modal-header">
          <h3>Hasta Sahibi Detayı</h3>
          <button class="btn btn-sm" type="button" @click="closeOwnerDetail">Kapat</button>
        </div>

        <div v-if="detailLoading" class="state modal-state">Yükleniyor...</div>
        <div v-else-if="!ownerDetail" class="state state-error modal-state">
          Detay yüklenemedi.
        </div>

        <div v-else class="modal-body">
          <div class="kv">
            <div><strong>Ad Soyad:</strong> {{ ownerDetail.fullName }}</div>
            <div><strong>Telefon:</strong> {{ ownerDetail.phoneE164 }}</div>
          </div>
          
          <!-- ✅ Notlar Bölümü (En Üstte) -->
          <section class="notes-section">
            <h4 class="section-title">Hasta Sahibi Notları</h4>
            
            <div class="note-add">
              <textarea 
                v-model="noteText" 
                placeholder="Bu hasta sahibi için yeni bir not yazın..."
                rows="2"
                class="note-textarea"
              ></textarea>
              <div class="note-add-actions">
                <button 
                  class="btn btn-sm" 
                  type="button" 
                  @click="handleAddNote" 
                  :disabled="noteAdding || !noteText.trim()"
                >
                  {{ noteAdding ? 'Ekleniyor...' : 'Not Ekle' }}
                </button>
                <span v-if="noteError" class="state state-error">{{ noteError }}</span>
              </div>
            </div>

            <div v-if="ownerDetail.notes && ownerDetail.notes.length > 0" class="notes-history">
              <div v-for="note in ownerDetail.notes" :key="note.id" class="note-item">
                <div class="note-content">{{ note.note }}</div>
                <div class="note-meta">
                  {{ new Date(note.createdAt).toLocaleString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) }}
                </div>
              </div>
            </div>
            <p v-else class="muted no-notes">Henüz not eklenmemiş.</p>
          </section>

          <h4 class="section-title">Evcil Hayvanlar</h4>

          <!-- ✅ Pet Ekle Formu -->
          <div class="pet-add">
            <div class="pet-add-grid">
              <div class="field">
                <label>Pet adı</label>
                <input v-model="newPet.name" type="text" />
              </div>

              <div class="field">
                <label>Tür</label>
                <input v-model="newPet.species" type="text" />
              </div>

              <div class="field field-small">
                <label>Yıl</label>
                <input v-model.number="newPet.ageYears" type="number" min="0" />
              </div>

              <div class="field field-small">
                <label>Ay</label>
                <input v-model.number="newPet.ageMonths" type="number" min="0" max="11" />
              </div>

              <div class="field">
                <label>Cins (Breed)</label>
                <input v-model="newPet.breed" type="text" />
              </div>

              <div class="field">
                <label>Doğum Tarihi</label>
                <input v-model="newPet.birthDate" type="date" />
              </div>

              <div class="field full">
                <label>Geçmiş</label>
                <input v-model="newPet.notes" type="text" />
              </div>
            </div>

            <div class="pet-add-actions">
              <button class="btn btn-sm" type="button" @click="addPet" :disabled="petAdding">
                {{ petAdding ? 'Ekleniyor...' : 'Pet Ekle' }}
              </button>
              <div v-if="petAddError" class="state state-error">{{ petAddError }}</div>
            </div>
          </div>
          <p v-if="petDeleteError" class="state state-error">{{ petDeleteError }}</p>

          <div v-if="!ownerDetail.pets || ownerDetail.pets.length === 0" class="muted">
            Kayıtlı hayvan yok.
          </div>

          <div v-else class="pets-list">
            <div v-for="p in ownerDetail.pets" :key="p.id" class="pet-row">
              <div class="pet-main">
                <div>
                  <strong>{{ p.name || '-' }}</strong>
                  <span class="muted">({{ p.species || '-' }})</span>
                </div>
                <div class="muted">
                  Yaş: {{ p.ageYears ?? '-' }}
                  <span v-if="p.ageMonths != null">y {{ p.ageMonths }}a</span>
                </div>
                <div v-if="p.notes" class="muted">Not: {{ p.notes }}</div>
              </div>
            <button class="btn btn-sm" type="button" @click="removePet(p.id)">
              Sil
           </button>
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { fetchOwners, createOwner, fetchOwner, addPetToOwner, deletePet, addOwnerNote, searchOwners } from '../api/owners'

const owners = ref([])
const searchQuery = ref('')
const loading = ref(false)
const error = ref('')
const petDeleteError = ref('')
const showDetailModal = ref(false)
const detailLoading = ref(false)
const ownerDetail = ref(null)
const selectedOwner = ref(null)

const creating = ref(false)
const formError = ref('')
const formSuccess = ref('')

const petAdding = ref(false)
const petAddError = ref('')

const noteText = ref('')
const noteAdding = ref(false)
const noteError = ref('')

const form = reactive({
  fullName: '',
  phoneE164: '',
  kvkkOptIn: true,
  pets: [{ name: '', species: '', ageYears: null, ageMonths: null, notes: '' }]
})

const newPet = reactive({
  name: '',
  species: '',
  ageYears: null,
  ageMonths: null,
  breed: '',
  birthDate: '',
  notes: ''
})

function resetNewPet() {
  newPet.name = ''
  newPet.species = ''
  newPet.ageYears = null
  newPet.ageMonths = null
  newPet.breed = ''
  newPet.birthDate = ''
  newPet.notes = ''
  petAddError.value = ''
}

async function loadOwners() {
  loading.value = true
  error.value = ''
  searchQuery.value = ''
  try {
    const res = await fetchOwners()
    owners.value = res?.data ?? res
  } catch (err) {
    console.error(err)
    error.value = 'Hasta sahipleri yüklenirken bir hata oluştu.'
  } finally {
    loading.value = false
  }
}

let searchTimeout = null
function handleSearch() {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(async () => {
    if (!searchQuery.value.trim()) {
      loadOwners()
      return
    }
    
    loading.value = true
    try {
      const res = await searchOwners(searchQuery.value.trim())
      owners.value = res?.data ?? res
    } catch (err) {
      console.error(err)
    } finally {
      loading.value = false
    }
  }, 400)
}

async function openOwnerDetail(id) {
  showDetailModal.value = true
  detailLoading.value = true
  try {
    selectedOwner.value = id
    const res = await fetchOwner(id)
    ownerDetail.value = res?.data ?? res
  } catch (err) {
    console.error(err)
    ownerDetail.value = null
  } finally {
    detailLoading.value = false
  }
}

function closeOwnerDetail() {
  showDetailModal.value = false
  ownerDetail.value = null
  selectedOwner.value = null
  resetNewPet()
}

async function addPet() {
  if (!selectedOwner.value) return

  petAddError.value = ''
  if (!newPet.name || !newPet.name.trim()) {
    petAddError.value = 'Pet adı zorunludur.'
    return
  }

  petAdding.value = true
  try {
    const payload = {
      name: newPet.name.trim(),
      species: newPet.species?.trim() || null,
      ageYears: newPet.ageYears ?? null,
      ageMonths: newPet.ageMonths ?? null,
      breed: newPet.breed?.trim() || null,
      birthDate: newPet.birthDate || null,
      notes: newPet.notes?.trim() || null
    }

    await addPetToOwner(selectedOwner.value, payload)

    const res = await fetchOwner(selectedOwner.value)
    ownerDetail.value = res?.data ?? res
    await loadOwners()

    resetNewPet()
  } catch (err) {
    console.error(err)
    petAddError.value = 'Pet eklenirken hata oluştu.'
  } finally {
    petAdding.value = false
  }
}

async function handleAddNote() {
  if (!selectedOwner.value || !noteText.value.trim()) return

  noteError.value = ''
  noteAdding.value = true
  try {
    await addOwnerNote(selectedOwner.value, noteText.value.trim())
    noteText.value = ''
    
    // Yenile
    const res = await fetchOwner(selectedOwner.value)
    ownerDetail.value = res?.data ?? res
  } catch (err) {
    console.error(err)
    noteError.value = 'Not eklenirken bir hata oluştu.'
  } finally {
    noteAdding.value = false
  }
}

async function removePet(petId) {
  if (!selectedOwner.value) return

  petDeleteError.value = ''

  try {
    await deletePet(petId) // (Aşağıda API wrapper kısmını netleştiriyorum)
    const res = await fetchOwner(selectedOwner.value)
    ownerDetail.value = res?.data ?? res
    await loadOwners()
  } catch (err) {
    // axios ise:
    const msg = err?.response?.data || 'Pet silinirken hata oluştu.'
    petDeleteError.value = typeof msg === 'string' ? msg : (msg?.message || 'Pet silinirken hata oluştu.')
    console.error(err)
  }
}

function addPetRow() {
  form.pets.push({ name: '', species: '', ageYears: null, ageMonths: null, notes: '' })
}

function removePetRow(index) {
  if (form.pets.length === 1) return
  form.pets.splice(index, 1)
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
      .filter(p => p.name && p.name.trim().length > 0)
      .map(p => ({
        name: p.name.trim(),
        species: p.species || null,
        ageYears: p.ageYears ?? null,
        ageMonths: p.ageMonths ?? null,
        notes: p.notes || null
      }))

    await createOwner({
      fullName: form.fullName.trim(),
      phoneE164: form.phoneE164.trim(),
      kvkkOptIn: true,
      pets: cleanedPets
    })

    formSuccess.value = 'Kayıt başarıyla oluşturuldu.'
    await loadOwners()

    form.fullName = ''
    form.phoneE164 = ''
    form.pets = [{ name: '', species: '', ageYears: null, ageMonths: null, notes: '' }]
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
  padding: 1rem;
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
  gap: 1rem;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex: 1;
  justify-content: flex-end;
}

.search-input {
  max-width: 200px;
  padding: 0.35rem 0.75rem;
  font-size: 0.8rem;
  border-radius: 8px;
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
  transition: background-color 0.12s ease, border-color 0.12s ease, box-shadow 0.12s ease, transform 0.08s ease;
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

.actions-col {
  width: 1%;
  white-space: nowrap;
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

.pet-card {
  background: #ffffff;
  border-radius: 0.75rem;
  padding: 0.75rem 0.9rem;
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.06);
  margin-bottom: 0.75rem;
}

.pet-card-header {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  margin-bottom: 0.5rem;
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
  grid-template-columns: 2fr 2fr 1fr 1fr;
  gap: 0.75rem;
  margin-bottom: 0.5rem;
}

.field-small input {
  max-width: 140px;
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

.add-pet-button {
  margin-top: 0.25rem;
  width: 100%;
  justify-content: center;
}

@media (max-width: 768px) {
  .pet-card-grid {
    grid-template-columns: 1fr;
  }
}

/* Modal */
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 16px;
  z-index: 9999;
}

.modal {
  width: min(820px, 100%);
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.25);
  overflow: hidden;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid #eee;
}

.modal-body {
  padding: 16px;
}

.modal-state {
  padding: 16px;
}

.kv {
  display: grid;
  gap: 6px;
}

.section-title {
  margin-top: 16px;
  margin-bottom: 10px;
}

.muted {
  opacity: 0.7;
}

/* Pet list in modal */
.pets-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-top: 10px;
}

.pet-row {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  border: 1px solid #eee;
  border-radius: 10px;
  padding: 12px;
}

.pet-main {
  display: grid;
  gap: 4px;
}

/* Pet add in modal */
.pet-add {
  margin: 12px 0 12px;
  padding: 12px;
  border: 1px dashed #e5e7eb;
  border-radius: 10px;
  background: #fafafa;
}

.pet-add-grid {
  display: grid;
  grid-template-columns: 2fr 2fr 1fr 1fr;
  gap: 12px;
}

.pet-add-grid .full {
  grid-column: 1 / -1;
}

.pet-add-actions {
  margin-top: 10px;
  display: flex;
  align-items: center;
  gap: 12px;
}

@media (max-width: 768px) {
  .pet-add-grid {
    grid-template-columns: 1fr;
  }
}

/* Notes Section */
.notes-section {
  margin: 1rem 0 1.5rem;
  padding: 1rem;
  background: #fdfaf3;
  border-radius: 12px;
  border: 1px solid #f3e8d2;
}

.note-add {
  margin-bottom: 1rem;
}

.note-textarea {
  width: 100%;
  resize: vertical;
  min-height: 60px;
  margin-bottom: 0.5rem;
  border: 1px solid #e5e7eb;
  background: #fff;
}

.note-add-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.notes-history {
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: 250px;
  overflow-y: auto;
  padding-right: 4px;
}

.note-item {
  padding: 10px;
  background: #fff;
  border-radius: 8px;
  border: 1px solid #f1f5f9;
  box-shadow: 0 2px 4px rgba(0,0,0,0.02);
}

.note-content {
  font-size: 0.9rem;
  color: #1e293b;
  white-space: pre-wrap;
  line-height: 1.4;
}

.note-meta {
  font-size: 0.75rem;
  color: #94a3b8;
  margin-top: 6px;
  text-align: right;
}

.no-notes {
  font-size: 0.85rem;
  font-style: italic;
  margin: 0.5rem 0;
}

.notes-history::-webkit-scrollbar {
  width: 5px;
}

.notes-history::-webkit-scrollbar-thumb {
  background: #e2e8f0;
  border-radius: 10px;
}
</style>
