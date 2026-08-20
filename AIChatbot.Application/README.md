AIChatbot API Reference (Full DTOs & Error messages)

This file documents all 53 API endpoints. For each endpoint it provides:
- HTTP method and path
- Authentication / rate-limiting
- Input DTO name with fields (type + required)
- Success response DTO with fields
- Error responses (status + exact message text returned by code where available)

Where DTOs are defined in code their names are used. Where fields are inferred they are marked as (inferred).

---

Controller: Authentication (base `/api/auth/V1/Security-engine`)

1) GET `/roles`
- Auth: Anonymous
- Input: query `token` (string, optional)
- Success 200: `RolesResponse` { `success`: bool, `roles`: `RoleDto[]` }
  - `RoleDto` (inferred): `{ id: string, name: string }`
- Errors:
  - 400 Bad Request: `{"success":false,"message":"Invalid token"}`
  - 500 Internal Server Error

2) POST `/register`
- Auth: Anonymous, rate-limited
- Input: `RegisterRequest` (defined) fields:
  - `FirstName` string (required),
  - `LastName` string (required),
  - `Email` string (required),
  - `Phone` string (optional),
  - `Password` string (required),
  - `Role` string (optional),
  - `Token` string (optional)
- Success 201: `RegisterResponse` (inferred): `{ success: true, message: string, userId: string }`
- Errors:
  - 400 Validation: `{"success":false,"message":"Validation failed"}`
  - 409 Conflict: `{"success":false,"message":"Email already registered"}`
  - 429 Too Many Requests
  - 500

3) POST `/send-register-otp`
- Auth: Anonymous, rate-limited
- Input: `SendRegisterOtpCommand` (defined) — typically `{ Email?: string, Phone?: string }`
- Success 200: `{"success":true,"message":"OTP sent successfully"}`
- Errors: 400 `{"success":false,"message":"Invalid email or phone"}`, 429, 500

4) POST `/resend-register-otp`
- Auth: Anonymous, rate-limited
- Input: `ResendRegisterOtpCommand` — `{ Email/Phone }`
- Success 200: `{"success":true,"message":"OTP resent successfully"}`
- Errors: 400 `{"success":false,"message":"No pending OTP to resend"}`, 429, 500

5) POST `/verify-register-otp`
- Auth: Anonymous, rate-limited
- Input: `VerifyRegisterOtpCommand` — `{ Email/Phone, Otp }`
- Success 200: `{ "success": true, "registerToken": string }`
- Errors: 400 `{"success":false,"message":"Invalid or expired OTP"}` , 429, 500

6) POST `/login`
- Auth: Anonymous, rate-limited
- Input: `LoginRequest` (defined): `{ Email: string, Password: string }`
- Success 200: `AuthResult` (defined in code): fields include `AccessToken`, `RefreshToken`, `ExpiresIn`, `User` (profile)
- Errors: 400 `{"success":false,"message":"Invalid email or password"}`, 429, 500

7) POST `/refresh`
- Auth: Anonymous, rate-limited
- Input: `RefreshTokenRequest` `{ RefreshToken: string }`
- Success 200: new `AuthResult` with tokens
- Errors: 400 `{"success":false,"message":"Invalid or expired refresh token"}`, 401, 429, 500

8) POST `/logout`
- Auth: `[Authorize(Policy = "AuthenticatedUser")]`
- Input: none; server reads userId from claims
- Success 204 No Content
- Errors: 401 thrown message: `"Invalid token"`, 500

9) GET `/Get/User-Details`
- Auth: `[Authorize(Policy = "AuthenticatedUser")]`
- Input: none (token)
- Success 200: `UserProfileResult` (defined in code)
- Errors: 401 `{"success":false,"message":"Invalid token"}`, 404 `{"success":false,"message":"User not found."}`, 500

Authentication controller summary: Validation errors return 400 with message text from validators or commands. Rate-limited endpoints may return 429.

Controller: Agent (base `/api/agent/V1/prompt-engine`)

10) POST `/Get-DB-Functions`
- Auth: Anonymous, rate-limited `chat`
- Input: `GetDbPromptRequest` (defined): `{ ConnectionId: Guid }`
- Success 200: `GetDbFunctionsResponse` {
  - `success`: true,
  - `connectionId`: Guid,
  - `database`: string,
  - `functions`: `FunctionDto[]` where `FunctionDto` = `{ id: Guid, functionName: string, systemPrompt: string }`
}
- Errors: 404 `{"success":false,"message":"Database not found."}`, 429, 500

11) POST `/Get-DB-Function-ById`
- Auth: Anonymous
- Input: `GetDbFunctionByIdRequest` `{ ConnectionId: Guid, FunctionId: Guid }`
- Success 200: `{ success: true, connectionId: Guid, database: string, function: FunctionDto }`
- Errors: 404 `{"success":false,"message":"Function not found."}`, 500

12) POST `/Get-Runtime`
- Auth: Anonymous
- Input: `GetRuntimeRequest` `{ UserId: string }`
- Success 200: `GetRuntimeResponse` {
  - `success`: true,
  - `userId`: string,
  - `role`: string,
  - `roleId`: string,
  - `connectionId`: Guid,
  - `runtime`: object mapping `"schema.table"` -> map of column metadata
}
- Errors:
  - 404 `{"success":false,"message":"User not found."}`
  - 400 `{"success":false,"message":"User role not found."}`
  - 400 `{"success":false,"message":"No database assigned to role."}`
  - 500

13) POST `/Get-Table-Runtime`
- Auth: Anonymous
- Input: `GetTableRuntimeRequest` `{ UserId: string, TableNames: string[] }`
- Success 200: `GetTableRuntimeResponse` {
  - `success`: true,
  - `userId`, `roleId`, `connectionId`,
  - `tables`: map `"schema.table"` -> `{ runtime: {...}, query: string, data: object[] }`
}
- Errors: 404 user, 400 `{"success":false,"message":"No DB assigned to role."}`, 400 `{"success":false,"message":"Connection not found."}`, 429, 500

Controller: Chat (base `/api/chat/V1/Conversation-engine`)

14) GET `/Get/ChatSessions`
- Auth: `[Authorize(Policy = "AuthenticatedUser")]`
- Input: token -> `UserId`
- Success 200: `ChatSessionSummaryResult[]` (defined)
- Errors: 401 `{"success":false,"message":"Unauthorized access."}`, 500

15) GET `/{chatSessionId}/messages`
- Auth: Authorized
- Input: route `chatSessionId` Guid
- Success 200: `ChatMessageResult[]`
- Errors: 401, 403, 404, 500

16) POST `/Push-Query/Session!`
- Auth: Authorized, rate-limited
- Input: `ChatRequest` { `ChatSessionId` Guid? , `Message` string }
- Validations:
  - 400 `{"success":false,"message":"Message cannot be empty."}`
  - 400 `{"success":false,"message":"Message too large."}` (if > 10000 chars)
- Success 200: `ChatExecutionResult` (ExecutionStatus.Success)
- Success 206: partial result when ExecutionStatus.PartiallyExecuted
- Errors: 401, 500 `{"success":false,"message":"Unexpected AI execution failure."}`

17) POST `/{chatSessionId}/messages/{messageId}/retry`
- Auth: Authorized
- Input: route params
- Success 200/206: `ChatExecutionResult`
- Errors: 401, 404, 500

18) DELETE `/{chatSessionId}/Delete`
- Auth: Authorized
- Input: route `chatSessionId`
- Success 204 No Content
- Errors: 401, 403, 404, 500

Controller: Supersetup (base `/api/supersetup/V1/setup-engine`)

19) POST `/connection/test`
- Auth: `[Authorize(Policy = "SuperAdminOnly")]`, rate-limited
- Input: `SaveConnectionCommand` {
  - `Id` Guid? (optional),
  - `ServerName` string (required),
  - `DatabaseName` string (required),
  - `AuthMode` string (required),
  - `Username` string?,
  - `Password` string?,
  - `ConnectionTimeout` int?,
  - `TrustCertificate` bool?
}
- Success 200: `SaveConnectionResult` `{ Success: true, Message: string, ConnectionId: Guid }`
- Errors (exact messages thrown in handler):
  - 400 `{"success":false,"message":"ServerName is required."}`
  - 400 `{"success":false,"message":"DatabaseName is required."}`
  - 400 `{"success":false,"message":"AuthMode is required."}`
  - 400 `{"success":false,"message":"Connection already exists for this server and database."}`
  - 400 `{"success":false,"message":"Another connection with same details already exists."}`
  - 400 `{"success":false,"message":"Unable to connect to database. Check server, credentials or database name."}`
  - 500

20) POST `/connection/create-kb`
- Auth: SuperAdminOnly
- Input: `CreateKnowledgebaseCommand` (defined)
- Success 200: `GenericResult`
- Errors: 400, 404, 500

21) POST `/connection/activate`
- Auth: SuperAdminOnly
- Input: `ActivateConnectionCommand` { ConnectionId: Guid }
- Success 200: `GenericResult`
- Errors: 400, 404, 500

22) POST `/connection/update-kb`
- Auth: SuperAdminOnly
- Input: `UpdateKnowledgebaseCommand`
- Success 200: `GenericResult`
- Errors: 400, 404, 500

23) POST `/function/Local`
- Auth: SuperAdminOnly
- Input: `SavePromptFunctionCommand` (defined)
- Success 200: `GenericResult`
- Errors: 400 validation, 500

24) DELETE (base)
- Auth: SuperAdminOnly
- Input: query `connectionId?: Guid`, `functionId?: Guid` (one must be present)
- Success 200: `GenericResult`
- Errors: 400 `{"success":false,"message":"Specify connectionId or functionId"}`, 404, 500

25) POST `/Function/global`
- Auth: SuperAdminOnly
- Input: `SaveGlobalPromptCommand`
- Success 200: `GenericResult`
- Errors: 400, 500

26) POST `/functions/sync-to-global`
- Auth: SuperAdminOnly
- Input: `SyncToGlobalCommand` { ConnectionId?: Guid, FunctionId?: Guid }
- Success 200: `{"success":true,"message":"Function synced with global"}`
- Errors: 400, 404, 500

27) GET `/connections`
- Auth: SuperAdminOnly
- Input: none
- Success 200: `ConnectionDto[]`
- Errors: 401/403, 500

28) GET `/connections/{connectionId}/functions`
- Auth: SuperAdminOnly
- Input: route `connectionId`
- Success 200: `FunctionDto[]`
- Errors: 404, 500

29) GET `/functions/{functionId}`
- Auth: SuperAdminOnly
- Input: route `functionId`
- Success 200: `FunctionDto`
- Errors: 404, 500

30) GET `/functions/global/Global_function`
- Auth: SuperAdminOnly
- Input: none
- Success 200: `GlobalFunctionNameDto[]`
- Errors: 500

Controller: Access (base `/api/access/V1/Data-Setup-engine`)

31) GET `/GETschema/{connectionId}`
- Auth: SuperAdminOnly
- Input: route `connectionId`
- Success 200: `DatabaseSchemaDto` (tables + columns)
- Errors: 404 `{"success":false,"message":"Connection not found."}`, 500

32) POST `/Role-and-DB/assign`
- Auth: SuperAdminOnly
- Input: `AssignRoleConnectionRequest` { `RoleId`: string, `ConnectionId`: Guid }
- Success 200: `GenericResult`
- Errors: 400, 404 role/connection, 409 already assigned, 500

33) GET `/Role-and-DB/{roleId}`
- Auth: SuperAdminOnly
- Input: route `roleId`
- Success 200: `RoleConnectionsDto[]`
- Errors: 404, 500

34) DELETE `/Role-and-DB/remove/{roleId}/{connectionId}`
- Auth: SuperAdminOnly
- Input: route params
- Success 200: `GenericResult`
- Errors: 404, 500

35) POST `/lookup-for-user-using/save`
- Auth: SuperAdminOnly
- Input: `SaveUserLookupConfigurationRequest` { RoleId, ConnectionId, UserTableName, UserIdColumn, EmailColumn, PhoneColumn, FirstNameColumn, LastNameColumn, FullNameColumn, UseSeparateFirstLast: bool, UseMergedFullName: bool }
- Success 200: `GenericResult`
- Errors: 400 validation, 404 table/columns not found, 500

36) GET `/lookup-for-user-using/{roleId}/{connectionId}`
- Auth: SuperAdminOnly
- Input: route
- Success 200: lookup config DTO
- Errors: 404, 500

37) DELETE `/lookup-for-user-using/{roleId}/{connectionId}`
- Auth: SuperAdminOnly
- Input: route
- Success 200: `GenericResult`
- Errors: 404, 500

38) POST `/Exclude-from-DB/save`
- Auth: SuperAdminOnly
- Input: `SaveConnectionExclusionRequest` { ConnectionId: Guid, Exclusions: TableExclusionDto[] }
- Success 200: `GenericResult`
- Errors: 400, 404, 500

39) GET `/Exclude-from-DB/{connectionId}`
- Auth: SuperAdminOnly
- Input: route
- Success 200: `{ connectionId: Guid, exclusions: TableExclusionDto[] }`
- Errors: 404, 500

40) DELETE `/Exclude-from-DB/{connectionId}`
- Auth: SuperAdminOnly
- Input: route
- Success 200: `GenericResult`
- Errors: 404, 500

41) POST `/runtime-access/save`
- Auth: SuperAdminOnly
- Input: `SaveRuntimePermissionRequest` { RoleId: string, ConnectionId: Guid, Permissions: RuntimePermissionDto[] }
  - `RuntimePermissionDto`: `{ SchemaName: string, TableName: string, ColumnName: string, FilterType: string, RuntimeKey?: string, Values?: string[] }`
- Success 200: `GenericResult` (message: "Runtime permissions and view saved successfully")
- Errors: 400 validation, 401/403, 404, 500

42) GET `/runtime-access/{roleId}/{connectionId}`
- Auth: SuperAdminOnly
- Input: route
- Success 200: `RuntimePermissionDto[]`
- Errors: 404, 500

43) DELETE `/runtime-access/{roleId}/{connectionId}`
- Auth: SuperAdminOnly
- Input: route
- Success 200: `GenericResult` (message: "Runtime permissions deleted successfully")
- Errors: 404, 500

44) GET `/runtime-access/view/{roleId}/{connectionId}`
- Auth: SuperAdminOnly
- Input: route
- Success 200: `RuntimePermissionViewDto` `{ ViewName: string, ViewSql: string }`
- Errors: 404, 500

Controller: Role management (base `/api/rolemanager/V1/role-engine`)

45) GET `/LC1_listRoles`
- Auth: Controller default `SuperAdminOnly` — requires authenticated user
- Input: token
- Success 200: `RoleListDto[]`
- Errors: 401 `{"success":false,"message":"Unauthorized access."}`, 500

46) POST `/LC1_gen`
- Auth: SuperAdminOnly
- Input: `CreateRoleRequest` { RoleName: string }
- Success 200: `GenericResult`
- Errors: 400 `{"success":false,"message":"Role creation failed: <details>"}`, 401, 500

47) DELETE `/Discard_Role/{roleId}`
- Auth: SuperAdminOnly
- Input: route `roleId`
- Success 200: `GenericResult`
- Errors: 401, 403, 404 `{"success":false,"message":"Role not found."}`, 500

48) GET `/LC1_listUser/{roleId}`
- Auth: SuperAdminOnly
- Input: route `roleId`
- Success 200: `RoleUsersDto[]`
- Errors: 401, 404, 500

49) DELETE `/Discard_user/{userId}`
- Auth: `[Authorize]` (any authenticated user)
- Input: route `userId`
- Success 200: `GenericResult` `{ "Success": true, "Message": "User deleted" }`
- Errors: 401, 403, 404 `{"success":false,"message":"User not found."}`, 500

---

Error conventions
- Validation / model errors: 400. Response typically `{"success":false,"message":"<text>","errors":{...}}` when validators used.
- Auth: 401 Unauthorized for missing/invalid token; 403 Forbidden for insufficient permissions. Response bodies vary but often `{ "success": false, "message": "Unauthorized access." }`.
- Not found: 404 with `{"success":false,"message":"<resource> not found."}`.
- Rate limit: 429 Too Many Requests.
- Server error: 500 Internal Server Error.

Next steps available on request:
- Export these DTOs and endpoints to an OpenAPI (Swagger) YAML/JSON file.
- Add example request/response bodies for each endpoint.

