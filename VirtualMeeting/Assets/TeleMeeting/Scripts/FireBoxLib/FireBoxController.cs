using Dummiesman;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TeleMeeting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FireBoxController : MonoBehaviour
{
    public static FireBoxController Instance { get; private set; }

    private AssetManager _sceneDownloader = null;
    private AssetManager _assetManager = null;
    private FBArcLoader _fbArcLoader = null;

    // Our scene ID we are currently using
    private string _sceneID = "";
    
    private const string FIREBOX_SCENE = "FireBoxInstance";

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        this._sceneDownloader = new AssetManager("http://teleapi.joshumax.me/rooms/getBlob/");
        this._assetManager = new AssetManager("file://");
        this._fbArcLoader = new FBArcLoader();

        Communicator.Instance.RegisterPacketListener(PacketList.LOAD_SCENE, OnNewSceneRequested);
        
        //SceneSwitchingController.Instance.switchHandler.AddSceneSwitchCallback(LoadScene);
        foreach(SceneSwitchingController controller in SceneSwitchingController.Instances)
        {
            controller.switchHandler.AddSceneSwitchCallback(LoadScene);
        }
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SceneManager.GetActiveScene().name != FIREBOX_SCENE)
        {
            return;
        }

        if (!this._sceneDownloader.RetrieveAssetData(this._sceneID, out byte[] fbArc))
        {
            Debug.LogError("Could not retrieve scene " + this._sceneID + " from API! Bailing out!");
            return;
        }

        // Load the FBArc container into the archive manager which AssetManager connects to
        this._fbArcLoader.LoadArchiveFromStream(new MemoryStream(fbArc));

        // Finally create our room instance based on the root XML file in the archive
        if (!this._assetManager.RetrieveAssetData("index.xml", out byte[] sceneXML))
        {
            Debug.LogError("Could not open index.xml for " + this._sceneID + "! Bailing out!");
            return;
        }
        
        // Now we can actually start generating the room! Hooray!
        new RoomInstance().CreateRoom(System.Text.Encoding.Default.GetString(sceneXML));
    }
    
    public void LoadScene(string sceneName)
    {
        Communicator.Instance.SendPacket(new LoadScenePacket(sceneName));
    }
    
    private void OnNewSceneRequested(BasePacket packet)
    {
        LoadScenePacket scene_packet = (LoadScenePacket)packet;
        Debug.Log("Got packet scene switch request: " + scene_packet.SceneID);

        // Update the scene ID we want to get from the remote source
        this._sceneID = scene_packet.SceneID;
        SceneManager.LoadScene(FIREBOX_SCENE);
    }
    
    public AssetManager GetAssetManager()
    {
        return this._assetManager;
    }

    public FBArcLoader GetArchiveLoader()
    {
        return this._fbArcLoader;
    }
}
