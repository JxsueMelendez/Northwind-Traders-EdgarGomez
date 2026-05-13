<script setup>
import { ref, computed, onMounted } from 'vue';
import { useOrderStore } from '../stores/orderStore';
import { useRouter } from 'vue-router';
import { useQuasar } from 'quasar';
import RealTimeMap from '../components/RealTimeMap.vue';

const store = useOrderStore();
const router = useRouter();
const q = useQuasar();

const step = ref(1);

const orderForm = ref({
  customerId: null,       // full CustomerDto object until submit
  employeeId: null,       // full EmployeeDto object until submit
  addressLine: '',
  city: '',
  region: '',
  country: '',
  freight: 0,
  details: [{ product: null, quantity: 1, unitPrice: 0 }]
});

// ------------------------------------------------------------------
// Step 1: Customer & Employee autocomplete
// ------------------------------------------------------------------
const customerSearch = ref('');
const employeeSearch = ref('');

const filteredCustomers = computed(() => {
  const q = customerSearch.value?.toLowerCase() || '';
  if (!q) return store.customers.slice(0, 20);
  return store.customers
    .filter(c => c.customerId.toLowerCase().includes(q) || c.companyName.toLowerCase().includes(q))
    .slice(0, 20);
});

const filteredEmployees = computed(() => {
  const q = employeeSearch.value?.toLowerCase() || '';
  if (!q) return store.employees.slice(0, 20);
  return store.employees
    .filter(e =>
      String(e.employeeId).includes(q) ||
      e.firstName.toLowerCase().includes(q) ||
      e.lastName.toLowerCase().includes(q)
    )
    .slice(0, 20);
});

function filterCustomers(val, update) {
  customerSearch.value = val;
  update();
}

function filterEmployees(val, update) {
  employeeSearch.value = val;
  update();
}

// ------------------------------------------------------------------
// Step 3: Product autocomplete per line item
// ------------------------------------------------------------------
function filteredProducts(searchVal) {
  const q = (searchVal || '').toLowerCase();
  if (!q) return store.products.slice(0, 20);
  return store.products
    .filter(p =>
      String(p.productId).includes(q) ||
      p.productName.toLowerCase().includes(q)
    )
    .slice(0, 20);
}

function onProductSelected(item, product) {
  if (product) {
    item.product = product;
    item.unitPrice = product.unitPrice;
  }
}

function addLineItem() {
  orderForm.value.details.push({ product: null, quantity: 1, unitPrice: 0 });
}

function removeLineItem(index) {
  if (orderForm.value.details.length === 1) return;
  orderForm.value.details.splice(index, 1);
}

const lineItemsTotal = computed(() =>
  orderForm.value.details.reduce((sum, item) => sum + (item.quantity * item.unitPrice), 0)
);

// ------------------------------------------------------------------
// Lifecycle
// ------------------------------------------------------------------
onMounted(async () => {
  await Promise.all([
    store.fetchCustomers(),
    store.fetchEmployees(),
    store.fetchProducts()
  ]);
});

function onLocationSelected(loc) {
  orderForm.value.addressLine = loc.address;
  orderForm.value.city = loc.city;
  orderForm.value.region = loc.region;
  orderForm.value.country = loc.country;
  q.notify({ 
    color: 'info', 
    message: 'Address updated from map!', 
    icon: 'location_on', 
    position: 'bottom-right',
    timeout: 2000
  });
}

// ------------------------------------------------------------------
// Submit
// ------------------------------------------------------------------
async function submitOrder() {
  if (!orderForm.value.customerId) {
    q.notify({ color: 'negative', message: 'Please select a customer.', icon: 'warning' });
    return;
  }

  // Deep validation of line items
  const details = orderForm.value.details;
  
  if (details.length === 0) {
    q.notify({ color: 'negative', message: 'Add at least one product line item.', icon: 'warning' });
    return;
  }

  const invalidItems = details.filter(d => !d.product || d.unitPrice <= 0 || d.quantity <= 0);
  if (invalidItems.length > 0) {
    q.dialog({
      title: 'Invalid Line Items',
      message: 'Some products are not selected or have invalid price/quantity (must be greater than 0). Please correct them before saving.',
      color: 'negative',
      persistent: true
    });
    return;
  }

  try {
    q.loading.show();
    await store.createOrder({
      customerId: orderForm.value.customerId,
      employeeId: orderForm.value.employeeId || null,
      addressLine: orderForm.value.addressLine,
      city: orderForm.value.city,
      region: orderForm.value.region,
      country: orderForm.value.country,
      freight: orderForm.value.freight,
      details: details.map(d => ({
        productId: d.product.productId,
        quantity: d.quantity,
        unitPrice: d.unitPrice
      }))
    });
    q.notify({ color: 'positive', message: 'Order created successfully!', icon: 'check' });
    router.push('/shipments');
  } catch (error) {
    q.notify({ color: 'negative', message: error?.response?.data?.message || 'Failed to create order.' });
  } finally {
    q.loading.hide();
  }
}
</script>

<template>
  <div>
    <div class="row q-mb-md">
      <div class="col-12">
        <h1 class="text-h4 text-weight-bold q-my-none">Create Order</h1>
        <p class="text-grey-7 q-mt-sm">Follow the steps below to initialize a new shipment.</p>
      </div>
    </div>

    <q-card flat bordered class="rounded-borders">
      <q-stepper v-model="step" color="primary" flat animated>

        <!-- ===== Step 1: Customer & Employee ===== -->
        <q-step :name="1" title="Customer & Employee" icon="person" :done="step > 1">
          <div class="q-pa-md">
            <!-- Customer autocomplete -->
            <div class="text-subtitle2 text-weight-bold q-mb-xs">Customer *</div>
            <q-select
              outlined
              dense
              v-model="orderForm.customerId"
              :options="filteredCustomers"
              option-label="companyName"
              option-value="customerId"
              emit-value
              map-options
              label="Search by ID or company name..."
              use-input
              input-debounce="200"
              @filter="filterCustomers"
              class="q-mb-xs"
              style="max-width: 480px;"
              clearable
            >
              <template v-slot:option="scope">
                <q-item v-bind="scope.itemProps">
                  <q-item-section>
                    <q-item-label>{{ scope.opt.companyName }}</q-item-label>
                    <q-item-label caption>ID: {{ scope.opt.customerId }} · {{ scope.opt.contactName }}</q-item-label>
                  </q-item-section>
                </q-item>
              </template>
              <template v-slot:no-option>
                <q-item><q-item-section class="text-grey-6">No customers found</q-item-section></q-item>
              </template>
            </q-select>
            <div v-if="orderForm.customerId" class="q-mb-lg text-caption text-grey-8">
              <q-chip dense color="primary" text-color="white" icon="badge">
                {{ orderForm.customerId.customerId }}
              </q-chip>
            </div>

            <!-- Employee autocomplete -->
            <div class="text-subtitle2 text-weight-bold q-mb-xs">Employee (Optional)</div>
            <q-select
              outlined
              dense
              v-model="orderForm.employeeId"
              :options="filteredEmployees"
              option-label="fullName"
              option-value="employeeId"
              emit-value
              map-options
              label="Search by ID or name..."
              use-input
              input-debounce="200"
              @filter="filterEmployees"
              style="max-width: 480px;"
              clearable
            >
              <template v-slot:option="scope">
                <q-item v-bind="scope.itemProps">
                  <q-item-section avatar>
                    <q-avatar color="primary" text-color="white" size="32px">
                      {{ scope.opt.firstName?.charAt(0) }}{{ scope.opt.lastName?.charAt(0) }}
                    </q-avatar>
                  </q-item-section>
                  <q-item-section>
                    <q-item-label>{{ scope.opt.firstName }} {{ scope.opt.lastName }}</q-item-label>
                    <q-item-label caption>Employee ID: {{ scope.opt.employeeId }}</q-item-label>
                  </q-item-section>
                </q-item>
              </template>
              <template v-slot:no-option>
                <q-item><q-item-section class="text-grey-6">No employees found</q-item-section></q-item>
              </template>
            </q-select>
          </div>
          <q-stepper-navigation>
            <q-btn @click="step = 2" color="primary" label="Continue" unelevated :disabled="!orderForm.customerId" />
          </q-stepper-navigation>
        </q-step>

        <!-- ===== Step 2: Shipping Address & Freight ===== -->
        <q-step :name="2" title="Shipping & Freight" icon="local_shipping" :done="step > 2">
          <div class="row q-col-gutter-lg q-pa-md">
            <div class="col-12 col-md-6">
              <q-input 
                outlined 
                dense 
                v-model="orderForm.addressLine" 
                label="Street Address *" 
                class="q-mb-md" 
                :rules="[val => !!val || 'Required']"
              />
              <div class="row q-col-gutter-md q-mb-md">
                <div class="col-4">
                  <q-input 
                    outlined 
                    dense 
                    v-model="orderForm.city" 
                    label="City *" 
                    :rules="[val => !!val || 'Required']"
                  />
                </div>
                <div class="col-4">
                  <q-input outlined dense v-model="orderForm.region" label="Region" />
                </div>
                <div class="col-4">
                  <q-input 
                    outlined 
                    dense 
                    v-model="orderForm.country" 
                    label="Country *" 
                    :rules="[val => !!val || 'Required']"
                  />
                </div>
              </div>
              <q-input outlined dense type="number" step="0.01" v-model.number="orderForm.freight" label="Freight Charges ($)" prefix="$" style="max-width: 220px;" :rules="[val => val >= 0 || 'Cannot be negative']" />
            </div>
            <div class="col-12 col-md-6">
              <div class="text-caption text-grey-7 q-mb-sm">
                <q-icon name="info" color="primary" /> Tip: Click on the map to automatically fill the address.
              </div>
              <RealTimeMap 
                :city="orderForm.city" 
                :country="orderForm.country" 
                clickable
                @location-selected="onLocationSelected"
              />
            </div>
          </div>
          <q-stepper-navigation class="q-mt-md">
            <q-btn 
              @click="step = 3" 
              color="primary" 
              label="Continue" 
              unelevated 
              :disabled="!orderForm.addressLine || !orderForm.city || !orderForm.country || orderForm.freight < 0" 
            />
            <q-btn flat @click="step = 1" color="grey-8" label="Back" class="q-ml-sm" />
          </q-stepper-navigation>
        </q-step>

        <!-- ===== Step 3: Line Items ===== -->
        <q-step :name="3" title="Line Items" icon="shopping_cart">
          <div class="q-pa-md">
            <div class="text-h6 text-weight-bold q-mb-lg">Order Details & Line Items</div>
            
            <!-- Grid Header -->
            <div class="row q-col-gutter-sm text-grey-7 text-caption text-weight-bold q-mb-sm q-px-md items-center gt-sm">
              <div class="col-12 col-md-5">PRODUCT</div>
              <div class="col-4 col-md-2">QUANTITY</div>
              <div class="col-4 col-md-2">UNIT PRICE</div>
              <div class="col-4 col-md-2 text-right">SUBTOTAL</div>
              <div class="col-12 col-md-1"></div>
            </div>

            <div class="q-gutter-y-sm q-mb-xl">
              <q-card 
                v-for="(item, index) in orderForm.details"
                :key="index"
                flat 
                bordered 
                class="rounded-borders q-pa-sm"
              >
                <div class="row q-col-gutter-sm items-start">
                  <!-- Product search autocomplete -->
                  <div class="col-12 col-md-5">
                    <q-select
                      outlined
                      dense
                      v-model="item.product"
                      :options="filteredProducts(item._search)"
                      option-label="productName"
                      label="Search Product *"
                      use-input
                      input-debounce="200"
                      @filter="(val, update) => { item._search = val; update(); }"
                      @update:model-value="(p) => onProductSelected(item, p)"
                      clearable
                    >
                      <template v-slot:option="scope">
                        <q-item v-bind="scope.itemProps">
                          <q-item-section>
                            <q-item-label>{{ scope.opt.productName }}</q-item-label>
                            <q-item-label caption>
                              ID: {{ scope.opt.productId }} · Stock: {{ scope.opt.unitsInStock }} · Price: ${{ scope.opt.unitPrice }}
                            </q-item-label>
                          </q-item-section>
                        </q-item>
                      </template>
                    </q-select>
                    <div v-if="item.product" class="q-mt-xs q-ml-xs">
                      <q-badge color="grey-3" text-color="grey-9" label="ID:" class="q-mr-xs" />
                      <span class="text-caption text-weight-bold">{{ item.product.productId }}</span>
                    </div>
                  </div>

                  <div class="col-6 col-md-2">
                    <q-input outlined dense type="number" v-model.number="item.quantity" label="Qty" min="1" stack-label />
                  </div>
                  
                  <div class="col-6 col-md-2">
                    <q-input 
                      outlined 
                      dense 
                      type="number" 
                      step="0.01" 
                      v-model.number="item.unitPrice" 
                      label="Price" 
                      prefix="$" 
                      stack-label
                      :rules="[val => val > 0 || 'Required']"
                      hide-bottom-space
                    />
                  </div>
                  
                  <div class="col-12 col-md-2 text-right self-center">
                    <div class="lt-md text-caption text-grey-6">Subtotal</div>
                    <div class="text-subtitle1 text-weight-bolder">${{ (item.quantity * item.unitPrice).toFixed(2) }}</div>
                  </div>
                  
                  <div class="col-12 col-md-1 flex flex-center">
                    <q-btn 
                      icon="delete" 
                      color="negative" 
                      flat 
                      round 
                      dense 
                      @click="removeLineItem(index)" 
                      :disabled="orderForm.details.length === 1"
                    />
                  </div>
                </div>
              </q-card>
            </div>

            <q-btn 
              unelevated 
              color="primary" 
              icon="add" 
              label="Add Product" 
              @click="addLineItem" 
              class="q-px-md"
            />

            <div class="row justify-end q-mt-xl">
              <div class="col-12 col-md-5">
                <q-card flat bordered class="bg-grey-1">
                  <q-card-section class="q-py-md">
                    <div class="row justify-between q-mb-sm">
                      <div class="text-grey-7">Items Subtotal</div>
                      <div class="text-weight-medium">${{ lineItemsTotal.toFixed(2) }}</div>
                    </div>
                    <div class="row justify-between q-mb-md">
                      <div class="text-grey-7">Freight Charges</div>
                      <div class="text-weight-medium text-blue-8">+ ${{ orderForm.freight.toFixed(2) }}</div>
                    </div>
                    <q-separator class="q-mb-md" />
                    <div class="row justify-between items-center">
                      <div class="text-subtitle1 text-weight-bold">Total Amount</div>
                      <div class="text-h5 text-weight-bolder text-primary">${{ (lineItemsTotal + orderForm.freight).toFixed(2) }}</div>
                    </div>
                  </q-card-section>
                </q-card>
              </div>
            </div>
          </div>
          <q-stepper-navigation>
            <q-btn color="primary" @click="submitOrder" label="Finish & Create Order" unelevated />
            <q-btn flat @click="step = 2" color="grey-8" label="Back" class="q-ml-sm" />
          </q-stepper-navigation>
        </q-step>

      </q-stepper>
    </q-card>
  </div>
</template>

<style scoped>
.border-bottom-light {
  border-bottom: 1px solid #e0e0e0;
}
.border-bottom-light:last-child {
  border-bottom: none;
}
</style>
