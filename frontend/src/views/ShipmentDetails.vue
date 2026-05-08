<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useOrderStore } from '../stores/orderStore';
import RealTimeMap from '../components/RealTimeMap.vue';
import { useQuasar } from 'quasar';
import axios from 'axios';

const route = useRoute();
const router = useRouter();
const store = useOrderStore();
const q = useQuasar();

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'
});

const shipment = ref(null);
const loading = ref(true);
const tempStatus = ref('In Transit');

onMounted(async () => {
  try {
    shipment.value = await store.getOrder(route.params.id);
    tempStatus.value = shipment.value.shippedDate ? 'Delivered' : 'In Transit';
  } catch (error) {
    q.notify({ color: 'negative', message: 'Failed to load shipment details' });
  } finally {
    loading.value = false;
  }
});

async function updateStatus() {
  try {
    const isDelivered = tempStatus.value === 'Delivered';
    await store.updateOrder(shipment.value.orderId, {
      customerId: shipment.value.customerId,
      employeeId: shipment.value.employeeId,
      addressLine: shipment.value.shipAddress,
      city: shipment.value.shipCity,
      region: shipment.value.shipRegion,
      country: shipment.value.shipCountry,
      freight: shipment.value.freight,
      shippedDate: isDelivered ? new Date().toISOString() : null,
      details: (shipment.value.lineItems || []).map(li => ({
        productId: li.productId,
        quantity: li.quantity,
        unitPrice: li.unitPrice
      }))
    });

    // Refresh
    shipment.value = await store.getOrder(route.params.id);
    tempStatus.value = shipment.value.shippedDate ? 'Delivered' : 'In Transit';
    q.notify({ color: 'positive', message: 'Status updated successfully!', icon: 'check' });
  } catch (error) {
    q.notify({ color: 'negative', message: 'Failed to update status' });
  }
}

function exportOrderPdf() {
  window.open(`${api.defaults.baseURL}/api/orders/export/pdf?orderId=${shipment.value.orderId}`, '_blank');
}

const lineItemColumns = [
  { name: 'productId', label: 'Product ID', field: 'productId', align: 'left' },
  { name: 'quantity', label: 'Qty', field: 'quantity', align: 'right' },
  { name: 'unitPrice', label: 'Price', field: 'unitPrice', format: val => `$${Number(val).toFixed(2)}`, align: 'right' },
  { name: 'discount', label: 'Discount', field: 'discount', format: val => `${(Number(val) * 100).toFixed(0)}%`, align: 'right' },
  { name: 'subtotal', label: 'Subtotal', field: row => (row.unitPrice * row.quantity * (1 - row.discount)).toFixed(2), format: val => `$${val}`, align: 'right' }
];
</script>

<template>
  <div v-if="loading" class="text-center q-pa-xl">
    <q-spinner color="primary" size="3em" />
  </div>
  <div v-else-if="shipment">
    <div class="row q-mb-md items-center">
      <q-btn flat color="grey-8" icon="arrow_back" label="Back to Shipments" @click="router.push('/shipments')" />
      <q-space />
      <q-btn outline color="primary" icon="picture_as_pdf" label="Export PDF" @click="exportOrderPdf" />
    </div>
    <div class="row q-col-gutter-lg">
      <!-- Left Column: Info & Line Items -->
      <div class="col-12 col-md-7">
        <h1 class="text-h4 text-weight-bold q-mb-sm">Order #{{ shipment.orderId }}</h1>
        <div class="row items-center q-mb-md q-gutter-sm">
           <q-badge :color="shipment.shippedDate ? 'positive' : 'info'" class="text-subtitle1 q-pa-sm">
              Status: {{ shipment.shippedDate ? 'Delivered' : 'In Transit' }}
           </q-badge>
           <q-select
             outlined dense
             v-model="tempStatus"
             :options="['In Transit', 'Delivered']"
             label="Change Status"
             style="width: 150px"
           />
           <q-btn color="primary" label="SET" @click="updateStatus" unelevated />
        </div>

        <q-card flat bordered class="rounded-borders q-mb-lg">
          <q-card-section>
            <div class="text-h6 text-weight-bold q-mb-md">Order Information</div>
            <div class="row q-col-gutter-md">
              <div class="col-6"><strong>Customer:</strong> <br>{{ shipment.customerName }}</div>
              <div class="col-6"><strong>Order Date:</strong> <br>{{ shipment.orderDate ? new Date(shipment.orderDate).toLocaleDateString() : 'N/A' }}</div>
              <div class="col-6"><strong>Employee ID:</strong> <br>{{ shipment.employeeId ?? 'N/A' }}</div>
              <div class="col-6"><strong>Freight:</strong> <br>${{ Number(shipment.freight || 0).toFixed(2) }}</div>
              <div class="col-6"><strong>Ship Address:</strong> <br>{{ shipment.shipAddress || 'N/A' }}</div>
              <div class="col-6"><strong>Ship Region:</strong> <br>{{ shipment.shipRegion || 'N/A' }}</div>
            </div>
          </q-card-section>
        </q-card>

        <q-card flat bordered class="rounded-borders">
          <q-card-section>
            <div class="text-h6 text-weight-bold q-mb-md">Line Items</div>
            <q-table
              v-if="shipment.lineItems && shipment.lineItems.length > 0"
              flat
              :rows="shipment.lineItems"
              :columns="lineItemColumns"
              row-key="productId"
              hide-pagination
            />
            <p v-else class="text-grey-7">No line items found.</p>
            <div class="text-right text-h6 q-mt-md">Total: ${{ Number(shipment.totalAmount).toFixed(2) }}</div>
          </q-card-section>
        </q-card>
      </div>
      <!-- Right Column: Map -->
      <div class="col-12 col-md-5">
        <q-card flat bordered class="rounded-borders h-full">
          <q-card-section>
            <div class="text-h6 text-weight-bold q-mb-md">Tracking Location</div>
            <div class="text-body2 q-mb-md">
              <strong>Origin:</strong> {{ shipment.shipCity || 'Unknown' }}, {{ shipment.shipCountry || 'Unknown' }}
            </div>
            <!-- OpenStreetMap RealTimeMap -->
            <RealTimeMap :city="shipment.shipCity" :country="shipment.shipCountry" />
          </q-card-section>
        </q-card>
      </div>
    </div>
  </div>
  <div v-else class="text-center q-pa-xl text-negative">
    Shipment not found.
  </div>
</template>
