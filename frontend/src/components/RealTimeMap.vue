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

// 1. Actualizamos los Props para recibir ciudad y país
const props = defineProps({
  city: {
    type: String,
    default: ''
  },
  country: {
    type: String,
    default: ''
  }
});

const mapContainer = ref(null);
let map = null;
let marker = null;

onMounted(() => {
  map = L.map(mapContainer.value, {
    zoomControl: true
  }).setView([40.7128, -74.0060], 12);

  L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
    attribution: '&copy; OpenStreetMap contributors &copy; CARTO',
    subdomains: 'abcd',
    maxZoom: 20
  }).addTo(map);

  marker = L.marker([40.7128, -74.0060]).addTo(map);
});

onBeforeUnmount(() => {
  if (map) {
    map.remove();
  }
});

let debounceTimer = null;

// 2. Observamos AMBAS propiedades al mismo tiempo
watch([() => props.city, () => props.country], ([newCity, newCountry]) => {
  // Solo buscamos si al menos tenemos una ciudad válida para buscar
  if (newCity && newCity.length > 2) {
    clearTimeout(debounceTimer);
    
    debounceTimer = setTimeout(async () => {
      try {
        // 3. Usamos la búsqueda estructurada de Nominatim
        // Limpiamos los espacios y creamos la URL
        const cityParam = encodeURIComponent(newCity.trim());
        const countryParam = newCountry ? `&country=${encodeURIComponent(newCountry.trim())}` : '';
        
        const url = `https://nominatim.openstreetmap.org/search?format=json&city=${cityParam}${countryParam}`;
        
        const response = await fetch(url);
        const data = await response.json();
        
        if (data && data.length > 0) {
          const lat = parseFloat(data[0].lat);
          const lon = parseFloat(data[0].lon);
          if (map && marker) {
            map.flyTo([lat, lon], 12, { duration: 1.5 });
            marker.setLatLng([lat, lon]);
          }
        }
      } catch (e) {
        console.warn("Error buscando la ubicación en el mapa:", e);
      }
    }, 1000); 
  }
}, { immediate: true });
</script>

<template>
  <div style="border: 1px solid #eaeaea; border-radius: 8px; overflow: hidden; height: 300px;">
    <div ref="mapContainer" style="width: 100%; height: 100%;"></div>
  </div>
</template>

<style scoped>
:deep(.leaflet-control-attribution) {
  background: rgba(255, 255, 255, 0.7) !important;
  font-size: 10px;
}
</style>