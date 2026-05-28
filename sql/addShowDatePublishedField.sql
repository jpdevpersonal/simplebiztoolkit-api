# Prompt: Add `ShowLastUpdated` flag to MenuItemPage (C# API)

Repo: <https://github.com/jpdevpersonal/simplebiztoolkit-api>

## Context

The Next.js CMS now sends a new boolean field `showLastUpdated` on the
`MenuItemPage` payload when creating or updating a page. The frontend default
is `true` (the "Last updated DD MMM YYYY · By …" stamp shows on the public
page). When unchecked in the admin editor, the field is sent as `false` and
the public page must hide that stamp.

The flag must be **persisted in the database** and returned on every
`MenuItemPage` read (list, by id, by slug, admin list, admin by id) so the
admin editor can rehydrate the checkbox state.

## Required changes

Please make the following changes to the API, matching the existing patterns
used by sibling boolean/optional fields on `MenuItemPage` (e.g. status,
canonicalUrl):

1. **Entity / domain model**
   - Add a property `bool ShowLastUpdated { get; set; }` to the
     `MenuItemPage` EF entity.
   - Default value: `true`.

2. **DTOs**
   - Add `ShowLastUpdated` (camelCase `showLastUpdated` in JSON) to:
     - The response/read DTO returned by `GET /api/menuitempages`,
       `GET /api/menuitempages/{id}`, `GET /api/menuitempages/slug/{slug}`,
       `GET /api/admin/pages`, and `GET /api/admin/pages/{id}`.
     - The create DTO accepted by `POST /api/admin/pages` (and the public
       POST if one exists).
     - The update DTO accepted by `PUT /api/admin/pages/{id}`.
   - On create, if the caller omits the field, default it to `true`.
   - On update (partial / PATCH-style), only overwrite the stored value when
     the field is explicitly present in the request body. If you use a
     `Partial<>`-style model with nullable booleans or a JSON-merge approach,
     follow whatever convention the codebase already uses for optional fields
     like `seoTitle` / `canonicalUrl`.

3. **AutoMapper / mapping profiles**
   - Add `ShowLastUpdated` to the entity ⇄ DTO maps in both directions.

4. **EF Core migration**
   - Generate a migration that adds a `ShowLastUpdated` column of type
     `bit` (SQL Server) / `boolean` (Postgres) to the `MenuItemPages` table.
   - **NOT NULL**, default value **`true`** at the database level (so existing
     rows backfill to `true` and the existing behaviour is preserved).
   - Name the migration something like `AddShowLastUpdatedToMenuItemPages`.

5. **Validation**
   - No special validation needed — it is a plain boolean.

6. **Unit / integration tests**
   - Update any existing `MenuItemPage` controller / service tests that
     assert on the full DTO shape to include `ShowLastUpdated`.
   - Add a test confirming:
     - On create with the field omitted, the saved entity has
       `ShowLastUpdated = true`.
     - On update with `showLastUpdated: false`, the persisted entity is
       updated to `false` and a subsequent GET returns `false`.
     - On update where the field is omitted entirely (partial update), the
       existing value is preserved.

7. **OpenAPI / Swagger**
   - If the API publishes a generated OpenAPI document, regenerate it so the
     new field appears on the request and response schemas for
     `MenuItemPage`.

## JSON contract (must match exactly)

The frontend sends and expects:

```json
{
  "id": "…",
  "slug": "…",
  "title": "…",
  "showLastUpdated": true
}
```

- JSON property name: **`showLastUpdated`** (camelCase).
- Type: `boolean`.
- Optional in inbound payloads, always present in outbound payloads.

## Acceptance criteria

- `GET /api/menuitempages/slug/{slug}` returns `showLastUpdated` for every
  page (existing rows return `true` by default).
- Creating a page via `POST /api/admin/pages` without the field stores
  `true`.
- Updating a page via `PUT /api/admin/pages/{id}` with
  `"showLastUpdated": false` stores `false` and the next GET returns `false`.
- All existing tests still pass; new tests cover the three cases above.
- Migration applies cleanly to a database with existing rows and backfills
  them to `true`.

## Out of scope

- No public API behaviour change beyond the field itself; the frontend is
  responsible for hiding the date stamp when `showLastUpdated === false`.
- No changes to caching, revalidation, or SEO/JSON-LD output.
