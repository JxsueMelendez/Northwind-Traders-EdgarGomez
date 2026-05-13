import { defineStore } from 'pinia'
import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080'
})

export const useOrderStore = defineStore('orders', {
  state: () => ({
    orders: [],
    customers: [],
    employees: [],
    products: [],
    loading: false,
    error: null
  }),
  actions: {
    async fetchOrders() {
      this.loading = true
      this.error = null
      try {
        const response = await api.get('/api/orders')
        this.orders = Array.isArray(response.data) ? response.data : []
      } catch (e) {
        this.error = e.message
        this.orders = []
      } finally {
        this.loading = false
      }
    },
    async getOrder(id) {
      try {
        const response = await api.get(`/api/orders/${id}`)
        return response.data
      } catch (e) {
        throw e
      }
    },
    async updateOrder(id, payload) {
      try {
        await api.put(`/api/orders/${id}`, payload)
        await this.fetchOrders()
      } catch (e) {
        throw e
      }
    },
    async createOrder(payload) {
      console.log('Creating order with payload:', payload)
      try {
        const response = await api.post('/api/orders', payload)
        console.log('Order created successfully:', response.data)
        await this.fetchOrders()
      } catch (e) {
        console.error('Error creating order:', e.response?.data || e.message)
        throw e
      }
    },

    // Lookup data
    async fetchCustomers() {
      try {
        const res = await api.get('/api/customers')
        this.customers = Array.isArray(res.data) ? res.data : []
      } catch (e) {
        this.customers = []
      }
    },
    async fetchEmployees() {
      try {
        const res = await api.get('/api/employees')
        this.employees = Array.isArray(res.data) ? res.data : []
      } catch (e) {
        this.employees = []
      }
    },
    async fetchProducts() {
      try {
        const res = await api.get('/api/products')
        this.products = Array.isArray(res.data) ? res.data : []
      } catch (e) {
        this.products = []
      }
    }
  }
})
