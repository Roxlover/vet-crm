<template>
  <div class="page">
    <header class="page-header">
      <h1>Ziyaret Kartı / İşlem Kaydı</h1>
      <p class="subtitle">
        Her ziyaret için aşağıdaki kartı doldurun; hatırlatmalar otomatik oluşsun.
      </p>
    </header>

    <!-- Sahip + hasta seçim alanı -->
    <section class="selector-card">
      <div class="field">
        <label>Hasta Sahibi</label>
        <select v-model="selectedOwnerId">
          <option value="">Seçiniz</option>
          <option
            v-for="owner in owners"
            :key="owner.id"
            :value="owner.id"
          >
            {{ owner.fullName }} ({{ owner.phoneE164 }})
          </option>
        </select>
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

    <!-- Asıl kart -->
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

        <div class="field-row">
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
            />
          </div>
        </div>

        <div class="form-row-inline">
          <div class="form-group">
            <label>Ne zaman gelecek? (Sonraki aşı / kontrol tarihi)</label>
            <input type="date" v-model="form.nextDate" />
          </div>

          <div class="form-group">
            <label>Ne için gelecek</label>
            <input
              type="text"
              v-model="form.purpose"

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
          <label>Görsel ekleme</label>
          <input type="file" @change="onFileChange" />
          <small class="hint">
            Örn: yara fotoğrafı, faturanın görüntüsü vb. (şimdilik sadece
            tutulmuyor).
          </small>
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
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { fetchOwners } from '../api/owners'
import { fetchPetsByOwner } from '../api/pets'
import { createVisit } from '../api/visits'

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
  amountTl: null,
  nextDate: '',
  purpose: '',
  ownerStatus: '',
  notes: '',
  imageFile: null,
})

// Seçilen sahip için pet listesi
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

// Owner seçilince: telefon + ad vs. doldur
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
  selectedPetId.value = '' // sahib değişince hasta sıfırlansın
})

// Pet seçilince: pet adını doldur
watch(selectedPetId, (newId) => {
  const idNum = Number(newId)
  const pet = pets.value.find((p) => p.id === idNum)
  if (pet) {
    petName.value = pet.name
  } else {
    petName.value = ''
  }
})

function onFileChange(event) {
  const file = event.target.files?.[0]
  form.imageFile = file || null
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
    // backend için alanları birleştir
    const proceduresText = form.vaccines
      ? `${form.procedures || ''}\nAşılar: ${form.vaccines}`
      : form.procedures || ''

    const notesParts = []
    if (form.ownerStatus) notesParts.push(`Sahip durumu: ${form.ownerStatus}`)
    if (form.notes) notesParts.push(form.notes)
    const notesText = notesParts.join('\n')

    const payload = {
      petId: Number(selectedPetId.value),
      performedAt: new Date(form.performedAt).toISOString(), // backend UTC bekliyordu
      procedures: proceduresText,
      amountTl: form.amountTl ?? 0,
      notes: notesText,
      nextDate: form.nextDate || null, // DateOnly gibi '2025-11-20' formatı
      purpose: form.purpose,
    }

    await createVisit(payload)

    success.value = 'Ziyaret kaydı başarıyla oluşturuldu.'

    // Formu kısmen temizle (sahip/hasta seçimi kalsın istersen)
    form.procedures = ''
    form.vaccines = ''
    form.performedAt = ''
    form.amountTl = null
    form.nextDate = ''
    form.ownerStatus = ''
    form.notes = ''
    form.imageFile = null
  } catch (e) {
    console.error(e)
    error.value = 'Kayıt oluşturulurken bir hata oluştu.'
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.page {
  padding: 1.5rem;
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
  gap: 12px;              /* aradaki boşluk */
}

.form-row-inline .form-group {
  flex: 1;                /* ikisi de eşit genişlikte olsun */
}

/* çok dar ekranda tekrar alta geçsin istersen: */
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
</style>
