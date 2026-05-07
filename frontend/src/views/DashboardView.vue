<template>
  <main class="page-dashboard">
    <!-- TOP HIGHLIGHTS (Premium CRM Style) -->
    <section class="highlights-grid">
      <!-- Activity Line Chart (Dynamic SVG) -->
      <div class="card highlight-card activity-chart">
        <div class="chart-header">
          <h3>Haftalık Aktivite</h3>
          <span class="trend-up" v-if="trendValue >= 0">+{{ trendValue }}%</span>
          <span class="trend-down" v-else>{{ trendValue }}%</span>
        </div>
        <div class="svg-wrapper">
          <svg viewBox="0 0 100 30" class="line-chart">
            <path :d="activityPath" fill="none" stroke="var(--primary)" stroke-width="2" />
            <circle
              v-for="(p, i) in activityPoints"
              :key="i"
              :cx="p.x"
              :cy="p.y"
              r="1.5"
              fill="var(--primary)"
            />
          </svg>
        </div>
        <div class="chart-labels">
          <span v-for="label in activityLabels" :key="label">{{ label }}</span>
        </div>
      </div>
    </section>

    <!-- STATS GRID - PREMIUM MOBILE CRM STYLE -->
    <!-- STATS GRID - PREMIUM MINIMALIST STYLE -->
    <section class="stats-grid">
      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Bugünkü Randevular</span>
          <div class="stat-value">{{ stats.todayAppointmentsCount }}</div>
          <span class="stat-sub">Aktif Bekleyen</span>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Aktif Hasta</span>
          <div class="stat-value">{{ stats.activePetsCount }}</div>
          <span class="stat-sub">Sistemdeki Toplam</span>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Aylık Tahsilat</span>
          <div class="stat-value">₺{{ formatCurrency(stats.monthlyRevenue) }}</div>
          <span class="stat-sub">Toplam Gelir</span>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-info">
          <span class="stat-label">Hatırlatıcılar</span>
          <div class="stat-value">{{ stats.pendingRemindersCount }}</div>
          <span class="stat-sub">İşlem Bekleyen</span>
        </div>
      </div>
    </section>

    <!-- TAKVİM GÖRÜNÜMÜ -->
    <section class="calendar-section">
      <div class="section-header">
        <h2 class="section-title">Randevu Takvimi</h2>
        <div class="header-actions">
           <button class="btn btn-ghost btn-sm">Tüm Randevular</button>
        </div>
      </div>
      
      <section class="card calendar-card">
        <div class="calendar-header">
          <div class="month-info">
            <h3>{{ formatMonthYear(currentMonth) }}</h3>
          </div>
          <div class="calendar-nav">
            <button class="nav-btn" @click="goToPrevMonth">Geri</button>
            <button class="nav-btn today-btn" @click="goToToday">Bugün</button>
            <button class="nav-btn" @click="goToNextMonth">İleri</button>
          </div>
        </div>

        <div class="calendar-grid-wrapper">
          <div class="calendar-grid">
            <!-- Gün Başlıkları -->
            <div class="weekday-header">
              <div v-for="l in weekdayLabels" :key="l" class="weekday">{{ l }}</div>
            </div>

            <!-- Günler -->
            <div v-for="(week, wIdx) in calendarWeeks" :key="wIdx" class="calendar-week">
              <div
                v-for="day in week"
                :key="day.iso"
                class="calendar-day"
                :class="{ 'not-current': !day.inCurrentMonth, 'is-today': day.isToday }"
                @click="openNewAppointmentFromCalendar(day)"
              >
                <div class="day-number">{{ day.date.getDate() }}</div>
                <div class="day-events">
                  <div
                    v-for="event in day.appointments"
                    :key="event.id"
                    class="event-pill"
                    @click.stop="openVisitFromCalendar(event)"
                  >
                    <span class="time">{{ formatTime(event.scheduledAt) }}</span>
                    <span class="pet">{{ event.petName }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </section>

  <!-- MODAL -->
  <div v-if="showDetail" class="modal-backdrop" @click.self="closeDetail">
   <div class="modal" @click.stop>
     <button class="close" @click.stop="closeDetail">Kapat</button>
    <div v-if="detailLoading" class="state">Yükleniyor...</div>

    <!-- SADECE seçili ziyaret yok *ve* yeni randevu modu kapalıysa "kayıt yok" de -->
      <div v-else class="detail-body">
        <template v-if="selectedDayEvents.length > 0">
          <h3>{{ formatMonthDay(selectedDayDate) }} - Günün Randevuları</h3>
          <div class="day-events-list">
            <div 
              v-for="ev in selectedDayEvents" 
              :key="ev.id" 
              class="day-event-row"
              @click="openVisitFromCalendar(ev)"
            >
              <span class="ev-time">{{ formatTime(ev.scheduledAt) }}</span>
              <span class="ev-info"><strong>{{ ev.petName }}</strong> ({{ ev.ownerName }})</span>
              <span class="ev-purpose">{{ ev.purpose || '—' }}</span>
            </div>
          </div>
          <div v-if="selectedDayEvents.length === 0" class="state state-info" style="margin-bottom: 1rem;">
            Bu güne ait kayıtlı randevu bulunamadı.
          </div>
          <hr class="divider" />
        </template>

        <h3 v-if="!showNewAppointment">{{ selectedVisit?.petName }} – {{ selectedVisit?.ownerName }}</h3>
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
  <span v-if="!visitEditOpen && selectedVisit">{{ selectedVisit.performedAt }}</span>

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
  <span v-if="!visitEditOpen && selectedVisit">{{ selectedVisit.microchipNumber || '—' }}</span>

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
  <span v-if="!visitEditOpen && selectedVisit">{{ selectedVisit.procedures || '—' }}</span>

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
  <span v-if="!visitEditOpen && selectedVisit">{{ selectedVisit.amountTl ?? '—' }} TL</span>

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
  <span v-if="!visitEditOpen && selectedVisit">{{ selectedVisit.notes || '—' }}</span>

  <textarea
    v-else-if="visitDraft"
    v-model="visitDraft.notes"
    class="input"
    rows="2"
    placeholder="Örn: 1 hafta sonra kontrol..."
  ></textarea>
</p>

        <p v-if="selectedVisit?.createdByUsername || selectedVisit?.createdByName">
  <strong>Kaydı ekleyen:</strong>
  {{ selectedVisit?.createdByUsername || selectedVisit?.createdByName }}
</p>

        <hr class="divider" />

<!-- Görsel alanı (çoklu) -->
<div v-if="selectedVisit">
  <div v-if="visitImages.length" class="visit-image-block">
    <button
      type="button"
      class="btn-secondary"
      @click="showImagePreview = !showImagePreview"
    >
      {{ showImagePreview ? 'Görselleri gizle' : 'Görselleri göster' }}
    </button>

    <div v-if="showImagePreview" class="visit-image-preview">
      <!-- Büyük ana görsel -->
      <div v-if="visitImageSrc" class="visit-image-main">
        <img
          :src="visitImageSrc"
          alt="Ziyaret görseli"
          @click="openImageModal"
        />
      </div>

      <!-- Thumbnail listesi -->
      <div v-if="visitImages.length > 1" class="visit-image-thumbs">
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
      Kapat
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
</main>
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
  fetchDashboardStats,
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

const selectedDayEvents = ref([])
const selectedDayDate = ref(null)

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

const stats = reactive({
  activePetsCount: 0,
  monthlyRevenue: 0,
  todayAppointmentsCount: 0,
  pendingRemindersCount: 0,
  weeklyActivity: []
})

const trendValue = ref(0)

const activityLabels = computed(() => {
  if (!stats.weeklyActivity.length) return []
  return stats.weeklyActivity.map(d => {
    const date = new Date(d.date)
    return date.toLocaleDateString('tr-TR', { weekday: 'short' })
  })
})

const activityPoints = computed(() => {
  if (!stats.weeklyActivity.length) return []
  const max = Math.max(...stats.weeklyActivity.map(d => d.visitCount), 1)
  return stats.weeklyActivity.map((d, i) => ({
    x: (i / (stats.weeklyActivity.length - 1)) * 100,
    y: 25 - (d.visitCount / max) * 20
  }))
})

const activityPath = computed(() => {
  if (!activityPoints.value.length) return ''
  const pts = activityPoints.value
  let d = `M ${pts[0].x} ${pts[0].y}`
  for (let i = 1; i < pts.length; i++) {
    d += ` L ${pts[i].x} ${pts[i].y}`
  }
  return d
})

function formatCurrency(val) {
  if (val >= 1000000) return (val / 1000000).toFixed(1) + 'M'
  if (val >= 1000) return (val / 1000).toFixed(1) + 'k'
  return val.toString()
}

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

    // Dashboard ve Takvimi tazele
    await loadStats()
    await loadCalendarForMonth(currentMonth.value)

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

    // Dashboard ve Takvimi tazele
    await loadStats()
    await loadCalendarForMonth(currentMonth.value)

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
  await loadStats()
  await goToToday()
})

async function loadStats() {
  try {
    const data = await fetchDashboardStats()
    Object.assign(stats, data)

    // Basit bir trend hesaplama (son gün vs önceki gün ortalaması gibi bir şey uydurabiliriz ya da 0 bırakırız)
    // Şimdilik sadece görsellik için 0 kalsın veya backend'den gelmesini bekleyelim.
    trendValue.value = 0
  } catch (e) {
    console.error('Stats fetch error', e)
  }
}


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

    // Dashboard ve Takvimi tazele
    await loadStats()
    await loadCalendarForMonth(currentMonth.value)
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

    // Dashboard ve Takvimi tazele
    await loadStats()
    await loadCalendarForMonth(currentMonth.value)
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
  if (!day || !day.iso) return

  selectedDayEvents.value = day.appointments || []
  selectedDayDate.value = day.date
  
  // Modalı ve Randevu formunu aç
  showDetail.value = true
  detailLoading.value = false
  showNewAppointment.value = true 
  
  // Tarih ve varsayılan saat ayarla
  appointmentDate.value = day.iso
  appointmentTime.value = '11:00'
  
  // Formu temizle
  appointmentPurpose.value = ''
  selectedDoctorId.value = null
  selectedPetIds.value = []
  appointmentMode.value = 'multiple'
  ownerPets.value = []
  selectedOwnerId.value = null
  selectedOwnerLabel.value = ''
  ownerQuery.value = ''
  ownerResults.value = []
  form.microchipNumber = ''
  
  selectedReminderId.value = null
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

    // 5) Dashboard ve Takvimi tazele
    await loadStats()
    await loadCalendarForMonth(currentMonth.value)

    // 6) Backend’den taze veri çek (modal kesin doğru kalsın)
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

    // Dashboard ve Takvimi tazele
    await loadStats()
    await loadCalendarForMonth(currentMonth.value)
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

function formatMonthDay(dt) {
  if (!dt) return ''
  return dt.toLocaleDateString('tr-TR', { day: 'numeric', month: 'long' })
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
  selectedDayEvents.value = []
  selectedDayDate.value = null

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
  
  // 🔹 DÜZELTME: Artık bir ziyarete (Visit) bağlı olma zorunluluğu yok
  // if (!selectedVisit.value || !selectedVisit.value.id) { ... }

  if (!currentUser) {
    alert('Oturumunuz sona erdi, lütfen tekrar giriş yapın.')
    router.push('/login')
    return
  }

  if (!selectedOwnerId.value) {
    alert('Lütfen hasta sahibini seçin.')
    appointmentSaving.value = false
    return
  }
  if (!selectedPetIds.value || selectedPetIds.value.length === 0) {
    alert('En az bir hayvan seçmelisiniz.')
    appointmentSaving.value = false
    return
  }
  if (!appointmentDate.value || !appointmentTime.value) {
    alert('Tarih ve saat seçin.')
    appointmentSaving.value = false
    return
  }

  if (!isTimeWithinWorkingHours(appointmentTime.value)) {
    alert('Randevu saati 10:30 - 19:30 arasında olmalıdır.')
    appointmentSaving.value = false
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
    await loadStats()
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
.page-dashboard {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}

@media (max-width: 768px) {
  .highlights-grid {
    display: none; /* Mobilde grafik çok yer kaplar, gizleyelim */
  }
}

/* STATS GRID - CLEAN SAAS STYLE */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1.25rem;
  margin-bottom: 2.5rem;
}

@media (max-width: 768px) {
  .stats-grid {
    grid-template-columns: 1fr 1fr; /* Mobilde 2'li yan yana */
    gap: 0.75rem;
  }
  
  .stat-card {
    padding: 1rem;
  }

  .stat-value {
    font-size: 1.4rem;
  }
}

.stat-card {
  background: #ffffff;
  padding: 1.5rem;
  border-radius: 16px;
  border: 1px solid #f1f5f9;
  box-shadow: 0 1px 3px rgba(0,0,0,0.02);
  transition: all 0.2s ease;
  position: relative;
  overflow: hidden;
}

.stat-card::after {
  content: "";
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
  background: var(--primary);
  opacity: 0.1;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 24px -10px rgba(0,0,0,0.05);
  border-color: var(--primary-light);
}

.stat-label {
  display: block;
  font-size: 0.85rem;
  font-weight: 600;
  color: #64748b;
  text-transform: uppercase;
  letter-spacing: 0.025em;
  margin-bottom: 0.5rem;
}

.stat-value {
  font-size: 1.85rem;
  font-weight: 800;
  color: #0f172a;
  line-height: 1;
  margin-bottom: 0.25rem;
}

.stat-sub {
  font-size: 0.75rem;
  color: #94a3b8;
  font-weight: 500;
}

/* SECTION HEADERS */
.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.section-title {
  font-size: 1.5rem;
  font-weight: 800;
  color: #0f172a;
  letter-spacing: -0.02em;
}

/* CALENDAR REFINEMENTS */
.calendar-card {
  border: 1px solid #f1f5f9;
  border-radius: 20px;
  overflow: hidden;
}

.calendar-header {
  padding: 1.5rem;
  background: #ffffff;
  border-bottom: 1px solid #f1f5f9;
}

.nav-btn {
  padding: 0.5rem 1rem;
  font-size: 0.85rem;
  font-weight: 600;
  color: #475569;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
}

.nav-btn:hover {
  background: #f1f5f9;
  color: #0f172a;
}

.today-btn {
  background: #ffffff;
  color: var(--primary);
  border-color: var(--primary-light);
}

/* ACTIVITY CHART */
.activity-chart {
  flex-direction: column;
  align-items: stretch;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.trend-up {
  color: #10b981;
  font-weight: 800;
  font-size: 0.85rem;
  background: #ecfdf5;
  padding: 0.25rem 0.6rem;
  border-radius: 8px;
}

.svg-wrapper {
  height: 60px;
  margin: 1rem 0;
}

.line-chart {
  width: 100%;
  height: 100%;
  overflow: visible;
}

.chart-labels {
  display: flex;
  justify-content: space-between;
  color: var(--text-muted);
  font-size: 0.75rem;
  font-weight: 700;
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
  margin-bottom: 0.25rem;
}

.subtitle {
  color: var(--text-muted);
  font-size: 1.1rem;
}

/* STATS GRID - PREMIUM MOBILE CRM STYLE */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 1.5rem;
  margin-bottom: 3.5rem;
}

.stat-card {
  padding: 1.75rem;
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  gap: 1.5rem;
  transition: var(--transition);
  border: 1px solid rgba(255, 255, 255, 0.8);
  box-shadow: var(--shadow-sm);
  position: relative;
  overflow: hidden;
}

.stat-card:hover {
  transform: translateY(-8px);
  box-shadow: var(--shadow-lg);
}

.stat-card.purple { background: #f5f3ff; color: #5b21b6; }
.stat-card.green { background: #f0fdf4; color: #166534; }
.stat-card.blue { background: #eff6ff; color: #1e40af; }
.stat-card.orange { background: #fff7ed; color: #9a3412; }

.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: 18px;
  background: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.85rem;
  box-shadow: 0 4px 12px rgba(0,0,0,0.04);
}

.stat-info {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 0.9rem;
  font-weight: 600;
  opacity: 0.8;
  margin-bottom: 0.25rem;
}

.stat-value {
  font-size: 1.75rem;
  font-weight: 800;
  font-family: 'Outfit', sans-serif;
}

.calendar-card {
  background: #ffffff;
  border-radius: var(--radius-xl);
  padding: 1.5rem;
  border: 1px solid #f1f5f9;
  box-shadow: var(--shadow-lg);
  margin-top: 1rem;
}

.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  padding: 0 0.5rem;
}

.calendar-header h3 {
  font-size: 1.25rem;
  font-weight: 800;
  color: var(--text-main);
  text-transform: capitalize;
}

.calendar-nav {
  display: flex;
  gap: 0.5rem;
}

.nav-btn {
  background: #f8fafc;
  border: 1px solid #f1f5f9;
  padding: 0.5rem 0.75rem;
  border-radius: 10px;
  cursor: pointer;
  font-weight: 700;
  transition: var(--transition);
}

.nav-btn:hover {
  background: var(--primary-light);
  color: var(--primary);
}

.today-btn {
  padding: 0.5rem 1rem;
  font-size: 0.85rem;
}

.calendar-grid-wrapper {
  overflow-x: auto;
}

.calendar-grid {
  min-width: 600px;
}

.weekday-header {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  margin-bottom: 0.5rem;
}

.weekday {
  text-align: center;
  font-size: 0.75rem;
  font-weight: 800;
  color: var(--text-muted);
  text-transform: uppercase;
  padding: 0.5rem;
}

.calendar-week {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  border-top: 1px solid #f1f5f9;
}

.calendar-day {
  min-height: 100px;
  padding: 0.75rem;
  border-right: 1px solid #f1f5f9;
  cursor: pointer;
  transition: background 0.2s ease;
}

@media (max-width: 768px) {
  .calendar-day {
    min-height: 60px;
    padding: 0.4rem;
  }

  .day-number {
    font-size: 0.8rem;
  }

  .event-pill {
    padding: 2px 4px;
    font-size: 0.6rem;
  }
}

.calendar-day:last-child {
  border-right: none;
}

.calendar-day:hover {
  background: #f8fafc;
}

.calendar-day.not-current {
  background: #fafafa;
  opacity: 0.5;
}

.calendar-day.is-today {
  background: var(--primary-light);
}

.day-number {
  font-size: 0.85rem;
  font-weight: 800;
  color: var(--text-main);
  margin-bottom: 0.5rem;
}

.day-events {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.event-pill {
  background: var(--primary);
  color: #ffffff;
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 0.7rem;
  font-weight: 700;
  display: flex;
  gap: 4px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  box-shadow: 0 2px 4px rgba(79, 70, 229, 0.2);
}

.event-pill .time {
  opacity: 0.8;
}

/* MODAL REFINEMENTS */
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
  max-width: 550px;
  background: #ffffff;
  border-radius: var(--radius-xl);
  padding: 3rem;
  position: relative;
  box-shadow: var(--shadow-lg);
  overflow-y: auto;
  max-height: 90vh;
}

.modal .close {
  position: absolute;
  top: 1.5rem;
  right: 1.5rem;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  border: none;
  background: #f1f5f9;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  transition: var(--transition);
}

.modal .close:hover {
  background: #e2e8f0;
  transform: rotate(90deg);
}

.modal h3 {
  font-size: 1.75rem;
  font-weight: 800;
  letter-spacing: -0.04em;
  margin-bottom: 1.5rem;
  color: var(--text-main);
}

.modal .input, .modal textarea, .modal select {
  width: 100%;
  border: 1px solid #f1f5f9;
  border-radius: var(--radius-md);
  padding: 1rem;
  background: #f8fafc;
  outline: none;
  font-size: 1rem;
  margin-top: 0.75rem;
  transition: var(--transition);
}

.modal .input:focus {
  background: #ffffff;
  border-color: var(--primary);
  box-shadow: 0 0 0 4px var(--primary-light);
}

.modal label {
  font-weight: 700;
  font-size: 0.9rem;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.btn {
  padding: 1rem 1.5rem;
  border-radius: 14px;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  transition: var(--transition);
  border: none;
}

.btn.primary {
  background: var(--primary);
  color: #ffffff;
  box-shadow: 0 10px 20px -5px rgba(79, 70, 229, 0.4);
}

.btn.primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 15px 30px -10px rgba(79, 70, 229, 0.5);
}

.btn-success { background: var(--success); color: white; }
.btn-fail { background: var(--danger); color: white; }

.next-visits-list {
  list-style: none;
  padding: 0;
  margin: 1rem 0;
}

.next-visits-list li {
  padding: 1rem;
  background: #f8fafc;
  border-radius: 12px;
  margin-bottom: 0.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
    margin-bottom: 1.5rem;
  }
  .highlights-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  .stats-grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  .modal {
    padding: 2rem 1.5rem;
    width: 95%;
  }
  .modal h3 {
    font-size: 1.5rem;
  }
  .section-title {
    flex-direction: row;
    justify-content: space-between;
  }
}

/* Day Events List */
.day-events-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-bottom: 1.5rem;
}

.day-event-row {
  padding: 1rem;
  background: #f8fafc;
  border-radius: 12px;
  cursor: pointer;
  transition: var(--transition);
  border: 1px solid #f1f5f9;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.day-event-row:hover {
  background: var(--primary-light);
  border-color: var(--primary);
  transform: translateY(-2px);
}

.day-event-row .ev-time {
  font-weight: 800;
  color: var(--primary);
  font-size: 0.85rem;
}

.day-event-row .ev-info {
  font-size: 1rem;
  color: var(--text-main);
}

.day-event-row .ev-purpose {
  font-size: 0.85rem;
  color: var(--text-muted);
}
</style>
