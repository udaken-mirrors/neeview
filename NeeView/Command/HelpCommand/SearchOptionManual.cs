﻿using System;
using System.Linq;
using System.Text;
using NeeLaboratory.IO.Search;
using NeeLaboratory.Text;
using NeeView.Text.SimpleHtmlBuilder;
using System.Diagnostics;

namespace NeeView
{
    public class SearchOptionManual
    {
        private static readonly string _template = """
            <h1>@_SearchManual.Title</h1>

            <h2>@_SearchManual.S1</h2>
            <p>
                <ul>
                    <li>@_SearchManual.S1.P1.I1</li>
                    <li>@_SearchManual.S1.P1.I2</li>
                    <li>@_SearchManual.S1.P1.I3</li>
                </ul>
            </p>

            <h3>@_SearchManual.S11</h3>

            [[ConjunctionAliasTable]]

            <h3>@_SearchManual.S12</h3>

            [[PropertyAliasTable]]

            <h3>@_SearchManual.S13</h3>

            [[MatchAliasTable]]

            <p>
                @_SearchManual.S13.P1
    
                <ul>
                    <li>@_SearchManual.S13.P1.I1</li>
                    <li>@_SearchManual.S13.P1.I2</li>
                    <li>@_SearchManual.S13.P1.I3</li>
                    <li>@_SearchManual.S13.P1.I4</li>
                    <li>@_SearchManual.S13.P1.I5</li>
                </ul>
            </p>

            <h3>@_SearchManual.S14</h3>

            <p>
                @_SearchManual.S14.P1
            </p>

            <p>
                @_SearchManual.S14.P2
            </p>

            <p>
                @_SearchManual.S14.P3
            </p>
    
            <h3>@_SearchManual.S15</h3>

            <p>
                @_SearchManual.S15.P1
            </p>
            <p>
                @_SearchManual.S15.P2
            </p>

            <hr/>
            <h2>@_SearchManual.S2</h2>

            @_SearchManual.S2.P1

            <h3>@_SearchManual.S21</h3>

            @_SearchManual.S21.P1
            
            <blockquote>@_SearchManual.S21.P2</blockquote>

            @_SearchManual.S21.P3

            <h3>@_SearchManual.S22</h3>

            @_SearchManual.S22.P1

            <h4>@_SearchManual.S22-1</h4>

            [[ConjunctionTable]]

            <h4>@_SearchManual.S22-2</h4>

            [[PropertyTable]]

            <h4>@_SearchManual.S22-3</h4>

            [[MatchTable]]

            <p>
              @_SearchManual.S22.P2
            </p>

            <p>
              @_SearchManual.S22.P3
              <blockquote>/p.date /m.lt</blockquote>
            </p>

            <p>
              @_SearchManual.S22.P4
              <blockquote>/p.date /m.fuzzy</blockquote>
            </p>

            <h3>@_SearchManual.S23</h3>

            @_SearchManual.S23.P1

            [[AliasTable]]

            <h3>@_SearchManual.S24</h3>

            <p>
                @_SearchManual.S24.P1
            </p>
                
            <p>
                @_SearchManual.S24.P2
                <blockquote>/p.meta.[key]</blockquote>
                @_SearchManual.S24.P3
            </p>

            <p>
                [[MetaTable]]
                @_SearchManual.S24.P4
            </p>

            <hr/>
            <h2>@_SearchManual.S3</h2>

            @_SearchManual.S3.P1
            <blockquote>ABC DEF</blockquote>

            @_SearchManual.S3.P2
            <blockquote>/re ^ABC$</blockquote>

            @_SearchManual.S3.P3
            <blockquote>"ABC DEF"</blockquote>

            @_SearchManual.S3.P4
            <blockquote>ABC /not /word DEF</blockquote>

            @_SearchManual.S3.P5
            <blockquote>/since 2019-04-01 /until 2019-05-01</blockquote>

            @_SearchManual.S3.P6
            <blockquote>/size /lt 10M</blockquote>

            @_SearchManual.S3.P7
            <blockquote>/bookmark ABC</blockquote>

            @_SearchManual.S3.P8
            <blockquote>/p.meta.parameters smile</blockquote>
            """;


        public static void OpenSearchOptionManual()
        {
            System.IO.Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "SearchOptions.html");

            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.WriteLine(CreateSearchOptionManual());
            }

            ExternalProcess.Start(fileName);
        }

        public static string CreateSearchOptionManual()
        {
            var builder = new StringBuilder();
            builder.AppendLine(HtmlHelpUtility.CreateHeader(ResourceService.GetString("@_SearchManual.Title")));
            builder.AppendLine($"<body>");

            var searchContext = new SearchContext()
             .AddProfile(new DateSearchProfile())
             .AddProfile(new SizeSearchProfile())
             .AddProfile(new BookSearchProfile())
             .AddProfile(new PageSearchProfile());

            var text = ResourceService.Replace(_template);
            text = text.Replace("[[ConjunctionAliasTable]]", CreateConjunctionAliasTable(searchContext));
            text = text.Replace("[[PropertyAliasTable]]", CreatePropertyAliasTable(searchContext));
            text = text.Replace("[[MatchAliasTable]]", CreateMatchAliasTable(searchContext));
            text = text.Replace("[[ConjunctionTable]]", CreateConjunctionTable(searchContext));
            text = text.Replace("[[PropertyTable]]", CreatePropertyTable(searchContext));
            text = text.Replace("[[MatchTable]]", CreateMatchTable(searchContext));
            text = text.Replace("[[AliasTable]]", CreateAliasTable(searchContext));
            text = text.Replace("[[MetaTable]]", CreateMetaTableString());
            builder.Append(text);

            builder.AppendLine("</body>");

            return builder.ToString();
        }


        // Alias: Conjunction Options
        private static string CreateConjunctionAliasTable(SearchContext searchContext)
        {
            var options = new string[] { "/and", "/or", "/not" };

            var node = new TagNode("table", "table-slim table-topless");

            node.AddNode(new TagNode("tr")
                .AddNode(new TagNode("th").AddText("@Word.Name"))
                .AddNode(new TagNode("th").AddText("@Word.Description")));

            foreach (var option in options)
            {
                Debug.Assert(searchContext.KeyAlias.Any(x => x.Key == option));

                var key = "@SearchOp.Conjunction." + option.TrimStart('/').ToTitleCase();

                node.AddNode(new TagNode("tr")
                    .AddNode(new TagNode("td").AddText(option))
                    .AddNode(new TagNode("td").AddText(key)));
            }

            return node.ToString();
        }

        // Alias: Property Options
        private static string CreatePropertyAliasTable(SearchContext searchContext)
        {
            var options = new string[] { "/text", "/date", "/size", "/bookmark", "/history", "/playlist", "/title", "/subject", "/rating", "/tags", "/comments" };

            var node = new TagNode("table", "table-slim table-topless");

            node.AddNode(new TagNode("tr")
                .AddNode(new TagNode("th").AddText("@Word.Name"))
                .AddNode(new TagNode("th").AddText("@Word.Description"))
                .AddNode(new TagNode("th").AddText("@Word.Supplement")));

            foreach (var option in options)
            {
                Debug.Assert(searchContext.KeyAlias.Any(x => x.Key == option));

                var key = "@SearchOp.Alias.Property." + option.TrimStart('/').ToTitleCase();

                node.AddNode(new TagNode("tr")
                    .AddNode(new TagNode("td", "nowrap").AddText(option))
                    .AddNode(new TagNode("td").AddText(key))
                    .AddNode(new TagNode("td").AddText(key + ".Remarks", ResourceService.ReplaceEmpty)));
            }

            node.AddNode(new TagNode("tr")
                .AddNode(new TagNode("td", "nowrap").AddText("/p.meta.[key]"))
                .AddNode(new TagNode("td").AddText("@SearchOp.Alias.Property.Meta"))
                .AddNode(new TagNode("td").AddText("@SearchOp.Alias.Property.Meta.Remarks")));

            return node.ToString();
        }

        // Alias: Match Options
        private static string CreateMatchAliasTable(SearchContext searchContext)
        {
            string[][] options = [["/m0", "/exact"], ["/m1", "/word"], ["/m2", "/fuzzy"], ["/re"], ["/ire"], ["/since"], ["/until"], ["/lt"], ["/le"], ["/eq"], ["/ne"], ["/ge"], ["/gt"]];

            var node = new TagNode("table", "table-slim table-topless");

            node.AddNode(new TagNode("tr")
                .AddNode(new TagNode("th").AddText("@Word.Name"))
                .AddNode(new TagNode("th").AddText("@Word.Description"))
                .AddNode(new TagNode("th").AddText("@Word.Supplement")));

            foreach (var option in options)
            {
                Debug.Assert(option.All(e => searchContext.KeyAlias.Any(x => x.Key == e)));

                var key = "@SearchOp.Alias.Match." + option[0].TrimStart('/').ToTitleCase();

                node.AddNode(new TagNode("tr")
                    .AddNode(new TagNode("td").AddText(string.Join(", ", option)))
                    .AddNode(new TagNode("td").AddText(key))
                    .AddNode(new TagNode("td").AddText(key + ".Remarks", ResourceService.ReplaceEmpty)));
            }

            return node.ToString();
        }

        // Conjunction Options (Detail)
        private static string CreateConjunctionTable(SearchContext searchContext)
        {
            var options = searchContext.KeyOptions.Where(e => e.Key.StartsWith("/c."));
            //var options = new string[] { "/and", "/or", "/not" };

            var node = new TagNode("table", "table-slim table-topless");

            node.AddNode(new TagNode("tr")
                .AddNode(new TagNode("th").AddText("@Word.Name"))
                .AddNode(new TagNode("th").AddText("@Word.Description")));

            foreach (var option in options)
            {
                var key = "@SearchOp.Conjunction." + option.Key[3..].ToTitleCase();
                node.AddNode(new TagNode("tr")
                    .AddNode(new TagNode("td").AddText(option.Key))
                    .AddNode(new TagNode("td").AddText(key)));
            }

            return node.ToString();
        }

        // Property Options (Detail)
        private static string CreatePropertyTable(SearchContext searchContext)
        {
            var options = searchContext.KeyOptions.Where(e => e.Value is PropertySearchKeyOption && e.Key != "/p.meta");

            var node = new TagNode("table", "table-slim table-topless");

            node.AddNode(new TagNode("tr")
                .AddNode(new TagNode("th").AddText("@Word.Name"))
                .AddNode(new TagNode("th").AddText("@Word.Description"))
                .AddNode(new TagNode("th").AddText("@Word.Type")));

            foreach (var option in options)
            {
                var profile = ((PropertySearchKeyOption)option.Value).Profile;

                var key = "@SearchOp.Property." + profile.Name.ToTitleCase();
                node.AddNode(new TagNode("tr")
                    .AddNode(new TagNode("td", "nowrap").AddText(option.Key))
                    .AddNode(new TagNode("td").AddText(key))
                    .AddNode(new TagNode("td").AddText(GetTypeString(profile.GetValueType()))));
            }

            // p.meta
            node.AddNode(new TagNode("tr")
                .AddNode(new TagNode("td", "nowrap").AddText("/p.meta.[key]"))
                .AddNode(new TagNode("td").AddText("@SearchOp.Property.Meta"))
                .AddNode(new TagNode("td").AddText(GetTypeString(typeof(string)))));

            return node.ToString();
        }

        private static string GetTypeString(Type type)
        {
            return Type.GetTypeCode(type) switch
            {
                TypeCode.Int64 => "Integer",
                _ => type.Name,
            };
        }

        // Match Options (Detail)
        private static string CreateMatchTable(SearchContext searchContext)
        {
            var options = searchContext.KeyOptions.Where(e => e.Value is FilterSearchKeyOption);

            var node = new TagNode("table", "table-slim table-topless");

            node.AddNode(new TagNode("tr")
                .AddNode(new TagNode("th").AddText("@Word.Name"))
                .AddNode(new TagNode("th").AddText("@Word.Description"))
                .AddNode(new TagNode("th").AddText("@Word.Type")));

            foreach (var option in options)
            {
                var profile = ((FilterSearchKeyOption)option.Value).Profile;

                var key = "@SearchOp.Match." + profile.Name.ToTitleCase();
                node.AddNode(new TagNode("tr")
                    .AddNode(new TagNode("td").AddText(option.Key))
                    .AddNode(new TagNode("td").AddText(key))
                    .AddNode(new TagNode("td").AddText(profile.IsValiant ? "@SearchOp.ValiantType" : "String")));
            }

            return node.ToString();
        }

        // Alias Options
        private static string CreateAliasTable(SearchContext searchContext)
        {
            var options = searchContext.KeyAlias;

            var node = new TagNode("table", "table-slim table-topless");

            node.AddNode(new TagNode("tr")
                .AddNode(new TagNode("th").AddText("@Word.Alias"))
                .AddNode(new TagNode("th").AddText("@Word.Decode")));

            foreach (var option in options)
            {
                node.AddNode(new TagNode("tr", "nowrap")
                    .AddNode(new TagNode("td").AddText(option.Key))
                    .AddNode(new TagNode("td").AddText(string.Join(" ", option.Value))));
            }

            return node.ToString();
        }

        private static string CreateMetaTableString()
        {
            var node = new TagNode("table")
                .AddAttribute("class", "table-slim table-topless");

            node.AddNode(new TagNode("tr")
                .AddNode(new TagNode("th").AddText("[key]"))
                .AddNode(new TagNode("th").AddText("@Word.Description")));

            foreach (var key in InformationKeyExtensions.DefaultKeys.Select(e => e.ToString()))
            {
                node.AddNode(new TagNode("tr")
                    .AddNode(new TagNode("td").AddText(key))
                    .AddNode(new TagNode("td").AddText("@InformationKey." + key)));
            }

            return node.ToString();
        }
    }

}

