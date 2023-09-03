namespace MemoryVectorDB
{
    public class FloatArrayUtils
    {
        public static float DotProduct(float[] a, float[] b)
        {
            float sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += a[i] * b[i];
            }

            return sum;
        }

        public static float Length(float[] vector)
        {
            float sum = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                sum += vector[i] * vector[i];
            }
            return (float)Math.Sqrt(sum);
        }

        public static float[] NormalizeVector(float[] vector)
        {
            var length = Length(vector);
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = vector[i] / length;
            }
            return vector;
        }
    }
}