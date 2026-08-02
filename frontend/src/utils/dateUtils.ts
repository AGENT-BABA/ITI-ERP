import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';

dayjs.extend(relativeTime);

export function formatDate(date: string): string {
  return dayjs(date).format('DD/MM/YYYY');
}

export function formatDateTime(date: string): string {
  return dayjs(date).format('DD/MM/YYYY HH:mm');
}

export function toRelativeTime(date: string): string {
  return dayjs(date).fromNow();
}

export function getFinancialYear(): string {
  const now = dayjs();
  const currentYear = now.year();
  const financialYearStart = now.month() >= 3 ? currentYear : currentYear - 1;
  return `${financialYearStart}-${(financialYearStart + 1).toString().slice(-2)}`;
}
