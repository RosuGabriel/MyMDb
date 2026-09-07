import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig({
  server: {
    port: 3000,
    proxy: {
      '/mymdb/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
      '/mymdb/static': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
  plugins: [
    react(),
    VitePWA({
      registerType: 'autoUpdate',
      injectRegister: 'script',
      devOptions: {
        enabled: true
      },
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,png,svg,webp,jpg,jpeg}'],
        globIgnores: ['**/film.png']
      },
      includeAssets: ['robots.txt', 'apple-touch-icon.png',
        'icon192.png', 'icon512.png', 'maskable-icon.png',
        'logo.png', 'icon-bgless.png'
      ],
      manifest: {
        name: 'RedPanda',
        short_name: 'RedPanda',
        description: 'RedPanda is a PWA app for managing your movie collection',
        display: 'standalone',
        background_color: '#282c34',
        theme_color: '#212529',
        start_url: '/',
        icons: [
          {
            src: 'icon192.png',
            sizes: '192x192',
            type: 'image/png',
            purpose: 'any'
          },
          {
            src: 'icon512.png',
            sizes: '512x512',
            type: 'image/png',
            purpose: 'any'
          },
          {
            src: 'icon-bgless.png',
            sizes: '1000x1000',
            type: 'image/png',
            purpose: 'any'
          },
          {
            src: 'maskable-icon.png',
            sizes: '1024x1024',
            type: 'image/png',
            purpose: 'maskable'
          }
        ]
      }
    })
  ]

});
