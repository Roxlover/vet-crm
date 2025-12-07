<template>
  <div class="page">
    <header class="page-header">
      <h1>Genel Bakış</h1>
      <p class="subtitle">
        Bugünkü, yarınki ve geciken hatırlatmaları özet halinde görün.
      </p>
    </header>

    <!-- LİSTE / TAKVİM SEKMELERİ -->
    <section class="view-tabs">
      <button
        class="tab-btn"
        :class="{ active: activeView === 'list' }"
        @click="activeView = 'list'"
      >
        Liste
      </button>
      <button
        class="tab-btn"
        :class="{ active: activeView === 'calendar' }"
        @click="showCalendar"
      >
        Takvim
      </button>
    </section>

    <!-- LİSTE GÖRÜNÜMÜ -->
    <section v-if="activeView === 'list'">
      <!-- 4 KUTU -->
      <section class="cards">
        <div
          class="card kpi today"
          :class="{ active: activeFilter === 'today' }"
          @click="loadList('today')"
        >
          <h2>Bugün</h2>
          <p class="value">{{ summary?.pendingToday ?? 0 }}</p>
          <p class="label">hatırlatma</p>
        </div>

        <div
          class="card kpi tomorrow"
          :class="{ active: activeFilter === 'tomorrow' }"
          @click="loadList('tomorrow')"
        >
          <h2>Yarın</h2>
          <p class="value">{{ summary?.pendingTomorrow ?? 0 }}</p>
          <p class="label">hatırlatma</p>
        </div>

        <div
          class="card kpi overdue"
          :class="{ active: activeFilter === 'overdue' }"
          @click="loadList('overdue')"
        >
          <h2>Geciken</h2>
          <p class="value">{{ summary?.overdue ?? 0 }}</p>
          <p class="label">henüz işlenmemiş</p>
        </div>

        <div
          class="card kpi done"
          :class="{ active: activeFilter === 'done' }"
          @click="loadList('done')"
        >
          <h2>İşlem Yapıldı</h2>
          <p class="value">{{ summary?.completed ?? 0 }}</p>
          <p class="label">tamamlanan</p>
        </div>
      </section>

      <!-- TABLO -->
      <section class="card upcoming">
        <div class="card-header">
          <h2>{{ titleForFilter() }}</h2>
        </div>

        <div v-if="listLoading" class="state">
          Yükleniyor...
        </div>

        <div
          v-else-if="!reminderList || reminderList.length === 0"
          class="state"
        >
          Kayıt bulunamadı.
        </div>

        <div v-else class="table-wrapper">
          <table class="table">
            <thead>
              <tr>
                <th>Hatırlatma</th>
                <th>Randevu</th>
                <th>Hasta</th>
                <th>Sahip</th>
                <th>İşlem</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="item in reminderList"
                :key="item.id"
                @click="openVisit(item)"
                class="clickable-row"
              >
                <td>{{ formatDate(item.reminderDate) }}</td>
                <td>{{ formatDate(item.appointmentDate) }}</td>
                <td>{{ item.petName }}</td>
                <td>{{ item.ownerName }}</td>
                <td class="procedure-cell">
                  {{ item.procedures }}
                  <span
                    v-if="item.creditAmountTl"
                    class="credit-pill"
                  >
                    • Veresiye: {{ item.creditAmountTl }} TL
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </section>

    <!-- TAKVİM GÖRÜNÜMÜ -->
    <section v-else class="calendar-section">
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
                @click="openNewAppointmentFromCalendar(day)"
              >
                <div class="day-number">
                  {{ day.date.getDate() }}
                </div>

                <div class="day-events">
                  <div
                    v-for="appt in day.appointments"
                    :key="appt?.visitId"
                    class="event-pill"
                    @click.stop="openVisitFromCalendar(appt)"
                  >
                    <span
                      class="event-time"
                      v-if="appt?.scheduledAt"
                    >
                      {{ formatTime(appt.scheduledAt) }}
                    </span>

                    <span class="event-text">
                      {{ appt?.petName }} – {{ appt?.ownerName }}
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
    <div class="modal">
      <button class="close" @click="closeDetail">×</button>
    <div v-if="detailLoading" class="state">Yükleniyor...</div>

    <!-- SADECE seçili ziyaret yok *ve* yeni randevu modu kapalıysa "kayıt yok" de -->
    <div v-else-if="!selectedVisit && !showNewAppointment" class="state">
      Kayıt bulunamadı.
    </div>
      <div v-else class="detail-body">
        <h3>{{ selectedVisit.petName }} – {{ selectedVisit.ownerName }}</h3>
        <p><strong>Yapılan işlem tarihi:</strong> {{ selectedVisit.performedAt }}</p>
        <p><strong>Ne zaman gelecek? :</strong> {{ formatDateTime(selectedVisit.nextDate) }}</p>
        <p><strong>Ne için gelecek? :</strong> {{ selectedVisit.purpose || '—' }}</p>
        <p><strong>Mikroçip numarası:</strong> {{ selectedVisit.microchipNumber || '—' }}</p>
        <p><strong>İşlem(ler):</strong> {{ selectedVisit.procedures || '—' }}</p>
        <p><strong>Tutar:</strong> {{ selectedVisit.amountTl ?? '—' }} TL</p>
        <p><strong>Hasta sahibine not:</strong> {{ selectedVisit.notes || '—' }}</p>
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
              :disabled="statusSaving"
              @click.stop="markReminder(false)"
            >
              Yapılmadı
            </button>
            <button
              class="btn-success"
              :disabled="statusSaving"
              @click.stop="markReminder(true)"
            >
              Yapıldı
            </button>
          </div>
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
    <img :src="visitImageSrc" alt="Ziyaret görseli" />
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
import { onMounted, ref, computed, reactive  } from 'vue'
import {
  fetchReminderSummary,
  fetchReminders,
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

const router = useRouter()
const form = reactive({
  microchipNumber: '',
})

const activeView = ref('list')          // 'list' | 'calendar'
const selectedReminderId = ref(null)
const statusSaving = ref(false)

const loading = ref(false)
const error = ref('')
const summary = ref(null)

const visitDetail = ref(null)           // (şimdilik kullanılmıyor)
const showDetailModal = ref(false)      // (şimdilik kullanılmıyor)

const showDetail = ref(false)
const detailLoading = ref(false)
const selectedVisit = ref(null)

const listLoading = ref(false)
const reminderList = ref([])
const activeFilter = ref('upcoming')

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

  // API'den nasıl gelirse yakalayalım:
  const rawImages = v.images || v.Images || []

  if (Array.isArray(rawImages) && rawImages.length) {
    return rawImages
  }

  // Eski tekli imageUrl desteği
  if (v.imageUrl) {
    return [{ id: 0, imageUrl: v.imageUrl }]
  }

  return []
})

const activeImageIndex = ref(0)

const visitImageSrc = computed(() => {
  if (!visitImages.value.length) return ''

  const img = visitImages.value[activeImageIndex.value] || visitImages.value[0]
  const url = img?.imageUrl
  if (!url) return ''

  return url.startsWith('http') ? url : API_BASE + url
})



onMounted(async () => {
  await loadSummary()
  await loadList('upcoming')
})

async function showCalendar() {
  activeView.value = 'calendar'
  if (calendarWeeks.value.length === 0) {
    await goToToday()
  }
}

function openImageModal() {
  if (!visitImageSrc.value) return
  showImageModal.value = true
}

function closeImageModal() {
  showImageModal.value = false
}

function toIsoDate(d) {
  return d.toISOString().slice(0, 10) // YYYY-MM-DD
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
    id: null,
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
    const iso = a.scheduledAt.slice(0, 10)
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
  console.log('openVisit item >>>', item)

  showImagePreview.value = false
  showDetail.value = true
  detailLoading.value = true
  selectedVisit.value = null
  selectedReminderId.value = item.id ?? null

  try {
    // 🔹 DETAYI ÇEK
    const detail = await fetchVisitDetail(item.visitId)

    console.log('VISIT DETAIL >>>', detail)
    console.log('images >>>', detail.images, detail.Images)

    selectedVisit.value = detail
    activeImageIndex.value = 0
    
    form.microchipNumber = detail.microchipNumber || ''

// 🔹 Veresiye input’u
creditAmount.value =
  detail.creditAmountTl != null ? detail.creditAmountTl.toString() : ''
creditEditOpen.value = false

    // 🔹 Veresiye input’u
    creditAmount.value =
      detail.creditAmountTl != null ? detail.creditAmountTl.toString() : ''
    creditEditOpen.value = false

    // 🔹 Owner bilgileri (yeni randevu formu için)
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
  if (!selectedReminderId.value) {
    alert('Bu kayıt bir hatırlatmaya bağlı değil, veresiye kaydedilemedi.')
    return
  }

  let raw = (creditAmount.value ?? '').toString().replace(',', '.')
  const val = parseFloat(raw)

  if (isNaN(val) || val < 0) {
    alert('Geçerli bir veresiye tutarı girin.')
    return
  }

  console.log('[CREDIT] PATCH isteği gönderiliyor...')
  console.log('[CREDIT] reminderId =', selectedReminderId.value)
  console.log('[CREDIT] body       =', { creditAmountTl: val })

  try {
    const res = await http.patch(`/reminders/${selectedReminderId.value}/credit`, {
      creditAmountTl: val,
    })

    console.log('[CREDIT] response status =', res.status)

    if (selectedVisit.value) {
      selectedVisit.value.creditAmountTl = val
    }

    await loadSummary()
    await loadList(activeFilter.value)

    creditEditOpen.value = false
  } catch (e) {
    console.error('veresiye kaydedilirken hata', e)
    console.log('[CREDIT] response?', e.response?.status, e.response?.data)
    alert('Veresiye kaydedilirken bir hata oluştu.')
  }
}

async function markReminder(completed) {
  if (!selectedReminderId.value) return

  statusSaving.value = true
  try {
    await updateReminderStatus(
      selectedReminderId.value,
      completed,
      !completed, // completed=false ise geciken olarak işaretle
    )

    await loadSummary()
    await loadList(completed ? 'done' : 'overdue')

    closeDetail()
  } catch (e) {
    console.error('markReminder error >>>', e)
  } finally {
    statusSaving.value = false
  }
}


async function openVisitDetail(item) {
  try {
    detailLoading.value = true
    showDetailModal.value = true
    visitDetail.value = await fetchVisitDetail(item.visitId)
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
  showImagePreview.value = false
  showImageModal.value = false
  activeImageIndex.value = 0
}

async function loadSummary() {
  loading.value = true
  error.value = ''
  try {
    const result = await fetchReminderSummary()
    console.log('SUMMARY FROM API >>>', result)
    summary.value = result
  } catch (e) {
    console.error(e)
    error.value = 'Hatırlatma özeti yüklenirken bir hata oluştu.'
  } finally {
    loading.value = false
  }
}

function isTimeWithinWorkingHours(timeStr) {
  if (!timeStr) return false
  const [h, m] = timeStr.split(':').map(Number)
  const total = h * 60 + m
  const start = 10 * 60 + 30   // 10:30
  const end = 19 * 60 + 30     // 19:30
  return total >= start && total <= end
}

async function submitAppointment() {
  const currentUser = getUser()
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
    microchipNumber: form.microchipNumber || null,
  }

  try {
    await createAppointment(payload)
    await loadSummary()
    await loadList(activeFilter.value)
    await loadCalendarForMonth(currentMonth.value)
    showNewAppointment.value = false
  } catch (e) {
    console.error('createAppointment error', e)
  }
}

async function loadList(filter) {
  activeFilter.value = filter
  listLoading.value = true
  try {
    reminderList.value = await fetchReminders(filter)
  } catch (e) {
    console.error(e)
  } finally {
    listLoading.value = false
  }
}

function formatDate(dateOnlyString) {
  if (!dateOnlyString) return '—'
  const [y, m, d] = dateOnlyString.split('-')
  return `${d}.${m}.${y}`
}

function titleForFilter() {
  switch (activeFilter.value) {
    case 'today':
      return 'Bugünkü hatırlatmalar'
    case 'tomorrow':
      return 'Yarının hatırlatmaları'
    case 'overdue':
      return 'Geciken hatırlatmalar'
    case 'done':
      return 'Tamamlanan işlemler'
    default:
      return 'Yaklaşan hatırlatmalar'
  }
}
</script>

<style scoped>
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

.cards {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

@media (max-width: 900px) {
  .cards {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 600px) {
  .cards {
    grid-template-columns: 1fr;
  }
}

.card {
  background: #ffffff;
  border-radius: 0.75rem;
  padding: 1rem;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.06);
}

.kpi {
  text-align: center;
  cursor: pointer;
  transition: transform 0.1s ease, box-shadow 0.1s ease;
}

.kpi:hover {
  transform: translateY(-2px);
  box-shadow: 0 14px 40px rgba(15, 23, 42, 0.12);
}

.kpi.active {
  outline: 2px solid #4ade80;
}

.kpi h2 {
  margin: 0;
  font-size: 0.9rem;
  color: #4b5563;
}

.kpi .value {
  margin: 0.25rem 0 0;
  font-size: 2rem;
  font-weight: 700;
}

.kpi .label {
  margin: 0;
  font-size: 0.8rem;
  color: #6b7280;
}

.today {
  border-top: 3px solid #22c55e;
}

.tomorrow {
  border-top: 3px solid #0ea5e9;
}

.overdue {
  border-top: 3px solid #f97316;
}

.card-header {
  margin-bottom: 0.5rem;
}

.card-header h2 {
  margin: 0;
  font-size: 1rem;
}

.state {
  font-size: 0.9rem;
  color: #6b7280;
  padding: 0.4rem 0;
}

.table-wrapper {
  width: 100%;
  overflow-x: auto;
}

.table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
  min-width: 640px;
}

.table th,
.table td {
  padding: 0.35rem 0.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.table th {
  text-align: left;
  font-weight: 600;
  color: #4b5563;
  font-size: 0.8rem;
}

.procedure-cell {
  max-width: 320px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.clickable-row {
  cursor: pointer;
}

.clickable-row:hover {
  background: #f9fafb;
}

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

.view-tabs {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.tab-btn {
  border: none;
  padding: 0.35rem 0.9rem;
  border-radius: 999px;
  background: #e5e7eb;
  font-size: 0.8rem;
  cursor: pointer;
}

.tab-btn.active {
  background: #111827;
  color: #fff;
}

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

</style>
