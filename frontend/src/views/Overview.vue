<script setup>
import { computed, onMounted, ref } from 'vue';
import { useOrderStore } from '../stores/orderStore';
import { Bar, Doughnut } from 'vue-chartjs';
import {
  Chart as ChartJS, Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale, ArcElement
} from 'chart.js';
import RealTimeMap from '../components/RealTimeMap.vue';

ChartJS.register(Title, Tooltip, Legend, BarElement, CategoryScale, LinearScale, ArcElement);

const store = useOrderStore();

// Granular Filters
const filterYear = ref('All');
const filterMonth = ref('All');
const filterWeek = ref('All');
const filterRegion = ref('All');

const monthOptions = ['All','Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
const weekOptions = computed(() => {
  const opts = ['All'];
  for (let i = 1; i <= 52; i++) opts.push(`W${i}`);
  return opts;
});

onMounted(() => {
  store.fetchOrders();
});

// Dynamic region options derived from actual data
const regionOptions = computed(() => {
  const regions = new Set();
  regions.add('All');
  store.orders.forEach(o => {
    if (o.shipCountry) regions.add(o.shipCountry);
    if (o.shipRegion) regions.add(o.shipRegion);
  });
  return Array.from(regions);
});

// Dynamic year options from data
const yearOptions = computed(() => {
  const years = new Set();
  years.add('All');
  store.orders.forEach(o => {
    if (o.orderDate) years.add(new Date(o.orderDate).getFullYear().toString());
  });
  return Array.from(years).sort();
});

const filteredOrders = computed(() => {
  return store.orders.filter(o => {
    if (filterYear.value !== 'All') {
      const year = o.orderDate ? new Date(o.orderDate).getFullYear().toString() : '';
      if (year !== filterYear.value) return false;
    }
    if (filterMonth.value !== 'All') {
      const monthIndex = monthOptions.indexOf(filterMonth.value); // 1-indexed
      if (!o.orderDate || new Date(o.orderDate).getMonth() + 1 !== monthIndex) return false;
    }
    if (filterWeek.value !== 'All') {
      const weekNum = parseInt(filterWeek.value.replace('W', ''));
      if (!o.orderDate) return false;
      const d = new Date(o.orderDate);
      const startOfYear = new Date(d.getFullYear(), 0, 1);
      const days = Math.floor((d - startOfYear) / (24 * 60 * 60 * 1000));
      const orderWeek = Math.ceil((days + startOfYear.getDay() + 1) / 7);
      if (orderWeek !== weekNum) return false;
    }
    if (filterRegion.value !== 'All') {
      const region = o.shipRegion || o.shipCountry || 'Unknown';
      if (region !== filterRegion.value) return false;
    }
    return true;
  });
});

// KPI Computations
const totalOrders = computed(() => filteredOrders.value.length);
const totalRevenue = computed(() => filteredOrders.value.reduce((sum, order) => sum + (order.totalAmount || 0), 0));
const totalInTransit = computed(() => filteredOrders.value.filter(o => o.orderDate && !o.shippedDate).length);
const totalDelivered = computed(() => filteredOrders.value.filter(o => o.shippedDate).length);

// Chart Data Setup
const barChartData = computed(() => {
  const counts = {};
  for (const order of filteredOrders.value) {
    const key = order.orderDate ? new Date(order.orderDate).toLocaleDateString('en-US', { month: 'short', year: 'numeric' }) : 'Unknown';
    counts[key] = (counts[key] || 0) + 1;
  }
  return {
    labels: Object.keys(counts),
    datasets: [{
      label: 'Orders',
      backgroundColor: '#3B82F6',
      borderRadius: 6,
      data: Object.values(counts)
    }]
  };
});

const doughnutChartData = computed(() => {
  const counts = {};
  for (const order of filteredOrders.value) {
    const region = order.shipCountry || order.shipRegion || 'Unknown';
    counts[region] = (counts[region] || 0) + 1;
  }
  const palette = ['#10B981', '#3B82F6', '#F59E0B', '#EF4444', '#8B5CF6', '#EC4899', '#06B6D4', '#84CC16'];
  return {
    labels: Object.keys(counts),
    datasets: [{
      backgroundColor: palette.slice(0, Object.keys(counts).length),
      data: Object.values(counts)
    }]
  };
});

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'bottom' },
    tooltip: {
      backgroundColor: '#111',
      titleColor: '#fff',
      bodyColor: '#fff',
      cornerRadius: 8,
      padding: 12
    }
  }
};

const activeTrackingPage = ref(1);

const activeOrders = computed(() => {
  return filteredOrders.value.filter(o => !o.shippedDate);
});

const paginatedActiveOrders = computed(() => {
  const start = (activeTrackingPage.value - 1) * 5;
  return activeOrders.value.slice(start, start + 5);
});

// Click-to-Focus map: track the currently selected shipment in the Active Tracking list
const focusedShipment = ref(null);

const mapCity = computed(() => {
  if (focusedShipment.value) return focusedShipment.value.shipCity || 'London';
  if (activeOrders.value.length > 0) return activeOrders.value[0].shipCity || 'London';
  return 'London';
});

const mapCountry = computed(() => {
  if (focusedShipment.value) return focusedShipment.value.shipCountry || 'UK';
  if (activeOrders.value.length > 0) return activeOrders.value[0].shipCountry || 'UK';
  return 'UK';
});

function selectShipment(order) {
  focusedShipment.value = order;
}

async function exportOrders(type) {
  const params = new URLSearchParams();
  if (filterYear.value !== 'All') params.append('year', filterYear.value);
  if (filterMonth.value !== 'All') params.append('month', monthOptions.indexOf(filterMonth.value));
  if (filterWeek.value !== 'All') params.append('week', filterWeek.value.replace('W', ''));
  if (filterRegion.value !== 'All') params.append('region', filterRegion.value);

  const url = `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'}/api/orders/export/${type}?${params.toString()}`;
  const link = document.createElement('a');
  link.href = url;
  link.setAttribute('download', `NorthwindReport.${type === 'pdf' ? 'pdf' : 'xlsx'}`);
  document.body.appendChild(link);
  link.click();
  link.remove();
}
</script>

<template>
  <div>
    <div class="row q-col-gutter-lg q-mb-xl items-end">
      <div class="col-12 col-md-4">
        <h1 class="text-h4 text-weight-bold q-my-none">Overview</h1>
        <p class="text-grey-7 q-mt-sm">High-level insights into your global logistics.</p>
      </div>
    </div>

    <div class="row q-mb-md items-center justify-between">
      <div class="row q-gutter-sm">
        <q-btn color="primary" outline icon="description" label="PDF Report" @click="exportOrders('pdf')" />
        <q-btn color="secondary" outline icon="table_view" label="Excel Report" @click="exportOrders('excel')" />
      </div>
      <div class="row q-gutter-sm justify-end">
        <q-select outlined dense v-model="filterYear" :options="yearOptions" label="Year" style="width: 110px" />
        <q-select outlined dense v-model="filterMonth" :options="monthOptions" label="Month" style="width: 110px" />
        <q-select outlined dense v-model="filterWeek" :options="weekOptions" label="Week" style="width: 110px" />
        <q-select outlined dense v-model="filterRegion" :options="regionOptions" label="Region" style="width: 140px" />
      </div>
    </div>

    <!-- KPI Cards -->
    <div class="row q-col-gutter-lg q-mb-xl">
      <template v-if="store.loading">
        <div v-for="n in 4" :key="n" class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered class="rounded-borders">
            <q-card-section>
              <q-skeleton type="text" width="60%" class="q-mb-sm" />
              <q-skeleton type="text" height="48px" />
            </q-card-section>
          </q-card>
        </div>
      </template>
      <template v-else>
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered class="rounded-borders hover-shadow transition-all">
            <q-card-section>
              <div class="text-overline text-grey-7">TOTAL REVENUE</div>
              <div class="text-h4 text-weight-bold text-primary">${{ totalRevenue.toFixed(2) }}</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered class="rounded-borders hover-shadow transition-all">
            <q-card-section>
              <div class="text-overline text-grey-7">TOTAL ORDERS</div>
              <div class="text-h4 text-weight-bold">{{ totalOrders }}</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered class="rounded-borders hover-shadow transition-all">
            <q-card-section>
              <div class="text-overline text-grey-7">IN TRANSIT</div>
              <div class="text-h4 text-weight-bold text-info">{{ totalInTransit }}</div>
            </q-card-section>
          </q-card>
        </div>
        <div class="col-12 col-sm-6 col-md-3">
          <q-card flat bordered class="rounded-borders hover-shadow transition-all">
            <q-card-section>
              <div class="text-overline text-grey-7">DELIVERED</div>
              <div class="text-h4 text-weight-bold text-positive">{{ totalDelivered }}</div>
            </q-card-section>
          </q-card>
        </div>
      </template>
    </div>

    <!-- Charts -->
    <div class="row q-col-gutter-lg q-mb-xl">
      <div class="col-12 col-md-8">
        <q-card flat bordered class="rounded-borders h-full">
          <q-card-section>
            <div class="text-h6 text-weight-bold">Orders Over Time</div>
            <div style="height: 300px; margin-top: 1rem;">
              <Bar :data="barChartData" :options="chartOptions" />
            </div>
          </q-card-section>
        </q-card>
      </div>
      <div class="col-12 col-md-4">
        <q-card flat bordered class="rounded-borders h-full">
          <q-card-section>
            <div class="text-h6 text-weight-bold">Shipments by Region</div>
            <div style="height: 300px; margin-top: 1rem;">
              <Doughnut :data="doughnutChartData" :options="chartOptions" />
            </div>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <!-- Global Fleet Tracking -->
    <div class="row q-col-gutter-lg q-mb-xl">
      <div class="col-12 col-md-8">
        <q-card flat bordered class="rounded-borders h-full">
          <q-card-section>
            <div class="text-h6 text-weight-bold">Global Tracking Map</div>
            <p class="text-grey-7">All 'In Transit' shipments displayed.</p>
            <div style="height: 400px; margin-top: 1rem; border-radius: 8px; overflow: hidden;">
              <RealTimeMap :city="mapCity" :country="mapCountry" />
            </div>
            <div v-if="focusedShipment" class="q-mt-sm text-body2 text-grey-8">
              <q-icon name="location_on" color="primary" /> Focused: Order #{{ focusedShipment.orderId }} — {{ focusedShipment.shipCity }}, {{ focusedShipment.shipCountry }}
            </div>
          </q-card-section>
        </q-card>
      </div>
      <div class="col-12 col-md-4">
        <q-card flat bordered class="rounded-borders h-full">
          <q-card-section>
            <div class="text-h6 text-weight-bold">Active Tracking</div>
            <p class="text-grey-7">{{ activeOrders.length }} shipment(s) in transit</p>
            <q-list separator>
               <q-item
                 v-for="order in paginatedActiveOrders"
                 :key="order.orderId"
                 clickable
                 v-ripple
                 @click="selectShipment(order)"
                 :class="focusedShipment?.orderId === order.orderId ? 'bg-blue-1' : ''"
                 style="border-radius: 8px; margin-bottom: 4px; transition: background 0.2s;"
               >
                  <q-item-section avatar>
                     <q-icon name="local_shipping" :color="focusedShipment?.orderId === order.orderId ? 'primary' : 'info'" />
                  </q-item-section>
                  <q-item-section>
                     <q-item-label>Order #{{ order.orderId }}</q-item-label>
                     <q-item-label caption>{{ order.shipCity || 'Unknown' }}, {{ order.shipCountry || 'Unknown' }}</q-item-label>
                  </q-item-section>
                  <q-item-section side>
                    <div class="column items-end q-gutter-xs">
                      <q-badge color="info" label="In Transit" />
                      <q-btn flat dense size="xs" color="primary" label="Details" :to="`/shipments/${order.orderId}`" stop />
                    </div>
                  </q-item-section>
               </q-item>
               <q-item v-if="activeOrders.length === 0">
                  <q-item-section class="text-grey-7 text-center">No active shipments.</q-item-section>
               </q-item>
            </q-list>
            <div class="flex flex-center q-mt-md" v-if="activeOrders.length > 5">
              <q-pagination
                v-model="activeTrackingPage"
                :max="Math.ceil(activeOrders.length / 5) || 1"
                direction-links
                color="primary"
              />
            </div>
          </q-card-section>
        </q-card>
      </div>
    </div>
  </div>
</template>

<style scoped>
.hover-shadow:hover {
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  transform: translateY(-2px);
}
.transition-all {
  transition: all 0.3s ease;
}
</style>
