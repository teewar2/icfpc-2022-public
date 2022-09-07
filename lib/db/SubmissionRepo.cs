using System;
using System.Threading.Tasks;
using AspNetCore.Yandex.ObjectStorage;
using AspNetCore.Yandex.ObjectStorage.Configuration;

namespace lib.db;

public class SubmissionRepo
{
    private readonly Settings settings;

    public SubmissionRepo(Settings settings)
    {
        this.settings = settings;
    }

    public async Task PutFile(string name, byte[] content)
    {
        var storage = CreateYandexStorageService();
        var res = await storage.PutObjectAsync(content, name);
        Console.WriteLine(res.Error);
    }
    public async Task<byte[]> GetFile(string name)
    {
        var storage = CreateYandexStorageService();
        return await storage.GetAsByteArrayAsync(name);
    }

    private YandexStorageService CreateYandexStorageService()
    {
        var objectStorage = new YandexStorageService(new YandexStorageOptions
        {
            AccessKey = settings.YandexCloudStaticKeyId,
            SecretKey = settings.YandexCloudStaticKey,
            BucketName = "icfpc2022",
            Endpoint = "storage.yandexcloud.net",
            Location = "ru-central1-a",
            Protocol = "https"
        });
        return objectStorage;
    }
}
