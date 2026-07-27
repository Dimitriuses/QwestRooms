# QwestRooms - Cleanup Roadmap

> **Working document.** This tracks the work of bringing a 2019 student project up to a
> standard worth showing an employer. Delete it before archiving the repo - the finished
> repo should speak for itself through its README, not through a list of its old flaws.

**Chosen track:** make it build, run and demo honestly on .NET Framework. No port to .NET 9.
**Estimated effort:** 2-4 focused days.
**Goal:** someone can clone, press F5, and see a working app - and the code they read on the
way through doesn't embarrass you.

---

## Baseline (measured 2026-07-27, before any changes)

| Check | Result |
| --- | --- |
| `nuget restore` | Passes, but surfaces **6 high-severity** CVEs |
| `msbuild` | **Fails.** v4.6.1 targeting pack is an empty stub; only v4.7.2 / v4.8 are installed |
| Runs from clean clone | **No** - no packages, no build, no database, no seed data |
| Tests | None |
| CI | None |
| README / LICENSE | None |

Restore emitted more advisories than GitHub's Dependabot has open PRs for:

| Package | Baseline | Severity |
| --- | --- | --- |
| `Newtonsoft.Json` | 6.0.4 | **High** |
| `Microsoft.Owin` | 4.0.1 | **High** (x2) |
| `Microsoft.Owin.Security.Cookies` | 3.0.1 | **High** |
| `Microsoft.AspNet.Identity.Owin` | 2.2.2 | **High** |
| `jQuery.Validation` | 1.11.1 | **High** |
| `jQuery` | 3.4.1 | Moderate (x2) |
| `bootstrap` | 3.0.0 | Moderate (x6) - *cleared in Phase 1.6* |

---

## Phase 1 - Make it build and run -- COMPLETE

Nothing else matters until this is true. Everything here was a hard blocker.

**Verified 2026-07-27:** solution builds clean; the app serves 27 room cards from a seeded
database (1000 rooms / 1000 addresses / 100 companies / 201 images); paging works across pages
1-38; the country -> city -> address filter narrows 27 rooms to 24; every CSS/JS asset is
served locally.

- [x] **1.1 Retarget 4.6.1 -> 4.8.** Required, not cosmetic: the 4.6.1 targeting pack no longer
      ships, so the solution did not compile on a current machine at all. Touched all three
      `.csproj` files, `Web.config`, and the `targetFramework` attributes in both
      `packages.config` files. 4.8 is in-support and preinstalled on Windows 10/11.
- [x] **1.2 Fix the dead root URL.** `RouteConfig` defaulted to a `HomeController` that does not
      exist, so `/` 404'd. Default route now points at `Room/Index`; the `_Layout` brand link
      pointed at the same missing controller and was repointed too.
- [x] **1.3 Fix the partial-view model mismatch.** `RoomsCollectionView.cshtml` declares
      `@model IndexViewModel`, but `GetRoomsByPage` and all three `Filter` branches passed a
      `List<RoomDTO>` - a runtime exception on both headline features.
      *Deviation from the original plan:* rather than splitting the partial in two, each action
      now passes the `IndexViewModel` it was already building and then discarding. Smaller change,
      and it fixes a second bug for free - the pager used to vanish after filtering.
- [x] **1.4 Make the database reproducible.** Seeding was commented out in `RoomsContext` and
      there were no migrations, so a clone got an empty schema. Rewrote the initializer
      (`RoomsDbInitializer`), fixed its output-path resolution, and registered it from a static
      constructor so it runs once per AppDomain. Switched from `DropCreateDatabaseAlways` to
      `DropCreateDatabaseIfModelChanges` so demo data survives a restart. Verified row counts
      against LocalDB. Also deleted `MockData/Company.sql`, a stale duplicate targeting a
      `Company` table that does not exist.
- [x] **1.5 Fix the registration crash.** `AcountController`'s constructor called
      `HttpContext.GetOwinContext()`, where `Controller.HttpContext` is still null. Removed it;
      the lazy property below already did the right thing.
- [x] **1.6 Repair the front end.** Removed every external dependency: jQuery 3.2.1 *slim* from a
      CDN (no `$.ajax`, which unobtrusive-ajax needs) loaded alongside local jQuery 3.4.1, a
      Bootstrap Material Design theme from unpkg, and a plugin from `cdn.rawgit.com` - shut down
      in 2019 and 404ing ever since. Upgraded local Bootstrap 3.0.0 -> 4.6.2 to match the
      Bootstrap 4 markup every view already used, rewrote the navbar in BS4 classes, and dropped
      the now-unused Modernizr package and glyphicon fonts.

**Found and fixed while verifying** (not in the original plan):

- `Ajax.ActionLink("", ...)` in the pager threw *"Value cannot be null or empty. Parameter name:
  linkText"* on every page after the first. It was masked before, because the model mismatch in
  1.3 threw earlier. The prev/next markup also nested a whole `<a>` element inside an `href`
  attribute. Pager rewritten.
- The page-number loop was `i < TotalPages`, so the last page was never linked.

---

## Phase 2 - Security and dependency hygiene -- COMPLETE

**Verified 2026-07-27:** `nuget restore` reports **zero** advisories; clean rebuild with **zero**
warnings; full auth cycle (register -> auto sign-in -> log out -> log in -> wrong password
rejected) passes against LocalDB with correctly hashed passwords; catalogue endpoints unaffected.

- [x] **2.1 Clear every advisory.** Bootstrap's six went in Phase 1. Upgraded
      `Newtonsoft.Json` 6.0.4 -> 13.0.4, the whole `Microsoft.Owin` family 4.0.1/3.0.1 -> 4.2.3,
      the `Microsoft.AspNet.Identity` family 2.2.2 -> 2.2.4, `jQuery` 3.4.1 -> 3.7.1 and
      `jQuery.Validation` 1.11.1 -> 1.21.0. Restore is now clean. This supersedes all four open
      Dependabot PRs - close them rather than merging, since they target versions we have passed.
- [x] **2.2 Turn off debug compilation.** `debug="false"` in `Web.config`.
- [x] **2.3 Harden registration.** Added `[ValidateAntiForgeryToken]` (verified: a POST without a
      token is rejected), a `ModelState.IsValid` guard, `[Required]` / `[EmailAddress]` /
      `[DataType(DataType.Password)]` / `[StringLength]` annotations, a confirm-password field
      with `[Compare]`, and `IdentityResult.Errors` surfaced into the validation summary instead
      of being discarded. The password renders masked now, not as plain text.
- [x] **2.4 Finish auth** (chosen over removing it). Added `Login` GET/POST, `Logout` POST, and a
      `Login` view; put the previously-declared-but-unused `AuthManager` to work; made the
      controller `[Authorize]` by default with `[AllowAnonymous]` on the public actions; and
      added log in / register / log out links to the navbar so the flow is discoverable. Deleted
      the `Index` action that returned a view which was never created. `LoginPath` now points at
      the real path.
- [x] **2.5 Scrub config leftovers.** Commented-out connection strings carrying the old machine
      name removed from `Web.config` and `App.config`.

**Found and fixed while verifying** (not in the original plan):

- Upgrading OWIN produced MSB3247 assembly conflicts, because `Microsoft.AspNet.Identity.Owin`
  2.2.4 still references the 3.0.1 security assemblies. Added binding redirects for
  `Microsoft.Owin.Security`, `.Security.Cookies` and `.Security.OAuth`, plus one for
  `Newtonsoft.Json`, which had none at all despite the 6.x -> 13.x jump.
- jQuery's upgrade renames the physical files, so `_Layout` and the `.csproj` both needed
  updating - easy to miss, and it would have 404'd every script on the page.
- Guarded the login `returnUrl` with `Url.IsLocalUrl` to avoid an open redirect, and added a
  `Dispose` override for the user manager.

**Carried forward:** `LoginPath` is `/Acount/Login` because the controller really is spelled
that way. Phase 3.7 renames it; that path must move at the same time.

---

## Phase 3 - The code an interviewer will actually read -- COMPLETE

**Verified 2026-07-27:** clean build, zero warnings, zero advisories. The database rebuilt itself
against the renamed schema and reseeded (1000 rooms / 1000 addresses / 201 images). A full
27-room page render now costs **2 SQL statements** (one COUNT, one projection SELECT), measured
from a cleared plan cache. Filtering and paging compose. Auth still works end to end after the
controller rename, and `/Acount/` is a 404.

- [x] **3.1 Stop loading whole tables into memory.** `GenericRepository.GetAll()` now returns
      `IQueryable<T>` instead of `AsEnumerable()`, so filtering, ordering and paging are composed
      into SQL. The room list used to read all 1000 rooms to display 27; it now reads 27.
- [x] **3.2 Kill the N+1.** Replaced lazy-loaded per-row mapping with a projection, so company,
      address and images arrive in the same statement. Measured: **2 statements per page**, down
      from roughly eighty.
- [x] **3.3 Move filtering out of the controller.** The ~70 lines of nested `foreach` across three
      near-identical branches are now one `ApplyFilter` method in `RoomsService`, evaluated in
      SQL. `GetAllCountry`'s O(n^2) HashSet dedupe is a `SELECT DISTINCT`.
- [x] **3.4 Get filter state out of `Session`.** Criteria bind from the query string into
      `RoomFilterViewModel` and are carried on the view model, so pager links inside a filtered
      list keep the filter. `Session` is gone from the codebase entirely.
- [x] **3.5 Replace hand-written mapping.** Done with reusable projection expressions rather than
      AutoMapper -- see the deviation note below.
- [x] **3.6 Delete the dead weight.** Removed `TestController`, `Views/Test/`, the orphaned
      `Views/Room/Index.cshtml`, `ICitiesService`/`CitiesService` (which existed only to serve
      `TestController`), the UI-side `CityViewModel`/`CountryViewModel` duplicates of the DTOs,
      and the commented-out blocks throughout.
- [x] **3.7 Fix the typos in public identifiers.** `Adress`->`Address`, `Acount`->`Account`,
      `Diffictly`->`Difficulty`, `MinPayers`->`MinPlayers`, `CountryVievModel`->`CountryViewModel`,
      `Colection`/`Viev`->`Collection`/`View`, `GetAllCitiesByCouyntries`->`CitiesPartial`. Also
      moved the repository out of the stray `DataAccessLayer.Repositories` namespace into
      `QwestRooms.DAL.Repositories`, and renamed the seed scripts from `Cities (fix).sql` to
      `Cities.sql`. All renames used `git mv`, so history is preserved.
      **`Qwest` is kept deliberately** -- it is the GitHub repository name, and changing it would
      break the URL. Phase 5.1 should say so in the README so it does not read as a typo.
- [x] **3.8 Settle on one language.** Code, comments and UI strings are English throughout. No
      resource files: with a handful of literals, a full localisation setup would be ceremony
      without benefit.
- [x] **3.9 Added `.editorconfig`.**

**Deviation - AutoMapper was not used.** The plan said to wire it up. Every AutoMapper release
compatible with .NET Framework 4.8 carries an unpatched high-severity advisory
(GHSA-rvv3-g6hj-g44x, DoS via uncontrolled recursion), fixed only in 15.1.1, which requires
.NET 8. Versions 11 and 12 target `netstandard2.1`, which .NET Framework cannot consume at all.
Adding it would have undone Phase 2's zero-advisory result to save a few lines. Instead,
`BLL/Mapping/Projections.cs` holds the entity-to-DTO mappings as reusable
`Expression<Func<TEntity, TDto>>` fields, applied with `Select`. That achieves what 3.5 actually
wanted -- one definition per DTO instead of a block copy-pasted into every service -- and it is
what makes the single-statement projection above possible. The one wart is that the address
initialiser is repeated inside the room projection, because an expression tree cannot invoke
another expression and remain translatable by EF6; composing them would need LINQKit, which is
not worth a dependency here. This is commented at the call site.

**Schema note:** renaming `Adress`, `Diffictly` and `MinPayers` changed EF's table and column
names, so the seed scripts had to change in lockstep. `DropCreateDatabaseIfModelChanges` picked
this up and rebuilt automatically on first request -- verified, including that the old
`Adresses` table is gone.

---

## Phase 4 - Evidence that you test and automate -- COMPLETE

**Verified 2026-07-27:** 34 tests, all passing in both Debug and Release. The CI command sequence
was run locally end to end (Release build -> vstest -> .trx artifact) so the workflow is not
guesswork.

- [x] **4.1 Added `QwestRooms.Tests`** (xUnit + Moq, SDK-style project targeting net48, so it uses
      PackageReference while the older projects keep packages.config -- `nuget restore` handles
      both). 34 tests across three fixtures:
      - `PageViewModelTests` - the boundary arithmetic that shipped broken. The page-number loop
        was `i < TotalPages`, so the last page was unreachable.
      - `RoomsServiceTests` - filtering by country, by city, the rule that a specific address
        outranks both, that `TotalCount` counts all matches rather than the current page, page
        clamping, and that the projection maps the renamed columns and nested address correctly.
      - `AddressesServiceTests` - that countries and cities are each offered once, which is the
        de-duplication the original code got wrong.

      The service tests run the **real projection expressions** over an in-memory `IQueryable`, so
      the mapping is exercised, not just the filter.

      **These tests were mutation-checked.** Reintroducing the original bugs -- changing the pager
      `Ceiling` back to `Floor`, and removing the city `Distinct()` -- made 4 and 1 tests fail
      respectively, with the city test reporting exactly the historical symptom (3 cities offered
      instead of 2). Both mutations were reverted and the suite is green. A test suite that has
      never been seen to fail is not evidence of anything.
- [x] **4.2 Added GitHub Actions CI** (`.github/workflows/build.yml`) - `windows-latest`,
      `nuget restore`, `msbuild /p:Configuration=Release`, then `vstest.console.exe` located via
      `vswhere`, with NuGet package caching and the `.trx` results uploaded as an artifact.

**Still to do in Phase 5:** the build badge, which needs the README that does not exist yet:
`![build](https://github.com/Dimitriuses/QwestRooms/actions/workflows/build.yml/badge.svg)`.
The workflow also triggers on the `cleanup` branch; drop that from the trigger list once this work
merges to `master`.

---

## Phase 5 - Presentation

The README is what most reviewers will actually read. Budget real time for it.

- [ ] **5.1 Write the README.** What the project is, a screenshot or GIF of the working filter,
      the three-layer architecture and why, the stack, and honest setup steps (prerequisites,
      restore, LocalDB, run). Open with one sentence framing it as a 2019 learning project,
      cleaned up in 2026 - that framing converts "dated code" into "shows self-assessment".
- [ ] **5.2 Fix the broken room logos before screenshotting.** Every room's `LogoPath` in the seed
      data is a hotlink to a real 2019 escape-room website. Many are already dead (one host,
      `*.netdna-ssl.com`, is fully decommissioned), so the card grid will screenshot with broken
      images. Either add an `onerror` fallback to a bundled placeholder image, or rewrite the seed
      `LogoPath` values to local placeholders. This is the first thing a reviewer sees.
- [ ] **5.3 Add a LICENSE** (MIT is the default choice).
- [ ] **5.4 Note the known limitations** in the README rather than leaving a reviewer to find
      them. Naming what you'd do differently is a strength signal, not a weakness.
- [ ] **5.5 Tag a release** (`v1.0`) so the history has a defined endpoint.
- [ ] **5.6 Delete this file,** then archive the repo on GitHub.

---

## Deliberately not doing

- **Porting to .NET 9 / ASP.NET Core.** It's the highest-signal option, but it's effectively a
  rewrite of the UI and DAL for 1-3 weeks. If you want a modern-.NET showcase, building something
  new is a better use of that time than porting a quest-room catalogue.
- **Rewriting the front end.** Bootstrap 4 with jQuery is period-appropriate for 2019 and honest
  about what the project is.
- **Rewriting git history.** The messages are poor (`asd`, `emm fix bags`), but rewriting them
  fakes a history that didn't happen. A good README covers it.
