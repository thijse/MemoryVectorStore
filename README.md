[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

# Memory Vector Store
Sample of implementing a simple in-memory vector store

This repository:
The repository contains three main projects: 
- Memory Vector Store project, which focuses on storing vectors in memory;
- Chunk Creator project, which extracts vectors from PDF files;
- Sample Search project, which demonstrates how to perform similarity searches using the stored vectors. Each project has its own set of code and resources, allowing you to explore and understand the implementation details.

 This code is based on the blog post titled ["Vector Search with C#: A Practical Approach for Small Datasets."](https://crispycode.net/vector-search-with-c-a-practical-approach-for-small-datasets/) 


## Code example

First we need to make chunks of the original PDF and build the embedding vectors

```cs
//  OpenAI service that we are going to use for embedding   
_openAiService = new OpenAIService(new OpenAiOptions()  {ApiKey = apiKey });

// Set up a MemoryVector database, to be filled with chunks of documents
// including an embedding vector of 1536 dimensions
// Also included is a callback that embeds any text item into a vector
_vectorCollection = new MemoryVectorDB.VectorDB<Chunk>(1536, ChunkEmbedingAsync);

// Get text fom pdf 
_document = PdfTextExtractor.GetText(documentPath);

// Generate chunks of 200 words and an overlap of 100 words 
_chunkGenerator = new ChunkGenerator(200, 100, _document);

// Loop through chunks
foreach (var chunk in _chunkGenerator.GetChunk())
{
    // Add the source reference to the chunk
    chunk.Source = documentPath;

    // Add the chunk to the vector store
    await _vectorCollection.AddAsync(chunk);

    // We remove the text from the chunk to safe memory: 
    // we just need the vector, start index, length and source
    // so we can recover the the chunk from the original document later
    chunk.Text = null!;
}
```

Now we can find the best matching chunks related to our query

```cs
// First we make a vector of the query like we have done for the chunks
var queryVector = querychunk?.GetVector()??new float[0];

// Next find the closest vectors to the query vector
var bestMatches = _vectorCollection.FindNearestSorted(queryVector, 10);

// And here they are
foreach (var item in bestMatches)
{
    ShowMatch(item.Value, queryVector);                    
} 
```
Note that the FindNearestSorted is just a brute-force comparison of the dot products between the query vector and all chunk vectors. For larger vector stores, indexes should be created

Finally we want a conversational network to interpret the chunks and answer the question

```cs
// Format the query to post to the LLM:
queryBuilder.AppendLine($"Answer the following query {query}. Only use the content below to construct the answer. If no content is shown below or if it is not applicable, answer: \"Sorry, I have no data on that\" \n\n");

// Insert the best matches
foreach (var match in bestMatches)
{
    var chunk = match.Value;
    queryBuilder.AppendLine(_document?.Text.Substring(chunk.StartCharNo, chunk.CharLength)+"\n" ?? "");
}

// Ask Completion to answer the query
var completionResult = await _openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
{
    Messages = new List<ChatMessage>
    {
        ChatMessage.FromSystem("Your are an AI assistant. The assistant is helpful, factual and friendly."), 
        ChatMessage.FromUser(queryBuilder.ToString()),
    },
    Model = Models.Gpt_3_5_Turbo,
});
```

