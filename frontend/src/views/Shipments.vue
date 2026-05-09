<script setup>
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useOrderStore } from '../stores/orderStore';
import { useQuasar } from 'quasar';

const router = useRouter();
const store = useOrderStore();
const q = useQuasar();

// Granular Filters
const filterYear = ref('All');
const filterMonth = ref('All');
const filterRegion = ref('All');
const filterStatus = ref('All');
const search = ref('');
const monthOptions = ['All','Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];

onMounted(() => {
  store.fetchOrders();
});

const regionOptions = computed(() => {
  const regions = new Set(['All']);
  store.orders.forEach(o => {
    if (o.shipCountry) regions.add(o.shipCountry);
  });
  return Array.from(regions);
});

const yearOptions = computed(() => {
  const years = new Set(['All']);
  store.orders.forEach(o => {
    if (o.orderDate) years.add(new Date(o.orderDate).getFullYear().toString());
  });
  return Array.from(years).sort();
});

const filteredOrders = computed(() => {
  return store.orders.filter(o => {
    // Global search
    if (search.value) {
      const q = search.value.toLowerCase();
      const matches = 
        String(o.orderId).toLowerCase().includes(q) ||
        (o.customerName || '').toLowerCase().includes(q) ||
        (o.shipCountry || '').toLowerCase().includes(q) ||
        (o.shipCity || '').toLowerCase().includes(q);
      if (!matches) return false;
    }

    if (filterYear.value !== 'All') {
      const year = o.orderDate ? new Date(o.orderDate).getFullYear().toString() : '';
      if (year !== filterYear.value) return false;
    }
    if (filterMonth.value !== 'All') {
      const monthIndex = monthOptions.indexOf(filterMonth.value);
      if (!o.orderDate || new Date(o.orderDate).getMonth() + 1 !== monthIndex) return false;
    }
    if (filterRegion.value !== 'All') {
      if ((o.shipCountry || 'Unknown') !== filterRegion.value) return false;
    }
    if (filterStatus.value !== 'All') {
      const isDelivered = !!o.shippedDate;
      if (filterStatus.value === 'Delivered' && !isDelivered) return false;
      if (filterStatus.value === 'In Transit' && isDelivered) return false;
    }
    return true;
  });
});

const columns = [
  { name: 'orderId', label: 'Order ID', field: 'orderId', sortable: true, align: 'left' },
  { name: 'customerName', label: 'Customer', field: 'customerName', sortable: true, align: 'left' },
  { name: 'status', label: 'Status', field: row => row.shippedDate ? 'Delivered' : 'In Transit', sortable: true, align: 'left' },
  { name: 'orderDate', label: 'Date', field: row => row.orderDate ? new Date(row.orderDate).toLocaleDateString() : 'N/A', sortable: true, align: 'left' },
  { name: 'region', label: 'Region', field: row => row.shipCountry || row.shipRegion || 'Unknown', sortable: true, align: 'left' },
  { name: 'freight', label: 'Freight', field: 'freight', format: val => `$${Number(val || 0).toFixed(2)}`, sortable: true, align: 'right' },
  { name: 'totalAmount', label: 'Total', field: 'totalAmount', format: val => `$${Number(val).toFixed(2)}`, sortable: true, align: 'right' },
  { name: 'actions', label: '', field: 'actions', align: 'right' }
];

function viewShipment(row) {
  router.push(`/shipments/${row.orderId}`);
}

async function updateStatus(row) {
  try {
    const isDelivered = row._tempStatus === 'Delivered';
    const fullOrder = await store.getOrder(row.orderId);
    if (!fullOrder) return;

    await store.updateOrder(row.orderId, {
      customerId: fullOrder.customerId,
      employeeId: fullOrder.employeeId,
      addressLine: fullOrder.shipAddress,
      city: fullOrder.shipCity,
      region: fullOrder.shipRegion,
      country: fullOrder.shipCountry,
      freight: fullOrder.freight,
      shippedDate: isDelivered ? new Date().toISOString() : null,
      details: (fullOrder.lineItems || []).map(li => ({
        productId: li.productId,
        quantity: li.quantity,
        unitPrice: li.unitPrice
      }))
    });
    q.notify({ color: 'positive', message: 'Status updated!', icon: 'check' });
  } catch (error) {
    q.notify({ color: 'negative', message: 'Failed to update status.' });
  }
}

async function exportOrders(type) {
  try {
    const params = new URLSearchParams();
    if (filterYear.value !== 'All') params.append('year', filterYear.value);
    if (filterMonth.value !== 'All') params.append('month', monthOptions.indexOf(filterMonth.value));
    if (filterRegion.value !== 'All') params.append('region', filterRegion.value);

    const url = `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'}/api/orders/export/${type}?${params.toString()}`;
    
    // Create a temporary link and trigger download
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `NorthwindOrders.${type === 'pdf' ? 'pdf' : 'xlsx'}`);
    document.body.appendChild(link);
    link.click();
    link.remove();

    q.notify({ color: 'positive', message: `Exporting to ${type.toUpperCase()}...`, icon: 'cloud_download' });
  } catch (error) {
    q.notify({ color: 'negative', message: 'Export failed' });
  }
}
</script>

<template>
  <div>
    <div class="row q-mb-md items-end">
      <div class="col-12 col-md-5">
        <h1 class="text-h4 text-weight-bold q-my-none">Tracking & Shipments</h1>
        <p class="text-grey-7 q-mt-sm">Manage and track all global shipments.</p>
      </div>
      <div class="col-12 col-md-7 text-right">
      </div>
    </div>
    <div class="row q-mb-md items-center justify-between">
      <div class="row q-gutter-sm items-center">
        <q-btn color="primary" icon="description" label="Export PDF" @click="exportOrders('pdf')" />
        <q-btn color="secondary" icon="table_view" label="Export Excel" @click="exportOrders('excel')" />
        <q-input
          outlined
          dense
          v-model="search"
          placeholder="Search Order ID, Customer, Region..."
          style="width: 300px"
          class="q-ml-md"
          clearable
        >
          <template v-slot:prepend>
            <q-icon name="search" />
          </template>
        </q-input>
      </div>
      <div class="row q-gutter-sm justify-end">
        <q-select outlined dense v-model="filterYear" :options="yearOptions" label="Year" style="width: 100px" />
        <q-select outlined dense v-model="filterMonth" :options="monthOptions" label="Month" style="width: 100px" />
        <q-select outlined dense v-model="filterRegion" :options="regionOptions" label="Region" style="width: 130px" />
        <q-select outlined dense v-model="filterStatus" :options="['All', 'In Transit', 'Delivered']" label="Status" style="width: 120px" />
      </div>
    </div>
    <q-card flat bordered class="rounded-borders relative-position">
      <q-table
        flat
        :rows="filteredOrders"
        :columns="columns"
        row-key="orderId"
        :pagination="{ rowsPerPage: 15 }"
      >
        <template v-slot:body-cell-status="props">
          <q-td :props="props">
            <q-badge :color="props.row.shippedDate ? 'positive' : 'info'" class="q-pa-xs cursor-pointer">
              {{ props.row.shippedDate ? 'Delivered' : 'In Transit' }}
              <q-popup-edit v-model="props.row.shippedDate" title="Update Status" v-slot="scope">
                <q-select v-model="props.row._tempStatus" :options="['In Transit', 'Delivered']" dense autofocus />
                <div class="row justify-end q-mt-sm">
                  <q-btn flat color="primary" label="SET" @click="() => { scope.set(); updateStatus(props.row); }" />
                </div>
              </q-popup-edit>
            </q-badge>
          </q-td>
        </template>
        <template v-slot:body-cell-actions="props">
          <q-td :props="props" class="text-right">
            <q-btn flat dense color="primary" label="Details" @click="viewShipment(props.row)" />
          </q-td>
        </template>
      </q-table>
      
      <q-inner-loading :showing="store.loading">
        <q-spinner-gears size="50px" color="primary" />
        <div class="q-mt-sm text-primary text-weight-bold">Loading Shipments...</div>
      </q-inner-loading>
    </q-card>
  </div>
</template>
