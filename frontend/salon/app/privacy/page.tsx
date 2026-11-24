import {Box, Paper, Stack, Typography} from "@mui/material";
import {accentInk} from "@/lib/theme";

export const metadata = {
    title: "Политика обработки персональных данных",
};

const SECTIONS: { title: string; paragraphs: string[] }[] = [
    {
        title: "1. Оператор",
        paragraphs: ["Индивидуальный предприниматель [ФИО], ИНН [ИНН], e-mail: [email]."],
    },
    {
        title: "2. Какие данные обрабатываются",
        paragraphs: [
            "Фамилия и имя, номер телефона, идентификатор пользователя VK ID.",
            "Дата и время входа, версия и дата согласия.",
            "Сведения о записях: услуга, дата, время, цена.",
        ],
    },
    {
        title: "3. Цели обработки",
        paragraphs: [
            "Оформление и учёт онлайн-записей.",
            "Связь с клиентом по вопросам записи.",
            "Учёт оказанных услуг.",
        ],
    },
    {
        title: "4. Правовые основания",
        paragraphs: [
            "Согласие на обработку персональных данных (пункт 1 части 1 статьи 6 Федерального закона № 152-ФЗ).",
            "Исполнение договора об оказании услуг.",
        ],
    },
    {
        title: "5. Как обрабатываются данные",
        paragraphs: [
            "Данные собираются, записываются, хранятся и используются только для целей выше.",
            "Персональные данные не передаются третьим лицам, кроме случаев, установленных законом.",
        ],
    },
    {
        title: "6. Вход через VK ID",
        paragraphs: [
            "Вход выполняется через сервис VK ID. Оператор получает имя и номер телефона из профиля VK.",
            "Пароль от VK и токены доступа не хранятся.",
        ],
    },
    {
        title: "7. Cookie",
        paragraphs: [
            "Сайт использует технические cookie для входа в личный кабинет.",
            "Cookie не используются для рекламы и не передаются третьим лицам.",
        ],
    },
    {
        title: "8. Отзыв согласия",
        paragraphs: [
            "Согласие можно отозвать в любое время письмом на адрес [email].",
            "После отзыва согласия вход в личный кабинет закрывается. Оператор прекращает обработку данных и удаляет их, если нет других оснований по закону.",
        ],
    },
    {
        title: "9. Контакты",
        paragraphs: ["Вопросы по обработке персональных данных: [email]."],
    },
];

export default function PrivacyPage() {
    return (
        <Box sx={{maxWidth: 560, mx: "auto", px: 2, pt: 3, pb: 6}}>
            <Paper sx={{p: 3.5, borderRadius: "24px", border: 1, borderColor: "divider", boxShadow: "0 30px 70px -40px rgba(26,26,26,0.55)"}}>
                <Stack spacing={2.5}>
                    <Stack spacing={0.5}>
                        <Typography variant="overline" sx={{color: accentInk, letterSpacing: "0.16em"}}>
                            Документы
                        </Typography>
                        <Typography variant="h5">Политика обработки персональных данных</Typography>
                    </Stack>
                    {SECTIONS.map(section => (
                        <Stack key={section.title} spacing={0.75}>
                            <Typography variant="subtitle1">{section.title}</Typography>
                            {section.paragraphs.map(paragraph => (
                                <Typography key={paragraph} variant="body2" color="text.secondary">
                                    {paragraph}
                                </Typography>
                            ))}
                        </Stack>
                    ))}
                </Stack>
            </Paper>
        </Box>
    );
}
