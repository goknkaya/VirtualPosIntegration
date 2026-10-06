export function onlyDigits(value: string): string {
  return value.replace(/\D/g, '')
}

export function formatCardNumber(digits: string): string {
  return digits.replace(/(\d{4})(?=\d)/g, '$1 ')
}

export function isValidLuhn(digits: string): boolean {
  if (digits.length < 12) return false

  let sum = 0
  let shouldDouble = false

  for (let i = digits.length - 1; i >= 0; i--) {
    let digit = Number(digits[i])
    if (shouldDouble) {
      digit *= 2
      if (digit > 9) digit -= 9
    }
    sum += digit
    shouldDouble = !shouldDouble
  }

  return sum % 10 === 0
}

export function isValidExpiry(month: string, year: string): boolean {
  const m = Number(month)
  if (!m || m < 1 || m > 12 || year.length !== 2) return false

  const lastDayOfMonth = new Date(2000 + Number(year), m, 0, 23, 59, 59)
  return lastDayOfMonth >= new Date()
}