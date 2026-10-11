export function plural(n: number, one: string, few: string, many: string) {
    const n10 = n % 10;
    const n100 = n % 100;
    if (n10 === 1 && n100 !== 11) return one;
    if (n10 >= 2 && n10 <= 4 && (n100 < 10 || n100 >= 20)) return few;
    return many;
}

export function formatPrice(price: number) {
    return `${price.toLocaleString("ru-RU")} ₽`;
}

export function formatTime(iso: string) {
    return new Date(iso).toLocaleTimeString("ru-RU", {hour: "2-digit", minute: "2-digit"});
}

export function formatDurationValue(mins: number) {
    const h = Math.floor(mins / 60);
    const m = mins % 60;
    const parts: string[] = [];
    if (h > 0) parts.push(`${h} ${plural(h, "час", "часа", "часов")}`);
    if (m > 0) parts.push(`${m} ${plural(m, "минута", "минуты", "минут")}`);
    return parts.join(" ");
}

export function formatDuration(start: string, end: string) {
    return formatDurationValue(Math.round((+new Date(end) - +new Date(start)) / 60000));
}

function startOfDay(d: Date) {
    return new Date(d.getFullYear(), d.getMonth(), d.getDate()).getTime();
}

export function formatDayLabel(iso: string) {
    const date = new Date(iso);
    const diffDays = Math.round((startOfDay(date) - startOfDay(new Date())) / 86400000);
    if (diffDays === 0) return "Сегодня";
    if (diffDays === 1) return "Завтра";
    if (diffDays === 2) return "Послезавтра";
    return date.toLocaleDateString("ru-RU", {weekday: "short", day: "numeric", month: "long"});
}

export function toIsoDate(d: Date) {
    const m = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${d.getFullYear()}-${m}-${day}`;
}

export function timeToMinutes(time: string) {
    const [h, m] = time.split(":").map(Number);
    return h * 60 + m;
}

export function minutesToTime(mins: number) {
    return `${String(Math.floor(mins / 60)).padStart(2, "0")}:${String(mins % 60).padStart(2, "0")}`;
}
