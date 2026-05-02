<template>
  <main class="page-owners">
    <header class="page-header">
      <div class="header-content">
        <h1>Hasta Sahipleri</h1>
        <p class="subtitle">Klinik kayıtlarınızı ve müşteri notlarını buradan yönetin.</p>
      </div>
    </header>

    <div class="owners-grid">
      <!-- Sol: Liste -->
      <div class="list-container">
        <div class="search-section">
          <div class="search-input-wrapper">
            <span class="search-icon">🔍</span>
            <input 
              v-model="searchQuery" 
              type="text" 
              placeholder="İsim veya telefon ile ara..." 
              @input="handleSearch"
            />
          </div>
          <button class="btn btn-secondary" @click="loadOwners" :disabled="loading" style="padding: 0 1.5rem; border-radius: 16px;">
            Yenile
          </button>
        </div>

        <div v-if="loading" class="state">Yükleniyor...</div>
        <div v-else-if="error" class="state state-error">{{ error }}</div>
        <div v-else-if="owners.length === 0" class="state">
          Henüz hasta sahibi eklenmemiş.
        </div>

        <div v-else class="owners-list">
          <div v-for="owner in owners" :key="owner.id" class="owner-card">
            <div class="owner-info">
              <span class="name">{{ owner.fullName }}</span>
              <span class="phone">{{ owner.phoneE164 }}</span>
            </div>
            <div class="owner-actions">
              <span class="pet-badge">{{ owner.petCount }} Pet</span>
              <button class="btn" style="background: #ffffff; color: var(--primary); padding: 0.5rem 1rem; border-radius: 10px; font-size: 0.9rem;" @click="openOwnerDetail(owner.id)">
                Detay
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Sağ: Yeni ekleme formu -->
      <div class="form-card">
        <h2>Yeni Hasta Sahibi</h2>

        <form class="form" @submit.prevent="handleCreate">
          <div class="form-group">
            <label for="fullName">Ad Soyad</label>
            <input id="fullName" v-model="form.fullName" type="text" placeholder="Müşteri Adı" required />
          </div>

          <div class="form-group">
            <label for="phone">Telefon</label>
            <input id="phone" v-model="form.phoneE164" type="tel" placeholder="905xxxxxxxxx" required />
            <small class="hint" style="margin-top: 0.5rem; display: block;">Ülke kodu ile birlikte (Örn: 90).</small>
          </div>

          <section class="pets-section">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;">
              <h3 style="font-size: 1.1rem; font-weight: 800;">Evcil Hayvanlar</h3>
            </div>

            <div v-for="(pet, index) in form.pets" :key="index" class="pet-edit-card">
              <button
                v-if="form.pets.length > 1"
                type="button"
                class="remove-btn"
                @click="removePetRow(index)"
              >
                ✕
              </button>

              <div class="pet-grid">
                <div class="form-group" style="margin-bottom: 0;">
                  <label>Ad</label>
                  <input v-model="pet.name" type="text" placeholder="Pamuk" />
                </div>

                <div class="form-group" style="margin-bottom: 0;">
                  <label>Tür</label>
                  <input v-model="pet.species" type="text" placeholder="Kedi" />
                </div>
              </div>
            </div>

            <button type="button" class="btn btn-ghost" @click="addPetRow">
              + Başka Pet Ekle
            </button>
          </section>

          <div class="form-actions" style="margin-top: 2rem;">
            <button class="btn btn-primary" type="submit" :disabled="creating">
              {{ creating ? 'Kaydediliyor...' : 'Kaydı Tamamla' }}
            </button>
          </div>

<template>
  <main class="page-owners">
    <header class="page-header">
      <div class="header-content">
        <h1>Hasta Sahipleri</h1>
        <p class="subtitle">Klinik kayıtlarınızı ve müşteri notlarını buradan yönetin.</p>
      </div>
    </header>

    <div class="owners-grid">
      <!-- Sol: Liste -->
      <div class="list-container">
        <div class="search-section">
          <div class="search-input-wrapper">
            <span class="search-icon">🔍</span>
            <input 
              v-model="searchQuery" 
              type="text" 
              placeholder="İsim veya telefon ile ara..." 
              @input="handleSearch"
            />
          </div>
          <button class="btn btn-secondary" @click="loadOwners" :disabled="loading" style="padding: 0 1.5rem; border-radius: 16px;">
            Yenile
          </button>
        </div>

        <div v-if="loading" class="state">Yükleniyor...</div>
        <div v-else-if="error" class="state state-error">{{ error }}</div>
        <div v-else-if="owners.length === 0" class="state">
          Henüz hasta sahibi eklenmemiş.
        </div>

        <div v-else class="owners-list">
          <div v-for="owner in owners" :key="owner.id" class="owner-card">
            <div class="owner-info">
              <span class="name">{{ owner.fullName }}</span>
              <span class="phone">{{ owner.phoneE164 }}</span>
            </div>
            <div class="owner-actions">
              <span class="pet-badge">{{ owner.petCount }} Pet</span>
              <button class="btn" style="background: #ffffff; color: var(--primary); padding: 0.5rem 1rem; border-radius: 10px; font-size: 0.9rem;" @click="openOwnerDetail(owner.id)">
                Detay
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Sağ: Yeni ekleme formu -->
      <div class="form-card">
        <h2>Yeni Hasta Sahibi</h2>

        <form class="form" @submit.prevent="handleCreate">
          <div class="form-group">
            <label for="fullName">Ad Soyad</label>
            <input id="fullName" v-model="form.fullName" type="text" placeholder="Müşteri Adı" required />
          </div>

          <div class="form-group">
            <label for="phone">Telefon</label>
            <input id="phone" v-model="form.phoneE164" type="tel" placeholder="905xxxxxxxxx" required />
            <small class="hint" style="margin-top: 0.5rem; display: block;">Ülke kodu ile birlikte (Örn: 90).</small>
          </div>

          <section class="pets-section">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;">
              <h3 style="font-size: 1.1rem; font-weight: 800;">Evcil Hayvanlar</h3>
            </div>

            <div v-for="(pet, index) in form.pets" :key="index" class="pet-edit-card">
              <button
                v-if="form.pets.length > 1"
                type="button"
                class="remove-btn"
                @click="removePetRow(index)"
              >
                ✕
              </button>

              <div class="pet-grid">
                <div class="form-group" style="margin-bottom: 0;">
                  <label>Ad</label>
                  <input v-model="pet.name" type="text" placeholder="Pamuk" />
                </div>

                <div class="form-group" style="margin-bottom: 0;">
                  <label>Tür</label>
                  <input v-model="pet.species" type="text" placeholder="Kedi" />
                </div>
              </div>
            </div>

            <button type="button" class="btn btn-ghost" @click="addPetRow">
              + Başka Pet Ekle
            </button>
          </section>

          <div class="form-actions" style="margin-top: 2rem;">
            <button class="btn btn-primary" type="submit" :disabled="creating">
              {{ creating ? 'Kaydediliyor...' : 'Kaydı Tamamla' }}
            </button>
          </div>

          <p v-if="formError" class="state state-error" style="margin-top: 1rem;">{{ formError }}</p>
          <p v-if="formSuccess" class="state state-success" style="margin-top: 1rem;">{{ formSuccess }}</p>
        </form>
      </div>
    </div>

    <!-- Owner Detail Modal -->
    <div v-if="showDetailModal" class="modal-backdrop" @click.self="closeOwnerDetail">
      <div class="modal modern-owner-modal">
        <button class="modal-close-btn" @click="closeOwnerDetail">×</button>
        
        <div v-if="detailLoading" class="state">Yükleniyor...</div>
        
        <div v-else-if="ownerDetail" class="modal-content">
          <header class="modal-header-section">
            <div class="owner-avatar-large">
              {{ ownerDetail.fullName[0].toUpperCase() }}
            </div>
            <div class="owner-main-info">
              <h2>{{ ownerDetail.fullName }}</h2>
              <div class="contact-pill">
                <span>📞 {{ ownerDetail.phoneE164 }}</span>
              </div>
            </div>
          </header>

          <div class="modal-grid">
            <!-- Sol: Notlar -->
            <div class="modal-left">
              <section class="notes-premium-section">
                <div class="section-header">
                  <h4>Müşteri Notları</h4>
                  <span class="count-badge">{{ ownerDetail.notes?.length || 0 }}</span>
                </div>
                
                <div class="note-input-box">
                  <textarea 
                    v-model="noteText" 
                    placeholder="Yeni bir not ekleyin..."
                    rows="2"
                  ></textarea>
                  <button 
                    class="btn btn-primary btn-sm" 
                    @click="handleAddNote" 
                    :disabled="noteAdding || !noteText.trim()"
                  >
                    {{ noteAdding ? '...' : 'Ekle' }}
                  </button>
                </div>

                <div class="notes-scroll-area">
                  <div v-for="note in (ownerDetail.notes || [])" :key="note.id" class="modern-note-card">
                    <p>{{ note.note }}</p>
                    <span class="date">{{ new Date(note.createdAt).toLocaleDateString('tr-TR') }}</span>
                  </div>
                  <div v-if="!ownerDetail.notes?.length" class="empty-notes-hint">
                    Henüz not eklenmemiş.
                  </div>
                </div>
              </section>
            </div>

            <!-- Sağ: Hayvanlar ve Ekleme -->
            <div class="modal-right">
              <section class="pets-management-section">
                <div class="section-header">
                  <h4>Kayıtlı Hayvanlar</h4>
                  <button class="btn btn-ghost btn-xs" @click="showAddPetForm = !showAddPetForm">
                    {{ showAddPetForm ? 'Kapat' : '+ Yeni Pet' }}
                  </button>
                </div>

                <!-- Pet Ekleme Formu (Inline) -->
                <div v-if="showAddPetForm" class="inline-add-pet-card">
                  <div class="inline-grid">
                    <input v-model="newPet.name" placeholder="Pet Adı" class="mini-input" />
                    <input v-model="newPet.species" placeholder="Tür (Kedi/Köpek)" class="mini-input" />
                    <input v-model="newPet.breed" placeholder="Cins" class="mini-input" />
                    <input v-model="newPet.birthDate" type="date" class="mini-input" />
                  </div>
                  <button class="btn btn-primary btn-sm full-width" @click="addPet" :disabled="petAdding">
                    {{ petAdding ? 'Ekleniyor...' : 'Hayvanı Kaydet' }}
                  </button>
                  <p v-if="petAddError" class="error-text">{{ petAddError }}</p>
                </div>

                <div class="pets-mini-list">
                  <div v-for="p in (ownerDetail.pets || [])" :key="p.id" class="pet-mini-card">
                    <div class="p-info">
                      <strong>{{ p.name }}</strong>
                      <span>{{ p.species }} <template v-if="p.breed">({{ p.breed }})</template></span>
                    </div>
                    <button class="delete-icon-btn" @click="removePet(p.id)">🗑️</button>
                  </div>
                  <div v-if="!ownerDetail.pets?.length" class="empty-pets-hint">
                    Henüz hayvan kaydı yok.
                  </div>
                </div>
              </section>
            </div>
          </div>
        </div>
      </div>
    </div>
  </main>
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
const showAddPetForm = ref(false)

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
  showAddPetForm.value = false
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
    showAddPetForm.value = false
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
    await deletePet(petId) 
    const res = await fetchOwner(selectedOwner.value)
    ownerDetail.value = res?.data ?? res
    await loadOwners()
  } catch (err) {
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
.page-owners {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2.5rem;
}

.page-header h1 {
  font-size: 2.25rem;
  letter-spacing: -0.05em;
  font-weight: 800;
}

.subtitle {
  color: var(--text-muted);
  font-size: 1.1rem;
}

.owners-grid {
  display: grid;
  grid-template-columns: 1fr 400px;
  gap: 2.5rem;
  align-items: start;
}

/* LIST CARD */
.list-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.search-section {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
}

.search-input-wrapper {
  flex: 1;
  position: relative;
}

.search-input-wrapper input {
  width: 100%;
  padding: 1rem 1.25rem 1rem 3rem;
  border-radius: 16px;
  border: 1px solid #f1f5f9;
  background: #ffffff;
  font-size: 1rem;
  box-shadow: var(--shadow-sm);
  transition: var(--transition);
}

.search-input-wrapper input:focus {
  border-color: var(--primary);
  box-shadow: 0 0 0 4px var(--primary-light);
  outline: none;
}

.search-icon {
  position: absolute;
  left: 1.25rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-muted);
}

/* OWNER CARDS */
.owners-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.25rem;
}

.owner-card {
  padding: 1.5rem;
  border-radius: var(--radius-lg);
  background: #ffffff;
  border: 1px solid rgba(255, 255, 255, 0.8);
  box-shadow: var(--shadow-sm);
  transition: var(--transition);
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.owner-card:hover {
  transform: translateY(-5px);
  box-shadow: var(--shadow-md);
}

/* Renkli Kart Varyasyonları */
.owner-card:nth-child(4n+1) { background: #f5f3ff; } /* Lavanta */
.owner-card:nth-child(4n+2) { background: #f0fdf4; } /* Mint */
.owner-card:nth-child(4n+3) { background: #eff6ff; } /* Blue */
.owner-card:nth-child(4n+4) { background: #fff7ed; } /* Peach */

.owner-info .name {
  display: block;
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--text-main);
  margin-bottom: 0.25rem;
}

.owner-info .phone {
  font-size: 0.95rem;
  color: var(--text-muted);
  font-weight: 500;
}

.owner-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  padding-top: 1rem;
  border-top: 1px solid rgba(0,0,0,0.05);
}

.pet-badge {
  background: #ffffff;
  padding: 0.4rem 0.8rem;
  border-radius: 10px;
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--primary);
  box-shadow: 0 2px 5px rgba(0,0,0,0.02);
}

/* FORM CARD */
.form-card {
  background: #ffffff;
  border-radius: var(--radius-xl);
  padding: 2.5rem;
  box-shadow: var(--shadow-lg);
  border: 1px solid #f1f5f9;
  position: sticky;
  top: 2rem;
}

.form-card h2 {
  font-size: 1.5rem;
  font-weight: 800;
  margin-bottom: 2rem;
  letter-spacing: -0.03em;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  font-weight: 700;
  font-size: 0.85rem;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 0.5rem;
}

.form-group input {
  width: 100%;
  padding: 1rem;
  border-radius: 12px;
  border: 1px solid #f1f5f9;
  background: #f8fafc;
  font-size: 1rem;
  transition: var(--transition);
}

.form-group input:focus {
  background: #ffffff;
  border-color: var(--primary);
  box-shadow: 0 0 0 4px var(--primary-light);
  outline: none;
}

/* PET ROWS IN FORM */
.pets-section {
  margin-top: 2rem;
  border-top: 2px dashed #f1f5f9;
  padding-top: 2rem;
}

.pet-edit-card {
  background: #f8fafc;
  padding: 1.25rem;
  border-radius: 16px;
  margin-bottom: 1rem;
  position: relative;
}

.pet-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.remove-btn {
  position: absolute;
  top: -10px;
  right: -10px;
  width: 28px;
  height: 28px;
  background: #ffffff;
  border: 1px solid #fee2e2;
  color: var(--danger);
  border-radius: 50%;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: var(--shadow-sm);
}

/* BUTTONS */
.btn {
  padding: 1rem 1.5rem;
  border-radius: 14px;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  transition: var(--transition);
  border: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.btn-primary {
  background: var(--primary);
  color: #ffffff;
  width: 100%;
  box-shadow: 0 10px 20px -5px rgba(79, 70, 229, 0.4);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 15px 30px -10px rgba(79, 70, 229, 0.5);
}

.btn-ghost {
  background: transparent;
  color: var(--primary);
  font-size: 0.9rem;
  width: 100%;
  margin-top: 0.5rem;
}

/* MODAL */
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(12px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  width: 90%;
  max-width: 800px;
  background: #ffffff;
  border-radius: var(--radius-xl);
  padding: 3rem;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: var(--shadow-lg);
}

@media (max-width: 1024px) {
  .owners-grid { grid-template-columns: 1fr; }
  .form-card { position: static; }
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
    margin-bottom: 1.5rem;
  }

  .search-pill {
    width: 100%;
    max-width: none;
  }

  .modal {
    padding: 2rem 1.5rem;
  }

  .owner-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .owner-actions {
    width: 100%;
    justify-content: space-between;
  }

  .pet-row {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .pet-actions {
    width: 100%;
    justify-content: flex-end;
  }
}
</style>
