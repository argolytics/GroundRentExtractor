using Azure.Storage.Blobs;
using Azure.AI.FormRecognizer;
using DataLibrary.Models;
using OpenQA.Selenium;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;

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
        catch (Azure.RequestFailedException)
        {
            return false;
        }
    }
    public async Task<bool> CategorizeBlob()
    {
        

        try
        {

            return true;
        }
        catch (Exception e)
        {
            Serilog.Log.Error($"{e.Message}");
            return false;
        }
    }
    public void TagBlob(string blobName, GroundRentPropertyModel propertyModel, GroundRentPdfModel pdfModel, string containerName)
    {
        try
        {
            var containerClient = new BlobContainerClient(_connectionString, containerName);
            var blob = containerClient.GetBlobClient(blobName);
            var dateOnlyFiled = DateOnly.FromDateTime((DateTime)pdfModel.DateTimeFiled);
            var dateOnlyUploaded  = DateOnly.FromDateTime(DateTime.Now);
            var blobTags = new Dictionary<string, string>
            {
                { "PropertyId", propertyModel.Id.ToString() },
                { "PdfId", pdfModel.Id.ToString() },
                { "County", propertyModel.County },
                { "AccountId", propertyModel.AccountId },
                { "DocumentFiledType", pdfModel.DocumentFiledType },
                { "AcknowledgementNumber", pdfModel.AcknowledgementNumber },
                { "AzureFormRecognizerCategory", string.Empty },
                { "DateFiled", dateOnlyFiled.ToString() },
                { "DateUploaded", dateOnlyUploaded.ToString() }
            };
            containerClient.GetBlobClient(blob.Name).SetTags(blobTags);
        }
        catch (Exception e)
        {
            Serilog.Log.Error($"{e.Message}");
        }
    }
}