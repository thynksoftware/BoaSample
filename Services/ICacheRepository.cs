namespace Boa.Sample.Services
{
    public interface ICacheRepository
    {
        void ClearAll();

        T Get<T>(string key);

        T Put<T>(string key, T item, CacheRepositoryOptions cacheRepositoryOptions, string category = "");

        void Remove(string key, bool contains = false);

        void RemoveByCategory(string category);

        void CheckExpiredCache();
    }
}