import { config } from '@vue/test-utils'
import { Quasar, Notify, Loading } from 'quasar'

// Mocking Quasar
config.global.plugins.push([Quasar, {
  plugins: {
    Notify,
    Loading
  }
}])
