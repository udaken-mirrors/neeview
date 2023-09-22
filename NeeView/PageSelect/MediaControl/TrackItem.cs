namespace NeeView
{
    public class TrackItem
    {
        public TrackItem(int iD, string name)
        {
            ID = iD;
            Name = name;
        }

        public int ID { get; }
        public string Name { get; }

        public override string ToString()
        {
            return $"{ID}: {Name}";
        }
    }
}

