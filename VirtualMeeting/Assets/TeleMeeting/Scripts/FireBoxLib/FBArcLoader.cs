using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

public class FBArcLoader : IDisposable
{
    private ZipArchive _zipArchive;

    public bool LoadArchiveFromStream(Stream archive)
    {
        try
        {
            this._zipArchive = new ZipArchive(archive, ZipArchiveMode.Read);
        } catch
        {
            return false;
        }

        return true;
    }

    public Stream GetResource(string path)
    {
        if (this._zipArchive == null)
        {
            return null;
        }

        if (path.StartsWith("/"))
        {
            // Trim the leading forward slash(es) as it confuses the entry reader
            path = path.TrimStart('/');
        }

        try
        {
            ZipArchiveEntry entry = this._zipArchive.GetEntry(path);

            if (entry == null)
            {
                Debug.LogWarning("Could not find resource in archive: " + path);
                return null;
            }

            Debug.Log("Got resource in archive stream handle: " + path);
            return entry.Open();
        } catch
        {
            Debug.LogWarning("coud not open archive resource entry!");
        }

        return null;
    }

    public void Dispose()
    {
        if (this._zipArchive != null)
        {
            this._zipArchive.Dispose();
            this._zipArchive = null;
        }
    }
}
