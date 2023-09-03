namespace MemoryVectorDB
{
    /// <summary>
    /// Base class for a vector object with some convenience methods
    /// </summary>
    /// 
    public class VectorObject : IVectorObject
    {
        // todo, make 

        protected float[]? _textVector = null;

        public float[] GetVector()
        {
            return _textVector ?? throw new Exception("TextVectors not set");
        }

        public void SetVector(List<float> vector)
        {
            _textVector = vector.ToArray();
            
        }
        public void SetVector(List<double> vector)
        {
            _textVector = vector.ConvertAll(new Converter<double, float>((d) => (float)d)).ToArray();
        }
    }
}
