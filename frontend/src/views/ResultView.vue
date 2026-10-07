<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { getPayment, type PaymentResult } from '@/services/api'

const route = useRoute()
const paymentNo = route.params.paymentNo as string

const payment = ref<PaymentResult | null>(null)
const pageStatus = ref<'loading' | 'ready' | 'error'>('loading')
const pageError = ref('')

const currency = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' })
const formatTry = (value: number) => currency.format(value)

// API tarihi UTC ("...Z") döner; tarayıcı kendi saat dilimine çevirir
const formatDate = (iso: string) => new Date(iso).toLocaleString('tr-TR')

onMounted(async () => {
  if (paymentNo === 'unknown') {
    pageError.value = 'Ödeme bilgisi bulunamadı.'
    pageStatus.value = 'error'
    return
  }

  try {
    const result = await getPayment(paymentNo)
    if (!result) {
      pageError.value = 'Ödeme bulunamadı.'
      pageStatus.value = 'error'
      return
    }
    payment.value = result
    pageStatus.value = 'ready'
  } catch (error) {
    pageError.value = error instanceof Error ? error.message : 'Ödeme bilgisi alınamadı.'
    pageStatus.value = 'error'
  }
})
</script>

<template>
  <main class="result">
    <p v-if="pageStatus === 'loading'">Ödeme sonucu yükleniyor...</p>

    <div v-else-if="pageStatus === 'error'" class="box failed">
      <h1>Bir sorun oluştu</h1>
      <p>{{ pageError }}</p>
      <RouterLink to="/">Yeni sipariş oluştur</RouterLink>
    </div>

    <template v-else-if="payment">
      <div v-if="payment.status === 'Succeeded'" class="box success">
        <div class="icon">✓</div>
        <h1>Ödemeniz alındı</h1>
        <p>{{ formatTry(payment.amount) }} tutarındaki ödemeniz başarıyla tamamlandı.</p>
      </div>

      <div v-else-if="payment.status === 'Pending3DS'" class="box pending">
        <h1>Ödeme işleniyor</h1>
        <p>3D Secure doğrulaması henüz tamamlanmadı.</p>
      </div>

      <div v-else class="box failed">
        <div class="icon">✕</div>
        <h1>Ödeme başarısız</h1>
        <p>{{ payment.errorMessage ?? 'Ödeme tamamlanamadı.' }}</p>
      </div>

      <table class="details">
        <tr><td>Banka</td><td>{{ payment.bankName ?? '-' }}</td></tr>
        <tr><td>Kart</td><td>{{ payment.maskedCardNumber }}</td></tr>
        <tr><td>Taksit</td><td>{{ payment.installment === 1 ? 'Tek çekim' : `${payment.installment} taksit` }}</td></tr>
        <tr><td>Tutar</td><td>{{ formatTry(payment.amount) }}</td></tr>
        <tr><td>Ödeme no</td><td>{{ payment.paymentNo.slice(0, 8).toUpperCase() }}</td></tr>
        <tr><td>Tarih</td><td>{{ formatDate(payment.createdAt) }}</td></tr>
      </table>

      <div class="actions">
        <RouterLink
          v-if="payment.status === 'Failed'"
          :to="{ name: 'checkout', params: { orderNo: payment.orderNo } }"
          class="button"
        >
          Tekrar dene
        </RouterLink>
        <RouterLink to="/" class="button secondary">Yeni sipariş</RouterLink>
      </div>
    </template>
  </main>
</template>

<style scoped>
.result { max-width: 420px; margin: 40px auto; padding: 0 16px; }
.box { padding: 20px; border-radius: 8px; text-align: center; margin-bottom: 20px; }
.box h1 { font-size: 22px; margin: 8px 0; }
.success { background: #e8f6ee; color: #1e7b45; }
.failed { background: #fdecea; color: #a8322a; }
.pending { background: #fff8e1; color: #8a6d00; }
.icon { font-size: 36px; font-weight: bold; }
.details { width: 100%; font-size: 14px; border-collapse: collapse; margin-bottom: 20px; }
.details td { padding: 8px 4px; border-bottom: 1px solid #e5e5e5; }
.details td:first-child { color: #666; }
.details td:last-child { text-align: right; }
.actions { display: flex; gap: 10px; }
.button { flex: 1; text-align: center; padding: 12px; border-radius: 6px; background: #1f6feb; color: #fff; text-decoration: none; }
.button.secondary { background: #ddd; color: #333; }
</style>