# Planned Tests

## Unit Tests

### Shared Kernel (`BookStore.SharedKernel`)

| Component | What to test | Priority |
|---|---|---|
| `Result` / `Result<T>` | Success/failure state consistency, value/error access guards, implicit conversion | High |
| `PagedResult<T>` | `TotalPages` edge cases (0 items, partial last page), `HasNextPage`/`HasPreviousPage` | Medium |
| `Error` subtypes | Correct code/description on `NotFoundError`, `ConflictError`, `ValidationError` | Low |

### Catalog Domain (`BookStore.Catalog.Domain`)

| Component | What to test | Priority |
|---|---|---|
| `Book.Create()` | Valid inputs succeed + emit `BookCreatedEvent`; missing Title/Author/Description return correct error codes | **High** |
| `Book.Update()` | State mutation + `BookUpdatedEvent` emission; validation failures return errors without mutating state | **High** |
| `Book.Delete()` | Emits `BookDeletedEvent` | Medium |
| `EntityBase` | `AddDomainEvent` / `ClearDomainEvents` collection behavior | Medium |
| `BookId` | Equality, `New()` factory | Low |

### Catalog Application (`BookStore.Catalog.Application`)

| Component | What to test | Priority |
|---|---|---|
| `CreateBookCommandHandler` | Delegates to `Book.Create()`, adds to context, returns ID on success; returns error on domain failure | **High** |
| `UpdateBookCommandHandler` | Not-found path (returns `NotFoundError`), success path delegates to `Book.Update()` | **High** |
| `DeleteBookCommandHandler` | Not-found path, success path removes entity | High |
| `SeedBooksCommandHandler` | Duplicate title deduplication (case-insensitive), batch creation, returns correct count | High |
| `GetAllBooksQueryHandler` | Pagination math (offset calculation, page boundaries) | Medium |
| `GetBookByIdQueryHandler` | Not-found returns error, success returns mapped DTO | Medium |
| `CreateBookCommandValidator` / `UpdateBookCommandValidator` | MaxLength violations, required field violations, correct error codes | Medium |
| `BookMapper` | `BookId` to `Guid` conversion, all fields mapped | Low |
| `PublishIntegrationEventHandler` | Domain event to integration event translation (pattern match for each event type); book-deleted-before-publish resilience | **High** |

### Gateway (`BookStore.Gateway`)

| Component | What to test | Priority |
|---|---|---|
| `SearchServiceClient` | Response deserialization and field mapping (keyword: `RelevanceScore`, semantic: `SimilarityScore`); `HttpRequestException` propagation | High |

### Catalog API (`BookStore.Catalog.Api`)

| Component | What to test | Priority |
|---|---|---|
| `ErrorExtensions.ToProblemHttpResult()` | `NotFoundError`->404, `ConflictError`->409, `ValidationError`->400 with field failures, fallback->500 | Medium |

---

## Integration Tests

### Catalog Service

| Scenario | What it validates | Priority |
|---|---|---|
| Command handler -> DB round-trip | Create book, read it back, verify all fields persisted | **High** |
| Audit timestamps | `CreatedAt` set on insert, `UpdatedAt` set on update (via `TimeProvider` injection) | **High** |
| Domain event -> integration event pipeline | Create book -> `CatalogDbContext.SaveChangesAsync` dispatches `DomainEventNotification` -> `PublishIntegrationEventHandler` publishes to MassTransit | **High** |
| Seed deduplication with real DB | Seed same titles twice, verify no duplicates | Medium |
| Pagination with real data | Insert N books, verify page counts and ordering | Medium |

### Keyword-Search Service

| Scenario | What it validates | Priority |
|---|---|---|
| `BookCreatedConsumer` | Publish `BookCreatedIntegrationEvent` -> consumer creates `SearchableBook` in DB | **High** |
| `BookUpdatedConsumer` | Update existing record; upsert when record missing (eventual consistency) | **High** |
| `BookDeletedConsumer` | Removes record; idempotent on missing record | Medium |
| Full-text search ranking | Insert multiple books, query, verify `ts_rank` ordering and case-insensitivity | **High** |
| Search with no results | Query that matches nothing returns empty list | Medium |

### Semantic-Search Service

| Scenario | What it validates | Priority |
|---|---|---|
| `BookCreatedConsumer` | Event -> embedding generation (mocked) -> `BookEmbedding` persisted with vector | **High** |
| `BookUpdatedConsumer` | Upsert with re-embedding | High |
| `BookDeletedConsumer` | Removes embedding record | Medium |
| Vector similarity search | Insert known vectors, query with a near vector, verify cosine distance ordering and threshold filtering (`MinSimilarity = 0.5`) | **High** |
| LLM summary generation | Mock `ITextGenerationService`, verify prompt construction includes search results as context | Medium |

### Gateway

| Scenario | What it validates | Priority |
|---|---|---|
| Compare endpoint - parallel execution | Both services called, results merged, `TotalElapsedMs` roughly equals max of the two | High |
| Compare endpoint - downstream failure | One service returns error -> 502 with problem details | High |

### Cross-Service (E2E with TestContainers)

| Scenario | What it validates | Priority |
|---|---|---|
| Create book -> keyword index updated | Full pipeline: API call -> domain event -> RabbitMQ -> consumer -> read model | Medium |
| Create book -> semantic index updated | Same, but with mocked Ollama embedding | Medium |
| Update/Delete propagation | Changes flow through to both search indices | Medium |

---

## What NOT to Test

- **Mappers** generated by Mapperly (source-gen is compile-time verified)
- **EF Core configurations** in isolation (tested implicitly by integration tests)
- **DI registration** (`DependencyInjection.cs` files) — tested by app startup in integration tests
- **Program.cs** pipeline setup — tested by `WebApplicationFactory` in integration tests
- **YARP reverse proxy routing** — framework behavior, not custom logic
- **OllamaEmbeddingService / OllamaTextGenerationService** — thin SDK wrappers with no logic; mock the `IEmbeddingService`/`ITextGenerationService` interfaces instead

---

## Test Project Structure

```
tests/
├── BookStore.SharedKernel.UnitTests/
├── BookStore.Catalog.Domain.UnitTests/
├── BookStore.Catalog.Application.UnitTests/
├── BookStore.Catalog.Api.UnitTests/
├── BookStore.Catalog.IntegrationTests/             # WebApplicationFactory + Testcontainers PostgreSQL + RabbitMQ
├── BookStore.KeywordSearch.Application.UnitTests/   # If consumers get non-trivial logic
├── BookStore.KeywordSearch.IntegrationTests/        # Testcontainers PostgreSQL + MassTransit test harness
├── BookStore.SemanticSearch.Application.UnitTests/
├── BookStore.SemanticSearch.IntegrationTests/       # Testcontainers PostgreSQL+pgvector + MassTransit test harness
└── BookStore.Gateway.UnitTests/                     # SearchServiceClient + ErrorExtensions
```

---

## Recommended Starting Order

1. **Catalog Domain unit tests** — business invariants are the highest-value, lowest-cost tests
2. **Catalog Application unit tests** — command/query handler orchestration
3. **Catalog integration tests** — DB round-trips and audit timestamp automation
4. **Keyword-Search integration tests** — consumer projections and full-text search ranking
5. **Semantic-Search integration tests** — consumer projections and vector similarity search
6. **Gateway unit tests** — SearchServiceClient response mapping
7. **Shared Kernel unit tests** — Result pattern and pagination
8. **Cross-service E2E tests** — full event-driven pipeline
