using System.Numerics;

namespace MemoryVectorDB
{
    public class VectorF 
    {
        public int Length { get; }
        public Vector<float> Value { get; set; }

        public VectorF(Vector<float> value, int length)
        {
            Length = length;
            Value = value;
        }

        public VectorF(float[] value)
        {
            Length = value.Length;
            Value = new Vector<float>(value);
        }

        public static implicit operator VectorF(float[] vector)
        {
            return new VectorF(vector);
        }

         public static implicit operator Vector<float>(VectorF vectorF)
        {
            return vectorF.Value;
        }

        public static implicit operator VectorF(List<float> vector)
        {
            return new VectorF(vector.ToArray());
        }

        public float[] ToArray()
        {
            var returnData = new float[Length];
            Value.CopyTo(returnData);
            return returnData;
        }
    }


        public class VectorM<T> where T : struct
    {
        public int Length      { get;      }
        public Vector<T> Value { get; set; }

        public VectorM(Vector<T> value, int length)
        {
            Length = length;
            Value = value;
        }

        public VectorM(T[] value)
        {
            Length = value.Length;
            Value = new Vector<T>(value);
        }

        public VectorM(T value)
        {
            Length = 1;
            Value = new Vector<T>(value);
        }

        public VectorM()
        {
            Length = 0;
            Value = new Vector<T>();
        }

        public static implicit operator Vector<T>(VectorM<T> vectorM)
        {
            return vectorM.Value;
        }

        public static implicit operator VectorM<T>(T[] vector)
        {
            return new VectorM<T>(vector);
        }

        public static implicit operator VectorM<T>(List<T> vector)
        {
            return new VectorM<T>(vector.ToArray());
        }

        public static implicit operator VectorM<T>(T value)
        {
            return new VectorM<T>(value);
        }

        public static implicit operator T[](VectorM<T> vectorM)
        {
            return vectorM.ToArray();
        }

        public static implicit operator T(VectorM<T> vectorM)
        {
            return vectorM.Value[0];
        }

        public T[] ToArray()
        {
            var returnData = new T[Length];
            Value.CopyTo(returnData);
            return returnData;
        }

    }
}
