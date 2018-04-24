
namespace XenoEngine.GeneralSystems
{
    public interface ISystemsProvider<TProvider>
    {
        TProvider SystemsProvider { get; }
    }

    public interface ISystems { }
}
