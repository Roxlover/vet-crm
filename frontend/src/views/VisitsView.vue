<template>
  <div class="page">
    <header class="page-header">
      <h1>Ziyaret Kartı / İşlem Kaydı</h1>
      <p class="subtitle">
        Her ziyaret için aşağıdaki kartı doldurun; hatırlatmalar otomatik oluşsun.
      </p>
    </header>

    <section class="selector-card">
<div class="field">
  <label>Hasta Sahibi</label>

  <div class="combo" ref="ownerComboRef">
    <!-- Seçili chip -->
    <div v-if="selectedOwner" class="chip">
      <span class="chip-text">
        {{ selectedOwner.fullName }} ({{ selectedOwner.phoneE164 }})
      </span>
      <button type="button" class="chip-x" @click="clearOwner" aria-label="Seçimi temizle">
        ×
      </button>
    </div>

    <!-- Arama inputu -->
    <input
      v-else
      ref="ownerInputRef"
      class="combo-input"
      type="text"
      v-model="ownerQuery"
      placeholder="En az 2 harf yazın..."
      @focus="openOwnerDropdown"
      @click="openOwnerDropdown"
      @keydown.down.prevent="moveOwnerActive(1)"
      @keydown.up.prevent="moveOwnerActive(-1)"
      @keydown.enter.prevent="selectActiveOwner"
      @keydown.esc.prevent="closeOwnerDropdown"
      autocomplete="off"
      role="combobox"
      :aria-expanded="ownerDropdownOpen ? 'true' : 'false'"
      aria-autocomplete="list"
    />

    <!-- Dropdown -->
    <div v-if="ownerDropdownOpen" class="combo-popover">
      <div class="combo-hint" v-if="ownerQuery.trim().length < 2">
        Aramak için en az 2 karakter yazın.
      </div>

      <ul
        v-else
        class="combo-list"
        role="listbox"
      >
        <li
          v-for="(owner, idx) in filteredOwners"
          :key="owner.id"
          class="combo-item"
          :class="{ active: idx === ownerActiveIndex }"
          role="option"
          @mousedown.prevent="selectOwner(owner)"
          @mousemove="ownerActiveIndex = idx"
        >
          <div class="combo-title">{{ owner.fullName }}</div>
          <div class="combo-sub">{{ owner.phoneE164 }}</div>
        </li>

        <li v-if="!filteredOwners.length" class="combo-empty">
          Sonuç bulunamadı.
        </li>
      </ul>
    </div>
  </div>
</div>

      <div class="field">
        <label>Hasta (Hayvan)</label>
        <select v-model="selectedPetId" :disabled="!selectedOwnerId">
          <option value="">Seçiniz</option>
          <option
            v-for="pet in petsForSelectedOwner"
            :key="pet.id"
            :value="pet.id"
          >
            {{ pet.name }} – {{ pet.species }}
          </option>
        </select>
      </div>
    </section>

    <section class="visit-card">
      <header class="visit-header">
        <div>
          <div><strong>Hasta sahibi:</strong> {{ ownerName || '—' }}</div>
          <div><strong>Hasta adı:</strong> {{ petName || '—' }}</div>
        </div>
        <div>
          <div><strong>Tel:</strong> {{ ownerPhone || '—' }}</div>
        </div>
      </header>

      <div class="visit-body">
        <div class="field">
          <label>Neler uygulandı?</label>
          <textarea v-model="form.procedures" rows="2" />
        </div>

        <div class="field">
          <label>Hangi aşılar uygulandı?</label>
          <textarea v-model="form.vaccines" rows="2" />
        </div>

       <div class="field-row money-row">
  <div class="field">
    <label>Ne zaman uygulandı?</label>
    <input type="datetime-local" v-model="form.performedAt" />
  </div>

  <div class="field">
    <label>Ne kadar aldım (TL)?</label>
    <input
      type="number"
      min="0"
      step="0.01"
      v-model.number="form.amountTl"
      placeholder="Örn: 500"
    />
  </div>

  <div class="field">
    <label>Veresiye (TL)</label>
    <input
      v-model="form.creditAmountTl"
      type="number"
      min="0"
      step="0.01"
      placeholder="Örn: 750"
    />
  </div>
</div>
 
        <div class="form-row-inline">
        <div class="field-group">
          <label>Ne zaman / ne için gelecek?</label>

          <div
            v-for="(item, index) in nextVisits"
            :key="index"
            class="next-visit-row"
          >
            <input
              type="date"
              v-model="item.date"
            />

            <input
              type="text"
              v-model="item.purpose"
              placeholder="Örn: iç/dış parazit, karma aşı..."
            />

            <button
              v-if="nextVisits.length > 1"
              type="button"
              class="btn-small"
              @click="removeNextVisitRow(index)"
            >
              -
            </button>
          </div>

          <button
            type="button"
            class="btn-secondary"
            @click="addNextVisitRow"
          >
            + Tarih ekle
          </button>

          <p class="hint">
            En az bir tarih girebilirsin, istersen birden fazla satır ekle.
          </p>
        </div>


          <div class="form-group">
            <label>Ne için gelecek</label>
            <input
              type="text"
              v-model="form.purpose"
            />
          </div>

          <div class="form-group">
            <label>Mikroçip numarası</label>
            <input
              type="text"
              v-model="form.microchipNumber"

            />
          </div>
        </div>

        <div class="field">
          <label>Hasta sahibi durumu</label>
          <textarea v-model="form.ownerStatus" rows="2" />
        </div>

        <div class="field">
          <label>Hasta sahibi için not</label>
          <textarea v-model="form.notes" rows="3" />
        </div>

        <div class="field">
          <label>Görsel çek / ekle</label>

          <input
            type="file"
            accept="image/*"
            multiple
            @change="onFilesChange"
          />

          <small class="hint">
            Örn: yara fotoğrafı, faturanın görüntüsü vb.
          </small>

          <div
            v-if="form.imagePreviews && form.imagePreviews.length"
            class="visit-image-preview-grid"
          >
            <div
              v-for="(src, idx) in form.imagePreviews"
              :key="idx"
              class="visit-image-thumb"
            >
              <img :src="src" :alt="`Görsel ${idx + 1}`" />
            </div>
          </div>
        </div>

        <p v-if="error" class="state state-error">{{ error }}</p>
        <p v-if="success" class="state state-success">{{ success }}</p>
      </div>

      <footer class="visit-footer">
        <button class="btn" @click="handleSave" :disabled="saving">
          {{ saving ? 'Kaydediliyor...' : 'Kaydet' }}
        </button>
      </footer>
    </section>
  </div>
</template>

<script setup>
import { computed, onMounted, onBeforeUnmount, reactive, ref, watch, nextTick  } from 'vue'
import { fetchOwners } from '../api/owners'
import { fetchPetsByOwner } from '../api/pets'
import { http } from '@/api/http'

const owners = ref([])
const pets = ref([])

const selectedOwnerId = ref('')
const selectedPetId = ref('')

const ownerName = ref('')
const petName = ref('')
const ownerPhone = ref('')

const error = ref('')
const success = ref('')
const saving = ref(false)

const form = reactive({
  procedures: '',
  vaccines: '',
  performedAt: '',
  creditAmountTl: '',
  amountTl: null,
  nextDate: '',
  purpose: '',
  ownerStatus: '',
  notes: '',
  imageFiles: [],     
  imagePreviews: [],   
  microchipNumber: '', 
})
const nextVisits = ref([
  { date: '', purpose: '' },
])
const ownerQuery = ref('')
const ownerDropdownOpen = ref(false)
const ownerActiveIndex = ref(0)

const ownerComboRef = ref(null)
const ownerInputRef = ref(null)

// selectedOwnerId zaten var; bunu kullanıyoruz
const selectedOwner = computed(() => {
  const idNum = Number(selectedOwnerId.value)
  if (!idNum) return null
  return owners.value.find(o => o.id === idNum) || null
})

const filteredOwners = computed(() => {
  const q = ownerQuery.value.trim().toLowerCase()
  if (q.length < 2) return []

  // fullName + phone içinde arama
  return owners.value
    .filter(o => {
      const haystack = `${o.fullName || ''} ${o.phoneE164 || ''}`.toLowerCase()
      return haystack.includes(q)
    })
    .slice(0, 50) // çok uzunsa sınırlayalım
})

function openOwnerDropdown() {
  ownerDropdownOpen.value = true
  ownerActiveIndex.value = 0
}

function closeOwnerDropdown() {
  ownerDropdownOpen.value = false
}

function selectOwner(owner) {
  selectedOwnerId.value = String(owner.id)
  // Chip görüneceği için query'yi temizleyelim
  ownerQuery.value = ''
  ownerActiveIndex.value = 0

  // Seçince dropdown kapansın (isterseniz kapatma):
  ownerDropdownOpen.value = false
}

function clearOwner() {
  selectedOwnerId.value = ''
  selectedPetId.value = ''
  ownerName.value = ''
  ownerPhone.value = ''
  petName.value = ''

  // Tekrar arama yapılabilsin
  ownerQuery.value = ''
  ownerDropdownOpen.value = true

  // Mobilde klavye açılsın: inputa focus
  nextTick(() => {
    ownerInputRef.value?.focus()
  })
}

function moveOwnerActive(delta) {
  if (!ownerDropdownOpen.value) ownerDropdownOpen.value = true
  if (ownerQuery.value.trim().length < 2) return
  const len = filteredOwners.value.length
  if (!len) return

  const next = ownerActiveIndex.value + delta
  if (next < 0) ownerActiveIndex.value = len - 1
  else if (next >= len) ownerActiveIndex.value = 0
  else ownerActiveIndex.value = next
}

function selectActiveOwner() {
  if (ownerQuery.value.trim().length < 2) return
  const owner = filteredOwners.value[ownerActiveIndex.value]
  if (owner) selectOwner(owner)
}

// Dışarı tıklayınca kapat
function onDocPointerDown(e) {
  const root = ownerComboRef.value
  if (!root) return
  if (!root.contains(e.target)) {
    ownerDropdownOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('pointerdown', onDocPointerDown)
})

onBeforeUnmount(() => {
  document.removeEventListener('pointerdown', onDocPointerDown)
})

// Seçim yoksa inputa tıklayınca klavye gelsin: dropdown aç + focus zaten inputta
watch(ownerDropdownOpen, (open) => {
  if (open && !selectedOwner.value) {
    nextTick(() => ownerInputRef.value?.focus())
  }
})

function addNextVisitRow() {
  nextVisits.value.push({ date: '', purpose: '' })
}

function removeNextVisitRow(index) {
  nextVisits.value.splice(index, 1)
}

const petsForSelectedOwner = computed(() =>
  pets.value.filter((p) => p.ownerId === Number(selectedOwnerId.value))
)

onMounted(async () => {
  await loadOwnersAndPets()
})

async function loadOwnersAndPets() {
  try {
    const [ownersData, petsData] = await Promise.all([
      fetchOwners(),
      fetchPetsByOwner(),
    ])
    owners.value = ownersData
    pets.value = petsData
  } catch (e) {
    console.error(e)
    error.value = 'Sahip ve hasta bilgileri yüklenirken hata oluştu.'
  }
}

watch(selectedOwnerId, (newId) => {
  const idNum = Number(newId)
  const owner = owners.value.find((o) => o.id === idNum)
  if (owner) {
    ownerName.value = owner.fullName
    ownerPhone.value = owner.phoneE164
  } else {
    ownerName.value = ''
    ownerPhone.value = ''
  }
  selectedPetId.value = '' 
})

watch(selectedPetId, (newId) => {
  const idNum = Number(newId)
  const pet = pets.value.find((p) => p.id === idNum)
  petName.value = pet ? pet.name : ''
})

function onFilesChange(event) {
  const input = event.target
  const files = Array.from(input.files || [])

  console.log('SEÇİLEN DOSYA SAYISI (BU SEÇİM) >>>', files.length)

  if (!files.length) return

  // Aynı dosya tekrar eklenmesin diye (name+size+lastModified ile)
  const existingKey = new Set(
    (form.imageFiles || []).map(f => `${f.name}_${f.size}_${f.lastModified}`)
  )

  const newFiles = files.filter(f => !existingKey.has(`${f.name}_${f.size}_${f.lastModified}`))

  // APPEND: önceki seçimleri ezme
  form.imageFiles = [...(form.imageFiles || []), ...newFiles]
  form.imagePreviews = form.imageFiles.map(f => URL.createObjectURL(f))

  console.log('TOPLAM DOSYA SAYISI (BİRİKTİRİLMİŞ) >>>', form.imageFiles.length)

  // Kritik: aynı dosyayı tekrar seçebilmek için input'u sıfırla
  input.value = ''
}

async function handleSave() {
  error.value = ''
  success.value = ''

  if (!selectedOwnerId.value || !selectedPetId.value) {
    error.value = 'Lütfen hasta sahibi ve hastayı seçin.'
    return
  }

  if (!form.performedAt) {
    error.value = 'Lütfen işlemin yapıldığı zamanı girin.'
    return
  }

  saving.value = true

  try {
    // 1) Metin alanlarını birleştir
    const proceduresText = form.vaccines
      ? `${form.procedures || ''}\nAşılar: ${form.vaccines}`
      : form.procedures || ''

    const notesParts = []
    if (form.ownerStatus) notesParts.push(`Sahip durumu: ${form.ownerStatus}`)
    if (form.notes) notesParts.push(form.notes)
    const notesText = notesParts.join('\n')


    const credit =
  form.creditAmountTl === '' || form.creditAmountTl === null || form.creditAmountTl === undefined
    ? null
    : Number(String(form.creditAmountTl).replace(',', '.'))

if (credit !== null && (!Number.isFinite(credit) || credit < 0)) {
  error.value = 'Veresiye negatif olamaz.'
  return
} 
    // 2) Ziyaret payload
    const payload = {
      petId: Number(selectedPetId.value),
      performedAt: new Date(form.performedAt).toISOString(),
      procedures: proceduresText,
      amountTl: form.amountTl ?? 0,
      creditAmountTl: credit,
      notes: notesText,
      plans: nextVisits.value
       .filter(x => x.date)
       .map(x => ({
         Date: x.date,        // YYYY-MM-DD
         Purpose: x.purpose || null,
         DoctorId: null,
         })),
 
      purpose: form.purpose || null,
      microchipNumber: form.microchipNumber || null,
    }

    const res = await http.post('/visits', payload)
    const createdVisit = res.data
    const visitId = createdVisit.id || createdVisit.Id

    if (form.imageFiles.length && visitId) {
      const fd = new FormData()
     for (const file of Array.from(form.imageFiles)) {
       fd.append('files', file)
     }

      console.log('IMAGE UPLOAD START', {
        visitId,
        count: form.imageFiles.length,
      })

      try {
        const resUpload = await http.post(`/visits/${visitId}/images`, fd, {
          headers: { 'Content-Type': 'multipart/form-data' },
        })
      console.log('UPLOAD RESULT COUNT >>>', Array.isArray(resUpload.data) ? resUpload.data.length : resUpload.data)
      success.value = `Ziyaret kaydedildi. ${Array.isArray(resUpload.data) ? resUpload.data.length : form.imageFiles.length} görsel yüklendi.`
      console.log('IMAGE UPLOAD OK', resUpload.status, resUpload.data)
      } catch (e) {
        console.error(
          'image upload error',
          e.response?.status,
          e.response?.data || e.message,
        )
      }

      form.imageFiles = []
      form.imagePreviews = []
    }

    success.value = 'Ziyaret kaydedildi.'
    form.procedures = ''
    form.vaccines = ''
    form.performedAt = ''
    form.creditAmountTl = ''
    form.amountTl = null
    form.nextDate = ''
    form.purpose = ''
    form.ownerStatus = ''
    form.notes = ''
    form.microchipNumber = ''
  } catch (e) {
    console.error('visit save error', e)
    error.value = 'Ziyaret kaydedilirken bir hata oluştu.'
  } finally {
    saving.value = false
  }
  success.value = 'Ziyaret kaydedildi.'

  setTimeout(() => {
    window.location.href = '/'
  }, 600)

}
</script>

<style scoped>
.page {
  width: 100%;
  max-width: 1024px;
  margin: 0 auto;
  padding: 1rem;
}

.page-header {
  margin-bottom: 1rem;
}

.subtitle {
  margin: 0.25rem 0 0;
  font-size: 0.85rem;
  color: #6b7280;
}

.selector-card {
  display: flex;
  gap: 1rem;
  background: #fff;
  padding: 1rem;
  border-radius: 0.75rem;
  margin-bottom: 1rem;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);
}

.field {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
}

.field-row {
  display: flex;
  gap: 1rem;
}

label {
  font-size: 0.85rem;
  font-weight: 500;
  color: #374151;
}

input,
textarea,
select {
  border-radius: 0.5rem;
  border: 1px solid #d1d5db;
  padding: 0.4rem 0.6rem;
  font-size: 0.85rem;
  font-family: inherit;
}

.visit-card {
  background: #fff;
  border-radius: 0.75rem;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);
  overflow: hidden;
}

.visit-header {
  display: flex;
  justify-content: space-between;
  padding: 0.9rem 1rem;
  border-bottom: 1px solid #e5e7eb;
  background: #f9fafb;
  font-size: 0.9rem;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 0.7rem;
}
.combo {
  position: relative;
}

.combo-input {
  width: 100%;
}

.combo-popover {
  position: absolute;
  z-index: 50;
  top: calc(100% + 6px);
  left: 0;
  right: 0;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 0.6rem;
  box-shadow: 0 18px 50px rgba(15, 23, 42, 0.10);
  overflow: hidden;
}

.combo-hint {
  padding: 0.6rem 0.7rem;
  font-size: 0.8rem;
  color: #6b7280;
}

.combo-list {
  list-style: none;
  margin: 0;
  padding: 0.25rem;
  max-height: 260px;
  overflow: auto;
}

.combo-item {
  cursor: pointer;
  padding: 0.55rem 0.6rem;
  border-radius: 0.5rem;
}

.combo-item.active,
.combo-item:hover {
  background: #f3f4f6;
}

.combo-title {
  font-size: 0.85rem;
  font-weight: 600;
  color: #111827;
}

.combo-sub {
  font-size: 0.78rem;
  color: #6b7280;
  margin-top: 0.1rem;
}

.combo-empty {
  padding: 0.7rem;
  font-size: 0.82rem;
  color: #6b7280;
}

.chip {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.6rem;
  border: 1px solid #d1d5db;
  border-radius: 999px;
  padding: 0.35rem 0.55rem;
  background: #f9fafb;
}

.chip-text {
  font-size: 0.85rem;
  color: #111827;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.chip-x {
  border: none;
  background: transparent;
  cursor: pointer;
  font-size: 1.1rem;
  line-height: 1;
  padding: 0 0.25rem;
  color: #6b7280;
}

.chip-x:hover {
  color: #111827;
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

.visit-body {
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.visit-footer {
  padding: 0.8rem 1rem 1rem;
  text-align: right;
}

.btn {
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 999px;
  background: #22c55e;
  color: #022c22;
  font-weight: 600;
  font-size: 0.85rem;
  cursor: pointer;
}

.btn:hover {
  filter: brightness(0.95);
}

.state {
  font-size: 0.85rem;
}

.state-error {
  color: #b91c1c;
}

.state-success {
  color: #15803d;
}

.hint {
  font-size: 0.75rem;
  color: #6b7280;
}

/* küçük thumbnail grid’i */
.visit-image-preview-grid {
  margin-top: 0.5rem;
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.visit-image-thumb img {
  width: 72px;
  height: 72px;
  object-fit: cover;
  border-radius: 6px;
  border: 1px solid #e5e7eb;
}

.next-visit-row {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.4rem;
}

.next-visit-row input[type='date'] {
  max-width: 150px;
}

.next-visit-row input[type='text'] {
  flex: 1;
}

.btn-small {
  border: none;
  background: #fee2e2;
  color: #b91c1c;
  border-radius: 999px;
  padding: 0 0.6rem;
  cursor: pointer;
}
/* Para alanlarını mobilde düzgün kır */
.money-row {
  flex-wrap: wrap;
}

.money-row .field {
  min-width: 0;
  flex: 1 1 220px; /* daralınca alta iner */
}

@media (max-width: 640px) {
  .selector-card {
    flex-direction: column; /* Hasta sahibi / hayvan mobilde alt alta */
  }

  .field-row {
    flex-direction: column; /* performedAt + amount + credit mobilde alt alta */
  }
}

</style>
