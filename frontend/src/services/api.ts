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

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export async function getBinInfo(bin: string): Promise<BinInfo | null> {
  const response = await fetch(`${API_BASE_URL}/api/bins/${bin}`)

  if (response.status === 404) return null
  if (!response.ok) throw new Error(`BIN sorgusu başarısız: ${response.status}`)

  return (await response.json()) as BinInfo
}

export async function getInstallments(bin: string, amount: number): Promise<InstallmentOption[]> {
  const response = await fetch(`${API_BASE_URL}/api/installments?bin=${bin}&amount=${amount}`)

  if (!response.ok) throw new Error(`Taksit sorgusu başarısız: ${response.status}`)

  return (await response.json()) as InstallmentOption[]
}