using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView.Susie
{
    public static class SusieCommandSerializer
    {
        public static byte[] Serialize<T>(T data)
        {
            return JsonSerializer.SerializeToUtf8Bytes(data, typeof(T), SusiePluginCommandJsonSerializerContext.Default);
        }

        public static T Deserialize<T>(byte[] source)
            where T : class
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            return JsonSerializer.Deserialize(source, typeof(T), SusiePluginCommandJsonSerializerContext.Default) as T ?? throw new FormatException();
        }
    }


    [JsonSourceGenerationOptions(IgnoreReadOnlyProperties = true)]
    [JsonSerializable(typeof(SusiePluginCommandResult))]
    [JsonSerializable(typeof(SusiePluginCommandInitialize))]
    [JsonSerializable(typeof(SusiePluginCommandGetPlugin))]
    [JsonSerializable(typeof(SusiePluginCommandGetPluginResult))]
    [JsonSerializable(typeof(SusiePluginCommandSetPlugin))]
    [JsonSerializable(typeof(SusiePluginCommandSetPluginOrder))]
    [JsonSerializable(typeof(SusiePluginCommandShowConfigurationDlg))]
    [JsonSerializable(typeof(SusiePluginCommandGetArchivePlugin))]
    [JsonSerializable(typeof(SusiePluginCommandGetArchivePluginResult))]
    [JsonSerializable(typeof(SusiePluginCommandGetImagePlugin))]
    [JsonSerializable(typeof(SusiePluginCommandGetImagePluginResult))]
    [JsonSerializable(typeof(SusiePluginCommandGetImage))]
    [JsonSerializable(typeof(SusiePluginCommandGetImageResult))]
    [JsonSerializable(typeof(SusiePluginCommandGetArchiveEntries))]
    [JsonSerializable(typeof(SusiePluginCommandGetArchiveEntriesResult))]
    [JsonSerializable(typeof(SusiePluginCommandExtractArchiveEntry))]
    [JsonSerializable(typeof(SusiePluginCommandExtractArchiveEntryToFolder))]
    [JsonSerializable(typeof(string))]
    public partial class SusiePluginCommandJsonSerializerContext : JsonSerializerContext
    {
    }
}
