using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class CallCentralized
{
    #region Singleton Implementation
    private static volatile CallCentralized _instance;

    private static object syncRoot = new Object();

    private CallCentralized()
    {

    }

    public static CallCentralized Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (syncRoot)
                {
                    if (_instance == null)
                        _instance = new CallCentralized();
                }
            }
            return _instance;
        }
    }
    #endregion

    public bool IsConnectCentralized = true;

    public T Get<T>(string Controller, string Action, params string[] pram)
    {
        try
        {
            IsConnectCentralized = true;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(EnumAppConfig.web_api_url);
            HttpResponseMessage response = client.GetAsync(string.Format("api/{0}/{1}?{2}", Controller, Action, string.Join("&", pram))).Result;
            if (response.IsSuccessStatusCode)
            {
                using (StreamReader sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        return serializer.Deserialize<T>(reader);
                    }
                }
            }
            else
            {
                IsConnectCentralized = false;
            }
        }
        catch (Exception)
        {
            IsConnectCentralized = false;
        }
        return default(T);
    }
    public T Post<T>(string Controller, string Action, object data)
    {
        try
        {
            IsConnectCentralized = true;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(EnumAppConfig.web_api_url);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.PostAsJsonAsync(string.Format("api/{0}/{1}", Controller, Action), data).Result;
            if (response.IsSuccessStatusCode)
            {
                using (StreamReader sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        return serializer.Deserialize<T>(reader);
                    }
                }
            }
            else
            {
                IsConnectCentralized = false;
            }
            return default(T);
        }
        catch (Exception)
        {
            IsConnectCentralized = false;
        }
        return default(T);
    }
}