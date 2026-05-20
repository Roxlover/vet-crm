<template>
  <main class="page-dashboard">


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
                    :class="{ 'is-visit': event.isVisit }"
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

  <!-- MODERN PREMIUM MODAL -->
  <div v-if="showDetail" class="modal-overlay" @click.self="closeDetail">
    <div class="modern-modal" @click.stop>
      <!-- Modal Header -->
      <header class="modal-header">
        <div class="header-info">
          <h2 v-if="!showNewAppointment">
            <template v-if="!visitEditOpen">
              <span class="pet-name">{{ selectedVisit?.petName || 'Ziyaret Detayı' }}</span>
              <span class="owner-name">
                {{ selectedVisit?.ownerName }}
                <button 
                  v-if="selectedVisit?.ownerPhone" 
                  class="btn-whatsapp-icon" 
                  title="WhatsApp ile Hatırlat"
                  @click="sendWhatsAppReminder(selectedVisit.ownerName, selectedVisit.ownerPhone, selectedVisit.petName, selectedVisit.performedAt, selectedVisit.procedures)"
                >
                  <svg viewBox="0 0 24 24" class="wp-icon" fill="currentColor">
                    <path d="M.057 24l1.687-6.163c-1.041-1.804-1.588-3.849-1.587-5.946C.06 5.348 5.397.01 12.008.01c3.202.001 6.212 1.246 8.477 3.514 2.266 2.268 3.507 5.28 3.505 8.484-.004 6.657-5.34 11.997-11.953 11.997-2.005-.001-3.973-.502-5.724-1.455L0 24zm6.59-4.846c1.6.95 3.197 1.451 4.819 1.452 5.485 0 9.94-4.447 9.943-9.923.002-2.652-1.031-5.147-2.907-7.027-1.878-1.88-4.376-2.914-7.026-2.915-5.486 0-9.941 4.448-9.944 9.927-.001 1.785.484 3.528 1.408 5.048L1.1 21.09l4.547-1.192-.04.024zm10.154-7.587c-.244-.122-1.442-.712-1.666-.793-.223-.08-.386-.122-.549.122-.163.243-.63.793-.772.955-.143.162-.285.182-.529.06-2.023-1.009-3.342-2.07-4.686-4.37-.354-.606-.035-.93.266-1.23.271-.27.549-.64.67-.89.12-.25.06-.47-.03-.65-.09-.18-.549-1.32-.752-1.81-.197-.474-.396-.41-.549-.418-.143-.007-.306-.007-.468-.007-.163 0-.427.06-.65.3-.224.24-.854.83-.854 2.03s.874 2.35 1.002 2.51c.122.16 1.7 2.59 4.12 3.64.57.25 1.02.4 1.37.5.58.18 1.1.16 1.52.1.47-.07 1.442-.59 1.646-1.16.204-.57.204-1.06.142-1.16-.06-.1-.22-.16-.47-.28z"/>
                  </svg>
                </button>
              </span>
            </template>
            <template v-else>
              <span class="edit-title">Kayıt Düzenleme</span>
            </template>
          </h2>
          <h2 v-else>Yeni Randevu Kaydı</h2>
        </div>
        <button class="modal-close-btn" @click="closeDetail">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M18 6L6 18M6 6l12 12"/></svg>
        </button>
      </header>

      <div class="modal-content-wrapper">
        <!-- SUCCESS STATE -->
        <div v-if="showSuccess" class="success-state">
           <div class="success-icon-wrapper">
             <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M20 6L9 17l-5-5"/></svg>
           </div>
           <h3>Başarıyla Kaydedildi!</h3>
           <p>Randevu ve ziyaret kaydı başarıyla oluşturuldu.</p>
        </div>

        <div v-else-if="detailLoading" class="loading-state">
          <div class="spinner"></div>
          <p>Veriler yükleniyor...</p>
        </div>

        <div v-else-if="!showSuccess" class="modal-body">
          <!-- 1. GÜNÜN RANDEVULARI (Eğer varsa) -->
          <section v-if="selectedDayEvents.length > 0" class="modal-section appointments-section">
            <h3 class="section-subtitle">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>
              {{ formatMonthDay(selectedDayDate) }} Randevuları
            </h3>
            <div class="mini-event-list">
              <div 
                v-for="ev in selectedDayEvents" 
                :key="ev.id" 
                class="mini-event-card"
                @click="openVisitFromCalendar(ev)"
              >
                <span class="ev-time">{{ formatTime(ev.scheduledAt) }}</span>
                <div class="ev-main">
                  <span class="ev-pet">{{ ev.petName }}</span>
                  <span class="ev-purpose">{{ ev.purpose || 'Genel Muayene' }}</span>
                </div>
                <svg class="chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9 5l7 7-7 7"/></svg>
              </div>
            </div>
          </section>

          <!-- 2. ZİYARET DETAYI (Eğer bir ziyaret seçiliyse) -->
          <section v-if="selectedVisit" class="modal-section visit-detail-section">
            <div class="section-header-row">
              <h3 class="section-subtitle">Ziyaret Bilgileri</h3>
              <div class="header-actions" style="display: flex; gap: 0.5rem; align-items: center;">
                <template v-if="!visitEditOpen">
                  <button class="btn-action" @click="openVisitEdit">Düzenle</button>
                  <button class="btn-danger-sm" @click="handleDeleteVisit" :disabled="visitSaving">Sil</button>
                </template>
                <div v-else class="edit-actions">
                  <button class="btn-text" @click="cancelVisitEdit" :disabled="visitSaving">İptal</button>
                  <button class="btn-primary-sm" @click="saveVisitEdit" :disabled="visitSaving">
                    {{ visitSaving ? '...' : 'Kaydet' }}
                  </button>
                </div>
              </div>
            </div>

            <div class="detail-grid">
              <!-- HASTA & SAHİBİ (DÜZENLEME MODUNDA GÖRÜNÜR) -->
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

              <div class="detail-item full">
                <label>İşlem Tarihi</label>
                <div v-if="!visitEditOpen && selectedVisit" class="val">{{ selectedVisit.performedAt }}</div>
                <input v-else-if="visitDraft" type="datetime-local" v-model="visitDraft.performedAt" class="modern-input" />
              </div>

              <div class="detail-item">
                <label>Mikroçip</label>
                <div v-if="!visitEditOpen" class="val">{{ selectedVisit?.microchipNumber || '—' }}</div>
                <input v-else-if="visitDraft" type="text" v-model="visitDraft.microchipNumber" class="modern-input" />
              </div>

              <div class="detail-item full">
                <label>Yapılan İşlemler</label>
                <div v-if="!visitEditOpen" class="val highlight">{{ selectedVisit?.procedures || '—' }}</div>
                <div v-else-if="visitDraft">
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
                  <textarea v-model="visitDraft.procedures" class="modern-input" rows="2"></textarea>
                </div>
              </div>

              <div class="detail-item">
                <label>Bilanço (Ziyaret Tutarı)</label>
                <div v-if="!visitEditOpen" class="val currency">{{ selectedVisit?.amountTl ?? '0' }} TL</div>
                <div v-else-if="visitDraft" class="input-with-suffix">
                  <input type="number" v-model.number="visitDraft.amountTl" class="modern-input" />
                  <span class="suffix">TL</span>
                </div>
              </div>

              <div class="detail-item full">
                <label>Ziyaret Notları</label>
                <div v-if="!visitEditOpen" class="val">{{ selectedVisit?.notes || '—' }}</div>
                <textarea v-else-if="visitDraft" v-model="visitDraft.notes" class="modern-input" rows="2"></textarea>
              </div>
            </div>

            <!-- FİNANSAL ÖZET KARTI -->
            <div class="finance-card">
              <div class="fin-row">
                <div class="fin-item">
                  <span class="fin-label">Veresiye</span>
                  <span class="fin-val" :class="{ 'has-debt': (selectedVisit.creditAmountTl || 0) > 0 }">
                    ₺{{ selectedVisit?.creditAmountTl || 0 }}
                  </span>
                </div>
                <div class="fin-item">
                  <span class="fin-label">Tahsilat</span>
                  <span class="fin-val success">₺{{ collectedShown }}</span>
                </div>
              </div>
              <div class="fin-actions">
                <button class="btn-outline-sm" @click="creditEditOpen = !creditEditOpen">Veresiye</button>
                <button class="btn-outline-sm" @click="collectedEditOpen = !collectedEditOpen">Tahsilat</button>
              </div>
              
              <!-- Hızlı Tahsilat/Veresiye Editörleri -->
              <div v-if="creditEditOpen || collectedEditOpen" class="quick-edit-box">
                <div v-if="creditEditOpen" class="edit-field">
                  <input v-model="creditAmount" type="number" placeholder="Veresiye tutarı..." class="modern-input" />
                  <button class="btn-primary-sm" @click="saveCredit" :disabled="savingCredit">Kaydet</button>
                </div>
                <div v-if="collectedEditOpen" class="edit-field">
                  <input v-model="collectedInput" type="text" placeholder="Tahsilat tutarı..." class="modern-input" />
                  <button class="btn-primary-sm" @click="saveCollected" :disabled="collectedSaving">Kaydet</button>
                </div>
              </div>
            </div>

            <!-- GÖRSEL GALERİSİ -->
            <div class="gallery-section">
              <div class="section-header-row">
                <label>Görseller ({{ visitImages.length }})</label>
                <label class="upload-link">
                  Ekle +
                  <input type="file" multiple accept="image/*" @change="onVisitImagesSelected" style="display:none" />
                </label>
              </div>
              <div v-if="visitImages.length" class="thumb-grid">
                <div v-for="(img, idx) in visitImages" :key="img.id || idx" class="thumb-wrapper" @click="activeImageIndex = idx; openImageModal()">
                  <img :src="img.imageUrl.startsWith('http') ? img.imageUrl : API_BASE + img.imageUrl" />
                </div>
              </div>
              <p v-else class="empty-hint">Bu ziyarete ait görsel yok.</p>
            </div>

            <!-- 2.5 GELECEK RANDEVULAR (PLANS) -->
            <div v-if="selectedVisit?.plans?.length" class="modal-section plans-section" style="margin-top: 2rem;">
              <h3 class="section-subtitle">Gelecek Randevular</h3>
              <div class="plans-list" style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-top: 1rem;">
                <div v-for="plan in selectedVisit.plans" :key="plan.id" class="plan-card" style="background: #f8fafc; padding: 1rem; border-radius: 16px; border: 1px solid #e2e8f0;">
                  <template v-if="editingPlanId !== plan.id">
                    <div style="display: flex; justify-content: space-between; align-items: flex-start;">
                      <div>
                        <div style="font-weight: 800; color: var(--primary);">{{ plan.date }}</div>
                        <div style="font-size: 0.9rem; margin-top: 0.25rem;">{{ plan.purpose || 'Kontrol' }}</div>
                      </div>
                      <button 
                        v-if="selectedVisit?.ownerPhone" 
                        class="btn-whatsapp-icon-sm" 
                        title="WhatsApp ile Hatırlat"
                        @click="sendWhatsAppReminder(selectedVisit.ownerName, selectedVisit.ownerPhone, selectedVisit.petName, plan.date, plan.purpose)"
                      >
                        <svg viewBox="0 0 24 24" class="wp-icon" fill="currentColor">
                          <path d="M.057 24l1.687-6.163c-1.041-1.804-1.588-3.849-1.587-5.946C.06 5.348 5.397.01 12.008.01c3.202.001 6.212 1.246 8.477 3.514 2.266 2.268 3.507 5.28 3.505 8.484-.004 6.657-5.34 11.997-11.953 11.997-2.005-.001-3.973-.502-5.724-1.455L0 24zm6.59-4.846c1.6.95 3.197 1.451 4.819 1.452 5.485 0 9.94-4.447 9.943-9.923.002-2.652-1.031-5.147-2.907-7.027-1.878-1.88-4.376-2.914-7.026-2.915-5.486 0-9.941 4.448-9.944 9.927-.001 1.785.484 3.528 1.408 5.048L1.1 21.09l4.547-1.192-.04.024zm10.154-7.587c-.244-.122-1.442-.712-1.666-.793-.223-.08-.386-.122-.549.122-.163.243-.63.793-.772.955-.143.162-.285.182-.529.06-2.023-1.009-3.342-2.07-4.686-4.37-.354-.606-.035-.93.266-1.23.271-.27.549-.64.67-.89.12-.25.06-.47-.03-.65-.09-.18-.549-1.32-.752-1.81-.197-.474-.396-.41-.549-.418-.143-.007-.306-.007-.468-.007-.163 0-.427.06-.65.3-.224.24-.854.83-.854 2.03s.874 2.35 1.002 2.51c.122.16 1.7 2.59 4.12 3.64.57.25 1.02.4 1.37.5.58.18 1.1.16 1.52.1.47-.07 1.442-.59 1.646-1.16.204-.57.204-1.06.142-1.16-.06-.1-.22-.16-.47-.28z"/>
                        </svg>
                      </button>
                    </div>
                    <button class="btn btn-ghost btn-xs" @click="startEditPlan(plan)" style="margin-top: 0.5rem; width: 100%;">Düzenle</button>
                  </template>
                  <template v-else>
                    <div class="plan-edit-form" style="display: flex; flex-direction: column; gap: 0.5rem;">
                      <input type="date" v-model="planDraft.date" class="modern-input" />
                      <input type="text" v-model="planDraft.purpose" placeholder="Amaç" class="modern-input" />
                      <div style="display: flex; gap: 0.5rem;">
                        <button class="btn btn-text btn-xs" @click="editingPlanId = null" style="flex: 1;">İptal</button>
                        <button class="btn btn-primary-sm btn-xs" @click="savePlanEdit(plan.id)" style="flex: 1;">Kaydet</button>
                        <button class="btn btn-danger-sm btn-xs" @click="deleteAppointment(plan.id)" title="Sil">✕</button>
                      </div>
                    </div>
                  </template>
                </div>
              </div>
            </div>
          </section>

          <!-- 3. YENİ RANDEVU FORMU -->
          <section class="modal-section appointment-form-wrapper">
            <div class="form-header-premium" @click="showNewAppointment = !showNewAppointment">
              <div class="header-left">
                <div class="icon-box">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 4v16m8-8H4"/></svg>
                </div>
                <h3>Yeni Randevu Oluştur</h3>
              </div>
              <svg :class="{ 'rotated': showNewAppointment }" class="toggle-chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M19 9l-7 7-7-7"/></svg>
            </div>

            <transition name="expand">
              <div v-if="showNewAppointment" class="form-body-premium">
                <div class="form-grid-modern">
                  <!-- Tarih & Saat -->
                  <div class="form-group split">
                    <div class="field-item">
                      <label>Randevu Tarihi</label>
                      <input type="date" v-model="appointmentDate" class="premium-input" />
                    </div>
                    <div class="field-item">
                      <label>Saat</label>
                      <input type="time" v-model="appointmentTime" class="premium-input" />
                    </div>
                  </div>

                  <!-- Hasta Sahibi Arama -->
                  <div class="form-group">
                    <label>Hasta Sahibi</label>
                    <div class="search-container">
                      <div class="search-input-wrapper">
                        <svg class="search-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><path d="M21 21l-4.35-4.35"/></svg>
                        <input 
                          type="text" 
                          v-model="ownerQuery" 
                          placeholder="İsim veya telefon ile ara..." 
                          class="premium-input has-icon"
                          @input="onOwnerQueryInput"
                          @focus="ownerSearchOpen = true"
                        />
                      </div>
                      <div v-if="ownerSearchOpen && ownerResults.length" class="premium-dropdown">
                        <div v-for="o in ownerResults" :key="o.id" class="dropdown-option" @click="selectOwner(o)">
                          <div class="option-main">{{ o.fullName }}</div>
                          <div class="option-sub">{{ o.phone }}</div>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Hayvan Seçimi -->
                  <div class="form-group">
                    <label>Hayvan(lar)</label>
                    <div class="pet-selection-grid">
                      <div v-for="pet in ownerPets" :key="pet.id" class="pet-toggle-card" :class="{ 'is-selected': selectedPetId === pet.id }">
                        <input type="radio" :id="'pet-cb-'+pet.id" :value="pet.id" v-model="selectedPetId" />
                        <label :for="'pet-cb-'+pet.id">
                          <span class="pet-icon">🐾</span>
                          <span class="pet-name-label">{{ pet.name }}</span>
                        </label>
                      </div>
                      <div v-if="!ownerPets.length && selectedOwnerId" class="empty-state-inline">Bu sahibe ait hayvan bulunamadı.</div>
                      <div v-if="!selectedOwnerId" class="empty-state-inline">Önce hasta sahibi seçin.</div>
                    </div>
                  </div>

                  <!-- Neden & Doktor -->
                  <div class="form-group">
                    <label>Randevu Nedeni</label>
                    <textarea v-model="appointmentPurpose" class="premium-input" rows="2" placeholder="Örn: Karma aşı, genel kontrol..."></textarea>
                  </div>

                  <div class="form-group">
                    <label>Görevli Doktor</label>
                    <div class="select-wrapper">
                      <select v-model="selectedDoctorId" class="premium-input">
                        <option :value="null">Doktor Seçilmedi</option>
                        <option v-for="doc in doctors" :key="doc.id" :value="doc.id">{{ doc.fullName }}</option>
                      </select>
                    </div>
                  </div>

                  <!-- NEW: Financial & Clinical Info -->
                  <div class="form-divider"><span>Klinik & Finansal (Opsiyonel)</span></div>

                  <div class="form-group">
                    <label>Yapılan İşlemler</label>
                    <div class="procedure-pills-container">
                      <button
                        v-for="pill in predefinedProcedures"
                        :key="pill"
                        type="button"
                        class="pill-select-btn"
                        :class="{ active: isProcedureSelected(pill, appointmentProcedures) }"
                        @click="toggleAppointmentProcedure(pill)"
                      >
                        {{ pill }}
                      </button>
                    </div>
                    <textarea v-model="appointmentProcedures" class="premium-input" rows="2" placeholder="Aşı, parazit, tıraş vb."></textarea>
                  </div>

                  <div class="form-group split">
                    <div class="field-item">
                      <label>Toplam Tutar (TL)</label>
                      <input type="number" v-model.number="appointmentAmount" class="premium-input" placeholder="0" />
                    </div>
                    <div class="field-item">
                      <label>Alınan Nakit/Kart (TL)</label>
                      <input type="number" v-model.number="appointmentPaid" class="premium-input" placeholder="0" />
                    </div>
                  </div>

                  <div class="form-group" v-if="appointmentCreditCalc > 0">
                    <div class="debt-warning">
                      <span>Kalan Borç (Veresiye):</span>
                      <strong>₺{{ appointmentCreditCalc }}</strong>
                    </div>
                  </div>

                  <div class="form-group">
                    <label>Ziyaret Notları</label>
                    <textarea v-model="appointmentNotes" class="premium-input" rows="2" placeholder="Özel notlar..."></textarea>
                  </div>

                  <div class="form-group">
                    <label>Görsel(ler) Ekle</label>
                    <div class="image-upload-zone">
                      <input type="file" multiple accept="image/*" @change="onAppointmentImagesSelected" id="app-img-input" style="display:none" />
                      <label for="app-img-input" class="upload-trigger">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 4v16m8-8H4"/></svg>
                        <span>{{ appointmentFiles.length > 0 ? appointmentFiles.length + ' Görsel Seçildi' : 'Görsel Seç...' }}</span>
                      </label>
                      <div v-if="appointmentFiles.length" class="selected-files-list">
                        <span v-for="(f, i) in appointmentFiles" :key="i" class="file-tag">{{ f.name }} <i @click.stop="removeAppFile(i)">✕</i></span>
                      </div>
                    </div>
                  </div>
                </div>

                <div class="form-footer-actions">
                  <button class="btn-secondary-modern" @click="showNewAppointment = false">Vazgeç</button>
                  <button class="btn-primary-modern" @click="submitAppointment" :disabled="appointmentSaving">
                    <span v-if="!appointmentSaving">Randevuyu Onayla</span>
                    <span v-else>Kaydediliyor...</span>
                  </button>
                </div>
              </div>
            </transition>
          </section>
        </div>
      </div>
    </div>
  </div>

  <!-- TAM EKRAN GÖRSEL MODAL -->
  <div v-if="showImageModal" class="image-modal-overlay" @click.self="closeImageModal">
    <div class="image-viewer">
      <img :src="visitImageSrc" />
      <button class="viewer-close" @click="closeImageModal">✕</button>
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

const editingPlanId = ref(null)
const planDraft = reactive({ date: '', purpose: '' })

function startEditPlan(plan) {
  editingPlanId.value = plan.id
  planDraft.date = plan.date
  planDraft.purpose = plan.purpose
}

async function savePlanEdit(planId) {
  try {
    const payload = {
      scheduledAt: new Date(planDraft.date).toISOString(), 
      purpose: planDraft.purpose,
      doctorId: selectedVisit.value.doctorId
    }
    await http.put(`/appointments/${planId}`, payload)
    
    // Refresh
    const visitId = selectedVisit.value?.id || selectedVisit.value?.Id
    const res = await fetchVisitDetail(visitId)
    selectedVisit.value = res.data ?? res
    editingPlanId.value = null
    
    await loadCalendarForMonth(currentMonth.value)
  } catch (err) {
    console.error(err)
    alert('Randevu güncellenirken hata oluştu.')
  }
}

async function deleteAppointment(id) {
  if (!confirm('Bu randevuyu silmek istediğinize emin misiniz?')) return
  try {
    await http.delete(`/appointments/${id}`)
    
    // Refresh
    const visitId = selectedVisit.value?.id || selectedVisit.value?.Id
    if (visitId) {
      const res = await fetchVisitDetail(visitId)
      selectedVisit.value = res.data ?? res
    }
    editingPlanId.value = null
    await loadCalendarForMonth(currentMonth.value)
  } catch (err) {
    console.error(err)
    alert('Randevu silinemedi.')
  }
}

async function handleDeleteVisit() {
  const visitId = selectedVisit.value?.id || selectedVisit.value?.Id
  if (!visitId) return
  if (!confirm('Bu ziyareti ve buna bağlı tüm kasa kayıtlarını SİLMEK istediğinize emin misiniz? Bu işlem geri alınamaz.')) return
  
  visitSaving.value = true
  try {
    await http.delete(`/visits/${visitId}`)
    closeDetail()
    await loadStats()
    await loadCalendarForMonth(currentMonth.value)
  } catch (err) {
    console.error(err)
    alert('Ziyaret silinirken hata oluştu.')
  } finally {
    visitSaving.value = false
  }
}

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
const selectedPetId = ref(null)
const appointmentMode = ref('multiple')
const appointmentAmount = ref(null)
const appointmentPaid = ref(null)
const appointmentCreditCalc = computed(() => Math.max(0, (appointmentAmount.value || 0) - (appointmentPaid.value || 0)))
const appointmentFiles = ref([])
const appointmentProcedures = ref('')
const appointmentNotes = ref('')
const showSuccess = ref(false)

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

const predefinedProcedures = [
  'İlaç A',
  'İlaç B',
  'İlaç C',
  'Aşı A',
  'Aşı B',
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

function toggleAppointmentProcedure(pill) {
  let currentVal = appointmentProcedures.value || ''
  let items = currentVal.split(',').map(i => i.trim()).filter(Boolean)
  const idx = items.findIndex(i => i.toLowerCase() === pill.toLowerCase())
  if (idx > -1) {
    items.splice(idx, 1)
  } else {
    items.push(pill)
  }
  appointmentProcedures.value = items.join(', ')
}

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
  const dtLocal = performedAt ? String(performedAt).slice(0, 16) : ''

  return {
    performedAt: dtLocal,
    microchipNumber: v.microchipNumber ?? v.MicrochipNumber ?? '',
    procedures: v.procedures ?? v.Procedures ?? '',
    notes: v.notes ?? v.Notes ?? '',
    amountTl: v.amountTl ?? v.AmountTl ?? null,
    creditAmountTl: v.creditAmountTl ?? v.CreditAmountTl ?? 0,
    
    // Pet & Owner Info for editing
    petName: v.petName ?? v.PetName ?? '',
    petSpecies: v.petSpecies ?? v.PetSpecies ?? v.species ?? v.Species ?? '',
    ownerName: v.ownerName ?? v.OwnerName ?? '',
    ownerPhone: v.ownerPhone ?? v.OwnerPhone ?? v.phoneE164 ?? v.PhoneE164 ?? '',

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
    // 1) Update Pet if changed
    const petId = selectedVisit.value?.petId || selectedVisit.value?.PetId
    if (petId) {
      const petPayload = {
        name: visitDraft.value.petName,
        species: visitDraft.value.petSpecies,
        // Diğer alanları korumak için mevcutları gönderiyoruz (Backend DTO gereği)
        breed: selectedVisit.value.breed || selectedVisit.value.Breed,
        birthDate: selectedVisit.value.birthDate || selectedVisit.value.BirthDate,
        notes: selectedVisit.value.petNotes || selectedVisit.value.PetNotes || ''
      }
      await http.put(`/pets/${petId}`, petPayload)
    }

    // 2) Update Owner if changed
    const ownerId = selectedVisit.value?.ownerId || selectedVisit.value?.OwnerId
    if (ownerId) {
      const ownerPayload = {
        fullName: visitDraft.value.ownerName,
        phoneE164: visitDraft.value.ownerPhone,
        email: selectedVisit.value.ownerEmail || selectedVisit.value.OwnerEmail,
        address: selectedVisit.value.ownerAddress || selectedVisit.value.OwnerAddress,
        kvkkOptIn: true
      }
      await http.put(`/owners/${ownerId}`, ownerPayload)
    }

    // 3) Update Visit
    const rawAmount = visitDraft.value.amountTl
    const amount =
      rawAmount === '' || rawAmount == null
        ? null
        : Number(String(rawAmount).replace(',', '.'))

    const performedAtLocal = (visitDraft.value.performedAt ?? '').toString().trim()
    if (!performedAtLocal) {
      visitSaveError.value = 'Yapılan işlem tarihi zorunludur.'
      visitSaving.value = false
      return
    }
    const performedAt = performedAtLocal.length === 16 ? `${performedAtLocal}:00` : performedAtLocal

    const payload = {
      performedAt,
      microchipNumber: (visitDraft.value.microchipNumber || '').trim() || null,
      procedures: (visitDraft.value.procedures || '').trim() || null,
      notes: (visitDraft.value.notes || '').trim() || null,
      amountTl: amount,
      creditAmountTl: visitDraft.value.creditAmountTl,
      nextDate: visitDraft.value.nextDate ?? (selectedVisit.value?.nextDate ?? selectedVisit.value?.NextDate ?? null),
      purpose: (visitDraft.value.purpose || '').trim() || null,
      plans: (visitDraft.value.plans || '').trim() || null,
    }

    await http.put(`/visits/${visitId}`, payload)

    // detail refresh
    const res = await fetchVisitDetail(visitId)
    const fresh = res?.data ?? res
    selectedVisit.value = fresh
    visitDetail.value = fresh

    await loadStats()
    await loadCalendarForMonth(currentMonth.value)

    visitEditOpen.value = false
    visitDraft.value = null
  } catch (e) {
    console.error('[VISIT_EDIT] save error', e)
    const msg = e?.response?.data
    visitSaveError.value = typeof msg === 'string' ? msg : (msg?.message || 'Güncelleme sırasında bir hata oluştu.')
  } finally {
    visitSaving.value = false
  }
}


async function onAppointmentImagesSelected(e) {
  const files = e?.target?.files
  if (!files) return
  appointmentFiles.value = Array.from(files)
}

function removeAppFile(idx) {
  appointmentFiles.value.splice(idx, 1)
}

async function submitAppointment() {
  if (appointmentSaving.value) return
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
  if (!selectedPetId.value) {
    alert('Lütfen bir hayvan seçin.')
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

  appointmentSaving.value = true
  const formData = new FormData()
  formData.append('OwnerId', selectedOwnerId.value)
  formData.append('PetId', selectedPetId.value)
  formData.append('ScheduledAt', `${appointmentDate.value}T${appointmentTime.value}`)
  formData.append('Purpose', appointmentPurpose.value)
  formData.append('DoctorId', selectedDoctorId.value || '')
  formData.append('Procedures', appointmentProcedures.value)
  formData.append('AmountTl', appointmentAmount.value || 0)
  formData.append('PaidAmountTl', appointmentPaid.value || 0)
  formData.append('Notes', appointmentNotes.value)
  appointmentFiles.value.forEach(f => formData.append('Images', f))

  try {
    await http.post('/appointments', formData, { headers: { 'Content-Type': 'multipart/form-data' } })
    
    // Bilgilendirme ekranını göster
    showSuccess.value = true
    
    // 2 saniye sonra her şeyi kapat
    setTimeout(() => {
      showSuccess.value = false
      closeDetail() // Tüm modalı kapatır
    }, 2000)

    await loadStats()
    await loadCalendarForMonth(currentMonth.value)
    
    // Temizlik
    appointmentFiles.value = []
    appointmentAmount.value = null
    appointmentPaid.value = null
    appointmentProcedures.value = ''
    appointmentNotes.value = ''
  } catch(e) {
    console.error(e)
    alert('Randevu kaydedilemedi.')
  } finally {
    appointmentSaving.value = false
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

  selectedPetId.value = null
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
  selectedPetId.value = null
  appointmentMode.value = 'multiple'
  ownerPets.value = []
  selectedOwnerId.value = null
  selectedOwnerLabel.value = ''
  ownerResults.value = []
  form.microchipNumber = ''
  
  // Yeni alanları temizle
  appointmentProcedures.value = ''
  appointmentAmount.value = null
  appointmentPaid.value = null
  appointmentNotes.value = ''
  appointmentFiles.value = []
  
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

  // Normalize data (handle both PascalCase and camelCase from backend)
  const normalized = (appointments || []).map(a => ({
    id: a.id ?? a.Id,
    visitId: a.visitId ?? a.VisitId,
    scheduledAt: a.scheduledAt ?? a.ScheduledAt,
    isVisit: a.isVisit ?? a.IsVisit ?? false,
    petName: a.petName ?? a.PetName ?? '—',
    ownerName: a.ownerName ?? a.OwnerName ?? '',
    purpose: a.purpose ?? a.Purpose ?? ''
  })).filter(a => a.scheduledAt)

  const byDate = {}
  normalized.forEach((a) => {
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

function sendWhatsAppReminder(ownerName, ownerPhone, petName, dateStr, purpose) {
  if (!ownerPhone) {
    alert('Müşterinin telefon numarası kayıtlı değil.');
    return;
  }
  const cleanPhone = ownerPhone.replace(/\D/g, '');
  
  let displayDate = dateStr || 'Belirtilmemiş Tarih';
  if (displayDate.includes('T') || (displayDate.includes('-') && displayDate.length >= 10)) {
    try {
      const dt = new Date(displayDate);
      if (!isNaN(dt.getTime())) {
        displayDate = dt.toLocaleString('tr-TR', {
          day: '2-digit',
          month: '2-digit',
          year: 'numeric',
          hour: '2-digit',
          minute: '2-digit'
        });
      }
    } catch (e) {
      console.error(e);
    }
  }

  const message = `Merhaba ${ownerName}, BullVet Veteriner Kliniği'nden hatırlatma: ${petName} isimli dostumuzun ${displayDate} tarihindeki "${purpose || 'Kontrol'}" randevusu/işlemi yaklaşmaktadır. Bilgi almak veya değişiklik yapmak için bizimle iletişime geçebilirsiniz. Sağlıklı günler dileriz!`;
  const url = `https://wa.me/${cleanPhone}?text=${encodeURIComponent(message)}`;
  window.open(url, '_blank');
}
</script>

<style scoped>
/* MODAL OVERLAY & CONTAINER */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
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
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  animation: modalScaleUp 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}

@keyframes modalScaleUp {
  from { opacity: 0; transform: scale(0.9) translateY(20px); }
  to { opacity: 1; transform: scale(1) translateY(0); }
}

/* HEADER */
.modal-header {
  padding: 1.5rem 2rem;
  background: #ffffff;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
  position: sticky;
  top: 0;
  z-index: 10;
}

.header-info h2 {
  font-size: 1.25rem;
  font-weight: 800;
  color: #0f172a;
  display: flex;
  flex-direction: column;
}

.pet-name { color: var(--primary); }
.owner-name { font-size: 0.85rem; color: #64748b; font-weight: 500; }

.modal-close-btn {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  border: none;
  background: #f1f5f9;
  color: #64748b;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.modal-close-btn:hover {
  background: #fee2e2;
  color: #ef4444;
  transform: rotate(90deg);
}

.modal-close-btn svg { width: 20px; height: 20px; }

/* CONTENT WRAPPER */
.modal-content-wrapper {
  flex: 1;
  overflow-y: auto;
  padding: 1.5rem 2rem;
  scrollbar-width: thin;
  scrollbar-color: #e2e8f0 transparent;
}

.modal-section {
  margin-bottom: 2rem;
}

.section-subtitle {
  font-size: 0.95rem;
  font-weight: 700;
  color: #334155;
  margin-bottom: 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.section-subtitle svg { width: 18px; height: 18px; color: var(--primary); }

/* APPOINTMENTS LIST */
.mini-event-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.mini-event-card {
  background: #f8fafc;
  padding: 1rem;
  border-radius: 16px;
  display: flex;
  align-items: center;
  gap: 1rem;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.mini-event-card:hover {
  background: #ffffff;
  border-color: var(--primary-light);
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}

.ev-time {
  font-weight: 800;
  color: var(--primary);
  background: #eef2ff;
  padding: 0.4rem 0.75rem;
  border-radius: 10px;
  font-size: 0.85rem;
}

.ev-main { flex: 1; display: flex; flex-direction: column; }
.ev-pet { font-weight: 700; color: #1e293b; font-size: 0.95rem; }
.ev-purpose { font-size: 0.8rem; color: #64748b; }
.chevron { width: 16px; height: 16px; color: #cbd5e1; }

/* VISIT DETAIL GRID */
.detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.25rem;
  background: #ffffff;
  border-radius: 20px;
}

.detail-item { display: flex; flex-direction: column; gap: 0.4rem; }
.detail-item.full { grid-column: span 2; }

.detail-item label {
  font-size: 0.75rem;
  font-weight: 700;
  color: #94a3b8;
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

.event-pill.is-visit {
  background: #10b981 !important;
  box-shadow: 0 2px 4px rgba(16, 185, 129, 0.2) !important;
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

/* --- NEW PREMIUM APPOINTMENT FORM STYLES --- */
.appointment-form-wrapper {
  background: #f8fafc;
  border-radius: 20px;
  border: 1px solid #e2e8f0;
  margin-top: 1rem;
  overflow: hidden;
}

.form-header-premium {
  padding: 1.25rem 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  background: #ffffff;
  border-bottom: 1px solid transparent;
}

.form-header-premium:hover { background: #f1f5f9; }

.header-left { display: flex; align-items: center; gap: 0.75rem; }
.icon-box {
  width: 32px;
  height: 32px;
  background: #eef2ff;
  color: #4f46e5;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}
.icon-box svg { width: 18px; height: 18px; }

.header-left h3 { font-size: 1rem; font-weight: 700; color: #1e293b; margin: 0; }

.toggle-chevron {
  width: 20px !important;
  height: 20px !important;
  color: #94a3b8;
  transition: transform 0.3s;
}
.toggle-chevron.rotated { transform: rotate(180deg); }

.form-body-premium { padding: 1.5rem; background: #ffffff; border-top: 1px solid #f1f5f9; }

.form-grid-modern { display: flex; flex-direction: column; gap: 1.25rem; }

.form-group label {
  display: block;
  font-size: 0.75rem;
  font-weight: 800;
  color: #64748b;
  text-transform: uppercase;
  margin-bottom: 0.5rem;
}

.premium-input {
  width: 100%;
  padding: 0.85rem 1rem;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  background: #ffffff;
  font-size: 0.95rem;
  transition: all 0.2s;
}

.premium-input:focus {
  outline: none;
  border-color: #4f46e5;
  box-shadow: 0 0 0 4px rgba(79, 70, 229, 0.1);
}

.pet-selection-grid { display: flex; flex-wrap: wrap; gap: 0.5rem; margin-top: 0.5rem; }

.pet-toggle-card { position: relative; }
.pet-toggle-card input { position: absolute; opacity: 0; }
.pet-toggle-card label {
  display: block;
  padding: 0.5rem 1rem;
  background: #ffffff;
  border: 1.5px solid #e2e8f0;
  border-radius: 12px;
  cursor: pointer;
  font-size: 0.85rem;
  font-weight: 700;
  color: #475569;
}

.pet-toggle-card.is-selected label {
  background: #eef2ff;
  border-color: #4f46e5;
  color: #4f46e5;
}

.form-footer-actions { display: flex; gap: 1rem; margin-top: 2rem; }

.btn-primary-modern {
  flex: 2;
  padding: 0.85rem;
  background: #4f46e5;
  color: #ffffff;
  border: none;
  border-radius: 12px;
  font-weight: 700;
  cursor: pointer;
  box-shadow: 0 4px 6px rgba(79, 70, 229, 0.2);
}

.btn-secondary-modern {
  flex: 1;
  padding: 0.85rem;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  font-weight: 700;
  color: #64748b;
  cursor: pointer;
}

/* HIGHLIGHTS & ACTIVITY CHART */
.highlights-grid {
  margin-bottom: 2.5rem;
}

.highlight-card.activity-chart {
  background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
  padding: 2rem;
  border-radius: 24px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.02);
  position: relative;
  overflow: hidden;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
}

.chart-header h3 {
  font-size: 1.1rem;
  font-weight: 800;
  color: #1e293b;
  margin: 0;
}

.trend-up { color: #10b981; font-weight: 700; font-size: 0.9rem; background: #ecfdf5; padding: 0.25rem 0.75rem; border-radius: 20px; }
.trend-down { color: #ef4444; font-weight: 700; font-size: 0.9rem; background: #fef2f2; padding: 0.25rem 0.75rem; border-radius: 20px; }

.svg-wrapper {
  height: 120px;
  width: 100%;
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
  margin-top: 1rem;
  padding: 0 0.5rem;
}

.chart-labels span {
  font-size: 0.7rem;
  font-weight: 700;
  color: #94a3b8;
  text-transform: uppercase;
}

/* SEARCH DROPDOWN & INPUT FIX */
.search-container { position: relative; width: 100%; }
.search-input-wrapper { position: relative; width: 100%; }
.search-icon {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  width: 18px !important;
  height: 18px !important;
  color: #94a3b8;
  pointer-events: none;
  z-index: 5;
}
.premium-input.has-icon {
  padding-left: 2.75rem !important;
}

.premium-dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: white;
  border-radius: 12px;
  box-shadow: 0 10px 25px rgba(0,0,0,0.1);
  z-index: 100;
  margin-top: 0.5rem;
  max-height: 200px;
  overflow-y: auto;
  border: 1px solid #e2e8f0;
}

.dropdown-option {
  padding: 0.75rem 1rem;
  display: flex;
  flex-direction: column;
  cursor: pointer;
  border-bottom: 1px solid #f1f5f9;
}
.dropdown-option:hover { background: #f8fafc; }
.option-main { font-weight: 700; color: #1e293b; }
.option-sub { font-size: 0.75rem; color: #64748b; }

.form-divider {
  grid-column: 1 / -1;
  display: flex;
  align-items: center;
  gap: 1rem;
  margin: 1.5rem 0 1rem;
}

.form-divider span {
  font-size: 0.75rem;
  font-weight: 700;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  white-space: nowrap;
}

.form-divider::after {
  content: '';
  flex: 1;
  height: 1px;
  background: #f1f5f9;
}

.debt-warning {
  background: #fff7ed;
  border: 1px solid #ffedd5;
  padding: 0.75rem 1rem;
  border-radius: 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.debt-warning span { font-size: 0.85rem; color: #9a3412; font-weight: 600; }
.debt-warning strong { color: #c2410c; font-size: 1rem; }

.image-upload-zone {
  margin-top: 0.5rem;
}

.upload-trigger {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  background: #f8fafc;
  border: 2px dashed #e2e8f0;
  padding: 1rem;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.2s;
  color: #64748b;
  font-weight: 600;
}

.upload-trigger:hover {
  background: #f1f5f9;
  border-color: var(--primary);
  color: var(--primary);
}

.upload-trigger svg { width: 20px; height: 20px; }

.selected-files-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-top: 1rem;
}

.file-tag {
  background: #eef2ff;
  color: #4f46e5;
  padding: 0.35rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.file-tag i {
  cursor: pointer;
  font-style: normal;
  opacity: 0.6;
}

.file-tag i:hover { opacity: 1; }

/* SUCCESS STATE */
.success-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  text-align: center;
  animation: fadeIn 0.3s ease;
}

.success-icon-wrapper {
  width: 80px;
  height: 80px;
  background: #ecfdf5;
  color: #10b981;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 1.5rem;
  animation: scaleBounce 0.5s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.success-icon-wrapper svg { width: 40px; height: 40px; }

.success-state h3 { font-size: 1.5rem; font-weight: 800; color: #064e3b; margin-bottom: 0.5rem; }
.success-state p { color: #065f46; opacity: 0.8; }

@keyframes scaleBounce {
  0% { transform: scale(0); }
  60% { transform: scale(1.2); }
  100% { transform: scale(1); }
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
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

/* WHATSAPP REMINDER BUTTONS */
.btn-whatsapp-icon {
  background: #25D366;
  color: white;
  border: none;
  border-radius: 50%;
  width: 28px;
  height: 28px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  margin-left: 0.5rem;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 2px 5px rgba(37, 211, 102, 0.3);
  vertical-align: middle;
}
.btn-whatsapp-icon:hover {
  background: #20ba5a;
  transform: scale(1.15) rotate(5deg);
  box-shadow: 0 4px 10px rgba(37, 211, 102, 0.5);
}
.btn-whatsapp-icon .wp-icon {
  width: 16px;
  height: 16px;
}

.btn-whatsapp-icon-sm {
  background: #25D366;
  color: white;
  border: none;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 2px 4px rgba(37, 211, 102, 0.3);
  flex-shrink: 0;
  margin-left: 0.5rem;
}
.btn-whatsapp-icon-sm:hover {
  background: #20ba5a;
  transform: scale(1.15) rotate(5deg);
  box-shadow: 0 3px 8px rgba(37, 211, 102, 0.5);
}
.btn-whatsapp-icon-sm .wp-icon {
  width: 13px;
  height: 13px;
}
</style>
