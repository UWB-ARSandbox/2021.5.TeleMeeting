public class AssetVideo : Asset
{
    public bool Loop;

    public bool Auto_play;

    public bool Tex_compress;

    public bool Sbs3d;

    public bool Ou3d;

    public bool Reverse3d;

    public AssetVideo(string id, string src,
        bool loop = false, bool auto_play = false, bool tex_compress = false,
        bool sbs3d = false, bool ou3d = false, bool reverse3d = false)
        : base(id, src)
    {
        this.Loop = loop;
        this.Auto_play = auto_play;
        this.Tex_compress = tex_compress;
        this.Sbs3d = sbs3d;
        this.Ou3d = ou3d;
        this.Reverse3d = reverse3d;
    }
}
