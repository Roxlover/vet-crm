
<template>
  <div class="page">
    <header class="page-header">
      <h1>Hastalar</h1>
      <p class="subtitle">Tüm hayvanlar, ziyaret geçmişi ve görseller.</p>
    </header>

    <div class="layout">
      <!-- Sol: Liste -->
      <section class="card">
        <div class="toolbar">
          <input v-model="q" class="input" placeholder="Ara: isim / tür / sahip" />
          <select v-model="ownerId" class="input">
            <option value="">Tüm sahipler</option>
            <option v-for="o in owners" :key="o.id" :value="String(o.id)">
              {{ o.fullName }}
            </option>
          </select>
        </div>

        <div v-if="loadingList" class="muted">Yükleniyor...</div>
        <div v-else>
          <button
            v-for="p in filteredPets"
            :key="p.id"
            class="pet-row"
            :class="{ active: selectedPetId === p.id }"
            @click="openPet(p.id)"
            type="button"
          >
            <div class="pet-name">{{ p.name }}</div>
            <div class="pet-meta">
              <span>{{ p.species }}</span>
              <span class="dot">•</span>
              <span>{{ p.ownerName }}</span>
            </div>
          </button>
        </div>
      </section>

      <!-- Sağ: Profil -->
      <section class="card">
        <div v-if="!profile && !loadingProfile" class="muted">
          Soldan bir hasta seç.
        </div>

        <div v-if="loadingProfile" class="muted">Detay yükleniyor...</div>

        <div v-if="profile && !loadingProfile">
        <div class="profile-head">
  <div>
    <h2 class="h2">{{ profile.name }}</h2>
    <div class="muted">
      {{ profile.species }}<span v-if="profile.breed"> – {{ profile.breed }}</span>
    </div>
    <div class="muted">
      Sahibi: <strong>{{ profile.ownerName }}</strong>
    </div>
  </div>

  <!-- SAĞ ÜST: PET EDIT AKSİYONLARI -->
  <div class="profile-actions">
    <button
      v-if="!petEditOpen"
      class="btn btn-sm"
      type="button"
      @click="openPetEdit"
    >
      Düzenle
    </button>

    <template v-else>
      <button
        class="btn btn-sm"
        type="button"
        @click="cancelPetEdit"
        :disabled="petSaving"
      >
        İptal
      </button>

      <button
        class="btn btn-sm"
        type="button"
        @click="savePetEdit"
        :disabled="petSaving"
      >
        {{ petSaving ? 'Kaydediliyor...' : 'Kaydet' }}
      </button>
    </template>
  </div>
</div>

<p v-if="petSaveError" class="state state-error">{{ petSaveError }}</p>

<div v-if="petEditOpen && !petDraft" class="muted">
  Düzenleme hazırlanıyor...
</div>

          <div class="grid2">
            <div class="info-box">
  <div class="label">Doğum</div>

  <div v-if="!petEditOpen">
    {{ profile.birthDate || '—' }}
  </div>

  <input
    v-else-if="petDraft"
    type="date"
    class="input"
    v-model="petDraft.birthDate"
  />
</div>

            <div class="info-box">
  <div class="label">Not</div>

  <div v-if="!petEditOpen">
    {{ profile.notes || '—' }}
  </div>

  <textarea
    v-else-if="petDraft"
    class="input"
    rows="3"
    v-model="petDraft.notes"
    placeholder="Örn: idrar kesesinde taş..."
  ></textarea>
</div>

            <div class="info-box">
             <div class="label">Yaş</div>
             <div>{{ formatAge(profile.ageYears, profile.ageMonths) }}</div>
            </div>

          </div>

          <h3 class="h3">Ziyaret Geçmişi</h3>

          <div v-if="!profile.visits?.length" class="muted">Ziyaret yok.</div>

          <div v-for="v in profile.visits" :key="v.visitId" class="visit-card">
            <div class="visit-top">
              <div>
<div class="visit-date">
  <span v-if="visitEditId !== v.visitId">{{ formatDt(v.performedAt) }}</span>

  <input
    v-else-if="visitDraft"
    class="input"
    type="datetime-local"
    v-model="visitDraft.performedAt"
  />
</div>
<div class="muted">
  <span class="label">Amaç:</span>
  <span v-if="visitEditId !== v.visitId">{{ v.purpose || '—' }}</span>

  <input
    v-else-if="visitDraft"
    class="input"
    type="text"
    v-model="visitDraft.purpose"
    placeholder="Örn: kontrol, karma aşı..."
  />
</div>

                <div class="muted" v-if="v.createdByName || v.createdByUsername">
                  Kaydı ekleyen: {{ v.createdByName || v.createdByUsername }}
                </div>
              </div>
<div class="visit-actions">
  <button
    v-if="visitEditId !== v.visitId"
    class="btn btn-sm"
    type="button"
    @click="openVisitEdit(v)"
  >
    Düzenle
  </button>

  <template v-else>
    <button
      class="btn btn-sm"
      type="button"
      @click="cancelVisitEdit"
      :disabled="visitSaving"
    >
      İptal
    </button>

    <button
      class="btn btn-sm"
      type="button"
      @click="saveVisitEdit(v)"
      :disabled="visitSaving"
    >
      {{ visitSaving ? 'Kaydediliyor...' : 'Kaydet' }}
    </button>
  </template>
</div>
<div class="money">
  <div>
    <span class="label">Tutar</span>

    <span v-if="visitEditId !== v.visitId">{{ fmtMoney(v.amountTl) }}</span>

    <input
      v-else-if="visitDraft"
      class="input"
      type="number"
      min="0"
      step="0.01"
      v-model.number="visitDraft.amountTl"
      placeholder="0"
    />
  </div>
<div>
  <span class="label">Veresiye</span>

  <span v-if="visitEditId !== v.visitId">
    {{ fmtMoney(v.creditAmountTl) }}
  </span>

  <input
    v-else-if="visitDraft"
    class="input"
    type="number"
    min="0"
    step="0.01"
    v-model.number="visitDraft.creditAmountTl"
    placeholder="0"
  />
</div>

</div>

            </div>
<!-- Procedures -->
<div class="muted pre">
  <div v-if="visitEditId !== v.visitId">
    {{ v.procedures || '—' }}
  </div>

  <textarea
    v-else-if="visitDraft"
    class="input"
    rows="3"
    v-model="visitDraft.procedures"
    placeholder="Örn: Karma aşı, tırnak kesimi..."
  ></textarea>
</div>

<!-- Notes -->
<div class="muted">
  <span class="label">Not:</span>

  <span v-if="visitEditId !== v.visitId">{{ v.notes || '—' }}</span>

  <textarea
    v-else-if="visitDraft"
    class="input"
    rows="2"
    v-model="visitDraft.notes"
    placeholder="Örn: 1 hafta sonra kontrol..."
  ></textarea>
</div>

           <div v-if="getVisitImages(v).length" class="img-grid">
  <a
    v-for="img in getVisitImages(v)"
    :key="img.id || img.Id || getImageUrl(img)"
    :href="normalizeMediaUrl(getImageUrl(img))"
    target="_blank"
    rel="noreferrer"
    class="img-thumb"
  >
    <img
      :src="normalizeMediaUrl(getImageUrl(img))"
      :alt="`Visit ${v.visitId} - ${img.id || img.Id || ''}`"
      loading="lazy"
    />
  </a>
</div>
<p v-if="visitSaveError && visitEditId === v.visitId" class="state state-error">
  {{ visitSaveError }}
</p>

<div v-if="visitEditId === v.visitId && !visitDraft" class="muted">
  Düzenleme hazırlanıyor...
</div>


          </div>
        </div>

        <p v-if="error" class="state state-error">{{ error }}</p>
      </section>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { http, API_BASE } from '@/api/http'
import { fetchOwners } from '@/api/owners'

const pets = ref([])
const owners = ref([])
const petSearch = ref('')
const showPetResults = ref(false)
const PET_MIN_CHARS = 2
const loadingList = ref(false)
const loadingProfile = ref(false)
const error = ref('')
const visitEditId = ref(null)     // şu an editlenen visitId
const visitDraft = ref(null)      // tek draft (aktif visit için)
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

  const performedAt = v.performedAt
    ? new Date(v.performedAt).toISOString().slice(0, 16)
    : ''

  return {
    performedAt,
    purpose: v.purpose ?? v.Purpose ?? '',
    procedures: v.procedures ?? v.Procedures ?? '',
    amountTl: v.amountTl ?? v.AmountTl ?? null,
    notes: v.notes ?? v.Notes ?? '',
    creditAmountTl: Number(v.creditAmountTl ?? v.CreditAmountTl ?? 0),
  }
}
function openVisitEdit(v) {
  visitSaveError.value = ''
  visitEditId.value = v.visitId
  visitDraft.value = toVisitDraft(v)
}
const selectedPet = computed(() => {
  const id = selectedPetId.value
  if (!id) return null
  return (pets.value || []).find(p => String(p.id ?? p.petId ?? p.PetId) === String(id)) || null
})

const filteredPetsForVisit = computed(() => {
  const term = petSearch.value.trim().toLowerCase()
  if (!showPetResults.value) return []

  if (term.length < PET_MIN_CHARS) return []

  return (pets.value || [])
    .filter(p => {
      const name = (p.name || p.Name || '').toLowerCase()
      const owner = (p.ownerName || p.OwnerName || '').toLowerCase()
      const species = (p.species || p.Species || '').toLowerCase()
      const breed = (p.breed || p.Breed || '').toLowerCase()
      return (
        name.includes(term) ||
        owner.includes(term) ||
        species.includes(term) ||
        breed.includes(term)
      )
    })
    .slice(0, 30)
})

function openPetSearch() {
  showPetResults.value = true
}

function selectPetForVisit(p) {
  const id = p.id ?? p.petId ?? p.PetId
  selectedPetId.value = id
  petSearch.value = ''
  showPetResults.value = false
}

function clearSelectedPet() {
  selectedPetId.value = null
  petSearch.value = ''
  showPetResults.value = true // tekrar aramaya devam etsin diye açık bırakıyorum
}
function cancelVisitEdit() {
  visitEditId.value = null
  visitDraft.value = null
  visitSaveError.value = ''
}
async function saveVisitEdit(v) {
  const visitId = v?.visitId
  if (!visitId) {
    visitSaveError.value = 'VisitId bulunamadı.'
    return
  }
  if (!visitDraft.value) {
    visitSaveError.value = 'Düzenleme verisi hazırlanamadı.'
    return
  }

  if (!visitDraft.value.performedAt) {
    visitSaveError.value = 'Yapılan işlem tarihi zorunludur.'
    return
  }

  visitSaving.value = true
  visitSaveError.value = ''

  try {
    const performedAtIso = new Date(visitDraft.value.performedAt).toISOString()

    const payload = {
      performedAt: performedAtIso,
      procedures: (visitDraft.value.procedures ?? '').trim() || null,
      amountTl: (visitDraft.value.amountTl ?? null),
      notes: (visitDraft.value.notes ?? '').trim() || null,
      purpose: (visitDraft.value.purpose ?? '').trim() || null,

    }

    await http.put(`/visits/${visitId}`, payload)
    const credit = Number(visitDraft.value.creditAmountTl ?? 0)
    if (Number.isNaN(credit) || credit < 0) {
  visitSaveError.value = 'Veresiye 0 veya daha büyük olmalıdır.'
  return
}
    await http.patch(`/visits/${visitId}/credit`, { creditAmountTl: credit })
    const idx = profile.value?.visits?.findIndex(x => x.visitId === visitId)
    if (idx >= 0) {
      const old = profile.value.visits[idx]
      profile.value.visits[idx] = {
        ...old,
        performedAt: performedAtIso,
        purpose: payload.purpose,
        procedures: payload.procedures,
        amountTl: payload.amountTl,
        notes: payload.notes,
        creditAmountTl: credit,
      }
    }


    cancelVisitEdit()
  } catch (e) {
    console.error('[VISIT_EDIT] save error', e)
    const msg = e?.response?.data
    visitSaveError.value =
      typeof msg === 'string'
        ? msg
        : (msg?.message || 'Ziyaret güncelleme başarısız.')
  } finally {
    visitSaving.value = false
  }
}

function getVisitImages(v) {
  return (v?.images || v?.Images || [])
}

function getImageUrl(img) {
  return img?.url || img?.imageUrl || img?.ImageUrl || img?.Url || ''
}

async function loadList() {
  error.value = ''
  loadingList.value = true
  try {
    const [ownersData, petsRes] = await Promise.all([
      fetchOwners(),
      http.get('/pets'),
    ])
    owners.value = ownersData
    pets.value = petsRes.data || []
  } catch (e) {
    console.error(e)
    error.value = 'Pet listesi yüklenemedi.'
  } finally {
    loadingList.value = false
  }
}

async function openPet(id) {
  selectedPetId.value = id
  petEditOpen.value = false
petDraft.value = null
petSaveError.value = ''
  profile.value = null
  error.value = ''
  loadingProfile.value = true
  try {
    const res = await http.get(`/pets/${id}/profile`)
    profile.value = res.data
  } catch (e) {
    console.error(e)
    error.value = 'Pet detayı yüklenirken hata oluştu.'
  } finally {
    loadingProfile.value = false
  }
}

function formatDt(iso) {
  try {
    const d = new Date(iso)
    return d.toLocaleString('tr-TR')
  } catch {
    return iso
  }
}
function toPetDraft(p) {
  if (!p) return null

  const bd = p.birthDate ?? p.BirthDate ?? null
  // Eğer bazen ISO gelirse input date bozulmasın:
  const birthDate = typeof bd === 'string' ? bd.slice(0, 10) : bd

  return {
    name: p.name ?? p.Name ?? '',
    species: p.species ?? p.Species ?? '',
    breed: p.breed ?? p.Breed ?? '',
    birthDate: birthDate || null,
    notes: p.notes ?? p.Notes ?? '',
  }
}

function openPetEdit() {
  if (!profile.value) return
  petSaveError.value = ''
  petDraft.value = toPetDraft(profile.value)
  petEditOpen.value = true
}

function cancelPetEdit() {
  petEditOpen.value = false
  petDraft.value = null
  petSaveError.value = ''
}

async function savePetEdit() {
  if (!profile.value) return

  const petId = profile.value.id ?? profile.value.petId ?? selectedPetId.value
  if (!petId) {
    petSaveError.value = 'PetId bulunamadı.'
    return
  }
  if (!petDraft.value) {
    petSaveError.value = 'Düzenleme verisi hazırlanamadı.'
    return
  }

  petSaving.value = true
  petSaveError.value = ''

  try {
    const payload = {
  name: (petDraft.value.name ?? profile.value.name ?? '').trim(), // zorunlu
  species: (petDraft.value.species ?? '').trim() || null,
  breed: (petDraft.value.breed ?? '').trim() || null,
  birthDate: petDraft.value.birthDate || null,
  notes: (petDraft.value.notes ?? '').trim() || null,
}
    await http.put(`/pets/${petId}`, payload)

    // Güncel profili tekrar çek (ekranın anında güncellensin)
    const res = await http.get(`/pets/${petId}/profile`)
    profile.value = res.data

    petEditOpen.value = false
    petDraft.value = null
  } catch (e) {
    console.error('[PET_EDIT] save error', e)
    const msg = e?.response?.data
    petSaveError.value =
      typeof msg === 'string'
        ? msg
        : (msg?.message || 'Hasta güncelleme başarısız.')
  } finally {
    petSaving.value = false
  }
}

function fmtMoney(val) {
  const n = Number(val ?? 0)
  return `${n.toFixed(2)}₺`
}
function formatAge(y, m) {
  if (y == null && m == null) return '—'
  const yy = Number(y ?? 0)
  const mm = Number(m ?? 0)
  if (yy <= 0 && mm <= 0) return '0 ay'
  if (yy > 0 && mm > 0) return `${yy} yıl ${mm} ay`
  if (yy > 0) return `${yy} yıl`
  return `${mm} ay`
}

</script>

<style scoped>
.page { width: 100%; max-width: 1200px; margin: 0 auto; padding: 1rem; }
.page-header { margin-bottom: 1rem; }
.subtitle { margin: 0.25rem 0 0; font-size: 0.85rem; color: #6b7280; }

.layout { display: grid; grid-template-columns: 360px 1fr; gap: 1rem; }
@media (max-width: 980px) { .layout { grid-template-columns: 1fr; } }

.card { background: #fff; border-radius: 0.75rem; box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06); padding: 1rem; }
.toolbar { display: flex; gap: 0.5rem; margin-bottom: 0.75rem; }
.input { border-radius: 0.5rem; border: 1px solid #d1d5db; padding: 0.45rem 0.6rem; font-size: 0.85rem; width: 100%; }
.profile-actions, .visit-actions { display:flex; gap:.5rem; align-items:center; justify-content:flex-end; }
.visit-actions { margin-left: auto; }

.visit-top { align-items:flex-start; }
.money .input { width: 140px; margin-left: .5rem; }

.pet-row { width: 100%; text-align: left; border: 1px solid #e5e7eb; background: #fff; padding: 0.65rem 0.7rem; border-radius: 0.65rem; margin-bottom: 0.5rem; cursor: pointer; }
.pet-row.active { border-color: #a7f3d0; box-shadow: 0 0 0 2px rgba(34,197,94,0.15); }
.pet-name { font-weight: 700; }
.pet-meta { font-size: 0.8rem; color: #6b7280; display: flex; align-items: center; gap: 0.4rem; }
.dot { opacity: 0.6; }

.muted { color: #6b7280; font-size: 0.85rem; }
.h2 { margin: 0; font-size: 1.2rem; }
.h3 { margin: 1rem 0 0.5rem; font-size: 1rem; }

.grid2 { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; margin-top: 0.75rem; }
@media (max-width: 640px) { .grid2 { grid-template-columns: 1fr; } }

.info-box { border: 1px solid #e5e7eb; border-radius: 0.65rem; padding: 0.75rem; }
.label { font-size: 0.75rem; color: #6b7280; margin-bottom: 0.25rem; }

.visit-card { border: 1px solid #e5e7eb; border-radius: 0.75rem; padding: 0.8rem; margin-top: 0.75rem; }
.visit-top { display: flex; justify-content: space-between; gap: 1rem; }
@media (max-width: 640px) { .visit-top { flex-direction: column; } }
.visit-date { font-weight: 700; }

.money { text-align: right; font-size: 0.85rem; }
@media (max-width: 640px) { .money { text-align: left; } }

.pre { white-space: pre-wrap; margin-top: 0.5rem; }

.img-grid { margin-top: 0.6rem; display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 6px; }
@media (max-width: 900px) { .img-grid { grid-template-columns: repeat(3, 1fr); } }
@media (max-width: 640px) { .img-grid { grid-template-columns: repeat(2, 1fr); } }

.img-thumb { display: block; border-radius: 8px; overflow: hidden; border: 1px solid #e5e7eb; }
.img-thumb img { width: 100%; height: 110px; object-fit: cover; display: block; }

.state-error { color: #b91c1c; margin-top: 0.75rem; }
.profile-head {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: flex-start;
}

.profile-actions {
  display: flex;
  gap: 8px;
  align-items: center;
  justify-content: flex-end;
  flex-wrap: wrap;
}

.btn {
  border: 1px solid #d1d5db;
  background: #fff;
  border-radius: 0.55rem;
  padding: 0.45rem 0.7rem;
  font-size: 0.85rem;
  cursor: pointer;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn.btn-sm {
  padding: 0.4rem 0.65rem;
  font-size: 0.85rem;
}


</style>
