public class AssetObject : Asset
{
    public string Tex;

    public string Mtl;

    public bool Tex_clamp;

    public bool Tex_linear;

    public bool Tex_compress;

    public bool Tex_mipmap;

    public string Tex_alpha;

    public bool Tex_premultiply;

    public string Tex_colorspace;

    public AssetObject(string id, string src,
        string tex = "", string mtl = "", bool tex_clamp = false,
        bool tex_linear = true, bool tex_compress = false, bool tex_mipmap = true,
        string tex_alpha = "undefined", bool tex_premultiply = true, string tex_colorspace = "sRGB")
        : base(id, src)
    {
        this.Tex = tex;
        this.Mtl = mtl;
        this.Tex_clamp = tex_clamp;
        this.Tex_linear = tex_linear;
        this.Tex_compress = tex_compress;
        this.Tex_mipmap = tex_mipmap;
        this.Tex_alpha = tex_alpha;
        this.Tex_premultiply = tex_premultiply;
        this.Tex_colorspace = tex_colorspace;
    }
}
