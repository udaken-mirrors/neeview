using System.Collections.Generic;

namespace NeeView
{
    public class WordNode
    {
        public WordNode()
        {
            Word = "";
        }

        public WordNode(string word)
        {
            Word = word;
        }

        public string Word { get; set; }

        public List<WordNode>? Children { get; set; }

    }
}
