using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json.Serialization;

namespace DataLibrary.Models;
public class FileModel
{
    public IBrowserFile? File { get; set; }

    [JsonPropertyName("assetID")]
    public string? AssetId { get; set; }
    public string? FileId { get; } = Guid.NewGuid().ToString();
    public string? UploadPath { get; set; }
    public string? ZipPath { get; set; }
    public string? CsvPath { get; set; }

    [JsonPropertyName("uploadUri")]
    public string? UploadUri { get; set; }

    [JsonPropertyName("downloadUri")]
    public string? DownloadUri { get; set; }
    public Uri? DownloadCheckLocation { get; set; }
}
