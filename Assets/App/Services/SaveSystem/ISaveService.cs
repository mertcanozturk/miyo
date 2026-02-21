using Cysharp.Threading.Tasks;

namespace Miyo.Services.Save
{
    public interface ISaveService
    {
        /// <summary>
        /// Veriyi JSON'a dönüştürüp Cloud Save'e kaydeder.
        /// </summary>
        UniTask SaveAsync<T>(string key, T data);

        /// <summary>
        /// Cloud Save'den veriyi okuyup T'ye deserialize eder.
        /// Kayıt yoksa defaultValue döner.
        /// </summary>
        UniTask<T> LoadAsync<T>(string key, T defaultValue = default);

        /// <summary>Verilen key için kayıt var mı?</summary>
        UniTask<bool> ExistsAsync(string key);

        /// <summary>Verilen key'e ait kaydı siler.</summary>
        UniTask DeleteAsync(string key);

        /// <summary>Tüm kayıtları siler.</summary>
        UniTask DeleteAllAsync();

        /// <summary>T'yi JSON'a çevirip UTF-8 byte dizisi olarak döner.</summary>
        byte[] Serialize<T>(T data);

        /// <summary>UTF-8 byte dizisini JSON üzerinden T'ye deserialize eder.</summary>
        T Deserialize<T>(byte[] bytes);
    }
}
