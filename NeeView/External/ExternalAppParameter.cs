namespace NeeView
{
    public record ExternalAppParameter(ExternalApp ExternalApp, IExternalAppOption Option);


    public interface IExternalAppOption
    {
        MultiPagePolicy MultiPagePolicy { get; }
    }


    public class StaticExternalAppOption : IExternalAppOption
    {
        public StaticExternalAppOption(MultiPagePolicy multiPagePolicy)
        {
            MultiPagePolicy = multiPagePolicy;
        }

        public MultiPagePolicy MultiPagePolicy { get; }
    }


    public class ExternalAppParameterCommandParameterFactory : ICommandParameterFactory<ExternalApp>
    {
        private readonly IExternalAppOption _option;

        public ExternalAppParameterCommandParameterFactory(IExternalAppOption option)
        {
            _option = option;
        }

        public object CreateParameter(ExternalApp externalApp)
        {
            return new ExternalAppParameter(externalApp, _option);
        }
    }
}
