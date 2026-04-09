using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.Tests.Infrastructure;

public static partial class BehaviorCatalog
{
    public static Task AssertCardBehavior<T>() where T : CardModel
    {
        BehaviorGoldenAssert.AssertType(typeof(T));
        return Task.CompletedTask;
    }

    public static Task AssertPowerBehavior<T>() where T : PowerModel
    {
        BehaviorGoldenAssert.AssertType(typeof(T));
        return Task.CompletedTask;
    }

    public static Task AssertRelicBehavior<T>()
    {
        BehaviorGoldenAssert.AssertType(typeof(T));
        return Task.CompletedTask;
    }
}
