# SIC — Agent Instructions

## Project

Blazor WebAssembly SPA + ASP.NET Core 8 Web API for event invitation management with WhatsApp Cloud API integration.

## Solution layout (all under `SIC/`)

| Project | Type | Path |
|---|---|---|
| `SIC.Shared` | Class library | `SIC.Shared/` |
| `SIC.Backend` | Web API | `SIC.Backend/` |
| `SIC.Frontend` | Blazor WebAssembly | `SIC.Frontend/` |

## Key commands (run from `SIC/`)

```powershell
dotnet build SIC.sln
dotnet run --project SIC.Backend/SIC.Backend.csproj     # http://localhost:5235 / https://localhost:7141
dotnet run --project SIC.Frontend/SIC.Frontend.csproj   # http://localhost:5124 / https://localhost:7174
dotnet ef migrations add <Name> --project SIC.Backend/SIC.Backend.csproj
dotnet ef database update --project SIC.Backend/SIC.Backend.csproj
docker compose up -d
```

No test project exists. No CI workflows in `.github/workflows/`.

## Architecture

- **Backend**: EF Core 9 + SQL Server, JWT auth, SignalR hub at `/hubs/whatsapp-chat`, Swagger at `/swagger`, auto-migration + seed on startup.
- **Frontend**: Hardcoded backend URL `https://localhost:7141/` in `SIC.Frontend/Program.cs:19`. Docker-compose overrides to `http://backend:5000/` (commented out).
- **Both** reference `SIC.Shared` (entities, DTOs, enums).
- Repository + UnitOfWork pattern with a `GenericController<T>` base class for CRUD.
- Real-time chat via SignalR + WhatsApp Cloud API. FTP storage for images. Stripe payments.

## Infrastructure (docker-compose)

`docker compose up -d` starts: SQL Server, FTP server (vsftpd), Nginx file server (port 9003), backend (port 5000), frontend (port 8080). Nginx config at `config/nginx.conf`.

## Dev notes

- `appsettings.json` contains real secrets (JWT key, WhatsApp tokens, Stripe, DB passwords) — do not commit changes.
- Frontend uses `SweetAlert2` and `Blazor WebAssembly Authentication`.
- Entity `UserCredit` has 1:1 with `User`. Many unique indexes defined in `DataContext.OnModelCreating`.
- Migration snapshots in `SIC.Backend/Migrations/` (~75 pairs) — run `dotnet ef database update` before starting the app if DB is stale.
- No tests, no linter, no typecheck — build success is the only verification.

## Recent changes

### Session 1 — Event requirement system (seed + admin pages + client form)
- **SeedDb.cs**: `CheckEventTypesAsync()` changed from "add all if empty" to "add missing only" (fixes runtime crash `Sequence contains no matching element` in `GetTypeId` when old types pre-exist)
- **SeedDb.cs**: `CheckEventRequirementsAsync()` — seeds 28 requirements (Name, Section, InputType, Placeholder, IsRequired, Min/MaxImages, SortOrder) mapped per event type via `EventTypeRequirement` links (17 event types)
- **New pages** (all under `SIC.Frontend/Pages/EventRequirements/`):
  - `EventRequirementsIndex.razor` — list event types with requirement count
  - `EventTypeRequirementsDetail.razor` — split view: assigned reqs (reorder/remove) + available reqs (search/add)
  - `EventRequirementsForm.razor` — dynamic form at `/event/{EventCode}/requisitos`, grouped by section, supports all `RequirementInputType` values (text, multiline, number, date, time, url, image upload via FTP)
  - `EventRequirementsAvailable.razor` — CRUD maintenance for requirement catalog (create/edit modal, soft delete)
- **DTO**: `EventTypeRequirementDTO` extended with `RequirementPlaceholder`, `RequirementMinImages`, `RequirementMaxImages`; repository mapping updated
- **Build fixes**: `using SIC.Shared.Response` added to 16 backend files; renamed `section` loop variable (Razor keyword conflict); fixed `List.Sort()` lambda; added type argument to `DeleteAsync<T>`
- **Navigation**: "Requisitos" and "Catálogo de Requisitos" links in `NavMenu.razor`; "Requisitos" buttons in `EventsIndex.razor`; "Catálogo" button in `EventTypeRequirementsDetail.razor`

### Session 2 — Form validation
- `EventRequirementsForm.razor.cs` — `SaveAll()` validates required fields before submit (text inputs must not be blank, image inputs must have at least 1 image); shows SweetAlert warning if validation fails
- Added `FailedFields` HashSet + `SubmitAttempted` flag + `GetInputClass()` helper → inputs show `is-invalid` CSS and "Este campo es obligatorio." message
- `SetAnswer()`, `HandleImageUpload()`, `RemoveImage()` clear individual field error on user interaction

### Session 3 — Inbox event ownership fix
- `MessageRepository.GetInboxAsync(string phoneNumber)` — groups last OUT message per contact, then per `EventCode` (one entry per event); filters results to only events owned by the user (looks up `UsuarioWhatsAppConfig` by `phoneNumber` → `UsuarioId` → `Events.UserId`)
- No interface or controller changes — all logic encapsulated in the repository
