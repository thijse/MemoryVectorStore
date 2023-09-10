using System.Text;

namespace DataChunker
{

    /// <summary>
    /// A document index, consisting of a description of the index and a character index in the document
    /// </summary>
    public class DocumentIndex
    {
        public DocumentIndex(string documentIndex, int characterNo, int characterLength)
        {
            Index           = documentIndex;
            CharacterNo     = characterNo;
            CharacterLength = characterLength;
        }

        public string Index        { get; }
        public int CharacterNo     { get; }
        public int CharacterLength { get; }
        //
    }

    /// <summary>
    /// Document is a class that contains a text and a list of indices. The indices are used to point to a specific part of the text.
    /// </summary>
    public class Document
    {
        private StringBuilder _text = new();
        public string Source               { get; set; } = null!;
        public List<DocumentIndex> Indices { get; set; } = new();
        public string Text                 { get { return _text.ToString(); } }


        public void Add(string text, string index)
        {
            Indices.Add(new DocumentIndex(index, _text.Length-1, text.Length));
            _text.Append(text);
        }

        public string GetText(string index)
        {
            var documentIndex = Indices.FirstOrDefault(i => i.Index == index);
            if (documentIndex == null) return string.Empty;
            return _text.ToString(documentIndex.CharacterNo, _text.Length - documentIndex.CharacterNo);
        }

        public string GetIndex(int characterNo)
        {
            var documentIndex = Indices.FirstOrDefault(i => i.CharacterNo <= characterNo && characterNo < i.CharacterNo + i.CharacterLength);
            if (documentIndex == null) return string.Empty;
            return documentIndex.Index;
        }

    }
}
