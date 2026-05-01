using System.Collections;
using System.Reflection;

namespace RomanaWeb.Tests;

/// <summary>
/// Anonymous types declared in another assembly are emitted with internal visibility,
/// so 'dynamic' access from xUnit fails with RuntimeBinderException. This helper
/// reads properties via reflection.
/// </summary>
internal static class ObjectAccess
{
    public static T Get<T>(object source, string propertyName)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        var prop = source.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop == null)
            throw new MissingMemberException(source.GetType().FullName, propertyName);
        var value = prop.GetValue(source);
        if (value is null) return default!;
        if (value is T tv) return tv;
        return (T)Convert.ChangeType(value, typeof(T))!;
    }
}
