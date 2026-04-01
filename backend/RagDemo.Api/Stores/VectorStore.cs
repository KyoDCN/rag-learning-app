using System.Collections.ObjectModel;

namespace RagDemo.Api.Stores;

public class VectorStore
{
    private readonly List<DocumentChunk> m_chunks = [];
    private readonly Lock m_lock = new();

    public void Add(DocumentChunk chunk)
    {
        lock (m_lock)
            m_chunks.Add(chunk);
    }

    public void Clear()
    {
        lock (m_lock)
            m_chunks.Clear();
    }

    public void AddRange(IEnumerable<DocumentChunk> chunks)
    {
        lock (m_lock)
            m_chunks.AddRange(chunks);
    }

    public void ReplaceAll(IEnumerable<DocumentChunk> chunks)
    {
        lock (m_lock)
        {
            m_chunks.Clear();
            m_chunks.AddRange(chunks);
        }
    }

    public int Count
    {
        get
        {
            lock (m_lock)
                return m_chunks.Count;
        }
    }

    /// <summary>
    /// Searches the vector store for chunks most semantically similar to the query embedding.
    /// Results are ranked by cosine similarity and filtered by a minimum score threshold.
    /// </summary>
    /// <param name="queryEmbedding">Vectors representation of texts.</param>
    /// <param name="topK">Number of results to return.</param>
    /// <param name="threshold">Filters out chunk results given a threshold to reduce hallucination effects.</param>
    /// <returns></returns>
    public ReadOnlyCollection<DocumentChunk> FindSimilar(float[] queryEmbedding, int? topK = null, float? threshold = null)
    {
        int topKValue = topK.GetValueOrDefault(5);
        float thresholdValue = threshold.GetValueOrDefault(0.75F);

        lock (m_lock)
        {
            return m_chunks
                .Select(chunk => new
                {
                    Chunk = chunk,
                    Score = CosineSimilarity(queryEmbedding, chunk.Embedding)
                })
                .Where(x => x.Score >= thresholdValue) // Filter out ones where scores don't meet threshold value
                .OrderByDescending(x => x.Score)
                .Take(topKValue) // Take only specific number of chunks
                .Select(x => x.Chunk)
                .ToList()
                .AsReadOnly();
        }
    }

    /// <summary>
    /// Measures the angle between two vectors.
    /// Returns 1.0 if identical or 0.0 of completely unrelated.
    /// Essentially determines similarity of vectors
    /// </summary>
    /// <param name="vectorA"></param>
    /// <param name="vectorB"></param>
    /// <returns></returns>
    private float CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        float dotProduct = 0F;
        float magnitudeA = 0F;
        float magnitudeB = 0F;

        for (int i=0; i<vectorA.Length; i++)
        {
            // Dot Product (Summation of Vector A * Vector B)
            dotProduct += vectorA[i] * vectorB[i];

            // Summation of Vector A magnitude
            magnitudeA += vectorA[i] * vectorA[i];

            // Summation of Vector B magnitude
            magnitudeB += vectorB[i] * vectorB[i];
        }

        float magnitude = MathF.Sqrt(magnitudeA) * MathF.Sqrt(magnitudeB);

        return magnitude != 0 ? dotProduct / magnitude : 0;
    }
}