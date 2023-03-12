namespace DataLibrary.Models;

public class BlobInfo
{
    public Stream Content { get; set; }
    public string ContentType { get; set; }
    public BlobInfo(Stream Content, string ContentType)
    {
        this.Content = Content;
        this.ContentType = ContentType;
    }
}
