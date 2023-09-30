namespace System;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
public class StringValueAttribute : Attribute
{
    public string StringValue { get; protected set; }

    public StringValueAttribute(string value)
    {
        this.StringValue = value;
    }
}

public static class StringValueExtensions
{
    public static string? GetStringValue(this Enum value)
    {
        Type type = value.GetType();

        var fieldInfo = type.GetField(value.ToString());
        var attributes = fieldInfo?.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

        return attributes?.FirstOrDefault()?.StringValue;
    }
}