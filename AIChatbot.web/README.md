AIChatbot API Reference

This README lists all 53 API endpoints found in the workspace with method, path, auth, request DTO (fields), successful response shape, and common error responses.

Format for each endpoint:
- Method & Path
- Auth / Rate limiting
- Request (content-type + DTO name + key fields)
- Success (status + example shape)
- Common errors (status + condition)

---

1) GET `api/auth/V1/Security-engine/roles`
- Auth: Anonymous
- Request: query `token` (string)
- Success 200: `{ "success": true, "roles": [ ... ] }`
- Errors: 400 malformed token, 500 server error

2) POST `api/auth/V1/Security-engine/register`
- Auth: Anonymous, rate-limited
- Request: `RegisterRequest` { `FirstName` string, `LastName` string, `Email` string, `Phone` string?, `Password` string, `Role` string?, `Token` string? }
- Success 201: command result (GenericResult-like) e.g. `{ "success": true, "message":"Registered", "userId": "..." }`
- Errors: 400 validation, 409 conflict (email exists), 429 rate-limit, 500

3) POST `api/auth/V1/Security-engine/send-register-otp`
- Auth: Anonymous, rate-limited
- Request: `SendRegisterOtpCommand` { email/phone }
- Success 200: `{ "success": true, "message": "OTP sent successfully" }`
- Errors: 400 invalid input, 429, 500

4) POST `api/auth/V1/Security-engine/resend-register-otp`
- Auth: Anonymous, rate-limited
- Request: `ResendRegisterOtpCommand` { email/phone }
- Success 200: `{ "success": true, "message": "OTP resent successfully" }`
- Errors: 400 no pending OTP, 429, 500

5) POST `api/auth/V1/Security-engine/verify-register-otp`
- Auth: Anonymous, rate-limited
- Request: `VerifyRegisterOtpCommand` { Email/Phone, Otp }
- Success 200: `{ "success": true, "registerToken": string }`
- Errors: 400 invalid/expired OTP, 429, 500

6) POST `api/auth/V1/Security-engine/login`
- Auth: Anonymous, rate-limited
- Request: `LoginRequest` { `Email`, `Password` }
- Success 200: `{ "accessToken": string, "refreshToken": string, "expiresIn": number, "user": { ... } }`
- Errors: 400 invalid credentials, 429, 500

7) POST `api/auth/V1/Security-engine/refresh`
- Auth: Anonymous, rate-limited
- Request: `RefreshTokenRequest` { `RefreshToken` }
- Success 200: `{ "accessToken": string, "refreshToken": string, "expiresIn": number }`
- Errors: 400 invalid/expired refresh token, 401, 429, 500

8) POST `api/auth/V1/Security-engine/logout`
- Auth: `AuthenticatedUser` policy
- Request: bearer token (server extracts user id)
- Success 204 NoContent
- Errors: 401 invalid/missing token, 500

9) GET `api/auth/V1/Security-engine/Get/User-Details`
- Auth: `AuthenticatedUser` policy
- Request: bearer token
- Success 200: user profile (GetUserProfileQuery result)
- Errors: 401, 404 user not found, 500

10) POST `api/auth/V1/Security-engine/forgot-password`
- Auth: Anonymous, rate-limited
- Request: `ForgotPasswordCommand` { `Email` }
- Success 200: GenericResult `{ "success": true, "message": "Reset link sent" }`
- Errors: 400 email not registered, 429, 500

11) POST `api/auth/V1/Security-engine/verify-otp`
- Auth: Anonymous, rate-limited
- Request: `VerifyOtpCommand` { token/otp }
- Success 200: `{ "success": true, "message": "Reset link sent to email" }`
- Errors: 400 invalid/expired OTP, 429, 500

12) POST `api/auth/V1/Security-engine/reset-password`
- Auth: Anonymous, rate-limited
- Request: `ResetPasswordCommand` { resetToken, newPassword }
- Success 200: `{ "success": true }`
- Errors: 400 invalid token, password policy violation, 429, 500

13) POST `api/auth/V1/Security-engine/change-password`
- Auth: `AuthenticatedUser` policy
- Request: `ChangePasswordCommand` { OldPassword, NewPassword }
- Success 200: `{ "success": true, "message": "Password changed successfully" }`
- Errors: 400 invalid old password, 401, 500

14) POST `api/agent/V1/prompt-engine/Get-DB-Functions`
- Auth: Anonymous, rate-limited (chat)
- Request: `GetDbPromptRequest` { `ConnectionId` Guid }
- Success 200: `{ "success": true, "connectionId": guid, "database": string, "functions": [ { functionId, functionName, systemPrompt } ] }`
- Errors: 404 connection not found, 429, 500

15) POST `api/agent/V1/prompt-engine/Get-DB-Function-ById`
- Auth: Anonymous
- Request: `GetDbFunctionByIdRequest` { `ConnectionId`, `FunctionId` }
- Success 200: `{ "success": true, "connectionId": guid, "database": string, "function": {functionId,functionName,systemPrompt} }`
- Errors: 404 connection or function not found, 500

16) POST `api/agent/V1/prompt-engine/Get-Runtime`
- Auth: Anonymous
- Request: `GetRuntimeRequest` { `UserId` }
- Success 200: `{ "success": true, "userId": string, "role": string, "roleId": string, "connectionId": guid, "runtime": { "schema.table": { "column": {filterType, runtimeKey, runtimeValue, values} } } }`
- Errors: 404 user not found, 400 missing role, 400 no DB assigned to role, 500

17) POST `api/agent/V1/prompt-engine/Get-Table-Runtime`
- Auth: Anonymous
- Request: `GetTableRuntimeRequest` { `UserId`, `TableNames`: [ "schema.table" ] }
- Success 200: `{ "success": true, "userId": string, "roleId": string, "connectionId": guid, "tables": { "schema.table": { runtime, query, data } } }`
- Errors: 404 user, 400 no role/DB mapping, 400 connection not found, 429, 500

18) GET `api/chat/V1/Conversation-engine/Get/ChatSessions`
- Auth: `AuthenticatedUser`
- Request: bearer token
- Success 200: list of chat sessions (GetChatSessionsQuery result)
- Errors: 401, 500

19) GET `api/chat/V1/Conversation-engine/{chatSessionId}/messages`
- Auth: `AuthenticatedUser`
- Request: route `chatSessionId` GUID
- Success 200: messages list (GetChatMessagesQuery result)
- Errors: 401, 403, 404, 500

20) POST `api/chat/V1/Conversation-engine/Push-Query/Session!`
- Auth: `AuthenticatedUser`, rate-limited (chat)
- Request: `ChatRequest` { `ChatSessionId`?, `Message` required }
- Validation: Message non-empty, max length 10000
- Success 200: `ChatExecutionResult` (ExecutionStatus.Success)
- Success 206: partial result (ExecutionStatus.PartiallyExecuted)
- Errors: 400 message empty, 400 message too large, 401, 500 unexpected AI failure

21) POST `api/chat/V1/Conversation-engine/{chatSessionId}/messages/{messageId}/retry`
- Auth: `AuthenticatedUser`
- Request: route guids
- Success 200 or 206: ChatExecutionResult
- Errors: 401, 404, 500

22) DELETE `api/chat/V1/Conversation-engine/{chatSessionId}/Delete`
- Auth: `AuthenticatedUser`
- Request: route `chatSessionId`
- Success 204 NoContent
- Errors: 401, 403, 404, 500

23) POST `api/supersetup/V1/setup-engine/connection/test`
- Auth: `SuperAdminOnly`, rate-limited (RolesPolicy)
- Request: `SaveConnectionCommand` { `Id`? Guid, `ServerName` string required, `DatabaseName` string required, `AuthMode` string required, `Username`?, `Password`?, `ConnectionTimeout`?, `TrustCertificate` bool }
- Behavior: validates required fields, tests DB connection, checks duplicates, creates or updates entity
- Success 200: `SaveConnectionResult` `{ "success": true, "message": "Connection tested and saved successfully.", "connectionId": guid }`
- Errors: 400 BadHttpRequestException when required fields missing or duplicate connection, 400 DB connection failure -> "Unable to connect to database...", 500

24) POST `api/supersetup/V1/setup-engine/connection/create-kb`
- Auth: `SuperAdminOnly`
- Request: `CreateKnowledgebaseCommand`
- Success 200: GenericResult
- Errors: 400, 404, 500

25) POST `api/supersetup/V1/setup-engine/connection/activate`
- Auth: `SuperAdminOnly`
- Request: `ActivateConnectionCommand` { ConnectionId }
- Success 200: GenericResult
- Errors: 400, 404, 500

26) POST `api/supersetup/V1/setup-engine/connection/update-kb`
- Auth: `SuperAdminOnly`
- Request: `UpdateKnowledgebaseCommand`
- Success 200: GenericResult
- Errors: 400, 404, 500

27) POST `api/supersetup/V1/setup-engine/function/Local`
- Auth: `SuperAdminOnly`
- Request: `SavePromptFunctionCommand`
- Success 200: GenericResult
- Errors: 400, 500

28) DELETE `api/supersetup/V1/setup-engine` (query `connectionId` or `functionId`)
- Auth: `SuperAdminOnly`
- Request: query `Guid? connectionId`, `Guid? functionId` (one required)
- Success 200: GenericResult
- Errors: 400 missing identifiers, 404 not found, 500

29) POST `api/supersetup/V1/setup-engine/Function/global`
- Auth: `SuperAdminOnly`
- Request: `SaveGlobalPromptCommand`
- Success 200: GenericResult
- Errors: 400, 500

30) POST `api/supersetup/V1/setup-engine/functions/sync-to-global`
- Auth: `SuperAdminOnly`
- Request: `SyncToGlobalCommand`
- Success 200: `{ "success": true, "message": "Function synced with global" }`
- Errors: 400, 404, 500

31) GET `api/supersetup/V1/setup-engine/connections`
- Auth: `SuperAdminOnly`
- Request: none
- Success 200: list of connections
- Errors: 401/403, 500

32) GET `api/supersetup/V1/setup-engine/connections/{connectionId}/functions`
- Auth: `SuperAdminOnly`
- Request: route `connectionId`
- Success 200: functions list
- Errors: 404, 500

33) GET `api/supersetup/V1/setup-engine/functions/{functionId}`
- Auth: `SuperAdminOnly`
- Request: route `functionId`
- Success 200: function DTO
- Errors: 404, 500

34) GET `api/supersetup/V1/setup-engine/functions/global/Global_function`
- Auth: `SuperAdminOnly`
- Request: none
- Success 200: list of global function names
- Errors: 500

35) GET `api/access/V1/Data-Setup-engine/GETschema/{connectionId}`
- Auth: `SuperAdminOnly`
- Request: route `connectionId`
- Success 200: DB schema DTO
- Errors: 404 connection not found, 500

36) POST `api/access/V1/Data-Setup-engine/Role-and-DB/assign`
- Auth: `SuperAdminOnly`
- Request: `AssignRoleConnectionRequest` { `RoleId`, `ConnectionId` }
- Success 200: GenericResult
- Errors: 400, 404, 409, 500

37) GET `api/access/V1/Data-Setup-engine/Role-and-DB/{roleId}`
- Auth: `SuperAdminOnly`
- Request: route `roleId`
- Success 200: list of DBs assigned to role
- Errors: 404, 500

38) DELETE `api/access/V1/Data-Setup-engine/Role-and-DB/remove/{roleId}/{connectionId}`
- Auth: `SuperAdminOnly`
- Request: route `roleId`, `connectionId`
- Success 200: GenericResult
- Errors: 404, 500

39) POST `api/access/V1/Data-Setup-engine/lookup-for-user-using/save`
- Auth: `SuperAdminOnly`
- Request: `SaveUserLookupConfigurationRequest` { RoleId, ConnectionId, UserTableName, UserIdColumn, EmailColumn, PhoneColumn, FirstNameColumn, LastNameColumn, FullNameColumn, UseSeparateFirstLast bool, UseMergedFullName bool }
- Success 200: GenericResult
- Errors: 400 validation, 404 table/columns not found, 500

40) GET `api/access/V1/Data-Setup-engine/lookup-for-user-using/{roleId}/{connectionId}`
- Auth: `SuperAdminOnly`
- Request: route `roleId`, `connectionId`
- Success 200: lookup configuration DTO
- Errors: 404 not configured, 500

41) DELETE `api/access/V1/Data-Setup-engine/lookup-for-user-using/{roleId}/{connectionId}`
- Auth: `SuperAdminOnly`
- Request: route `roleId`, `connectionId`
- Success 200: GenericResult
- Errors: 404, 500

42) POST `api/access/V1/Data-Setup-engine/Exclude-from-DB/save`
- Auth: `SuperAdminOnly`
- Request: `SaveConnectionExclusionRequest` { ConnectionId, Exclusions[] }
- Success 200: GenericResult
- Errors: 400, 404, 500

43) GET `api/access/V1/Data-Setup-engine/Exclude-from-DB/{connectionId}`
- Auth: `SuperAdminOnly`
- Request: route `connectionId`
- Success 200: `{ connectionId, exclusions: [...] }`
- Errors: 404, 500

44) DELETE `api/access/V1/Data-Setup-engine/Exclude-from-DB/{connectionId}`
- Auth: `SuperAdminOnly`
- Request: route `connectionId`
- Success 200: GenericResult
- Errors: 404, 500

45) POST `api/access/V1/Data-Setup-engine/runtime-access/save`
- Auth: `SuperAdminOnly`
- Request: `SaveRuntimePermissionRequest` { RoleId, ConnectionId, Permissions: [ { SchemaName, TableName, ColumnName, FilterType, RuntimeKey?, Values? } ] }
- Success 200: `{ "success": true, "message": "Runtime permissions and view saved successfully" }`
- Errors: 400 validation, 401/403, 404, 500

46) GET `api/access/V1/Data-Setup-engine/runtime-access/{roleId}/{connectionId}`
- Auth: `SuperAdminOnly`
- Request: route `roleId`, `connectionId`
- Success 200: list of runtime permission DTOs
- Errors: 404, 500

47) DELETE `api/access/V1/Data-Setup-engine/runtime-access/{roleId}/{connectionId}`
- Auth: `SuperAdminOnly`
- Request: route `roleId`, `connectionId`
- Success 200: `{ "success": true, "message": "Runtime permissions deleted successfully" }`
- Errors: 404, 500

48) GET `api/access/V1/Data-Setup-engine/runtime-access/view/{roleId}/{connectionId}`
- Auth: `SuperAdminOnly`
- Request: route `roleId`, `connectionId`
- Success 200: `RuntimePermissionViewDto` `{ "ViewName": string, "ViewSql": string }`
- Errors: 404, 500

49) GET `api/rolemanager/V1/role-engine/LC1_listRoles`
- Auth: controller default `SuperAdminOnly` but handler checks authenticated `UserId`
- Request: bearer token
- Success 200: list of role DTOs
- Errors: 401, 500

50) POST `api/rolemanager/V1/role-engine/LC1_gen`
- Auth: controller default `SuperAdminOnly`
- Request: `CreateRoleRequest` { `RoleName` }
- Success 200: GenericResult
- Errors: 400 validation, 401, 500

51) DELETE `api/rolemanager/V1/role-engine/Discard_Role/{roleId}`
- Auth: controller default `SuperAdminOnly`
- Request: route `roleId`, bearer token
- Success 200: GenericResult
- Errors: 401, 403, 404, 500

52) GET `api/rolemanager/V1/role-engine/LC1_listUser/{roleId}`
- Auth: controller default `SuperAdminOnly`
- Request: route `roleId`
- Success 200: users list for role
- Errors: 401, 404, 500

53) DELETE `api/rolemanager/V1/role-engine/Discard_user/{userId}`
- Auth: `[Authorize]` (any authenticated user)
- Request: route `userId`
- Success 200: GenericResult
- Errors: 401, 403, 404, 500

---

Notes
- Many endpoints return `GenericResult` which typically has `{ "Success": bool, "Message": string }`.
- Validation errors are usually 400 (FluentValidation may produce structured error responses).
- Authentication/authorization failures return 401/403 from ASP.NET Core.
- Rate-limited endpoints may return 429.
- Chat execution endpoints can return 206 Partial Content when the operation is partially executed.

If you want I can:
- Generate an OpenAPI (Swagger) JSON/YAML file from these endpoint specs and the DTOs in the codebase.
- Export this README as `docs/api.md` or add examples for each DTO.

