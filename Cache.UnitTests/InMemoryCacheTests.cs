using AwesomeAssertions;
using Cache.Core;
using Microsoft.Extensions.Logging;
using NSubstitute;

using static Cache.UnitTests.TestMother;

namespace Cache.UnitTests;

public class Tests
{
    private ILogger FakeLogger;

    [SetUp]
    public void Setup()
    {
        FakeLogger = Substitute.For<ILogger>();
    }

    [TestCase(Key1)]
    [TestCase(Key2)]
    public void Set_WhenLimitHit_EvictLastAccessedObject(string keyToEvict)
    {
        // arrange
        var cache = new InMemoryCache(FakeLogger, new InMemoryCacheOptions() { MaxNumItems = 2 });
        cache.Set(Key1, Value1);
        cache.Set(Key2, Value2);

        // act
        var keys = new List<string>() { Key1, Key2 }.First(x => x != keyToEvict);
        cache.TryGet(keys, out var _);
        cache.Set(Key3, Value3);
        var result = cache.TryGet(keyToEvict, out var _);

        // assert
        result.Should().BeFalse();
        FakeLogger.Received().LogInformation($"Evicted key: [{keyToEvict}]");
    }

    [Test]
    public void TryGet_WhenSameKeyUsed_OverwritesExistingEntry()
    {
        // arrange
        var cache = new InMemoryCache(FakeLogger);
        cache.Set(Key1, Value2);
        cache.Set(Key1, Value1);

        // act
        cache.TryGet(Key1, out var result);

        // assert
        result.Should().Be(Value1);
    }
}