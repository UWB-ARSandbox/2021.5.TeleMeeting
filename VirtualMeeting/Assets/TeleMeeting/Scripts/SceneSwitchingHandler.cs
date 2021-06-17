using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UnityEngine;

namespace TeleMeeting
{
    public class SceneSwitchingHandler : MonoBehaviour
    {
        private readonly string baseUrl = "http://teleapi.joshumax.me";

        private readonly List<Action<string>> sceneSwitchCallback = new List<Action<string>>();

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddSceneSwitchCallback(Action<string> callback)
        {
            Debug.Log("Added new scene switch callback");
            this.sceneSwitchCallback.Add(callback);
        }

        public void ButtonHandler(string roomId, string arcId)
        {
            Debug.Log(roomId);
            byte[] data = null;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = null;
                try
                {
                    response = client.GetAsync(string.Format("{0}/rooms/getBlob/{1}", baseUrl, arcId)).Result;
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Error has occurred when trying to access: {0}. Status code: {1}",
                        string.Format("{0}/rooms/getBlob/{1}", baseUrl, roomId),
                        response != null ? (int) response.StatusCode : -1));
                }
                data = response.Content.ReadAsByteArrayAsync().Result;
            }
            foreach (var cb in this.sceneSwitchCallback)
            {
                Debug.Log("Invoking callback for selected scene - ButtonHandler()");
                cb?.Invoke(arcId);
            }
            gameObject.SetActive(false);
        }
    }
}
