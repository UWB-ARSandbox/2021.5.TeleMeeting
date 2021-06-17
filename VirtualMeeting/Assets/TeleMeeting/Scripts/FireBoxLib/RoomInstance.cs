using ASL;
using Dummiesman;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomInstance
{
    private readonly Dictionary<string, Asset> _roomAssets;

    private readonly List<Item> _roomItems;
    
    private RoomConfiguration _roomConfig;

    public RoomInstance()
    {
        this._roomAssets = new Dictionary<string, Asset>();
        this._roomItems = new List<Item>();
    }

    public void CreateRoom(string xml)
    {
        FireBoxParser fireBoxParser = new FireBoxParser(this);
        fireBoxParser.Parse(xml);

        // After everything has been parsed, begin generating the room
        BuildInitialRoom();
        AddItemsToRoom();

        // Re-synchronize ASL objects
        GameLiftManager.GetInstance().m_GameController.SyncronizeID(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    public void AddAsset(Asset asset)
    {
        this._roomAssets.Add(asset.ID, asset);
    }

    public void AddRoomItem(Item item)
    {
        this._roomItems.Add(item);
    }

    public void SetRoomConfiguration(RoomConfiguration config)
    {
        this._roomConfig = config;
    }

    private void BuildInitialRoom()
    {
        GameObject player = TeleMeeting.MainController.Player;

        // Set our initial room player position/orientation
        // This is adjusted to the initial player height set in the application
        player.transform.position =
            new Vector3(this._roomConfig.Pos.x,
            this._roomConfig.Pos.y + TeleMeeting.MainController.getInstance().InitialPlayerPosition.y,
            this._roomConfig.Pos.z);
        player.transform.forward = this._roomConfig.Fwd;
        player.transform.rotation = Quaternion.Euler(this._roomConfig.Rotation);

        // Finally, set skybox if applicable
        string[] skybox_ids = { this._roomConfig.Skybox_front_id, this._roomConfig.Skybox_back_id,
            this._roomConfig.Skybox_left_id, this._roomConfig.Skybox_right_id,
            this._roomConfig.Skybox_up_id, this._roomConfig.Skybox_down_id };
        
        if (Array.TrueForAll(skybox_ids, x => x != "" && this._roomAssets.ContainsKey(x)))
        {
            Debug.Log("Setting a new skybox for the environment");

            List<Texture2D> skybox_textures = new List<Texture2D>();

            foreach (string skybox_id in skybox_ids)
            {
                // Retrieve the image texture data
                FireBoxController.Instance.GetAssetManager()
                    .RetrieveAssetData(this._roomAssets[skybox_id].Src, out byte[] textureBytes);

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(textureBytes);

                skybox_textures.Add(tex);
            }

            Material result = new Material(Shader.Find("RenderFX/Skybox"));
            result.SetTexture("_FrontTex", skybox_textures[0]);
            result.SetTexture("_BackTex", skybox_textures[1]);
            result.SetTexture("_LeftTex", skybox_textures[2]);
            result.SetTexture("_RightTex", skybox_textures[3]);
            result.SetTexture("_UpTex", skybox_textures[4]);
            result.SetTexture("_DownTex", skybox_textures[5]);

            // Finally we set the new skybox to this texture
            RenderSettings.skybox = result;
        }
    }

    private void AddItemsToRoom()
    {
        foreach (Item item in this._roomItems)
        {
            switch (item.GetType().Name)
            {
                case nameof(ImageItem):
                    {
                        Debug.Log("Adding parsed image item into room scene...");
                        RoomAdd_Image((ImageItem)item);
                        break;
                    }

                case nameof(SoundItem):
                    {
                        Debug.Log("Adding parsed sound item into room scene...");
                        RoomAdd_Sound((SoundItem)item);
                        break;
                    }

                case nameof(ObjectItem):
                    {
                        Debug.Log("Adding parsed object item into room scene...");
                        RoomAdd_Object((ObjectItem)item);
                        break;
                    }

                case nameof(WhiteboardProjectorItem):
                    {
                        Debug.Log("Adding parsed whiteboard+projector item into room scene...");
                        RoomAdd_WhiteboardProjector((WhiteboardProjectorItem)item);
                        break;
                    }

                default:
                    Debug.LogError("Item in room items list is of an unknown type! " + item.GetType().Name);
                    break;
            }
        }
    }

    private void RoomAdd_Image(ImageItem item)
    {
        if (!this._roomAssets.ContainsKey(item.ID) || !(this._roomAssets[item.ID] is AssetImage))
        {
            Debug.LogWarning("Could not find matching asset reference for image: " + item.ID);
            return;
        }

        // Retrieve the image texture data
        FireBoxController.Instance.GetAssetManager()
            .RetrieveAssetData(this._roomAssets[item.ID].Src, out byte[] textureBytes);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(textureBytes);

        Material material = new Material(Shader.Find("Diffuse"))
        {
            mainTexture = tex
        };

        // Create our image plane
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

        plane.transform.position = item.Pos;
        plane.transform.forward = item.Fwd;
        plane.transform.rotation = Quaternion.Euler(item.Rotation);
        plane.transform.localScale = item.Scale;
        
        plane.GetComponent<Renderer>().material = material;
    }

    private void RoomAdd_Sound(SoundItem item)
    {
        if (!this._roomAssets.ContainsKey(item.ID) || !(this._roomAssets[item.ID] is AssetSound))
        {
            Debug.LogWarning("Could not find matching asset reference for sound: " + item.ID);
            return;
        }

        // Retrieve the sound data
        FireBoxController.Instance.GetAssetManager()
            .RetrieveAssetData(this._roomAssets[item.ID].Src, out byte[] soundBytes);

        GameObject soundObj = new GameObject("SoundObject");
        AudioSource source = soundObj.AddComponent(typeof(AudioSource)) as AudioSource;

        // Should we loop this audio or have it oneshot?
        source.loop = item.Loop;
        
        // FIXME: For some reason the audio doesn't seem to loop using this...
        source.clip = WavUtility.ToAudioClip(soundBytes);

        // Finally we can play it
        source.Play();
    }

    private void RoomAdd_Object(ObjectItem item)
    {
        if (!this._roomAssets.ContainsKey(item.ID) || !(this._roomAssets[item.ID] is AssetObject))
        {
            Debug.LogWarning("Could not find matching asset reference for object: " + item.ID);
            return;
        }

        AssetObject assetRef = (AssetObject)this._roomAssets[item.ID];

        // Retrieve the object data
        FireBoxController.Instance.GetAssetManager()
            .RetrieveAssetData(this._roomAssets[item.ID].Src, out byte[] objBytes);

        // Our new object to instantiate
        GameObject obj;

        if (assetRef.Mtl != "")
        {
            // Retrieve and apply the material data
            FireBoxController.Instance.GetAssetManager()
                .RetrieveAssetData(assetRef.Mtl, out byte[] mtlBytes);

            obj = new OBJLoader().Load(new MemoryStream(objBytes), new MemoryStream(mtlBytes));
        }
        else
        {
            // Object has no associated material
            obj = new OBJLoader().Load(new MemoryStream(objBytes));
        }

        if (assetRef.Tex != "")
        {
            // Retrieve and apply the texture data
            FireBoxController.Instance.GetAssetManager()
                .RetrieveAssetData(assetRef.Tex, out byte[] textureBytes);

            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(textureBytes);

            Material material = new Material(Shader.Find("Diffuse"))
            {
                mainTexture = tex
            };
            
            // WARNING: In OBJLoader the objects are children of the Wavefront object!
            obj.transform.GetChild(0).GetComponent<Renderer>().material = material;
        }

        obj.transform.position = item.Pos;
        obj.transform.forward = item.Fwd;
        obj.transform.rotation = Quaternion.Euler(item.Rotation);
        obj.transform.localScale = item.Scale;
        obj.SetActive(item.Visible);

        // Animate the object using the default (non-JS engine) first
        FBAnimatedObject anim = obj.AddComponent<FBAnimatedObject>();
        anim.Velocity = item.Vel;
        anim.Rotate_axis = item.Rotate_axis;
        anim.Rotate_deg_per_sec = item.Rotate_deg_per_sec;
    }

    private void RoomAdd_WhiteboardProjector(WhiteboardProjectorItem item)
    {
        GameObject wb = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/WhiteBoard+Projector"),
            item.Pos, Quaternion.identity) as GameObject;

        wb.transform.forward = item.Fwd;
        wb.transform.rotation = Quaternion.Euler(item.Rotation);
        wb.transform.localScale = new Vector3(0.1f, item.Scale.y, item.Scale.z);
    }
}
