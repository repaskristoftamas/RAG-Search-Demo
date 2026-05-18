# RAG Roadmap

## 1. Hybrid Search with Reciprocal Rank Fusion

The gateway already calls keyword and semantic search in parallel and presents them side-by-side.
The next step is **fusing** those two result sets into a single ranked list.

### What to build

- Implement Reciprocal Rank Fusion (RRF) in the comparison endpoint: for each document appearing in either result set, compute `score = sum(1 / (k + rank))` across both lists (k = 60 is the standard constant).
- Expose a new `/api/v1/search` unified endpoint on the gateway that returns the fused results as the default search experience.
- Allow callers to configure the weighting between keyword and semantic scores (e.g., `?keywordWeight=0.4&semanticWeight=0.6`).

### Why it matters

Pure vector search struggles with exact term matches (product codes, author names); pure keyword search misses synonyms and intent. Hybrid search gets the best of both and is what production search systems (Elasticsearch kNN + BM25, Azure AI Search, Vespa) actually do.

---

## 2. Chunking and Long-Document Support

Currently each book is embedded as a single string (`"{Title} by {Author}. {Description}"`).
This works because descriptions are short, but falls apart for longer content.

### What to build

- Add a `Synopsis` or `FullText` field to books in the catalog (seed with multi-paragraph data).
- Implement a chunking pipeline in the semantic search consumer:
  - **Recursive character splitting** with configurable chunk size (e.g., 512 tokens) and overlap (e.g., 50 tokens).
  - Store multiple `BookEmbedding` rows per book, each with a `ChunkIndex` and the chunk text.
- Implement **parent-child retrieval**: retrieve the best-matching chunk, but return the full document (or a wider window around the chunk) as context to the LLM.
- Compare chunk sizes (256 / 512 / 1024 tokens) and measure which gives the best retrieval quality.

### Why it matters

Chunking strategy is the single biggest lever in RAG quality. Interviewers will ask about it. The tradeoffs (small chunks = precise retrieval but lost context; large chunks = more context but diluted embeddings) are non-obvious and worth experiencing firsthand.

---

## 3. Retrieval Evaluation with Ground Truth

There is no way to measure whether search results are actually good. Adding evaluation makes the project stand out and builds the habit of measuring before optimizing.

### What to build

- Create a ground truth dataset: 30-50 queries with labeled relevant book IDs (store as a JSON file in `seed/`).
- Implement an evaluation endpoint or CLI command that runs all queries and computes:
  - **Precision@K**: fraction of top-K results that are relevant.
  - **MRR** (Mean Reciprocal Rank): how high the first relevant result appears.
  - **nDCG** (normalized Discounted Cumulative Gain): rewards relevant results appearing earlier.
- Run evaluation against keyword search, semantic search, and hybrid search to compare them quantitatively.
- Optionally integrate [RAGAS](https://docs.ragas.io/) for RAG-specific metrics: faithfulness (does the LLM answer match the retrieved context?), answer relevancy, context precision.

### Why it matters

Almost nobody evaluates their RAG demos. Being able to say "I measured MRR and found that hybrid search improved it by X% over pure vector search" is a concrete, credible claim that separates you from everyone who just wired up a pipeline.
