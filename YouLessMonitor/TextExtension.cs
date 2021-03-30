using System;
using System.IO;
using System.Windows.Markup;

namespace YouLessMonitor
{
    public class TextExtension : MarkupExtension
    {
        private readonly string _FileName;

        public TextExtension(string fileName)
        {
            _FileName = fileName;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            try
            {
                return File.ReadAllText(_FileName);
            }
            catch { }
            return $"Could not load {_FileName}";
        }
    }
}