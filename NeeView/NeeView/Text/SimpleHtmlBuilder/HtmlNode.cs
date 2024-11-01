using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// Simple HTML generation assistant
namespace NeeView.Text.SimpleHtmlBuilder
{
    public delegate string TextEvaluator(string text);

    public class HtmlNode
    {
        public static TextEvaluator DefaultTextEvaluator { get; set; } = e => e;
    }

    public class TagNode : HtmlNode
    {
        private readonly string _name;
        private List<string>? _attributes;
        private List<HtmlNode>? _nodes;

        public TagNode(string name)
        {
            _name = name;
        }

        public TagNode(string name, string classValue)
        {
            _name = name;
            AddAttribute("class", classValue);
        }

        public TagNode AddAttribute(string name, string value)
        {
            _attributes ??= new List<string>();
            _attributes.Add($"{name}=\"{value}\"");
            return this;
        }

        public TagNode AddNode(HtmlNode node)
        {
            _nodes ??= new List<HtmlNode>();
            _nodes.Add(node);
            return this;
        }

        public TagNode AddText(string text)
        {
            return AddNode(new TextNode(DefaultTextEvaluator(text)));
        }

        public TagNode AddText(string text, TextEvaluator textEvaluator)
        {
            return AddNode(new TextNode(textEvaluator(text)));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var tagWithAttribute = _name + (_attributes is null ? "" : " " + string.Join(" ", _attributes));
            var tag = _name;

            if (_nodes is null)
            {
                builder.Append(CultureInfo.InvariantCulture, $"<{tagWithAttribute}/>");
            }
            else
            {
                builder.Append(CultureInfo.InvariantCulture, $"<{tagWithAttribute}>");
                foreach (var node in _nodes)
                {
                    builder.Append(node.ToString());
                }
                builder.Append(CultureInfo.InvariantCulture, $"</{tag}>");
            }
            return builder.ToString();
        }
    }

    public class TextNode : HtmlNode
    {
        private readonly string _text;

        public TextNode(string text)
        {
            _text = text;
        }

        public override string ToString()
        {
            return _text;
        }
    }
}


