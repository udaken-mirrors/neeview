using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class TitleString : BindableBase
    {
        private readonly TitleStringService _titleStringService;
        private string _format = "";
        private List<string> _keys = new();
        private string _title = "";

        private readonly List<string> _keywords = new()
        {
            "$Book",
            "$PageMax",
            "$Page",
            "$PageL",
            "$PageR",
            "$FullPath",
            "$FullPathL",
            "$FullPathR",
            "$FullName",
            "$FullNameL",
            "$FullNameR",
            "$Name",
            "$NameL",
            "$NameR",
            "$SizeEx",
            "$SizeExL",
            "$SizeExR",
            "$Size",
            "$SizeL",
            "$SizeR",
            "$ViewScale",
            "$Scale",
            "$ScaleL",
            "$ScaleR",
        };

        public TitleString(TitleStringService titleStringService)
        {
            _titleStringService = titleStringService;
            _titleStringService.Changed += TitleStringService_Changed;
        }


        public string Title
        {
            get { return _title; }
            private set { SetProperty(ref _title, value); }
        }


        private void TitleStringService_Changed(object? sender, EventArgs e)
        {
            UpdateTitle();
        }

        public void SetFormat(string format)
        {
            if (_format != format)
            {
                _format = format;
                _keys = _keywords.Where(e => format.Contains(e)).ToList();
                UpdateTitle();
            }
        }

        public void UpdateTitle()
        {
            Title = _titleStringService.Replace(_format, _keys);
        }
    }

}
