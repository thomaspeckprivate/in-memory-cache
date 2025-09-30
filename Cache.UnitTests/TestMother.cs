namespace Cache.UnitTests;

internal static class TestMother
{
    public const string Key1 = "key1";
    public const string Key2 = "key2";
    public const string Key3 = "key3";
    public const string Value1 = "value1";
    public const string Value2 = "value2";
    public const string Value3 = "value3";
    public static TestObject ValueDeepObject = new("value");
    public static List<TestObject> KeyDeepObjects = [new("deepkey1"), new("deepkey2")];
    public static List<TestObject> ValueDeepObjects = [new("deepvalue1"), new("deepvalue2")];

    internal record TestObject(string Value);
}
