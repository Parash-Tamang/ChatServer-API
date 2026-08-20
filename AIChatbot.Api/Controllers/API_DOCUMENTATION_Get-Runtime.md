# Get-Runtime API Documentation

## Overview
The `Get-Runtime` endpoint retrieves the complete role-based access control (RBAC) permissions and runtime configurations for a user. It returns all accessible tables with their permission levels, required filters, and user lookup configurations. This endpoint provides a comprehensive view of what data a user can access based on their assigned role.

---

## Endpoint Details

| Property | Value |
|----------|-------|
| **HTTP Method** | POST |
| **Route** | `/api/agent/V1/prompt-engine/Get-Runtime` |
| **Authorization** | AllowAnonymous |
| **Rate Limiting** | Enabled (chat) |
| **Controller** | AgentController |

---

## Request

### Request Method
```
POST /api/agent/V1/prompt-engine/Get-Runtime
Content-Type: application/json
```

### Request Body
```json
{
  "userId": "string"
}
```

### Request Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `userId` | string | Yes | The unique identifier of the user requesting runtime permissions |

### Request Example
```json
{
  "userId": "user-12345"
}
```

---

## Response

### Success Response (HTTP 200)
```json
{
  "metadata": {
    "version": "1.0",
    "database": "string",
    "description": "Role-Based Access Control (RBAC) Permissions Dictionary",
    "note": "Only columns with required filters (id, enum) are listed. Other columns are readable if the table is accessible."
  },
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

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `metadata` | object | Information about the response and API version |
| `metadata.version` | string | API version (currently "1.0") |
| `metadata.database` | string | Name of the connected database |
| `metadata.description` | string | Description of the response content |
| `metadata.note` | string | Important note about filter visibility |
| `userLookupConfiguration` | object | Configuration for user lookup in external systems (null if not configured) |
| `userLookupConfiguration.userTableName` | string | Name of the table containing user data |
| `userLookupConfiguration.userIdColumn` | string | Column name for user ID |
| `userLookupConfiguration.emailColumn` | string | Column name for email |
| `userLookupConfiguration.phoneColumn` | string | Column name for phone |
| `permissions` | object | All role-based permissions grouped by role name |
| `permissions[roleName]` | object | Permissions for a specific role |
| `permissions[roleName][schema.table]` | object | Permissions for a specific table |
| `permissions[roleName][schema.table].reason` | string | Reason/justification for the access level |
| `permissions[roleName][schema.table].access_level` | string | Access level: `full` (all columns), `filtered` (conditional access), or `denied` (no access) |
| `permissions[roleName][schema.table].required_filters` | array | List of filters that must be applied to queries |
| `required_filters[].column` | string | Column name to apply filter on |
| `required_filters[].filter_type` | string | Type of filter: `id` (user ID based) or `enum` (predefined enumeration values) |
| `required_filters[].values` | array/string | Actual filter values to use in queries |

---

## Error Responses

### 404 Not Found - User Not Found
```json
{
  "success": false,
  "message": "User not found."
}
```
**HTTP Status**: 404  
**Cause**: The provided `userId` does not exist in the system.  
**Resolution**: Verify the user ID is correct and the user exists in the database.

---

### 400 Bad Request - User Role Not Found
```json
{
  "success": false,
  "message": "User role not found."
}
```
**HTTP Status**: 400  
**Cause**: The user exists but has no assigned role.  
**Resolution**: Assign a role to the user before calling this endpoint.

---

### 400 Bad Request - Role Entity Not Found
```json
{
  "success": false,
  "message": "Role entity not found."
}
```
**HTTP Status**: 400  
**Cause**: The role assigned to the user could not be retrieved from the system.  
**Resolution**: Verify the role exists in the system and is properly configured.

---

### 400 Bad Request - No Database Assigned to Role
```json
{
  "success": false,
  "message": "No database assigned to role."
}
```
**HTTP Status**: 400  
**Cause**: The user's role has no database connection mapping configured.  
**Resolution**: Configure a database connection mapping for the user's role.

---

## Access Levels Explained

### Full Access
```json
{
  "access_level": "full",
  "required_filters": []
}
```
- User can access all columns and rows in the table
- No filters are required
- User can perform all operations (read, write, etc.)

### Filtered Access
```json
{
  "access_level": "filtered",
  "required_filters": [
    {
      "column": "DepartmentId",
      "filter_type": "id",
      "values": ["dept-101", "dept-102"]
    },
    {
      "column": "Status",
      "filter_type": "enum",
      "values": ["Active", "Pending"]
    }
  ]
}
```
- User can only access rows that match the specified filters
- Multiple filters are combined with AND logic
- `id` filters use runtime resolved values (e.g., user IDs)
- `enum` filters use predefined enumeration values

### Denied Access
```json
{
  "access_level": "denied",
  "required_filters": []
}
```
- User has no access to this table
- Queries should reject access or return empty results

---

## Filter Types

### ID Filter (`filter_type: "id"`)
Values are resolved at runtime based on user context (e.g., user ID, department ID).

**Example**:
```json
{
  "column": "UserId",
  "filter_type": "id",
  "values": ["user-001", "user-002", "user-003"]
}
```

**SQL Translation**:
```sql
WHERE UserId IN ('user-001', 'user-002', 'user-003')
```

### Enum Filter (`filter_type: "enum"`)
Values are predefined enumerations (e.g., status codes, categories).

**Example**:
```json
{
  "column": "Status",
  "filter_type": "enum",
  "values": ["Active", "Pending"]
}
```

**SQL Translation**:
```sql
WHERE Status IN ('Active', 'Pending')
```

---

## Use Cases

### Use Case 1: Complete Permissions View
**Scenario**: Admin dashboard needs to display all permissions a user has access to.

**Request**:
```json
{
  "userId": "admin-001"
}
```

**Response**:
```json
{
  "metadata": {
    "version": "1.0",
    "database": "CompanyDB",
    "description": "Role-Based Access Control (RBAC) Permissions Dictionary",
    "note": "Only columns with required filters (id, enum) are listed. Other columns are readable if the table is accessible."
  },
  "userLookupConfiguration": {
    "userTableName": "dbo.Users",
    "userIdColumn": "UserId",
    "emailColumn": "Email",
    "phoneColumn": "Phone"
  },
  "permissions": {
    "admin": {
      "dbo.Users": {
        "reason": "Admin has full access to all users",
        "access_level": "full",
        "required_filters": []
      },
      "dbo.Orders": {
        "reason": "Admin has full access to orders",
        "access_level": "full",
        "required_filters": []
      },
      "dbo.Reports": {
        "reason": "Admin can view reports for all departments",
        "access_level": "full",
        "required_filters": []
      }
    }
  }
}
```

### Use Case 2: Manager with Filtered Access
**Scenario**: Manager needs to see their data access restrictions.

**Request**:
```json
{
  "userId": "manager-005"
}
```

**Response**:
```json
{
  "metadata": { ... },
  "userLookupConfiguration": { ... },
  "permissions": {
    "manager": {
      "dbo.Users": {
        "reason": "Manager can only view users in their department",
        "access_level": "filtered",
        "required_filters": [
          {
            "column": "DepartmentId",
            "filter_type": "id",
            "values": ["dept-sales-101"]
          }
        ]
      },
      "dbo.Orders": {
        "reason": "Manager can view orders for their department with pending status",
        "access_level": "filtered",
        "required_filters": [
          {
            "column": "DepartmentId",
            "filter_type": "id",
            "values": ["dept-sales-101"]
          },
          {
            "column": "OrderStatus",
            "filter_type": "enum",
            "values": ["Pending", "Processing"]
          }
        ]
      },
      "dbo.SensitiveData": {
        "reason": "Access denied - not authorized",
        "access_level": "denied",
        "required_filters": []
      }
    }
  }
}
```

### Use Case 3: Data Validation Before Query Execution
**Scenario**: Application validates user permissions before executing a database query.

**Request**:
```json
{
  "userId": "user-123"
}
```

**Application Logic**:
```csharp
// Get runtime permissions
var runtime = await GetRuntime("user-123");

// User wants to query dbo.Orders
var requestedTable = "dbo.Orders";
var tablePerms = runtime.permissions[userRole][requestedTable];

if (tablePerms.access_level == "denied")
{
    return Unauthorized("Access denied");
}

// Build query with filters
var query = _dbContext.Orders.AsQueryable();

foreach (var filter in tablePerms.required_filters)
{
    if (filter.filter_type == "id")
    {
        query = query.Where(o => filter.values.Contains(o.DepartmentId));
    }
    else if (filter.filter_type == "enum")
    {
        query = query.Where(o => filter.values.Contains(o.OrderStatus));
    }
}

var results = await query.ToListAsync();
```

---

## Integration Guide

### Prerequisites
- User must exist in the system with `UserId`
- User must have an assigned role
- Role must have a database connection mapping configured
- Role permissions must be defined for tables

### Steps to Integrate

1. **Make API Call**
   ```csharp
   var client = new HttpClient();
   var request = new GetRuntimeRequest
   {
       UserId = "user-id-here"
   };
   
   var response = await client.PostAsJsonAsync(
       "https://your-api/api/agent/V1/prompt-engine/Get-Runtime",
       request
   );
   
   var runtime = await response.Content.ReadAsAsync<RuntimeResponse>();
   ```

2. **Validate Response**
   ```csharp
   if (!response.IsSuccessStatusCode)
   {
       // Handle error
       var errorContent = await response.Content.ReadAsStringAsync();
       throw new Exception($"API Error: {errorContent}");
   }
   ```

3. **Extract User Lookup Configuration**
   ```csharp
   var lookupConfig = runtime.userLookupConfiguration;
   if (lookupConfig != null)
   {
       var userTable = lookupConfig.userTableName;
       var userIdCol = lookupConfig.userIdColumn;
       // Use for external user lookups
   }
   ```

4. **Apply Permissions to Queries**
   ```csharp
   var userRole = runtime.permissions.Keys.First();
   var tablePermissions = runtime.permissions[userRole];
   
   foreach (var (tableName, perms) in tablePermissions)
   {
       if (perms.access_level == "denied")
           continue;
       
       if (perms.access_level == "filtered")
       {
           // Apply filters to queries
           foreach (var filter in perms.required_filters)
           {
               // Build WHERE clause from filter.column and filter.values
           }
       }
   }
   ```

### Example Complete Implementation (C#)

```csharp
public class PermissionService
{
    private readonly HttpClient _httpClient;
    
    public async Task<PermissionCheckResult> ValidateTableAccess(
        string userId, 
        string tableName)
    {
        // Get runtime
        var response = await _httpClient.PostAsJsonAsync(
            "https://api/agent/V1/prompt-engine/Get-Runtime",
            new { userId }
        );
        
        if (!response.IsSuccessStatusCode)
            return new PermissionCheckResult { Allowed = false, Reason = "API Error" };
        
        var runtime = await response.Content.ReadAsAsync<RuntimeResponse>();
        var userRole = runtime.permissions.Keys.First();
        
        if (!runtime.permissions[userRole].TryGetValue(tableName, out var tablePerms))
            return new PermissionCheckResult { Allowed = false, Reason = "Table not found in permissions" };
        
        if (tablePerms.access_level == "denied")
            return new PermissionCheckResult { Allowed = false, Reason = "Access denied" };
        
        return new PermissionCheckResult 
        { 
            Allowed = true, 
            Filters = tablePerms.required_filters,
            Reason = tablePerms.reason
        };
    }
}
```

---

## Performance Considerations

- **Response Size**: Complete runtime includes all accessible tables - can be large for users with extensive permissions
- **Caching**: Consider caching the runtime response for frequently accessed users (with TTL)
- **Database Hits**: The endpoint queries multiple repositories:
  - User lookup (UserManager)
  - Role lookup (RoleManager)
  - Role-Connection mapping
  - Runtime permissions
  - Runtime view
  - Lookup configuration
  - Runtime context resolution (may involve external calls)
- **External Lookups**: If user lookup configurations involve external systems, latency may increase

---

## Difference: Get-Runtime vs Get-Table-Runtime

| Feature | Get-Runtime | Get-Table-Runtime |
|---------|-------------|-------------------|
| **Scope** | All tables | Specific tables only |
| **Response Size** | Large (all tables) | Smaller (selected tables) |
| **Use Case** | Complete permission view, caching | Targeted queries, specific features |
| **Performance** | More expensive | More efficient for targeted access |
| **Filtering** | N/A | Filters to requested tables only |

---

## Security Notes

- The endpoint is marked `AllowAnonymous` but access is strictly controlled by role assignments
- Permissions are resolved at runtime based on user's role
- External user lookups are validated before returning
- All filter values are validated before being returned
- Rate limiting is applied to prevent abuse

---

## Related Endpoints

- `[HttpPost("Get-Table-Runtime")]` - Get permissions for specific tables only
- `[HttpPost("Get-DB-Functions")]` - Get available database functions
- `[HttpPost("Get-DB-Function-ById")]` - Get specific function details

---

## Response Caching Strategy

### Recommended Caching Pattern
```csharp
private readonly IMemoryCache _cache;

public async Task<RuntimeResponse> GetRuntimeCached(string userId, TimeSpan cacheDuration)
{
    var cacheKey = $"runtime_{userId}";
    
    if (_cache.TryGetValue(cacheKey, out RuntimeResponse cached))
        return cached;
    
    var response = await GetRuntime(userId);
    _cache.Set(cacheKey, response, cacheDuration);
    
    return response;
}
```

### Cache Invalidation Triggers
- User role changes
- Role permissions updated
- Database connection reassigned
- User lookup configuration modified

---

## Last Updated
Generated: 2026-06-12

## Support
For issues or questions regarding this API, contact the development team or refer to the AgentController implementation.
