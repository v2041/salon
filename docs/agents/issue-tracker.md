# Issue tracker: GitHub

Issues и спеки этого репозитория живут в GitHub Issues. Все операции — через CLI `gh`.

Репозиторий: `v2041/salon`. `gh` определяет его сам внутри клона.

## Соглашения

- **Создать issue**: `gh issue create --title "..." --body "..."`. Для многострочного тела используйте heredoc.
- **Прочитать issue**: `gh issue view <number> --json number,title,body,labels,comments`.
- **Список issues**: `gh issue list --state open --json number,title,body,labels,comments --jq '[.[] | {number, title, body, labels: [.labels[].name], comments: [.comments[].body]}]'`. Добавьте фильтры `--label` и `--state`.
- **Сделать issue подзадачей**: `gh issue create --parent <parent> ...` или `gh issue edit <parent> --add-sub-issue <child>` (`gh` 2.94+). Для старого `gh`: `gh api --method POST repos/<owner>/<repo>/issues/<parent>/sub_issues -F sub_issue_id=<child-db-id>`. Без подзадач добавьте `Part of #<parent>` в начало тела.
- **Комментарий**: `gh issue comment <number> --body "..."`
- **Метки**: `gh issue edit <number> --add-label "..."` / `--remove-label "..."`
- **Закрыть**: `gh issue close <number> --comment "..."`

## PR как поверхность triage

**PR как поверхность запросов: нет.** _(Поставьте `yes`, если репозиторий считает внешние PR запросами функций; `/triage` читает этот флаг.)_

Если флаг `yes`, PR проходят те же метки и состояния. Используйте `gh pr`:

- **Прочитать PR**: `gh pr view <number> --comments` и `gh pr diff <number>`.
- **Список внешних PR**: `gh api --paginate 'repos/{owner}/{repo}/pulls?state=open' --jq '.[] | select(.author_association | IN("OWNER","MEMBER","COLLABORATOR") | not) | {number, title, author: .user.login, author_association, labels: [.labels[].name]}'`.
- **Комментарий / метка / закрытие**: `gh pr comment`, `gh pr edit --add-label`/`--remove-label`, `gh pr close`.

GitHub использует общую нумерацию для issues и PR. Число `#42` может быть и тем, и другим. Проверьте `gh pr view 42`, затем `gh issue view 42`.

## Когда скилл говорит «опубликовать в трекер»

Создайте GitHub issue.

## Когда скилл говорит «получить тикет»

Прочитайте его по инструкции выше.

## Операции wayfinder

Использует `/wayfinder`. **Карта** — один issue с **дочерними** issue-тикетами.

- **Карта**: один issue с меткой `wayfinder:map`. Тело хранит Notes / Decisions-so-far / Fog. Создание: `gh issue create --label wayfinder:map`.
- **Дочерний тикет**: issue, привязанный к карте как GitHub sub-issue (см. выше). Если sub-issues недоступны, добавьте дочерний тикет в task list тела карты и поставьте `Part of #<map>` в начало тела. Метки: `wayfinder:<type>` (`research`/`prototype`/`grilling`/`task`). После взятия тикет назначен исполнителю.
- **Блокировка**: нативные зависимости GitHub. Команда: `gh api --method POST repos/<owner>/<repo>/issues/<child>/dependencies/blocked_by -F issue_id=<blocker-db-id>`. `<blocker-db-id>` — числовой database id блокера (`gh api repos/<owner>/<repo>/issues/<n> --jq .id`), не `#number` и не `node_id`. GitHub отдаёт `issue_dependencies_summary.blocked_by`. Если зависимости недоступны, поставьте строку `Blocked by: #<n>, #<n>` в начало тела. Тикет разблокирован, когда все блокеры закрыты.
- **Запрос frontier**: список открытых дочерних issue карты (`gh issue list --state open`). Уберите тикеты с открытым блокером (`issue_dependencies_summary.blocked_by > 0` или открытый issue в строке `Blocked by`) и с assignee. Побеждает первый по порядку карты.
- **Взять тикет**: `gh issue edit <n> --add-assignee @me`. Это первая запись сессии.
- **Закрыть**: `gh issue comment <n> --body "<answer>"`, затем `gh issue close <n>`, затем добавьте указатель на контекст (gist + ссылка) в Decisions-so-far карты.
