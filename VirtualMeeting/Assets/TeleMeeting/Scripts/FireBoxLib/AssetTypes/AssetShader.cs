public class AssetShader : Asset
{
    public string Vertex_src;

    public AssetShader(string id, string src = "",
        string vertex_src = "")
        : base(id, src)
    {
        this.Vertex_src = vertex_src;
    }
}
