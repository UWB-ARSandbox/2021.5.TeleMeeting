public class AssetWebSurface : Asset
{
    public int Width;

    public int Height;
    
    public bool Show_url_bar;

    public AssetWebSurface(string id, string src,
        int width, int height, bool show_url_bar = false)
        : base(id, src)
    {
        this.Width = width;
        this.Height = height;
        this.Show_url_bar = show_url_bar;
    }
}
