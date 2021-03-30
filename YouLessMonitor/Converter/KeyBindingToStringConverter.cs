using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace YouLessMonitor
{
    public class KeyBindingToStringConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (KeyBinding)value;
            if (b.Modifiers == ModifierKeys.None) return b.Key.ToString();
            var modifiers = GetFlags<ModifierKeys>(b.Modifiers, ModifierKeys.None).
                            Select(CreateModifierString);
            return $"{String.Join("+", modifiers)}+{b.Key}";
        }

        private string CreateModifierString(ModifierKeys modifier)
        {
            switch (modifier)
            {
                case ModifierKeys.Alt: return ModifierKeys.Alt.ToString();
                case ModifierKeys.Control: return "Ctrl";
                case ModifierKeys.Shift: return ModifierKeys.Shift.ToString();
                case ModifierKeys.Windows: return "Win";
            }
            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        
        private static IEnumerable<T> GetFlags<T>(Enum input, Enum exclude = null) where T : Enum
        {
            return Enum.GetValues(input.GetType()).Cast<T>().Where(e => input.HasFlag(e) && !e.Equals(exclude));
        }
    }
}
