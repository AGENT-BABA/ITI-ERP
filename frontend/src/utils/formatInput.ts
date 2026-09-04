/**
 * Format a 10-digit phone number as "XXXXX XXXXX" (5-5)
 * Strips non-digits and truncates to 10 digits.
 */
export function formatPhone(value: string): string {
  const digits = value.replace(/\D/g, '').slice(0, 10);
  if (digits.length <= 5) return digits;
  return `${digits.slice(0, 5)} ${digits.slice(5)}`;
}

/**
 * Return raw digits only from a formatted or unformatted phone string.
 */
export function rawPhone(value: string): string {
  return value.replace(/\D/g, '').slice(0, 10);
}

/**
 * Format a 12-digit Aadhar number as "XXXX XXXX XXXX" (4-4-4)
 * Strips non-digits and truncates to 12 digits.
 */
export function formatAadhar(value: string): string {
  const digits = value.replace(/\D/g, '').slice(0, 12);
  const parts: string[] = [];
  for (let i = 0; i < digits.length; i += 4) {
    parts.push(digits.slice(i, i + 4));
  }
  return parts.join(' ');
}

/**
 * Return raw digits only from a formatted or unformatted Aadhar string.
 */
export function rawAadhar(value: string): string {
  return value.replace(/\D/g, '').slice(0, 12);
}
