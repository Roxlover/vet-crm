<template>
  <main class="page-visits">
    <header class="page-header">
      <div class="header-content">
        <h1>Ziyaretler</h1>
        <p class="subtitle">Klinik ziyaretlerini ve tedavi geçmişlerini buradan izleyin.</p>
      </div>
    </header>

    <div class="visits-layout">
      <!-- Sol: Filtreler ve Liste -->
      <div class="list-section">
        <div class="filters-bar">
          <div class="filter-item">
            <span class="filter-icon">📅</span>
            <input type="date" v-model="filterDate" @change="onDateChange" />
          </div>
          <div class="filter-item">
            <span class="filter-icon">🔍</span>
            <input type="text" v-model="searchQuery" placeholder="Pet veya sahip ismi..." @input="handleSearch" />
          </div>
        </div>

        <div class="quick-filters">
          <button class="q-btn" :class="{ active: activeFilter === 'today' }" @click="setQuickFilter('today')">Bugün</button>
          <button class="q-btn" :class="{ active: activeFilter === 'yesterday' }" @click="setQuickFilter('yesterday')">Dün</button>
          <button class="q-btn" :class="{ active: activeFilter === 'lastWeek' }" @click="setQuickFilter('lastWeek')">Geçen Hafta</button>
          <button class="q-btn" :class="{ active: activeFilter === 'lastMonth' }" @click="setQuickFilter('lastMonth')">Geçen Ay</button>
          <button class="q-btn" @click="setQuickFilter('all')">Tümü</button>
        </div>

        <div v-if="loading" class="state">Yükleniyor...</div>
        <div v-else-if="error" class="state state-error">{{ error }}</div>
        <div v-else-if="visits.length === 0" class="state">
          Henüz ziyaret kaydı bulunamadı.
        </div>

        <div v-else class="visit-list">
          <div v-for="visit in visits" :key="visit.id" class="visit-card" @click="openVisitDetail(visit)">
            <div class="visit-header">
              <div>
                <span class="pet-name">{{ visit.petName }}</span>
                <span class="owner-info">{{ visit.ownerName }}</span>
              </div>
              <div class="visit-time">
                {{ new Date(visit.performedAt).toLocaleDateString('tr-TR') }}
              </div>
            </div>

            <div class="treatment-info">
              <p><strong>İşlemler:</strong> {{ visit.procedures || 'Belirtilmedi' }}</p>
              <p v-if="visit.notes"><strong>Notlar:</strong> {{ visit.notes }}</p>
            </div>

            <div class="visit-footer">
              <span class="amount-badge">{{ visit.amountTl }} TL</span>
              <span v-if="visit.creditAmountTl > 0" style="color: var(--danger); font-weight: 700;">
                Veresiye: {{ visit.creditAmountTl }} TL
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Sağ: Yeni Ziyaret Formu -->
      <div class="form-card">
        <h2>Yeni Ziyaret</h2>

        <div class="form-group">
          <label>Hasta Sahibi</label>
          <div class="combo" ref="ownerComboRef">
            <div v-if="selectedOwner" class="chip" style="background: var(--primary-light); border: none; padding: 0.75rem 1rem; border-radius: 12px; display: flex; align-items: center; justify-content: space-between;">
              <span class="chip-text" style="font-weight: 700; color: var(--primary);">
                {{ selectedOwner.fullName }}
              </span>
              <button type="button" class="chip-x" @click="clearOwner" style="border: none; background: transparent; font-size: 1.2rem; cursor: pointer; color: var(--primary);">×</button>
            </div>
            <input
              v-else
              ref="ownerInputRef"
              class="combo-input"
              type="text"
              v-model="ownerQuery"
              placeholder="Sahip ara..."
              @focus="openOwnerDropdown"
              @input="openOwnerDropdown"
              @click="openOwnerDropdown"
            />
            <div v-if="ownerDropdownOpen" class="combo-popover">
              <div v-for="(owner, idx) in filteredOwners" :key="owner.id" class="combo-item" @mousedown="selectOwner(owner)">
                <div class="combo-title">{{ owner.fullName }}</div>
                <div class="combo-sub">{{ owner.phoneE164 }}</div>
              </div>
              <div v-if="ownerQuery.length >= 2 && !filteredOwners.length" class="combo-item">Sonuç bulunamadı.</div>
            </div>
          </div>
        </div>

        <div class="form-group">
          <label>Hasta (Pet)</label>
          <select v-model="selectedPetId" :disabled="!selectedOwnerId">
            <option value="">Seçiniz</option>
            <option v-for="pet in petsForSelectedOwner" :key="pet.id" :value="pet.id">
              {{ pet.name }} ({{ pet.species }})
            </option>
          </select>
        </div>

        <div class="form-group">
          <label>Uygulanan Tedavi</label>
          <textarea v-model="form.procedures" rows="3" placeholder="Neler yapıldı?"></textarea>
        </div>

        <div class="form-group">
          <label>Zaman</label>
          <input type="datetime-local" v-model="form.performedAt" />
        </div>

        <!-- Görsel Ekleme (Cloudflare R2) -->
        <div class="form-group">
          <label>Görsel(ler) Ekle</label>
          <div class="image-upload-wrapper" style="display: flex; flex-direction: column; gap: 0.5rem;">
            <button v-if="isNative" type="button" class="btn btn-secondary btn-sm" @click="takePicture" style="display: flex; align-items: center; justify-content: center; gap: 0.5rem;">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"></path><circle cx="12" cy="13" r="4"></circle></svg>
              Kamera ile Fotoğraf Çek
            </button>
            <input
              v-else
              type="file"
              accept="image/*"
              multiple
              @change="onFilesSelected"
              class="file-input"
            />
            <div v-if="form.imageFiles.length > 0" class="file-count">
              {{ form.imageFiles.length }} dosya seçildi.
            </div>
          </div>
        </div>

        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
          <div class="form-group">
            <label>Tutar (TL)</label>
            <input type="number" v-model.number="form.amountTl" placeholder="0.00" />
          </div>
          <div class="form-group">
            <label>Veresiye (TL)</label>
            <input type="number" v-model.number="form.creditAmountTl" placeholder="0.00" />
          </div>
        </div>

        <button class="btn btn-primary" style="margin-top: 1rem;" @click="handleSave" :disabled="saving">
          {{ saving ? 'Kaydediliyor...' : 'Ziyareti Kaydet' }}
        </button>

        <p v-if="error" class="state state-error" style="margin-top: 1rem;">{{ error }}</p>
        <p v-if="success" class="state state-success" style="margin-top: 1rem;">{{ success }}</p>
      </div>
    </div>

    <!-- VISIT DETAIL MODAL -->
    <div v-if="showDetailModal" class="modal-overlay" @click.self="closeDetailModal">
      <div class="modern-modal" @click.stop>
        <header class="modal-header">
          <div class="header-info">
            <template v-if="!visitEditOpen">
              <h2>
                <span class="pet-name">{{ selectedVisit?.petName }}</span>
                <span class="owner-name">{{ selectedVisit?.ownerName }}</span>
              </h2>
            </template>
            <template v-else>
              <h2 class="edit-title">Kayıt Düzenleme</h2>
            </template>
          </div>
          <button class="modal-close-btn" @click="closeDetailModal">✕</button>
        </header>

        <div class="modal-body" v-if="selectedVisit">
          <div class="section-header-row">
            <h3 class="section-subtitle">Ziyaret Bilgileri</h3>
            <div class="header-actions">
              <template v-if="!visitEditOpen">
                <button class="btn btn-ghost btn-xs" @click="openVisitEdit">Düzenle</button>
                <button class="btn btn-danger-sm btn-xs" @click="handleDeleteVisit" :disabled="visitSaving">Sil</button>
              </template>
              <div v-else class="edit-actions" style="display: flex; gap: 0.5rem;">
                <button class="btn btn-text btn-xs" @click="cancelVisitEdit">İptal</button>
                <button class="btn btn-primary-sm btn-xs" @click="saveVisitEdit" :disabled="visitSaving">
                  {{ visitSaving ? '...' : 'Kaydet' }}
                </button>
              </div>
            </div>
          </div>

          <div class="detail-grid" style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
            <template v-if="visitEditOpen && visitDraft">
              <div class="detail-item">
                <label>Pet Adı</label>
                <input type="text" v-model="visitDraft.petName" class="modern-input" />
              </div>
              <div class="detail-item">
                <label>Pet Türü</label>
                <input type="text" v-model="visitDraft.petSpecies" class="modern-input" />
              </div>
              <div class="detail-item">
                <label>Hasta Sahibi</label>
                <input type="text" v-model="visitDraft.ownerName" class="modern-input" />
              </div>
              <div class="detail-item">
                <label>Sahip Telefon</label>
                <input type="text" v-model="visitDraft.ownerPhone" class="modern-input" />
              </div>
            </template>

            <div class="detail-item full" style="grid-column: span 2;">
              <label>İşlem Tarihi</label>
              <div v-if="!visitEditOpen" class="val">{{ new Date(selectedVisit.performedAt).toLocaleString('tr-TR') }}</div>
              <input v-else-if="visitDraft" type="datetime-local" v-model="visitDraft.performedAt" class="modern-input" />
            </div>

            <div class="detail-item" style="grid-column: span 2;">
              <label>Yapılan İşlemler</label>
              <div v-if="!visitEditOpen" class="val highlight">{{ selectedVisit.procedures || '—' }}</div>
              <textarea v-else-if="visitDraft" v-model="visitDraft.procedures" class="modern-input" rows="3"></textarea>
            </div>

            <div class="detail-item">
              <label>Ziyaret Tutarı (TL)</label>
              <div v-if="!visitEditOpen" class="val">{{ selectedVisit.amountTl }} TL</div>
              <input v-else-if="visitDraft" type="number" v-model.number="visitDraft.amountTl" class="modern-input" />
            </div>

            <div class="detail-item">
              <label>Veresiye (TL)</label>
              <div v-if="!visitEditOpen" class="val">{{ selectedVisit.creditAmountTl || 0 }} TL</div>
              <input v-else-if="visitDraft" type="number" v-model.number="visitDraft.creditAmountTl" class="modern-input" />
            </div>

            <div class="detail-item full" style="grid-column: span 2;">
              <label>Notlar</label>
              <div v-if="!visitEditOpen" class="val">{{ selectedVisit.notes || '—' }}</div>
              <textarea v-else-if="visitDraft" v-model="visitDraft.notes" class="modern-input" rows="2"></textarea>
            </div>
          </div>

          <!-- BİLANÇO HIZLI TIKLA -->
          <div class="finance-card" style="margin-top: 2rem; background: #f8fafc; padding: 1.5rem; border-radius: 20px;">
            <div style="display: flex; justify-content: space-between; align-items: center;">
              <div>
                <span style="font-size: 0.8rem; color: #64748b; display: block;">Kalan Veresiye</span>
                <strong style="font-size: 1.25rem; color: var(--danger);">₺{{ selectedVisit.creditAmountTl || 0 }}</strong>
              </div>
              <div style="text-align: right;">
                <span style="font-size: 0.8rem; color: #64748b; display: block;">Tahsil Edilen</span>
                <strong style="font-size: 1.25rem; color: var(--success);">₺{{ selectedVisit.collectedAmountTl || 0 }}</strong>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>

<script setup>
import { computed, onMounted, onBeforeUnmount, reactive, ref, watch, nextTick  } from 'vue'
import { fetchOwners } from '../api/owners'
import { fetchPets, fetchPetsByOwner } from '../api/pets'
import { http } from '@/api/http'
import { uploadVisitImages } from '../api/visits'
import { Camera, CameraResultType, CameraSource } from '@capacitor/camera'
import { Capacitor } from '@capacitor/core'

const isNative = Capacitor.isNativePlatform()

const visits = ref([])
const owners = ref([])
const pets = ref([])
const loading = ref(false)
const filterDate = ref('') 
const searchQuery = ref('')
const activeFilter = ref('all')
const startDate = ref(null)
const endDate = ref(null)

const selectedOwnerId = ref('')
const selectedPetId = ref('')
const error = ref('')
const success = ref('')
const saving = ref(false)
const form = reactive({
  procedures: '',
  vaccines: '',
  performedAt: new Date().toISOString().substr(0, 16),
  creditAmountTl: '',
  amountTl: null,
  notes: '',
  imageFiles: [],
  microchipNumber: '',
})

const showDetailModal = ref(false)
const selectedVisit = ref(null)
const visitEditOpen = ref(false)
const visitSaving = ref(false)
const visitDraft = ref(null)

async function openVisitDetail(v) {
  loading.value = true
  try {
    const res = await http.get(`/visits/${v.id}`)
    selectedVisit.value = res.data
    showDetailModal.value = true
  } catch (err) {
    console.error(err)
    alert('Ziyaret detayı yüklenemedi.')
  } finally {
    loading.value = false
  }
}

function closeDetailModal() {
  showDetailModal.value = false
  selectedVisit.value = null
  visitEditOpen.value = false
  visitDraft.value = null
}

function openVisitEdit() {
  if (!selectedVisit.value) return
  const v = selectedVisit.value
  visitDraft.value = {
    performedAt: new Date(v.performedAt).toISOString().substr(0, 16),
    procedures: v.procedures || '',
    notes: v.notes || '',
    amountTl: v.amountTl || 0,
    creditAmountTl: v.creditAmountTl || 0,
    petName: v.petName || '',
    petSpecies: v.species || v.petSpecies || '',
    ownerName: v.ownerName || '',
    ownerPhone: v.phoneE164 || v.ownerPhone || '',
  }
  visitEditOpen.value = true
}

function cancelVisitEdit() {
  visitEditOpen.value = false
  visitDraft.value = null
}

async function saveVisitEdit() {
  if (!selectedVisit.value || !visitDraft.value) return
  visitSaving.value = true
  try {
    const v = selectedVisit.value
    
    // 1. Pet update
    if (v.petId) {
      await http.put(`/pets/${v.petId}`, {
        name: visitDraft.value.petName,
        species: visitDraft.value.petSpecies,
        breed: v.breed,
        birthDate: v.birthDate,
        notes: v.petNotes || ''
      })
    }
    
    // 2. Owner update
    if (v.ownerId) {
      await http.put(`/owners/${v.ownerId}`, {
        fullName: visitDraft.value.ownerName,
        phoneE164: visitDraft.value.ownerPhone,
        kvkkOptIn: true
      })
    }
    
    // 3. Visit update
    await http.put(`/visits/${v.id}`, {
      performedAt: new Date(visitDraft.value.performedAt).toISOString(),
      procedures: (visitDraft.value.procedures || '').trim(),
      amountTl: visitDraft.value.amountTl,
      creditAmountTl: visitDraft.value.creditAmountTl,
      notes: (visitDraft.value.notes || '').trim(),
      nextDate: v.nextDate,
      purpose: v.purpose
    })
    
    // Refresh
    const res = await http.get(`/visits/${v.id}`)
    selectedVisit.value = res.data
    visitEditOpen.value = false
    loadVisits()
  } catch (err) {
    console.error(err)
    alert('Güncelleme sırasında hata oluştu.')
  } finally {
    visitSaving.value = false
  }
}

async function handleDeleteVisit() {
  const visitId = selectedVisit.value?.id || selectedVisit.value?.Id
  if (!visitId) return
  if (!confirm('Bu ziyareti silmek istediğinize emin misiniz? (Kasa kayıtları da etkilenecektir)')) return
  
  visitSaving.value = true
  try {
    await http.delete(`/visits/${visitId}`)
    closeDetailModal()
    loadVisits()
  } catch (err) {
    console.error(err)
    alert('Ziyaret silinirken hata oluştu.')
  } finally {
    visitSaving.value = false
  }
}

const ownerQuery = ref('')
const ownerDropdownOpen = ref(false)
const ownerComboRef = ref(null)
const ownerInputRef = ref(null)

const selectedOwner = computed(() => {
  const idNum = Number(selectedOwnerId.value)
  if (!idNum) return null
  return owners.value.find(o => o.id === idNum) || null
})

const filteredOwners = computed(() => {
  const q = ownerQuery.value.trim().toLowerCase()
  if (q.length < 2) return []
  return owners.value.filter(o => 
    (o.fullName || '').toLowerCase().includes(q) || (o.phoneE164 || '').includes(q)
  ).slice(0, 50)
})

const petsForSelectedOwner = computed(() =>
  pets.value.filter((p) => p.ownerId === Number(selectedOwnerId.value))
)

async function loadVisits() {
  loading.value = true
  try {
    let params = new URLSearchParams()
    if (filterDate.value) params.append('date', filterDate.value)
    if (startDate.value) params.append('startDate', startDate.value)
    if (endDate.value) params.append('endDate', endDate.value)
    if (searchQuery.value) params.append('query', searchQuery.value)

    const res = await http.get(`/visits?${params.toString()}`)
    visits.value = res.data
  } catch (err) {
    error.value = 'Ziyaretler yüklenemedi.'
  } finally {
    loading.value = false
  }
}

function onDateChange() {
  activeFilter.value = 'custom'
  startDate.value = null
  endDate.value = null
  loadVisits()
}

function setQuickFilter(type) {
  activeFilter.value = type
  filterDate.value = ''
  
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())

  if (type === 'today') {
    startDate.value = today.toISOString()
    endDate.value = null
  } else if (type === 'yesterday') {
    const yesterday = new Date(today)
    yesterday.setDate(yesterday.getDate() - 1)
    const yesterdayEnd = new Date(yesterday)
    yesterdayEnd.setHours(23, 59, 59, 999)
    startDate.value = yesterday.toISOString()
    endDate.value = yesterdayEnd.toISOString()
  } else if (type === 'lastWeek') {
    const lastWeek = new Date(today)
    lastWeek.setDate(lastWeek.getDate() - 7)
    startDate.value = lastWeek.toISOString()
    endDate.value = null
  } else if (type === 'lastMonth') {
    const lastMonth = new Date(today)
    lastMonth.setMonth(lastMonth.getMonth() - 1)
    startDate.value = lastMonth.toISOString()
    endDate.value = null
  } else {
    startDate.value = null
    endDate.value = null
  }
  
  loadVisits()
}

async function loadOwnersAndPets() {
  try {
    const [ownersData, petsData] = await Promise.all([
      fetchOwners(),
      fetchPets(), 
    ])
    // 🔹 GÜVENLİ VERİ EŞLEME: res.data veya doğrudan liste olabilir
    owners.value = Array.isArray(ownersData) ? ownersData : (ownersData?.data ?? [])
    pets.value = Array.isArray(petsData) ? petsData : (petsData?.data ?? [])
  } catch (e) {
    console.error('loadOwnersAndPets ERROR:', e)
    error.value = 'Bilgiler yüklenirken hata oluştu.'
  }
}

function onFilesSelected(e) {
  form.imageFiles = Array.from(e.target.files || [])
}

async function takePicture() {
  try {
    const image = await Camera.getPhoto({
      quality: 80,
      allowEditing: false,
      resultType: CameraResultType.Uri,
      source: CameraSource.Camera
    })

    const response = await fetch(image.webPath)
    const blob = await response.blob()
    const file = new File([blob], 'photo_' + new Date().getTime() + '.jpg', { type: 'image/jpeg' })
    
    form.imageFiles.push(file)
  } catch (e) {
    if (e.message && !e.message.includes('User cancelled')) {
      console.error("Camera error", e)
      error.value = 'Kamera açılırken bir hata oluştu.'
    }
  }
}

function openOwnerDropdown() {
  ownerDropdownOpen.value = true
}

function selectOwner(owner) {
  selectedOwnerId.value = String(owner.id)
  ownerQuery.value = ''
  ownerDropdownOpen.value = false
}

function clearOwner() {
  selectedOwnerId.value = ''
  selectedPetId.value = ''
  ownerQuery.value = ''
}

async function handleSave() {
  error.value = ''
  success.value = ''
  if (!selectedOwnerId.value || !selectedPetId.value) {
    error.value = 'Lütfen hasta sahibi ve hastayı seçin.'
    return
  }
  saving.value = true
  try {
    const payload = {
      petId: Number(selectedPetId.value),
      performedAt: new Date(form.performedAt).toISOString(),
      procedures: form.procedures,
      amountTl: form.amountTl ?? 0,
      creditAmountTl: form.creditAmountTl ? Number(form.creditAmountTl) : null,
      notes: form.notes,
    }
    
    // 1. Ziyareti oluştur
    const res = await http.post('/visits', payload)
    const newVisit = res.data
    const visitId = newVisit.id || newVisit.Id

    // 2. Eğer görsel varsa Cloudflare'e (R2) yükle
    if (form.imageFiles && form.imageFiles.length > 0) {
      await uploadVisitImages(visitId, form.imageFiles)
    }

    success.value = 'Ziyaret başarıyla kaydedildi.'
    
    // Formu sıfırla
    form.procedures = ''
    form.amountTl = null
    form.creditAmountTl = ''
    form.notes = ''
    form.imageFiles = []
    selectedPetId.value = ''
    selectedOwnerId.value = ''
    
    loadVisits()
  } catch (e) {
    console.error('handleSave ERROR:', e)
    error.value = 'Kaydedilirken bir hata oluştu.'
  } finally {
    saving.value = false
  }
}

function handleSearch() {
  // Local or server search
}

function onDocPointerDown(e) {
  if (ownerComboRef.value && !ownerComboRef.value.contains(e.target)) {
    ownerDropdownOpen.value = false
  }
}

onMounted(() => {
  loadVisits()
  loadOwnersAndPets()
  document.addEventListener('pointerdown', onDocPointerDown)
})

onBeforeUnmount(() => {
  document.removeEventListener('pointerdown', onDocPointerDown)
})
</script>

<style scoped>
.page-visits {
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

.visits-layout {
  display: grid;
  grid-template-columns: 1fr 420px;
  gap: 2.5rem;
  align-items: start;
}

@media (max-width: 1024px) {
  .visits-layout {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }
  
  .form-card {
    position: static;
    order: -1; /* Mobilde formu en üste alalım */
  }
}

/* FILTERS & LIST */
.list-section {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.filters-bar {
  display: flex;
  gap: 1rem;
  background: #ffffff;
  padding: 1rem;
  border-radius: 20px;
  box-shadow: var(--shadow-sm);
  border: 1px solid #f1f5f9;
}

@media (max-width: 768px) {
  .filters-bar {
    flex-direction: column;
    padding: 0.75rem;
    gap: 0.75rem;
  }
}

.filter-item {
  flex: 1;
  position: relative;
}

.filter-item input {
  width: 100%;
  padding: 0.75rem 1rem 0.75rem 2.5rem;
  border-radius: 14px;
  border: 1px solid #f1f5f9;
  background: #f8fafc;
  font-size: 0.95rem;
  transition: var(--transition);
}

.filter-item input:focus {
  background: #ffffff;
  border-color: var(--primary);
  box-shadow: 0 0 0 4px var(--primary-light);
  outline: none;
}

.filter-icon {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-muted);
  font-size: 0.9rem;
}

/* VISIT CARDS */
.visit-list {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1.25rem;
}

.visit-card {
  background: #ffffff;
  border-radius: var(--radius-lg);
  padding: 1.75rem;
  border: 1px solid #f1f5f9;
  box-shadow: var(--shadow-sm);
  transition: var(--transition);
  position: relative;
}

@media (max-width: 768px) {
  .visit-card {
    padding: 1.25rem;
  }
  
  .pet-name {
    font-size: 1.1rem;
  }
}

.visit-card:hover {
  transform: translateX(8px);
  box-shadow: var(--shadow-md);
  border-color: var(--primary-light);
}

.visit-card::before {
  content: '';
  position: absolute;
  left: 0;
  top: 1.5rem;
  bottom: 1.5rem;
  width: 5px;
  border-radius: 0 10px 10px 0;
  background: var(--primary);
}

.visit-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1.25rem;
}

.pet-name {
  font-size: 1.25rem;
  font-weight: 800;
  color: var(--text-main);
  letter-spacing: -0.02em;
}

.owner-info {
  font-size: 0.95rem;
  color: var(--text-muted);
  margin-top: 0.25rem;
}

.visit-time {
  font-size: 0.85rem;
  font-weight: 700;
  color: var(--primary);
  background: var(--primary-light);
  padding: 0.4rem 0.8rem;
  border-radius: 10px;
}

.treatment-info {
  background: #f8fafc;
  padding: 1.25rem;
  border-radius: 16px;
  margin-bottom: 1.25rem;
}

.treatment-info p {
  font-size: 0.95rem;
  line-height: 1.6;
  color: var(--text-main);
}

.visit-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 1rem;
  border-top: 1px solid #f1f5f9;
}

.amount-badge {
  font-weight: 800;
  color: var(--success);
  font-size: 1.1rem;
}

/* FORM CARD (STICKY) */
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

.form-group input, .form-group select, .form-group textarea {
  width: 100%;
  padding: 1rem;
  border-radius: 12px;
  border: 1px solid #f1f5f9;
  background: #f8fafc;
  font-size: 1rem;
  transition: var(--transition);
  font-family: inherit;
}

.form-group input:focus, .form-group select:focus, .form-group textarea:focus {
  background: #ffffff;
  border-color: var(--primary);
  box-shadow: 0 0 0 4px var(--primary-light);
  outline: none;
}

/* COMBOBOX REFINEMENT */
.combo-popover {
  position: absolute;
  z-index: 1000; /* Çok yüksek tutalım */
  top: 100%;
  left: 0;
  right: 0;
  background: #ffffff;
  border-radius: 16px;
  box-shadow: 0 10px 40px rgba(0,0,0,0.15);
  margin-top: 8px;
  overflow-y: auto;
  max-height: 250px;
}

.combo-item {
  padding: 1rem;
  cursor: pointer;
  transition: var(--transition);
}

.combo-item:hover, .combo-item.active {
  background: var(--primary-light);
}

.combo-title { font-weight: 700; }
.combo-sub { font-size: 0.85rem; color: var(--text-muted); }

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

@media (max-width: 1024px) {
  .visits-layout { grid-template-columns: 1fr; }
  .form-card { position: static; }
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
    margin-bottom: 1.5rem;
  }

  .page-header h1 {
    font-size: 1.75rem;
  }

  .visit-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.75rem;
  }
  
  .visit-time {
    width: auto;
    font-size: 0.75rem;
  }

  .visit-footer {
    flex-direction: row;
    justify-content: space-between;
    font-size: 0.9rem;
  }
}

  .form-card {
    padding: 1.5rem;
  }

  .modal {
    padding: 2rem 1.5rem;
  }


.quick-filters {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
  overflow-x: auto;
  padding-bottom: 0.5rem;
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none; /* Hide scrollbar */
}

.quick-filters::-webkit-scrollbar {
  display: none;
}

.q-btn {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  border: 1px solid #f1f5f9;
  background: #ffffff;
  color: var(--text-muted);
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  transition: var(--transition);
}

.q-btn:hover {
  background: #f8fafc;
  border-color: var(--primary-light);
}

.q-btn.active {
  background: var(--primary);
  color: #ffffff;
  border-color: var(--primary);
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.2);
}
.visit-card {
  cursor: pointer;
}

/* MODAL STYLES (MATCHING DASHBOARD) */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(8px);
  z-index: 2000;
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 1rem;
}

.modern-modal {
  background: #ffffff;
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  border-radius: 24px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
}

.modal-header {
  padding: 1.5rem 2rem;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-header h2 { font-size: 1.25rem; font-weight: 800; display: flex; flex-direction: column; }
.modal-header .pet-name { color: var(--primary); }
.modal-header .owner-name { font-size: 0.85rem; color: #64748b; }

.modal-close-btn {
  background: #f1f5f9;
  border: none;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  cursor: pointer;
}

.modal-body { padding: 2rem; overflow-y: auto; }

.section-header-row { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
.section-subtitle { font-weight: 800; color: #1e293b; }

.detail-item { display: flex; flex-direction: column; gap: 0.4rem; margin-bottom: 1rem; }
.detail-item label { font-size: 0.75rem; font-weight: 700; color: #94a3b8; text-transform: uppercase; }
.detail-item .val { font-weight: 600; color: #334155; }
.detail-item .val.highlight { color: var(--primary); }

.modern-input {
  width: 100%;
  padding: 0.75rem;
  border-radius: 10px;
  border: 1px solid #e2e8f0;
  background: #f8fafc;
  font-family: inherit;
}

.modern-input:focus { border-color: var(--primary); outline: none; background: #fff; }

.btn-primary-sm { background: var(--primary); color: #fff; border: none; padding: 0.5rem 1rem; border-radius: 8px; font-weight: 700; cursor: pointer; }
.btn-text { background: transparent; border: none; color: #64748b; font-weight: 600; cursor: pointer; }
.btn-xs { padding: 0.4rem 0.8rem; font-size: 0.8rem; }
</style>
