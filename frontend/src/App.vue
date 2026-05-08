<script setup>
import { computed } from 'vue';
import { useQuasar } from 'quasar';
import { useRouter, useRoute } from 'vue-router';
import axios from 'axios';

const q = useQuasar();
const router = useRouter();
const route = useRoute();

const activeTab = computed({
  get: () => {
    if (route.path.includes('/dashboard')) return 'dashboard';
    if (route.path.includes('/shipments')) return 'shipments';
    if (route.path.includes('/new-order')) return 'create-order';
    return 'dashboard';
  },
  set: (val) => {
    if (val === 'dashboard') router.push('/dashboard');
    if (val === 'shipments') router.push('/shipments');
    if (val === 'create-order') router.push('/new-order');
  }
});

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'
});

function exportExcel() {
  q.notify({ color: 'primary', message: 'Downloading Excel Report...' });
  window.open(`${api.defaults.baseURL}/api/orders/export/excel`, '_blank');
}

function exportPdf() {
  q.notify({ color: 'primary', message: 'Downloading PDF Report...' });
  window.open(`${api.defaults.baseURL}/api/orders/export/pdf`, '_blank');
}
</script>

<template>
  <q-layout view="hHh lpR fFf" class="bg-white text-black" style="font-family: 'Inter', sans-serif;">
    <!-- Top Navigation (Vercel Style) -->
    <q-header bordered class="bg-white text-black" style="border-bottom: 1px solid #eaeaea;">
      <div class="q-px-lg flex items-center" style="height: 64px;">
        <div class="text-weight-bold flex items-center q-mr-xl" style="font-size: 1.2rem; cursor: pointer;" @click="activeTab = 'dashboard'">
          <q-icon name="change_history" size="md" class="q-mr-sm" color="primary"/>
          Northwind OMS
        </div>
        <q-tabs v-model="activeTab" no-caps dense class="text-grey-7" active-color="primary" indicator-color="primary">
          <q-tab name="dashboard" label="Overview" />
          <q-tab name="shipments" label="Tracking & Shipments" />
          <q-tab name="create-order" label="New Order" />
        </q-tabs>
        <q-space />
        <q-btn flat dense icon="picture_as_pdf" @click="exportPdf" tooltip="Export PDF" class="q-mr-sm text-grey-8" />
        <q-btn flat dense icon="table_view" @click="exportExcel" tooltip="Export Excel" class="text-grey-8" />
      </div>
    </q-header>

    <q-page-container>
      <q-page padding class="q-pa-xl" style="max-width: 1400px; margin: 0 auto;">
         <router-view v-slot="{ Component }">
            <transition name="fade" mode="out-in">
               <component :is="Component" />
            </transition>
         </router-view>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<style>
.h-full {
  height: 100%;
}
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
