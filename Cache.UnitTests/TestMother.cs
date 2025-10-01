namespace Cache.UnitTests;

internal static class TestMother
{
    public const string Key1 = "key1";
    public const string Key2 = "key2";
    public const string Key3 = "key3";
    public const string Value1 = "value1";
    public const string Value2 = "value2";
    public const string Value3 = "value3";
    public static TestObject ComplexKey = new("deepkey1");
    public static TestObject ComplexKeyCopy = new("deepkey1");

    internal record TestObject(string Value);
}
