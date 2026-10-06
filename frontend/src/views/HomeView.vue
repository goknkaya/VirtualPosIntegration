<script setup lang="ts">
import { ref } from 'vue'

interface WeatherForecast {
  date: string
  temperatureC: number
  summary: string | null
}

const forecasts = ref<WeatherForecast[]>([])
const errorMessage = ref('')

async function loadForecasts() {
  try {
    const response = await fetch('https://localhost:7177/weatherforecast')
    forecasts.value = await response.json()
  } catch (error) {
    errorMessage.value = 'API isteği başarısız oldu. Konsolu kontrol et (F12).'
  }
}
</script>

<template>
  <main>
    <button @click="loadForecasts">API'den veri getir</button>
    <p v-if="errorMessage">{{ errorMessage }}</p>
    <ul>
      <li v-for="item in forecasts" :key="item.date">
        {{ item.date }}: {{ item.temperatureC }} °C, {{ item.summary }}
      </li>
    </ul>
  </main>
</template>