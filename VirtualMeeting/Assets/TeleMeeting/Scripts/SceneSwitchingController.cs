using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using UnityEngine;
using UnityEngine.UI;
using Valve.Newtonsoft.Json.Linq;

namespace TeleMeeting
{
    [RequireComponent(typeof(SceneSwitchingHandler))]
    public class SceneSwitchingController : MonoBehaviour
    {
        public static SceneSwitchingController[] Instances
        {
            get
            {
                return FindObjectsOfType<SceneSwitchingController>(true);
            }
        }
        private string baseUrl = "http://teleapi.joshumax.me";
        public Button cancelButton;

        public Action onMenuClose;
        public SceneSwitchingHandler switchHandler;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Assert(cancelButton != null);
            cancelButton.onClick.AddListener(CancelClicked);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void GenerateSceneGrid(int pageNumber)
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler
                {AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate}))
            {
                HttpResponseMessage response = null;
                try
                {
                    response = client.GetAsync(string.Format("{0}/rooms/browse/{1}", baseUrl, pageNumber)).Result;

                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Error has occurred when trying to access: {0}. Status code: {1}",
                        string.Format("{0}/rooms/browse/{1}", baseUrl, pageNumber), response != null ? (int) response.StatusCode : -1));
                }

                string content = response.Content.ReadAsStringAsync().Result;
                JObject jsonContent = JObject.Parse(content);
                int index = 0;
                foreach (JToken room in jsonContent["rooms"])
                {
                    GenerateSceneSquare(room, index);
                    index++;
                }

                for (int i = index; i < 10; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        public void GenerateSceneSquare(JToken room, int index)
        {
            GameObject button = transform.GetChild(index).gameObject;
            string title = room["title"].ToString();
            string image = room["image"].ToString();
            string roomId = room["uuid"].ToString();
            string arcId = room["arc_id"].ToString();
            byte[] data;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = null;
                try
                {
                    response = client.GetAsync(string.Format("{0}/rooms/getBlob/{1}", baseUrl, image)).Result;
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Error has occurred when trying to access: {0}. Status code: {1}",
                        string.Format("{0}/rooms/getBlob/{1}", baseUrl, image),
                        response != null ? (int) response.StatusCode : -1));
                }
                data = response.Content.ReadAsByteArrayAsync().Result;
            }
            Texture2D tex = new Texture2D(256, 256);
            tex.LoadImage(data);
            button.GetComponent<RawImage>().texture = tex;
            button.transform.GetChild(0).GetComponent<Text>().text = title;
            button.name = arcId;
            // need to make a function that the buttons call when clicked to change scene to the clicked scene
            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(delegate
            {
                switchHandler.ButtonHandler(roomId, arcId);
            });
        }

        public void CancelClicked()
        {
            gameObject.SetActive(false);
            onMenuClose?.Invoke();
        }
    }
}
