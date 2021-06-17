public class AssetImage : Asset
{
    public bool Sbs3d;

    public bool Ou3d;

    public bool Reverse3d;

    public bool Tex_clamp;

    public bool Tex_linear;

    public bool Tex_compress;

    public string Tex_alpha;

    public bool Tex_premultiply;

    public string Tex_colorspace;

    public AssetImage(string id, string src,
        bool sbs3d = false, bool ou3d = false, bool reverse3d = false,
        bool tex_clamp = false, bool tex_linear = true, bool tex_compress = false,
        string tex_alpha = "undefined", bool tex_premultiply = true, string tex_colorspace = "sRGB")
        : base(id, src)
    {
        this.Sbs3d = sbs3d;
        this.Ou3d = ou3d;
        this.Reverse3d = reverse3d;
        this.Tex_clamp = tex_clamp;
        this.Tex_linear = tex_linear;
        this.Tex_compress = tex_compress;
        this.Tex_alpha = tex_alpha;
        this.Tex_premultiply = tex_premultiply;
        this.Tex_colorspace = tex_colorspace;
    }
}
