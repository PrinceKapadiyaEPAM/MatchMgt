# B2B Mobile API Reference

Base URL: `https://<host>/api/b2b`

All catalogue endpoints require a Bearer JWT token in the `Authorization` header.
Auth endpoints (`/auth/login`, `/auth/register`) are public.

---

## Authentication

### POST /api/b2b/auth/login

Authenticate and receive a JWT token.

**Request body**
```json
{
  "email": "user@example.com",
  "password": "yourpassword"
}
```

**200 OK**
```json
{
  "token": "<JWT>",
  "expiresAt": "2026-06-12T10:00:00Z",
  "userType": "PartyUser",
  "fullName": "John Doe"
}
```
`userType` is `"PartyUser"` when the account is linked to a Party, `"GuestUser"` otherwise.

**Error responses**

| Status | `error` message |
|--------|----------------|
| 400 | `"Email and password are required."` |
| 401 | `"Invalid email or password."` |
| 401 | `"Your account is pending approval. Please contact your administrator."` |
| 401 | `"Your account has been deactivated. Please contact your administrator."` |

---

### POST /api/b2b/auth/register

Self-register a new B2B account. Registered accounts start with `ApprovalStatus = "Pending"` and `IsActive = false`. An admin must approve the account before the user can log in.

**Request body**
```json
{
  "fullName": "John Doe",
  "email": "user@example.com",
  "password": "min8chars",
  "confirmPassword": "min8chars",
  "phone": "+91 98765 43210"
}
```
`phone` is optional.

**200 OK**
```json
{
  "message": "Registration successful. Your account is pending approval. Please contact your administrator."
}
```

**Error responses**

| Status | `error` message |
|--------|----------------|
| 400 | `"All required fields must be provided."` |
| 400 | `"Passwords do not match."` |
| 400 | `"Password must be at least 8 characters."` |
| 400 | `"This email address is already registered."` |

---

## Authorization Header

All catalogue endpoints require:

```
Authorization: Bearer <token>
```

The token is the value returned by `/auth/login`. Tokens expire after the configured `JwtSettings:ExpiryMinutes` (default 60 minutes).

---

## Catalogue

### GET /api/b2b/catalogue

List catalogue items with pagination and optional search.

**Query parameters**

| Parameter | Type | Default | Max | Description |
|-----------|------|---------|-----|-------------|
| `page` | int | 1 | — | Page number (1-based) |
| `pageSize` | int | 20 | 100 | Items per page |
| `search` | string | — | — | Case-insensitive name search |

**200 OK**
```json
{
  "page": 1,
  "pageSize": 20,
  "totalCount": 45,
  "totalPages": 3,
  "items": [
    {
      "id": 7,
      "name": "Design A",
      "fold": "40",
      "photoUrl": "/uploads/catalogues/photo_abc123.jpg",
      "price": 1250.00,
      "stockQty": 12,
      "stockStatus": "In Stock"
    }
  ]
}
```

**Field notes**
- `photoUrl` — `null` when no photo is uploaded; prefix with the host to build the full URL.
- `price` — `null` when the admin has not set a price, or when the user's `ShowPrices` access right is disabled.
- `stockQty` — `null` when `ShowStock` is disabled.
- `stockStatus` — one of `"In Stock"`, `"Out of Stock"`, `"Coming Soon — N day(s)"`, or `null` when `ShowStock` is disabled.
- Party-linked users receive their party's override price when one is configured; otherwise the default catalogue price is returned.

**403 Forbidden** — returned when the user's `ShowCatalogue` access right is disabled.

---

### GET /api/b2b/catalogue/{id}

Get full details of a single catalogue item.

**Path parameter**

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | int | Catalogue item ID |

**200 OK**
```json
{
  "id": 7,
  "name": "Design A",
  "fold": "40",
  "remark": "Premium quality cotton",
  "photoUrl": "/uploads/catalogues/photo_abc123.jpg",
  "pdfUrl": "/uploads/catalogues/doc_xyz789.pdf",
  "price": 1250.00,
  "stockQty": 12,
  "stockStatus": "In Stock",
  "restockDate": "2026-07-01"
}
```

**Field notes**
- `pdfUrl` — `null` when no PDF is attached.
- `restockDate` — ISO `yyyy-MM-dd` format; `null` when not set or when `ShowStock` is disabled.
- `price`, `stockQty`, `stockStatus` — same access-right rules as the list endpoint.

**Error responses**

| Status | Condition |
|--------|-----------|
| 403 | `ShowCatalogue` access right is disabled for this user |
| 404 | `{ "error": "Catalogue item not found." }` |

---

## Stock Status Values

| Value | Meaning |
|-------|---------|
| `"In Stock"` | Available bales > 0 |
| `"Out of Stock"` | No bales and no upcoming restock date |
| `"Coming Soon — N day(s)"` | Out of stock but restock date is set in the future |

Stock is calculated as:
```
Stock = Sum(InventoryTransactions of type Stock)
      - Sum(InventoryTransactions of type Order)
      - Sum(DispatchEntries.Bale where Status = "Ok" or "Pending")
```

---

## User Types & Pricing

| User Type | Price shown |
|-----------|-------------|
| `GuestUser` (no party) | Default catalogue price |
| `PartyUser` (linked to a party) | Party override price if set, otherwise default catalogue price |

---

## Access Rights

Access rights are configured per-user by an admin in the web app (B2B Users → Access Rights screen).

| Right | Effect when disabled |
|-------|---------------------|
| `ShowCatalogue` | All catalogue endpoints return `403 Forbidden` |
| `ShowPrices` | `price` field is `null` in all responses |
| `ShowStock` | `stockQty`, `stockStatus`, `restockDate` fields are `null` |

---

## JWT Configuration

Set in `appsettings.json` (or environment overrides):

```json
{
  "JwtSettings": {
    "SecretKey": "<min-32-char-secret>",
    "ExpiryMinutes": 60
  }
}
```

The token payload contains a single claim: `nameidentifier` = B2BUser ID (integer).

---

## Account Lifecycle

```
Register (/auth/register)
   └─► ApprovalStatus = "Pending", IsActive = false
          │
          ▼ Admin approves (web app: B2B Users → Approve)
       ApprovalStatus = "Approved", IsActive = true
          │
          ▼ Can now login (/auth/login)
       JWT token issued
          │
          ├─ Admin can deactivate → IsActive = false → 401 on next login
          └─ Admin can reject → ApprovalStatus = "Rejected" → 401 on next login
```
