#pragma warning disable CA1822

namespace NeeView
{
    public class EnvironmentAccessor
    {
        [WordNodeMember]
        public string NeeViewPath => Environment.AssemblyLocation;

        [WordNodeMember]
        public string UserSettingFilePath => SaveData.UserSettingFilePath;

        [WordNodeMember]
        public string PackageType => Environment.PackageType;

        [WordNodeMember]
        public string UserAgent => Environment.UserAgent;

        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());

            return node;
        }
    }
}
