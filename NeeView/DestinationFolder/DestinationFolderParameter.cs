namespace NeeView
{
    public record DestinationFolderParameter(DestinationFolder DestinationFolder, IDestinationFolderOption Option);


    public interface IDestinationFolderOption
    {
        MultiPagePolicy MultiPagePolicy { get; }
    }


    public class StaticDestinationFolderOption : IDestinationFolderOption
    {
        public StaticDestinationFolderOption(MultiPagePolicy multiPagePolicy)
        {
            MultiPagePolicy = multiPagePolicy;
        }

        public MultiPagePolicy MultiPagePolicy { get; }
    }


    public class DestinationFolderParameterCommandParameterFactory : ICommandParameterFactory<DestinationFolder>
    {
        private readonly IDestinationFolderOption _folderParameter;

        public DestinationFolderParameterCommandParameterFactory(IDestinationFolderOption folderParameter)
        {
            _folderParameter = folderParameter;
        }

        public object CreateParameter(DestinationFolder folder)
        {
            return new DestinationFolderParameter(folder, _folderParameter);
        }
    }
}
