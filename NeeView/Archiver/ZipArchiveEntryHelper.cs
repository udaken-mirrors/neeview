using System;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace NeeView
{
    public static class ZipArchiveEntryHelper
    {
        private static bool _initialized;
        private static bool _enabled;
        private static FieldInfo? _generalPurposeBitFlagField;
        private static FieldInfo? _storedEntryNameBytesField;
        private static FieldInfo? _storedEntryNameField;

        /// <summary>
        /// 初期化
        /// </summary>
        /// <remarks>
        /// 修理に必要なプライベートフィールドのリフレクション取得
        /// </remarks>
        private static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                var type = typeof(ZipArchiveEntry);
                _generalPurposeBitFlagField = type.GetField("_generalPurposeBitFlag", BindingFlags.NonPublic | BindingFlags.Instance);
                _storedEntryNameBytesField = type.GetField("_storedEntryNameBytes", BindingFlags.NonPublic | BindingFlags.Instance);
                _storedEntryNameField = type.GetField("_storedEntryName", BindingFlags.NonPublic | BindingFlags.Instance);

                _enabled = _generalPurposeBitFlagField is not null
                    && _storedEntryNameBytesField is not null && _storedEntryNameBytesField.FieldType == typeof(byte[])
                    && _storedEntryNameField is not null && _storedEntryNameField.FieldType == typeof(string);
            }
            catch
            {
                _enabled = false;
            }
        }

        /// <summary>
        /// エントリ名を UTF-8 フラグを反映したものに修正する
        /// </summary>
        /// <remarks>
        /// .NET8 は UTF8 フラグが使用されていない不具合がある。この処理はこの問題に対応されるまでの応急処置です。<br/>
        /// <see href="https://github.com/dotnet/runtime/issues/92283"/>
        /// </remarks>
        /// <param name="entry">修正するエントリ</param>
        public static void RepairEntryName(ZipArchiveEntry entry)
        {
            if (!_initialized)
            {
                Initialize();
            }

            if (!_enabled) return;

            // has UTF-8 flag ?
            if (_generalPurposeBitFlagField is null) return;
            try
            {
#pragma warning disable CS8605 // null の可能性がある値をボックス化解除しています。
                // NOTE: BitFlagValues にキャストしたいけどアクセスできないため ushort に静的キャスト
                var generalPurposeBitFlag = (ushort)_generalPurposeBitFlagField.GetValue(entry);
#pragma warning restore CS8605 // null の可能性がある値をボックス化解除しています。
                if ((generalPurposeBitFlag & (1 << 11)) == 0) return;
            }
            catch
            {
                // キャスト例外が発生したときはライブラリの構造が変化したと考えられるため、この機能自体を無効化する    
                _enabled = false;
                return;
            }

            // convert to UTF-8
            if (_storedEntryNameBytesField is null) return;
            if (_storedEntryNameBytesField.GetValue(entry) is not byte[] storedEntryNameBytes) return;
            var entryName = Encoding.UTF8.GetString(storedEntryNameBytes);

            // set correct entry name
            if (_storedEntryNameField is null) return;
            _storedEntryNameField.SetValue(entry, entryName);
        }
    }
}
