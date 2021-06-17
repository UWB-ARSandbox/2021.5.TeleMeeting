using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using MeshForwardDirection = uDesktopDuplication.Texture.MeshForwardDirection;
using DuplicatorState = uDesktopDuplication.DuplicatorState;

namespace TeleMeeting
{
    public class ScreenSharePickerController : MonoBehaviour
    {

        public GameObject monitorPrefab;
        private bool active = false;
        private int monitorCount = 0;
        private ProjectorController controller = null;
        public class MonitorInfo
        {
            public GameObject gameObject { get; set; }
            public Quaternion originalRotation { get; set; }
            public Vector3 originalLocalScale { get; set; }
            public uDesktopDuplication.Texture uddTexture { get; set; }
            public Mesh mesh { get; set; }
        }

        private List<MonitorInfo> monitors_ = new List<MonitorInfo>();
        public List<MonitorInfo> monitors { get { return monitors_; } }

        public class SavedMonitorInfo
        {
            public float widthScale = 1f;
            public float heightScale = 1f;
        }

        private List<SavedMonitorInfo> savedInfoList_ = new List<SavedMonitorInfo>();
        public List<SavedMonitorInfo> savedInfoList { get { return savedInfoList_; } }

        public enum ScaleMode
        {
            Real,
            Fixed,
            Pixel,
        }

        public float scale = 0.5f;

        public ScaleMode scaleMode = ScaleMode.Fixed;

        public MeshForwardDirection meshForwardDirection = MeshForwardDirection.Z;

        // Start is called before the first frame update
        private void Start()
        {
            gameObject.SetActive(false);
        }
        public void Activate(ProjectorController cont)
        {
            controller = cont;
            if (active)
            {
                for (int i = 0; i < (monitorCount * 2); i++)
                {
                    transform.GetChild(i).gameObject.SetActive(true);
                    gameObject.SetActive(true);
                }
                return;
            }
            active = true;
            gameObject.SetActive(true);
            var n = uDesktopDuplication.Manager.monitorCount;
            monitorCount = n;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            //float gap = rectTransform.sizeDelta.x*0.05f;
            float gap = 0.05f;

            //float size = (rectTransform.sizeDelta.x - (gap * (1 + n)))/n;
            float size = (1 - (gap * (1 + n))) / n;
            float vertGap = (1 - size) / 2;
            //float vertGap = 0.05f;
            //RectTransform pickerRect = gameObject.GetComponent<RectTransform>();
            //Vector2 pickerRectAnchorMin = pickerRect.anchorMin;
            //Vector2 pickerRectAnchorMax = pickerRect.anchorMax;
            //pickerRectAnchorMin.y = pickerRectAnchorMin.y + 0.05f * n;
            //pickerRectAnchorMax.y = pickerRectAnchorMax.y - 0.05f * n;
            //pickerRect.anchorMin = pickerRectAnchorMin;
            //pickerRect.anchorMax = pickerRectAnchorMax;

            for (int i = 0; i < n; ++i)
            {
                // Create monitor obeject
                var go = Instantiate(monitorPrefab);
                go.name = uDesktopDuplication.Manager.monitors[i].name;
                go.GetComponent<Renderer>().forceRenderingOff = true;
                // Saved infomation
                if (savedInfoList.Count == i)
                {
                    savedInfoList.Add(new SavedMonitorInfo());
                    Assert.AreEqual(i, savedInfoList.Count - 1);
                }
                var savedInfo = savedInfoList[i];

                // Expand AABB
                var mesh = go.GetComponent<MeshFilter>().mesh; // clone
                var aabbScale = mesh.bounds.size;
                aabbScale.y = Mathf.Max(aabbScale.y, aabbScale.x);
                aabbScale.z = Mathf.Max(aabbScale.z, aabbScale.x);
                mesh.bounds = new Bounds(mesh.bounds.center, aabbScale);

                // Assign monitor
                var texture = go.GetComponent<uDesktopDuplication.Texture>();
                texture.monitorId = i;
                var monitor = texture.monitor;

                // Set width / height
                float width = 1f, height = 1f;
                switch (scaleMode)
                {
                    case ScaleMode.Real:
                        width = monitor.widthMeter;
                        height = monitor.heightMeter;
                        break;
                    case ScaleMode.Fixed:
                        width = scale * (monitor.isHorizontal ? monitor.aspect : 1f);
                        height = scale * (monitor.isHorizontal ? 1f : 1f / monitor.aspect);
                        break;
                    case ScaleMode.Pixel:
                        width = scale * (monitor.isHorizontal ? 1f : monitor.aspect) * ((float)monitor.width / 1920);
                        height = scale * (monitor.isHorizontal ? 1f / monitor.aspect : 1f) * ((float)monitor.width / 1920);
                        break;
                }

                width *= savedInfo.widthScale;
                height *= savedInfo.heightScale;

                if (meshForwardDirection == MeshForwardDirection.Y)
                {
                    go.transform.localScale = new Vector3(width, go.transform.localScale.y, height);
                }
                else
                {
                    go.transform.localScale = new Vector3(width, height, go.transform.localScale.z);
                }

                // Set parent as this object
                go.transform.SetParent(transform);

                // Save
                var info = new MonitorInfo();
                info.gameObject = go;
                info.originalRotation = go.transform.rotation;
                info.originalLocalScale = go.transform.localScale;
                info.uddTexture = texture;
                info.mesh = mesh;
                monitors.Add(info);

                GameObject go2 = new GameObject();
                RectTransform go2xform = go2.AddComponent<RectTransform>();

                go2.transform.SetParent(transform);
                //go2xform.anchoredPosition = new Vector3(((i - 1) * size) + (i * gap),vertGap,0);
                go2xform.anchoredPosition = Vector2.zero;
                go2xform.anchorMin = new Vector2(i * size + (i + 1) * gap, vertGap);
                go2xform.anchorMax = new Vector2(i * size + (i + 1) * gap + size, 1 - vertGap);
                go2xform.localRotation = Quaternion.Euler(0, 180, 180);
                //new Vector3(0, 180f, 180f);


                go2xform.offsetMin = Vector2.zero;
                go2xform.offsetMax = Vector2.zero;

                MonitorEventHandler go2Trigger = go2.AddComponent<MonitorEventHandler>();
                go2Trigger.MonitorNumber = i;
                go2Trigger.OnMonitorClicked += MonitorSelected;

                //go2xform.localPosition = Vector3.zero;
                //go2xform.ForceUpdateRectTransforms();
                //go2xform.sizeDelta = new Vector2(size, size);
                RawImage img = go2.AddComponent<RawImage>();

                img.texture = go.GetComponent<Renderer>().material.mainTexture;

                //go.SetActive(false);

            }

            // Sort monitors in coordinate order
            monitors.Sort((a, b) => a.uddTexture.monitor.left - b.uddTexture.monitor.left);
        }

        private void MonitorSelected(int val)
        {
            for (int i = 0; i < (monitorCount * 2); i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            gameObject.SetActive(false);
            controller.SetScreenID(val);
            if(!controller.active)
                controller.ScreenActivated();
            controller = null;
        }
    }

    public class MonitorEventHandler : EventTrigger
    {
        public Action<int> OnMonitorClicked;
        public int MonitorNumber;
        public override void OnPointerClick(PointerEventData data)
        {
            OnMonitorClicked.Invoke(MonitorNumber);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            OnMonitorClicked.Invoke(MonitorNumber);
        }
    }
}
