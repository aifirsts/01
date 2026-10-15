# 🔍 АУДИТ ОТПЕЧАТКА АГЕНТСКОЙ БРАУЗЕРНОЙ СЕССИИ — 15.10.2026

**Сессия:** Arena.ai Agent Mode · ветка `arena/c491e47f-01` · репозиторий `aifirsts/01`
**Объект тестирования:** Chromium 153.0.8010.0 (headless, сборка `@sparticuz/chromium`), управляемый через **CDP** (puppeteer-core 25) — эталонная модель того, как работают облачные ИИ-агенты (Browser Use, OpenAI Operator, Skyvern, Claude Computer Use и др.)
**Платформа:** контейнер 2 vCPU / 3.8 GiB RAM / Linux x86_64 / DejaVu-шрифты / программный GPU (SwiftShader) / egress-allowlist

---

## 1. TL;DR

| Вывод | Данные |
| :--- | :--- |
| Сессия **определяется как автоматизированная всеми** запущенными детекторами | CreepJS: **88 % like headless**, **100 % headless**, **0 % stealth** · FPScanner: **fastBotDetection = true** (7 high/medium-флагов) · Rebrowser: **2 красных** теста |
| JS-окружение **не подменено** (никакого антидетекта) | CreepJS `lies.totalLies = 0`, `hasToStringProxy = false`, `hasIframeProxy = false`, `hasBadChromeRuntime = false` |
| Сетевой уровень = **настоящий Chromium**, а не эмуляция | JA4 = `t13i1615h2_…`, HTTP/2 SETTINGS = `1:65536, 2:0, 4:6291456, 6:262144` — бит-в-бит эталон Chrome |
| Критические «сдающие» признаки | `navigator.webdriver = true` (в т.ч. **внутри iframe**), `HeadlessChrome` в `User-Agent` **и** в `sec-ch-ua` / `userAgentData.brands`, отсутствие `window.chrome`, 0 плагинов, `en-US@posix`, `UTC`, `pointer: none` / `hover: none`, `navigator.maxTouchPoints = 0` |
| Аппаратный профиль контейнера | 2 ядра, 4 ГБ, экран 1920×1080, **WebGL недоступен** (SwiftShader не поднимается в GPU-процессе), 1 шрифт из 28 проверенных, мультимедиа-устройств 0 |

---

## 2. Ограничение, которое нельзя обойти: сетевой allowlist песочницы

Из этой песочницы разрешены только `github.com`, `codeload.github.com`, `api.github.com`, `registry.npmjs.org`, `pypi.org`, `files.pythonhosted.org`. Проверка всех 17 сайтов из списка:

```
bot-detector.rebrowser.net      -> 000 (нет соединения)   creepjs              -> 000
bot.incolumitas.com             -> 000                     browserleaks.com     -> 000
browserscan.net                 -> 000                     pixelscan.net        -> 000
tls.peet.ws                     -> 000                     deviceandbrowserinfo -> 000
fingerprint-scan.com            -> 000                     bot.sannysoft.com    -> 000
iphey.com                       -> 000                     amiunique.org        -> 000
coveryourtracks.eff.org         -> 000                     overpoweredjs.com    -> 000
webbrowsertools.com             -> 000                     antoinevastel.com    -> 000
cside.com                       -> 000
```

**Поэтому тесты выполнены офлайн, но не «на глазок»:** развёрнуты реальные детекторы из открытых репозиториев (тот же код, что стоит на сайтах), а для закрытых онлайн-сервисов воспроизведена **измерительная логика** — в том числе перехват TLS ClientHello и HTTP/2-фреймов для JA3/JA4, которые обычно отдаёт `tls.peet.ws`.

---

## 3. ТАБЛИЦА A — статус по каждому сайту из списка и полученный результат

| # | Сайт | Что проверяет | Как выполнено | **Результат** |
| :-: | :--- | :--- | :--- | :--- |
| 1 | **bot-detector.rebrowser.net** | CDP-утечки Puppeteer/Playwright, webdriver, UA-CH | ✅ Запущен реальный код (репозиторий `rebrowser/rebrowser-bot-detector`) | **4 🟢 PASS · 2 ⚪ N/A · 2 🔴 FAIL** (`navigatorWebdriver`, `useragent`) |
| 2 | **abrahamjuliot.github.io/creepjs** | глубокий аудит отпечатка, «lies», V8, Worker, canvas/audio | ✅ Собран из исходников (`pnpm build`) и запущен | **88 % like headless · 100 % headless · 0 % stealth · Lies 0** |
| 3 | **bot.sannysoft.com** | классические флаги Selenium/headless (движок `antoinevastel/fpscanner`) | ✅ Собрана и запущена сама библиотека FPScanner (это её секция «Fingerprint Scanner» на sannysoft) | **fastBotDetection = true**; сработали 7 флагов, 14 — чисто |
| 4 | **tls.peet.ws** (JA3/JA4/HTTP-2) | TLS- и HTTP/2-отпечаток клиента | ⚠️ Прямо недоступен → **локальный перехват ClientHello** (свой TLS-сервер, разбор руками) | **JA4 `t13i1615h2_f0a184148aaa_*`**, TLS 1.3, 16 шифров, 17 расширений, GREASE; SETTINGS Chrome-эталон |
| 5 | **browserleaks.com** | canvas/webgl/audio/fonts/webrtc/CH | ⚠️ Прямо недоступен → те же измерения напрямую в DOM | Canvas-хэш `11b75067`, **WebGL: ERROR**, шрифтов 1/28, Audio-сумма `124.04347776696522`, WebRTC — только mDNS-кандидат |
| 6 | **pixelscan.net** | согласованность «железо ↔ сеть ↔ IP» | ⚠️ Закрытый онлайн-сервис → воспроизведены его проверки согласованности | Согласованность **внутренняя** (Linux/Linux/Linux) ✅, но **UTC+en-US+датацентр** — типичный маркер; IP-часть недоступна |
| 7 | **overpoweredjs.com** | 36 компонентов отпечатка + анти-бот API | ✅ Библиотека запущена локально (`dist/es5/opjs.min.js`); вердикт API требует внешней сети | 36 компонентов собрано; `userAgentData_brands = [Chromium, **HeadlessChrome**]` |
| 8 | **bot.incolumitas.com** | поведение, тайминги, CDP | ❌ Исходники закрыты, прямой доступ заблокирован | Поведенческий класс частично покрыт CreepJS (`resistance`, `engine: Blink`) |
| 9 | **browserscan.net/bot-detection** | WebDriver-утечки, headless, WebRTC | ❌ Онлайн-only, закрытый | Эквивалент покрыт FPScanner + Rebrowser |
| 10 | **deviceandbrowserinfo.com/are_you_a_bot** | сигналы окружения + поведение | ❌ Онлайн-only | Эквивалент — таблица C |
| 11 | **fingerprint-scan.com** | утечки + расширения браузера | ❌ Онлайн-only | Расширения: 0 (`fpscanner.extensions.bitmask = 00000000`) |
| 12 | **iphey.com** | Trust Score (браузер + IP/DNS) | ❌ Нужен внешний IP и DNS | Недоступно из-за allowlist |
| 13 | **amiunique.org** | энтропия и уникальность против внешней БД | ❌ Scala/Play-приложение + внешняя БД | Статистика недоступна |
| 14 | **coveryourtracks.eff.org** | биты энтропии, трекеры | ❌ Онлайн-only (EFF) | — |
| 15 | **fingerprint.com/products/bot-detection** | коммерческий FP Pro | ❌ Онлайн-only | — |
| 16 | **cside.com**, **usefoil.com** | коммерческая атрибуция ИИ-агентов | ❌ Онлайн-only | — |
| 17 | **webbrowsertools.com** (canvas/webgl fingerprint) | canvas/WebGL-хэши | ❌ Онлайн-only | Заменено прямыми измерениями (п. 5) |

**Итог по покрытию:** 5 стендов выполнено полноценно (1, 2, 3, 4, 5/7) — это все открытые движки детекции; 12 сайтов недоступны исключительно из-за сетевого allowlist песочницы (для них подготовлен `harness.html` — см. §7).

---

## 4. ТАБЛИЦА B — детальные результаты выполненных тестов

### B1. Rebrowser Bot Detector (9 тестов, движок анти-бот-проверок CDP)

| Тест | Оценка | Комментарий движка |
| :--- | :-: | :--- |
| `dummyFn` | 🟢 PASS | `window.dummyFn()` доступен из главного мира |
| `runtimeEnableLeak` | 🟢 PASS | Утечка `Runtime.Enable` **не** обнаружена (puppeteer 25 исправлен) |
| `viewport` | 🟢 PASS | 1920×1080 не совпадает с дефолтами библиотек автоматизации |
| `pwInitScripts` | 🟢 PASS | `window.__pwInitScripts` отсутствует |
| `bypassCsp` | 🟢 PASS | CSP включён и работает штатно |
| `sourceUrlLeak` | ⚪ N/A | требует дополнительного вызова |
| `exposeFunctionLeak` | ⚪ N/A | `page.exposeFunction` не использовался |
| **`navigatorWebdriver`** | 🔴 **FAIL** | `navigator.webdriver = true` → прям улика автоматизации |
| **`useragent`** | 🔴 **FAIL** | `Google Chrome` отсутствует в `navigator.userAgentData` (это Chrome for Testing/headless) |

### B2. CreepJS — сводные метрики

| Метрика | Значение |
| :--- | :--- |
| % like headless | **88** |
| % headless | **100** |
| % stealth | **0** |
| Lies (подмена JS-прототипов) | **0** (data: {}, `totalLies: 0`) |
| FP ID | `bb1cd2b447124c9903278b6226ba7c8d9e2fa08b9a5fab2b5ea72f9fad9f8704` |
| FuzzyFingerprint | `837ff238562c8e048d994cf23ffdbd38fff5b8ecffdc84f8ed29000000000000` |
| Resistance | `engine: Blink`, privacy/security/mode/extension: `unknown` |
| platformEstimate | Windows 1 · Mac 0.91 · Linux 0.91 · ChromeOS 0.73 |
| `lies.$hash` | `f4c4e0371fde124bbb71790a9d668d18af1a5236965caed7959ee1aaa6bda301` |

**Сработавшие «like headless»-флаги (13):** `noChrome`, `hasPermissionsBug`, `noPlugins`, `noMimeTypes`, `notificationIsDenied`, `hasKnownBgColor`, `prefersLightColor`, `pdfIsDisabled`, `noTaskbar`, `hasVvpScreenRes`, `noWebShare`, `noContentIndex`, `noContactsManager`, `noDownlinkMax`
**Сработавшие «headless»-флаги (3):** `webDriverIsOn`, `hasHeadlessUA`, **`hasHeadlessWorkerUA`** (headless-UA виден и в Web Worker-контексте)
**Stealth-флаги:** все `false` (нет iframe-прокси, нет подмены `toString`, нет «высокого индекса Chrome»)

### B3. FPScanner (движок тестов sannysoft) — вердикт

| Итог | Значение |
| :--- | :--- |
| `fastBotDetection` | **true** ⚠️ |
| FSID | `FS1_011010001100000100100_11010h02ba_1920x1080c02m04b00000h-3747f_f00001111000101111000111111111e00000000p1010h-18bc2_0h01b1dd_1h-53968_en1tUTC_h2cea_0110h-5a89e` |

**Обнаружено (detected = true):**

| Флаг | Severity |
| :--- | :-: |
| `hasWebdriver` | high |
| `hasWebdriverWritable` | high |
| `hasCDP` | high |
| `hasMissingChromeObject` | high |
| `hasWebdriverIframe` (**webdriver=true внутри iframe**) | high |
| `hasBotUserAgent` | high |
| `hasUTCTimezone` | medium |

**Чисто (detected = false):** `hasSeleniumProperty`, `hasPlaywright`, `hasImpossibleDeviceMemory`, `hasHighCPUCount`, `hasWebdriverWorker`, `hasMismatchWebGLInWorker`, `hasMismatchPlatformIframe`, `hasMismatchPlatformWorker`, `hasSwiftshaderRenderer`, `hasMismatchLanguages`, `hasInconsistentEtsl`, `hasGPUMismatch`, `hasPlatformMismatch`, `headlessChromeScreenResolution`

Полный сигнальный набор, отправленный детектору (расшифровано на локальном сервере): `automation.webdriver = true`, `automation.cdp = true`, `automation.playwright = false`, `device = {cpu 2, ram 4, 1920×1080, multimedia 0/0/0, pointer: none, hover: false, keyboardLayout: ""}`, `browser.features.chrome = false`, `browser.plugins.pluginCount = 0`, `contexts.iframe.webdriver = true`, `contexts.webWorker.{memory 4, cpu 2}`.

### B4. OverpoweredJS (36 компонентов)

| Компонент | Значение |
| :--- | :--- |
| `userAgentData_brands` | `["Chromium", "**HeadlessChrome**"]` — прямой маркер |
| `userAgentData_fullVersionList` | `{HeadlessChrome: 153.0.8010.0, Chromium: 153.0.8010.0}` |
| `screen` | 1920×1080, depth 24, avail 1920×1080, extended = false |
| `screen_orientation.type` | **`portrait-primary` при landscape-экране 16:9** — несогласованность |
| `devicePixelRatio` / `jsHeapSizeLimit` | 1 / 2 190 000 000 (≈2.04 ГБ) |
| `getBattery` | level 1, charging true, chargingTime 0, dischargingTime ∞ |
| `timingResolution_smallestInterval` | 0.09999999997671694 мс |
| `brave` | likeBrave/isBrave false, mode −1, clues `[false×4]` |

### B5. Сетевой уровень — реальный перехват (реплика `tls.peet.ws`)

**HTTP/1.1-подключение**

| Параметр | Значение |
| :--- | :--- |
| TLS | TLSv1.3, `TLS_AES_256_GCM_SHA384` |
| JA3 | `152389d9608ec982f20d8b860876160a` (строка: `771,aaaa-1301-1302-1303-c02b-c02f-c02c-c030-cca9-cca8-c013-c014-9c-9d-2f-35,eaea-2d-a-d-44cd-17-12-23-10-fe0d-5-2b-b-1b-ff01-33-8a8a,2a2a-11ec-1d-17-18,0`) |
| JA4 | `tt13i1615h2_f0a184148aaa_44ad223425a7` |
| Шифров / расширений | 16 / 17, GREASE присутствует (2 «greased» расширения) |
| Key share | `0x11ec` = **X25519MLKEM768** (постквантовый гибрид — признак Chrome 15x) |
| SNI | отсутствует (IP-адрес вместо домена) |
| Порядок заголовков | `Host · Connection · sec-ch-ua · sec-ch-ua-mobile · sec-ch-ua-platform · Upgrade-Insecure-Requests · UA · Accept · Sec-Fetch-Site · Sec-Fetch-Mode · Sec-Fetch-User · Sec-Fetch-Dest · Accept-Encoding` — **канонический порядок Chrome** |
| `sec-ch-ua` | `"HeadlessChrome";v="153", "Not_A Brand";v="8", "Chromium";v="153"` |
| `Sec-Fetch-*` | корректны (`none`/`navigate`/`?1`/`document`) — нарушения логики **нет** |

**HTTP/2-подключение (ALPN = h2)**

| Параметр | Значение |
| :--- | :--- |
| Preface | `PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n` — принят |
| SETTINGS | `1:65536 (HEADER_TABLE_SIZE)`, `2:0 (ENABLE_PUSH)`, `4:6291456 (INITIAL_WINDOW_SIZE)`, `6:262144 (MAX_HEADER_LIST_SIZE)` |
| Далее | `WINDOW_UPDATE(len 4)` → `HEADERS(len 440, flags 0x25 = END_STREAM+END_HEADERS+PRIORITY)` |
| Вывод | отпечаток **идентичен эталонному Chrome** — «дешёвых» сетевых несоответствий нет |
| JA4 (h2) | `tt13i1615h2_f0a184148aaa_4e0723982b4b` |
| ⚠️ Важно | JA3 **меняется от соединения к соединению** (GREASE + случайный порядок расширений Chrome) — стабильный идентификатор только структурная часть JA4 `t13i1615h2` |

### B6. Прямые измерения браузерной сессии (класс BrowserLeaks)

| Категория | Значение |
| :--- | :--- |
| User-Agent | `Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/153.0.8010.0 Safari/537.36` |
| `navigator.webdriver` | **true** |
| `navigator.platform` / vendor | `Linux x86_64` / `Google Inc.` |
| plugins / mimeTypes / pdfViewer | **0 / 0 / false** |
| `chrome.runtime` / `chrome.loadTimes` / `chrome.csi` | **отсутствуют все три** |
| Ядра / память / touch | 2 / 4 ГБ / `maxTouchPoints = 0` |
| Языки | **`["en-US@posix"]`** (нестандартный суффикс `@posix` — маркер контейнера) |
| Экран | 1920×1080, avail 1920×1080, depth 24, DPR 1, `orientation: portrait-primary` |
| Часовой пояс / локаль | **UTC** / `en-US` |
| Canvas 2D | хэш `11b75067` (FPScanner) / `4b598b73` (прямой djb2, len 13538) / CreepJS `data: c395c58d` |
| **WebGL** | **недоступен** — `images: blocked, pixels: blocked, params (0): blocked, gpu: blocked` (SwiftShader не инициализируется в GPU-процессе контейнера) |
| WebGPU | unsupported |
| Аудио (OfflineAudioContext) | доступно: sum `124.04347776696522`, gain `-20.538…`, unique-сэмплов 4735 |
| Шрифты | **1 из 51 загружен** (`DejaVu Sans`), `systemFonts: Arial`; прямой тест: 1 из 28 |
| CSS media | `pointer: none`, `any-pointer: none`, `hover: none`, `any-hover: none`, `prefers-color-scheme: light`, `device-aspect-ratio: 16/9`, `color-gamut: srgb` |
| Permissions API | geolocation/notifications/clipboard/camera/microphone = `prompt` (у настоящего Chrome — `default` для notifications ⇒ `hasPermissionsBug`) |
| WebRTC | только один host-кандидат с **mDNS-именем** `*.local`; STUN-соединение — **blocked** (утечки локального/публичного IP нет) |
| Мультимедиа-устройства | speakers 0, microphones 0, webcams 0 |
| Battery | `level = 1 (100 %), charging = true` — «питание от сети» (типично для сервера) |
| Storage / JS heap | quota ≈ 10 ГБ (`[10737418240]`), `jsHeapSizeLimit ≈ 2.04 ГБ` |
| Сеть (Network Information) | `rtt: 0`, `downlink: 10`, `effectiveType: 4g`, `saveData: false` |
| Версия фич Blink | `113-115+` (несоответствие фактической версии 153 — известная особенность сборок) |

---

## 5. ТАБЛИЦА C — сводный профиль: что о сессии знает любой сайт

| Слой | Сигнал | Значение в сессии | Как воспринимается антибот-системой |
| :--- | :--- | :--- | :--- |
| **Сеть** | TLS/JA4 | `t13i1615h2_…` — подлинный Chrome | ✅ «клиент честный» |
| | HTTP/2-фреймы | Chrome-эталон | ✅ |
| | Порядок заголовков | Канонический Chrome | ✅ несоответствий нет |
| | `sec-ch-ua` / `User-Agent` | **HeadlessChrome** | 🔴 **критично** |
| | `Sec-Fetch-*` | Корректные | ✅ (в отличие от многих агент-фреймворков) |
| **Браузер** | `navigator.webdriver` | **true** (в т.ч. в iframe) | 🔴 **критично** |
| | `window.chrome` | Отсутствует | 🔴 high |
| | Плагины / mimeTypes | 0 / 0 | 🔴 |
| | CDP-артефакты (`Runtime.Enable` и пр.) | Не обнаружены | ✅ |
| | Lies (подмена прототипов) | 0 | ✅ «чисто», но не спасает |
| **Железо** | Ядра / RAM | 2 / 4 ГБ | 🟡 слабый сигнал (нормально для VM) |
| | GPU / WebGL | **Недоступен** | 🔴 (`hasSwiftshaderRenderer = false`, `webGL = ERROR`) |
| | Шрифты | 1 (только DejaVu Sans) | 🔴 сильный контейнер-маркер |
| | Медиа-устройства, Battery | 0/0/0, charging 100 % | 🟡 |
| **Окружение** | TZ / локаль / языки | **UTC** / en-US / `en-US@posix` | 🔴 (`hasUTCTimezone` medium, суффикс `@posix`) |
| | Экран и указатель | 1920×1080, `pointer: none`, `hover: none` | 🔴 (нет реального устройства ввода) |
| **Контексты** | Web Worker UA | Тоже `HeadlessChrome` | 🔴 (`hasHeadlessWorkerUA`) |
| | iframe | `webdriver = true` | 🔴 (`hasWebdriverIframe`) |

---

## 6. Интерпретация

1. **Это «голый» headless-профиль без единой маскировки.** Классическая картина: агент управляет настоящим Chromium через CDP, ничего не подменяет — `lies = 0`, `stealth = 0`. Практически все современные фреймворки (Browser Use, puppeteer/playwright-based агенты) выглядят именно так, если не применены `rebrowser-patches` / `patchright` / camoufox.
2. **Самое дешёвое и самое «убийственное» — идентичность.** `HeadlessChrome` в UA + `sec-ch-ua` + `userAgentData.brands` + `navigator.webdriver = true` дают 100 % детект без всякого «машинного обучения». Именно по этой группе сработали 2 красных теста Rebrowser и 5 high-флагов FPScanner.
3. **Поведенческий слой (cloud-2026) мы в этой проверке не имитировали.** Согласно FP-Agent (arXiv 2605.01247), именно поведение — набор текста «вставкой», отсутствие траектории курсора, дискретные прыжки скролла — отличает агентов друг от друга и от людей; Cloudflare в их замере поймал лишь 1 агента из 7, тогда как поведенческий классификатор — всех 7.
4. **Сетевой уровень у агента в облаке обычно чист**, и здесь это подтверждено: JA4 и HTTP/2 — подлинный Chrome. Значит, фильтровать агента по TLS бессмысленно: реальная детекция идёт по JS-окружению и поведению.
5. **Контейнер добавляет вторичные улики:** 1 шрифт, `pointer: none`, UTC, `en-US@posix`, 0 медиаустройств, WebGL недоступен, battery «на зарядке». Каждая по отдельности — «нормальная VM», в сумме — устойчивый «cloud/headless»-профиль.

---

## 7. Как прогнать тесты на живых сайтах (для вашей реальной сессии)

Так как онлайн-домены из песочницы недоступны, подготовлен автономный тест-стенд: **`fp-audit/harness.html`** — откройте его в своём браузере (двойным щелчком или через локальный сервер), и он выполнит локальные проверки (Canvas, WebGL-рендерер, шрифты, Audio, WebRTC-утечка, UA-CH, `webdriver`, плагины, media queries, TZ), покажет результат таблицей, сохранит JSON и даст кнопки-ссылки на все живые онлайн-детекторы из списка.

Онологин-проверки, которые я не могу выполнить из контейнера:

| Сайт | Ссылка |
| :--- | :--- |
| Rebrowser Bot Detector | https://bot-detector.rebrowser.net/ |
| CreepJS | https://abrahamjuliot.github.io/creepjs/ |
| Sannysoft Antibot | https://bot.sannysoft.com/ |
| Incolumitas Bot Test | https://bot.incolumitas.com/ |
| BrowserScan Bot Detection | https://www.browserscan.net/bot-detection |
| Pixelscan (bot-check) | https://pixelscan.net/bot-check |
| BrowserLeaks | https://browserleaks.com/ |
| TLS/JA3/JA4 (Peet) | https://tls.peet.ws/api/all |
| Device & Browser Info | https://deviceandbrowserinfo.com/are_you_a_bot |
| Fingerprint-Scan | https://fingerprint-scan.com/ |
| IPhey | https://iphey.com/ |
| AmIUnique | https://amiunique.org/ |
| Cover Your Tracks (EFF) | https://coveryourtracks.eff.org/ |
| Fingerprint.com Bot Detection | https://fingerprint.com/products/bot-detection/ |

---

## 8. Приложение — артефакты и воспроизводимость

| Файл | Содержимое |
| :--- | :--- |
| `fp-audit/data/01-dom-probes.json` | прямые измерения DOM/API (Canvas, Audio, шрифты, WebRTC, permissions…) |
| `fp-audit/data/02-creepjs-fingerprint.json` | полный объект `window.Fingerprint` от CreepJS |
| `fp-audit/data/03-fpscanner-decrypted.json` | расшифрованная нагрузка FPScanner + `fastBotDetectionDetails` |
| `fp-audit/data/04-overpoweredjs.json` | 36 компонентов OverpoweredJS |
| `fp-audit/data/05-tls-http1-capture.json` | ClientHello → JA3/JA4 + заголовки HTTP/1.1 |
| `fp-audit/data/06-tls-http2-capture.json` | ALPN h2, preface, SETTINGS/WINDOW_UPDATE/HEADERS |
| `fp-audit/data/07-creepjs-page.txt`, `08-fpscanner-page.txt` | текстовые отчёты страниц |
| `fp-audit/report.html` | визуальный отчёт (автономный) |
| `fp-audit/harness.html` | стенд для запуска в вашем браузере |

**Ключевые команды (вне репозитория, в `/home/user/fp-lab`):**

```bash
# Chromium 153 + libs + SwiftShader из npm (@sparticuz/chromium), распаковка brotli
node -e 'const z=require("zlib"),f=require("fs");f.writeFileSync("runtime/chromium",z.brotliDecompressSync(f.readFileSync("package/bin/chromium.br")),{mode:0o755})'
LD_LIBRARY_PATH=runtime/libs/lib:runtime/swiftshader ./runtime/chromium --headless=new --no-sandbox --dump-dom about:blank

# Управление через CDP (puppeteer-core) + локальный статический стенд
npm i puppeteer-core ws && python3 -m http.server 8099 --bind 0.0.0.0   # site/{creepjs,rebrowser,opjs}

# CreepJS: сборка из исходников
pnpm install && pnpm run build          # → docs/creep.js, docs/style.min.css

# FPScanner: сборка + сервер-приёмник
PUPPETEER_SKIP_DOWNLOAD=1 npm install --ignore-scripts && npm run build:dev
FINGERPRINT_KEY=dev-key node examples/nodejs/demo-server.js   # → расшифрованный отпечаток

# Перехват TLS ClientHello + HTTP/2 (реплика tls.peet.ws)
python3 capture-server.py 8443 http/1.1 capture.json
python3 capture-server.py 8444 h2 capture-h2.json
```

---

**Вывод одной строкой:** в текущем виде агентская браузерная сессия в этой песочнице **детектируется на 100 %** — не по сети (TLS/HTTP-2 подлинные), а по идентичности (`HeadlessChrome`, `navigator.webdriver`, отсутствие `window.chrome`), контекстам (iframe/Worker), окружению (UTC, `en-US@posix`, 1 шрифт, `pointer: none`) и отсутствию GPU.
