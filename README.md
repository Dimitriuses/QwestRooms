# QwestRooms

[![build](https://github.com/Dimitriuses/QwestRooms/actions/workflows/build.yml/badge.svg)](https://github.com/Dimitriuses/QwestRooms/actions/workflows/build.yml)

A catalogue of escape-quest rooms: a browsable, paged grid of 1000 rooms with a cascading
country → city → address filter, built on ASP.NET MVC 5 with a three-layer architecture.

> **About this repository.** I wrote this in 2019 while learning ASP.NET MVC and layered
> application design. In 2026 I came back to it, fixed what was broken, and documented it properly
> before archiving. The [What changed in 2026](#what-changed-in-2026) section is deliberately
> candid about what needed fixing — reading your own old code critically is part of the point.

![The room catalogue](docs/images/catalogue.png)

## Features

- **Paged catalogue** of 1000 seeded rooms, 27 per page, rendered as flip cards showing rating,
  fear level, difficulty on the front and players, duration, company and address on the back.
- **Cascading filter** — pick a country, then a city within it, then a specific address. Each
  level offers only values that are actually reachable given the previous choice.
- **Filtering and paging compose.** The criteria travel in the query string, so a filtered list
  can be paged through, linked to, bookmarked, and opened in two tabs with different filters.
- **Accounts** — registration, login and logout via ASP.NET Identity, with anti-forgery protection
  and server-side validation.

![The cascading filter](docs/images/filter.png)

## Stack

| | |
| --- | --- |
| Runtime | .NET Framework 4.8 |
| Web | ASP.NET MVC 5, Razor, jQuery, Bootstrap 4 |
| Data | Entity Framework 6, SQL Server LocalDB |
| Auth | ASP.NET Identity 2 over OWIN cookie authentication |
| DI | Autofac |
| Tests | xUnit, Moq |
| CI | GitHub Actions (windows-latest) |

## Architecture

Three layers, each a separate project, with dependencies pointing inward from the UI:

```
QwestRooms.UI  ──►  QwestRooms.BLL  ──►  QwestRooms.DAL
  controllers        services              EF entities
  Razor views        DTOs + projections     DbContext
  view models        filter criteria        generic repository
  Autofac wiring     paging                 seed data + initializer
```

- **`QwestRooms.DAL`** owns the entity model, `RoomsContext` (which also carries the Identity
  tables), and a generic repository that exposes `IQueryable<T>` so callers can compose queries
  that execute in the database rather than in memory.
- **`QwestRooms.BLL`** holds the business rules: what a filter means, how paging works, and the
  entity → DTO projections. The projections are `Expression<Func<TEntity, TDto>>` values applied
  with `Select`, so they become the SELECT list of a single SQL statement — rendering a full page
  of 27 rooms with their company, address and images costs **two queries**, one COUNT and one
  projection.
- **`QwestRooms.UI`** is thin by design: controllers bind query-string criteria to a view model,
  hand them to a service, and render. No business logic lives here.
- **`QwestRooms.Tests`** covers the paging arithmetic, the filter rules, and the de-duplication of
  filter options.

## Getting started

### Prerequisites

- Windows
- Visual Studio 2022 (or Build Tools) with the **.NET Framework 4.8 targeting pack**
- **SQL Server LocalDB** — installed with Visual Studio by default

### Run it

```powershell
git clone https://github.com/Dimitriuses/QwestRooms.git
cd QwestRooms

nuget restore QwestRooms.sln          # Visual Studio also restores on open
msbuild QwestRooms.sln /p:Configuration=Debug

# then press F5 in Visual Studio, or host QwestRooms.UI under IIS Express
```

There is no database setup step. On the first request the application creates
`QwestRooms.DAL.RoomsContext` in LocalDB and seeds it from the scripts in
`QwestRooms.DAL/MockData` — 50 countries, 100 cities, 1000 addresses, 100 companies, 1000 rooms.

To start over from clean data, drop the database and let the next request recreate it:

```powershell
sqlcmd -S "(LocalDb)\MSSQLLocalDB" -d master -Q "alter database [QwestRooms.DAL.RoomsContext] set single_user with rollback immediate; drop database [QwestRooms.DAL.RoomsContext];"
```

### Run the tests

From Visual Studio's Test Explorer, or:

```powershell
vstest.console.exe QwestRooms.Tests\bin\Debug\net48\QwestRooms.Tests.dll
```

## What changed in 2026

The project had not built since 2019 — the .NET Framework 4.6.1 targeting pack it needed is no
longer shipped, so it would not compile on a current machine at all. Beyond that:

- **It could not run.** The default route pointed at a controller that did not exist, so `/`
  returned 404. Paging and filtering both threw at runtime because the actions passed a
  `List<RoomDTO>` to a partial declaring a different model. The database was never seeded — the
  initializer was commented out and there were no migrations.
- **Every dependency was vulnerable.** Restoring reported twelve advisories, six of them high
  severity. All are now cleared.
- **Auth went nowhere.** Registration existed but crashed on every request, there was no login or
  logout, the password field rendered as plain text, and the anti-forgery token the form emitted
  was never validated.
- **Queries loaded whole tables.** The repository returned `AsEnumerable()`, so filtering, sorting
  and paging happened in memory after reading everything — the room list read all 1000 rooms to
  display 27, then issued a further query per room for its related data. That page now costs two
  queries.
- **Business logic lived in the controller** as ~70 lines of nested loops across three
  near-identical branches, with filter state kept in `Session` — which meant filters could not be
  combined with paging, shared as a URL, or used in two tabs at once.

Some of the most visible problems were small: `Adress`, `Acount`, `Diffictly` and `MinPayers` were
misspelled throughout the public API, and comments mixed three languages.

`Qwest` in the name is intentional — a play on "quest" — and is kept because it is the repository
name.

## Known limitations

These are deliberate, given the project's scope. I am listing them because a reviewer will find
them anyway:

- **The catalogue is read-only.** There is no create/edit/delete for rooms; the repository
  supports writes but nothing in the UI uses them.
- **Nothing is access-controlled.** Accounts work end to end, but no page requires being signed
  in, so authentication is demonstrated rather than used.
- **Room logos are 2019 hotlinks** to real escape-room websites, and roughly two thirds are now
  dead. A bundled placeholder is shown when an image fails to load, which is why the screenshots
  above have a lot of padlocks.
- **The seed data is generated, not real.** Room names and descriptions are lorem ipsum, and
  cities are not geographically consistent with their countries.
- **The schema is created by an EF initializer, not migrations.** Fine for a demo that reseeds
  from scratch; a real deployment would want migrations.
- **It targets .NET Framework 4.8, not .NET 8+.** Porting would mean rewriting the UI and data
  layers, which is a different project rather than a cleanup of this one.

## License

[MIT](LICENSE).
