using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;//packages\Microsoft.AspNet.WebApi.Client.4.0.30506.0\lib\net40\System.Net.Http.Formatting.dll
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
//<package id = "Microsoft.AspNet.WebApi.Client" version="4.0.30506.0" targetFramework="net40" />
//  <package id = "Microsoft.Net.Http" version="2.0.20710.0" targetFramework="net40" />
//https://github.com/aspnet/AspNetWebStack
namespace EDoc2.ApiClient.Utils
{
    public class RequestUtils
    {
        public static string GetParamsUrl(Dictionary<string, string> requestParams)
        {
            string url = "";
            if (requestParams != null && requestParams.Count > 0)
            {
                url += "?";
                int i = 0;
                foreach (var key in requestParams.Keys)
                {
                    url += key + "=" + requestParams[key].Trim();
                    i++;
                    if (i < requestParams.Count)
                        url += "&";
                }
            }
            return url;
        }
        public static string GetParamsJson(Dictionary<string, string> requestParams)
        {
            string json = "{";
            if (requestParams != null)
            {
                int i = 0;
                foreach (var key in requestParams.Keys)
                {
                    i++;
                    json += "\"" + key + "\":\"" + requestParams[key] + "\"";
                    if (i < requestParams.Keys.Count)
                    {
                        json += ",";
                    }
                }
            }
            json += "}";
            return json;
        }
        private static HttpResponseMessage Get(Dictionary<string, string> resource, Dictionary<string, string> head, Dictionary<string, string> requesParams)
        {
            if (resource == null)
                return null;
            if (string.IsNullOrEmpty(resource["BaseUrl"]))
                throw new ArgumentNullException("baseUrl not is null!");
            string resourceUrl = resource["Value"];
            if (string.IsNullOrEmpty(resourceUrl))
                throw new ArgumentNullException("resource not is null!");
            HttpResponseMessage response = null;
            if (requesParams != null)
                resourceUrl += GetParamsUrl(requesParams);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(resource["BaseUrl"]);
                // Add an Accept header for JSON format.
                // 为JSON格式添加一个Accept报头
                //注意这里的格式哦，为 "username:password"
                //  mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
                //  myReq.Credentials = mycache;
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                if (head != null)
                {
                    foreach (var key in head.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key,
                                                head[key]);
                    }
                }
                response = client.GetAsync(resourceUrl).Result;
            }
            return response;
        }


        private static HttpResponseMessage Post(Dictionary<string, string> resource, Dictionary<string, string> head, dynamic requesParams)
        {
            if (resource == null)
                return null;
            if (string.IsNullOrEmpty(resource["BaseUrl"]))
                throw new ArgumentNullException("baseUrl not is null!");
            string resourceUrl = resource["Value"];
            //    string requestJson = "{}";
            if (string.IsNullOrEmpty(resourceUrl))
                throw new ArgumentNullException("resource not is null!");
            HttpResponseMessage response = null;
            if (requesParams != null && requesParams is IDictionary)
                resourceUrl += GetParamsUrl(requesParams);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(resource["BaseUrl"]);
                // Add an Accept header for JSON format.
                // 为JSON格式添加一个Accept报头
                //注意这里的格式哦，为 "username:password"
                //  mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
                //  myReq.Credentials = mycache;
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                if (head != null)
                {
                    foreach (var key in head.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key,
                                                head[key]);
                    }
                }
                if (requesParams is IDictionary)
                {
                    var request = new { req = "1" };
                    response = HttpClientExtensions.PostAsJsonAsync(client, resourceUrl, request).Result;
                }
                else
                {
                    MediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();

                    // Use the JSON formatter to create the content of the request body.
                    // 使用JSON格式化器创建请求体内容。
                    HttpContent content = new ObjectContent<string>(requesParams, jsonFormatter);
                    response = client.PostAsync(resourceUrl, content).Result;
                }
            }
            return response;
        }
        private static HttpResponseMessage Post<T>(Dictionary<string, string> resource, Dictionary<string, string> head, dynamic requesParams)
        {
            if (resource == null)
                return null;
            if (string.IsNullOrEmpty(resource["BaseUrl"]))
                throw new ArgumentNullException("baseUrl not is null!");
            string resourceUrl = resource["Value"];
            //  string requestJson = "{}";
            if (string.IsNullOrEmpty(resourceUrl))
                throw new ArgumentNullException("resource not is null!");
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(resource["BaseUrl"]);
                // Add an Accept header for JSON format.
                // 为JSON格式添加一个Accept报头
                //注意这里的格式哦，为 "username:password"
                //  mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
                //  myReq.Credentials = mycache;
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                if (head != null)
                {
                    foreach (var key in head.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key,
                                                head[key]);
                    }
                }
                if (requesParams is string)
                {
                    response = HttpClientExtensions.PostAsJsonAsync(client, resourceUrl, requesParams).Result;
                }
                else
                {
                    MediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
                    // Use the JSON formatter to create the content of the request body.
                    // 使用JSON格式化器创建请求体内容。
                    HttpContent content = new ObjectContent<T>(requesParams, jsonFormatter);
                    response = client.PostAsync(resourceUrl, content).Result;
                }
            }
            return response;
        }
        private static HttpResponseMessage Put(Dictionary<string, string> resource, Dictionary<string, string> head, dynamic requesParams)
        {
            if (resource == null)
                return null;
            if (string.IsNullOrEmpty(resource["BaseUrl"]))
                throw new ArgumentNullException("baseUrl not is null!");
            string resourceUrl = resource["Value"];
            if (string.IsNullOrEmpty(resourceUrl))
                throw new ArgumentNullException("resource not is null!");
            HttpResponseMessage response = null;
            if (requesParams != null && requesParams is IDictionary)
                resourceUrl += GetParamsUrl(requesParams);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(resource["BaseUrl"]);
                // Add an Accept header for JSON format.
                // 为JSON格式添加一个Accept报头
                //注意这里的格式哦，为 "username:password"
                //  mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
                //  myReq.Credentials = mycache;
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                if (head != null)
                {
                    foreach (var key in head.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key,
                                                head[key]);
                    }
                }
                if (requesParams is string)
                {
                    response = HttpClientExtensions.PutAsJsonAsync(client, resourceUrl, requesParams).Result;
                }
                else
                {
                    MediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();

                    // Use the JSON formatter to create the content of the request body.
                    // 使用JSON格式化器创建请求体内容。
                    HttpContent content = new ObjectContent<string>(requesParams, jsonFormatter);

                    response = client.PutAsync(resourceUrl, content).Result;
                }
            }
            return response;
        }

        private static HttpResponseMessage PostForm(Dictionary<string, string> resource, Dictionary<string, string> head, Dictionary<string, string> formParams)
        {
            if (resource == null)
                return null;
            if (string.IsNullOrEmpty(resource["BaseUrl"]))
                throw new ArgumentNullException("baseUrl not is null!");
            string resourceUrl = resource["Value"];
            if (string.IsNullOrEmpty(resourceUrl))
                throw new ArgumentNullException("resource not is null!");
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(resource["BaseUrl"]);
                // Add an Accept header for JSON format.
                // 为JSON格式添加一个Accept报头
                //注意这里的格式哦，为 "username:password"
                //  mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
                //  myReq.Credentials = mycache;
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                if (head != null)
                {
                    foreach (var key in head.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key,
                                                head[key]);
                    }
                }
                string text2 = "";
                if (formParams != null)
                {
                    foreach (string current in formParams.Keys)
                    {
                        string text3 = text2;
                        text2 = string.Concat(new string[]
                     {
                        text3,
                        "&",
                        current,
                        "=",
                        System.Web.HttpUtility.UrlEncode(formParams[current])
                     });
                    }
                    if (text2.Length > 0)
                    {
                        text2 = text2.Substring(1);
                    }
                }
                // Use the JSON formatter to create the content of the request body.
                // 使用JSON格式化器创建请求体内容。
                StringContent stringContent = new StringContent(text2, Encoding.UTF8, "application/x-www-form-urlencoded");
                response = client.PostAsync(resourceUrl, stringContent).Result;

            }
            return response;
        }
        private static HttpResponseMessage Put<T>(Dictionary<string, string> resource, Dictionary<string, string> head, dynamic requesParams)
        {
            if (resource == null)
                return null;
            if (string.IsNullOrEmpty(resource["BaseUrl"]))
                throw new ArgumentNullException("baseUrl not is null!");
            string resourceUrl = resource["Value"];
            if (string.IsNullOrEmpty(resourceUrl))
                throw new ArgumentNullException("resource not is null!");
            HttpResponseMessage response = null;
            if (requesParams != null && requesParams is IDictionary)
                resourceUrl += GetParamsUrl(requesParams);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(resource["BaseUrl"]);
                // Add an Accept header for JSON format.
                // 为JSON格式添加一个Accept报头
                //注意这里的格式哦，为 "username:password"
                //  mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
                //  myReq.Credentials = mycache;
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                if (head != null)
                {
                    foreach (var key in head.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key,
                                                head[key]);
                    }
                }
                if (requesParams is string)
                {
                    response = HttpClientExtensions.PutAsJsonAsync(client, resourceUrl, requesParams).Result;
                }
                else
                {
                    MediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
                    // Use the JSON formatter to create the content of the request body.
                    // 使用JSON格式化器创建请求体内容。
                    HttpContent content = new ObjectContent<T>(requesParams, jsonFormatter);
                    response = client.PutAsync(resourceUrl, content).Result;
                }
            }
            return response;
        }
        private static HttpResponseMessage Delete(Dictionary<string, string> resource, Dictionary<string, string> head)
        {
            if (string.IsNullOrEmpty(resource["BaseUrl"]))
                throw new ArgumentNullException("baseUrl not is null!");
            if (string.IsNullOrEmpty(resource["Value"]))
                throw new ArgumentNullException("resource not is null!");
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(resource["BaseUrl"]);
                // Add an Accept header for JSON format.
                // 为JSON格式添加一个Accept报头
                //注意这里的格式哦，为 "username:password"
                //  mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
                //  myReq.Credentials = mycache;
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                if (head != null)
                {
                    foreach (var key in head.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key,
                                                head[key]);
                    }
                }
                response = client.DeleteAsync(resource["BaseUrl"]).Result;
            }
            return response;
        }
        private static HttpResponseMessage UploadFile(Dictionary<string, string> resource, Dictionary<string, string> head, Stream stream, string fileName)
        {
            if (resource == null)
                return null;
            if (string.IsNullOrEmpty(resource["BaseUrl"]))
                throw new ArgumentNullException("baseUrl not is null!");
            if (string.IsNullOrEmpty(resource["Value"]))
                throw new ArgumentNullException("resource not is null!");
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(resource["BaseUrl"]);
                // Add an Accept header for JSON format.
                // 为JSON格式添加一个Accept报头
                //注意这里的格式哦，为 "username:password"
                //  mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
                //  myReq.Credentials = mycache;
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                if (head != null)
                {
                    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/multipart/form-data"));
                    foreach (var key in head.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key,
                                                head[key]);
                    }
                }
                // Create a stream content for the file
                StreamContent content = new StreamContent(stream);
                // Create Multipart form data content, add our submitter data and our stream content
                MultipartFormDataContent formData = new MultipartFormDataContent();
                formData.Add(content, "filename", fileName);
                // Post the MIME multipart form data upload with the file
                response = client.PutAsync(resource["Value"], formData).Result;
            }
            return response;
        }
        private static HttpResponseMessage UploadFileNew(Dictionary<string, string> resource, Dictionary<string, string> head, Stream stream, string fileName, Dictionary<string, string> postParam)
        {
            if (resource == null)
                return null;
            if (string.IsNullOrEmpty(resource["BaseUrl"]))
                throw new ArgumentNullException("baseUrl not is null!");
            if (string.IsNullOrEmpty(resource["Value"]))
                throw new ArgumentNullException("resource not is null!");
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(resource["BaseUrl"]);
                // Add an Accept header for JSON format.
                // 为JSON格式添加一个Accept报头
                //注意这里的格式哦，为 "username:password"
                //  mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
                //  myReq.Credentials = mycache;
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                if (head != null)
                {
                    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/multipart/form-data"));
                    foreach (var key in head.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key,
                                                head[key]);
                    }
                }
                // Create a stream content for the file
                StreamContent content = new StreamContent(stream);
                // Create Multipart form data content, add our submitter data and our stream content
                MultipartFormDataContent formData = new MultipartFormDataContent();
                formData.Add(content, "filename", fileName);
                if (postParam != null)
                {
                    foreach (var item in postParam)
                    {
                        formData.Add(content, item.Key, item.Value);
                    }
                }
                // Post the MIME multipart form data upload with the file
                response = client.PutAsync(resource["Value"], formData).Result;
            }
            return response;
        }
        private static HttpResponseMessage UploadPostFile(Dictionary<string, string> resource, Dictionary<string, string> head, Stream stream, string fileName)
        {
            if (resource == null)
                return null;
            if (string.IsNullOrEmpty(resource["BaseUrl"]))
                throw new ArgumentNullException("baseUrl not is null!");
            if (string.IsNullOrEmpty(resource["Value"]))
                throw new ArgumentNullException("resource not is null!");
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(resource["BaseUrl"]);
                // Add an Accept header for JSON format.
                // 为JSON格式添加一个Accept报头
                //注意这里的格式哦，为 "username:password"
                //  mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
                //  myReq.Credentials = mycache;
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                if (head != null)
                {
                    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/multipart/form-data"));
                    //foreach (var key in head.Keys)
                    //{
                    //    client.DefaultRequestHeaders.Add(key,
                    //                            head[key]);
                    //}
                }
                // Create a stream content for the file
                StreamContent content = new StreamContent(stream);
                content.Headers.Add("Content-Type", "application/octet-stream");
                // Create Multipart form data content, add our submitter data and our stream content
                MultipartFormDataContent formData = new MultipartFormDataContent();
                formData.Add(content, "file", fileName);
                // Post the MIME multipart form data upload with the file
                response = client.PostAsync(resource["Value"], formData).Result;
            }
            return response;
        }
        private static HttpResponseMessage UploadPostFileNew(Dictionary<string, string> resource, Dictionary<string, string> head, Stream stream, string fileName, Dictionary<string, string> postParam)
        {
            if (resource == null)
                return null;
            if (string.IsNullOrEmpty(resource["BaseUrl"]))
                throw new ArgumentNullException("baseUrl not is null!");
            if (string.IsNullOrEmpty(resource["Value"]))
                throw new ArgumentNullException("resource not is null!");
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(resource["BaseUrl"]);
                // Add an Accept header for JSON format.
                // 为JSON格式添加一个Accept报头
                //注意这里的格式哦，为 "username:password"
                //  mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
                //  myReq.Credentials = mycache;
                //client.DefaultRequestHeaders.Accept.Add(
                //    new MediaTypeWithQualityHeaderValue("application/json"));
                if (head != null)
                {
                    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/multipart/form-data"));
                    //foreach (var key in head.Keys)
                    //{
                    //    client.DefaultRequestHeaders.Add(key,
                    //                            head[key]);
                    //}
                }
                // Create a stream content for the file
                StreamContent content = new StreamContent(stream);
                content.Headers.Add("Content-Type", "application/octet-stream");
                // Create Multipart form data content, add our submitter data and our stream content
                MultipartFormDataContent formData = new MultipartFormDataContent();
                formData.Add(content, "file", fileName);
                if (postParam != null)
                {
                    foreach (var item in postParam)
                    {
                        formData.Add(content, item.Key, item.Value);
                    }
                }

                // Post the MIME multipart form data upload with the file
                response = client.PostAsync(resource["Value"], formData).Result;
            }
            return response;
        }
        public static string UploadFileReturnJsonNew(Dictionary<string, string> resource, Dictionary<string, string> head, Stream stream, string fileName, Dictionary<string, string> postParam, string method = "PUT")
        {
            string json = "";
            HttpResponseMessage response = null;
            if (method.Equals("PUT"))
            {
                response = UploadFileNew(resource, head, stream, fileName, postParam);
            }
            else
            {
                response = UploadPostFileNew(resource, head, stream, fileName, postParam);
            }
            if (response != null && response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                json = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                //throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return json;
        }

        public static string UploadFileReturnJson(Dictionary<string, string> resource, Dictionary<string, string> head, Stream stream, string fileName, string method = "PUT")
        {
            string json = "";
            HttpResponseMessage response = null;
            if (method.Equals("PUT"))
            {
                response = UploadFile(resource, head, stream, fileName);
            }
            else
            {
                response = UploadPostFile(resource, head, stream, fileName);
            }
            if (response != null && response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                json = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                //throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return json;
        }
        public static Stream UploadFileReturnStream(Dictionary<string, string> resource, Dictionary<string, string> head, Stream stream, string fileName, string method = "PUT")
        {
            Stream streamResult;
            HttpResponseMessage response = null;
            if (method.Equals("PUT"))
            {
                response = UploadFile(resource, head, stream, fileName);

            }
            else
            {
                response = UploadPostFile(resource, head, stream, fileName);
            }
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                streamResult = response.Content.ReadAsStreamAsync().Result;
            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return streamResult;
        }
        public static string GetReturnJson(Dictionary<string, string> resource, Dictionary<string, string> head, Dictionary<string, string> requesParams)
        {
            string jsonResult = "";
            HttpResponseMessage response = Get(resource, head, requesParams);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                jsonResult = response.Content.ReadAsStringAsync().Result;

            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return jsonResult;
        }
        public static Stream GetReturnStream(Dictionary<string, string> resource, Dictionary<string, string> head, Dictionary<string, string> requesParams)
        {
            Stream streamResult;
            HttpResponseMessage response = Get(resource, head, requesParams);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                streamResult = response.Content.ReadAsStreamAsync().Result;

            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return streamResult;
        }
        public static T GetReturnObject<T>(Dictionary<string, string> resource, Dictionary<string, string> head, Dictionary<string, string> requesParams)
        {
            T resultObj = default(T);
            HttpResponseMessage response = Get(resource, head, requesParams);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                resultObj = response.Content.ReadAsAsync<T>().Result;
            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return resultObj;
        }

        public static string PostReturnJson(Dictionary<string, string> resource, Dictionary<string, string> head, dynamic requesParams)
        {
            string jsonResult = "";
            HttpResponseMessage response = Post(resource, head, requesParams);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                jsonResult = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return jsonResult;
        }
        public static string PostReturnJson<T>(Dictionary<string, string> resource, Dictionary<string, string> head, dynamic requesParams)
        {
            string jsonResult = "";
            HttpResponseMessage response = Post<T>(resource, head, requesParams);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                jsonResult = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return jsonResult;
        }
        public static T PostReturnObject<T>(Dictionary<string, string> resource, Dictionary<string, string> head, dynamic requesParams)
        {
            T jsonObj = default(T);
            HttpResponseMessage response = Post(resource, head, requesParams);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                jsonObj = response.Content.ReadAsAsync<T>().Result;
            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return jsonObj;
        }
        public static string PutReturnJson(Dictionary<string, string> resource, Dictionary<string, string> head, dynamic requesParams)
        {
            string jsonResult = "";
            HttpResponseMessage response = Put(resource, head, requesParams);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                jsonResult = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return jsonResult;
        }
        public static string PostFormJson(Dictionary<string, string> resource, Dictionary<string, string> head, Dictionary<string, string> formParams)
        {
            string jsonResult = "";
            HttpResponseMessage response = PostForm(resource, head, formParams);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                jsonResult = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return jsonResult;
        }

        public static string PutReturnJson<T>(Dictionary<string, string> resource, Dictionary<string, string> head, dynamic requesParams)
        {
            string jsonResult = "";
            HttpResponseMessage response = Put<T>(resource, head, requesParams);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                jsonResult = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return jsonResult;
        }
        public static T PutReturnObject<T>(Dictionary<string, string> resource, Dictionary<string, string> head, dynamic requesParams)
        {
            T jsonObj = default(T);
            HttpResponseMessage response = Put(resource, head, requesParams);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                jsonObj = response.Content.ReadAsAsync<T>().Result;
            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return jsonObj;

        }
        public static string DeleteReturnJson(Dictionary<string, string> resource, Dictionary<string, string> head)
        {
            string json = "";
            HttpResponseMessage response = Delete(resource, head);
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                // 解析响应体。阻塞！
                json = response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                throw new RequestApiException(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
            }
            return json;
        }
    }
    public class RequestApiException : Exception
    {
        public RequestApiException(string message)
            : base(message)
        {
        }
        public RequestApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
