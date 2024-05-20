using System.Collections.Generic;

namespace NeeView
{
    public class ExternalAppCollection : List<ExternalApp>
    {
        public ExternalAppCollection()
        {
        }

        public ExternalAppCollection(IEnumerable<ExternalApp> collection) : base(collection)
        {
        }

        public ExternalApp CreateNew()
        {
            var item = new ExternalApp();
            this.Add(item);
            return item;
        }
    }


}
