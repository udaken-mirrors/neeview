using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;

namespace NeeView
{
    public class FileInformationRecord : BindableBase, IHasKey<FileInforamtionKey>
    {
        private object? _value;


        public FileInformationRecord(InformationKey key, object? value)
        {
            Debug.Assert(key != InformationKey.ExtraValue);

            Key = new FileInforamtionKey(key);
            Group = key.ToInformationGroup();
            _value = value;
        }

        public FileInformationRecord(string name, InformationGroup group, object? value)
        {
            Key = new FileInforamtionKey(name);
            Group = group;
            Value = value;
        }

        public FileInforamtionKey Key { get; private set; }

        public InformationGroup Group { get; private set; }

        public object? Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public FileInformationRecord Clone()
        {
            return (FileInformationRecord)MemberwiseClone();
        }

        public override string ToString()
        {
            return $"Key = {Key}, Value = {Value}";
        }
    }
}
