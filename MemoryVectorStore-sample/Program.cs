using MemoryVectorDB;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using OpenAI.Managers;
using OpenAI;
using DataChunker;

namespace MemoryVectorDB_sample
{
  
    // todo: add sample code interpreting the chunks
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string OpenAIkey           = File.Exists("apikey.txt") ?File.ReadAllText("apikey.txt") :""; // "API key here"; // OpenAI key
            string documentPath        = "D:\\Users\\Thijs\\OneDrive\\devel\\LLM\\MemoryVectorDB\\TestData\\Robinson-Crusoe-in-Levels-PDF.pdf"; // PDF document
            string documentVectorsPath = $"{documentPath}.json"                                                                               ; // Vectors created by embedding algorithm
            string documentTextPath    = $"{documentPath}.txt"                                                                                ; // text only document
            string queryString         = $"Canibals and hiding bodies"                                                                        ; // string to look for

            Console.WriteLine("** Starting embedding demo");

            var embeddingSample = new EmbeddingSample(OpenAIkey);
            // Create embedding, only needed once

            if (File.Exists(documentVectorsPath) && File.Exists(documentTextPath))
            {
                Console.WriteLine("** Vectors already exist, reading previous embedding"); 
                // Read embedding
                embeddingSample.DeserializeDocumentText(documentTextPath);
                await embeddingSample.DeserializeVectorsAsync(documentVectorsPath);
            }
            else
            {
                Console.WriteLine("** Embedding document");
                await embeddingSample.WordEmbeddingAsync(documentPath, documentTextPath);
                      embeddingSample.SerializeDocumentText(documentTextPath);
                await embeddingSample.SerializeVectorsAsync(documentVectorsPath);
            }
         
            await embeddingSample.GetFindingsAsync(queryString);

            // Todo: have OpenAI interpret the embeded chunks

            Console.WriteLine("** Done");
        }



        public class EmbeddingSample
        {
            private VectorDB<Chunk>         _vectorCollection;
            private OpenAIService           _openAiService;
            private Document?               _document;
            private ChunkGenerator?         _chunkGenerator;

            public EmbeddingSample(string apiKey)
            {
                //  OpenAI service that we are going to use for embedding   
                _openAiService = new OpenAIService(new OpenAiOptions()  {ApiKey = apiKey });

                // Collection of vectors made of chunks of document
                _vectorCollection = new MemoryVectorDB.VectorDB<Chunk>(100, ChunkEmbedingAsync);
            }

            public async Task WordEmbeddingAsync(string documentPath, string documentTextPath )
            {

                // Document to embed
                _document = PdfTextExtractor.GetText(documentPath);

                // Chunk generator
                _chunkGenerator = new ChunkGenerator(200, 100, _document);

                var i = 0;
                // Get the chunks and embed them
                foreach (var chunk in _chunkGenerator.GetChunk())
                {
                    Console.WriteLine($"***Chunk {i++}***");
                    Console.WriteLine(chunk.Text);

                    // Add the source reference
                    chunk.Source = documentPath;

                    // Embed the chunk
                    await _vectorCollection.AddAsync(chunk);

                    // We clean out the text, to safe memory: we just need the vector, start index and length
                    chunk.Text = null!;

                    // break after 5 chunks
                    //if (i == 5) break;
                }
            }
            public async Task SerializeVectorsAsync(string fileName)
            {
                await _vectorCollection.SerializeJsonAsync(fileName);        
            }

            public void SerializeDocumentText(string documentTextPath)
            {
                // Write the document to disk
                if (_document == null) return;
                File.WriteAllText(documentTextPath, _document.Text);
            }

            internal void DeserializeDocumentText(string documentTextPath)
            {
                _document = new Document();
                _document.Add(File.ReadAllText(documentTextPath),"");
                _document.Source = documentTextPath;
            }


            internal async Task DeserializeVectorsAsync(string fileName)
            {
                await _vectorCollection.DeserializeJsonAsync(fileName);
            }

            // Callback function for embedding in the vector database
            private async Task<Chunk?> ChunkEmbedingAsync(Chunk inputObject)
            {
                var embeddingResult = await _openAiService.Embeddings.CreateEmbedding(new EmbeddingCreateRequest(){
                    InputAsList = new List<string> { inputObject.Text },
                    Model       = Models.TextEmbeddingAdaV2
                });

                if (embeddingResult.Successful)
                {
                    var value = embeddingResult.Data.FirstOrDefault()?.Embedding;
                    if (value==null) return null!;
                    inputObject.SetVector(value);
                    return inputObject;
                }
                else { return null!; }                    
            }

            public async Task GetFindingsAsync(string query)
            {
                var querychunk  = await ChunkEmbedingAsync(new Chunk() { Text = query });  
                var queryVector = querychunk?.GetVector()??new float[0];
                var bestMatches = _vectorCollection.FindNearestSorted(queryVector, 10);

                foreach (var item in bestMatches)
                {
                    ShowMatch(item.Value, queryVector);                    
                }                
            }

            private void ShowMatch(Chunk chunk, float[] queryVector)
            {
                var dotProduct = DotProduct(chunk.GetVector(), queryVector);
                Console.WriteLine($"Match: {dotProduct} - {chunk.StartCharNo} - {chunk.CharLength}");                
                Console.WriteLine(_document?.Text.Substring(chunk.StartCharNo, chunk.CharLength)??"");
                Console.WriteLine();
                Console.WriteLine();
            }

            public static float DotProduct(float[] a, float[] b)
            {
                float sum = 0;
                for (int i = 0; i < a.Length; i++)
                {
                    sum += a[i] * b[i];
                }

                return sum;
            }
        }

    }
}