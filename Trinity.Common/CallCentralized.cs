using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


public static class CallCentralized
{
    

    public static T Get<T>(string Controller, string Action, out bool statusCentralized, params string[] pram)
    {
        try
        {
            if (EnumAppConfig.ByPassCentralizedDB)
            {
                statusCentralized = true;
                return default(T);
            }

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
                        statusCentralized = true;
                        return serializer.Deserialize<T>(reader);
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        statusCentralized = false;
        return default(T);
    }
    public static T Post<T>(string Controller, string Action, out bool statusCentralized, object data)
    {

        try
        {
            if (EnumAppConfig.ByPassCentralizedDB)
            {
                statusCentralized = true;
                return default(T);
            }

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
                        statusCentralized = true;
                        return serializer.Deserialize<T>(reader);
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        statusCentralized = false;
        return default(T);
    }
    public static T Post<T>(string Controller, string Action, out bool statusCentralized, params string[] pram)
    {

        try
        {
            if (EnumAppConfig.ByPassCentralizedDB)
            {
                statusCentralized = true;
                return default(T);
            }

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(EnumAppConfig.web_api_url);
            //client.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.PostAsJsonAsync(string.Format("api/{0}/{1}?{2}", Controller, Action, string.Join("&", pram)), "").Result;
            if (response.IsSuccessStatusCode)
            {
                using (StreamReader sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        statusCentralized = true;
                        return serializer.Deserialize<T>(reader);
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        statusCentralized = false;
        return default(T);
    }
    public static void Post(string Controller, string Action, out bool statusCentralized, params string[] pram)
    {
        try
        {
            if (EnumAppConfig.ByPassCentralizedDB)
            {
                statusCentralized = true;
            }
            else
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(EnumAppConfig.web_api_url);
                var response = client.PostAsJsonAsync(string.Format("api/{0}/{1}?{2}", Controller, Action, string.Join("&", pram)), "").Result;
                if (response.IsSuccessStatusCode)
                {
                    statusCentralized = true;
                }
                else
                {
                    statusCentralized = false;

                }
            }

            
        }
        catch (Exception)
        {
            statusCentralized = false;
        }
    }
    public static void Post(string Controller, string Action, out bool statusCentralized, object data)
    {

        try
        {
            if (EnumAppConfig.ByPassCentralizedDB)
            {
                statusCentralized = true;

            }
            else
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(EnumAppConfig.web_api_url);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.PostAsJsonAsync(string.Format("api/{0}/{1}", Controller, Action), data).Result;
                if (response.IsSuccessStatusCode)
                {
                    statusCentralized = true;
                }
                else
                {
                    statusCentralized = false;
                }
            }

            
        }
        catch (Exception)
        {
            statusCentralized = false;
        }
    }
    public static T Get<T>(string Controller, string Action, params string[] pram)
    {
        bool statusCentralized;
        return Get<T>(Controller, Action, out statusCentralized, pram);
    }
    public static T Post<T>(string Controller, string Action, object data)
    {
        bool statusCentralized;
        return Post<T>(Controller, Action, out statusCentralized, data);
    }
    public static T Post<T>(string Controller, string Action,  params string[] pram)
    {
        bool statusCentralized;
        return Post<T>(Controller, Action,out statusCentralized, pram);
    }
    public static void Post(string Controller, string Action, params string[] pram)
    {
        bool statusCentralized;
        Post(Controller, Action, out statusCentralized, pram);
    }
    public static void Post(string Controller, string Action, object data)
    {
        bool statusCentralized;
        Post(Controller, Action, out statusCentralized, data);
    }
}