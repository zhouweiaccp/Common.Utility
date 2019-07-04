using System;  
using System.Configuration;  
using System.Web;  
using System.Web.Configuration;  
  
namespace Whir.ezEIP  
{  
    /// <summary>  
    /// WebConfig读写辅助类  var config = new WebConfigManager();  config.SetAppSetting("StrRegex", "");   https://www.cnblogs.com/zhangqs008/p/3773630.html
    /// </summary>  
    public class WebConfigManager : IDisposable  
    {  
        private Configuration _config;  
  
        /// <summary>  
        /// WebConfig读写辅助类  
        /// </summary>  
        public WebConfigManager()  
            : this(HttpContext.Current.Request.ApplicationPath)  
        {  
        }  
        /// <summary>  
        /// WebConfig读写辅助类  
        /// </summary>  
        /// <param name="path"></param>  
        public WebConfigManager(string path)  
        {  
            _config = WebConfigurationManager.OpenWebConfiguration(path);  
        }  
 
        #region IDisposable Members  
  
        public void Dispose()  
        {  
            if (_config != null)  
            {  
                _config.Save();  
            }  
        }  
 
        #endregion  
  
        /// <summary>   
        /// 设置应用程序配置节点，如果已经存在此节点，则会修改该节点的值，否则添加此节点  
        /// </summary>   
        /// <param name="key">节点名称</param>   
        /// <param name="value">节点值</param>   
        public void SetAppSetting(string key, string value)  
        {  
            var appSetting = (AppSettingsSection)_config.GetSection("appSettings");  
            if (appSetting.Settings[key] == null) //如果不存在此节点，则添加   
            {  
                appSetting.Settings.Add(key, value);  
            }  
            else //如果存在此节点，则修改   
            {  
                appSetting.Settings[key].Value = value;  
            }  
            Save();  
        }  
  
        /// <summary>   
        /// 设置数据库连接字符串节点，如果不存在此节点，则会添加此节点及对应的值，存在则修改   
        /// </summary>   
        /// <param name="key">节点名称</param>  
        /// <param name="connectionString"> </param>  
        public void SetConnectionString(string key, string connectionString)  
        {  
            var connectionSetting = (ConnectionStringsSection)_config.GetSection("connectionStrings");  
            if (connectionSetting.ConnectionStrings[key] == null) //如果不存在此节点，则添加   
            {  
                var connectionStringSettings = new ConnectionStringSettings(key, connectionString);  
                connectionSetting.ConnectionStrings.Add(connectionStringSettings);  
            }  
            else //如果存在此节点，则修改   
            {  
                connectionSetting.ConnectionStrings[key].ConnectionString = connectionString;  
            }  
            Save();  
        }  
  
        /// <summary>   
        /// 保存所作的修改   
        /// </summary>   
        public void Save()  
        {  
            _config.Save();  
            _config = null;  
        }  
    }  
}  