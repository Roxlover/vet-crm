import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './style.css'

// Capacitor Push Notifications
import { PushNotifications } from '@capacitor/push-notifications'

import { registerSW } from 'virtual:pwa-register'

console.log('BUILD:', __BUILD_TIME__)

// Service worker update handling
const updateSW = registerSW({
  onNeedRefresh() {
    const ok = window.confirm('Yeni sürüm hazır. Şimdi güncellensin mi?')
    if (ok) updateSW(true)
  },
  onOfflineReady() {
    console.log('PWA cache hazır (offline-ready).')
  },
})

// Initialize Push Notifications
PushNotifications.requestPermissions().then(result => {
  if (result.receive === 'granted') {
    // Register with APNs/FCM
    PushNotifications.register()
  } else {
    console.warn('Push notification permission not granted')
  }
})

PushNotifications.addListener('registration', token => {
  console.log('Push registration token:', token.value)
  // TODO: send token to backend if needed
})

PushNotifications.addListener('pushNotificationReceived', notification => {
  console.log('Push notification received:', notification)
})

PushNotifications.addListener('pushNotificationActionPerformed', notification => {
  console.log('Push notification action performed', notification)
  // Handle notification click, navigate if needed
})

const app = createApp(App)
app.use(router)
app.mount('#app')
