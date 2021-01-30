using Microsoft.VisualStudio.TestTools.UnitTesting;
using HD.Helper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Net.Http.Formatting;
using System.Threading.Tasks;
namespace HD.Helper.Common.Tests
{
    [TestClass()]
    public class MultipartFormTests
    {
        [TestMethod()]
        public async void HttpClientGetDemo()
        {//https://csharp.hotexamples.com/examples/System.Net.Http/HttpClient/PostAsJsonAsync/php-httpclient-postasjsonasync-method-examples.html
            var client = new HttpClient();// met het gebruik van using zorgen we ervoor dat alles netjes wordt opgeruimd als je buiten de scope zit. 

            client.BaseAddress = new Uri("http://localhost:50752/"); //de URL waar onze website op draait
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); //we krijgen JSON terug van onze API

            // HTTP GET
            HttpResponseMessage response = client.GetAsync("api/RekeningsApi/1").Result; //ik wil de eerste klant ophalen. Deze bevindt zich in de api, de RekeningsApiController en dan nummer 1
            if (response.IsSuccessStatusCode) //Controleren of we geen 404 krijgen, maar iets in de 200-reeks
            {
                //zetten de response asynchroon om in een rekening-object
                Console.WriteLine("Rekeningnaam: {0}\tBeschrijving: {1}\tSaldo: {2}");
            }
            //Console.ReadKey(); //even op een toets drukken om verder te gaan

        }

        [TestMethod()]
        public async void HttpClientPostDemo()
        {

            var targeturi = "https://ismaelc-facebooktest.p.mashape.com/oauth_url";

            var client = new System.Net.Http.HttpClient();

            HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("consumerKey"," App.ConsumerKey"),
                new KeyValuePair<string, string>("consumerSecret", "App.ConsumerSecret"),
                new KeyValuePair<string, string>("callbackUrl", "App.CallbackUrl")
            });

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            content.Headers.Add("X-Mashape-Authorization", " App.MashapeHeader");


            var response = client.PostAsync(targeturi, content).Result;


            if (response.IsSuccessStatusCode)
            {
                string respContent = response.Content.ReadAsStringAsync().Result;
                //await response.Content.ReadAsStringAsync();
                //                string loginUrl = await Task.(() => JsonObject.Parse(respContent).GetNamedString("url"));
            }

        }

        [TestMethod()]
        public async void HttpClientPostFileDemo()
        {

            var targeturi = "https://ismaelc-facebooktest.p.mashape.com/oauth_url";

            var client = new System.Net.Http.HttpClient();
            MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("dsxx.txt"));
            // Create a stream content for the file
            StreamContent content = new StreamContent(stream);
            // Create Multipart form data content, add our submitter data and our stream content
            MultipartFormDataContent formData = new MultipartFormDataContent();
            formData.Add(content, "filename", "aa.txt");
            formData.Add(content, "id", "123");


            var response = client.PostAsync(targeturi, content).Result;


            if (response.IsSuccessStatusCode)
            {
                string respContent = response.Content.ReadAsStringAsync().Result;
                //await response.Content.ReadAsStringAsync();
                //                string loginUrl = await Task.(() => JsonObject.Parse(respContent).GetNamedString("url"));
            }

        }
        [TestMethod()]
        public async void HttpClientExtensionsPostDemo()
        {

            var targeturi = "https://ismaelc-facebooktest.p.mashape.com/oauth_url";

            var client = new System.Net.Http.HttpClient();
            client.BaseAddress = new Uri(targeturi);
            HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("consumerKey"," App.ConsumerKey"),
                new KeyValuePair<string, string>("consumerSecret", "App.ConsumerSecret"),
                new KeyValuePair<string, string>("callbackUrl", "App.CallbackUrl")
            });

            //使用JSON格式化器创建请求体内容。
            //MediaTypeFormatter jsonFormatter = new JsonMediaTypeFormatter();
            //HttpContent content = new ObjectContent<string>(requesParams, jsonFormatter);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            content.Headers.Add("X-Mashape-Authorization", " App.MashapeHeader");
        //    var response = HttpClientExtensions.PostAsJsonAsync(client, targeturi, new { rquest = "1" }).Result;

        }
        [TestMethod()]
        public void MultipartFormTest()
        {

            try
            {
                HD.Helper.Common.WebClientHelper webClient = new HD.Helper.Common.WebClientHelper();
                webClient.Encoding = System.Text.Encoding.UTF8;
                var fileList = System.IO.Directory.GetFiles("D:\\", "*.*", SearchOption.TopDirectoryOnly).ToList().Take(5);
                foreach (var item in fileList)
                {

                    Console.WriteLine(item);
                    string filepath = item;
                    string filename = System.IO.Path.GetFileName(filepath);
                    System.IO.FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    byte[] fileData = new byte[stream.Length];
                    stream.Read(fileData, 0, fileData.Length);


                    string url = "http://192.168.253.125/Document/File_Update.aspx?tkn=00000dfaaaaed89e4568a10c90b522573603&upload=UPDATE_1305031";
                    long file_path = stream.Length;
                    int folder_id = 10;
                    //{0}\1{1}\1{2}\1{3}\1{4}\1{5}\1{6}, this.folder_id, this.file_name, this.file_remark, this.file_path, masterFileId, attachType, this.metaData
                    string objfile = string.Format(@"{0}\1{1}\1{2}\1{3}\1{4}\1{5}\1{6}", folder_id, filename, filename, (int)file_path, 0, 0, "").Replace("\\1", ((char)1).ToString());


                    HD.Helper.Common.MultipartForm multipartForm = new HD.Helper.Common.MultipartForm();
                    multipartForm.AddString("FILE_MODE", "Upload");
                    multipartForm.AddString("FILE_INFO", objfile);
                    multipartForm.AddFlie(filename, filename, fileData, (int)file_path);


                    string result = webClient.Post(url, multipartForm);


                    Console.WriteLine(result);
                }
                Console.ReadLine();
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee);
            }
            Assert.Fail();
        }
    }
}