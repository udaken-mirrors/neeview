using NeeView.Windows.Property;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Resources;

namespace NeeView
{
    public class ScriptManual
    {
        public void OpenScriptManual()
        {
            Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "ScriptManual.html");

            // create html file
            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.Write(CreateScriptManualText());
            }

            // open in browser
            ExternalProcess.Start(fileName);
        }


        private string CreateScriptManualText()
        {
            var builder = new StringBuilder();

            builder.Append(HtmlHelpUtility.CreateHeader("NeeView Script Manual"));
            builder.Append($"<body>");

            builder.Append(Properties.TextResources.GetString("_Document.ScriptManual.html"));

            AppendScriptReference(builder);

            AppendConfigList(builder);

            AppendCommandList(builder);

            AppendObsoleteList(builder);

            builder.Append(Properties.TextResources.GetString("_Document.ScriptManualExample.html"));

            builder.Append("</body>");
            builder.Append(HtmlHelpUtility.CreateFooter());

            return builder.ToString();
        }

        private static StringBuilder AppendScriptReference(StringBuilder builder)
        {
            builder.Append($"<h1 class=\"sub\">{ResourceService.GetString("@ScriptReference")}</h1>");
            builder.Append($"<p>{ResourceService.GetString("@ScriptReference.Summary")}</p>").AppendLine();

            var htmlBuilder = new HtmlReferenceBuilder(builder);

            htmlBuilder.CreateMethods(typeof(JavaScriptEngine), null);

            htmlBuilder.Append($"<hr/>").AppendLine();

            htmlBuilder.Append(typeof(CommandHost), "nv");

            htmlBuilder.Append(typeof(CommandAccessor));

            htmlBuilder.Append(typeof(EnvironmentAccessor));
            htmlBuilder.Append(typeof(BookAccessor));
            htmlBuilder.Append(typeof(BookConfigAccessor));
            htmlBuilder.Append(typeof(PageAccessor));
            htmlBuilder.Append(typeof(ViewPageAccessor));

            htmlBuilder.Append(typeof(BookshelfPanelAccessor));
            htmlBuilder.Append(typeof(BookshelfItemAccessor));

            htmlBuilder.Append(typeof(PageListPanelAccessor));

            htmlBuilder.Append(typeof(BookmarkPanelAccessor));
            htmlBuilder.Append(typeof(BookmarkItemAccessor));

            htmlBuilder.Append(typeof(PlaylistPanelAccessor));
            htmlBuilder.Append(typeof(PlaylistItemAccessor));

            htmlBuilder.Append(typeof(HistoryPanelAccessor));
            htmlBuilder.Append(typeof(HistoryItemAccessor));

            htmlBuilder.Append(typeof(InformationPanelAccessor));

            htmlBuilder.Append(typeof(EffectPanelAccessor));

            htmlBuilder.Append(typeof(NavigatorPanelAccessor));

            htmlBuilder.Append($"<hr/>").AppendLine();

            htmlBuilder.Append(typeof(AutoRotateType));
            htmlBuilder.Append(typeof(FolderOrder));
            htmlBuilder.Append(typeof(PageSortMode));
            htmlBuilder.Append(typeof(PageNameFormat));
            htmlBuilder.Append(typeof(PageReadOrder));
            htmlBuilder.Append(typeof(PanelListItemStyle));

            return htmlBuilder.ToStringBuilder();
        }

        private static StringBuilder AppendConfigList(StringBuilder builder)
        {
            builder.Append($"<h1 class=\"sub\" id=\"ConfigList\">{Properties.TextResources.GetString("Word.ConfigList")}</h1>");
            builder.Append("<table class=\"table-slim table-topless\">");
            builder.Append($"<tr><th>{Properties.TextResources.GetString("Word.Name")}</th><th>{Properties.TextResources.GetString("Word.Type")}</th><th>{Properties.TextResources.GetString("Word.Summary")}</th></tr>");
            builder.Append(new ConfigMap(null).Map.CreateHelpHtml("nv.Config"));
            builder.Append("</table>");
            return builder;
        }

        private StringBuilder AppendCommandList(StringBuilder builder)
        {
            var executeMethodArgTypes = new Type[] { typeof(object), typeof(CommandContext) };

            builder.Append($"<h1 class=\"sub\" id=\"CommandList\">{Properties.TextResources.GetString("Word.CommandList")}</h1>");
            builder.Append("<table class=\"table-slim table-topless\">");
            builder.Append($"<tr><th>{Properties.TextResources.GetString("Word.Group")}</th><th>{Properties.TextResources.GetString("Word.Command")}</th><th>{Properties.TextResources.GetString("Word.CommandName")}</th><th>{Properties.TextResources.GetString("Word.Argument")}</th><th>{Properties.TextResources.GetString("Word.CommandParameter")}</th><th>{Properties.TextResources.GetString("Word.Summary")}</th></tr>");
            foreach (var command in CommandTable.Current.Values.OrderBy(e => e.Order))
            {
                string argument = "";
                {
                    var type = command.GetType();
                    var info = type.GetMethod(nameof(command.Execute), executeMethodArgTypes) ?? throw new InvalidOperationException();
                    var attribute = (MethodArgumentAttribute?)Attribute.GetCustomAttributes(info, typeof(MethodArgumentAttribute)).FirstOrDefault();
                    if (attribute != null)
                    {
                        var tokens = MethodArgumentAttributeExtensions.GetMethodNote(info, attribute)?.Split('|');
                        int index = 0;
                        argument += "<dl>";
                        while (tokens is not null && index < tokens.Length)
                        {
                            var dt = tokens.ElementAtOrDefault(index++);
                            var dd = tokens.ElementAtOrDefault(index++);
                            argument += $"<dt>{dt}</dt><dd>{dd}</dd>";
                        }
                        argument += "</dl>";
                    }
                }

                string properties = "";
                if (command.Parameter != null)
                {
                    var type = command.Parameter.GetType();
                    var title = "";

                    if (command.Share != null)
                    {
                        properties = "<p style=\"color:red\">" + string.Format(Properties.TextResources.GetString("CommandParameter.Share"), command.Share.Name) + "</p>";
                    }

                    foreach (PropertyInfo info in type.GetProperties())
                    {
                        var attribute = (PropertyMemberAttribute?)Attribute.GetCustomAttributes(info, typeof(PropertyMemberAttribute)).FirstOrDefault();
                        if (attribute != null && attribute.IsVisible)
                        {
                            var titleString = PropertyMemberAttributeExtensions.GetPropertyTitle(info, attribute);
                            if (titleString != null)
                            {
                                title = titleString + " / ";
                            }

                            var enums = "";
                            if (info.PropertyType.IsEnum)
                            {
                                enums = string.Join(" / ", info.PropertyType.VisibleAliasNameDictionary().Select(e => $"\"{e.Key}\": {e.Value}")) + "<br/>";
                            }

                            var propertyName = PropertyMemberAttributeExtensions.GetPropertyName(info, attribute).TrimEnd(Properties.TextResources.GetString("Word.Period").ToArray()) + Properties.TextResources.GetString("Word.Period");
                            var text = title + propertyName;

                            var propertyTips = PropertyMemberAttributeExtensions.GetPropertyTips(info, attribute);
                            if (propertyTips != null)
                            {
                                text = text + " " + propertyTips;
                            }

                            properties = properties + $"<dt><b>{info.Name}</b>: {info.PropertyType.ToManualString()}</dt><dd>{enums + text}<dd/>";
                        }
                    }
                    if (!string.IsNullOrEmpty(properties))
                    {
                        properties = "<dl>" + properties + "</dl>";
                    }
                }

                builder.Append($"<tr><td>{command.Group}</td><td>{command.Text}</td><td><b>{command.Name}</b></td><td>{argument}</td><td>{properties}</td><td>{command.Remarks}</td></tr>");
            }
            builder.Append("</table>");

            return builder;
        }


        private static StringBuilder AppendObsoleteList(StringBuilder builder)
        {
            builder.Append($"<h1 class=\"sub\" id=\"ObsoleteList\">{Properties.TextResources.GetString("Word.ObsoleteList")}</h1>");

            var commandHost = new CommandHost();
            var root = ScriptNodeTreeBuilder.Create(commandHost, "nv");

            var groups = root.GetUnitEnumerator(null)
                .Where(e => e.Node.Obsolete != null)
                .GroupBy(e => e.Node.Alternative?.Version)
                .OrderBy(e => e.Key);

            // ver.40 and later
            foreach (var group in groups.Where(e => e.Key >= 40))
            {
                builder.Append($"<h2>Version {group.Key}.0</h2>");
                builder.Append("<table class=\"table-slim table-topless\">");
                builder.Append($"<tr><th>{Properties.TextResources.GetString("Word.Name")}</th><th>{Properties.TextResources.GetString("Word.Alternative")}</th></tr>");
                foreach (var unit in group.OrderBy(e => e.FullName))
                {
                    builder.Append($"<tr><td>{unit.FullName}</td><td>{unit.Alternative}</td>");
                }
                builder.Append("</table>");
            }

            return builder;
        }
    }
}
