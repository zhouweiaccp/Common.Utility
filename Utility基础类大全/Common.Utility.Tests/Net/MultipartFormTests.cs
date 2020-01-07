using Microsoft.VisualStudio.TestTools.UnitTesting;
using HD.Helper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HD.Helper.Common.Tests
{
    [TestClass()]
    public class MultipartFormTests
    {
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