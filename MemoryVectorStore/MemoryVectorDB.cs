using System.Text.Json;

namespace MemoryVectorDB
{

    /// <summary>
    /// Data objects that should be stored insied a collection
    /// </summary>
    public interface IVectorObject
    {
        float[] GetVector();
    }

    /// <summary>
    /// Data model for serializing and desializing to disk
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VectorCollectionData<T> where T : IVectorObject
    {
        public List<T> VectorObjects { get; set; } = null!;
    }


    /// <summary>
    /// A collection of <see cref="IVectorObject"/>
    /// </summary>
    public class VectorDB<T> where T : IVectorObject
    {

        // Delegate for creating a vector from an object
        public delegate Task<T?> EmbeddingDelegate(T inputObject);

        //private  int _dimensions;
        private VectorDB<T>.EmbeddingDelegate _embeddingDelegate;
        private List<T> _vectorObjects = new();

        public VectorDB(int dimensions, EmbeddingDelegate embeddingDelegate)
        { 
            _embeddingDelegate = embeddingDelegate;
        }


        public async Task AddAsync(T vectorObject)
        {
            // Generate the vector to identify the object
            T? vectorizedObject = default(T);
            if (_embeddingDelegate != null) vectorizedObject = await _embeddingDelegate(vectorObject);

            //Normalize vector not necessary, already done by the embedding service
            if (vectorizedObject != null)
            {
                _vectorObjects.Add(vectorizedObject);
            }            
        }

        public async Task AddRangeAsync(IEnumerable<T> vectorObjects)
        {
            foreach (var vectorObject in vectorObjects){ await AddAsync(vectorObject);}            
        }

        public IVectorObject GetItem(int index)
        {
                return _vectorObjects[index];
        }

        /// <summary>
        /// Find nearest vector by comparing all vectors to the query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public SortedList<float, T> FindNearestSorted(float[] query, int noItems)
        {
            var descending = Comparer<float>.Create((a, b) => Comparer<float>.Default.Compare(b, a));

            SortedList<float, T> nearestObjects = new(descending);
            // Naive lock around everything. Can probably be done much faster
            
            for (int i = 0; i < _vectorObjects.Count; i++)
            {
                // Find the lowest dot product of the top finds
                float maxDotProduct = nearestObjects.Count!=0?nearestObjects.Last().Key:0;
                // check if the current object is closer than the lowest dot product in the top finds
                float dotProduct = DotProduct(_vectorObjects[i].GetVector(), query);
                  
                if (dotProduct > maxDotProduct)
                {
                    // Add to the list
                    nearestObjects.Add(dotProduct, _vectorObjects[i]);
                    // Remove the last item if the list is too long
                    if (nearestObjects.Count > noItems) nearestObjects.RemoveAt(nearestObjects.Count - 1);
                }
            }            
            return nearestObjects;
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

        public List<T> FindNearest(float[] query, int noItems) {
            var sortedList = FindNearestSorted(query, noItems);
            return new List<T>((sortedList.Values));
        }


        /// <summary>
        /// Serialize to disk
        /// </summary>
        /// <param name="path">Path to serialize to</param>
        /// <returns></returns>
        public async Task SerializeJsonAsync(string path)
        {
            var json = JsonSerializer.Serialize(new VectorCollectionData<T>
                {
                    //Dimensions = Dimensions,
                    VectorObjects    = _vectorObjects
                });

            await File.WriteAllTextAsync(path, json);
        }

        /// <summary>
        /// Deserialize from disk
        /// </summary>
        /// <param name="path">Path to serialize from</param>
        /// <returns></returns>
        public async Task DeserializeJsonAsync(string path)
        {
            var json           = await File.ReadAllTextAsync(path);
            var data           = JsonSerializer.Deserialize<VectorCollectionData<T>>(json);
            if (data?.VectorObjects != null) { _vectorObjects = data.VectorObjects; }
        }
    }
}