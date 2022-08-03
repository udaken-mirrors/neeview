using System.Collections.Generic;

namespace NeeView.Collections
{
    public interface IHasName
    {
        // TODO: IHasName.Name is not null
        string? Name { get; }
    }

    public class HasNameComparer : IComparer<IHasName>
    {
        public int Compare(IHasName? x, IHasName? y)
        {
            if (x is null || x.Name is null)
            {
                return y is null || y.Name is null ? 0 : -1;
            }
            else if (y is null || y.Name is null)
            {
                return 1;
            }
            else
            {
                return NaturalSort.Compare(x.Name, y.Name);
            }
        }
    }

    public class NameComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }
            else if (y == null)
            {
                return 1;
            }
            else
            {
                return NaturalSort.Compare(x, y);
            }
        }
    }
}
