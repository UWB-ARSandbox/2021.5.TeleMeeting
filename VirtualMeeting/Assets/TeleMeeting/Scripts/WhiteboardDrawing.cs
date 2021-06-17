using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using ASL;
using Microsoft.MixedReality.Toolkit.WindowsMixedReality;
using SimpleDemos;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Valve.VR;
namespace TeleMeeting
{
    public class WhiteboardDrawing : MonoBehaviour
    {
        private Vector2 boardSize;
        private Dictionary<int, Vector3> previousSpots;
        private Color origColor;
        private Color drawingColor;
        private GameObject VRPlayer;
        // left hand for vr
        private GameObject lHand;
        // right hand for vr
        private GameObject rHand;

        private Texture2D texture;
        private int textureSize = 1000;
        private Color32[] pixels;
        private float aspectRatio;

        // Start is called before the first frame update
        void Start()
        {
            // change path when building project
            /*
            string path = Application.dataPath + "/VirtualMeeting/Images/Form.txt";
            byte[] byteArray = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D();
            tex.LoadImage(byteArray);
            */
            Material[] materials = GetComponent<MeshRenderer>().materials;
            if(gameObject.GetComponent<ProjectorController>() == null)
            {
                Material newmat = Resources.Load("Materials/TestDrawing", typeof(Material)) as Material;
                newmat.mainTextureScale = new Vector2(1, -1);
                materials[0] = newmat;
            }
            


            previousSpots = new Dictionary<int, Vector3>();
            // use SteamVR_Behavior_Pose on cubes
            boardSize = new Vector2(10 * transform.lossyScale.x, 10 * transform.lossyScale.z);
            aspectRatio = transform.lossyScale.x / transform.lossyScale.y;
            texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            origColor = texture.GetPixel(0, 0);
            origColor.a = 0.0f;
            Shader shader = Shader.Find("Unlit/Transparent");
            Material mat = new Material(shader);
            mat.mainTexture = texture;
            materials[1] = mat;
            GetComponent<MeshRenderer>().materials = materials;
            MessageController msgCon = gameObject.GetComponent<MessageController>();
            if (msgCon == null)
                msgCon = gameObject.AddComponent<MessageController>();
            msgCon.onWhiteBoardMessage += WhiteboardMessage;
            pixels = texture.GetPixels32();
            ClearWhiteboardPrivate();
        }


        // Update is called once per frame
        void Update()
        {
            texture.SetPixels32(pixels);
            texture.Apply(false);
        }
        
        
        /**
         * DrawOnWhiteboard Function
         * values -
         * 0: x value of hit
         * 1: y value of hit
         * 2: z value of hit
         * 3: 1 or 0 if the mouse is currently being held down
         * 4: marker size
         * 5: marker color r
         * 6: marker color g
         * 7: marker color b
         */
        public void DrawOnWhiteboard(float[] values)
        {
            int peer_id = GameLiftManager.GetInstance().m_PeerId;
            int hashedName = GameLiftManager.GetInstance().m_Username.GetHashCode();
            drawingColor = new Color(values[5], values[6], values[7], 1.0f);
            Vector3 hit = new Vector3(values[0], values[1], values[2]);
            int markerSize = (int)values[4];
            bool mouseHeldDown = (int)values[3] == 1;
            float[] myValues = new float[12];
            myValues[0] = 3;
            myValues[1] = peer_id;
            myValues[2] = hashedName;
            myValues[3] = hit.x;
            myValues[4] = hit.y;
            myValues[5] = hit.z;
            //mouse is pressed or held down
            myValues[6] = values[3];
            //erase mode is on or off
            myValues[7] = 0;
            myValues[8] = values[4];
            myValues[9] = values[5];
            myValues[10] = values[6];
            myValues[11] = values[7];
            if (!previousSpots.ContainsKey(hashedName))
            {
                previousSpots.Add(hashedName, hit);
            }
            if (!mouseHeldDown)
            {
                previousSpots[hashedName] = hit;
                GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    GetComponent<ASLObject>().SendFloatArray(myValues);
                });
            }
            if (mouseHeldDown)
            {
                //WorldtoUVSpace(hit);
                Vector2Int texturePos = WorldtoUVSpace(hit);
                ColorTexture(texturePos[0], texturePos[1], markerSize);
                if (previousSpots[hashedName] != hit)
                {
                    Vector2Int prevTexturePos = WorldtoUVSpace(previousSpots[hashedName]);
                    DrawLineBetweenPoints(texturePos, prevTexturePos, markerSize);
                    //texture.Apply(false);
                    previousSpots[hashedName] = hit;
                }

                GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    GetComponent<ASLObject>().SendFloatArray(myValues);
                });
            }
        }
        /**
         * DrawOnWhiteboard Function
         * values -
         * 0: x value of hit
         * 1: y value of hit
         * 2: z value of hit
         * 3: 1 or 0 if the mouse is currently being held down
         * 4: marker size
         */
        public void EraseOnWhiteboard(float[] values)
        {
            int peer_id = GameLiftManager.GetInstance().m_PeerId;
            int hashedName = GameLiftManager.GetInstance().m_Username.GetHashCode();
            drawingColor = origColor;
            Vector3 hit = new Vector3(values[0], values[1], values[2]);
            int markerSize = (int)values[4];
            bool mouseHeldDown = (int)values[3] == 1;
            float[] myValues = new float[9];
            myValues[0] = 3;
            myValues[1] = peer_id;
            myValues[2] = hashedName;
            myValues[3] = hit.x;
            myValues[4] = hit.y;
            myValues[5] = hit.z;
            //mouse is pressed or held down
            myValues[6] = values[3];
            //erase mode is on or off
            myValues[7] = 1;
            myValues[8] = values[4];
            if (!previousSpots.ContainsKey(hashedName))
            {
                previousSpots.Add(hashedName, hit);
            }
            if (!mouseHeldDown)
            {
                previousSpots[hashedName] = hit;
                GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    GetComponent<ASLObject>().SendFloatArray(myValues);
                });
            }
            if (mouseHeldDown)
            {
                //WorldtoUVSpace(hit);
                Vector2Int texturePos = WorldtoUVSpace(hit);
                ColorTexture(texturePos[0], texturePos[1], markerSize);
                Vector2Int prevTexturePos = WorldtoUVSpace(previousSpots[hashedName]);
                DrawLineBetweenPoints(texturePos, prevTexturePos, markerSize);
                //texture.Apply(false);
                previousSpots[hashedName] = hit;
                GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    GetComponent<ASLObject>().SendFloatArray(myValues);
                });
            }
        }

        /**
         * FloatCallbackFunction
         * floats -
         * 0: Message code -> 3 whiteboard update message
         * 1: User id of client making the action
         * 2: Hashed code of the client's name
         * 3: x value of the hit
         * 4: y value of the hit
         * 5: z values of the hit
         * 6: 1 or 0 based on if the mouse is being pressed or held down
         * 7: erase mode 1 or 0
         * 8: marker size
         * 9: r of the drawing color
         * 10: g of the drawing color
         * 11: b of the drawing color
         */
        private void WhiteboardMessage(float[] floats)
        {
            int receiving_id = (int)floats[1];

            if (GameLiftManager.GetInstance().m_PeerId == receiving_id)
            {
                return;
            }
            if(receiving_id == -1)
            {
                ClearWhiteboardPrivate();
                return;
            }
            int hashedName = (int) floats[2];
            int markerSize = (int)floats[8];
            Vector3 recievedPos = new Vector3(floats[3], floats[4], floats[5]);
            int mouseHeldDown = (int)floats[6];
            int erase = (int)floats[7];
            if (erase == 1)
            {
                drawingColor = origColor;
            }
            else
            {
                drawingColor = new Color(floats[9], floats[10], floats[11]);
            }
            Vector2Int texturePos = WorldtoUVSpace(recievedPos);
            if (!previousSpots.ContainsKey(hashedName))
            {
                previousSpots.Add(hashedName, recievedPos);
            }
            if (mouseHeldDown == 0)
            {
                previousSpots[hashedName] = recievedPos;
                ColorTexture(texturePos[0], texturePos[1], markerSize);
                //texture.Apply(false);
            }
            else
            {
                ColorTexture(texturePos[0], texturePos[1], markerSize);
                Vector2Int prevTexturePos = WorldtoUVSpace(previousSpots[hashedName]);
                DrawLineBetweenPoints(texturePos, prevTexturePos, markerSize);
                //texture.Apply(false);
                previousSpots[hashedName] = recievedPos;
            }
        }

        public void ClearWhiteboard()
        {
            ClearWhiteboardPrivate();
            GetComponent<ASLObject>().SendAndSetClaim(() =>
            {
                float[] myValues = new float[2];
                myValues[0] = (int)MessageController.MessageCode.WhiteBoardMessage;
                myValues[1] = -1;
                GetComponent<ASLObject>().SendFloatArray(myValues);
            });
        }

        void ClearWhiteboardPrivate()
        {
            for (int i = 0; i < texture.width; i++)
            {
                for (int j = 0; j < texture.height; j++)
                {
                    pixels[i + j * texture.width] = origColor;
                }
            }
            //texture.SetPixels32(pixels);
            //texture.Apply(false);
        }

        void ColorTexture(int x, int y, int markerSize)
        {
            if (markerSize == 1)
            {
                pixels[x + y * texture.width] = drawingColor;
                return;
            }
            for (int i =(-markerSize / 2); i <(markerSize / 2); i++)
            {
                for (int j = (int)((-markerSize / 2) * aspectRatio); j < (int)(markerSize / 2 * aspectRatio); j++)
                {
                    if ((x + i >= 0 && x + i <= texture.width - 1) && (y + j >= 0 && y + j <= texture.height - 1))
                    {
                        //texture.SetPixel(x + i, y + j, drawingColor);
                        pixels[(x + i) + ((j + y) * texture.width)] = drawingColor;
                    }
                }
            }
        }

        void DrawLineBetweenPoints(Vector2Int current, Vector2Int previous, int markerSize)
        {
            Vector2 temp = previous;
            float frac = 1 / Mathf.Sqrt(Mathf.Pow(current.x - previous.x, 2) + Mathf.Pow(current.y - previous.y, 2));
            float percentage = 0;
            //Color[] pixels = texture.GetPixels();
            //Color32[] pixels32 = texture.GetPixels32();

            while ((int)temp.x != current.x || (int)temp.y != current.y)
            {
                temp = Vector2.Lerp(previous, current, percentage);
                percentage += frac;
                ColorTexture((int)temp.x, (int)temp.y, markerSize);
            }
            //texture.SetPixels32(pixels);
        }

        /*
        Vector2Int WorldCoordinateToTexture2D(Vector3 value)
        {
            Vector3 boardFace = board.transform.up;

            // if i want to worry about rotating where forward is no longer facing up then uncomment

            Vector3 boardForward = board.transform.forward;
            int xForward = boardForward.x < 0 ? -1 : 1;
            int yForward = boardForward.y < 0 ? -1 : 1;
            int zForward = boardForward.z < 0 ? -1 : 1;


            Vector3 location = board.transform.position;
            int xFacing = boardFace.x < 0 ? -1 : 1;
            int yFacing = boardFace.y < 0 ? -1 : 1;
            int zFacing = boardFace.z < 0 ? -1 : 1;
            Vector2 relativePos;
            Vector2 posOnBoard = new Vector2();
            if (xFacing == 1 && zFacing == 1)
            {
                relativePos = new Vector2(location.x - value.x, value.z - location.z);

            }
            else if (xFacing == 1 && zFacing == -1)
            {
                relativePos = new Vector2(value.x - location.x, value.z - location.z);
            }
            else if (xFacing == -1 && zFacing == 1)
            {
                relativePos = new Vector2(location.x - value.x, location.z - value.z);
            }
            else
            {
                relativePos = new Vector2(value.x - location.x, location.z - value.z);
            }
            //Debug.Log(relativePos);
            //Debug.Log(boardFace);
            float xPos = Mathf.Sqrt(Mathf.Pow(relativePos.x, 2) + Mathf.Pow(relativePos.y, 2));
            if (Mathf.Abs(Mathf.Round(boardFace.x * 10)) >= Mathf.Abs(Mathf.Round(boardFace.z * 10)))
            {
                xPos *= relativePos.y < 0 ? -1 : 1;
            }
            else
            {
                xPos *= relativePos.x < 0 ? -1 : 1;
            }
            //Debug.Log(xPos);
            posOnBoard.x = xPos;
            float yPos = value.y - location.y;
            posOnBoard.y = yPos;
            float xMin = location.x - boardSize[0] / 2;
            float yMin = location.y - boardSize[1] / 2;
            if (yForward == 1)
            {
                posOnBoard.x *= -1;
                posOnBoard.y *= -1;
            }

            posOnBoard.x += boardSize[0] / 2;
            posOnBoard.y += boardSize[1] / 2;

            float xNormal = posOnBoard.x / boardSize[0];
            float yNormal = posOnBoard.y / boardSize[1];
            int textureX = (int) (xNormal * texture.width);
            int textureY = (int) (yNormal * texture.height);

            return new Vector2Int(textureX, textureY);
        }
        */


        Vector2Int WorldtoUVSpace(Vector3 pos)
        {
            //Get the TRS Matrix of the Board
            //Matrix4x4 trs = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            //Invert the TRS Matrix (For conversion from World to Local Space)
            //trs = trs.inverse;
            //Multiply Position by TRS Matrix
            Matrix4x4 trs = transform.worldToLocalMatrix;
            Vector3 localPosition = trs.MultiplyPoint(pos);

            //Create Position Vector on the Plane
            Vector2 planePosition = Vector2.zero;
            //Negate X and Y
            planePosition.x = localPosition.x;
            planePosition.y = -localPosition.y;
            
            
            //Debug.Log(localPosition.x + "," + localPosition.y + "," + localPosition.z);
            
            
            
            //Debug.LogError(boardSize.x + "," + boardSize.y);
            //Create TRS 3x3 Matrix for Conversion from Board Space to Texture Space
            Vector2 Translation = new Vector2(-1 * (10f / 2), -1 * (10f / 2));
            Vector2 Scaling = new Vector2(10f / texture.width, 10f / texture.height);
            Matrix3x3 pixelTRS = Matrix3x3Helpers.CreateTRS(Translation, 0, Scaling);
            //Invert Texture Space Matrix
            pixelTRS = pixelTRS.Invert();

            //Multiply Plane position by Texture Space TRS Matrix
            planePosition = Matrix3x3.MultiplyVector2(pixelTRS, planePosition);

            //Convert to Vector2 Int
            Vector2Int texturePosition = Vector2Int.RoundToInt(planePosition);

            return texturePosition;
        }
        //Functions to handle interaction from vr hand
        private bool drawing = false;

        public virtual void PointerClicked(HandController.PointerClickValues vals)
        {
            drawing = false;
        }

        public virtual void PointerHeld(HandController.PointerClickValues vals)
        {
            if(vals.pointerEvent.distance < IndexPlayerIntereractions.DRAWING_DISTANCE)
            {
                if (vals.pointerHand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardPen)
                {
                    float[] values = new float[8];
                    values[0] = vals.pointerEvent.hit.x;
                    values[1] = vals.pointerEvent.hit.y;
                    values[2] = vals.pointerEvent.hit.z;
                    values[3] = drawing ? 1.0f : 0;
                    values[4] = IndexPlayerIntereractions.markerSize;
                    values[5] = IndexPlayerIntereractions.drawingColor.r;
                    values[6] = IndexPlayerIntereractions.drawingColor.g;
                    values[7] = IndexPlayerIntereractions.drawingColor.b;
                    DrawOnWhiteboard(values);
                    drawing = true;
                }
                else if (vals.pointerHand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardEraser)
                {
                    float[] values = new float[5];
                    values[0] = vals.pointerEvent.hit.x;
                    values[1] = vals.pointerEvent.hit.y;
                    values[2] = vals.pointerEvent.hit.z;
                    values[3] = drawing ? 1.0f : 0;
                    values[4] = IndexPlayerIntereractions.markerSize;
                    EraseOnWhiteboard(values);
                    drawing = true;
                }
            }
            
            
        }

        public virtual void PointerOut(HandController.PointerClickValues vals)
        {
            if (vals.pointerHand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardPen || vals.pointerHand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardEraser)
                vals.laserPointer.color = Color.red;
            drawing = false;
        }

        public virtual void PointerHover(HandController.PointerClickValues vals)
        {
            if (vals.pointerEvent.distance < IndexPlayerIntereractions.DRAWING_DISTANCE)
            {
                if (vals.pointerHand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardPen ||
                    vals.pointerHand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardEraser)
                    vals.laserPointer.color = Color.green;
            }
            else
            {
                if (vals.pointerHand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardPen || vals.pointerHand.currentAttachedObject == IndexPlayerIntereractions.instance.whiteboardEraser)
                    vals.laserPointer.color = Color.red;
            }
        }
    }
}
