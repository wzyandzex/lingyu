namespace Aetherion.Application.Ports
{
    public interface ILocalization
    {
        string Get(string key, string fallback = null);
    }
}
