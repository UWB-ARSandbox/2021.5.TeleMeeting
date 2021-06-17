using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

public class FireBoxParser
{
    private readonly RoomInstance _room;

    // Asset constants
    private const string ASSET_IMAGE = "AssetImage";
    private const string ASSET_OBJECT = "AssetObject";
    private const string ASSET_SCRIPT = "AssetScript";
    private const string ASSET_SOUND = "AssetSound";

    // Room constants
    private const string ROOM_IMAGE = "Image";
    private const string ROOM_OBJECT = "Object";
    private const string ROOM_SOUND = "Sound";
    private const string ROOM_WB_PROJ = "WhiteboardProjector";

    public FireBoxParser(RoomInstance room)
    {
        this._room = room;
    }

    public void Parse(string input)
    {
        Match match = Regex.Match(input, @"<FireBoxRoom>(?s)(.+?)</FireBoxRoom>");

        if (!match.Success)
        {
            Debug.LogError("Could not find FireBoxRoom code in input! Stopping parser.");
            return;
        }

        // Pull out the FireBoxRoom XML
        string tmpFireBoxXML = "<FireBoxRoom>" + match.Groups[1].Value + "</FireBoxRoom>";
        XElement xmlTree = XDocument.Parse(tmpFireBoxXML).Root;

        // First parse assets from FireBox XML
        IEnumerable<XElement> assets =
            from el in xmlTree.Element("Assets").Descendants()
            select el;
        ParseAssets(assets);

        // Next parse the initial room configuration
        XElement roomNode = xmlTree.Element("Room");
        ParseRoomConfiguration(roomNode);

        // Finally parse the objects/contents in the room
        IEnumerable<XElement> objects =
            from el in roomNode.Descendants()
            select el;
        ParseRoomContents(objects);
    }

    private void ParseRoomContents(IEnumerable<XElement> contents)
    {
        foreach (XElement item in contents)
        {
            switch (item.Name.LocalName)
            {
                case ROOM_IMAGE:
                    {
                        ImageItem itemRef = new ImageItem();

                        var id = item.Attribute("id");
                        var pos = item.Attribute("pos");
                        var fwd = item.Attribute("fwd");
                        var rotation = item.Attribute("rotation");
                        var scale = item.Attribute("scale");

                        if (id != null)
                            itemRef.ID = id.Value;
                        if (pos != null)
                            itemRef.Pos = ObjectUtils.StringToVector3(pos.Value);
                        if (fwd != null)
                            itemRef.Fwd = ObjectUtils.StringToVector3(fwd.Value);
                        if (rotation != null)
                            itemRef.Rotation = ObjectUtils.StringToVector3(rotation.Value);
                        if (scale != null)
                            itemRef.Scale = ObjectUtils.StringToVector3(scale.Value);

                        Debug.Log("Adding new image room item: " + id);
                        this._room.AddRoomItem(itemRef);
                        break;
                    }

                case ROOM_SOUND:
                    {
                        var id = item.Attribute("id");

                        if (id == null)
                            break;

                        SoundItem itemRef = new SoundItem(id.Value);

                        var loop = item.Attribute("loop");

                        if (loop != null)
                            itemRef.Loop = bool.Parse(loop.Value);

                        Debug.Log("Adding new sound room item: " + id);
                        this._room.AddRoomItem(itemRef);
                        break;
                    }

                case ROOM_OBJECT:
                    {
                        var id = item.Attribute("id");

                        if (id == null)
                            break;

                        ObjectItem itemRef = new ObjectItem(id.Value);

                        var pos = item.Attribute("pos");
                        var fwd = item.Attribute("fwd");
                        var vel = item.Attribute("vel");
                        var rotation = item.Attribute("rotation");
                        var scale = item.Attribute("scale");
                        var rotate_axis  = item.Attribute("rotate_axis");
                        var rotate_deg_per_sec = item.Attribute("rotate_deg_per_sec");
                        var visible = item.Attribute("visible");
                        var onclick = item.Attribute("onclick");

                        if (pos != null)
                            itemRef.Pos = ObjectUtils.StringToVector3(pos.Value);
                        if (fwd != null)
                            itemRef.Fwd = ObjectUtils.StringToVector3(fwd.Value);
                        if (vel != null)
                            itemRef.Vel = float.Parse(vel.Value);
                        if (rotation != null)
                            itemRef.Rotation = ObjectUtils.StringToVector3(rotation.Value);
                        if (scale != null)
                            itemRef.Scale = ObjectUtils.StringToVector3(scale.Value);
                        if (rotate_axis != null)
                            itemRef.Rotate_axis = ObjectUtils.StringToVector3(rotate_axis.Value);
                        if (rotate_deg_per_sec != null)
                            itemRef.Rotate_deg_per_sec = float.Parse(rotate_deg_per_sec.Value);
                        if (visible != null)
                            itemRef.Visible = bool.Parse(visible.Value);
                        if (onclick != null)
                            itemRef.OnClick = onclick.Value;

                        Debug.Log("Adding new object room item: " + id);
                        this._room.AddRoomItem(itemRef);
                        break;
                    }

                case ROOM_WB_PROJ:
                    {
                        WhiteboardProjectorItem itemRef = new WhiteboardProjectorItem();

                        var pos = item.Attribute("pos");
                        var fwd = item.Attribute("fwd");
                        var rotation = item.Attribute("rotation");
                        var scale = item.Attribute("scale");

                        if (pos != null)
                            itemRef.Pos = ObjectUtils.StringToVector3(pos.Value);
                        if (fwd != null)
                            itemRef.Fwd = ObjectUtils.StringToVector3(fwd.Value);
                        if (rotation != null)
                            itemRef.Rotation = ObjectUtils.StringToVector3(rotation.Value);
                        if (scale != null)
                            itemRef.Scale = ObjectUtils.StringToVector3(scale.Value);

                        Debug.Log("Adding new whiteboard+projector room item");
                        this._room.AddRoomItem(itemRef);
                        break;
                    }

                default:
                    Debug.LogWarning("Unknown room item type: " + item.Name);
                    break;
            }
        }
    }

    private void ParseRoomConfiguration(XElement roomNode)
    {
        RoomConfiguration roomConfiguration = new RoomConfiguration();

        // TODO: Add more of the supported attributes
        var skybox_left_id = roomNode.Attribute("skybox_left_id");
        var skybox_right_id = roomNode.Attribute("skybox_right_id");
        var skybox_up_id = roomNode.Attribute("skybox_up_id");
        var skybox_down_id = roomNode.Attribute("skybox_down_id");
        var skybox_front_id = roomNode.Attribute("skybox_front_id");
        var skybox_back_id = roomNode.Attribute("skybox_back_id");
        var pos = roomNode.Attribute("pos");
        var fwd = roomNode.Attribute("fwd");
        var rotation = roomNode.Attribute("rotation");
        var jump_velocity = roomNode.Attribute("jump_velocity");
        var walk_speed = roomNode.Attribute("walk_speed");
        var run_speed = roomNode.Attribute("run_speed");
        var gravity = roomNode.Attribute("gravity");

        if (skybox_left_id != null)
            roomConfiguration.Skybox_left_id = skybox_left_id.Value;
        if (skybox_right_id != null)
            roomConfiguration.Skybox_right_id = skybox_right_id.Value;
        if (skybox_up_id != null)
            roomConfiguration.Skybox_up_id = skybox_up_id.Value;
        if (skybox_down_id != null)
            roomConfiguration.Skybox_down_id = skybox_down_id.Value;
        if (skybox_front_id != null)
            roomConfiguration.Skybox_front_id = skybox_front_id.Value;
        if (skybox_back_id != null)
            roomConfiguration.Skybox_back_id = skybox_back_id.Value;
        if (pos != null)
            roomConfiguration.Pos = ObjectUtils.StringToVector3(pos.Value);
        if (fwd != null)
            roomConfiguration.Fwd = ObjectUtils.StringToVector3(fwd.Value);
        if (rotation != null)
            roomConfiguration.Rotation = ObjectUtils.StringToVector3(rotation.Value);
        if (jump_velocity != null)
            roomConfiguration.Jump_velocity = float.Parse(jump_velocity.Value);
        if (walk_speed != null)
            roomConfiguration.Walk_speed = float.Parse(walk_speed.Value);
        if (run_speed != null)
            roomConfiguration.Run_speed = float.Parse(run_speed.Value);
        if (gravity != null)
            roomConfiguration.Gravity = float.Parse(gravity.Value);

        Debug.Log("Parsed XML room configuration parameters");
        this._room.SetRoomConfiguration(roomConfiguration);
    }

    private void ParseAssets(IEnumerable<XElement> assets)
    {
        foreach (XElement asset in assets)
        {
            var id = asset.Attribute("id");
            var src = asset.Attribute("src");

            // We need a src attribute for all elements
            if (src == null)
            {
                Debug.LogError("Malformed asset, missing src attribute! " + asset.Name);
                continue;
            }

            switch (asset.Name.LocalName)
            {
                case ASSET_IMAGE:
                    {
                        if (id == null)
                            break;

                        AssetImage assetRef = new AssetImage(id.Value, src.Value);

                        var sbs3d = asset.Attribute("sbs3d");
                        var ou3d = asset.Attribute("ou3d");
                        var reverse3d = asset.Attribute("reverse3d");
                        var tex_clamp = asset.Attribute("tex_clamp");
                        var tex_linear = asset.Attribute("tex_linear");
                        var tex_compress = asset.Attribute("tex_compress");
                        var tex_alpha = asset.Attribute("tex_alpha");
                        var tex_premultiply = asset.Attribute("tex_premultiply");
                        var tex_colorspace = asset.Attribute("tex_colorspace");

                        if (sbs3d != null)
                            assetRef.Sbs3d = bool.Parse(sbs3d.Value);
                        if (ou3d != null)
                            assetRef.Ou3d = bool.Parse(ou3d.Value);
                        if (reverse3d != null)
                            assetRef.Reverse3d = bool.Parse(reverse3d.Value);
                        if (tex_clamp != null)
                            assetRef.Tex_clamp = bool.Parse(tex_clamp.Value);
                        if (tex_linear != null)
                            assetRef.Tex_linear = bool.Parse(tex_linear.Value);
                        if (tex_compress != null)
                            assetRef.Tex_compress = bool.Parse(tex_compress.Value);
                        if (tex_alpha != null)
                            assetRef.Tex_alpha = tex_alpha.Value;
                        if (tex_premultiply != null)
                            assetRef.Tex_premultiply = bool.Parse(tex_premultiply.Value);
                        if (tex_colorspace != null)
                            assetRef.Tex_colorspace = tex_colorspace.Value;

                        Debug.Log("Adding new image asset: " + id);
                        this._room.AddAsset(assetRef);
                        break;
                    }

                case ASSET_OBJECT:
                    {
                        if (id == null)
                            break;

                        AssetObject assetRef = new AssetObject(id.Value, src.Value);

                        var tex = asset.Attribute("tex");
                        var mtl = asset.Attribute("mtl");
                        var tex_clamp = asset.Attribute("tex_clamp");
                        var tex_linear = asset.Attribute("tex_linear");
                        var tex_mipmap = asset.Attribute("tex_mipmap");
                        var tex_compress = asset.Attribute("tex_compress");
                        var tex_alpha = asset.Attribute("tex_alpha");
                        var tex_premultiply = asset.Attribute("tex_premultiply");
                        var tex_colorspace = asset.Attribute("tex_colorspace");

                        if (tex != null)
                            assetRef.Tex = tex.Value;
                        if (mtl != null)
                            assetRef.Mtl = mtl.Value;
                        if (tex_clamp != null)
                            assetRef.Tex_clamp = bool.Parse(tex_clamp.Value);
                        if (tex_linear != null)
                            assetRef.Tex_linear = bool.Parse(tex_linear.Value);
                        if (tex_mipmap != null)
                            assetRef.Tex_mipmap = bool.Parse(tex_mipmap.Value);
                        if (tex_compress != null)
                            assetRef.Tex_compress = bool.Parse(tex_compress.Value);
                        if (tex_alpha != null)
                            assetRef.Tex_alpha = tex_alpha.Value;
                        if (tex_premultiply != null)
                            assetRef.Tex_premultiply = bool.Parse(tex_premultiply.Value);
                        if (tex_colorspace != null)
                            assetRef.Tex_colorspace = tex_colorspace.Value;

                        Debug.Log("Adding new object asset: " + id);
                        this._room.AddAsset(assetRef);
                        break;
                    }

                case ASSET_SCRIPT:
                    {
                        AssetScript assetRef = new AssetScript(src.Value);

                        Debug.Log("Adding new script asset: " + src);
                        this._room.AddAsset(assetRef);
                        break;
                    }

                case ASSET_SOUND:
                    {
                        if (id == null)
                            break;

                        AssetSound assetRef = new AssetSound(id.Value, src.Value);

                        Debug.Log("Adding new sound asset: " + src);
                        this._room.AddAsset(assetRef);
                        break;
                    }

                default:
                    Debug.LogWarning("Unknown asset type: " + asset.Name);
                    break;
            }
        }
    }
}
