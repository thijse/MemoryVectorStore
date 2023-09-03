using MemoryVectorDB;

namespace DataChunker
{
    public partial class Chunk : VectorObject
    {
        public float[]? TextVectors {
            get { return base._textVector; }
            set { _textVector = value;     }
        } 
        public int StartCharNo   { get; set; } = 0;
        public int CharLength    { get; set; } = 0;
        public string Text       { get; set; } = "";
        public string Source     { get; set; } = "";
        public Chunk(string startIndex, int startCharNo, int charLength, string text)
        {
            StartCharNo = startCharNo;
            CharLength  = charLength;
            Text        = text;
        }

        public Chunk(){}
    }

    public class ChunkGenerator
    {
        private int _index; // current character index in the document
        private int _chunkSize;
        private int _overlap;
        private Document _document;

        /// <summary>
        /// Creates a chunker that will chunk the document into chunks of chunkSize number of words, with an overlap of words .
        /// </summary>
        /// <param name="chunkSize">number of words in the chuck</param>
        /// <param name="overlap">number of words overlapping with the previous chunk</param>
        /// <param name="document">Document to go through</param>        
        public ChunkGenerator(int chunkSize, int overlap, Document document)
        {
            _index = 0;
            _chunkSize = chunkSize;
            _overlap   = overlap;
            _document  = document;

        }

        // Get the character index after next word, starting from the current index
        int GetNextWord(int currentIndex)
        {
            //Break Word at length 
            int wordBreak = 50; // break word at 50 characters, longest word in english is 45 characters

            // get the next word
            var currWordLength = 0;
            int index = currentIndex;
            while (index < _document.Text.Length && !char.IsWhiteSpace(_document.Text[index]))
            {
                index++; currWordLength++;
                // word is too long, break it
                if (currWordLength > wordBreak) return index;
            }
            // get the whitespaces following the word
            while (index < _document.Text.Length && char.IsWhiteSpace(_document.Text[index]))
            {
                index++;
            }
            return index;
        }

        int GetNextWords(int currentIndex, int numberOfWords)
        {
            int index = currentIndex;
            for (int i = 0; i < numberOfWords; i++)
            {
                index = GetNextWord(index);
            }
            return index;
        }   

        // yields the next chunk, with chunkSize in (space separated) words, with an overlap (in words)
        public IEnumerable<Chunk> GetChunk()
        {
            // get the first chunk
            int index = _index;

            // overlap index is start for the next chunk
            int overlapIndex = GetNextWords(index, _chunkSize - _overlap);
            // index is end of this chunk
            int endIndex = GetNextWords(overlapIndex, _overlap);
            
            while (endIndex < _document.Text.Length)
            {
                yield return new Chunk(_document.GetIndex(index), index, endIndex - index, _document.Text.Substring(index, endIndex - index));
                index        = overlapIndex;
                overlapIndex = GetNextWords(index, _chunkSize - _overlap);
                endIndex     = GetNextWords(overlapIndex, _overlap);
            }
            _index = index;
            yield break;
        }             
    }
}
