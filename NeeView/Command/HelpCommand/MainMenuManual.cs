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
                writer.WriteLine(HtmlHelpUtility.CreateHeader("NeeView MainMenu List"));

                writer.WriteLine($"<body><h1>NeeView {Properties.TextResources.GetString("Word.MainMenu")}</h1>");

                foreach (var pair in groups)
                {
                    writer.WriteLine($"<h3>{pair.Key.Replace("_", "")}</h3>");
                    writer.WriteLine("<table class=\"table-slim\">");
                    foreach (var item in pair.Value)
                    {
                        string name = string.Concat(Enumerable.Repeat("&nbsp;", item.Depth * 2)) + item.Element.DispLabel;

                        writer.WriteLine($"<td>{name}<td>{item.Element.Note}<tr>");
                    }
                    writer.WriteLine("</table>");
                }
                writer.WriteLine("</body>");

                writer.WriteLine(HtmlHelpUtility.CreateFooter());
            }

            ExternalProcess.Start(fileName);
        }
    }
}


