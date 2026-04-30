<template>
  <div class="page">
    <header class="page-header">
      <h1>Genel Bakış</h1>
      <p class="subtitle">
        Bugünkü, yarınki ve geciken hatırlatmaları özet halinde görün.
      </p>
    </header>

    <!-- TAKVİM GÖRÜNÜMÜ -->
    <section class="calendar-section">
      <section class="card calendar-card">
        <div class="calendar-header">
          <div class="calendar-nav">
            <button class="icon-btn" @click="goToPrevMonth">‹</button>
            <div class="month-title">
              {{ formatMonthYear(currentMonth) }}
            </div>
            <button class="icon-btn" @click="goToNextMonth">›</button>
          </div>
          <button class="btn-today" @click="goToToday">
            Bugün
          </button>
        </div>

        <div v-if="calendarLoading" class="state">
          Yükleniyor...
        </div>

        <div v-else class="calendar-grid">
          <!-- Gün isimleri -->
          <div class="calendar-weekdays">
            <div
              v-for="d in weekdayLabels"
              :key="d"
              class="weekday"
            >
              {{ d }}
            </div>
          </div>

          <!-- Haftalar -->
          <div class="calendar-weeks">
            <div
              v-for="(week, wi) in calendarWeeks"
              :key="wi"
              class="calendar-week"
            >
              <div
                v-for="day in week"
                :key="day.iso"
                class="calendar-day"
                :class="{
                  'other-month': !day.inCurrentMonth,
                  today: day.isToday,
                }"
              @click="day.appointments && day.appointments.length && openVisitFromCalendar(day.appointments[0])"
              >
                <div class="day-number">
                  {{ day.date.getDate() }}
                </div>

                <div class="day-events">
                  <div
                    v-for="appt in day.appointments"
                    :key="appt?.reminderId ?? appt?.id ?? appt?.visitId"
                    class="event-pill"
                    @click.stop="appt && openVisitFromCalendar(appt)"
                  >
                  <span
                    class="event-time"
                    v-if="appt?.scheduledAt"
                  >
                    {{ formatTime(appt.scheduledAt) }}
                  </span>

                    <span class="event-text">
                      {{ appt?.petName || '—' }} – {{ appt?.ownerName || '—' }}
                    </span>

                    <span
                      v-if="appt?.purpose"
                      class="event-purpose"
                    >
                      {{ appt.purpose }}
                    </span>

                    <div class="event-meta">
                      <span v-if="appt?.doctorName">
                        Dr: {{ appt.doctorName }}
                      </span>
                      <span
                        v-if="
                          appt?.createdByUsername ||
                          appt?.createdByName
                        "
                      >
                        • Ekleyen:
                        {{
                          appt.createdByUsername ||
                          appt.createdByName
                        }}
                      </span>
                    </div>
                  </div>

                  <div
                    v-if="
                      !day.appointments ||
                      day.appointments.length === 0
                    "
                    class="no-event-placeholder"
                  >
                    —
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </section>
  </div>

  <!-- MODAL -->
  <div v-if="showDetail" class="modal-backdrop" @click.self="closeDetail">
   <div class="modal" @click.stop>
     <button class="close" @click.stop="closeDetail">×</button>
    <div v-if="detailLoading" class="state">Yükleniyor...</div>

    <!-- SADECE seçili ziyaret yok *ve* yeni randevu modu kapalıysa "kayıt yok" de -->
    <div v-else-if="!selectedVisit && !showNewAppointment" class="state">
      Kayıt bulunamadı.
    </div>
      <div v-else class="detail-body">
        <h3>{{ selectedVisit.petName }} – {{ selectedVisit.ownerName }}</h3>
        <div
  class="row"
  style="display:flex; gap:.5rem; justify-content:flex-end; align-items:center; margin:.25rem 0 .75rem;"
>
  <button
    v-if="!visitEditOpen"
    class="btn btn-sm"
    type="button"
    @click="openVisitEdit"
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
      @click="saveVisitEdit"
      :disabled="visitSaving"
    >
      {{ visitSaving ? 'Kaydediliyor...' : 'Kaydet' }}
    </button>
  </template>
</div>

<p v-if="visitSaveError" class="state state-error">{{ visitSaveError }}</p>

<div v-if="visitEditOpen && !visitDraft" class="state">
  Düzenleme hazırlanıyor...
</div>
        <p>
  <strong>Yapılan işlem tarihi:</strong>
  <span v-if="!visitEditOpen">{{ selectedVisit.performedAt }}</span>

  <input
    v-else-if="visitDraft"
    type="datetime-local"
    v-model="visitDraft.performedAt"
    class="input"
  />
</p>

  <!-- Ne zaman / ne için gelecek? -->
<div
  v-if="(
    (selectedVisit?.nextVisits?.length || selectedVisit?.NextVisits?.length) ||
    (selectedVisit?.plans?.length || selectedVisit?.Plans?.length) ||
    (selectedVisit?.nextDate || selectedVisit?.NextDate) ||
    (selectedVisit?.purpose || selectedVisit?.Purpose)
  )"
>
  <p><strong>Ne zaman / ne için gelecek?</strong></p>

  <ul class="next-visits-list">
    <li
      v-for="n in (
        selectedVisit?.nextVisits ||
        selectedVisit?.NextVisits ||
        selectedVisit?.plans ||
        selectedVisit?.Plans ||
        [{ nextDate: (selectedVisit.nextDate || selectedVisit.NextDate), purpose: (selectedVisit.purpose || selectedVisit.Purpose) }]
      )"
      :key="n.id || n.Id || n.nextDate || n.date || n.Date || 'single'"
    >
      <span>{{ formatDateTime(n.nextDate || n.date || n.Date) }}</span>
      <span>
        –
        {{
          n.purpose ??
          n.Purpose ??
          selectedVisit?.purpose ??
          selectedVisit?.Purpose ??
          '—'
        }}
      </span>
    </li>
  </ul>
</div>
 

       <div class="image-upload-row">
  <label class="btn">
    Görsel Ekle
    <input
      type="file"
      accept="image/*"
      multiple
      @change="onVisitImagesSelected"
      style="display:none;"
    />
  </label>

  <span v-if="imageUploading" class="hint">Yükleniyor...</span>
  <span v-if="imageUploadError" class="state state-error">{{ imageUploadError }}</span>
</div>
        <p>
  <strong>Mikroçip numarası:</strong>
  <span v-if="!visitEditOpen">{{ selectedVisit.microchipNumber || '—' }}</span>

  <input
    v-else-if="visitDraft"
    type="text"
    v-model="visitDraft.microchipNumber"
    class="input"
    placeholder="Örn: 900xxxx..."
  />
</p>
 
        <p>
  <strong>İşlem(ler):</strong>
  <span v-if="!visitEditOpen">{{ selectedVisit.procedures || '—' }}</span>

  <textarea
    v-else-if="visitDraft"
    v-model="visitDraft.procedures"
    class="input"
    rows="2"
    placeholder="Örn: Karma aşı, tırnak kesimi..."
  ></textarea>
</p>
        <p>
  <strong>Tutar:</strong>
  <span v-if="!visitEditOpen">{{ selectedVisit.amountTl ?? '—' }} TL</span>

  <input
    v-else-if="visitDraft"
    type="number"
    min="0"
    step="0.01"
    v-model.number="visitDraft.amountTl"
    class="input"
    placeholder="Örn: 1500"
  />
</p>
 
        <p>
  <strong>Hasta sahibine not:</strong>
  <span v-if="!visitEditOpen">{{ selectedVisit.notes || '—' }}</span>

  <textarea
    v-else-if="visitDraft"
    v-model="visitDraft.notes"
    class="input"
    rows="2"
    placeholder="Örn: 1 hafta sonra kontrol..."
  ></textarea>
</p>

        <p v-if="selectedVisit.createdByUsername || selectedVisit.createdByName">
          <strong>Kaydı ekleyen:</strong>
          {{ selectedVisit.createdByUsername || selectedVisit.createdByName }}
        </p>

        <hr class="divider" />

        <!-- İŞLEM DURUMU (Yapıldı / Yapılmadı) -->
        <div
          v-if="canEditIslemDurumu"
          class="status-row"
        >
          <div class="status-text">
            <strong>İşlem durumu:</strong>
            <span>Bu işlem işleme alındı mı?</span>
          </div>
         <div class="status-buttons">
<button
  class="btn-fail"
  type="button"
  @click="markReminder(false)"
  :disabled="statusSaving"
>
  Yapılmadı
</button>

<button
  class="btn-success"
  type="button"
  @click="markReminder(true)"
  :disabled="statusSaving"
>
  Yapıldı
</button>

</div>

<p v-if="statusError" class="state state-error">{{ statusError }}</p>

        </div>

<!-- Görsel alanı (çoklu) -->
<div v-if="selectedVisit">
  <div
    v-if="visitImages.length"
    class="visit-image-block"
  >
    <button
      type="button"
      class="btn-secondary"
      @click="showImagePreview = !showImagePreview"
    >
      {{ showImagePreview ? 'Görselleri gizle' : 'Görselleri göster' }}
    </button>

    <div v-if="showImagePreview" class="visit-image-preview">
      <!-- Büyük ana görsel -->
      <div
        v-if="visitImageSrc"
        class="visit-image-main"
      >
        <img
          :src="visitImageSrc"
          alt="Ziyaret görseli"
          @click="openImageModal"
        />
      </div>

      <!-- Thumbnail listesi -->
      <div
        v-if="visitImages.length > 1"
        class="visit-image-thumbs"
      >
        <button
          v-for="(img, idx) in visitImages"
          :key="img.id || idx"
          type="button"
          class="thumb"
          :class="{ active: idx === activeImageIndex }"
          @click="activeImageIndex = idx"
        >
          <img
            :src="img.imageUrl.startsWith('http') ? img.imageUrl : API_BASE + img.imageUrl"
            :alt="`Görsel ${idx + 1}`"
          />
        </button>
      </div>
    </div>
  </div>

  <div v-else class="visit-image-empty">
    Bu ziyarete ait kayıtlı görsel bulunmuyor.
  </div>
</div>

<!-- TAM EKRAN GÖRSEL MODALI (aynen kalabilir, sadece visitImageSrc kullanıyor) -->
<div
  v-if="showImageModal"
  class="image-modal-backdrop"
  @click.self="closeImageModal"
>
  <div class="image-modal-content">
    <img :src="visitImageSrc" class="visit-img-preview" alt="Ziyaret görseli" />
    <button class="image-modal-close" @click="closeImageModal">
      ✕
    </button>
  </div>
</div> 


        <!-- TAM EKRAN GÖRSEL MODALI -->
        <!-- <div
          v-if="showImageModal"
          class="image-modal-backdrop"
          @click.self="closeImageModal"
        >
          <div class="image-modal-content">
            <img :src="visitImageSrc" alt="Ziyaret görseli" />
            <button class="image-modal-close" @click="closeImageModal">
              ✕
            </button>
          </div>
        </div> -->

        <!-- VERESİYE GÖRÜNÜMÜ + EDİT -->
        <div class="credit-row">
          <div class="credit-text">
            <strong>Veresiye:</strong>
            <span v-if="selectedVisit && selectedVisit.creditAmountTl != null">
              {{ selectedVisit.creditAmountTl }} TL
            </span>
            <span v-else>Yok</span>
          </div>
          <div class="credit-actions">
            <button
              class="btn-credit"
              type="button"
              @click="creditEditOpen = !creditEditOpen"
            >
              {{ creditEditOpen ? 'İptal' : 'Veresiye Yaz / Güncelle' }}
            </button>
          </div>
        </div>

        <div v-if="creditEditOpen" class="field-row">
          <label>Veresiye (TL)</label>
          <input
            v-model="creditAmount"
            type="number"
            min="0"
            step="0.01"
            placeholder="Örn: 750"
          />
          <button
            class="btn-success"
            type="button"
            @click="saveCredit"
            :disabled="savingCredit"
          >
            {{ savingCredit ? 'Kaydediliyor...' : 'Veresiyeyi Kaydet' }}
          </button>
        </div>
<div class="row">
  <strong>Ne kadar alındı:</strong>
  <span>{{ collectedShown }} TL</span>

  <button class="btn btn-sm" type="button" @click="collectedEditOpen = !collectedEditOpen">
    {{ collectedEditOpen ? 'İptal' : 'Tahsilat Gir / Güncelle' }}
  </button>
</div>

<div v-if="collectedEditOpen" class="edit-box">
  <label>Ne kadar alındı (TL)</label>
  <input type="text" inputmode="decimal" placeholder="Örn: 1450,50" v-model="collectedInput" />
<button class="btn btn-sm" type="button" @click="saveCollected" :disabled="collectedSaving">
  {{ collectedSaving ? 'Kaydediliyor...' : 'Tahsilatı Kaydet' }}
</button>

</div>

        <!-- YENİ RANDEVU FORMU -->
        <hr class="divider" />

        <div class="new-appointment-header">
          <h4>Yeni Randevu Oluştur</h4>
          <button class="btn-toggle" @click="showNewAppointment = !showNewAppointment">
            {{ showNewAppointment ? 'Gizle' : 'Oluştur' }}
          </button>
        </div>

        <div v-if="showNewAppointment" class="new-appointment">
          <!-- Tarih & Saat -->
          <div class="field-row">
            <label>Tarih</label>
            <input type="date" v-model="appointmentDate" />
          </div>
          <div class="field-row">
            <label>Saat</label>
            <input
              type="time"
              v-model="appointmentTime"
              min="10:30"
              max="19:30"
              step="900"
            />
          </div>

          <!-- Açıklama -->
          <div class="field-row">
            <label>Ne için gelecek?</label>
            <textarea
              v-model="appointmentPurpose"
              rows="2"
              placeholder="Örn: Karma aşı, kontrol, tırnak kesimi..."
            ></textarea>
          </div>

          <div class="field">
            <label>Mikroçip numarası</label>
            <input
              type="text"
              v-model="form.microchipNumber"

            />
          </div>

          <!-- Doktor -->
          <div class="field-row">
            <label>İşlemi yapacak doktor</label>
            <select v-model="selectedDoctorId">
              <option :value="null">Doktor seç (opsiyonel)</option>
              <option
                v-for="doc in doctors"
                :key="doc.id"
                :value="doc.id"
              >
                {{ doc.fullName }}
              </option>
            </select>
          </div>

          <!-- Hasta sahibi arama -->
          <div class="field-row owner-search" @click.stop>
            <label>Hasta Sahibi</label>
            <div class="owner-input-wrapper">
              <input
                type="text"
                v-model="ownerQuery"
                placeholder="İsim veya telefon ile ara..."
                @input="onOwnerQueryInput"
                @focus="ownerSearchOpen = true"
              />
              <div
                v-if="ownerSearchOpen && ownerResults.length > 0"
                class="owner-results"
              >
                <div
                  v-for="o in ownerResults"
                  :key="o.id"
                  class="owner-result-item"
                  @click="selectOwner(o)"
                >
                  <div class="owner-name">{{ o.fullName }}</div>
                  <div class="owner-phone">{{ o.phone }}</div>
                </div>
              </div>
            </div>
            <p class="hint" v-if="!selectedOwnerId">
              Önce hasta sahibini seçin, ardından hayvan(lar)ı işaretleyin.
            </p>
          </div>

          <!-- Hayvan seçimi -->
          <div class="field-row">
            <label>Hayvan(lar)</label>

            <div class="mode-row">
              <label>
                <input
                  type="radio"
                  value="single"
                  v-model="appointmentMode"
                />
                Tek hayvan seç
              </label>
              <label>
                <input
                  type="radio"
                  value="multiple"
                  v-model="appointmentMode"
                />
                Birden fazla hayvan
              </label>
            </div>

            <div class="pets-list">
              <p v-if="!ownerPets || ownerPets.length === 0" class="hint">
                Bu hasta sahibine tanımlı başka hayvan bulunamadı.
              </p>
              <label
                v-for="pet in ownerPets"
                :key="pet.id"
                class="pet-option"
              >
                <input
                  type="checkbox"
                  :value="pet.id"
                  v-model="selectedPetIds"
                  :disabled="
                    appointmentMode === 'single' &&
                    selectedPetIds.length >= 1 &&
                    !selectedPetIds.includes(pet.id)
                  "
                />
                {{ pet.name }}
              </label>
            </div>
          </div>

          <div class="actions-row">
            <button class="btn-fail" @click="showNewAppointment = false">
              Vazgeç
            </button>
            <button class="btn-success" @click="submitAppointment">
              Randevuyu Kaydet
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref, computed, reactive, nextTick } from 'vue'
import {
  fetchVisitDetail,
  fetchDoctors,
  fetchOwnerPets,
  createAppointment,
  fetchCalendarAppointments,
  searchOwners,
  updateReminderStatus,
} from '../api/dashboard'
import { http, API_BASE } from '@/api/http'
import { useRouter } from 'vue-router'
import { getUser } from '@/utils/auth'
import { uploadVisitImages } from '../api/visits'
import { updateVisitStatus, updateVisitCollected } from '@/api/visits'

const router = useRouter()
const form = reactive({
  microchipNumber: '',
})

const collectedEditOpen = ref(false)
const collectedInput = ref(null)
const collectedSaving = ref(false)
const collectedError = ref('')
const appointmentSaving = ref(false)
const selectedReminderId = ref(null)
const imageUploading = ref(false)
const imageUploadError = ref('')
const statusSaving = ref(false)
const statusError = ref('')
const visitDetail = ref(null)           // (şimdilik kullanılmıyor)
const showDetailModal = ref(false)      // (şimdilik kullanılmıyor)
const visitEditOpen = ref(false)
const visitDraft = ref(null)
const visitSaveError = ref('')
const visitSaving = ref(false)

const showDetail = ref(false)
const detailLoading = ref(false)
const selectedVisit = ref(null)
const collectedShown = computed(() =>
  selectedVisit.value?.collectedAmountTl ??
  selectedVisit.value?.CollectedAmountTl ??
  0
)
const ownerPets = ref([])
const showNewAppointment = ref(false)
const appointmentDate = ref('')
const appointmentTime = ref('')
const appointmentPurpose = ref('')
const selectedPetIds = ref([])
const appointmentMode = ref('multiple')

const currentMonth = ref(new Date())
const calendarLoading = ref(false)
const calendarAppointments = ref([])
const calendarWeeks = ref([])
const weekdayLabels = ['Pzt', 'Sal', 'Çar', 'Per', 'Cum', 'Cmt', 'Paz']

const selectedOwnerId = ref(null)
const selectedOwnerLabel = ref('')
const ownerQuery = ref('')
const ownerResults = ref([])
const ownerSearchOpen = ref(false)
let ownerSearchTimeout = null

const doctors = ref([])
const selectedDoctorId = ref(null)

const creditEditOpen = ref(false)
const creditAmount = ref('')
const savingCredit = ref(false)

const showImagePreview = ref(false)
const showImageModal = ref(false)

const rawUser = getUser()

// Artık giriş yapmış herkes "Yapılmadı / Yapıldı" butonlarını görebilir
const canEditIslemDurumu = computed(() => !!rawUser)


const visitImages = computed(() => {
  const v = selectedVisit.value
  if (!v) return []

  // API farklı casing ile döndürebilir
  const rawImages = v.images || v.Images || []

  if (Array.isArray(rawImages) && rawImages.length) {
    return rawImages
  }

  // Eski tekli alan desteği (backend DTO: ImageUrl)
  const single =
    v.imageUrl ||
    v.ImageUrl ||
    v.imageURL ||
    v.ImageURL

  if (single) {
    return [{ id: 0, imageUrl: single }]
  }

  return []
})

const activeImageIndex = ref(0)

const visitImageSrc = computed(() => {
  if (!visitImages.value.length) return ''

  const img = visitImages.value[activeImageIndex.value] || visitImages.value[0]

  const rawUrl =
    img?.imageUrl ||
    img?.ImageUrl ||
    img?.url ||
    img?.Url

  if (!rawUrl) return ''

  // absolute ise olduğu gibi
  if (rawUrl.startsWith('http')) return rawUrl

  // relative ise API_BASE ile birleştir (çift slash önle)
  const base = API_BASE.endsWith('/') ? API_BASE.slice(0, -1) : API_BASE
  const path = rawUrl.startsWith('/') ? rawUrl : `/${rawUrl}`
  return `${base}${path}`
})

async function saveCollected() {
  const visitId = selectedVisit.value?.id || selectedVisit.value?.Id
  if (!visitId) {
    collectedError.value = 'VisitId bulunamadı.'
    return
  }

  collectedError.value = ''

  // 1) input -> string normalize
  const raw = (collectedInput.value ?? '').toString().trim()

  // 2) boşsa: null (istersen 0 yerine null yapıyoruz)
  //    (Backend mantığınız 0 gönderince "sil" gibi davranıyorsa, bunu 0 yapabilirsiniz.
  const amount =
    raw === '' ? null : Number(raw.replace(',', '.'))

  // 3) validasyon
  if (amount !== null && (Number.isNaN(amount) || amount < 0)) {
    collectedError.value = 'Geçerli bir tahsilat girin.'
    return
  }

  collectedSaving.value = true
  try {
    await updateVisitCollected(visitId, {
      collectedAmountTl: amount,
      note: `Tahsilat (VisitId=${visitId})`,
    })

    // Modal içeriğini tazele
    const res = await fetchVisitDetail(visitId)
    const fresh = res?.data ?? res
    selectedVisit.value = fresh
    visitDetail.value = fresh

    collectedEditOpen.value = false
  } catch (e) {
    console.error('[COLLECTED] save error', e)
    const msg = e?.response?.data
    collectedError.value =
      typeof msg === 'string'
        ? msg
        : (msg?.message || 'Tahsilat kaydedilemedi.')
  } finally {
    collectedSaving.value = false
  }
}
function toVisitDraft(v) {
  if (!v) return null

  const performedAt = v.performedAt ?? v.PerformedAt ?? ''
  // datetime-local input için: ISO geldiyse "YYYY-MM-DDTHH:mm" formatına kırpacağız
  const dtLocal = performedAt ? String(performedAt).slice(0, 16) : ''

  return {
    performedAt: dtLocal,
    microchipNumber: v.microchipNumber ?? v.MicrochipNumber ?? '',
    procedures: v.procedures ?? v.Procedures ?? '',
    notes: v.notes ?? v.Notes ?? '',
    amountTl: v.amountTl ?? v.AmountTl ?? null,

    // PUT DTO’da bunlar varsa null basmamak için taşıyoruz:
    nextDate: v.nextDate ?? v.NextDate ?? null,
    purpose: v.purpose ?? v.Purpose ?? null,
    plans: v.plans ?? v.Plans ?? null,
  }
}

function openVisitEdit() {
  if (!selectedVisit.value) return
  visitSaveError.value = ''
  visitDraft.value = toVisitDraft(selectedVisit.value)
  visitEditOpen.value = true
}

function cancelVisitEdit() {
  visitEditOpen.value = false
  visitDraft.value = null
  visitSaveError.value = ''
}

async function saveVisitEdit() {
  const visitId = selectedVisit.value?.id || selectedVisit.value?.Id
  if (!visitId) {
    visitSaveError.value = 'VisitId bulunamadı.'
    return
  }
  if (!visitDraft.value) {
    visitSaveError.value = 'Düzenleme verisi hazırlanamadı.'
    return
  }

  visitSaving.value = true
  visitSaveError.value = ''

  try {
    // amount normalize
    const rawAmount = visitDraft.value.amountTl
    const amount =
      rawAmount === '' || rawAmount == null
        ? null
        : Number(String(rawAmount).replace(',', '.'))

    if (amount !== null && (Number.isNaN(amount) || amount < 0)) {
      visitSaveError.value = 'Tutar geçersiz.'
      return
    }

    const performedAtLocal = (visitDraft.value.performedAt ?? '').toString().trim()
if (!performedAtLocal) {
  visitSaveError.value = 'Yapılan işlem tarihi zorunludur.'
  visitSaving.value = false
  return
}
const performedAt = `${performedAtLocal}:00`

    const payload = {
      performedAt,
      microchipNumber: visitDraft.value.microchipNumber || null,
      procedures: visitDraft.value.procedures || null,
      notes: visitDraft.value.notes || null,
      amountTl: amount,

      // Bunlar DTO’da varsa null’a düşmesin diye taşı:
      nextDate: visitDraft.value.nextDate ?? (selectedVisit.value?.nextDate ?? selectedVisit.value?.NextDate ?? null),
      purpose: visitDraft.value.purpose ?? (selectedVisit.value?.purpose ?? selectedVisit.value?.Purpose ?? null),
      plans:
  visitDraft.value.plans ??
  selectedVisit.value?.plans ??
  selectedVisit.value?.Plans ??
  selectedVisit.value?.nextVisits ??
  selectedVisit.value?.NextVisits ??
  null,
    }

    await http.put(`/visits/${visitId}`, payload)

    // detail refresh
    const res = await fetchVisitDetail(visitId)
    const fresh = res?.data ?? res
    selectedVisit.value = fresh
    visitDetail.value = fresh

    visitEditOpen.value = false
    visitDraft.value = null
  } catch (e) {
    console.error('[VISIT_EDIT] save error', e)
    const msg = e?.response?.data
    visitSaveError.value =
      typeof msg === 'string'
        ? msg
        : (msg?.message || 'Ziyaret güncellenemedi.')
  } finally {
    visitSaving.value = false
  }
}


async function onVisitImagesSelected(e) {
  const files = e?.target?.files
  if (!files || files.length === 0) return
  if (!selectedVisit.value?.id) return

  imageUploadError.value = ''
  imageUploading.value = true

  try {
    await uploadVisitImages(selectedVisit.value.id, files)

    const detail = await fetchVisitDetail(selectedVisit.value.id)
    selectedVisit.value = detail?.data ?? detail

    activeImageIndex.value = 0

    e.target.value = ''
  } catch (err) {
    console.error(err)
    imageUploadError.value = 'Görseller yüklenirken hata oluştu.'
  } finally {
    imageUploading.value = false
  }
}

onMounted(async () => {
  await goToToday()
})


// showCalendar removed

async function markSelectedVisitCompleted() {
  if (!selectedVisit.value?.id) return

  statusSaving.value = true
  statusError.value = ''

  try {
    await updateVisitStatus(selectedVisit.value.id, 'Completed')

    // detail refresh
    const detail = await fetchVisitDetail(selectedVisit.value.id)
    selectedVisit.value = detail?.data ?? detail
  } catch (e) {
    console.error(e)
    statusError.value = 'Durum güncellenemedi.'
  } finally {
    statusSaving.value = false
  }
}

async function markSelectedVisitMissed() {
  if (!selectedVisit.value?.id) return

  statusSaving.value = true
  statusError.value = ''

  try {
    await updateVisitStatus(selectedVisit.value.id, 'Missed')

    // detail refresh
    const detail = await fetchVisitDetail(selectedVisit.value.id)
    selectedVisit.value = detail?.data ?? detail
  } catch (e) {
    console.error(e)
    statusError.value = 'Durum güncellenemedi.'
  } finally {
    statusSaving.value = false
  }
}


function openImageModal() {
  if (!visitImageSrc.value) return
  showImageModal.value = true
}

function closeImageModal() {
  showImageModal.value = false
}

function toLocalYmd(d) {
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, "0")
  const day = String(d.getDate()).padStart(2, "0")
  return `${y}-${m}-${day}`
}
function uniqById(arr) {
  const m = new Map()
  for (const x of (arr || [])) {
    if (!x) continue
    // id bazen string gelebilir, normalize edelim
    const key = String(x.id ?? x.Id ?? '')
    if (!key) continue
    m.set(key, x) // son gelen kazansın
  }
  return Array.from(m.values())
}


function toIsoDate(d) {
  return toLocalYmd(d)
}

function toLocalIsoDate(isoOrDate) {
  const d = isoOrDate instanceof Date ? isoOrDate : new Date(isoOrDate)
  return toLocalYmd(d)
}

function onOwnerQueryInput() {
  ownerSearchOpen.value = true

  if (ownerSearchTimeout) {
    clearTimeout(ownerSearchTimeout)
  }

  ownerSearchTimeout = setTimeout(async () => {
    const q = ownerQuery.value.trim()
    if (!q) {
      ownerResults.value = []
      return
    }
    try {
      ownerResults.value = await searchOwners(q)
    } catch (e) {
      console.error('owner search error', e)
    }
  }, 300)
}

async function selectOwner(owner) {
  selectedOwnerId.value = owner.id
  selectedOwnerLabel.value = `${owner.fullName} (${owner.phone})`
  ownerQuery.value = selectedOwnerLabel.value
  ownerSearchOpen.value = false

  try {
    ownerPets.value = await fetchOwnerPets(owner.id)
  } catch (e) {
    console.error('fetchOwnerPets error', e)
    ownerPets.value = []
  }

  selectedPetIds.value = []
}

function closeOwnerSearch() {
  ownerSearchOpen.value = false
}

// --- Takvim yardımcıları ---
function startOfCalendarGrid(date) {
  const first = new Date(date.getFullYear(), date.getMonth(), 1)
  const day = first.getDay() || 7 // Paz=7, Pzt=1
  const diff = day - 1
  first.setDate(first.getDate() - diff)
  return first
}

function endOfCalendarGrid(date) {
  const start = startOfCalendarGrid(date)
  const end = new Date(start)
  end.setDate(start.getDate() + 6 * 7 - 1)
  return end
}

async function openVisitFromCalendar(event) {
  const fakeItem = {
    id: event.reminderId ?? null,   // <-- kritik
    visitId: event.visitId,
  }
  await openVisit(fakeItem)
}

function openNewAppointmentFromCalendar(day) {
  showDetail.value = true
  detailLoading.value = false
  selectedReminderId.value = null
  showNewAppointment.value = true
  appointmentDate.value = day.iso
  appointmentTime.value = ''
  appointmentPurpose.value = ''
  selectedDoctorId.value = null
  selectedPetIds.value = []
  appointmentMode.value = 'single'
  ownerPets.value = []
  selectedOwnerId.value = null
  selectedOwnerLabel.value = ''
  ownerQuery.value = ''
  ownerResults.value = []
  form.microchipNumber = ''
  showImagePreview.value = false
  selectedVisit.value = null
  activeImageIndex.value = 0
}



async function loadCalendarForMonth(baseDate) {
  calendarLoading.value = true
  try {
    const start = startOfCalendarGrid(baseDate)
    const end = endOfCalendarGrid(baseDate)
    const from = toIsoDate(start)
    const to = toIsoDate(end)

    const data = await fetchCalendarAppointments(from, to)
    calendarAppointments.value = data
    buildCalendarWeeks(baseDate, data)
  } catch (e) {
    console.error('Takvim yüklenirken hata:', e)
  } finally {
    calendarLoading.value = false
  }
}

function buildCalendarWeeks(baseDate, appointments) {
  const start = startOfCalendarGrid(baseDate)
  const weeks = []

  const safeAppointments = (appointments || []).filter(
    (a) => a && a.scheduledAt
  )

  const byDate = {}
  safeAppointments.forEach((a) => {
  const iso = toIsoDate(new Date(a.scheduledAt))
    if (!byDate[iso]) byDate[iso] = []
    byDate[iso].push(a)
  })

  const todayIso = toIsoDate(new Date())
  let current = new Date(start)

  for (let w = 0; w < 6; w++) {
    const week = []
    for (let d = 0; d < 7; d++) {
      const iso = toIsoDate(current)
      week.push({
        date: new Date(current),
        iso,
        inCurrentMonth: current.getMonth() === baseDate.getMonth(),
        isToday: iso === todayIso,
        appointments: byDate[iso] || [],
      })
      current.setDate(current.getDate() + 1)
    }
    weeks.push(week)
  }

  calendarWeeks.value = weeks
}

async function goToPrevMonth() {
  currentMonth.value = new Date(
    currentMonth.value.getFullYear(),
    currentMonth.value.getMonth() - 1,
    1,
  )
  await loadCalendarForMonth(currentMonth.value)
}

async function goToNextMonth() {
  currentMonth.value = new Date(
    currentMonth.value.getFullYear(),
    currentMonth.value.getMonth() + 1,
    1,
  )
  await loadCalendarForMonth(currentMonth.value)
}

async function goToToday() {
  currentMonth.value = new Date()
  await loadCalendarForMonth(currentMonth.value)
}

function formatMonthYear(date) {
  return date.toLocaleDateString('tr-TR', {
    month: 'long',
    year: 'numeric',
  })
}

function formatTime(iso) {
  const d = new Date(iso)
  return d.toLocaleTimeString('tr-TR', {
    hour: '2-digit',
    minute: '2-digit',
  })
}

async function openVisit(item) {

  showImagePreview.value = false
  showDetail.value = true
  detailLoading.value = true
  selectedVisit.value = null
  selectedReminderId.value = item?.id ?? null
  try {
    const res = await fetchVisitDetail(item.visitId)
const detail = res?.data ?? res

selectedVisit.value = detail
activeImageIndex.value = 0
form.microchipNumber = detail.microchipNumber || ''

const existingCollected =
  detail?.collectedAmountTl ?? detail?.CollectedAmountTl

collectedInput.value =
  existingCollected != null ? String(existingCollected) : ''
collectedEditOpen.value = false
collectedError.value = ''

    creditAmount.value =
      detail.creditAmountTl != null ? detail.creditAmountTl.toString() : ''
    creditEditOpen.value = false

    if (detail.ownerId) {
      selectedOwnerId.value = detail.ownerId
      selectedOwnerLabel.value = `${detail.ownerName}`
      ownerQuery.value = selectedOwnerLabel.value

      try {
        ownerPets.value = await fetchOwnerPets(detail.ownerId)
      } catch (e) {
        console.error('fetchOwnerPets error', e)
        ownerPets.value = []
      }
    }
  } catch (e) {
    console.error('fetchVisitDetail error >>>', e)
  } finally {
    detailLoading.value = false
  }

  // 🔹 Doktor drop-down
  try {
    doctors.value = await fetchDoctors()
  } catch (e) {
    console.error('Doktorlar yüklenirken hata:', e)
  }
}

async function saveCredit() {
  // 1) VisitId’yi modalda seçili kayıttan bul
  const visitId =
    selectedVisit.value?.id ||
    selectedVisit.value?.Id ||
    selectedVisit.value?.visitId ||
    selectedVisit.value?.VisitId

  if (!visitId) {
    alert('VisitId bulunamadı, veresiye kaydedilemedi.')
    return
  }

  // 2) input parse
  let raw = (creditAmount.value ?? '').toString().replace(',', '.')
  const val = parseFloat(raw)

  if (isNaN(val) || val < 0) {
    alert('Geçerli bir veresiye tutarı girin.')
    return
  }

  savingCredit.value = true
  try {
    // 3) Artık VISIT endpoint’i
    await http.patch(`/visits/${visitId}/credit`, { creditAmountTl: val })

    // 4) Modal anında güncellensin (optimistic)
    if (selectedVisit.value) {
      selectedVisit.value = { ...selectedVisit.value, creditAmountTl: val }
    }

    // 6) Takvim açıksa takvimi tazele
    await loadCalendarForMonth(currentMonth.value)

    // 7) İstersen backend’den taze veri çek (modal kesin doğru kalsın)
    try {
      const fresh = await fetchVisitDetail(visitId)
      selectedVisit.value = fresh
    } catch (e) {
      console.error('[CREDIT] fetchVisitDetail after patch failed', e)
    }

    creditEditOpen.value = false
  } catch (e) {
    console.error('saveCredit error', e.response?.status, e.response?.data || e.message)
    alert('Veresiye kaydedilirken bir hata oluştu.')
  } finally {
    savingCredit.value = false
  }
}



async function markReminder(completed) {
  if (!selectedReminderId.value) {
    console.warn('[markReminder] selectedReminderId is null', selectedReminderId.value)
    return
  }

  statusSaving.value = true
  statusError.value = ''

  try {
    await updateReminderStatus(
      selectedReminderId.value,
      completed,
      !completed // yapılmadı seçilince overdue'a düşürmek istiyorsan kalsın
    )

    // Listeyi doğru filtreye al
    const nextFilter = completed ? 'done' : 'overdue'
    activeFilter.value = nextFilter

    // Modal açıksa detail'i tazele + collected alanını yeniden hesapla
    const visitId =
      selectedVisit.value?.id ||
      selectedVisit.value?.Id ||
      selectedVisit.value?.visitId ||
      selectedVisit.value?.VisitId

    if (visitId) {
      const res = await fetchVisitDetail(visitId)
      const detail = res?.data ?? res
      selectedVisit.value = detail
      visitDetail.value = detail // opsiyonel ama sen zaten tutuyorsun

      const total = Number(detail?.amountTl ?? detail?.AmountTl ?? 0)
      const credit = Number(detail?.creditAmountTl ?? detail?.CreditAmountTl ?? 0)
      const collected = Math.max(0, total - credit)

      collectedInput.value = total > 0 ? collected : null
      collectedEditOpen.value = false
    }
  } catch (e) {
    console.error('markReminder error >>>', e)
    statusError.value = 'Durum güncellenemedi.'
  } finally {
    statusSaving.value = false
  }
}

async function openVisitDetail(item) {
  detailLoading.value = true
  showDetailModal.value = true
  collectedError.value = ''
  statusError.value = ''

  try {
    const visitId =
      item?.visitId || item?.VisitId || item?.id || item?.Id

    if (!visitId) {
      collectedError.value = 'Kayıt bulunamadı (VisitId yok).'
      selectedVisit.value = null
      visitDetail.value = null
      showDetailModal.value = false
      return
    }

    const res = await fetchVisitDetail(visitId)
    const detail = res?.data ?? res

    // KRİTİK: Modal tek kaynağı selectedVisit olsun
    selectedVisit.value = detail
    visitDetail.value = detail

    // Default tahsilat input’u
    const total = Number(detail?.amountTl ?? detail?.AmountTl ?? 0)
    const credit = Number(detail?.creditAmountTl ?? detail?.CreditAmountTl ?? 0)

    const existingCollected =
      detail?.collectedAmountTl ?? detail?.CollectedAmountTl

    const derivedCollected = Math.max(0, total - credit)
    const initialCollected =
      existingCollected != null ? Number(existingCollected) : derivedCollected

    collectedInput.value = total > 0 ? initialCollected : null
    collectedEditOpen.value = false
  } catch (e) {
    console.error('[openVisitDetail] error', e)
    // toastError varsa:
    // toastError('Kayıt bulunamadı.')
    collectedError.value = 'Kayıt bulunamadı.'
    showDetailModal.value = false
    selectedVisit.value = null
    visitDetail.value = null
  } finally {
    detailLoading.value = false
  }
}

function formatDateTime(dt) {
  if (!dt) return '—'
  const d = new Date(dt)
  return d.toLocaleDateString('tr-TR')
}

function closeDetail() {
  showDetail.value = false
  showDetailModal.value = false
  showImagePreview.value = false
  showImageModal.value = false
  activeImageIndex.value = 0
  showNewAppointment.value = false
  selectedVisit.value = null
  visitDetail.value = null
  selectedReminderId.value = null

  collectedEditOpen.value = false
  collectedInput.value = null
  collectedError.value = ''
  statusError.value = ''
}

// loadSummary removed

function isTimeWithinWorkingHours(timeStr) {
  if (!timeStr) return false
  const [h, m] = timeStr.split(':').map(Number)
  const total = h * 60 + m
  const start = 10 * 60 + 30   // 10:30
  const end = 19 * 60 + 30     // 19:30
  return total >= start && total <= end
}

async function submitAppointment() {
  if (appointmentSaving.value) return
  appointmentSaving.value = true
  const currentUser = getUser()
  if (!selectedVisit.value || !selectedVisit.value.id) {
    alert('Randevu oluşturmak için önce bir ziyaret kaydı (Visit) açmalısınız.')
    return
  }

  if (!currentUser) {
    alert('Oturumunuz sona erdi, lütfen tekrar giriş yapın.')
    router.push('/login')
    return
  }

  if (!selectedOwnerId.value) {
    alert('Lütfen hasta sahibini seçin.')
    return
  }
  if (!selectedPetIds.value || selectedPetIds.value.length === 0) {
    alert('En az bir hayvan seçmelisiniz.')
    return
  }
  if (!appointmentDate.value || !appointmentTime.value) {
    alert('Tarih ve saat seçin.')
    return
  }

  if (!isTimeWithinWorkingHours(appointmentTime.value)) {
    alert('Randevu saati 10:30 - 19:30 arasında olmalıdır.')
    return
  }

  const isoDateTime = new Date(
    `${appointmentDate.value}T${appointmentTime.value}:00`
  ).toISOString()

  const payload = {
    ownerId: selectedOwnerId.value,
    petIds: selectedPetIds.value,
    scheduledAt: isoDateTime,
    purpose: appointmentPurpose.value,
    doctorId: selectedDoctorId.value || null,
    visitId: selectedVisit.value ? selectedVisit.value.id : null,
    microchipNumber: form.microchipNumber || null,
  }


  try {
    await createAppointment(payload)
    await loadCalendarForMonth(currentMonth.value)
    showNewAppointment.value = false
  }finally {
  appointmentSaving.value = false
  }

}

// loadList removed

// titleForFilter removed
</script>

<style scoped>
.next-visits-list {
  margin: 0.5rem 0 0;
  padding-left: 1.1rem;
}
.next-visits-list li {
  margin: 0.25rem 0;
  line-height: 1.35;
  word-break: break-word;
}

.page {
  width: 100%;
  max-width: 1024px;
  margin: 0 auto;
  padding: 1rem 1rem 1.5rem;
}

.page-header {
  margin-bottom: 1rem;
}

.subtitle {
  margin: 0.25rem 0 0;
  font-size: 0.85rem;
  color: #6b7280;
}

/* .cards styles removed */

.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
}

.modal {
  background: white;
  border-radius: 0.75rem;
  padding: 1.25rem 1.5rem;
  max-width: 600px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  position: relative;
}

.modal .close {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
  border: none;
  background: transparent;
  font-size: 1.5rem;
  cursor: pointer;
}

.status-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  margin-top: 0.75rem;
}

.status-text span {
  margin-left: 0.25rem;
  font-size: 0.85rem;
  color: #4b5563;
}

.status-buttons {
  display: flex;
  gap: 0.5rem;
}

.btn-success,
.btn-fail {
  border: none;
  padding: 0.4rem 0.9rem;
  border-radius: 999px;
  font-size: 0.8rem;
  cursor: pointer;
  font-weight: 600;
}

.btn-success {
  background: #22c55e;
  color: #fff;
}

.btn-fail {
  background: #ef4444;
  color: #fff;
}

.btn-success:disabled,
.btn-fail:disabled {
  opacity: 0.6;
  cursor: default;
}

.btn-secondary {
  border: none;
  padding: 0.35rem 0.9rem;
  border-radius: 999px;
  background: #e5e7eb;
  color: #111827;
  font-size: 0.8rem;
  cursor: pointer;
}

.divider {
  margin: 0.75rem 0;
  border: none;
  border-top: 1px solid #e5e7eb;
}

.new-appointment-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 0.5rem;
  margin-bottom: 0.25rem;
}

.new-appointment-header h4 {
  margin: 0;
  font-size: 0.9rem;
}

.btn-toggle {
  border: none;
  background: #e5e7eb;
  border-radius: 999px;
  padding: 0.2rem 0.8rem;
  font-size: 0.75rem;
  cursor: pointer;
}

.new-appointment .field-row {
  margin-top: 0.5rem;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.85rem;
}
.visit-img-preview {
  width: 100%;
  max-height: 70vh;
  object-fit: contain;
  background: #111;
  border-radius: 10px;
  display: block;
}

.new-appointment label {
  font-weight: 600;
}

.new-appointment input[type='date'],
.new-appointment input[type='time'],
.new-appointment textarea {
  border-radius: 0.5rem;
  border: 1px solid #d1d5db;
  padding: 0.35rem 0.5rem;
  font-size: 0.85rem;
}

.mode-row {
  display: flex;
  gap: 1rem;
  font-size: 0.8rem;
}

.pets-list {
  margin-top: 0.35rem;
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1rem;
}

.pet-option {
  font-size: 0.8rem;
}

.hint {
  font-size: 0.8rem;
  color: #6b7280;
}

.new-appointment select {
  border-radius: 0.5rem;
  border: 1px solid #d1d5db;
  padding: 0.35rem 0.5rem;
  font-size: 0.85rem;
}

/* .view-tabs styles removed */

.visit-image-thumb img {
  max-width: 100%;
  max-height: 120px;
  border-radius: 8px;
  cursor: pointer;
  object-fit: cover;
  border: 1px solid #e5e7eb;
}

/* TAM EKRAN GÖRSEL MODAL */
.image-modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.75);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
}

.image-modal-content {
  position: relative;
  max-width: 90vw;
  max-height: 90vh;
}

.image-modal-content img {
  max-width: 100%;
  max-height: 100%;
  display: block;
  border-radius: 12px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.6);
}

.image-modal-close {
  position: absolute;
  top: 8px;
  right: 8px;
  border: none;
  background: rgba(0, 0, 0, 0.6);
  color: #fff;
  border-radius: 999px;
  width: 32px;
  height: 32px;
  cursor: pointer;
  font-size: 16px;
}

/* Takvim */
.calendar-card {
  margin-top: 0.5rem;
  overflow-x: auto;
}

.event-purpose {
  display: block;
  font-size: 0.68rem;
  color: #111827;
}

.calendar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.calendar-nav {
  display: flex;
  align-items: center;
  gap: 0.35rem;
}

.icon-btn {
  border: none;
  border-radius: 999px;
  padding: 0.2rem 0.6rem;
  background: #e5e7eb;
  cursor: pointer;
}

.month-title {
  font-weight: 600;
  text-transform: capitalize;
}

.btn-today {
  border: none;
  padding: 0.3rem 0.9rem;
  border-radius: 999px;
  background: #111827;
  color: #fff;
  font-size: 0.8rem;
  cursor: pointer;
}

.calendar-grid {
  margin-top: 0.25rem;
}

.calendar-weekdays,
.calendar-week {
  display: grid;
  grid-template-columns: repeat(7, minmax(0, 1fr));
}

.weekday {
  font-size: 0.75rem;
  text-align: center;
  color: #6b7280;
  padding: 0.25rem 0;
}

.calendar-day {
  border: 1px solid #e5e7eb;
  min-height: 90px;
  padding: 0.25rem;
  font-size: 0.75rem;
  background: #ffffff;
  display: flex;
  flex-direction: column;
}

.calendar-day.other-month {
  background: #f9fafb;
  color: #9ca3af;
}

.calendar-day.today {
  border-color: #0ea5e9;
  box-shadow: 0 0 0 1px #0ea5e9;
}

.day-number {
  font-weight: 600;
  margin-bottom: 0.25rem;
}

.day-events {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}

.event-pill {
  border-radius: 0.4rem;
  padding: 0.15rem 0.3rem;
  background: #eff6ff;
  font-size: 0.7rem;
  line-height: 1.2;
}

.event-time {
  font-weight: 600;
  margin-right: 0.2rem;
}

.no-event-placeholder {
  font-size: 0.7rem;
  color: #d1d5db;
}

.event-text {
  display: block;
}

.event-meta {
  font-size: 0.65rem;
  color: #6b7280;
}

.owner-search {
  position: relative;
}

.owner-input-wrapper {
  position: relative;
}

.owner-input-wrapper input {
  width: 100%;
  border-radius: 0.5rem;
  border: 1px solid #d1d5db;
  padding: 0.35rem 0.5rem;
  font-size: 0.85rem;
}

.owner-results {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: #ffffff;
  border-radius: 0.5rem;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.15);
  margin-top: 0.2rem;
  max-height: 220px;
  overflow-y: auto;
  z-index: 60;
}

.owner-result-item {
  padding: 0.4rem 0.6rem;
  cursor: pointer;
}

.owner-result-item:hover {
  background: #f3f4f6;
}

.owner-name {
  font-size: 0.85rem;
  font-weight: 500;
}

.owner-phone {
  font-size: 0.75rem;
  color: #6b7280;
}

.credit-row {
  margin-top: 0.75rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.85rem;
}

.credit-text span {
  margin-left: 0.25rem;
}

.btn-credit {
  border: none;
  padding: 0.35rem 0.9rem;
  border-radius: 999px;
  background: #facc15;
  color: #78350f;
  font-size: 0.8rem;
  cursor: pointer;
}

.credit-form {
  margin-top: 0.5rem;
  display: flex;
  gap: 0.5rem;
  align-items: center;
  font-size: 0.85rem;
}

.credit-form input {
  max-width: 120px;
  border-radius: 0.5rem;
  border: 1px solid #d1d5db;
  padding: 0.35rem 0.5rem;
  font-size: 0.85rem;
}

.credit-pill {
  margin-left: 0.25rem;
  padding: 0.05rem 0.4rem;
  border-radius: 999px;
  background: #fef3c7;
  color: #92400e;
  font-size: 0.72rem;
}

.visit-image-block {
  margin-top: 12px;
}

.visit-image-preview {
  margin-top: 8px;
  border-radius: 8px;
  overflow: hidden;
  border: 1px solid #e5e5e5;
  max-height: 260px;
  background: #f7f7f7;
}

.visit-image-preview img {
  display: block;
  width: 100%;
  height: auto;
  object-fit: contain;
}

.visit-image-empty {
  margin-top: 8px;
  font-size: 12px;
  color: #999;
}

@media (max-width: 768px) {
  .page {
    padding: 1rem;
  }

  .calendar-day {
    min-height: 72px;
    padding: 0.2rem;
  }

  .day-number {
    font-size: 0.7rem;
  }

  .event-pill {
    font-size: 0.65rem;
  }
}

@media (min-width: 1024px) {
  .page {
    padding-bottom: 2rem;
  }
}

@media (max-width: 480px) {
  .page {
    padding: 0.75rem;
  }

  .page-header h1 {
    font-size: 1.1rem;
  }

  .subtitle {
    font-size: 0.8rem;
  }

  .cards {
    gap: 0.75rem;
  }

  .modal {
    max-width: 100%;
    margin: 0 8px;
    padding: 1rem;
  }
}
.visit-image-main img {
  width: 100%;
  max-height: 220px;
  object-fit: contain;
  border-radius: 8px;
  cursor: pointer;
}

.visit-image-thumbs {
  margin-top: 8px;
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.visit-image-thumbs .thumb {
  border: none;
  padding: 0;
  background: transparent;
  border-radius: 6px;
  overflow: hidden;
  cursor: pointer;
  border: 2px solid transparent;
}

.visit-image-thumbs .thumb.active {
  border-color: #0ea5e9;
}

.visit-image-thumbs img {
  width: 64px;
  height: 64px;
  object-fit: cover;
  display: block;
}
.next-visits-list {
  margin: 0.25rem 0 0;
  padding-left: 1rem;
  font-size: 0.85rem;
}

.next-visits-list li + li {
  margin-top: 0.15rem;
}
.visit-img-thumb {
  width: 72px;
  height: 72px;
  object-fit: cover;
  border-radius: 8px;
  cursor: pointer;
}
.visit-img-preview {
  width: 100%;
  max-height: 70vh;
  object-fit: contain;
  background: #111;
  border-radius: 10px;
}
/* -------------------------
   VISIT EDIT (Modal) UI
-------------------------- */

/* Düzenle / İptal / Kaydet butonlarının bulunduğu üst satır */
.visit-edit-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
  align-items: center;
  margin: 0.25rem 0 0.75rem;
}

/* Edit modda satırların düzeni */
.visit-edit-field {
  display: grid;
  grid-template-columns: 220px 1fr;
  gap: 0.6rem;
  align-items: start;
  margin: 0.5rem 0;
}

@media (max-width: 640px) {
  .visit-edit-field {
    grid-template-columns: 1fr;
  }
}

/* Label/strong görünümü */
.visit-edit-field strong {
  display: block;
  font-size: 0.85rem;
  color: #374151;
  margin-top: 0.35rem;
}

/* Modal içindeki input/textarea/select ortak görünüm */
.modal .input,
.modal input[type="text"],
.modal input[type="number"],
.modal input[type="date"],
.modal input[type="time"],
.modal input[type="datetime-local"],
.modal textarea,
.modal select {
  width: 100%;
  border: 1px solid #d1d5db;
  border-radius: 0.6rem;
  padding: 0.55rem 0.65rem;
  font-size: 0.9rem;
  background: #fff;
  outline: none;
}

.modal textarea {
  resize: vertical;
  min-height: 72px;
}

.modal input:focus,
.modal textarea:focus,
.modal select:focus {
  border-color: #93c5fd;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
}

/* “Düzenleme hazırlanıyor…” state */
.modal .state {
  padding: 0.65rem 0.75rem;
  border-radius: 0.6rem;
  background: #f9fafb;
  border: 1px solid #e5e7eb;
}

.modal .state-error {
  background: #fef2f2;
  border-color: #fecaca;
}

/* Küçük butonlar (btn-sm) varsa daha sıkı */
.btn.btn-sm {
  padding: 0.45rem 0.65rem;
  font-size: 0.85rem;
  border-radius: 0.55rem;
}

/* Secondary buton (görselleri göster/gizle gibi) */
.btn-secondary {
  border: 1px solid #d1d5db;
  background: #fff;
  border-radius: 0.6rem;
  padding: 0.45rem 0.65rem;
  font-size: 0.85rem;
}

/* Edit kutuları (tahsilat/veresiye) */
.edit-box,
.field-row {
  margin-top: 0.6rem;
  padding: 0.75rem;
  border-radius: 0.75rem;
  background: #f9fafb;
  border: 1px solid #e5e7eb;
}

.edit-box label,
.field-row label {
  display: block;
  font-size: 0.8rem;
  color: #6b7280;
  margin-bottom: 0.35rem;
}

/* Modal içindeki “row” satırları daha tutarlı olsun */
.modal .row {
  display: flex;
  gap: 0.6rem;
  align-items: center;
  justify-content: space-between;
  margin: 0.5rem 0;
}

@media (max-width: 640px) {
  .modal .row {
    flex-direction: column;
    align-items: stretch;
  }
  .modal .row > button {
    width: 100%;
  }
}

</style>
