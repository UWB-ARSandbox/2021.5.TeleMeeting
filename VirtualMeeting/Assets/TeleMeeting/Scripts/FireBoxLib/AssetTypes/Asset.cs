public abstract class Asset {
    public string ID
    {
        get;
        private set;
    }
    public string Src
    {
        get;
        private set;
    }

    public Asset(string id, string src)
    {
        this.ID = id;
        this.Src = src;
    }
}
