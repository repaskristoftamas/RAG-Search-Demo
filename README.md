# BookStore RAG Search Demo

A microservices system comparing **keyword-based search** (PostgreSQL full-text search) against **semantic/RAG-based search** (pgvector + Ollama LLM) over a book catalog.

Built with .NET 9, Clean Architecture, DDD, Event-Driven Architecture, Docker.

**Note:** Clean Architecture, DDD, EDA, RabbitMQ, MassTransit and microservices are not strictly necessary, but they are part of my agent-based project template, which helps me quickly build the basic architecture for projects using AI agents.

## Architecture

```
                     +-----------------+
                     |   API Gateway   |  YARP reverse proxy + comparison endpoint
                     |   Port: 5000    |
                     +--------+--------+
                              |
           +------------------+------------------+
           |                  |                  |
    +------v------+   +-------v-------+   +------v-------+
    |   Catalog   |   |   Keyword     |   |   Semantic   |
    |   Service   |   |   Search      |   |   Search     |
    |  Port: 5010 |   |  Port: 5020   |   |  Port: 5030  |
    +------+------+   +-------+-------+   +------+-------+
           |                  |                  |
    publishes          subscribes          subscribes
           v                  v                  v
    +---------------------------------------------+
    |           RabbitMQ  (5672 / 15672)           |
    +---------------------------------------------+
           |                  |                  |
    +------v------+   +-------v-------+   +------v-------+
    | PostgreSQL  |   | PostgreSQL    |   | PostgreSQL   |
    | catalog_db  |   | keyword_db   |   | semantic_db  |
    | Port: 5433  |   | Port: 5434   |   | + pgvector   |
    +-------------+   +---------------+   | Port: 5435   |
                                          +------+-------+
                                                 |
                                          +------v-------+
                                          |    Ollama    |
                                          | embeddings   |
                                          | + LLM gen    |
                                          | Port: 11434  |
                                          +--------------+
```

**Communication:**

- **Sync**: Gateway proxies HTTP to services via YARP
- **Async**: Catalog publishes integration events to RabbitMQ via MassTransit; both search services consume independently (fanout)
- **Comparison**: Gateway calls both search services in parallel, merges results with timing

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- ~8 GB free RAM (Ollama models need ~4 GB)

## Quick Start

### 1. Start infrastructure and services

```powershell
docker compose -f deploy/docker-compose.yml up --build -d
```

### 2. Pull Ollama models

The semantic search service requires two Ollama models. Pull them into the running container:

```powershell
docker compose -f deploy/docker-compose.yml exec ollama ollama pull nomic-embed-text
docker compose -f deploy/docker-compose.yml exec ollama ollama pull llama3.2
```

### 3. Seed the catalog

```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/v1/books/seed" -ContentType "application/json" -InFile seed/books.json
```

Wait a few seconds for events to propagate through RabbitMQ to both search services.

### 4. Search and compare

**Keyword search** (PostgreSQL full-text search with tsvector/tsquery):

```powershell
Invoke-RestMethod "http://localhost:5000/api/v1/keyword-search/search?q=time%20travel%20philosophy&maxResults=5"
```

**Semantic search** (pgvector cosine similarity + optional LLM summary):

```powershell
Invoke-RestMethod "http://localhost:5000/api/v1/semantic-search/search?q=time%20travel%20philosophy&maxResults=5&includeLlmSummary=true"
```

**Side-by-side comparison** (calls both in parallel with timing):

```powershell
Invoke-RestMethod "http://localhost:5000/api/v1/compare?q=time%20travel%20philosophy&maxResults=5"
```

## API Endpoints

### Catalog Service (via Gateway)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/v1/books?page=1&pageSize=20` | Paginated list |
| `GET` | `/api/v1/books/{id}` | Get by ID |
| `POST` | `/api/v1/books` | Create book |
| `PUT` | `/api/v1/books/{id}` | Update book |
| `DELETE` | `/api/v1/books/{id}` | Delete book |
| `POST` | `/api/v1/books/seed` | Seed from JSON |

### Keyword Search (via Gateway)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/v1/keyword-search/search?q={query}&maxResults=5` | Full-text search |

### Semantic Search (via Gateway)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/v1/semantic-search/search?q={query}&maxResults=5&includeLlmSummary=false` | Vector search + optional RAG |

### Gateway

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/v1/compare?q={query}&maxResults=10` | Side-by-side comparison |

## How It Works

### Keyword Search

Uses PostgreSQL's built-in full-text search. A `tsvector` generated column combines title (weight A), author (weight B), and description (weight C) with a GIN index. Queries use `plainto_tsquery` with `ts_rank` for relevance scoring.

### Semantic Search (RAG)

1. **Embedding**: When a book is created/updated, the text `"{title} by {author}. {description}"` is sent to Ollama's `nomic-embed-text` model to generate a 768-dimension vector, stored via pgvector.
2. **Retrieval**: Query text is embedded the same way, then pgvector finds the top-K most similar books by cosine distance.
3. **Generation** (optional): The top-K results are passed as context to Ollama's `llama3.2` model, which generates a natural language summary answering the query.

### Event Flow

```
Book created in Catalog
  -> BookCreatedEvent (domain event, in-process)
  -> PublishIntegrationEventHandler bridges to MassTransit
  -> BookCreatedIntegrationEvent published to RabbitMQ
  -> BookCreatedConsumer in Keyword Search (inserts SearchableBook)
  -> BookCreatedConsumer in Semantic Search (generates embedding, inserts BookEmbedding)
```

## Project Structure

```
src/
  shared/
    BookStore.SharedKernel/        Result pattern, EntityBase, domain event interfaces
    BookStore.Contracts/           Integration event records (cross-service boundary)
  services/
    catalog/                       Domain -> Application -> Infrastructure -> Api
    keyword-search/                Application -> Infrastructure -> Api (read projection)
    semantic-search/               Application -> Infrastructure -> Api (read projection)
  gateway/
    BookStore.Gateway/             YARP reverse proxy + comparison endpoint
tests/                             xUnit + Shouldly + Moq
deploy/
  docker-compose.yml               Full stack
seed/
  books.json                       50 sample books
```

## Key Libraries

| Library | Purpose |
|---------|---------|
| [Mediator](https://github.com/martinothamar/Mediator) | Source-generated CQRS |
| [FluentValidation](https://docs.fluentvalidation.net/) | Request validation |
| [Riok.Mapperly](https://mapperly.riok.app/) | Source-generated mapping |
| [MassTransit](https://masstransit.io/) | Messaging over RabbitMQ |
| [Npgsql + EF Core](https://www.npgsql.org/efcore/) | PostgreSQL provider |
| [pgvector](https://github.com/pgvector/pgvector) | Vector similarity search |
| [OllamaSharp](https://github.com/awaescher/OllamaSharp) | Ollama .NET client |
| [YARP](https://microsoft.github.io/reverse-proxy/) | Reverse proxy |

## Monitoring

- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
