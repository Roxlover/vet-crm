# Vet-CRM Mobil Uygulama Yol Haritası

Projeyi incelediğimde hali hazırda **Vue 3 + Vite** kullandığınızı ve en önemlisi **Capacitor**'un (capacitor.config.json, android ve ios klasörleri) projeye eklendiğini görüyorum. Yani mobil uygulamanın temelleri atılmış durumda!

Mevcut web uygulamasını (PWA) tam teşekküllü, mağazalarda yayınlanabilir bir mobil (Android & iOS) uygulamaya çevirmek için izlememiz gereken yol haritası aşağıdadır:

## 1. Geliştirme Ortamı Hazırlığı (Development Setup)
Capacitor projeleri yerel (native) platformları kullandığı için cihaz simülatörlerine ve derleme araçlarına ihtiyacımız var.
- **Android İçin:** [Android Studio](https://developer.android.com/studio) indirilip kurulmalı. Android SDK ve Sanal Cihaz (Emulator) ayarlanmalı.
- **iOS İçin (Opsiyonel/Mac Gerekli):** Uygulamayı Apple App Store'a çıkarmak ve iOS cihazlarda test etmek için bir macOS cihaza ve [Xcode](https://developer.apple.com/xcode/)'a ihtiyacınız olacak.

## 2. Arayüzün Mobile Uyarlanması (Mobile-First UI)
Mevcut arayüzünüzün telefonda bir web sitesi gibi değil, "gerçek bir uygulama" gibi hissettirmesi gerekir.
- **Safe Area (Çentik) Yönetimi:** Modern telefonlardaki üst çentik ve alt kaydırma çubukları için CSS güncellemeleri (`env(safe-area-inset-top)` vb.).
- **Dokunmatik Geri Bildirimler:** Tıklama (hover) efektleri yerine dokunma (active/ripple) efektleri eklenmesi.
- **Mobil Navigasyon:** Web'deki üst menü yerine, mobilde alt bar (Bottom Navigation) veya Hamburger menüye geçiş yapılması.
- **Klavye Yönetimi:** Form doldururken klavyenin ekranı kapatmasını önleyici düzenlemeler (Capacitor Keyboard plugin).

## 3. Donanım ve Native Özelliklerin Entegrasyonu
Tarayıcı kısıtlamalarından kurtulup telefonun özelliklerini kullanabiliriz. (Capacitor eklentileri ile)
- **Push Bildirimler:** Firebase Cloud Messaging (FCM) entegrasyonu ile kullanıcılara bildirim gönderme.
- **Kamera ve Galeri:** Hayvan/Hasta fotoğrafları yüklemek için Native kamera arayüzü kullanımı (`@capacitor/camera`).
- **Konum:** Eğer CRM'de veteriner veya klinik konum işlemleri varsa (`@capacitor/geolocation`).
- **Güvenli Depolama:** Tarayıcıdaki `localStorage` yerine, token ve hassas bilgileri cihazın güvenli hafızasında tutmak (`@capacitor/preferences`).
- **Geri Tuşu (Android):** Android cihazlardaki fiziksel geri tuşunun web'deki yönlendirmelerle (Vue Router) uyumlu çalışması.

## 4. Geliştirme ve Derleme Süreci
Kod yazarken her seferinde uygulamayı baştan derlemek yerine, "Live Reload" (Canlı Yenileme) özelliği kurmalıyız.
- Telefon veya emülatör üzerinde anlık kod değişikliklerini görme (Vite IP adresi ile).
- Değişiklikler bittikten sonra üretim derlemesi:
  1. `npm run build` (Vue'yu derler)
  2. `npx cap sync` (Derlenen kodları Android ve iOS klasörlerine taşır)

## 5. Gerçek Cihazlarda Test
Emülatörler dışında doğrudan telefonunuza kablo ile bağlayarak test etme aşaması.
- Android telefonu "Geliştirici Modu"na alıp USB Hata Ayıklama ile projeyi cihaza yükleme.
- Cihaz performansını ve akıcılığı test etme.

## 6. Mağazalara (App Store & Google Play) Yükleme
Uygulama hazır olduğunda marketlere sunulması:
- **Android (Google Play):**
  - Uygulama ikonları ve başlangıç ekranı (Splash Screen) görsellerinin ayarlanması (`@capacitor/splash-screen`).
  - `.aab` (Android App Bundle) formatında üretim sürümü alınması ve imzalanması (Keystore oluşturma).
  - Google Play Console hesabı açıp yükleme.
- **iOS (App Store):**
  - Apple Developer Account ($99/yıl) açılması.
  - Xcode üzerinden sertifikaların ve Provisioning Profile'ların alınması.
  - App Store Connect'e `.ipa` dosyasının yüklenmesi ve inceleme onayı.

---

### Nasıl İlerleyelim?
Şu an temeller atıldığı için ilk iş olarak uygulamanızı bilgisayardaki bir **Android Emulator'de** çalıştırmakla veya arayüzde **"Safe Area" (Çentik alanları)** gibi mobil düzeltmeleri yapmakla başlayabiliriz. 

Hangi adımdan başlamak istersiniz? Eğer bilgisayarınızda Android Studio kuruluysa hemen projeyi ayaklandırabiliriz!
