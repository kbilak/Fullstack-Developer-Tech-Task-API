# Store Visits Tracking API

A REST API for tracking customer visits in stores. Built for the Fullstack Developer Tech Task.

## What's This About?

A store owner wants to track how many people visit his shops so he can optimize staffing and reduce checkout queues. This API handles the data collection side - managing stores and recording customer entries.

## Quick Start

```bash
cd API
dotnet build
dotnet run --project API
```

Open http://localhost:5113 for Swagger UI. The database gets created and seeded automatically on first run.

**Requirements:** .NET 9 SDK

## Tech Choices

- **ASP.NET Core 9** - latest version, solid framework
- **Entity Framework Core 9** - task mentioned EF as nice-to-have, so here it is
- **SQLite** - zero config, just works. Can swap to SQL Server by changing one line
- **xUnit** - standard .NET testing framework, 79 tests total
- **Built-in DI** - task mentioned Unity, but ASP.NET Core's DI does the same job without extra dependencies

## Required Endpoints (from spec)

All endpoints match the specification. Responses include `status` (bool) and `message` (string) as required.

| Endpoint | What it does |
|----------|--------------|
| `POST /api/Store` | Create a store |
| `GET /api/Store/{id}` | Get store details |
| `PUT /api/Store/{id}` | Update a store |
| `DELETE /api/Store/{id}` | Delete a store |
| `POST /api/Entry` | Record a customer visit* |
| `GET /api/Store/statistics/{id}?startDate=&endDate=` | Get visit stats for a store |

### One Small Change

The spec had `POST /api/store/entry` but I used `POST /api/Entry` instead. Same request/response, just a different URL. Why? Entries felt like their own resource, and having a separate controller makes the code cleaner and easier to test. Happy to discuss this choice.

## Extra Features

Since the spec encouraged improvements, I added a few things:

### More Ways to Query Entries

```
GET /api/Entry                              - list all entries (paginated)
GET /api/Entry/store/{storeId}              - entries for one store
GET /api/Entry/date/{date}                  - entries from a specific day
GET /api/Entry/store/{storeId}/date/{date}  - combine both filters
GET /api/Entry/date?startDate=&endDate=     - date range query
```

### Entry Management

The spec only required adding entries. I added full CRUD:

```
PUT /api/Entry/{id}      - update entry date
DELETE /api/Entry/{id}   - delete single entry
DELETE /api/Entry/bulk   - delete multiple entries at once
```

### Store Listing with Search & Sort

```
GET /api/Store?page=1&pageSize=10&sort=name:asc&search=warsaw
```

Supports pagination, sorting by name/entry count, and text search.

### Bulk Operations

```
DELETE /api/Store/bulk   - delete multiple stores
DELETE /api/Entry/bulk   - delete multiple entries
```

Both accept an array of IDs in the body: `[1, 2, 3]`

### Cross-Store Statistics

```
GET /api/Entry/statistics?startDate=&endDate=
```

Returns daily counts and per-store breakdown - useful for comparing stores.

### Other Additions

- **Pagination metadata** - all list endpoints return `totalItems`, `totalPages`, etc.
- **Database indexes** - added indexes on frequently queried columns (dates, store names)
- **Sample data** - first run seeds 27 stores and ~2700 entries from the last 90 days
- **Date validation** - returns 400 if date range is invalid

## Project Structure

```
API/
├── API/           # Controllers, Program.cs
├── Data/          # EF context, repositories, DTOs
└── Tests/         # xUnit tests (controllers + repositories)
```

Went with a simple layered architecture + repository pattern. For 2 entities and ~15 endpoints, anything fancier would be overkill.

## Running Tests

```bash
dotnet test
```

79 tests covering both repository logic and HTTP endpoints. Tests use SQLite in-memory (not EF's InMemory provider) because it behaves more like a real database.

## Switching to SQL Server

1. Update connection string in `appsettings.json`
2. Change `UseSqlite(...)` to `UseSqlServer(...)` in `Program.cs`

## AI Usage

The task mentioned AI is welcome, so here's what I did myself vs where AI helped:

**My work:**
- Overall architecture decisions and project structure
- Core business logic (controllers, repositories, DTOs)
- Database schema and EF configuration
- Deciding which extra features to add and how
- REST API design and endpoint structure
- Choosing tech stack and NuGet packages
- Repository pattern implementation
- Error handling and HTTP status codes (CreatedAtAction, NotFound, Ok)
- Validation logic and edge cases (DTO validation, Date range validation, ModelState check)
- Dependency injection setup
- Debugging and fixing issues 

**AI assisted with:**
- XML comments and Swagger documentation
- Generating sample seed data (27 stores, random entries)
- Filtering and sorting logic (pagination, search queries)
- Writing test cases
- Final code review, refactoring suggestions, and minor optimizations

I can walk through any part of the code and explain the reasoning behind it.

---
