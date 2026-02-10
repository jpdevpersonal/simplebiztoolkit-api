# C# API Requirements for Simple Biz Toolkit CMS

This document specifies the exact API endpoints your C# backend must implement to integrate with the Next.js frontend.

---

## Base URL

```
https://www.simplebiztoolkit.com
```

Set this in your Next.js `.env.local`:

```env
NEXT_PUBLIC_API_URL=https://www.simplebiztoolkit.com
```

---

## CORS Configuration

Your C# API must allow requests from your Next.js domain:

```csharp
app.UseCors(policy => policy
    .WithOrigins("https://www.simplebiztoolkit.com", "http://localhost:3000")
    .AllowAnyMethod()
    .AllowAnyHeader()
);
```

---

## Authentication

### Endpoint: `POST /api/auth/login`

**Request**:

```json
{
  "email": "admin@example.com",
  "password": "password123"
}
```

**Response**:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "user-id",
    "email": "admin@example.com",
    "name": "Admin User"
  }
}
```

---

## Articles API

### 1. Get Published Articles

```http
GET /api/articles?status=published
```

**Response**:

```json
{
  "data": [
    {
      "id": "uuid",
      "slug": "bookkeeping-made-simple",
      "title": "Bookkeeping made simple",
      "subtitle": "A simple workflow...",
      "description": "A simple workflow for tracking...",
      "content": "<section data-component=\"section\">...</section>",
      "dateISO": "2026-01-01",
      "dateModified": "2026-01-05",
      "category": "Bookkeeping",
      "readingMinutes": 6,
      "badges": ["Bookkeeping", "Small Business"],
      "featuredImage": "/images/article.webp",
      "headerImage": "/images/article.webp",
      "status": "published",
      "seoTitle": "Bookkeeping made simple | Guide",
      "seoDescription": "Learn bookkeeping...",
      "ogImage": "/images/article.webp",
      "canonicalUrl": "/blog/bookkeeping-made-simple"
    }
  ]
}
```

### 2. Get All Articles (Admin)

```http
GET /api/articles
Authorization: Bearer {token}
```

**Response**: Same as above, but includes drafts

### 3. Get Article by Slug

```http
GET /api/articles/slug/{slug}
```

**Example**: `/api/articles/slug/bookkeeping-made-simple`

**Response**:

```json
{
  "data": {
    "id": "uuid",
    "slug": "bookkeeping-made-simple",
    "title": "Bookkeeping made simple",
    ...
  }
}
```

### 4. Get Article by ID (Admin)

```http
GET /api/articles/{id}
Authorization: Bearer {token}
```

### 5. Create Article (Admin)

```http
POST /api/articles
Authorization: Bearer {token}
Content-Type: application/json
```

**Request**:

```json
{
  "slug": "new-article",
  "title": "New Article Title",
  "subtitle": "Subtitle here",
  "description": "Brief description",
  "content": "<section data-component=\"section\">Content here</section>",
  "category": "Bookkeeping",
  "readingMinutes": 5,
  "badges": ["Tag1", "Tag2"],
  "featuredImage": "/images/new.webp",
  "headerImage": "/images/new.webp",
  "status": "draft"
}
```

**Response**:

```json
{
  "data": {
    "id": "new-uuid",
    "slug": "new-article",
    ...
  }
}
```

**After creating/updating**, trigger revalidation:

```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-Revalidation-Secret", revalidationSecret);
await client.PostAsJsonAsync(
    "https://www.simplebiztoolkit.com/api/revalidate",
    new { type = "article", slug = "new-article" }
);
```

### 6. Update Article (Admin)

```http
PUT /api/articles/{id}
Authorization: Bearer {token}
Content-Type: application/json
```

**Request**: Same as Create

**Response**:

```json
{
  "data": { ... }
}
```

### 7. Delete Article (Admin)

```http
DELETE /api/articles/{id}
Authorization: Bearer {token}
```

**Response**:

```json
{
  "success": true
}
```

---

## Products API

### 1. Get All Categories with Products

```http
GET /api/products/categories
```

**Response**:

```json
{
  "data": [
    {
      "id": "cat-uuid",
      "slug": "accounting-ledger",
      "name": "Accounting Ledger",
      "summary": "Simple printable ledgers...",
      "howThisHelps": "Keeps your finances organized...",
      "heroImage": "/images/hero.webp",
      "items": [
        {
          "id": "prod-uuid",
          "title": "Printable Monthly Accounting Ledger",
          "slug": "monthly-accounting-ledger",
          "problem": "You need a clean way to track...",
          "description": "This monthly accounting ledger...",
          "bullets": ["General ledger layout", "Print anytime"],
          "image": "/images/product.webp",
          "etsyUrl": "https://etsy.com/listing/123",
          "price": "$1.93",
          "categoryId": "cat-uuid",
          "status": "published"
        }
      ]
    }
  ]
}
```

### 2. Get Category by Slug

```http
GET /api/products/categories/slug/{slug}
```

**Example**: `/api/products/categories/slug/accounting-ledger`

**Response**:

```json
{
  "data": {
    "id": "cat-uuid",
    "slug": "accounting-ledger",
    "name": "Accounting Ledger",
    "summary": "...",
    "howThisHelps": "...",
    "heroImage": "/images/hero.webp",
    "items": [...]
  }
}
```

### 3. Get Product by Slug

```http
GET /api/products/slug/{categorySlug}/{productSlug}
```

**Example**: `/api/products/slug/accounting-ledger/monthly-accounting-ledger`

**Response**:

```json
{
  "data": {
    "id": "prod-uuid",
    "title": "Printable Monthly Accounting Ledger",
    "slug": "monthly-accounting-ledger",
    "problem": "...",
    "description": "...",
    "bullets": ["..."],
    "image": "/images/product.webp",
    "etsyUrl": "https://etsy.com/listing/123",
    "price": "$1.93",
    "categoryId": "cat-uuid",
    "status": "published"
  }
}
```

### 4. Create Product (Admin)

```http
POST /api/products
Authorization: Bearer {token}
Content-Type: application/json
```

**Request**:

```json
{
  "title": "New Product",
  "slug": "new-product",
  "problem": "Problem statement",
  "description": "Full description",
  "bullets": ["Bullet 1", "Bullet 2"],
  "image": "/images/new.webp",
  "etsyUrl": "https://etsy.com/listing/456",
  "price": "$2.99",
  "categoryId": "cat-uuid",
  "status": "published"
}
```

### 5. Update Product (Admin)

```http
PUT /api/products/{id}
Authorization: Bearer {token}
```

### 6. Delete Product (Admin)

```http
DELETE /api/products/{id}
Authorization: Bearer {token}
```

---

## Categories API

### 1. Create Category (Admin)

```http
POST /api/products/categories
Authorization: Bearer {token}
Content-Type: application/json
```

**Request**:

```json
{
  "slug": "new-category",
  "name": "New Category",
  "summary": "Category summary",
  "howThisHelps": "How it helps users",
  "heroImage": "/images/hero.webp"
}
```

### 2. Update Category (Admin)

```http
PUT /api/products/categories/{id}
Authorization: Bearer {token}
```

---

## Error Handling

All endpoints should return errors in this format:

```json
{
  "error": "Error message here",
  "statusCode": 400
}
```

HTTP Status Codes:

- `200` - Success
- `400` - Bad Request (validation error)
- `401` - Unauthorized (missing/invalid token)
- `404` - Not Found
- `500` - Internal Server Error

---

## Database Indexes

For performance, create indexes on:

- `Articles.Slug` (unique)
- `Articles.Status`
- `Products.Slug`
- `Products.CategoryId`
- `Categories.Slug` (unique)

---

## Example C# Implementation (Minimal)

```csharp
[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ApiResponse<Article>>> GetBySlug(string slug)
    {
        var article = await _articleService.GetBySlugAsync(slug);

        if (article == null)
            return NotFound(new { error = "Article not found", statusCode = 404 });

        return Ok(new { data = article, statusCode = 200 });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<Article>>> Create([FromBody] CreateArticleDto dto)
    {
        var article = await _articleService.CreateAsync(dto);

        // Trigger Next.js revalidation
        await TriggerRevalidation("article", dto.Slug);

        return Ok(new { data = article, statusCode = 200 });
    }

    private async Task TriggerRevalidation(string type, string slug)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Revalidation-Secret", _config["RevalidationSecret"]);

        await client.PostAsJsonAsync(
            $"{_config["NextJsUrl"]}/api/revalidate",
            new { type, slug }
        );
    }
}
```

---

## Testing Your API

Use this checklist:

- [ ] Can fetch published articles without auth
- [ ] Can fetch article by slug
- [ ] Can create article with auth token
- [ ] Can update article with auth token
- [ ] 401 error when auth missing
- [ ] 404 error for non-existent slug
- [ ] Revalidation webhook called after updates
- [ ] CORS allows frontend domain
- [ ] All product endpoints working
- [ ] All category endpoints working

---

## Security Checklist

- [ ] JWT tokens validated
- [ ] Input sanitized (prevent SQL injection)
- [ ] HTML content sanitized (prevent XSS)
- [ ] Rate limiting enabled
- [ ] HTTPS required in production
- [ ] Secrets in environment variables
- [ ] CORS restricted to your domain

---

Done! Your C# API implementing these endpoints will integrate seamlessly with the Next.js frontend.
