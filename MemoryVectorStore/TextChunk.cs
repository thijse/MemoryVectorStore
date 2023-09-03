using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MemoryVectorDB
{
    /// <summary>
    /// A chunk of text from a document. Will also contain the pagination number, an index within the page and a source reference.
    /// </summary>
    public class TextChunk : IVectorObject
    {
        public string Source        { get; set; } = null!;
        public int Page             { get; set; }
        public int OnPageIndex      { get; set; }
        public string Text          { get; set; } = null!;
        Vector<float>? TextVectors  { get; set; }

        public Vector<float> GetVector()
        {
            return TextVectors ?? throw new Exception("TextVectors not set");
        }

        public void SetVector(float[] vector)
        {
            TextVectors = new Vector<float>(vector);
        }

        public void SetVector(List<float> vector)
        {
            TextVectors = new Vector<float>(vector.ToArray());
        }

        public void SetVector(List<double> vector)
        {
            float[] floatList = vector.ConvertAll(new Converter<double, float>((d) => (float)d )).ToArray();
            TextVectors = new Vector<float>(floatList);
        }

    }
}
