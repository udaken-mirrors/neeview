using System.Runtime.Serialization;
using System.Text;

namespace NeeLaboratory.IO
{

    [DataContract]
    public class RemoteCommand
    {
        public RemoteCommand()
        {
            Id = "";
            Args = System.Array.Empty<string>();
        }

        public RemoteCommand(string id)
        {
            Id = id;
            Args = System.Array.Empty<string>();
        }

        public RemoteCommand(string id, params string[] args)
        {
            Id = id;
            Args = args;
        }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string[] Args { get; set; }
    }

}
