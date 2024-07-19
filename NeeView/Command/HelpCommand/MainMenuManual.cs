using NeeView.Text.SimpleHtmlBuilder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public static class MainMenuManual
    {
        public static void OpenMainMenuManual()
        {
            var groups = new Dictionary<string, List<MenuTree.TableData>>();

            var children = MainMenu.Current.MenuSource.Children;
            if (children is null) throw new InvalidOperationException("MenuSource.Children must not be null");

            foreach (var group in children)
            {
                groups.Add(group.Label, group.GetTable(0));
            }

            System.IO.Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "MainMenuList.html");

            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                var title = "NeeView " + ResourceService.GetString("@Word.MainMenu");
                writer.WriteLine(HtmlHelpUtility.CreateHeader(title));

                var node = new TagNode("body");
                node.AddNode(new TagNode("h1").AddText(title));

                foreach (var pair in groups)
                {
                    node.AddNode(new TagNode("h3").AddText(pair.Key.Replace("_", "")));

                    var table = new TagNode("table", "table-slim");
                    foreach (var item in pair.Value)
                    {
                        string name = string.Concat(Enumerable.Repeat("&nbsp;", item.Depth * 2)) + item.Element.DispLabel;
                        table.AddNode(new TagNode("tr")
                            .AddNode(new TagNode("td").AddText(name))
                            .AddNode(new TagNode("td").AddText(item.Element.Note)));
                    }
                    node.AddNode(table);
                }

                writer.WriteLine(node.ToString());
                writer.WriteLine(HtmlHelpUtility.CreateFooter());
            }

            ExternalProcess.Start(fileName);
        }
    }
}


