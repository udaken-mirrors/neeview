namespace NeeView
{
    public class ConfigMap
    {
        public ConfigMap(IAccessDiagnostics? accessDiagnostics)
        {
            Map = new PropertyMap("nv.Config", NeeView.Config.Current, accessDiagnostics);

            // エクスプローラーのコンテキストメニューへの追加フラグ
            if (Environment.IsZipLikePackage)
            {
                ((PropertyMap?)Map[nameof(NeeView.Config.System)])?.AddProperty(ExplorerContextMenu.Current, nameof(ExplorerContextMenu.IsEnabled), "IsExplorerContextMenuEnabled");
            }
        }

        public PropertyMap Map { get; private set; }
    }
}
