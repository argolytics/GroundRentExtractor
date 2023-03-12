using Azure.Storage.Blobs;
using OpenQA.Selenium;

namespace DataLibrary.Services;

public class BlobService
{
    private readonly string _connectionString;

    public BlobService(string connectionString)
    {
        _connectionString = connectionString;
    }
    public async Task<bool> UploadBlob(string blobName, PrintDocument printDocument, string containerName)
    {
        try
        {
            var containerClient = new BlobContainerClient(_connectionString, containerName);
            var blob = containerClient.GetBlobClient(blobName);
            Stream stream = new MemoryStream(printDocument.AsByteArray);
            await blob.UploadAsync(stream);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
