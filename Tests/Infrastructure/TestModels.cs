using MegaCrit.Sts2.Core.Models;

namespace TheArchitect.Tests.Infrastructure;

/// <summary>
/// Canonical models live in <see cref="ModelDb"/>, so tests must never construct
/// them directly: a second instance of a registered model throws
/// "duplicate canonical model". These helpers hand out the registered canonical
/// instance, or a mutable clone when the test needs to mutate it.
/// </summary>
public static class TestModels
{
    public static T Card<T>() where T : CardModel => ModelDb.Card<T>();

    public static T MutableCard<T>() where T : CardModel => (T)ModelDb.Card<T>().MutableClone();

    public static T Relic<T>() where T : RelicModel => ModelDb.Relic<T>();

    public static T MutableRelic<T>() where T : RelicModel => (T)ModelDb.Relic<T>().MutableClone();
}
