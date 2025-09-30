namespace Cache.Core;

public class InMemoryCacheOptions
{
    public const string Name = "InMemoryCache";

    public long MaxNumItems { get; set; } = 100;
}