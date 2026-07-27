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

## Phase 3 - The code an interviewer will actually read

This is the phase that changes their opinion of you. Prioritise depth over breadth: a reviewer
reading `RoomController` and `GenericRepository` forms their judgement in about ninety seconds.

- [ ] **3.1 Stop loading whole tables into memory.** `GenericRepository.GetAll()` returns
      `AsEnumerable()`, so every filter, sort and page runs client-side after a full table load -
      `Index` fetches *every* room and *every* address to show 27 of them. Return `IQueryable<T>`
      and let the provider compose the query.
- [ ] **3.2 Kill the N+1.** Lazy-loaded `virtual` navigations plus per-row mapping means a query
      per room for `Company`, `Adress` and `Images`. Add explicit `Include`s / projection.
- [ ] **3.3 Move filtering out of the controller.** `RoomController.Filter` is ~70 lines of nested
      `foreach` across three near-identical branches, and `GetAllCountry` does an O(n^2) dedupe
      using a `HashSet` as if it were a list. This logic is the entire reason the BLL project
      exists - move it there behind a single filter method taking a criteria object.
- [ ] **3.4 Get filter state out of `Session`.** Session-held filters break across tabs, can't be
      shared as a URL, don't survive the back button, and can't combine with paging. Pass criteria
      as query-string parameters bound to a model.
- [ ] **3.5 Replace hand-written mapping.** The same DTO-construction block is copy-pasted through
      every service. Wire up AutoMapper - it's already imported and commented out.
- [ ] **3.6 Delete the dead weight.** `TestController` and `Views/Test/`, the orphaned
      `Views/Room/Index.cshtml`, and the large commented-out blocks in nearly every file.
- [ ] **3.7 Fix the typos in public identifiers.** `Adress`, `Acount`, `Diffictly`, `MinPayers`,
      `CountryVievModel`, `Colection`, `Viev`, `GetAllCitiesByCouyntries`, `listFiltredRooms`.
      These are the single most visible signal in the repo and a pure find-and-replace to fix.
      Decide on `Qwest` vs `Quest` too - if the pun is deliberate, say so in the README so it
      doesn't read as another misspelling.
- [ ] **3.8 Settle on one language.** Comments and hardcoded UI strings mix Ukrainian and Russian
      with English identifiers. Pick English for code and comments; keep Ukrainian UI copy if you
      like, but move it into resource files rather than inline literals.
- [ ] **3.9 Add an `.editorconfig`** so the formatting stays consistent from here on.

---

## Phase 4 - Evidence that you test and automate

Small but disproportionately persuasive. Two or three real tests beat an empty test project.

- [ ] **4.1 Add a test project** (xUnit or NUnit + Moq) covering the logic worth trusting:
      `PageViewModel` boundary arithmetic - which had a real off-by-one that shipped - and the
      Phase 3.3 filter service against a mocked repository.
- [ ] **4.2 Add GitHub Actions CI** - `windows-latest`, `nuget restore`, `msbuild`, `vstest`, with
      the build badge in the README. This is what makes the repo look maintained rather than
      abandoned.

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
