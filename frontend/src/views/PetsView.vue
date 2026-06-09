<template>
  <main class="page-pets">
    <header class="page-header">
      <div class="header-content">
        <h1>Hastalar</h1>
        <p class="subtitle">Tüm hayvanlar, ziyaret geçmişi ve görselleri buradan yönetin.</p>
      </div>
    </header>

    <div class="pets-view-layout">
      <!-- Sol: Liste -->
      <aside class="sidebar-section">
        <div class="search-card">
          <div class="search-input-wrapper">
            <input v-model="q" class="search-input" placeholder="İsim, tür veya sahip ara..." />
          </div>
          <select v-model="ownerId" class="owner-select">
            <option value="">Tüm Sahipler</option>
            <option v-for="o in owners" :key="o.id" :value="String(o.id)">
              {{ o.fullName }}
            </option>
          </select>
        </div>

        <div v-if="loadingList" class="state">Yükleniyor...</div>
        <div v-else class="pet-list">
          <button
            v-for="p in filteredPets"
            :key="p.id"
            class="pet-row-card"
            :class="{ active: selectedPetId === p.id }"
            @click="openPet(p.id)"
          >
            <div class="pet-avatar">{{ (p.name || '?')[0].toUpperCase() }}</div>
            <div class="pet-info">
              <span class="name">{{ p.name }}</span>
              <span class="meta">{{ p.species }} • {{ p.ownerName }}</span>
            </div>
          </button>
        </div>
      </aside>

      <!-- Sağ: Profil -->
      <section class="profile-section">
        <div v-if="!profile && !loadingProfile" class="empty-state-card">
          <div class="empty-icon"></div>
          <h3>Hasta Seçilmedi</h3>
          <p>Detayları görüntülemek için soldaki listeden bir hasta seçin.</p>
        </div>

        <div v-if="loadingProfile" class="state">Profil yükleniyor...</div>

        <div v-if="profile && !loadingProfile" class="profile-container">
          <div class="profile-header-card">
            <div class="profile-title-area">
              <h2 class="profile-name">{{ profile.name }}</h2>
              <p class="profile-subtitle">
                {{ profile.species }} <span v-if="profile.breed">• {{ profile.breed }}</span>
              </p>
              <p class="profile-owner">Sahibi: <strong>{{ profile.ownerName }}</strong></p>
            </div>
            <div class="profile-actions">
              <button v-if="!petEditOpen" class="btn btn-secondary" @click="openPetEdit">
                Düzenle
              </button>
              <button v-if="!petEditOpen" class="btn btn-danger-sm" @click="handleDeletePet" style="margin-left: 0.5rem;">
                Sil
              </button>
              <template v-else>
                <button class="close" @click.stop="closeDetail">Kapat</button>
                <button class="btn btn-ghost" @click="cancelPetEdit" :disabled="petSaving">İptal</button>
                <button class="btn btn-primary" @click="savePetEdit" :disabled="petSaving">
                  {{ petSaving ? 'Kaydediliyor...' : 'Kaydet' }}
                </button>
              </template>
            </div>
          </div>

          <div v-if="petSaveError" class="state state-error">{{ petSaveError }}</div>

          <div class="info-grid">
            <div class="info-card">
              <span class="label">Doğum Tarihi</span>
              <div v-if="!petEditOpen" class="value">{{ profile.birthDate || '—' }}</div>
              <input v-else-if="petDraft" type="date" v-model="petDraft.birthDate" class="edit-input" />
            </div>
            <div class="info-card">
              <span class="label">Yaş</span>
              <div class="value">{{ formatAge(profile.ageYears ?? profile.AgeYears, profile.ageMonths ?? profile.AgeMonths) }}</div>
            </div>
            <div class="info-card">
              <span class="label">Mikroçip No</span>
              <div v-if="!petEditOpen" class="value">{{ profile.microchipNumber || '—' }}</div>
              <input v-else-if="petDraft" type="text" v-model="petDraft.microchipNumber" class="edit-input" placeholder="Mikroçip no" />
            </div>
            <div class="info-card wide">
              <span class="label">Notlar</span>
              <div v-if="!petEditOpen" class="value">{{ profile.notes || 'Not eklenmemiş.' }}</div>
              <textarea v-else-if="petDraft" v-model="petDraft.notes" rows="3" class="edit-input" placeholder="Not ekleyin..."></textarea>
            </div>
          </div>

          <h3 class="section-title">Ziyaret Geçmişi</h3>

          <div v-if="!(profile.visits?.length || profile.Visits?.length)" class="empty-visits">
            <p>Bu hasta için henüz ziyaret kaydı bulunmuyor.</p>
          </div>

          <div v-else class="visit-timeline">
            <div v-for="v in (profile.visits || profile.Visits)" :key="v.id || v.visitId || v.VisitId" class="modern-visit-card">
              <div class="visit-header">
                <div class="visit-meta">
                  <span class="visit-date">{{ formatDt(v.performedAt || v.PerformedAt) }}</span>
                  <span class="visit-tag">{{ v.purpose || v.Purpose || 'Genel Muayene' }}</span>
                </div>
                <div class="visit-finances-summary">
                  <div class="fin-pill amount">
                    <span class="label">Tutar:</span>
                    <span class="value">{{ fmtMoney(v.amountTl || v.AmountTl) }}</span>
                  </div>
                  <div class="fin-pill success">
                    <span class="label">Nakit:</span>
                    <span class="value" style="color: var(--success); font-weight: bold;">{{ fmtMoney((v.collectedAmountTl ?? v.CollectedAmountTl) ?? ((v.amountTl || v.AmountTl) - (v.creditAmountTl || v.CreditAmountTl || 0))) }}</span>
                  </div>
                  <div class="fin-pill credit" v-if="(v.creditAmountTl || v.CreditAmountTl) > 0">
                    <span class="label">Veresiye:</span>
                    <span class="value">{{ fmtMoney(v.creditAmountTl || v.CreditAmountTl) }}</span>
                  </div>
                </div>
              </div>

              <div class="visit-content">
                <!-- DÜZENLEME MODU (ZİYARET) -->
                <div v-if="visitEditId === (v.visitId || v.VisitId || v.id)" class="visit-edit-box" style="display: flex; flex-direction: column; gap: 1rem; margin-top: 1rem; background: #f8fafc; padding: 1.5rem; border-radius: 12px; border: 1px solid var(--primary-light);">
                  <div class="edit-grid" style="display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem;">
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b;">PET ADI</label>
                      <input v-model="visitDraft.petName" class="edit-input" />
                    </div>
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b;">PET TÜRÜ</label>
                      <input v-model="visitDraft.petSpecies" class="edit-input" />
                    </div>
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b;">HASTA SAHİBİ</label>
                      <input v-model="visitDraft.ownerName" class="edit-input" />
                    </div>
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b;">SAHİP TELEFON</label>
                      <input v-model="visitDraft.ownerPhone" class="edit-input" />
                    </div>
                  </div>

                  <div class="field">
                    <label style="font-size: 0.7rem; font-weight: 700; color: #64748b;">İŞLEM TARİHİ</label>
                    <input type="datetime-local" v-model="visitDraft.performedAt" class="edit-input" />
                  </div>
                  
                  <div class="field">
                    <label style="font-size: 0.7rem; font-weight: 700; color: #64748b;">UYGULANAN İŞLEMLER</label>
                    <div class="procedure-pills-container">
                      <button
                        v-for="pill in predefinedProcedures"
                        :key="pill"
                        type="button"
                        class="pill-select-btn"
                        :class="{ active: isProcedureSelected(pill, visitDraft.procedures) }"
                        @click="toggleProcedure(pill, visitDraft, 'procedures')"
                      >
                        {{ pill }}
                      </button>
                    </div>
                    <textarea v-model="visitDraft.procedures" class="edit-input" rows="3"></textarea>
                  </div>

                  <div style="display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 0.75rem;">
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b;">TUTAR (TL)</label>
                      <input type="number" v-model.number="visitDraft.amountTl" @input="onEditAmountInput" class="edit-input" />
                    </div>
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b;">NAKİT (TL)</label>
                      <input type="number" v-model.number="visitDraft.collectedAmountTl" @input="onEditCollectedInput" class="edit-input" />
                    </div>
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b;">VERESİYE (TL)</label>
                      <input type="number" v-model.number="visitDraft.creditAmountTl" @input="onEditCreditInput" class="edit-input" />
                    </div>
                  </div>

                  <div class="field">
                    <label style="font-size: 0.7rem; font-weight: 700; color: #64748b;">NOTLAR</label>
                    <textarea v-model="visitDraft.notes" class="edit-input" rows="2"></textarea>
                  </div>

                  <div class="edit-actions" style="display: flex; gap: 0.5rem; justify-content: flex-end;">
                    <button class="btn btn-ghost" @click="cancelVisitEdit">İptal</button>
                    <button class="btn btn-primary" @click="saveVisitEdit(v)" :disabled="visitSaving">
                      {{ visitSaving ? '...' : 'Kaydet' }}
                    </button>
                    <button class="btn btn-danger-sm" @click="handleDeleteVisit(v)" :disabled="visitSaving">Sil</button>
                  </div>
                </div>

                <template v-else>
                  <div class="procedure-block">
                    <label>Uygulanan İşlemler</label>
                    <p>{{ v.procedures || v.Procedures || 'İşlem kaydı girilmemiş.' }}</p>
                  </div>
                  
                  <div v-if="v.notes || v.Notes" class="notes-block">
                    <label>Hekim Notu</label>
                    <p>{{ v.notes || v.Notes }}</p>
                  </div>

                  <div style="margin-top: 1rem;">
                    <button class="btn btn-secondary btn-sm" @click="openVisitEdit(v)">Düzenle</button>
                  </div>
                </template>

                <div v-if="getVisitImages(v).length" class="visit-gallery">
                  <div class="gallery-grid">
                    <div v-for="(img, idx) in getVisitImages(v)" :key="idx" class="gallery-item">
                      <img :src="normalizeMediaUrl(getImageUrl(img))" alt="Visit Image" />
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  </main>
</template>

<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { http, API_BASE } from '@/api/http'
import { fetchOwners } from '@/api/owners'

const route = useRoute()
const pets = ref([])
const owners = ref([])
const loadingList = ref(false)
const loadingProfile = ref(false)
const error = ref('')
const visitEditId = ref(null)
const visitDraft = ref(null)
const visitSaving = ref(false)
const visitSaveError = ref('')
const selectedPetId = ref(null)
const profile = ref(null)
const petEditOpen = ref(false)
const petDraft = ref(null)
const petSaving = ref(false)
const petSaveError = ref('')
const q = ref('')
const ownerId = ref('')

const filteredPets = computed(() => {
  const term = q.value.trim().toLowerCase()
  return pets.value.filter(p => {
    const ownerOk = !ownerId.value || String(p.ownerId) === ownerId.value
    if (!ownerOk) return false
    if (!term) return true
    return (
      (p.name || '').toLowerCase().includes(term) ||
      (p.species || '').toLowerCase().includes(term) ||
      (p.ownerName || '').toLowerCase().includes(term)
    )
  })
})

onMounted(async () => {
  await loadList()
  
  // URL'den gelen ID varsa aç
  if (route.query.id) {
    openPet(Number(route.query.id))
  }
})

// Query param değişirse (örneğin aynı sayfadayken başka pete geçilirse)
watch(() => route.query.id, (newId) => {
  if (newId) {
    openPet(Number(newId))
  }
})

function normalizeMediaUrl(rawUrl) {
  if (!rawUrl) return ''
  if (rawUrl.startsWith('http')) return rawUrl
  const base = API_BASE.endsWith('/') ? API_BASE.slice(0, -1) : API_BASE
  const path = rawUrl.startsWith('/') ? rawUrl : `/${rawUrl}`
  return `${base}${path}`
}

function toVisitDraft(v) {
  if (!v) return null
  const perf = v.performedAt || v.PerformedAt || ''
  const amount = v.amountTl ?? v.AmountTl ?? null
  const credit = v.creditAmountTl ?? v.CreditAmountTl ?? null
  const collected = v.collectedAmountTl ?? v.CollectedAmountTl ?? null
  return {
    performedAt: perf ? new Date(perf).toISOString().slice(0, 16) : '',
    purpose: v.purpose || v.Purpose || '',
    procedures: v.procedures || v.Procedures || '',
    amountTl: amount,
    notes: v.notes || v.Notes || '',
    creditAmountTl: Number(credit ?? 0),
    collectedAmountTl: collected ?? (amount !== null ? Math.max(0, amount - (credit || 0)) : null),

    // Pet & Owner Info
    petName: v.petName ?? v.PetName ?? '',
    petSpecies: v.species ?? v.Species ?? v.petSpecies ?? v.PetSpecies ?? '',
    ownerName: v.ownerName ?? v.OwnerName ?? '',
    ownerPhone: v.phoneE164 ?? v.PhoneE164 ?? v.ownerPhone ?? v.OwnerPhone ?? '',
  }
}

function openVisitEdit(v) {
  visitSaveError.value = ''
  visitEditId.value = v.visitId || v.VisitId
  visitDraft.value = toVisitDraft(v)
}

function cancelVisitEdit() {
  visitEditId.value = null
  visitDraft.value = null
  visitSaveError.value = ''
}

function onEditAmountInput() {
  const total = visitDraft.value.amountTl || 0
  const credit = visitDraft.value.creditAmountTl || 0
  visitDraft.value.collectedAmountTl = Math.max(0, total - credit)
}

function onEditCreditInput() {
  const total = visitDraft.value.amountTl || 0
  const credit = visitDraft.value.creditAmountTl || 0
  if (credit > total) {
    visitDraft.value.amountTl = credit
  }
  visitDraft.value.collectedAmountTl = Math.max(0, (visitDraft.value.amountTl || 0) - credit)
}

function onEditCollectedInput() {
  const total = visitDraft.value.amountTl || 0
  const collected = visitDraft.value.collectedAmountTl || 0
  if (collected > total) {
    visitDraft.value.amountTl = collected
  }
  visitDraft.value.creditAmountTl = Math.max(0, (visitDraft.value.amountTl || 0) - collected)
}

async function saveVisitEdit(v) {
  const visitId = v?.visitId || v?.VisitId || v?.id
  if (!visitId || !visitDraft.value) return
  visitSaving.value = true
  try {
    // 1) Update Pet if changed
    const petId = profile.value.id
    if (petId) {
      await http.put(`/pets/${petId}`, {
        name: visitDraft.value.petName,
        species: visitDraft.value.petSpecies,
        breed: profile.value.breed,
        birthDate: profile.value.birthDate,
        notes: profile.value.notes || ''
      })
    }

    // 2) Update Owner if changed
    const ownerId = profile.value.ownerId
    if (ownerId) {
      await http.put(`/owners/${ownerId}`, {
        fullName: visitDraft.value.ownerName,
        phoneE164: visitDraft.value.ownerPhone,
        kvkkOptIn: true
      })
    }

    // 3) Update Visit
    const payload = {
      performedAt: new Date(visitDraft.value.performedAt).toISOString(),
      procedures: (visitDraft.value.procedures || '').trim() || null,
      amountTl: visitDraft.value.amountTl,
      notes: (visitDraft.value.notes || '').trim() || null,
      purpose: (visitDraft.value.purpose || '').trim() || null,
      nextDate: v.nextDate || v.NextDate || null,
      creditAmountTl: visitDraft.value.creditAmountTl,
      collectedAmountTl: visitDraft.value.collectedAmountTl
    }
    await http.put(`/visits/${visitId}`, payload)
    
    // Refresh
    await openPet(selectedPetId.value)
    cancelVisitEdit()
  } catch (e) {
    console.error(e)
    visitSaveError.value = 'Güncellenemedi.'
  } finally {
    visitSaving.value = false
  }
}

function getVisitImages(v) {
  return v?.images || v?.Images || []
}

function getImageUrl(img) {
  return img?.url || img?.imageUrl || ''
}

async function loadList() {
  loadingList.value = true
  try {
    const [ownersData, petsRes] = await Promise.all([
      fetchOwners(),
      http.get('/pets'),
    ])
    owners.value = ownersData
    pets.value = petsRes.data || []
  } catch (e) {
    error.value = 'Hata oluştu.'
  } finally {
    loadingList.value = false
  }
}

async function openPet(id) {
  selectedPetId.value = id
  petEditOpen.value = false
  loadingProfile.value = true
  try {
    const res = await http.get(`/pets/${id}/profile`)
    console.log('--- PET PROFILE DATA ---', res.data)
    console.log('Visits property:', res.data.visits || res.data.Visits)
    profile.value = res.data
  } catch (e) {
    error.value = 'Profil yüklenemedi.'
  } finally {
    loadingProfile.value = false
  }
}

function formatDt(iso) {
  return iso ? new Date(iso).toLocaleString('tr-TR') : '—'
}

function formatAge(y, m) {
  if (y == null && m == null) return '—'
  if (y > 0) return `${y} yıl ${m || 0} ay`
  return `${m || 0} ay`
}

function fmtMoney(val) {
  return `${Number(val || 0).toFixed(2)}₺`
}

function openPetEdit() {
  if (!profile.value) return
  petDraft.value = {
    name: profile.value.name,
    species: profile.value.species,
    breed: profile.value.breed,
    birthDate: profile.value.birthDate?.slice(0, 10) || null,
    microchipNumber: profile.value.microchipNumber || '',
    notes: profile.value.notes,
  }
  petEditOpen.value = true
}

function cancelPetEdit() {
  petEditOpen.value = false
  petDraft.value = null
}

async function savePetEdit() {
  if (!profile.value || !petDraft.value) return
  petSaving.value = true
  try {
    await http.put(`/pets/${selectedPetId.value}`, petDraft.value)
    const res = await http.get(`/pets/${selectedPetId.value}/profile`)
    profile.value = res.data
    petEditOpen.value = false
  } catch (e) {
    petSaveError.value = 'Güncellenemedi.'
  } finally {
    petSaving.value = false
  }
}

async function handleDeletePet() {
  if (!selectedPetId.value) return
  if (!confirm('Bu hastayı silmek istediğinize emin misiniz?')) return
  
  try {
    await http.delete(`/pets/${selectedPetId.value}`)
    profile.value = null
    selectedPetId.value = null
    await loadList()
  } catch (err) {
    console.error(err)
    alert('Hasta silinirken hata oluştu. (Ziyaret kaydı olan hastaları önce silmeniz gerekebilir.)')
  }
}

async function handleDeleteVisit(v) {
  const visitId = v.visitId || v.VisitId || v.id
  if (!visitId) return
  if (!confirm('Bu ziyareti silmek istediğinize emin misiniz?')) return
  
  visitSaving.value = true
  try {
    await http.delete(`/visits/${visitId}`)
    await openPet(selectedPetId.value)
  } catch (err) {
    console.error(err)
    alert('Ziyaret silinemedi.')
  } finally {
    visitSaving.value = false
  }
}

const predefinedProcedures = [
  'İlaç A',
  'İlaç B',
  'İlaç C',
  'Kedi karma',
  'Köpek karma',
  'Corona',
  'Lösemi',
  'Kuduz',
  'Mantar',
  'Lyme',
  'İç dış parazit',
  'Genel Muayene',
  'Cerrahi Operasyon',
  'Laboratuvar Tahlili'
]

function isProcedureSelected(pill, currentStr) {
  const str = currentStr || ''
  const items = str.split(',').map(i => i.trim().toLowerCase()).filter(Boolean)
  return items.includes(pill.toLowerCase())
}

function toggleProcedure(pill, targetObj, key) {
  let currentVal = targetObj[key] || ''
  let items = currentVal.split(',').map(i => i.trim()).filter(Boolean)
  const idx = items.findIndex(i => i.toLowerCase() === pill.toLowerCase())
  if (idx > -1) {
    items.splice(idx, 1)
  } else {
    items.push(pill)
  }
  targetObj[key] = items.join(', ')
}
</script>

<style scoped>
.page-pets {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}

.pets-view-layout {
  display: grid;
  grid-template-columns: 350px 1fr;
  gap: 2.5rem;
  align-items: start;
}

@media (max-width: 1024px) {
  .pets-view-layout {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }
}

/* SIDEBAR */
.sidebar-section {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.search-card {
  background: #ffffff;
  padding: 1.5rem;
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-sm);
  border: 1px solid #f1f5f9;
}

.search-input-wrapper {
  position: relative;
  margin-bottom: 1rem;
}

.search-input-wrapper .icon {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--text-muted);
}

.search-input {
  width: 100%;
  padding: 0.8rem 1rem 0.8rem 2.5rem;
  border-radius: 12px;
  border: 1px solid #f1f5f9;
  background: #f8fafc;
  font-size: 0.95rem;
  transition: var(--transition);
}

.owner-select {
  width: 100%;
  padding: 0.8rem;
  border-radius: 12px;
  border: 1px solid #f1f5f9;
  background: #f8fafc;
  font-size: 0.9rem;
}

@media (max-width: 768px) {
  .search-card {
    padding: 1rem;
    margin-bottom: 0.5rem;
  }
  
  .pet-list {
    flex-direction: row;
    overflow-x: auto;
    padding-bottom: 0.5rem;
    gap: 0.5rem;
  }

  .pet-row-card {
    min-width: 160px;
    padding: 0.75rem;
    flex-shrink: 0;
  }

  .pet-avatar {
    width: 32px;
    height: 32px;
    font-size: 0.9rem;
  }
}

.pet-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.pet-row-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1.25rem;
  background: #ffffff;
  border: 1px solid #f1f5f9;
  border-radius: var(--radius-lg);
  cursor: pointer;
  transition: var(--transition);
  text-align: left;
  width: 100%;
}

.pet-row-card:hover {
  transform: translateX(5px);
  background: #f8fafc;
}

.pet-row-card.active {
  border-color: var(--primary);
  background: var(--primary-light);
  box-shadow: var(--shadow-sm);
}

.pet-avatar {
  width: 44px;
  height: 44px;
  background: var(--primary);
  color: white;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 1.2rem;
}

.pet-info .name {
  display: block;
  font-weight: 700;
  color: var(--text-main);
}

.pet-info .meta {
  font-size: 0.85rem;
  color: var(--text-muted);
}

/* PROFILE SECTION */
.profile-section {
  min-height: 500px;
}

.profile-header-card {
  background: #ffffff;
  padding: 2.5rem;
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-sm);
  border: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.profile-name {
  font-size: 2rem;
  font-weight: 800;
  letter-spacing: -0.04em;
  margin-bottom: 0.25rem;
}

.profile-subtitle {
  font-size: 1.1rem;
  color: var(--primary);
  font-weight: 700;
  margin-bottom: 0.5rem;
}

.profile-owner {
  color: var(--text-muted);
  font-size: 0.95rem;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.5rem;
  margin-bottom: 2.5rem;
}

@media (max-width: 768px) {
  .profile-header-card {
    padding: 1.5rem;
    flex-direction: column;
    text-align: center;
    gap: 1.5rem;
  }

  .info-grid {
    grid-template-columns: 1fr;
    gap: 0.75rem;
  }

  .info-card.wide {
    grid-column: span 1;
  }
}

.info-card {
  background: #ffffff;
  padding: 1.5rem;
  border-radius: var(--radius-lg);
  border: 1px solid #f1f5f9;
}

.info-card.wide { grid-column: span 2; }

.info-card .label {
  display: block;
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 0.5rem;
}

.info-card .value {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--text-main);
}

/* VISIT TIMELINE */
.section-title {
  font-size: 1.5rem;
  font-weight: 800;
  margin-bottom: 1.5rem;
}

.visit-timeline {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.modern-visit-card {
  background: #ffffff;
  padding: 2rem;
  border-radius: var(--radius-xl);
  border: 1px solid #f1f5f9;
  box-shadow: var(--shadow-sm);
  position: relative;
}

.modern-visit-card::before {
  content: '';
  position: absolute;
  left: 0;
  top: 1.5rem;
  bottom: 1.5rem;
  width: 5px;
  background: var(--primary);
  border-radius: 0 10px 10px 0;
}

.visit-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1.5rem;
}

.visit-date {
  display: block;
  font-weight: 800;
  font-size: 1.1rem;
  color: var(--text-main);
}

.visit-purpose {
  font-size: 0.9rem;
  color: var(--primary);
  font-weight: 700;
}

.procedure-text {
  background: #f8fafc;
  padding: 1.5rem;
  border-radius: 16px;
  margin-bottom: 1.5rem;
  font-size: 0.95rem;
  line-height: 1.6;
}

.visit-finances {
  display: flex;
  gap: 2rem;
  margin-bottom: 1.5rem;
}

.finance-item .label {
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--text-muted);
  display: block;
  margin-bottom: 0.25rem;
}

.finance-item .value {
  font-size: 1.1rem;
  font-weight: 800;
  color: var(--success);
}

.finance-item .value.danger { color: var(--danger); }

.visit-gallery {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.gallery-item img {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 12px;
  border: 1px solid #f1f5f9;
}

/* BUTTONS & INPUTS */
.btn {
  padding: 0.75rem 1.25rem;
  border-radius: 12px;
  font-weight: 700;
  cursor: pointer;
  transition: var(--transition);
  border: none;
}

.btn-primary { background: var(--primary); color: white; }
.btn-secondary { background: var(--primary-light); color: var(--primary); }
.btn-ghost { background: transparent; color: var(--text-muted); }

.edit-input {
  width: 100%;
  padding: 0.75rem;
  border-radius: 10px;
  border: 1px solid var(--primary-light);
  background: #f8fafc;
  font-family: inherit;
}

.edit-input.tiny { width: 120px; }

.empty-state-card {
  background: #ffffff;
  padding: 4rem;
  border-radius: var(--radius-xl);
  text-align: center;
  border: 1px dashed #e2e8f0;
}

.empty-icon { font-size: 3rem; margin-bottom: 1rem; }

@media (max-width: 1024px) {
  .pets-view-layout { grid-template-columns: 1fr; }
}

@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    align-items: flex-start;
    gap: 1.5rem;
  }

  .pet-main {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .pet-avatar {
    width: 80px;
    height: 80px;
    font-size: 1.75rem;
  }

  .pet-title h1 {
    font-size: 2rem;
  }

  .quick-stats {
    flex-direction: column;
    gap: 0.75rem;
    width: 100%;
  }

  .stat-pill {
    width: 100%;
    justify-content: center;
  }

  .profile-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }

  .visit-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .visit-actions {
    width: 100%;
    justify-content: space-between;
  }

  .btn {
    flex: 1;
    text-align: center;
    padding: 0.75rem 0.5rem;
    font-size: 0.85rem;
  }

  .finance-grid {
    grid-template-columns: 1fr;
    gap: 0.75rem;
  }

  .empty-state-card {
    padding: 2rem 1rem;
  }
}
.modern-visit-card {
  background: #ffffff;
  border-radius: 16px;
  border: 1px solid #f1f5f9;
  padding: 1.5rem;
  margin-bottom: 1.5rem;
  transition: var(--transition);
  box-shadow: var(--shadow-sm);
}

.visit-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
  padding-bottom: 0.75rem;
  border-bottom: 1px solid #f8fafc;
}

.visit-meta {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.visit-date {
  font-weight: 800;
  color: var(--text-main);
  font-size: 1.1rem;
}

.visit-purpose {
  font-size: 0.9rem;
  color: var(--primary);
  font-weight: 700;
  text-transform: uppercase;
}

.visit-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.procedure-text {
  color: var(--text-main);
  line-height: 1.6;
  font-size: 1rem;
}

.visit-notes-area {
  background: #fffbeb;
  padding: 1rem;
  border-radius: 12px;
  border-left: 4px solid #fbbf24;
  font-size: 0.95rem;
}

.visit-finances {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-top: 0.5rem;
}

.finance-item {
  background: #f8fafc;
  padding: 1rem;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.finance-item .label {
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
}

.finance-item .value {
  font-size: 1.25rem;
  font-weight: 800;
  font-family: 'Outfit', sans-serif;
}

.value.success { color: #10b981; }
.value.danger { color: #ef4444; }

.edit-group {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.currency {
  font-weight: 700;
  color: var(--text-muted);
}

.gallery-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
  gap: 0.75rem;
  margin-top: 0.5rem;
}

.gallery-item {
  aspect-ratio: 1;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #f1f5f9;
  cursor: zoom-in;
  transition: var(--transition);
}

.gallery-item:hover {
  transform: scale(1.05);
  box-shadow: var(--shadow-md);
}

.gallery-item img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.edit-input {
  width: 100%;
  padding: 0.75rem;
  border-radius: 8px;
  border: 1px solid var(--primary);
  background: #fff;
}

.tiny { padding: 0.4rem; font-size: 0.9rem; }

@media (max-width: 768px) {
  .pets-view-layout { grid-template-columns: 1fr; }
  .visit-finances { grid-template-columns: 1fr; }
}

.procedure-pills-container {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
  margin-top: 0.25rem;
}

.pill-select-btn {
  padding: 0.4rem 0.8rem;
  border-radius: 20px;
  border: 1px solid #e2e8f0;
  background: #ffffff;
  color: #64748b;
  font-size: 0.8rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.02);
}

.pill-select-btn:hover {
  background: #f8fafc;
  color: var(--primary);
  border-color: var(--primary-light);
  transform: translateY(-1px);
}

.pill-select-btn.active {
  background: var(--primary);
  color: #ffffff;
  border-color: var(--primary);
  box-shadow: 0 4px 6px -1px rgba(79, 70, 229, 0.2);
}
</style>

