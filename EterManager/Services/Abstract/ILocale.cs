// ReSharper disable once CheckNamespace
namespace EterManager.Services
{
    public interface ILocale
    {
        /// <summary>
        /// Returns locale string based on identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        string GetString(string identifier);

    }
}