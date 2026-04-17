import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './style.css'

import { registerSW } from 'virtual:pwa-register'

console.log('BUILD:', __BUILD_TIME__)

// Update stratejisi: yeni sürüm varsa kullanıcıya sor
const updateSW = registerSW({
  onNeedRefresh() {
    const ok = window.confirm('Yeni sürüm hazır. Şimdi güncellensin mi?')
    if (ok) updateSW(true)
  },
  onOfflineReady() {
    // offline zorunlu değil ama cache hazır bilgisi
    console.log('PWA cache hazır (offline-ready).')
  },
})

const app = createApp(App)
app.use(router)
app.mount('#app')
