<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { createOrder } from '@/services/api'

const router = useRouter()

const amount = ref<number | null>(null)
const buyerPhone = ref('')
const errorMessage = ref('')
const loading = ref(false)

async function onSubmit() {
  errorMessage.value = ''

  if (!amount.value || amount.value <= 0) {
    errorMessage.value = 'Geçerli bir tutar girin.'
    return
  }

  loading.value = true
  try {
    const order = await createOrder(amount.value, buyerPhone.value)
    await router.push({ name: 'checkout', params: { orderNo: order.orderNo } })
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Sipariş oluşturulamadı.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <main class="page">
    <h1>Sipariş oluştur</h1>
    <p class="hint">Demo: gerçek bir sitedeki sepetin yerine geçen ekran.</p>

    <form @submit.prevent="onSubmit" novalidate>
      <label>
        Tutar (TL)
        <input v-model.number="amount" type="number" step="0.01" min="0.01" placeholder="1250,00" />
      </label>

      <label>
        Cep telefonu
        <input v-model="buyerPhone" inputmode="tel" autocomplete="tel" placeholder="0555 555 55 55" />
      </label>

      <small v-if="errorMessage">{{ errorMessage }}</small>

      <button type="submit" :disabled="loading">
        {{ loading ? 'Oluşturuluyor...' : 'Ödemeye geç' }}
      </button>
    </form>
  </main>
</template>

<style scoped>
.page { max-width: 420px; margin: 40px auto; padding: 0 16px; }
.hint { color: #666; font-size: 14px; margin-bottom: 24px; }
form { display: flex; flex-direction: column; gap: 14px; }
label { display: flex; flex-direction: column; gap: 4px; font-size: 14px; }
input { width: 100%; padding: 10px; font-size: 16px; border: 1px solid #bbb; border-radius: 6px; }
small { color: #c0392b; }
button { padding: 12px; font-size: 16px; background: #1f6feb; color: white; border: none; border-radius: 6px; cursor: pointer; }
button:disabled { opacity: 0.6; cursor: default; }
</style>