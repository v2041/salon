import {existsSync, readFileSync} from "node:fs";
import path from "node:path";
import {Box, Paper, Stack, Typography} from "@mui/material";
import {accentInk} from "@/lib/theme";

export const metadata = {
    title: "Политика обработки персональных данных",
};

type PolicySection = {title: string; paragraphs: string[]};

// Текст политики: content/privacy.local.md (локальный, в .gitignore), иначе шаблон content/privacy.md.
function readPolicyText(): string {
    const contentDir = path.join(process.cwd(), "content");
    const local = path.join(contentDir, "privacy.local.md");
    const template = path.join(contentDir, "privacy.md");
    return readFileSync(existsSync(local) ? local : template, "utf8");
}

// "## Заголовок" — заголовок раздела. Строка "<!-- ... -->" — комментарий, не выводится.
// Каждая другая непустая строка — отдельный абзац.
function parsePolicy(text: string): PolicySection[] {
    const sections: PolicySection[] = [];
    let current: PolicySection | null = null;
    for (const rawLine of text.split(/\r?\n/)) {
        const line = rawLine.trim();
        if (line === "" || line.startsWith("<!--")) {
            continue;
        }
        const heading = /^#{1,4}\s+(.+)$/.exec(line);
        if (heading) {
            current = {title: heading[1], paragraphs: []};
            sections.push(current);
            continue;
        }
        if (current === null) {
            current = {title: "", paragraphs: []};
            sections.push(current);
        }
        current.paragraphs.push(line);
    }
    return sections;
}

export default function PrivacyPage() {
    const sections = parsePolicy(readPolicyText());
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
                    {sections.map((section, sectionIndex) => (
                        <Stack key={sectionIndex} spacing={0.75}>
                            {section.title !== "" && <Typography variant="subtitle1">{section.title}</Typography>}
                            {section.paragraphs.map((paragraph, paragraphIndex) => (
                                <Typography key={paragraphIndex} variant="body2" color="text.secondary">
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
