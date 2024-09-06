namespace NeeView
{
    public interface ICommandParameterFactory<T>
    {
        object CreateParameter(T source);
    }

}
