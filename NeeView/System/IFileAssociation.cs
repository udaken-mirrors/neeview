namespace NeeView
{
    public interface IFileAssociation
    {
        FileAssociationCategory Category { get; }
        string Extension { get; }
        string? Description { get; }
        bool IsEnabled { get; set; }
    }

}