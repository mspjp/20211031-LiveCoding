#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"

using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

/// <summary>
/// Lineサーバから取得したStreamを指定ストレージにアップロード
/// </summary>
static async Task PutLineContentsToStorageAsync(Stream stream,string ContainerName,string PathWithFileName)
{
    // ストレージアクセス情報の作成
    var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureStorageAccount"));
    var blobClient = storageAccount.CreateCloudBlobClient();

    // retry設定 3秒3回
    blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

    var container = blobClient.GetContainerReference(ContainerName);

    await container.CreateIfNotExistsAsync();

    // ストレージアクセスポリシーの設定
    /*container.SetPermissions(
        new BlobContainerPermissions
        {
            PublicAccess = BlobContainerPublicAccessType.Off,
        });
*/
    // Blob へファイルをアップロード
    var blob = container.GetBlockBlobReference(PathWithFileName);

    await blob.UploadFromStreamAsync(stream);
}

/// <summary>
/// 指定ストレージからコンテンツを取得
/// </summary>
/// <returns>Stream</returns>
static async Task<Stream> GetLineContentsFromStorageAsync(string ContainerName, string PathWithFileName)
{
    // ストレージアクセス情報の作成
    var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureStorageAccount"));
    var blobClient = storageAccount.CreateCloudBlobClient();

    // retry設定 3秒3回
    blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

    var container = blobClient.GetContainerReference(ContainerName);

    // Blob からダウンロード
    var blob = container.GetBlockBlobReference(PathWithFileName);

    var memoryStream = new MemoryStream();
    
    await blob.DownloadToStreamAsync(memoryStream);
    
    return memoryStream;
}