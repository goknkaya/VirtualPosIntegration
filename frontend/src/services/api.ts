// ---------- Tipler (C#'taki DTO'ların karşılığı) ----------

export interface BinInfo {
  bin: string
  bankName: string
  bankEftCode: string | null
  cardScheme: string
  cardType: string
  brandName: string | null
  isCreditCard: boolean
}

export interface InstallmentOption {
  count: number
  totalAmount: number
  monthlyAmount: number
}

export interface Order {
  orderNo: string
  amount: number
  currency: string
  buyerPhoneMasked: string
  status: string
  createdAt: string
}

export interface InitializePaymentRequest {
  orderNo: string
  cardHolderName: string
  cardNumber: string
  expireMonth: string
  expireYear: string
  cvc: string
  installment: number
}

export interface InitializePaymentResult {
  isSuccess: boolean
  paymentNo: string | null
  redirectUrl: string | null
  htmlContent: string | null
  errorMessage: string | null
}

export interface PaymentResult {
  paymentNo: string
  orderNo: string
  status: 'Pending3DS' | 'Succeeded' | 'Failed' | 'Cancelled'
  amount: number
  installment: number
  maskedCardNumber: string
  bankName: string | null
  errorMessage: string | null
  createdAt: string
}

// ---------- Ortak ----------

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

// API'nin döndüğü { message: "..." } hata metnini okur
async function readError(response: Response): Promise<string> {
  try {
    const body = await response.json()
    return body.message ?? `İstek başarısız (${response.status})`
  } catch {
    return `İstek başarısız (${response.status})`
  }
}

// ---------- BIN ve taksit ----------

export async function getBinInfo(bin: string): Promise<BinInfo | null> {
  const response = await fetch(`${API_BASE_URL}/api/bins/${bin}`)
  if (response.status === 404) return null
  if (!response.ok) throw new Error(await readError(response))
  return (await response.json()) as BinInfo
}

export async function getInstallments(bin: string, amount: number): Promise<InstallmentOption[]> {
  const response = await fetch(`${API_BASE_URL}/api/installments?bin=${bin}&amount=${amount}`)
  if (!response.ok) throw new Error(await readError(response))
  return (await response.json()) as InstallmentOption[]
}

// ---------- Sipariş ----------

export async function createOrder(amount: number, buyerPhone: string): Promise<Order> {
  const response = await fetch(`${API_BASE_URL}/api/orders`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ amount, buyerPhone }),
  })
  if (!response.ok) throw new Error(await readError(response))
  return (await response.json()) as Order
}

export async function getOrder(orderNo: string): Promise<Order | null> {
  const response = await fetch(`${API_BASE_URL}/api/orders/${orderNo}`)
  if (response.status === 404) return null
  if (!response.ok) throw new Error(await readError(response))
  return (await response.json()) as Order
}

// ---------- Ödeme ----------

export async function initializePayment(request: InitializePaymentRequest): Promise<InitializePaymentResult> {
  const response = await fetch(`${API_BASE_URL}/api/payments/3ds/initialize`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  })
  if (!response.ok) throw new Error(await readError(response))
  return (await response.json()) as InitializePaymentResult
}

export async function getPayment(paymentNo: string): Promise<PaymentResult | null> {
  const response = await fetch(`${API_BASE_URL}/api/payments/${paymentNo}`)
  if (response.status === 404) return null
  if (!response.ok) throw new Error(await readError(response))
  return (await response.json()) as PaymentResult
}