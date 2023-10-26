using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Setting;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Diagnostics;

namespace NeeView.Setting
{
    /// <summary>
    /// 設定画面 Model
    /// </summary>
    public class SettingWindowModel : BindableBase
    {
        private class SettingItemRecord
        {
            public SettingItemRecord(SettingPage page, SettingItemSection section, SettingItem item)
            {
                Debug.Assert(page != null && section != null && item != null);
                Page = page;
                Section = section;
                Item = item;
            }

            public SettingPage Page { get; }

            public SettingItemSection Section { get; }

            public SettingItem Item { get; }

            public string GetSearchText()
            {
                return Page.GetSearchText() + " " + Section.GetSearchText() + " " + Item.GetSearchText();
            }
        }


        private static Type? _latestSelectedPageType;

        private readonly List<SettingPage> _pages;
        private readonly List<SettingItemRecord> _records;


        public SettingWindowModel()
        {
            _pages = new List<SettingPage>
            {
                new SettingPageGeneral(),
                new SettingPageFileTypes(),
                new SettingPageWindow(),
                new SettingPagePanels(),
                new SettingPageSlideshow(),
                new SettingPageManipulate(),
                new SettingPageBook(),
                new SettingPageHistory(),
                new SettingPageCommand()
            };

            _latestSelectedPageType = _latestSelectedPageType ?? typeof(SettingPageGeneral);
            var page = GetSettingPagesEnumerator(_pages).FirstOrDefault(e => e.GetType() == _latestSelectedPageType);
            if (page != null)
            {
                page.IsSelected = true;
            }

            _records = CreateSettingItemRecordList(_pages);
        }


        public List<SettingPage> Pages
        {
            get { return _pages; }
        }

        public SettingPage SearchPage { get; } = new SettingPage(Properties.Resources.SettingPage_SearchResult);



        private List<SettingItemRecord> CreateSettingItemRecordList(IEnumerable<SettingPage> pages)
        {
            var list = new List<SettingItemRecord>();

            foreach (var page in GetSettingPagesEnumerator(pages))
            {
                if (page.Items != null)
                {
                    foreach (var item in page.Items)
                    {
                        var section = item as SettingItemSection;
                        Debug.Assert(section != null);

                        foreach (var child in section.Children)
                        {
                            list.Add(new SettingItemRecord(page, section, child));
                        }
                    }
                }
            }

            return list;
        }


        public void SetSelectedPage(SettingPage? page)
        {
            if (page == null) return;
            _latestSelectedPageType = page != SearchPage ? page.GetType() : null;
        }

        private IEnumerable<SettingPage> GetSettingPagesEnumerator()
        {
            return GetSettingPagesEnumerator(_pages);
        }

        private IEnumerable<SettingPage> GetSettingPagesEnumerator(IEnumerable<SettingPage> pages)
        {
            if (pages == null) yield break;

            foreach (var page in pages)
            {
                yield return page;
                if (page.Children != null)
                {
                    foreach (var child in GetSettingPagesEnumerator(page.Children))
                    {
                        yield return child;
                    }
                }
            }
        }

        public void ClearPageContentCache()
        {
            SearchPage.ClearContentCache();

            foreach (var page in GetSettingPagesEnumerator())
            {
                page.ClearContentCache();
            }
        }

        public void UpdateSearchPage(string keyword)
        {
            var _searchKeywordTokens = keyword.Split(' ')
                .Select(e => NeeLaboratory.IO.Search.StringUtils.ToNormalizedWord(e.Trim(), true))
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();

            var items = new List<SettingItem>();

            if (_searchKeywordTokens.Any())
            {
                var groups = _records
                    .Where(e => IsMatch(e, _searchKeywordTokens))
                    .GroupBy(e => e.Section);

                if (groups.Any())
                {
                    items.Add(new SettingItemSection(Properties.Resources.SettingPage_SearchResult));
                    foreach (var group in groups)
                    {
                        var section = new SettingItemSection(group.Key.Header, group.Key.Tips);
                        section.Children.AddRange(group.Select(e => e.Item.SearchResultItem));
                        items.Add(section);
                    }
                }
                else
                {
                    items.Add(new SettingItemSection(Properties.Resources.SettingPage_SearchResult_NotFound));
                }
            }
            else
            {
                items.Add(new SettingItemSection(Properties.Resources.SettingPage_SearchResult_NotFound));
            }

            SearchPage.SetItems(items);


            bool IsMatch(SettingItemRecord record, List<string> tokens)
            {
                var text = NeeLaboratory.IO.Search.StringUtils.ToNormalizedWord(record.GetSearchText(), true);
                return tokens.All(e => text.Contains(e, StringComparison.CurrentCulture));
            }
        }

    }
}
