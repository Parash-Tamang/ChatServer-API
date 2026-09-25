# AIChatbot — Software Project Report

## 1. Cover Page
- **Project Name:** AIChatbot

## 2. Executive Summary
The repository implements an enterprise AI-assisted chat and data-access platform that combines:
- **User authentication** using ASP.NET Core Identity and **JWT bearer** tokens.
- **Chat session management** (sessions and messages persist in a SQL Server database).
- **Role-based data access (RBAC)** driven by configurable database connections, runtime permissions, and user-context lookups.
- **AI integration**: user prompts and session context are sent to an external AI service via HTTP (knowledgebase creation/update and chat execution).
- **Administrative web UI** (MVC) that orchestrates setup of connections, prompts, roles, and runtime permissions by calling the backend API.

Intended users include:
- End users who log in and chat.
- Super administrators managing roles, database connections, prompts, knowledgebases, and runtime permissions.

## 3. Project Overview
### Overall objective
Provide an authenticated AI chat experience that is constrained by role-configured access to underlying database data, including runtime-permission resolution and filter requirements.

### Core functionality (evidence-based)
- Auth flows: register, OTP verification, login, refresh, logout, forgot/reset password, change password (see `AIChatbot.Api/Controllers/AuthController.cs`).
- Chat flows: list sessions, list messages, send chat message, retry assistant response, delete chat session, generate Excel for messages (see `AIChatbot.Api/Controllers/ChatController.cs`).
- Agent/RBAC runtime resolution:
  - Retrieve database functions/prompts for a connection (`AgentController.Get-DB-Functions`).
  - Resolve runtime RBAC permissions and required filters for a user (`AgentController.Get-Runtime`).
  - Resolve permissions for a subset of tables (`AgentController.Get-Table-Runtime`).
  (see `AIChatbot.Api/Controllers/AgentController.cs`).
- Knowledgebase creation/update/delete for configured database connections (`AiProviderService.PrepareDatabaseAsync`, `UpdateDatabaseAsync`, `NotifyConnectionDeletedAsync`).

### Scope
Backend APIs (`AIChatbot.Api`), application/domain/infrastructure layers, and MVC web frontend (`AIChatbot.web`).

## 4. Technology Stack
| Layer | Technology | Purpose |
| ----- | ---------- | ------- |
| Frontend | ASP.NET Core MVC (`AddControllersWithViews`) | Administrative UI + chat UI pages |
| Backend | ASP.NET Core Web API | REST endpoints (controllers) |
| Backend | MediatR + CQRS-style commands/queries | Command/query execution pipeline (e.g., `SendChatMessageCommand`) |
| Backend | FluentValidation | Request validation pipeline (`ValidationBehavior<,>`) |
| Database | EF Core + SQL Server | Persistence (`AddDbContext(...UseSqlServer...)`) |
| Authentication | ASP.NET Core Identity | Users/roles, token providers |
| Authorization | JWT Bearer + policies | `AuthenticatedUser`, `SuperAdminOnly` |
| External AI Integration | HttpClient -> external AI service | Knowledgebase and chat execution |
| Frontend HTTP | Typed API client abstraction (e.g., `ApiClient`) | Calls backend endpoints |
| Misc | Rate limiting middleware | Policy-based request throttling |

## 5. Architecture Overview
### Architecture style
Layered architecture:
- **API layer**: Controllers in `AIChatbot.Api`.
- **Application layer**: Commands/Queries/Validators in `AIChatbot.Application` (MediatR).
- **Domain layer**: Entities in `AIChatbot.Domain`.
- **Infrastructure layer**: EF Core DbContext and repositories, AI provider implementation.
- **Web layer**: MVC frontend in `AIChatbot.web`.

### Design patterns (evidence-based)
- **CQRS + Mediator**: Controllers call `_mediator.Send(...)` for commands/queries (e.g., `ChatController`).
- **Repository pattern**: Controllers depend on abstractions like `IConnectionRepository`, `IPromptFunctionRepository` (injected in `ChatController` and others).
- **Middleware pipeline**: Custom middleware registered in `AIChatbot.Api/Program.cs`.

### Textual architecture diagram
```
[Browser/MVC UI]
   -> (HTTP)
[AIChatbot.web Controllers/Services]
   -> (JWT/Backend API)
[AIChatbot.Api Controllers]
   -> (MediatR)
[AIChatbot.Application Command/Query Handlers]
   -> (repositories)
[AIChatbot.Infrastructure Repositories + AppDbContext]
   -> (persistence)
[SQL Server]

AI execution:
[SendChatMessage*]
   -> [AiProviderService (HttpClient)]
   -> (HTTP)
[External AI service]
   -> structured JSON response
```

## 6. Folder Structure (major modules)
- `AIChatbot.Api/`:
  - `Controllers/`: REST API endpoints.
  - `Middleware/`: security headers, JSON escape fix, exception mapping.
- `AIChatbot.Application/`:
  - `Auth/`: commands/handlers/queries/validators.
  - `Chat/`: commands/handlers/queries/validators.
  - `RoleManagement/`, `Supersetup/`, `RoleConnectionAccessRetrive/`: RBAC and setup commands.
  - `Abstractions/`: interfaces for services and repositories.
- `AIChatbot.Domain/`:
  - `Entities/`: persisted domain models (ChatSession, Message, ConnectionString, RBAC entities, etc.).
- `AIChatbot.Infrastructure/`:
  - `Data/`: `AppDbContext`, `AppDbContextFactory`.
  - `Repositories/`: repository implementations.
  - `AI/`: `AiProviderService` and response provider.
  - `Identity/`: identity-related services (auth, email, captcha, seeding, runtime context resolution).
- `AIChatbot.web/`:
  - `Controllers/`: MVC endpoints for login/chat/admin pages.
  - `Services/`, `Interfaces/`: typed API services (e.g., `AuthService`).

## 7. Module Analysis (evidence-based)
### 7.1 API Layer (`AIChatbot.Api`)
- **Agent module** (`AgentController`): resolves functions and RBAC runtime permissions.
  - Key dependencies: `IConnectionRepository`, `IPromptFunctionRepository`, `ILocalFunctionRepository`, `UserManager<ApplicationUser>`, `RoleManager<IdentityRole>`, `IRoleConnectionMappingRepository`, `IRoleConnectionUserLookupConfigurationRepository`, `IRoleRuntimePermissionRepository`, `IRuntimePermissionViewRepository`, `IRuntimeContextResolver`.
- **Chat module** (`ChatController`): manages chat sessions and message submission.
  - Key dependencies: `IMediator`, `IConnectionRepository`, `IPromptFunctionRepository`, `ILocalFunctionRepository`.
- **Auth module** (`AuthController`): registration, login, refresh, password reset, profile.
  - Key dependencies: `IMediator`.
- **Role/Setup modules**:
  - `RolemanagerController`: role CRUD and user listing/deletion (superadmin policy).
  - `SupersetupController`: connection testing, knowledgebase create/update/activate, prompt function storage.
  - `AccessController`: superadmin-only schema retrieval, assignments, exclusions, runtime permissions.

### 7.2 Application Layer (`AIChatbot.Application`)
- MediatR commands/queries are the primary execution model.
- Example:
  - `SendChatMessageCommand` is defined as `record SendChatMessageCommand(string UserId, Guid? ChatSessionId, string Message) : IRequest<ChatExecutionResult>`.

### 7.3 Domain Layer (`AIChatbot.Domain`)
Representative persisted entities:
- `ChatSession`: `Id`, `UserId`, `CreatedAt`, `Messages`.
- `Message`: `Id`, `ChatSessionId`, `Role`, `Content`, `CreatedAt`.
- `ConnectionString`: connection metadata including encrypted password `PasswordEncrypted` and flags like `Verified`, `IsDeleted`.
- `RoleConnectionMapping`: maps a role to a connection (`RoleId` (string), `ConnectionId` (Guid)).
- `RoleRuntimePermission`: describes table/column permissions, filter type (`FilterType`), optional `RuntimeKey` and `FilterValuesJson`, and `AccessLevel`/`Reason`.

### 7.4 Infrastructure Layer (`AIChatbot.Infrastructure`)
- **EF Core**: `AppDbContext : IdentityDbContext<ApplicationUser>`.
- **AI provider**: `AiProviderService` implements `IAiProviderService`.
  - Knowledgebase lifecycle calls:
    - `knowledgebase/{db}/schemas/create`
    - `knowledgebase/{db}/views/create`
    - `knowledgebase/{db}/schemas/update`
    - `knowledgebase/{db}/views/update`
    - `knowledgebase/{db}/delete`
  - Chat execution call:
    - `POST chat/` with payload containing user data, history (last 10 messages), `session_context`, and a `connection_string` dictionary.
  - Response parsing into `LlmResponse`.

## 8. Database Analysis
### Database type
- **SQL Server** via EF Core (`UseSqlServer`).

### DbContext
- `AIChatbot.Infrastructure/Data/AppDbContext.cs`
- Inherits: `IdentityDbContext<ApplicationUser>`.

### Tables/collections (entities registered in `AppDbContext`)
Evidence from DbSets:
- `ChatSessions`
- `Messages`
- `RefreshTokens`
- `ResponseMetadata`
- `ChatSessionNaming`
- `ConnectionStrings`
- `LocalFunctionBlocks`
- `PromptFunctions`
- `ConnectionExclusions`
- `RoleInstructions`
- `RoleConnectionMappings`
- `ExternalUserMappings`
- `RoleRuntimePermissions`
- `OtpVerifications`
- `RoleConnectionUserLookupConfigurations`
- `RuntimePermissionViews`

### Relationships (explicitly configured)
Evidence from `OnModelCreating`:
- `ChatSession` -> `Messages` (1-to-many)
  - `Message.ChatSessionId` foreign key
  - Cascade delete: `.OnDelete(DeleteBehavior.Cascade)`.
- `ConnectionString` -> `PromptFunction` (`ConnectionStringId`) cascade delete.
- `LocalFunctionBlock` uniqueness: index on `{ ConnectionId, FunctionName }`.
- `ConnectionExclusion` -> `ConnectionString` foreign key `ConnectionId` cascade delete.
- `RoleConnectionMapping`:
  - has one `Role` (IdentityRole) via `RoleId` cascade delete
  - has one `Connection` via `ConnectionId` cascade delete
- `ExternalUserMapping`:
  - has one `ApplicationUser` via `ApplicationUserId` cascade delete
  - has one `Connection` via `ConnectionId` cascade delete
- `RoleInstruction` entity exists; relationship mapping beyond key not identified from available snippet.

### Primary keys
Evidence from explicit `HasKey` calls:
- `ChatSession.Id`
- `Message.Id`
- `ConnectionString.Id`
- `LocalFunctionBlock.Id` (assumed by `HasKey(x => x.Id)` in its mapping)
- `PromptFunction.Id`
- `ConnectionExclusion.Id`
- `RoleConnectionMapping.Id`
- `ExternalUserMapping.Id`
- `RoleInstruction.Id`

### Foreign keys
Evidence from explicit `HasForeignKey` usage in `OnModelCreating` for multiple entities (as listed above).

## 9. API Documentation
> Only endpoints visible via code that was read are listed as fully enumerated. Any endpoint not read is not identified from the available source code.

### 9.1 `AIChatbot.Api/Controllers/AuthController.cs`
Base route: `api/auth/V1/Security-engine`

Endpoints (evidence-based from file read):
- `GET roles?token={token}` (AllowAnonymous, rate-limited `register`)
- `POST register` (AllowAnonymous, rate-limited `register`)
- `POST send-register-otp` (AllowAnonymous, rate-limited `register`)
- `POST resend-register-otp` (AllowAnonymous, rate-limited `register`)
- `POST verify-register-otp` (AllowAnonymous, rate-limited `register`)
- `POST login` (AllowAnonymous, rate-limited `login`)
- `POST refresh` (AllowAnonymous, rate-limited `login`)
- `POST logout` (AuthenticatedUser)
- `GET Get/User-Details` (AuthenticatedUser)
- `POST forgot-password` (AllowAnonymous, rate-limited `login`)
- `POST verify-otp` (AllowAnonymous, rate-limited `login`)
- `POST reset-password` (AllowAnonymous, rate-limited `login`)
- `POST change-password` (AuthenticatedUser)

Request/response DTOs are not fully enumerated in this report because those DTO classes were not read.

### 9.2 `AIChatbot.Api/Controllers/ChatController.cs`
Base route: `api/chat/V1/Conversation-engine`
Auth: `[Authorize(Policy="AuthenticatedUser")]`
Rate limiting: `EnableRateLimiting("chat")`

Endpoints (evidence-based):
- `GET Get/ChatSessions`
- `GET {chatSessionId:guid}/messages`
- `POST Push-Query/Session!` (basic validation: non-empty message, max length 10,000)
- `POST excel/generate` (returns XLSX file bytes)
- `POST {chatSessionId:guid}/messages/{messageId:guid}/retry`
- `DELETE {chatSessionId:guid}/Delete` (returns 204 NoContent)

### 9.3 `AIChatbot.Api/Controllers/AgentController.cs`
Base route: `api/agent/V1/prompt-engine`
Auth: `[AllowAnonymous]` (note: runtime resolution still depends on user/role mapping)
Rate limiting: `EnableRateLimiting("chat")`

Endpoints (evidence-based):
- `POST Get-DB-Functions`
- `POST Get-DB-Function-ById`
- `POST Get-Runtime`
- `POST Get-Table-Runtime`

### 9.4 `AIChatbot.Api/Controllers/AccessController.cs`
Base route: `api/access/V1/Data-Setup-engine`
Auth: `SuperAdminOnly`
Rate limiting: `RolesPolicy`

Endpoints (evidence-based from file read):
- `GET GETschema/{connectionId}`
- `POST Role-and-DB/assign`
- `GET Role-and-DB/{roleId}`
- `DELETE Role-and-DB/remove/{roleId}/{connectionId}`
- `POST lookup-for-user-using/save`
- `GET lookup-for-user-using/{roleId}/{connectionId}`
- `DELETE lookup-for-user-using/{roleId}/{connectionId}`
- `POST Exclude-from-DB/save`
- `GET Exclude-from-DB/{connectionId}`
- `DELETE Exclude-from-DB/{connectionId}`
- `POST runtime-access/save`
- `GET runtime-access/{roleId}/{connectionId}`
- `DELETE runtime-access/{roleId}/{connectionId}`
- `GET runtime-access/view/{roleId}/{connectionId}`

### 9.5 `AIChatbot.Api/Controllers/RolemanagerController.cs`
Base route: `api/rolemanager/V1/role-engine`
Auth: `SuperAdminOnly` (controller-level)
Rate limiting: `RolesPolicy`.

Endpoints (evidence-based from file read):
- `GET LC1_listRoles`
- `POST LC1_gen`
- `DELETE Discard_Role/{roleId}`
- `GET LC1_listUser/{roleId}`
- `GET roles-by-connection/{connectionId:guid}`
- `DELETE Discard_user/{userId}` (additionally `[Authorize]`)

### 9.6 `AIChatbot.Api/Controllers/SupersetupController.cs`
Base route: `api/supersetup/V1/setup-engine`
Auth: `SuperAdminOnly`
Rate limiting: `RolesPolicy`

Endpoints (evidence-based from file read):
- `POST connection/test`
- `POST connection/create-kb`
- `POST connection/activate`
- `POST connection/update-kb`
- `POST function/Local`
- `DELETE` (expects `connectionId` and/or `functionId` as query params)
- `POST Function/global`
- `POST functions/sync-to-global`
- `GET connections`
- `GET connections/{connectionId}/functions`
- `GET functions/{functionId}`
- `GET functions/global/Global_function`

### 9.7 Remaining controllers
`AIChatbot.Api/Controllers/*` includes controllers listed in directory, but not all were read. Any endpoint not covered above is **Not identified from the available source code.**

## 10. Authentication & Authorization
### Authentication mechanism
- JWT bearer authentication configured in `AIChatbot.Api/Program.cs`.
  - Token validation: issuer, audience, lifetime, signing key.
  - Signing key from env var `JWT__KEY` or `Jwt:Key`.
- Identity integration: `AddIdentity<ApplicationUser, IdentityRole>()` with token providers.

### Authorization mechanism
- Policies configured in `Program.cs`:
  - `SuperAdminOnly`: `policy.RequireRole("SuperAdmin")`
  - `AuthenticatedUser`: `policy.RequireAuthenticatedUser()`

### Claim extraction
Controllers resolve user id by checking (evidence from controllers):
- `ClaimTypes.NameIdentifier`
- then `
