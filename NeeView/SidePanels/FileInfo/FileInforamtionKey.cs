using System;
using System.ComponentModel;
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// 画像情報項目キー (Immutable)
    /// </summary>
    public record class FileInforamtionKey 
    {
        public FileInforamtionKey(InformationKey key)
        {
            Debug.Assert(key != InformationKey.ExtraValue);

            Key = key;
            Name = "";
        }

        public FileInforamtionKey(string name)
        {
            Key = InformationKey.ExtraValue;
            Name = name;
        }


        public InformationKey Key { get; private set; }

        public string Name { get; private set; }



        public bool IsExtra()
        {
            return Key.IsExtra();
        }

        public override string ToString()
        {
            if (IsExtra())
            {
                return Name;
            }
            else
            {
                return AliasNameExtensions.GetAliasName(Key) ?? Key.ToString();
            }
        }
    }
}
