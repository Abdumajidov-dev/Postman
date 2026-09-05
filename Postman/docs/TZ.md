# Texnik Topshiriq (TZ) — "Pochtachi" API Client

> Postman'ga muqobil, shaxsiy va kompaniya ichida ishlatiladigan API test/dizayn platformasi.
> Versiya: 0.1 (draft) · Sana: 2026-09-05

---

## 1. Loyiha maqsadi

Postman'ning asosiy funksiyalarini qamrab oluvchi, lekin o'z brendimiz va dizaynimizga ega desktop API client yaratish. **Faqat shaxsiy va kompaniya ichida foydalanish uchun — SaaS/tashqi sotish yo'q.**

**Nima uchun o'zimiznikini qilamiz, Postman/Swagger'ni ishlatavermaymiz:**
- Kompaniya ma'lumotlari (API kalitlar, ichki endpointlar) uchinchi tomon serverida saqlanmaydi — o'z backend'imiz, o'z bazamiz.
- Swagger kuchli tomoni — API hujjatlash (schema-first), lekin test/ishlatish tajribasi zaif. Postman kuchli tomoni — request builder/testing, lekin hujjatlash Swagger darajasida emas. **Ikkalasining kuchli tomonini birlashtiramiz**: Postman'dek to'liq request/test tajribasi + OpenAPI schema'dan avtomatik hujjat/collection generatsiya (bo'lim 3.10).
- Kerakli funksiyalarni o'zimiz belgilaymiz, keraksiz og'irlikni olib tashlaymiz.
- Kelajakda ichki tool'lar bilan integratsiya (CI/CD, ichki auth, monitoring) oson bo'ladi.

## 2. Foydalanuvchi rollari

| Rol | Huquqlar |
|---|---|
| **Owner** | Workspace yaratish/o'chirish, a'zolarni boshqarish |
| **Admin** | Workspace sozlamalari, a'zo qo'shish/olib tashlash, barcha collection'larni tahrirlash |
| **Member** | O'ziga biriktirilgan workspace'da collection/request yaratish, tahrirlash, ishga tushirish |
| **Viewer** | Faqat ko'rish va so'rovlarni ishga tushirish, tahrirlash huquqisiz |

## 3. Funktsional talablar

### 3.1 Request Builder (yadro)
- HTTP metodlar: GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS
- URL + query params jadval ko'rinishida (key-value, enable/disable checkbox)
- Headers jadvali (autocomplete bilan: `Content-Type`, `Authorization` va h.k.)
- Body turlari: `none`, `form-data`, `x-www-form-urlencoded`, `raw` (JSON/XML/Text/HTML), `binary`, `GraphQL`
- Response ko'rish: status code, vaqt (ms), hajm (KB), headers, cookies, pretty/raw/preview rejimlari (JSON syntax highlight, HTML preview)
- cURL import/export (bitta so'rovni cURL'dan yaratish yoki cURL'ga aylantirish)

### 3.2 Collections & Folders
- Ierarxik papka tuzilmasi (nested folders)
- Drag & drop bilan tartiblash
- Collection darajasidagi sozlamalar: default auth, default headers, base URL variable
- Postman Collection v2.1 formatida import/export (moslik uchun)
- OpenAPI/Swagger (JSON/YAML) import → avtomatik collection generatsiya

### 3.3 Environment & Variables
- Scope darajalari: **Global → Workspace → Collection → Environment → Local (so'rov ichida)**
- `{{variable}}` sintaksisi, real vaqtda preview (qiymat qanday resolve bo'lishini ko'rsatish)
- Sensitive (secret) o'zgaruvchilar — UI'da yashiriladi (`••••`), export paytida maskalanadi

#### 3.3.1 Ko'p o'lchamli o'zgaruvchilar (Switch Dimensions) — bizning farqimiz

Postman'da muammo: `baseUrl` kabi o'zgaruvchini local↔prod orasida almashtirish uchun **butun Environment'ni** almashtirish kerak, yoki har bir qiymatni qo'lda o'zgartirish kerak bo'ladi. Bizda buning o'rniga:

- Bitta o'zgaruvchi bir nechta **nomlangan qiymatga** ega bo'ladi (masalan `baseUrl`: `local` → `http://localhost:5000`, `global` → `https://api.company.com`)
- Yuqori panelda global **switch (dropdown)** turadi — uni bosib o'zgartirsa, shu o'zgaruvchiga bog'liq **barcha so'rovlarda, barcha collection'larda** bir zumda yangi qiymat ishlaydi. Har bir joyni qo'lda tuzatish shart emas.
- Switch **mustaqil o'lchamlar (dimensions)** sifatida ishlaydi — masalan bir vaqtda ikkita switch bo'lishi mumkin:
  - **Environment switch:** `Local` / `Staging` / `Production`
  - **Role switch:** `Admin` / `User` / `SuperAdmin` — masalan `authToken` o'zgaruvchisi shu switch orqali kerakli rol tokenini beradi
- Ikkala switch **bir-biridan mustaqil** — Environment'ni `Production`'ga, Role'ni `Admin`'ga qo'yish mumkin, va shu kombinatsiyaga mos qiymat resolve bo'ladi (agar o'zgaruvchida shu kombinatsiya uchun qiymat belgilangan bo'lsa; bo'lmasa eng yaqin/default qiymatga tushadi)
- Yangi dimension (masalan `Region: UZ/RU/EU`) xohlagan vaqt qo'shilishi mumkin — arxitektura kengaytiriladigan qilib quriladi
- Bitta joydan (Switch Manager paneli) barcha dimension va ularning qiymatlarini boshqarish mumkin

### 3.4 Autentifikatsiya turlari
- No Auth, Basic Auth, Bearer Token, API Key (header/query), Digest Auth
- OAuth 2.0 (Authorization Code, Client Credentials, Password Grant grant turlari)
- Auth meros qilib olish (inherit from parent — collection/folder darajasidan)

### 3.5 Scripting (Pre-request / Tests)
- JavaScript sandbox (izolyatsiyalangan VM, `pm.*` API Postman'ga o'xshash: `pm.environment.set()`, `pm.test()`, `pm.expect()`)
- Pre-request script: so'rov jo'natishdan oldin bajariladi (token yangilash, signature hisoblash)
- Test script: javobni tekshirish (status code, JSON schema, response time)
- Chaining: bitta so'rov javobidan keyingisiga qiymat uzatish

### 3.6 Collection Runner
- Butun collection yoki papkani ketma-ket ishga tushirish
- Iteratsiyalar (CSV/JSON data file bilan data-driven testing)
- Natijalar hisoboti: qaysi test o'tdi/o'tmadi, vaqt statistikasi
- CLI versiyasi (Newman'ga o'xshash) — CI/CD pipeline'ga integratsiya uchun

### 3.7 Jamoa bilan ishlash (Team Workspace)
- Workspace yaratish, a'zo taklif qilish (email orqali)
- Real-time sinxronizatsiya (SignalR): kimdir collection'ni o'zgartirsa, boshqalarga darhol ko'rinadi
- Rol asosida ruxsatlar (3.2-bo'lim)
- O'zgarishlar tarixi (kim, qachon, nimani o'zgartirdi)
- Kommentariya qoldirish (request/collection darajasida)

### 3.8 History
- Har bir yuborilgan so'rov avtomatik saqlanadi (local + cloud sync)
- Qidirish va filtrlash (sana, metod, status code bo'yicha)
- Tarixdan so'rovni tiklash yoki collection'ga saqlash

### 3.9 Mock Server (Phase 3)
- Collection asosida avtomatik mock endpoint generatsiya
- Static yoki dynamic (script asosida) javoblar
- Frontend jamoasi backend tayyor bo'lmasdan turib ishlashi uchun

### 3.10 Import/Export & Moslik
- Postman Collection format (v2.1) — to'liq import/export moslik
- OpenAPI 3.0 / Swagger 2.0 import
- Insomnia format import (nice-to-have)

## 4. Nofunktsional talablar

- **Performance:** so'rov yuborish overhead'i <100ms (network vaqtidan tashqari), UI 60fps
- **Xavfsizlik:**
  - Tokenlar/parollar OS keychain'da shifrlangan holda saqlanadi (Windows Credential Manager / macOS Keychain / libsecret)
  - Barcha backend so'rovlar TLS orqali
  - SQL injection, XSS himoyasi (parametrized query, input sanitization)
  - Secret qiymatlar log'larda va UI'da maskalanadi
- **Offline-first:** shaxsiy foydalanishda internet bo'lmasa ham local SQLite orqali ishlaydi, internet qaytganda sync bo'ladi
- **Cross-platform:** Windows, macOS, Linux (bitta kodbaza)
- **Auto-update:** yangi versiya chiqqanda ilova o'zi yangilanadi
- **Kengaytiriluvchanlik:** kelajakda plugin/extension tizimi qo'shish mumkin bo'lishi kerak (arxitektura shunga tayyor bo'lsin)

## 5. Texnologik stack

### 5.1 Desktop Client
| Qatlam | Texnologiya |
|---|---|
| Shell | **Electron** (Node.js asosida — CORS cheklovisiz to'g'ridan-to'g'ri HTTP so'rov yuborish uchun) |
| UI Framework | **Vue 3** + Composition API + `<script setup>` |
| State | **Pinia** |
| Til | **TypeScript** |
| Styling | Tailwind CSS + o'z design-token tizimi (bo'lim 7) |
| Local DB | SQLite (`better-sqlite3`) — offline cache, history |
| Request executor | Electron main process (Node `undici`/`axios`) — barcha metod, binary, stream qo'llab-quvvatlanadi |
| Script sandbox | `isolated-vm` yoki `vm2` o'rniga xavfsizroq alternativ (pre-request/test scriptlar uchun) |

### 5.2 Backend (Cloud sync, Team workspace)
Clean Architecture (4 qatlam):
```
src/
  Postman.Domain/          # Entities, Interfaces, Enums
  Postman.Application/     # DTO, Services, CQRS Handlers (MediatR)
  Postman.Infrastructure/  # EF Core, PostgreSQL, External services
  Postman.API/             # Controllers, SignalR Hubs, Middleware
tests/
  Postman.UnitTests/
  Postman.IntegrationTests/
```
| Qism | Texnologiya |
|---|---|
| Framework | ASP.NET Core Web API (.NET 8+) |
| ORM | Entity Framework Core (Code-First, migrations) |
| DB | PostgreSQL |
| Real-time | SignalR (workspace sync, live collaboration) |
| Auth | JWT + Refresh Token, ASP.NET Identity |
| Pattern | Repository + Unit of Work, CQRS (MediatR) |
| Validation | FluentValidation |
| Logging | Serilog |
| API docs | Swagger/OpenAPI |
| Konteynerlash | Docker + docker-compose (API + PostgreSQL + Redis) |
| Cache/Pub-Sub | Redis (SignalR backplane, session cache) |

### 5.3 CI/CD (keyingi bosqich)
- GitHub Actions: build, test, lint har bir PR'da
- Electron builder → Windows(.exe)/macOS(.dmg)/Linux(.AppImage) release
- Backend → Docker image → kompaniya serveriga deploy

## 6. Ma'lumotlar modeli (asosiy entitylar)

```
User (Id, Email, PasswordHash, FullName, AvatarUrl)
Workspace (Id, Name, OwnerId, CreatedAt)
WorkspaceMember (WorkspaceId, UserId, Role)
Collection (Id, WorkspaceId, ParentFolderId?, Name, Description, AuthConfigId?)
Folder (Id, CollectionId, ParentFolderId?, Name, Order)
Request (Id, FolderId?, CollectionId, Name, Method, Url, Headers[], QueryParams[], Body, AuthConfigId?, PreRequestScript, TestScript, Order)
Environment (Id, WorkspaceId, Name, IsActive)
SwitchDimension (Id, WorkspaceId, Name)                    # masalan "Environment", "Role"
SwitchOption (Id, SwitchDimensionId, Name, Order)          # masalan "Local", "Global" / "Admin", "User"
Variable (Id, Scope[Global|Workspace|Collection|Environment], OwnerId, Key, IsSecret)
VariableValue (Id, VariableId, SwitchOptionId?, Value)     # SwitchOptionId=null -> default qiymat
AuthConfig (Id, Type[NoAuth|Basic|Bearer|ApiKey|OAuth2|Digest], ConfigJson)
RequestHistory (Id, UserId, RequestSnapshotJson, ResponseSnapshotJson, ExecutedAt)
Comment (Id, EntityType, EntityId, UserId, Text, CreatedAt)
```

## 7. Dizayn tizimi ("10 yillik dizayner" darajasi)

### 7.1 Vizual yo'nalish
- **Dark-mode birinchi** (developer tool'lar uchun standart), Light theme ham to'liq qo'llab-quvvatlanadi
- Brend rangi: Postman'ning to'q sarig'idan farqli — **electric indigo/violet** (`#6C5CE7` asosiy, gradient aksentlar bilan) + neytral kulrang shkala
- HTTP metod ranglari: GET — yashil, POST — sariq/amber, PUT — ko'k, PATCH — binafsha, DELETE — qizil (barchasi kontrast talablariga mos, WCAG AA)
- Tipografiya: UI uchun **Inter**, kod/JSON/response uchun **JetBrains Mono**

### 7.2 Layout
- Klassik 3-panelli tuzilma: chapda Sidebar (workspace/collections tree), markazda Request builder (tab'lar), pastda/o'ngda Response panel
- Panellar resize qilinadigan (drag), foydalanuvchi joylashuvni saqlab qoladi
- **Command Palette** (`Cmd/Ctrl+K`) — tez navigatsiya, so'rov yaratish, environment almashtirish
- Klaviatura-first: barcha asosiy amallar shortcut bilan (Send — `Cmd+Enter`, Save — `Cmd+S`)

### 7.3 Komponent tizimi
- O'z design-token kutubxonasi (spacing scale: 4/8/12/16/24/32px, radius scale, elevation/shadow scale)
- Qayta ishlatiluvchi komponentlar: Button, Input, Tabs, Dropdown, Table (key-value editor), Modal, Toast, Skeleton loader, Empty state
- Har bir interaktiv holat: default/hover/focus/active/disabled/loading/error — barchasi dizaynda hisobga olinadi
- Mikro-animatsiyalar: tab almashish, panel resize, response kelishi (subtle, 150-200ms transition) — ortiqcha emas, funksional

### 7.4 Empty & error states
- Bo'sh collection, natija topilmadi, tarmoq xatosi kabi holatlar uchun maxsus illyustratsiya + aniq call-to-action
- Xato xabarlar texnik emas, tushunarli tilda ("So'rov vaqti tugadi — server 30 soniya ichida javob bermadi")

## 8. Loyiha bosqichlari (Roadmap)

| Bosqich | Muddat (taxminiy) | Qamrov |
|---|---|---|
| **0. Fundament** | 1 hafta | TZ tasdiqlash, design system (Figma), arxitektura skeleton, CI setup |
| **1. MVP** | 4-6 hafta | Request builder, Collections/Folders, Environment/Variables, Auth turlari, Response viewer, Local history, Postman import/export |
| **2. Jamoa** | 3-4 hafta | Backend (ASP.NET Core + PostgreSQL), Auth (JWT), Workspace/Team, SignalR real-time sync, Pre-request/Test scripts, Collection Runner |
| **3. Kengaytirish** | 3-4 hafta | Mock server, OpenAPI import, CLI runner, Comments |
| **4. Polish & Release** | 2 hafta | Auto-update, cross-platform build, performance optimizatsiya, beta test |

## 9. Ochiq savollar (keyingi muhokama uchun)

1. Backend qayerda hosting qilinadi — kompaniya o'z serverimi, yoki cloud (Azure/AWS)?
2. SSO (Google/Microsoft) kerakmi, yoki email+parol yetarlimi?
3. Boshlang'ich jamoa hajmi qancha — bir nechta odammi yoki butun kompaniyami (bu SignalR/infra scale'ga ta'sir qiladi)?

> Tasdiqlangan: loyiha faqat ichki foydalanish uchun, SaaS/tashqi sotish yo'q.

---

*Ushbu TZ — draft. Tasdiqlangandan so'ng arxitektura skeleton va design system bilan ishni boshlaymiz.*
