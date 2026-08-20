# Get-Table-Runtime API Documentation

## Overview
The `Get-Table-Runtime` endpoint retrieves role-based access control (RBAC) permissions and runtime configurations for specific database tables. It resolves runtime values from external user lookup configurations and returns filtered table access information based on the user's role.

---

## Endpoint Details

| Property | Value |
|----------|-------|
| **HTTP Method** | POST |
| **Route** | `/api/agent/V1/prompt-engine/Get-Table-Runtime` |
| **Authorization** | AllowAnonymous |
| **Rate Limiting** | Enabled (chat) |
| **Controller** | AgentController |

---

## Request

### Request Method
```
POST /api/agent/V1/prompt-engine/Get-Table-Runtime
Content-Type: application/json
```

### Request Body
```json
{
  "userId": "string",
  "tableNames": ["string", "string"]
}
```

### Request Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | string | Yes | The unique identifier of the user requesting table runtime data |
| `tableNames` | array[string] | Yes | List of fully qualified table names in format `schema.table` (e.g., `dbo.Users`, `sales.Orders`) |

### Request Example
```json
{
  "userId": "user-12345",
  "tableNames": ["dbo.Users", "dbo.Orders", "sales.Products"]
}
```

---

## Response

### Success Response (HTTP 200)
```json
{
  "success": "success",
  "messages": null,
  "userId": "string",
  "roleId": "string",
  "connectionId": "string",
  "userLookupConfiguration": {
    "userTableName": "string",
    "userIdColumn": "string",
    "emailColumn": "string",
    "phoneColumn": "string"
  },
  "permissions": {
    "roleName": {
      "schema.tableName": {
        "reason": "string",
        "access_level": "full|filtered|denied",
        "required_filters": [
          {
            "column": "string",
            "filter_type": "id|enum",
            "values": ["value1", "value2"] | "comma-separated-values"
          }
        ]
      }
    }
  }
}
```

### Partial Success Response (Some Tables Not Configured)
```json
{
  "success": "some tables not configured",
  "messages": {
    "schema.unconfiguredTable": "This table is not configured with lookup"
  },
  "userId": "string",
  "roleId": "string",
  "connectionId": "string",
  "userLookupConfiguration": { ... },
  "permissions": { ... }
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `success` | string | Status indicator: `"success"` or `"some tables not configured"` |
| `messages` | object | Contains error messages for tables that are not configured (null if all configured) |
| `userId` | string | The user ID from the request |
| `roleId` | string | The role ID associated with the user |
| `connectionId` | string | The database connection ID |
| `userLookupConfiguration` | object | Configuration for user lookup in external systems |
| `userLookupConfiguration.userTableName` | string | Name of the table containing user data |
| `userLookupConfiguration.userIdColumn` | string | Column name for user ID |
| `userLookupConfiguration.emailColumn` | string | Column name for email |
| `userLookupConfiguration.phoneColumn` | string | Column name for phone |
| `permissions` | object | Role-based permissions grouped by role name |
| `permissions[roleName][schema.table]` | object | Permissions for a specific table |
| `permissions[roleName][schema.table].reason` | string | Reason for the access level |
| `permissions[roleName][schema.table].access_level` | string | Access level: `full`, `filtered`, or `denied` |
| `permissions[roleName][schema.table].required_filters` | array | List of filters applied to the table |
| `required_filters[].column` | string | Column name to filter on |
| `required_filters[].filter_type` | string | Type of filter: `id` (user ID based) or `enum` (predefined values) |
| `required_filters[].values` | array/string | Filter values applicable to this column |

---

## Error Responses

### 404 Not Found - User Not Found
```json
{
  "success": false,
  "message": "User not found."
}
```
**Cause**: The provided `userId` does not exist in the system.

---

### 400 Bad Request - User Role Not Found
```json
{
  "success": false,
  "message": "User role not found."
}
```
**Cause**: The user exists but has no assigned role.

---

### 400 Bad Request - Role Not Found
```json
{
  "success": false,
  "message": "Role not found."
}
```
**Cause**: The role associated with the user could not be retrieved from the system.

---

### 400 Bad Request - No DB Assigned to Role
```json
{
  "success": false,
  "message": "No DB assigned to role."
}
```
**Cause**: The user's role has no database connection mapping configured.

---

### 400 Bad Request - Connection Not Found
```json
{
  "success": false,
  "message": "Connection not found."
}
```
**Cause**: The database connection associated with the role mapping could not be found.

---

## Use Cases

### Use Case 1: Retrieve Filtered Access for Multiple Tables
**Scenario**: A user with a "Manager" role needs to see permissions for Users, Orders, and Products tables.

**Request**:
```json
{
  "userId": "manager-001",
  "tableNames": ["dbo.Users", "sales.Orders", "inventory.Products"]
}
```

**Response**:
```json
{
  "success": "success",
  "userId": "manager-001",
  "roleId": "role-002",
  "connectionId": "conn-003",
  "userLookupConfiguration": {
    "userTableName": "dbo.Users",
    "userIdColumn": "UserId",
    "emailColumn": "Email",
    "phoneColumn": "Phone"
  },
  "permissions": {
    "manager": {
      "dbo.Users": {
        "reason": "Manager can view user data for their department",
        "access_level": "filtered",
        "required_filters": [
          {
            "column": "DepartmentId",
            "filter_type": "id",
            "values": ["dept-101", "dept-102"]
          }
        ]
      },
      "sales.Orders": {
        "reason": "Manager has full access to orders",
        "access_level": "full",
        "required_filters": []
      },
      "inventory.Products": {
        "reason": "Access denied - not in inventory team",
        "access_level": "denied",
        "required_filters": []
      }
    }
  }
}
```

### Use Case 2: Partial Configuration
**Scenario**: Some requested tables are not configured for the user's role.

**Request**:
```json
{
  "userId": "user-123",
  "tableNames": ["dbo.Users", "dbo.Analytics"]
}
```

**Response**:
```json
{
  "success": "some tables not configured",
  "messages": {
    "dbo.Analytics": "This table is not configured with lookup"
  },
  "userId": "user-123",
  "roleId": "role-456",
  "connectionId": "conn-789",
  "userLookupConfiguration": { ... },
  "permissions": {
    "user": {
      "dbo.Users": { ... }
    }
  }
}
```

---

## Integration Guide

### Prerequisites
- User must exist in the system
- User must have an assigned role
- Role must have a database connection mapping
- Requested tables must exist in the connected database

### Steps to Integrate

1. **Prepare Request**
   - Collect the user ID
   - Compile a list of fully qualified table names

2. **Make API Call**
   ```csharp
   var client = new HttpClient();
   var request = new GetTableRuntimeRequest
   {
       UserId = "user-id",
       TableNames = new[] { "dbo.Users", "dbo.Orders" }
   };
   
   var response = await client.PostAsJsonAsync(
       "https://your-api/api/agent/V1/prompt-engine/Get-Table-Runtime",
       request
   );
   ```

3. **Handle Response**
   - Check the `success` field to determine status
   - Extract permissions for each table
   - Apply filters based on `required_filters` array
   - Build SQL WHERE clauses from filter values

4. **Apply Filters in Queries**
   - For `filter_type: "id"` - Use values as primary keys or user IDs
   - For `filter_type: "enum"` - Use values as column values (e.g., status codes)

### Example Implementation (C#)
```csharp
public async Task<List<UserData>> GetUserDataWithPermissions(string userId, string tableName)
{
    var response = await _agentController.GetTableRuntime(
        new GetTableRuntimeRequest 
        { 
            UserId = userId, 
            TableNames = new[] { tableName } 
        }
    );
    
    if (response.success != "success")
        throw new Exception("User does not have access to this table");
    
    var permissions = response.permissions[userRole][tableName];
    
    if (permissions.access_level == "denied")
        throw new UnauthorizedAccessException("Access denied");
    
    var filters = permissions.required_filters;
    var query = _dbContext.Users.AsQueryable();
    
    foreach (var filter in filters)
    {
        if (filter.filter_type == "id")
            query = query.Where(u => filter.values.Contains(u.Id));
    }
    
    return await query.ToListAsync();
}
```

---

## Performance Considerations

- **Caching**: Consider caching permissions for frequently accessed users/roles
- **Batch Processing**: Group multiple table requests into single API calls
- **Database Load**: Runtime permission queries hit multiple repositories
- **External User Lookup**: Resolution of runtime values may involve external API calls

---

## Security Notes

- The endpoint uses rate limiting (chat policy)
- Although marked `AllowAnonymous`, access is controlled by role assignments
- Permissions are resolved at runtime based on user's role
- Filter values are validated before being returned

---

## Related Endpoints

- `[HttpPost("Get-Runtime")]` - Get complete runtime permissions for a user
- `[HttpPost("Get-DB-Functions")]` - Get available database functions
- `[HttpPost("Get-DB-Function-ById")]` - Get specific function details

---

## Last Updated
Generated: 2026-06-12

## Support
For issues or questions regarding this API, contact the development team or refer to the AgentController implementation.
