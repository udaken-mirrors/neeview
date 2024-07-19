using NeeView.Windows.Property;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NeeView
{
    public class ScriptManual
    {
        private static readonly string _manualTemplate = """
            <h1>@_ScriptManual.Title</h1>

            <h2>@_ScriptManual.S1</h2>
            <p>
                @_ScriptManual.S1.P1
            </p>
            <p>
                @_ScriptManual.S1.P2

                <ul>
                    <li>@_ScriptManual.S1.P2.I1</li>
                    <li>@_ScriptManual.S1.P2.I2</li>
                    <li>@_ScriptManual.S1.P2.I3</li>
                    <li>@_ScriptManual.S1.P2.I4</li>
                </ul>
            </p>
            <p>
                @_ScriptManual.S1.P3

                <table class="table-slim table-topless">
                    <tr><th>@_ScriptManual.S1.P3.T00</th><th>@_ScriptManual.S1.P3.T01</th></tr>
                    <tr><td>&#64;name</td><td>@_ScriptManual.S1.P3.T11</td></tr>
                    <tr><td>&#64;description</td><td>@_ScriptManual.S1.P3.T21</td></tr>
                    <tr><td>&#64;shortCutKey</td><td>@_ScriptManual.S1.P3.T31</td></tr>
                    <tr><td>&#64;mouseGesture</td><td>@_ScriptManual.S1.P3.T41</td></tr>
                    <tr><td>&#64;touchGesture</td><td>@_ScriptManual.S1.P3.T51</td></tr>
                </table>
            </p>

            <h2>@_ScriptManual.S2</h2>
            <p>
                @_ScriptManual.S2.P1

                <ul>
                    <li>@_ScriptManual.S2.P1.I1</li>
                    <li>@_ScriptManual.S2.P1.I2</li>
                </ul>
            </p>

            <h2>@_ScriptManual.S3</h2>
            <p>
                @_ScriptManual.S3.P1

                <table class="table-slim">
                    <tr><td>OnBookLoaded.nvjs</td><td>@_ScriptManual.S3.P1.T01</td></tr>
                    <tr><td>OnPageChanged.nvjs</td><td>@_ScriptManual.S3.P1.T11</td></tr>
                </table>
            </p>

            <h2>@_ScriptManual.S4</h2>
            <p>
                @_ScriptManual.S4.P1
            </p>

            <h4>@_ScriptManual.S4.T</h4>
            <p>
                <table class="table-slim">
                    <tr><td>cls</td><td>@_ScriptManual.S4.T.T01</td></tr>
                    <tr><td>exit</td><td>@_ScriptManual.S4.T.T11</td></tr>
                    <tr><td>help, ?</td><td>@_ScriptManual.S4.T.T21</td></tr>
                </table>
            </p>
            """;

        private static readonly string _exampleTemplate = """
            <h1 class="sub">@_ScriptManual.S9</h1>

            @_ScriptManual.S9.P1

            <h3>@_ScriptManual.S9-1</h3>
            <p>
              <pre>OpenMsPaint.nvjs<code class="example">@[/Resources/Scripts/OpenMsPaint.nvjs]</code></pre>
            </p>

            <h3>@_ScriptManual.S9-2</h3>
            <p>
              <pre>OpenNeeView.nvjs<code class="example">@[/Resources/Scripts/OpenNeeView.nvjs]</code></pre>
            </p>

            <h3>@_ScriptManual.S9-3</h3>
            <p>
             <pre>ToggleUnsharpMask.nvjs<code class="example">@[/Resources/Scripts/ToggleUnsharpMask.nvjs]</code></pre>
            </p>

            <h3>@_ScriptManual.S9-4</h3>
            <p>
              <pre>OnBookLoaded.nvjs<code class="example">@[/Resources/Scripts/OnBookLoaded.ReadOrder.nvjs]</code></pre>
            </p>

            <h3>@_ScriptManual.S9-5</h3>
            <p>
              <pre>OnBookLoaded.nvjs<code class="example">@[/Resources/Scripts/OnBookLoaded.Media.nvjs]</code></pre>
            </p>
            <p>
              <pre>ToggleFullScreenAndMediaPlay.nvjs<code class="example">@[/Resources/Scripts/ToggleFullScreenAndMediaPlay.nvjs]</code></pre>
            </p>
            """;

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

        private string GetScriptManualText()
        {
            return ResourceService.Replace(_manualTemplate);
        }

        private string GetScriptExampleText()
        {
            return ResourceService.Replace(_exampleTemplate);
        }

        private string CreateScriptManualText()
        {
            var builder = new StringBuilder();

            builder.Append(HtmlHelpUtility.CreateHeader(ResourceService.GetString("@_ScriptManual.Title")));
            builder.Append($"<body>");

            builder.Append(GetScriptManualText());

            AppendScriptReference(builder);

            AppendConfigList(builder);

            AppendCommandList(builder);

            AppendObsoleteList(builder);

            builder.Append(GetScriptExampleText());

            builder.Append("</body>");
            builder.Append(HtmlHelpUtility.CreateFooter());

            return builder.ToString();
        }

        private static StringBuilder AppendScriptReference(StringBuilder builder)
        {
            builder.Append($"<h1 class=\"sub\">{ResourceService.GetString("@_ScriptManual.S5")}</h1>");
            builder.Append($"<p>{ResourceService.GetString("@_ScriptManual.S5.P1")}</p>").AppendLine();

            var htmlBuilder = new HtmlReferenceBuilder(builder);

            htmlBuilder.CreateMethodTable(typeof(JavaScriptEngine), null);

            htmlBuilder.Append($"<hr/>").AppendLine();

            htmlBuilder.Append(typeof(CommandHost), "nv");

            var collection = DocumentableTypeCollector.Collect(typeof(CommandHost));

            foreach (var classType in collection.Where(e => !e.IsEnum).OrderBy(e => e.Name))
            {
                htmlBuilder.Append(classType);
            }

            htmlBuilder.Append($"<hr/>").AppendLine();

            foreach (var enumType in collection.Where(e => e.IsEnum).OrderBy(e => e.Name))
            {
                htmlBuilder.Append(enumType);
            }

            return htmlBuilder.ToStringBuilder();
        }

        private static StringBuilder AppendConfigList(StringBuilder builder)
        {
            builder.Append($"<h1 class=\"sub\" id=\"ConfigList\">{ResourceService.GetString("@_ScriptManual.S6")}</h1>");
            builder.Append("<table class=\"table-slim table-topless\">");
            builder.Append($"<tr><th>{Properties.TextResources.GetString("Word.Name")}</th><th>{Properties.TextResources.GetString("Word.Type")}</th><th>{Properties.TextResources.GetString("Word.Summary")}</th></tr>");
            builder.Append(new ConfigMap(null).Map.CreateHelpHtml("nv.Config"));
            builder.Append("</table>");
            return builder;
        }

        private StringBuilder AppendCommandList(StringBuilder builder)
        {
            var executeMethodArgTypes = new Type[] { typeof(object), typeof(CommandContext) };

            builder.Append($"<h1 class=\"sub\" id=\"CommandList\">{ResourceService.GetString("@_ScriptManual.S7")}</h1>");
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
            builder.Append($"<h1 class=\"sub\" id=\"ObsoleteList\">{ResourceService.GetString("@_ScriptManual.S8")}</h1>");

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
