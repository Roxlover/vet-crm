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
            <span class="search-icon">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="11" cy="11" r="8"></circle>
                <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
              </svg>
            </span>
            <input 
              v-model="searchQuery" 
              type="text" 
              placeholder="İsim veya telefon ile ara..." 
              @input="handleSearch"
            />
          </div>
          <button class="refresh-btn" @click="loadOwners" :disabled="loading" title="Yenile">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="23 4 23 10 17 10"></polyline>
              <path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"></path>
            </svg>
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

          <div class="form-group">
            <label for="password">Müşteri Portalı Şifresi</label>
            <input id="password" v-model="form.password" type="password" placeholder="Müşteri giriş şifresi (isteğe bağlı)" />
            <small class="hint" style="margin-top: 0.5rem; display: block;">Boş bırakılırsa hasta sahibi portala giriş yapamaz.</small>
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

                <div class="form-group" style="margin-bottom: 0;">
                  <label>Yaş (Yıl)</label>
                  <input v-model.number="pet.ageYears" type="number" placeholder="0" />
                </div>

                <div class="form-group" style="margin-bottom: 0;">
                  <label>Yaş (Ay)</label>
                  <input v-model.number="pet.ageMonths" type="number" placeholder="0" />
                </div>

                <div class="form-group full-width" style="margin-bottom: 0; grid-column: span 2;">
                  <label>Mikroçip No</label>
                  <input v-model="pet.microchipNumber" type="text" placeholder="Mikroçip numarası" />
                </div>

                <div class="form-group full-width" style="margin-bottom: 0; grid-column: span 2;">
                  <label>Pet Hakkında Notlar</label>
                  <textarea v-model="pet.notes" placeholder="Mizaç, alerji vb..." class="mini-textarea"></textarea>
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
        <button class="modal-close-btn" @click="closeOwnerDetail">Kapat</button>
        
        <div v-if="detailLoading" class="state">Yükleniyor...</div>
        
        <div v-else-if="ownerDetail" class="modal-content">
          <header class="modal-header-section">
            <div class="owner-main-info" style="text-align: center; width: 100%;">
              <template v-if="!ownerEditOpen">
                <h2>{{ ownerDetail.fullName }}</h2>
                <div class="contact-pill">
                  <span>Telefon: {{ ownerDetail.phoneE164 }}</span>
                  <button class="btn btn-ghost btn-xs" @click="openOwnerEdit" style="margin-left: 1rem;">Düzenle</button>
                  <button class="btn btn-danger-sm btn-xs" @click="handleDeleteOwner" style="margin-left: 0.5rem;">Sil</button>
                </div>
              </template>
              <template v-else>
                <div class="owner-edit-form" style="max-width: 400px; margin: 0 auto; display: flex; flex-direction: column; gap: 0.5rem;">
                  <input v-model="ownerDraft.fullName" class="modern-input" placeholder="Ad Soyad" />
                  <input v-model="ownerDraft.phoneE164" class="modern-input" placeholder="Telefon" />
                  <input v-model="ownerDraft.password" class="modern-input" type="password" placeholder="Yeni Şifre (Değiştirmek istemiyorsanız boş bırakın)" />
                  <div style="display: flex; gap: 0.5rem; justify-content: center; margin-top: 0.5rem;">
                    <button class="btn btn-text btn-sm" @click="cancelOwnerEdit">İptal</button>
                    <button class="btn btn-primary-sm" @click="saveOwnerEdit" :disabled="ownerSaving">Kaydet</button>
                  </div>
                </div>
              </template>
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
                
                <div class="note-input-box" style="display: flex; flex-direction: column; gap: 0.75rem; padding: 1rem; background: #f8fafc; border-radius: 12px; border: 1px solid #e2e8f0; margin-bottom: 1.5rem;">
                  <textarea 
                    v-model="noteText" 
                    placeholder="Yeni bir not ekleyin veya görsel seçin..."
                    rows="2"
                    style="width: 100%; border: 1px solid #cbd5e1; border-radius: 8px; padding: 0.5rem 0.75rem; font-family: inherit; font-size: 0.9rem; resize: vertical;"
                  ></textarea>
                  
                  <!-- Note Image Preview -->
                  <div v-if="noteImagePreviewUrl" class="note-image-preview" style="position: relative; width: 60px; height: 60px; border-radius: 8px; overflow: hidden; border: 1px solid var(--primary-light);">
                    <img :src="noteImagePreviewUrl" style="width: 100%; height: 100%; object-fit: cover;" />
                    <button type="button" @click="clearSelectedNoteImage" style="position: absolute; top: 2px; right: 2px; background: rgba(0,0,0,0.6); color: #fff; border: none; border-radius: 50%; width: 16px; height: 16px; font-size: 10px; cursor: pointer; display: flex; align-items: center; justify-content: center; line-height: 1;">✕</button>
                  </div>

                  <div style="display: flex; justify-content: space-between; align-items: center;">
                    <label class="btn btn-ghost btn-xs" style="cursor: pointer; display: inline-flex; align-items: center; gap: 0.3rem; padding: 0.4rem 0.6rem; border-radius: 8px; border: 1px solid #cbd5e1; background: #ffffff;">
                      <span style="font-size: 1.1rem; line-height: 1;">📷</span> Görsel Ekle
                      <input 
                        type="file" 
                        accept="image/*" 
                        @change="handleNoteFileChange" 
                        style="display: none;" 
                      />
                    </label>
                    
                    <button 
                      class="btn btn-primary btn-sm" 
                      @click="handleAddNote" 
                      :disabled="noteAdding || (!noteText?.trim() && !noteImageFile)"
                      style="border-radius: 8px; padding: 0.4rem 1rem;"
                    >
                      {{ noteAdding ? 'Ekleniyor...' : 'Ekle' }}
                    </button>
                  </div>
                </div>

                <div class="notes-scroll-area">
                  <div v-for="note in (ownerDetail.notes || [])" :key="note.id" class="modern-note-card">
                    <template v-if="editingNoteId !== note.id">
                      <p v-if="note.note">{{ note.note }}</p>
                      
                      <!-- Note Image View -->
                      <div v-if="note.imageUrl" class="note-image-wrapper" style="margin-top: 0.75rem; max-width: 220px; border-radius: 8px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: var(--shadow-sm); background: #f8fafc;">
                        <img 
                          :src="normalizeMediaUrl(note.imageUrl)" 
                          style="width: 100%; max-height: 160px; object-fit: cover; cursor: pointer; display: block;" 
                          @click="window.open(normalizeMediaUrl(note.imageUrl), '_blank')" 
                          title="Görseli Büyüt"
                        />
                      </div>

                      <div class="note-footer">
                        <span class="date">{{ new Date(note.createdAt).toLocaleDateString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }) }}</span>
                        <div class="note-actions">
                          <button class="btn-text-action" @click="startEditNote(note)">Düzenle</button>
                          <button class="btn-text-action delete" @click="handleDeleteNote(note.id)">Sil</button>
                        </div>
                      </div>
                    </template>
                    <template v-else>
                      <textarea v-model="noteDraft" class="mini-textarea-note" rows="2"></textarea>
                      <div class="note-edit-actions">
                        <button class="btn btn-text btn-xs" @click="cancelEditNote">İptal</button>
                        <button class="btn btn-primary-sm btn-xs" @click="saveNoteEdit" :disabled="noteSaving">Kaydet</button>
                      </div>
                    </template>
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
                    <input v-model.number="newPet.ageYears" type="number" placeholder="Yaş (Yıl)" class="mini-input" />
                    <input v-model.number="newPet.ageMonths" type="number" placeholder="Yaş (Ay)" class="mini-input" />
                    <input v-model="newPet.breed" placeholder="Cins" class="mini-input" />
                    <input v-model="newPet.birthDate" type="date" class="mini-input" />
                    <input v-model="newPet.microchipNumber" placeholder="Mikroçip No" class="mini-input" style="grid-column: span 2;" />
                    <textarea v-model="newPet.notes" placeholder="Notlar..." class="mini-input full-width" style="grid-column: span 2; min-height: 60px;"></textarea>
                  </div>
                  <button class="btn btn-primary btn-sm full-width" @click="addPet" :disabled="petAdding">
                    {{ petAdding ? 'Ekleniyor...' : 'Hayvanı Kaydet' }}
                  </button>
                  <p v-if="petAddError" class="error-text">{{ petAddError }}</p>
                </div>

                <div class="pets-mini-list">
                  <div 
                    v-for="p in (ownerDetail.pets || [])" 
                    :key="p.id" 
                    class="pet-mini-card clickable"
                    @click="goToPetProfile(p.id)"
                  >
                    <template v-if="editingPetId !== p.id">
                      <div class="p-info">
                        <strong>{{ p.name }}</strong>
                        <span>{{ p.species }} <template v-if="p.breed">({{ p.breed }})</template></span>
                      </div>
                      <div class="p-budget">
                        <span class="total-badge" title="Toplam Bütçe">Tutar: {{ p.totalAmount?.toFixed(2) }} ₺</span>
                        <span v-if="p.totalCredit > 0" class="credit-badge" title="Veresiye">Veresiye: {{ p.totalCredit?.toFixed(2) }} ₺</span>
                      </div>
                      <div class="p-actions" style="display: flex; gap: 0.5rem;">
                        <button class="btn btn-ghost btn-xs" @click.stop="startEditPet(p)">Düzenle</button>
                        <button class="delete-icon-btn" @click.stop="removePet(p.id)">Sil</button>
                      </div>
                    </template>
                    <template v-else>
                      <div class="pet-inline-edit" @click.stop style="width: 100%; display: flex; flex-direction: column; gap: 0.5rem;">
                        <input v-model="petDraft.name" placeholder="Ad" class="mini-input" />
                        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 0.5rem;">
                          <input v-model="petDraft.species" placeholder="Tür" class="mini-input" />
                          <input v-model="petDraft.breed" placeholder="Cins" class="mini-input" />
                        </div>
                        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 0.5rem;">
                          <input v-model.number="petDraft.ageYears" type="number" placeholder="Yaş (Yıl)" class="mini-input" min="0" />
                          <input v-model.number="petDraft.ageMonths" type="number" placeholder="Yaş (Ay)" class="mini-input" min="0" max="11" />
                        </div>
                        <input v-model="petDraft.microchipNumber" placeholder="Mikroçip No" class="mini-input" />
                        <textarea v-model="petDraft.notes" placeholder="Notlar" class="mini-input" style="min-height: 50px;"></textarea>
                        <div style="border-top: 1px dashed #e2e8f0; padding-top: 0.5rem; margin-top: 0.25rem;">
                          <p style="font-size: 0.7rem; font-weight: 700; color: #94a3b8; text-transform: uppercase; margin-bottom: 0.4rem;">Hızlı Ücret Girişi</p>
                          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 0.5rem;">
                            <input v-model.number="petDraft.amountTl" type="number" placeholder="Tutar (₺)" class="mini-input" min="0" step="0.01" />
                            <input v-model.number="petDraft.creditAmountTl" type="number" placeholder="Veresiye (₺)" class="mini-input" min="0" step="0.01" />
                          </div>
                          <p style="font-size: 0.65rem; color: #94a3b8; margin-top: 0.3rem;">Tutar girilirse yeni bir ziyaret kaydı oluşturulur ve bilanço güncellenir.</p>
                        </div>
                        <div style="display: flex; gap: 0.5rem;">
                          <button class="btn btn-text btn-xs" @click="editingPetId = null">İptal</button>
                          <button class="btn btn-primary-sm btn-xs" @click="savePetEdit">Kaydet</button>
                        </div>
                      </div>
                    </template>
                  </div>
                  <div v-if="!ownerDetail.pets?.length" class="empty-pets-hint">
                    Henüz hayvan kaydı yok.
                  </div>
                </div>
              </section>
            </div>
          </div>

          <!-- ZİYARET GEÇMİŞİ VE İŞLEMLER (YENİ) -->
          <div class="visits-full-section" style="margin-top: 2rem; border-top: 1px solid #e2e8f0; padding-top: 2rem;">
            <div class="section-header" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem;">
              <h4 style="font-size: 1.25rem; font-weight: 800; color: var(--text-main);">Ziyaret Geçmişi ve İşlemler</h4>
              <button class="btn btn-primary btn-sm" @click="showAddVisitForm = !showAddVisitForm">
                {{ showAddVisitForm ? 'Kapat' : '+ Yeni İşlem Ekle' }}
              </button>
            </div>

            <div v-if="showAddVisitForm" class="modern-visit-card" style="margin-bottom: 2rem; border-color: var(--primary-light); background: #f8fafc; padding: 1.5rem; border-radius: 12px; border: 1px solid #e2e8f0;">
              <h5 style="margin-bottom: 1rem; font-weight: 700; color: var(--primary);">Yeni Ziyaret / İşlem Kaydı</h5>
              <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1rem;">
                <div class="form-group" style="margin-bottom: 0;">
                  <label style="font-size: 0.8rem; font-weight: bold; color: #64748b;">Hangi Hayvan İçin?</label>
                  <select v-model="newVisit.petId" class="modern-input" style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;">
                    <option value="">-- Hayvan Seçin --</option>
                    <option v-for="p in ownerDetail?.pets" :key="p.id" :value="p.id">{{ p.name }} ({{ p.species || 'Tür yok' }}) - Çip: {{ p.microchipNumber || 'Yok' }}</option>
                  </select>
                </div>
                <div class="form-group" style="margin-bottom: 0;">
                  <label style="font-size: 0.8rem; font-weight: bold; color: #64748b;">İşlem Tarihi</label>
                  <input type="datetime-local" v-model="newVisit.performedAt" class="modern-input" style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;" />
                </div>
              </div>
              <div class="form-group" style="margin-bottom: 1rem;">
                <label style="font-size: 0.8rem; font-weight: bold; color: #64748b;">Uygulanan İşlemler (Aşı, Parazit vb.)</label>
                <div class="procedure-pills-container">
                  <button
                    v-for="pill in predefinedProcedures"
                    :key="pill"
                    type="button"
                    class="pill-select-btn"
                    :class="{ active: isProcedureSelected(pill, newVisit.procedures) }"
                    @click="toggleProcedure(pill, newVisit, 'procedures')"
                  >
                    {{ pill }}
                  </button>
                </div>
                <textarea v-model="newVisit.procedures" class="modern-input" rows="2" placeholder="Örn: İç dış parazit yapıldı..." style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;"></textarea>
              </div>
              <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1rem;">
                <div class="form-group" style="margin-bottom: 0;">
                  <label style="font-size: 0.8rem; font-weight: bold; color: #64748b;">Alınan Ücret (TL)</label>
                  <input type="number" v-model.number="newVisit.amountTl" class="modern-input" min="0" step="0.01" style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;" />
                </div>
                <div class="form-group" style="margin-bottom: 0;">
                  <label style="font-size: 0.8rem; font-weight: bold; color: #64748b;">Veresiye (TL)</label>
                  <input type="number" v-model.number="newVisit.creditAmountTl" class="modern-input" min="0" step="0.01" style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;" />
                </div>
              </div>
              <div class="form-group" style="margin-bottom: 1rem;">
                <label style="font-size: 0.8rem; font-weight: bold; color: #64748b;">Fotoğraf / Görsel Yükle (İsteğe Bağlı)</label>
                <input type="file" multiple accept="image/*" @change="handleVisitFiles" class="modern-input" style="padding: 0.5rem; width: 100%;" />
                <small style="color: #64748b; margin-top: 0.25rem; display: block;">Birden fazla görsel seçebilirsiniz.</small>
              </div>
              <div style="display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1.5rem;">
                <button class="btn btn-ghost" @click="showAddVisitForm = false">İptal</button>
                <button class="btn btn-primary" @click="handleAddVisit" :disabled="visitAdding">
                  {{ visitAdding ? 'Kaydediliyor...' : 'İşlemi Kaydet' }}
                </button>
              </div>
              <p v-if="visitAddError" class="state state-error" style="margin-top: 1rem;">{{ visitAddError }}</p>
            </div>

            <div v-if="loadingVisits" class="state">Ziyaretler yükleniyor...</div>
            <div v-else-if="ownerVisits.length === 0" class="empty-notes-hint" style="text-align: center; padding: 2rem;">
              Bu müşteriye ait henüz bir ziyaret kaydı bulunmuyor.
            </div>
            <div v-else class="visit-timeline" style="display: flex; flex-direction: column; gap: 1.5rem;">
              <div v-for="v in ownerVisits" :key="v.id || v.visitId" class="modern-visit-card" style="background: #ffffff; padding: 1.5rem; border-radius: 12px; border: 1px solid #f1f5f9; box-shadow: var(--shadow-sm); position: relative; border-left: 4px solid var(--primary);">
                <!-- DÜZENLEME MODU (ZİYARET) -->
                <div v-if="visitEditId === (v.id || v.visitId)">
                  <div class="edit-grid" style="display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; margin-bottom: 1rem;">
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b; display: block; margin-bottom: 0.25rem;">İŞLEM TARİHİ</label>
                      <input type="datetime-local" v-model="visitDraft.performedAt" class="edit-input" style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;" />
                    </div>
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b; display: block; margin-bottom: 0.25rem;">HANGİ HAYVAN İÇİN?</label>
                      <select v-model="visitDraft.petId" class="edit-input" style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;">
                        <option v-for="p in ownerDetail?.pets" :key="p.id" :value="p.id">{{ p.name }}</option>
                      </select>
                    </div>
                  </div>
                  
                  <div class="field" style="margin-bottom: 1rem;">
                    <label style="font-size: 0.7rem; font-weight: 700; color: #64748b; display: block; margin-bottom: 0.25rem;">UYGULANAN İŞLEMLER</label>
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
                    <textarea v-model="visitDraft.procedures" class="edit-input" rows="3" style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;"></textarea>
                  </div>

                  <div style="display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 0.75rem; margin-bottom: 1rem;">
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b; display: block; margin-bottom: 0.25rem;">TUTAR (TL)</label>
                      <input type="number" v-model.number="visitDraft.amountTl" @input="onEditAmountInput" class="edit-input" style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;" />
                    </div>
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b; display: block; margin-bottom: 0.25rem;">NAKİT (TL)</label>
                      <input type="number" v-model.number="visitDraft.collectedAmountTl" @input="onEditCollectedInput" class="edit-input" style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;" />
                    </div>
                    <div class="field">
                      <label style="font-size: 0.7rem; font-weight: 700; color: #64748b; display: block; margin-bottom: 0.25rem;">VERESİYE (TL)</label>
                      <input type="number" v-model.number="visitDraft.creditAmountTl" @input="onEditCreditInput" class="edit-input" style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;" />
                    </div>
                  </div>

                  <div class="field" style="margin-bottom: 1rem;">
                    <label style="font-size: 0.7rem; font-weight: 700; color: #64748b; display: block; margin-bottom: 0.25rem;">HEKİM NOTU</label>
                    <textarea v-model="visitDraft.notes" class="edit-input" rows="2" style="padding: 0.5rem; border-radius: 8px; width: 100%; border: 1px solid #cbd5e1;"></textarea>
                  </div>

                  <div v-if="visitSaveError" class="state state-error" style="font-size: 0.85rem; margin-bottom: 1rem; padding: 0.5rem;">{{ visitSaveError }}</div>

                  <div class="edit-actions" style="display: flex; gap: 0.5rem; justify-content: flex-end;">
                    <button class="btn btn-ghost btn-sm" @click="cancelVisitEdit">İptal</button>
                    <button class="btn btn-primary btn-sm" @click="saveVisitEdit(v)" :disabled="visitSaving">
                      {{ visitSaving ? 'Kaydediliyor...' : 'Kaydet' }}
                    </button>
                    <button class="btn btn-danger-sm btn-sm" @click="handleDeleteVisit(v)" :disabled="visitSaving">Sil</button>
                  </div>
                </div>

                <!-- GÖRÜNTÜLEME MODU -->
                <div v-else>
                  <div class="visit-header" style="display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1rem;">
                    <div>
                      <span style="display: block; font-weight: 800; font-size: 1.1rem;">{{ formatDt(v.performedAt) }}</span>
                      <span style="font-size: 0.85rem; color: var(--primary); font-weight: 700;">Pet: {{ v.petName || 'Bilinmiyor' }}</span>
                    </div>
                    <div style="display: flex; gap: 1rem; text-align: right;">
                      <div>
                        <span style="font-size: 0.7rem; color: #64748b; display: block;">TOPLAM TUTAR</span>
                        <span style="font-weight: 800; color: #1e293b;">{{ fmtMoney(v.amountTl) }}</span>
                      </div>
                      <div>
                        <span style="font-size: 0.7rem; color: #64748b; display: block;">NAKİT</span>
                        <span style="font-weight: 800; color: var(--success);">{{ fmtMoney(v.collectedAmountTl ?? (v.amountTl - (v.creditAmountTl ?? 0))) }}</span>
                      </div>
                      <div v-if="v.creditAmountTl > 0">
                        <span style="font-size: 0.7rem; color: #64748b; display: block;">VERESİYE</span>
                        <span style="font-weight: 800; color: var(--danger);">{{ fmtMoney(v.creditAmountTl) }}</span>
                      </div>
                    </div>
                  </div>
                  <div class="procedure-block" style="background: #f8fafc; padding: 1rem; border-radius: 8px; font-size: 0.95rem; line-height: 1.5; margin-bottom: 1rem;">
                    <strong style="color: #64748b; font-size: 0.8rem; text-transform: uppercase;">Uygulanan İşlemler</strong>
                    <p style="margin-top: 0.25rem;">{{ v.procedures || 'Belirtilmemiş' }}</p>
                  </div>
                  <div v-if="v.notes" class="notes-block" style="background: #fffbeb; padding: 1rem; border-radius: 8px; font-size: 0.95rem; line-height: 1.5; margin-bottom: 1rem; border: 1px solid #fef3c7;">
                    <strong style="color: #d97706; font-size: 0.8rem; text-transform: uppercase;">Hekim Notu</strong>
                    <p style="margin-top: 0.25rem;">{{ v.notes }}</p>
                  </div>
                  
                  <div v-if="getVisitImages(v).length > 0" class="visit-gallery" style="display: flex; flex-wrap: wrap; gap: 0.5rem; margin-top: 1rem;">
                    <div v-for="(img, idx) in getVisitImages(v)" :key="idx" class="gallery-item" style="width: 80px; height: 80px; border-radius: 8px; overflow: hidden; border: 1px solid #e2e8f0;">
                      <img :src="normalizeMediaUrl(getImageUrl(img))" style="width: 100%; height: 100%; object-fit: cover; cursor: pointer;" @click="window.open(normalizeMediaUrl(getImageUrl(img)), '_blank')" />
                    </div>
                  </div>

                  <div style="display: flex; justify-content: flex-start; margin-top: 1rem;">
                    <button class="btn btn-secondary btn-sm" @click="openVisitEdit(v)">Düzenle</button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { fetchOwners, createOwner, fetchOwner, addPetToOwner, deletePet, addOwnerNote, searchOwners, updateOwnerNote, deleteOwnerNote } from '../api/owners'
import { fetchVisits, createVisit, uploadVisitImages } from '../api/visits'
import { API_BASE } from '../api/http'

const router = useRouter()
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

const noteError = ref('')
const noteText = ref('')
const noteAdding = ref(false)
const noteImageFile = ref(null)
const noteImagePreviewUrl = ref(null)

function handleNoteFileChange(e) {
  const file = e.target.files?.[0]
  if (file) {
    noteImageFile.value = file
    noteImagePreviewUrl.value = URL.createObjectURL(file)
  }
}

function clearSelectedNoteImage() {
  noteImageFile.value = null
  if (noteImagePreviewUrl.value) {
    URL.revokeObjectURL(noteImagePreviewUrl.value)
    noteImagePreviewUrl.value = null
  }
}

const ownerEditOpen = ref(false)
const ownerSaving = ref(false)
const ownerDraft = reactive({ fullName: '', phoneE164: '', password: '' })

const editingPetId = ref(null)
const petDraft = reactive({ name: '', species: '', breed: '', ageYears: null, ageMonths: null, notes: '', amountTl: null, creditAmountTl: null })

const editingNoteId = ref(null)
const noteDraft = ref('')
const noteSaving = ref(false)

const ownerVisits = ref([])
const loadingVisits = ref(false)

const visitEditId = ref(null)
const visitDraft = ref(null)
const visitSaving = ref(false)
const visitSaveError = ref('')

function toVisitDraft(v) {
  if (!v) return null
  const perf = v.performedAt || v.PerformedAt || ''
  const amount = v.amountTl ?? v.AmountTl ?? null
  const credit = v.creditAmountTl ?? v.CreditAmountTl ?? null
  const collected = v.collectedAmountTl ?? v.CollectedAmountTl ?? null
  return {
    performedAt: perf ? new Date(perf).toISOString().slice(0, 16) : '',
    procedures: v.procedures || v.Procedures || '',
    amountTl: amount,
    notes: v.notes || v.Notes || '',
    creditAmountTl: credit,
    collectedAmountTl: collected ?? (amount !== null ? Math.max(0, amount - (credit || 0)) : null),
    petId: v.petId ?? v.PetId ?? ''
  }
}

function openVisitEdit(v) {
  visitSaveError.value = ''
  visitEditId.value = v.id || v.Id || v.visitId || v.VisitId
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
  const visitId = v?.id || v?.Id || v?.visitId || v?.VisitId
  if (!visitId || !visitDraft.value) return
  visitSaving.value = true
  visitSaveError.value = ''
  try {
    const { http } = await import('@/api/http')
    const payload = {
      performedAt: new Date(visitDraft.value.performedAt).toISOString(),
      procedures: (visitDraft.value.procedures || '').trim() || null,
      amountTl: visitDraft.value.amountTl,
      notes: (visitDraft.value.notes || '').trim() || null,
      creditAmountTl: visitDraft.value.creditAmountTl,
      collectedAmountTl: visitDraft.value.collectedAmountTl,
      petId: Number(visitDraft.value.petId)
    }
    await http.put(`/visits/${visitId}`, payload)
    await loadOwnerVisits()
    const res = await fetchOwner(selectedOwner.value)
    ownerDetail.value = res?.data ?? res
    cancelVisitEdit()
  } catch (e) {
    console.error(e)
    visitSaveError.value = 'Ziyaret güncellenirken hata oluştu.'
  } finally {
    visitSaving.value = false
  }
}

async function handleDeleteVisit(v) {
  const visitId = v?.id || v?.Id || v?.visitId || v?.VisitId
  if (!visitId) return
  if (!confirm('Bu ziyareti silmek istediğinize emin misiniz?')) return
  visitSaving.value = true
  try {
    const { http } = await import('@/api/http')
    await http.delete(`/visits/${visitId}`)
    await loadOwnerVisits()
    const res = await fetchOwner(selectedOwner.value)
    ownerDetail.value = res?.data ?? res
    cancelVisitEdit()
  } catch (err) {
    console.error(err)
    alert('Ziyaret silinemedi.')
  } finally {
    visitSaving.value = false
  }
}

const showAddVisitForm = ref(false)
const visitAdding = ref(false)
const visitAddError = ref('')
const newVisit = reactive({
  petId: '',
  performedAt: new Date().toISOString().slice(0, 16),
  procedures: '',
  amountTl: null,
  creditAmountTl: null,
  notes: '',
})
const visitImagesToUpload = ref([])

function formatDt(iso) { return iso ? new Date(iso).toLocaleString('tr-TR') : '—' }
function fmtMoney(val) { return `${Number(val || 0).toFixed(2)}₺` }
function getImageUrl(img) { return img?.url || img?.imageUrl || '' }
function getVisitImages(v) { return v?.images || v?.Images || [] }
function normalizeMediaUrl(rawUrl) {
  if (!rawUrl) return ''
  if (rawUrl.startsWith('http')) return rawUrl
  const base = API_BASE.endsWith('/') ? API_BASE.slice(0, -1) : API_BASE
  const path = rawUrl.startsWith('/') ? rawUrl : `/${rawUrl}`
  return `${base}${path}`
}

async function loadOwnerVisits() {
  if (!selectedOwner.value) return
  loadingVisits.value = true
  try {
    const res = await fetchVisits({ ownerId: selectedOwner.value })
    ownerVisits.value = res?.data ?? res
  } catch (e) {
    console.error('Ziyaretler yüklenemedi:', e)
  } finally {
    loadingVisits.value = false
  }
}

function handleVisitFiles(e) {
  visitImagesToUpload.value = Array.from(e.target.files)
}

async function handleAddVisit() {
  if (!newVisit.petId) { visitAddError.value = 'Lütfen bir hayvan (hasta) seçin.'; return }
  if (!newVisit.procedures?.trim()) { visitAddError.value = 'Lütfen uygulanan tedaviyi (neler yapıldığını) girin.'; return }
  
  const hasAmount = newVisit.amountTl !== null && newVisit.amountTl !== undefined && newVisit.amountTl > 0;
  const hasCredit = newVisit.creditAmountTl !== null && newVisit.creditAmountTl !== undefined && newVisit.creditAmountTl > 0;
  
  if (!hasAmount && !hasCredit) {
    visitAddError.value = 'Lütfen alınan ücret (tutar) veya veresiye miktarından en az birini mutlaka girin.';
    return;
  }
  
  visitAdding.value = true
  visitAddError.value = ''
  try {
    const payload = {
      petId: Number(newVisit.petId),
      performedAt: new Date(newVisit.performedAt).toISOString(),
      procedures: newVisit.procedures.trim(),
      amountTl: newVisit.amountTl,
      creditAmountTl: newVisit.creditAmountTl,
      status: 1, // completed
      notes: newVisit.notes?.trim() || null
    }
    
    const created = await createVisit(payload)
    const newVisitId = created?.data?.id || created?.id

    if (newVisitId && visitImagesToUpload.value.length > 0) {
      await uploadVisitImages(newVisitId, visitImagesToUpload.value)
    }

    // Refresh everything
    await loadOwnerVisits()
    const res = await fetchOwner(selectedOwner.value)
    ownerDetail.value = res?.data ?? res
    
    // Reset form
    showAddVisitForm.value = false
    newVisit.procedures = ''
    newVisit.amountTl = null
    newVisit.creditAmountTl = null
    newVisit.notes = ''
    newVisit.petId = ''
    visitImagesToUpload.value = []
    
    // Reset file input UI by creating a tiny reactive toggle if needed, or user can just click normally next time
  } catch(err) {
    console.error(err)
    visitAddError.value = 'Ziyaret eklenirken hata oluştu.'
  } finally {
    visitAdding.value = false
  }
}

function startEditNote(note) {
  editingNoteId.value = note.id
  noteDraft.value = note.note
}

function cancelEditNote() {
  editingNoteId.value = null
  noteDraft.value = ''
}

async function saveNoteEdit() {
  if (!selectedOwner.value || !editingNoteId.value) return
  if (!noteDraft.value.trim()) return

  noteSaving.value = true
  try {
    await updateOwnerNote(selectedOwner.value, editingNoteId.value, noteDraft.value.trim())
    
    // Refresh
    const res = await fetchOwner(selectedOwner.value)
    ownerDetail.value = res?.data ?? res
    editingNoteId.value = null
  } catch (err) {
    console.error(err)
    alert('Not güncellenirken hata oluştu.')
  } finally {
    noteSaving.value = false
  }
}

async function handleDeleteNote(noteId) {
  if (!selectedOwner.value) return
  if (!confirm('Bu notu silmek istediğinize emin misiniz?')) return

  try {
    await deleteOwnerNote(selectedOwner.value, noteId)
    
    // Refresh
    const res = await fetchOwner(selectedOwner.value)
    ownerDetail.value = res?.data ?? res
  } catch (err) {
    console.error(err)
    alert('Not silinirken hata oluştu.')
  }
}

function openOwnerEdit() {
  if (!ownerDetail.value) return
  ownerDraft.fullName = ownerDetail.value.fullName
  ownerDraft.phoneE164 = ownerDetail.value.phoneE164
  ownerDraft.password = ''
  ownerEditOpen.value = true
}

function cancelOwnerEdit() {
  ownerEditOpen.value = false
}

async function saveOwnerEdit() {
  if (!selectedOwner.value) return
  ownerSaving.value = true
  try {
    const payload = {
      fullName: ownerDraft.fullName,
      phoneE164: ownerDraft.phoneE164,
      email: ownerDetail.value.email,
      address: ownerDetail.value.address,
      password: ownerDraft.password?.trim() || null,
      kvkkOptIn: ownerDetail.value.kvkkOptIn
    }
    const { http } = await import('@/api/http')
    await http.put(`/owners/${selectedOwner.value}`, payload)
    
    // Refresh
    const res = await fetchOwner(selectedOwner.value)
    ownerDetail.value = res?.data ?? res
    await loadOwners()
    ownerEditOpen.value = false
  } catch (err) {
    console.error(err)
    alert('Sahip bilgileri güncellenirken hata oluştu.')
  } finally {
    ownerSaving.value = false
  }
}

function startEditPet(pet) {
  editingPetId.value = pet.id
  petDraft.name = pet.name || ''
  petDraft.species = pet.species || ''
  petDraft.breed = pet.breed || ''
  petDraft.ageYears = pet.ageYears ?? null
  petDraft.ageMonths = pet.ageMonths ?? null
  petDraft.notes = pet.notes || ''
  petDraft.amountTl = null
  petDraft.creditAmountTl = null
}

async function savePetEdit() {
  if (!editingPetId.value) return
  try {
    const { http } = await import('@/api/http')
    
    // 1. Pet bilgilerini güncelle
    const payload = {
      name: (petDraft.name || '').trim(),
      species: (petDraft.species || '').trim() || null,
      breed: (petDraft.breed || '').trim() || null,
      notes: (petDraft.notes || '').trim() || null,
      ageYears: petDraft.ageYears ?? null,
      ageMonths: petDraft.ageMonths ?? null,
    }
    await http.put(`/pets/${editingPetId.value}`, payload)
    
    // 2. Ücret girildiyse hızlı ziyaret kaydı oluştur (bilanço otomatik senkronize olur)
    if (petDraft.amountTl && Number(petDraft.amountTl) > 0) {
      const visitPayload = {
        petId: editingPetId.value,
        performedAt: new Date().toISOString(),
        procedures: 'Hızlı ücret girişi',
        amountTl: Number(petDraft.amountTl),
        creditAmountTl: petDraft.creditAmountTl ? Number(petDraft.creditAmountTl) : null,
        status: 1, // 1 = Completed (Yapıldı)
        notes: null,
      }
      await http.post('/visits', visitPayload)
    }
    
    // 3. Sahip detayını yenile
    const res = await fetchOwner(selectedOwner.value)
    ownerDetail.value = res?.data ?? res
    editingPetId.value = null
  } catch (err) {
    console.error(err)
    alert('Pet bilgileri güncellenirken hata oluştu.')
  }
}

async function handleDeleteOwner() {
  if (!selectedOwner.value) return
  if (!confirm('Bu hasta sahibini ve TÜM bağlı pet kayıtlarını silmek istediğinize emin misiniz? Bu işlem geri alınamaz.')) return
  
  try {
    const { http } = await import('@/api/http')
    await http.delete(`/owners/${selectedOwner.value}`)
    closeOwnerDetail()
    await loadOwners()
  } catch (err) {
    console.error(err)
    alert('Sahip silinirken hata oluştu. (Ziyaret kaydı olan petleri önce silmeniz gerekebilir.)')
  }
}

async function removePet(id) {
  if (!confirm('Bu pet kaydını silmek istediğinize emin misiniz?')) return
  try {
    const { http } = await import('@/api/http')
    await http.delete(`/pets/${id}`)
    
    // Refresh
    if (selectedOwner.value) {
      const res = await fetchOwner(selectedOwner.value)
      ownerDetail.value = res?.data ?? res
    }
  } catch (err) {
    console.error(err)
    const msg = err.response?.data
    alert(typeof msg === 'string' ? msg : 'Pet silinemedi. (Ziyaret kaydı olabilir.)')
  }
}

const form = reactive({
  fullName: '',
  phoneE164: '',
  password: '',
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
    await loadOwnerVisits()
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
  showAddVisitForm.value = false
  showAddPetForm.value = false
  resetNewPet()
  clearSelectedNoteImage()
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
  if (!selectedOwner.value) return
  if (!noteText.value?.trim() && !noteImageFile.value) return

  noteError.value = ''
  noteAdding.value = true
  try {
    await addOwnerNote(selectedOwner.value, noteText.value?.trim() || '', noteImageFile.value)
    noteText.value = ''
    clearSelectedNoteImage()
    
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

function goToPetProfile(petId) {
  closeOwnerDetail()
  router.push({ name: 'pets', query: { id: petId } })
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
      password: form.password?.trim() || null,
      kvkkOptIn: true,
      pets: cleanedPets
    })

    formSuccess.value = 'Kayıt başarıyla oluşturuldu.'
    await loadOwners()

    form.fullName = ''
    form.phoneE164 = ''
    form.password = ''
    form.pets = [{ name: '', species: '', ageYears: null, ageMonths: null, notes: '' }]
  } catch (err) {
    console.error(err)
    formError.value = 'Kayıt oluşturulurken bir hata oluştu.'
  } finally {
    creating.value = false
  }
}

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

@media (max-width: 1024px) {
  .owners-grid {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }
  
  .form-card {
    position: static;
    order: -1; /* Mobilde formu en üste alalım veya gizleyelim */
  }
}

/* LIST CARD */
.list-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.search-section {
  display: flex;
  gap: 0.75rem;
  align-items: center;
  margin-bottom: 1rem;
}

.search-input-wrapper {
  flex: 1;
  position: relative;
  min-width: 0;
}

.search-input-wrapper input {
  width: 100%;
  box-sizing: border-box;
  padding: 0.85rem 1rem 0.85rem 2.75rem;
  border-radius: 14px;
  border: 1.5px solid #e2e8f0;
  background: #ffffff;
  font-size: 1rem;
  color: #1e293b;
  box-shadow: var(--shadow-sm);
  transition: var(--transition);
}

.search-input-wrapper input::placeholder {
  color: #94a3b8;
}

.search-input-wrapper input:focus {
  border-color: var(--primary);
  box-shadow: 0 0 0 3px var(--primary-light);
  outline: none;
}

.search-icon {
  position: absolute;
  left: 0.9rem;
  top: 50%;
  transform: translateY(-50%);
  color: #94a3b8;
  pointer-events: none;
  display: flex;
  align-items: center;
}

.refresh-btn {
  flex-shrink: 0;
  width: 46px;
  height: 46px;
  border-radius: 14px;
  border: 1.5px solid #e2e8f0;
  background: #ffffff;
  color: #64748b;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: var(--shadow-sm);
}

.refresh-btn:hover {
  background: var(--primary);
  border-color: var(--primary);
  color: #ffffff;
}

.refresh-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* OWNER CARDS */
.owners-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.25rem;
}

@media (max-width: 768px) {
  .owners-list {
    grid-template-columns: 1fr;
  }
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
  align-items: center;
  gap: 0.75rem;
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

.modern-owner-modal {
  width: 95%;
  max-width: 900px;
  background: #ffffff;
  border-radius: 30px;
  padding: 0;
  overflow: hidden;
  position: relative;
  box-shadow: 0 30px 60px -12px rgba(15, 23, 42, 0.25);
  animation: modalScale 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

@media (max-width: 768px) {
  .modern-owner-modal {
    width: 100%;
    height: 100%;
    max-width: none;
    border-radius: 0;
    overflow-y: auto;
  }
  
  .modal-grid {
    grid-template-columns: 1fr !important;
  }

  .modal-header-section {
    padding: 2rem 1.5rem;
  }
}

@keyframes modalScale {
  from { opacity: 0; transform: scale(0.95) translateY(20px); }
  to { opacity: 1; transform: scale(1) translateY(0); }
}

.modal-close-btn {
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
  z-index: 10;
  transition: all 0.2s;
}

.modal-close-btn:hover {
  background: #e2e8f0;
  transform: rotate(90deg);
}

.modal-content {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.modal-header-section {
  padding: 3rem;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  display: flex;
  align-items: center;
  gap: 2rem;
  border-bottom: 1px solid #e2e8f0;
}

.owner-avatar-large {
  width: 100px;
  height: 100px;
  background: var(--primary);
  color: white;
  border-radius: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 3rem;
  font-weight: 800;
  box-shadow: 0 10px 25px -5px rgba(79, 70, 229, 0.3);
}

.owner-main-info h2 {
  font-size: 2.5rem;
  font-weight: 800;
  letter-spacing: -0.04em;
  color: var(--text-main);
  margin-bottom: 0.5rem;
}

.contact-pill {
  display: inline-flex;
  padding: 0.5rem 1rem;
  background: #ffffff;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1rem;
  color: var(--primary);
  border: 1px solid #e2e8f0;
}

.modal-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  padding: 2.5rem;
  gap: 2.5rem;
}

/* SECTION COMMON */
.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.section-header h4 {
  font-size: 1.25rem;
  font-weight: 800;
  color: var(--text-main);
}

.count-badge {
  padding: 0.25rem 0.75rem;
  background: #f1f5f9;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 800;
  color: var(--text-muted);
}

/* NOTES SECTION */
.note-input-box {
  background: #f8fafc;
  padding: 1rem;
  border-radius: 20px;
  margin-bottom: 1.5rem;
  border: 1px solid #e2e8f0;
}

.note-input-box textarea {
  width: 100%;
  border: none;
  background: transparent;
  resize: none;
  outline: none;
  font-size: 0.95rem;
  margin-bottom: 0.5rem;
}

.notes-scroll-area {
  max-height: 300px;
  overflow-y: auto;
  padding-right: 0.5rem;
}

.modern-note-card {
  padding: 1.25rem;
  background: #ffffff;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  margin-bottom: 0.75rem;
  transition: all 0.2s;
}

.modern-note-card:hover {
  border-color: var(--primary-light);
  transform: translateX(4px);
}

.modern-note-card p {
  font-size: 0.95rem;
  line-height: 1.5;
  color: var(--text-main);
  margin-bottom: 0.5rem;
}

.modern-note-card .date {
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--text-muted);
}

/* PETS SECTION */
.inline-add-pet-card {
  background: #f5f3ff;
  padding: 1.5rem;
  border-radius: 20px;
  margin-bottom: 1.5rem;
  border: 1px solid #ddd6fe;
}

.inline-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.mini-input {
  width: 100%;
  padding: 0.75rem;
  border-radius: 10px;
  border: 1px solid #ffffff;
  background: #ffffff;
  font-size: 0.85rem;
}

.full-width { width: 100%; }

.pets-mini-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.pet-mini-card {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.25rem;
  background: #f8fafc;
  border-radius: 16px;
  border: 1px solid #f1f5f9;
  transition: all 0.2s;
}

.pet-mini-card.clickable {
  cursor: pointer;
}

.pet-mini-card.clickable:hover {
  background: #f1f5f9;
  border-color: var(--primary-light);
  transform: scale(1.02);
}

.p-info {
  display: flex;
  flex-direction: column;
}

.p-budget {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.25rem;
}

.total-badge {
  font-size: 0.85rem;
  font-weight: 800;
  color: var(--success);
}

.credit-badge {
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--danger);
  background: #fef2f2;
  padding: 2px 6px;
  border-radius: 6px;
}

.p-info strong {
  font-size: 1.05rem;
  color: var(--text-main);
}

.mini-textarea {
  width: 100%;
  padding: 0.75rem;
  border-radius: 10px;
  border: 1px solid #f1f5f9;
  background: #ffffff;
  font-size: 0.9rem;
  font-family: inherit;
  resize: vertical;
  min-height: 80px;
}

.pet-mini-card .p-info span { font-size: 0.85rem; color: #64748b; font-weight: 500; }

.delete-icon-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  font-size: 1.2rem;
  opacity: 0.5;
  transition: opacity 0.2s;
}

.delete-icon-btn:hover {
  opacity: 1;
}

.btn-xs {
  padding: 0.3rem 0.75rem;
  font-size: 0.8rem;
  border-radius: 8px;
}

@media (max-width: 1024px) {
  .owners-grid { grid-template-columns: 1fr; }
  .form-card { position: static; }
}

@media (max-width: 768px) {
  .modal-grid { grid-template-columns: 1fr; }
  .modal-header-section { padding: 2rem; flex-direction: column; text-align: center; }
  .owner-avatar-large { width: 80px; height: 80px; font-size: 2rem; }
  .owner-main-info h2 { font-size: 1.75rem; }
  
  .page-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
    margin-bottom: 1.5rem;
  }
}
.btn-text-action {
  background: transparent;
  border: none;
  font-size: 0.75rem;
  font-weight: 700;
  color: var(--primary);
  cursor: pointer;
  padding: 0;
  margin-left: 0.75rem;
  opacity: 0.7;
  transition: all 0.2s;
}

.btn-text-action:hover {
  opacity: 1;
  text-decoration: underline;
}

.btn-text-action.delete {
  color: var(--danger);
}

.note-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 0.5rem;
  border-top: 1px solid #f1f5f9;
  padding-top: 0.5rem;
}

.note-edit-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 0.5rem;
}

.mini-textarea-note {
  width: 100%;
  padding: 0.75rem;
  border-radius: 12px;
  border: 1px solid var(--primary-light);
  background: #f8fafc;
  font-size: 0.95rem;
  font-family: inherit;
  resize: vertical;
  min-height: 60px;
  outline: none;
}

.mini-textarea-note:focus {
  border-color: var(--primary);
  background: #ffffff;
}

.btn-primary-sm {
  background: var(--primary);
  color: white;
  border: none;
  padding: 0.4rem 1rem;
  border-radius: 8px;
  font-weight: 700;
  font-size: 0.8rem;
  cursor: pointer;
}

.btn-primary-sm:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-text {
  background: transparent;
  border: none;
  color: var(--text-muted);
  cursor: pointer;
  font-weight: 600;
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
