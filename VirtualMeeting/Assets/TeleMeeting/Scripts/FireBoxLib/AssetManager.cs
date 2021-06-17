using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AssetManager
{
    private readonly string _base_url;
    private readonly List<Asset> _assets = new List<Asset>();
    private readonly Dictionary<string, byte[]> _data_cache = new Dictionary<string, byte[]>();

    public AssetManager(string base_url)
    {
        // Make sure we always have a trailing slash
        if (!base_url.EndsWith("/"))
        {
            base_url += "/";
        }

        this._base_url = base_url;
    }

    public void AddAsset(Asset asset)
    {
        this._assets.Add(asset);
    }

    public Asset FindAssetByID(string id)
    {
        foreach (Asset asset in this._assets)
        {
            if (asset.ID == id)
            {
                return asset;
            }
        }

        return null;
    }

    public bool RetrieveAssetData(string src_path, out byte[] data)
    {
        // Check if the source path is relative or not and correct/canonicalize it
        if (!IsAbsoluteUrl(src_path) && !this._base_url.StartsWith("file://"))
        {
            src_path = this.CanonicalizeUrl(src_path);
        }

        // Try and get the data from the cache first
        if (this._data_cache.TryGetValue(src_path, out data))
        {
            Debug.Log("Got existing item from data cache: " + src_path);
            return true;
        }

        // If we're getting this from an FBArc, don't pull from http.
        // We can still reference http objects so we need to check if we
        // didn't canonicalize the URL first, which represents a web asset
        if (!IsAbsoluteUrl(src_path) && this._base_url.StartsWith("file://"))
        {
            Debug.Log("Getting asset from local archive: " + src_path);
            Stream stream = FireBoxController.Instance.GetArchiveLoader().GetResource(src_path);

            if (stream == null)
            {
                return false;
            }

            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            data = ms.ToArray();
            
            // We don't cache file:// paths
            return true;
        }

        // Try and download the data from the web and then cache it
        try
        {
            Debug.Log("Downloading new item to asset cache: " + src_path);
            data = new System.Net.WebClient().DownloadData(src_path);

            // Add the data to the cache for future look-ups
            this._data_cache.Add(src_path, data);
            return true;
        } catch
        {
            Debug.LogWarning("Cannot get asset over HTTP connection: " + src_path);
        }
        
        return false;
    }

    private bool IsAbsoluteUrl(string src_path)
    {
        return src_path.StartsWith("http://") || src_path.StartsWith("https://");
    }

    private string CanonicalizeUrl(string src_path)
    {
        // Url is relative, no need to grab base Uri of host
        if (!src_path.StartsWith("/"))
        {
            return this._base_url + src_path;
        }

        Uri uri = new Uri(src_path);
        return string.Format("{0}://{1}/", uri.Scheme, uri.Host) + "/" + src_path.TrimStart('/');
    }
}
