namespace DataTouch.Web.Models;

public class PortfolioGalleryModel
{
    public bool EnablePhotos { get; set; } = true;
    public bool EnableVideos { get; set; } = false;
    public List<GalleryItemModel> Photos { get; set; } = new();
    public List<GalleryItemModel> Videos { get; set; } = new();
}

public class GalleryItemModel
{
    public string Url { get; set; } = "";
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int Order { get; set; }
}
