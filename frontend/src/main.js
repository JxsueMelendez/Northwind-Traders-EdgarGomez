import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { Quasar, Notify, Loading } from 'quasar'
import router from './router'
import '@quasar/extras/material-icons/material-icons.css'
import 'quasar/src/css/index.sass'
import App from './App.vue'

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.use(Quasar, {
  plugins: {
    Notify,
    Loading
  },
  config: {
    brand: {
      primary: '#000000',
      secondary: '#666666',
      accent: '#eaeaea',
      dark: '#111111',
      positive: '#10B981',
      negative: '#EF4444',
      info: '#3B82F6',
      warning: '#F59E0B'
    }
  }
})

app.mount('#app')
