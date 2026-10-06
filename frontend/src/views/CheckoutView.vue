<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { getBinInfo, getInstallments, type BinInfo, type InstallmentOption } from '@/services/api'
import { formatCardNumber, isValidExpiry, isValidLuhn, onlyDigits } from '@/utils/card'

const amount = 1250

const currency = new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' })
const formatTry = (value: number) => currency.format(value)

const cardHolder = ref('')
const cardDigits = ref('')
const expiryMonth = ref('')
const expiryYear = ref('')
const cvv = ref('')
const submitted = ref(false)

const binInfo = ref<BinInfo | null>(null)
const binStatus = ref<'idle' | 'loading' | 'found' | 'notFound' | 'error'>('idle')

const installments = ref<InstallmentOption[]>([])
const selectedInstallment = ref(1)

const formattedCardNumber = computed(() => formatCardNumber(cardDigits.value))

function onCardInput(event: Event) {
  const input = event.target as HTMLInputElement
  cardDigits.value = onlyDigits(input.value).slice(0, 16)
  input.value = formattedCardNumber.value
}

// İlk 6 hanede sorgula, 8 hane olunca daha kesin sonuç için tekrar sorgula
const binKey = computed(() => {
  const d = cardDigits.value
  if (d.length >= 8) return d.slice(0, 8)
  if (d.length >= 6) return d.slice(0, 6)
  return ''
})

watch(binKey, async (key) => {
  binInfo.value = null
  installments.value = []
  selectedInstallment.value = 1

  if (!key) {
    binStatus.value = 'idle'
    return
  }

  binStatus.value = 'loading'
  try {
    const info = await getBinInfo(key)
    if (binKey.value !== key) return // bu arada kullanıcı numarayı değiştirdiyse eski yanıtı yok say
    binInfo.value = info
    binStatus.value = info ? 'found' : 'notFound'

    if (info) {
      const options = await getInstallments(key, amount)
      if (binKey.value !== key) return
      installments.value = options
    }
  } catch {
    binStatus.value = 'error'
  }
})

// Seçili taksitin toplam tutarı; taksitler henüz yüklenmediyse sipariş tutarı
const payableAmount = computed(() => {
  const option = installments.value.find((o) => o.count === selectedInstallment.value)
  return option ? option.totalAmount : amount
})

const errors = computed(() => ({
  cardHolder: cardHolder.value.trim().length < 3 ? 'Kart üzerindeki ismi girin.' : '',
  cardNumber: !isValidLuhn(cardDigits.value) ? 'Geçerli bir kart numarası girin.' : '',
  expiry: !isValidExpiry(expiryMonth.value, expiryYear.value) ? 'Son kullanma tarihi geçersiz.' : '',
  cvv: !/^\d{3,4}$/.test(cvv.value) ? 'CVV 3 veya 4 haneli olmalı.' : '',
}))

const isFormValid = computed(() => Object.values(errors.value).every((e) => !e))

function onSubmit() {
  submitted.value = true
  if (!isFormValid.value) return
  alert(`Form geçerli. ${selectedInstallment.value} taksit, ${formatTry(payableAmount.value)}. Ödeme akışı Faz 5'te bağlanacak.`)
}
</script>

<template>
  <main class="checkout">
    <h1>Ödeme</h1>
    <p class="amount">Sipariş tutarı: <strong>{{ formatTry(amount) }}</strong></p>

    <form @submit.prevent="onSubmit" novalidate>
      <label>
        Kart üzerindeki isim
        <input v-model="cardHolder" autocomplete="cc-name" placeholder="Ad Soyad" />
        <small v-if="submitted && errors.cardHolder">{{ errors.cardHolder }}</small>
      </label>

      <label>
        Kart numarası
        <input
          :value="formattedCardNumber"
          @input="onCardInput"
          inputmode="numeric"
          autocomplete="cc-number"
          placeholder="0000 0000 0000 0000"
        />
        <small v-if="submitted && errors.cardNumber">{{ errors.cardNumber }}</small>
      </label>

      <div v-if="binStatus !== 'idle'" class="bank-info">
        <span v-if="binStatus === 'loading'">Banka sorgulanıyor...</span>
        <template v-else-if="binStatus === 'found' && binInfo">
          <strong>{{ binInfo.bankName }}</strong>
          <span>
            {{ binInfo.cardScheme }} · {{ binInfo.cardType }}
            <template v-if="binInfo.brandName && binInfo.brandName !== 'Marka Yok'">
              · {{ binInfo.brandName }}
            </template>
          </span>
        </template>
        <span v-else-if="binStatus === 'notFound'">Bu kartın bankası bulunamadı.</span>
        <span v-else>Banka bilgisi alınamadı.</span>
      </div>

      <fieldset v-if="installments.length > 0" class="installments">
        <legend>Taksit seçenekleri</legend>
        <label v-for="option in installments" :key="option.count" class="installment-option">
          <input type="radio" :value="option.count" v-model="selectedInstallment" />
          <span>{{ option.count === 1 ? 'Tek çekim' : `${option.count} taksit` }}</span>
          <span v-if="option.count > 1" class="monthly">{{ option.count }} × {{ formatTry(option.monthlyAmount) }}</span>
          <strong>{{ formatTry(option.totalAmount) }}</strong>
        </label>
      </fieldset>

      <div class="row">
        <label>
          Ay
          <input v-model="expiryMonth" inputmode="numeric" maxlength="2" placeholder="AA" />
        </label>
        <label>
          Yıl
          <input v-model="expiryYear" inputmode="numeric" maxlength="2" placeholder="YY" />
        </label>
        <label>
          CVV
          <input v-model="cvv" type="password" inputmode="numeric" maxlength="4" autocomplete="cc-csc" />
        </label>
      </div>
      <small v-if="submitted && errors.expiry">{{ errors.expiry }}</small>
      <small v-if="submitted && errors.cvv">{{ errors.cvv }}</small>

      <button type="submit">{{ formatTry(payableAmount) }} Öde</button>
    </form>
  </main>
</template>

<style scoped>
.checkout { max-width: 420px; margin: 40px auto; padding: 0 16px; }
.amount { margin-bottom: 24px; }
form { display: flex; flex-direction: column; gap: 14px; }
label { display: flex; flex-direction: column; gap: 4px; font-size: 14px; }
input { width: 100%; padding: 10px; font-size: 16px; border: 1px solid #bbb; border-radius: 6px; }
.row { display: flex; gap: 10px; }
.row label { flex: 1; min-width: 0; }
small { color: #c0392b; }
.bank-info { display: flex; flex-direction: column; padding: 10px; background: #eef4ff; border-radius: 6px; }
.installments { border: 1px solid #ddd; border-radius: 6px; padding: 8px 12px; margin: 0; }
.installments legend { font-size: 14px; padding: 0 4px; }
.installment-option { flex-direction: row; align-items: center; gap: 10px; padding: 6px 0; cursor: pointer; }
.installment-option input { width: auto; }
.installment-option .monthly { color: #666; flex: 1; text-align: right; }
.installment-option strong { min-width: 90px; text-align: right; margin-left: auto; }
button { padding: 12px; font-size: 16px; background: #1f6feb; color: white; border: none; border-radius: 6px; cursor: pointer; }
</style>