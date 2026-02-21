using System;
using System.Text;

namespace Miyo.UI.MVVM
{
    /// <summary>
    /// ParentLoginViewModel → "parent-login"
    ///         HomeViewModel        → "home"
    ///         GameSelectViewModel  → "game-select"
    /// </summary>
    public static class ViewModelIdHelper
    {
        public static string GetId<TViewModel>() where TViewModel : ViewModelBase
            => GetId(typeof(TViewModel));

        public static string GetId(Type viewModelType)
        {
            var name = viewModelType.Name;

            if (name.EndsWith("ViewModel", StringComparison.Ordinal))
                name = name[..^"ViewModel".Length];

            return ToKebabCase(name);
        }

        private static string ToKebabCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var sb = new StringBuilder(input.Length + 4);
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) && i > 0)
                    sb.Append('-');
                sb.Append(char.ToLowerInvariant(input[i]));
            }
            return sb.ToString();
        }
    }
}
