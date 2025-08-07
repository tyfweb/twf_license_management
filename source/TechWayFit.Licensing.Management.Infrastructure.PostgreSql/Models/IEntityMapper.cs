public interface IEntityMapper<TSource, TDestination>
{
    /// <summary>
    /// Maps an entity of type TSource to TDestination.
    /// </summary>
    TDestination Map(TSource source);
    /// <summary>
    /// Maps an entity of type TDestination to TSource.
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    TSource Map();
}