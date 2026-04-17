import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  plugins: [
    vue(),

    VitePWA({
      // sw.js dist içine üretilecek
      filename: 'sw.js',
      registerType: 'prompt', // "update available" davranışı için uygun

      includeAssets: [
        'icons/apple-touch-icon.png',
        'icons/icon-192.png',
        'icons/icon-512.png',
      ],

      manifest: {
        name: 'e-Bull Vet',
        short_name: 'e-BullVet',
        start_url: '/',
        scope: '/',
        display: 'standalone',
        background_color: '#ffffff',
        theme_color: '#ffffff',
        icons: [
          { src: '/icons/icon-192.png', sizes: '192x192', type: 'image/png' },
          { src: '/icons/icon-512.png', sizes: '512x512', type: 'image/png' },
          { src: '/icons/icon-512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
        ],
      },

      workbox: {
        // Offline şart değil ama hızlı açılış için iyi.
        // Yeni build gelince eski cache’i temiz tutar.
        cleanupOutdatedCaches: true,
        clientsClaim: true,
        skipWaiting: false, // prompt kullandığımız için false kalsın
      },
    }),
  ],

  define: {
    __BUILD_TIME__: JSON.stringify(new Date().toISOString()),
  },

  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
})
