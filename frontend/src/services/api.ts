export interface BinInfo {
  bin: string
  bankName: string
  bankEftCode: string | null
  cardScheme: string
  cardType: string
  brandName: string | null
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL

export async function getBinInfo(bin: string): Promise<BinInfo | null> {
  const response = await fetch(`${API_BASE_URL}/api/bins/${bin}`)

  if (response.status === 404) return null
  if (!response.ok) throw new Error(`BIN sorgusu başarısız: ${response.status}`)

  return (await response.json()) as BinInfo
}