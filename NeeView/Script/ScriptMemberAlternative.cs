using System;

namespace NeeView
{
    public class ScriptMemberAlternative
    {
        private readonly ObsoleteAttribute? _obsoleteAttribute;
        private readonly AlternativeAttribute? _alternativeAttribute;
        private readonly string? _prefix;
        private string? _alternativeMessage;

        public ScriptMemberAlternative(string name, ObsoleteAttribute? obsoleteAttribute, AlternativeAttribute? alternativeAttribute, string? prefix = null)
        {
            Name = name;
            _obsoleteAttribute = obsoleteAttribute;
            _alternativeAttribute = alternativeAttribute;
            _prefix = prefix;
        }

        public string Name { get; }
        public bool HasObsolete => _obsoleteAttribute != null;
        public bool HasAlternative => _alternativeAttribute != null;

        public string ObsoleteMessage => GetObsoleteMessage();

        public string AlternativeMessage
        {
            get => _alternativeMessage ?? GetAlternativeMessage();
            set => _alternativeMessage = value;
        }

        public int Version => _alternativeAttribute?.Version ?? 0;

        public ScriptErrorLevel ErrorLevel => _alternativeAttribute?.ErrorLevel ?? ScriptErrorLevel.Error;


        private string GetObsoleteMessage()
        {
            return _obsoleteAttribute?.Message ?? "";
        }

        private string GetAlternativeMessage()
        {
            if (_alternativeAttribute?.Alternative is null) return "x";

            if (_alternativeAttribute.IsFullName || _prefix is null)
            {
                return _alternativeAttribute.Alternative;
            }
            else
            {
                return _prefix + "." + _alternativeAttribute.Alternative;
            }
        }
    }
}
