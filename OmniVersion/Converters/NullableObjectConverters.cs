using Avalonia.Data.Converters;

namespace OmniVersion.Converters;

public static class NullableObjectConverters
{
    public static readonly FuncValueConverter<object?, bool> IsNotNull = new(x => x != null);
}