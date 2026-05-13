<script setup>
import { defineProps, ref, watch, onMounted, onBeforeUnmount } from 'vue';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';

delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: new URL('leaflet/dist/images/marker-icon-2x.png', import.meta.url).href,
  iconUrl: new URL('leaflet/dist/images/marker-icon.png', import.meta.url).href,
  shadowUrl: new URL('leaflet/dist/images/marker-shadow.png', import.meta.url).href,
});

// 1. Update Props to receive city and country
const props = defineProps({
  city: {
    type: String,
    default: ''
  },
  country: {
    type: String,
    default: ''
  },
  clickable: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['location-selected']);

const mapContainer = ref(null);
const loading = ref(false);
let map = null;
let marker = null;

onMounted(() => {
  map = L.map(mapContainer.value, {
    zoomControl: true
  }).setView([13.668827937821325, -89.25545237696733], 15);

  L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
    attribution: '&copy; OpenStreetMap contributors &copy; CARTO',
    subdomains: 'abcd',
    maxZoom: 20
  }).addTo(map);

  marker = L.marker([13.668827937821325, -89.25545237696733]).addTo(map);

  if (props.clickable) {
    map.on('click', async (e) => {
      const { lat, lng } = e.latlng;
      marker.setLatLng([lat, lng]);
      
      loading.value = true;
      try {
        const response = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`);
        const data = await response.json();
        
        if (data && data.address) {
          emit('location-selected', {
            address: data.address.road || data.address.suburb || data.address.neighbourhood || '',
            city: data.address.city || data.address.town || data.address.village || '',
            region: data.address.state || data.address.region || '',
            country: data.address.country || '',
            lat,
            lng
          });
        }
      } catch (err) {
        console.error("Reverse geocoding error:", err);
      } finally {
        loading.value = false;
      }
    });
  }
});

onBeforeUnmount(() => {
  if (map) {
    map.remove();
  }
});

let debounceTimer = null;

// 2. Observe BOTH properties simultaneously
watch([() => props.city, () => props.country], ([newCity, newCountry]) => {
  // Only search if we have a valid city name (at least 3 characters)
  if (newCity && newCity.length > 2) {
    clearTimeout(debounceTimer);
    loading.value = true;
    
    debounceTimer = setTimeout(async () => {
      try {
        // 3. Use Nominatim structured search API
        const cityParam = encodeURIComponent(newCity.trim());
        const countryParam = newCountry ? `&country=${encodeURIComponent(newCountry.trim())}` : '';
        
        const url = `https://nominatim.openstreetmap.org/search?format=json&city=${cityParam}${countryParam}`;
        
        const response = await fetch(url);
        const data = await response.json();
        
        if (data && data.length > 0) {
          const lat = parseFloat(data[0].lat);
          const lon = parseFloat(data[0].lon);
          if (map && marker) {
            // Only fly if coordinates are significantly different to avoid jitter
            const currentCenter = map.getCenter();
            if (Math.abs(currentCenter.lat - lat) > 0.01 || Math.abs(currentCenter.lng - lon) > 0.01) {
              map.flyTo([lat, lon], 12, { duration: 1.5 });
              marker.setLatLng([lat, lon]);
            }
          }
        }
      } catch (e) {
        console.warn("Error buscando la ubicación en el mapa:", e);
      } finally {
        loading.value = false;
      }
    }, 1000); 
  } else {
    loading.value = false;
  }
}, { immediate: true });
</script>

<template>
  <div style="border: 1px solid #eaeaea; border-radius: 8px; overflow: hidden; height: 300px; position: relative;">
    <div ref="mapContainer" style="width: 100%; height: 100%;"></div>
    <q-inner-loading :showing="loading">
      <q-spinner-gears size="50px" color="primary" />
      <div class="q-mt-md">Searching location...</div>
    </q-inner-loading>
  </div>
</template>

<style scoped>
:deep(.leaflet-control-attribution) {
  background: rgba(255, 255, 255, 0.7) !important;
  font-size: 10px;
}
</style>