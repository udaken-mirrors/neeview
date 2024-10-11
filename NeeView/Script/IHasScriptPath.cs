namespace NeeView
{
    public interface IHasScriptPath
    {
        string? ScriptPath { get; }
    }


    public class DummyScriptPath : IHasScriptPath
    {
        public string? ScriptPath => null;
    }
}
