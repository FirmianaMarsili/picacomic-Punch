/// <summary>
/// 类说明：HttpHelps类，用来实现Http访问，Post或者Get方式的，直接访问，带Cookie的，带证书的等方式，可以设置代理
/// 编码日期：2011-09-20
/// 编 码 人：苏飞
/// 联系方式：361983679  
/// 更新网址：http://www.cckan.net/thread-3-1-1.html
/// 修改日期：2012-12-09
/// </summary>

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace DotNet.Utilities
{
    /// <summary>
    /// Http连接操作帮助类 
    /// <example>
    /// 下面是个例子大家可以看一下
    /// 
    ///  HttpHelper http = new HttpHelper();
    ///  HttpItem item = new HttpItem()
    ///  {
    ///   URL = "http://www.cckan.net",//URL     必需项
    ///   Encoding = "gbk",//编码格式（utf-8,gb2312,gbk）     可选项 默认类会自动识别
    ///   Method = "get",//URL     可选项 默认为Get
    ///   //Timeout = 100000,//连接超时时间     可选项默认为100000
    ///   //ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
    ///   //IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写
    ///   Cookie = "",//字符串Cookie     可选项
    ///   // UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)",//用户的浏览器类型，版本，操作系统     可选项有默认值
    ///   // Accept = "text/html, application/xhtml+xml, */*",//    可选项有默认值
    ///   // ContentType = "text/html",//返回类型    可选项有默认值
    ///   Referer = "http://www.cckan.net",//来源URL     可选项
    ///   //Allowautoredirect = true,//是否根据３０１跳转     可选项
    ///   //CerPath = "d:\\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数
    ///   //Connectionlimit = 1024,//最大连接数     可选项 默认为1024
    ///   //Postdata = "username=sufei&pwd=cckan.net",//Post数据     可选项GET时不需要写
    ///   //ProxyIp = "192.168.1.105",//代理服务器ID     可选项 不需要代理 时可以不设置这三个参数
    ///   //ProxyPwd = "123456",//代理服务器密码     可选项
    ///   // ProxyUserName = "administrator",//代理服务器账户名     可选项
    ///  };
    ///  //得到HTML代码
    ///  string html = http.GetHtml(item);
    ///
    ///  //取出返回的Cookie
    ///  string cookie = item.Cookie;
    ///  //取出返回的Request
    ///  HttpWebRequest request = item.Request;
    ///  //取出返回的Response
    ///  HttpWebResponse response = item.Response;
    ///  //取出返回的Reader
    ///  StreamReader reader = item.Reader;
    ///  //取出返回的Headers
    ///  WebHeaderCollection header = response.Headers;
    /// </example>
    /// </summary>
    public class HttpHelper
    {
        #region 预定义方法或者变更

        //默认的编码
        private Encoding encoding = Encoding.Default;
        //HttpWebRequest对象用来发起请求
        private HttpWebRequest request = null;
        //获取影响流的数据对象
        private HttpWebResponse response = null;
        //读取流的对象
        private StreamReader reader = null;
        //需要返回的数据对象
        private string returnData = "String Error";

        /// <summary>
        /// 根据相传入的数据，得到相应页面数据
        /// </summary>
        /// <param name="strPostdata">传入的数据Post方式,get方式传NUll或者空字符串都可以</param>
        /// <returns>string类型的响应数据</returns>
        private string GetHttpRequestData(HttpItem objhttpitem)
        {
            try
            {
                #region 得到请求的response

                using (response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.Cookies != null)
                    {
                        objhttpitem.CookieCollection = response.Cookies;
                    }
                    if (response.Headers["set-cookie"] != null)
                    {
                        objhttpitem.Cookie = response.Headers["set-cookie"];
                    }

                    objhttpitem.Response = response;
                    objhttpitem.Request = request;
                    //从这里开始我们要无视编码了
                    if (encoding == null)
                    {
                        MemoryStream _stream = new MemoryStream();
                        if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                        {
                            objhttpitem.Reader = reader;
                            //开始读取流并设置编码方式
                            //new GZipStream(response.GetResponseStream(), CompressionMode.Decompress).CopyTo(_stream, 10240);
                            //.net4.0以下写法
                            _stream = GetMemoryStream(response.GetResponseStream());
                        }
                        else
                        {
                            objhttpitem.Reader = reader;
                            //response.GetResponseStream().CopyTo(_stream, 10240);
                            // .net4.0以下写法
                            _stream = GetMemoryStream(response.GetResponseStream());
                        }
                        byte[] RawResponse = _stream.ToArray();
                        string temp = Encoding.Default.GetString(RawResponse, 0, RawResponse.Length);
                        //<meta(.*?)charset([\s]?)=[^>](.*?)>
                        Match meta = Regex.Match(temp, "<meta([^<]*)charset=([^<]*)[\"']", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        string charter = (meta.Groups.Count > 2) ? meta.Groups[2].Value : string.Empty;
                        charter = charter.Replace("\"", string.Empty).Replace("'", string.Empty).Replace(";", string.Empty);
                        if (charter.Length > 0)
                        {
                            charter = charter.ToLower().Replace("iso-8859-1", "gbk");
                            encoding = Encoding.GetEncoding(charter);
                        }
                        else
                        {
                            if (response.CharacterSet.ToLower().Trim() == "iso-8859-1")
                            {
                                encoding = Encoding.GetEncoding("GBK");
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(response.CharacterSet.Trim()))
                                {
                                    encoding = Encoding.UTF8;
                                }
                                else
                                {
                                    encoding = Encoding.GetEncoding(response.CharacterSet);
                                }
                            }
                        }
                        returnData = encoding.GetString(RawResponse);
                    }
                    else
                    {
                        if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
                        {
                            //开始读取流并设置编码方式
                            using (reader = new StreamReader(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress), encoding))
                            {
                                objhttpitem.Reader = reader;
                                returnData = reader.ReadToEnd();

                            }
                        }
                        else
                        {
                            //开始读取流并设置编码方式
                            using (reader = new StreamReader(response.GetResponseStream(), encoding))
                            {
                                objhttpitem.Reader = reader;
                                returnData = reader.ReadToEnd();
                            }
                        }
                    }
                }

                #endregion
            }
            catch (WebException ex)
            {
                //这里是在发生异常时返回的错误信息
                returnData = "String Error";
                response = (HttpWebResponse)ex.Response;
                objhttpitem.Response = response;
            }
            if (objhttpitem.IsToLower)
            {
                returnData = returnData.ToLower();
            }
            return returnData;
        }

        /// <summary>
        /// 4.0以下.net版本取数据使用
        /// </summary>
        /// <param name="streamResponse">流</param>
        private static MemoryStream GetMemoryStream(Stream streamResponse)
        {
            MemoryStream _stream = new MemoryStream();
            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = streamResponse.Read(buffer, 0, Length);
            // write the required bytes  
            while (bytesRead > 0)
            {
                _stream.Write(buffer, 0, bytesRead);
                bytesRead = streamResponse.Read(buffer, 0, Length);
            }
            return _stream;
        }

        /// <summary>
        /// 为请求准备参数
        /// </summary>
        ///<param name="objhttpItem">参数列表</param>
        /// <param name="_Encoding">读取数据时的编码方式</param>
        private void SetRequest(HttpItem objhttpItem)
        {
            #region 验证证书

            if (!string.IsNullOrEmpty(objhttpItem.CerPath))
            {
                //这一句一定要写在创建连接的前面。使用回调的方法进行证书验证。
                ServicePointManager.ServerCertificateValidationCallback =
                    new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);

                //初始化对像，并设置请求的URL地址
                request = (HttpWebRequest)WebRequest.Create(GetUrl(objhttpItem.URL));
                //创建证书文件
                X509Certificate objx509 = new X509Certificate(objhttpItem.CerPath);
                //添加到请求里
                request.ClientCertificates.Add(objx509);
            }
            else
            {
                //初始化对像，并设置请求的URL地址
                request = (HttpWebRequest)WebRequest.Create(GetUrl(objhttpItem.URL));
            }
            #endregion

            #region 设置代理
            if (string.IsNullOrEmpty(objhttpItem.ProxyUserName) && string.IsNullOrEmpty(objhttpItem.ProxyPwd) && string.IsNullOrEmpty(objhttpItem.ProxyIp))
            {
                //不需要设置
            }
            else
            {
                //设置代理服务器
                WebProxy myProxy = new WebProxy(objhttpItem.ProxyIp, false);
                //建议连接
                myProxy.Credentials = new NetworkCredential(objhttpItem.ProxyUserName, objhttpItem.ProxyPwd);
                //给当前请求对象
                request.Proxy = myProxy;
                //设置安全凭证
                request.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            #endregion

            //请求方式Get或者Post
            request.Method = objhttpItem.Method;
            request.Timeout = objhttpItem.Timeout;
            request.ReadWriteTimeout = objhttpItem.ReadWriteTimeout;
            //Accept
            request.Accept = objhttpItem.Accept;
            //ContentType返回类型
            request.ContentType = objhttpItem.ContentType;
            //UserAgent客户端的访问类型，包括浏览器版本和操作系统信息
            request.UserAgent = objhttpItem.UserAgent;

            #region 编码
            if (string.IsNullOrEmpty(objhttpItem.Encoding) || objhttpItem.Encoding.ToLower().Trim() == "null")
            {
                //读取数据时的编码方式
                encoding = null;
            }
            else
            {
                //读取数据时的编码方式
                encoding = System.Text.Encoding.GetEncoding(objhttpItem.Encoding);
            }
            #endregion

            #region Cookie
            if (!string.IsNullOrEmpty(objhttpItem.Cookie))
            {
                //Cookie
                request.Headers[HttpRequestHeader.Cookie] = objhttpItem.Cookie;
            }

            //设置Cookie
            if (objhttpItem.CookieCollection != null)
            {
                if (request.CookieContainer.Count == 0)
                {
                    request.CookieContainer.Add(objhttpItem.CookieCollection);
                }
                else
                {
                    request.CookieContainer = new CookieContainer();
                    request.CookieContainer.Add(objhttpItem.CookieCollection);
                }
            }
            #endregion

            //来源地址
            request.Referer = objhttpItem.Referer;
            //是否执行跳转功能
            request.AllowAutoRedirect = objhttpItem.Allowautoredirect;

            #region Post数据
            //验证在得到结果时是否有传入数据
            if (!string.IsNullOrEmpty(objhttpItem.Postdata) && request.Method.Trim().ToLower().Contains("post"))
            {
                byte[] buffer = Encoding.Default.GetBytes(objhttpItem.Postdata);
                request.ContentLength = buffer.Length;
                request.GetRequestStream().Write(buffer, 0, buffer.Length);
            }
            #endregion

            //设置最大连接
            if (objhttpItem.Connectionlimit > 0)
            {
                request.ServicePoint.ConnectionLimit = objhttpItem.Connectionlimit;
            }
        }

        //回调验证证书问题
        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            // 总是接受    
            return true;
        }

        #endregion

        #region 普通类型

        /// <summary>    
        /// 传入一个正确或不正确的URl，返回正确的URL
        /// </summary>    
        /// <param name="URL">url</param>   
        /// <returns>
        /// </returns>    
        public static string GetUrl(string URL)
        {
            if (!(URL.Contains("http://") || URL.Contains("https://")))
            {
                URL = "http://" + URL;
            }
            return URL;
        }

        ///<summary>
        ///采用https协议访问网络,根据传入的URl地址，得到响应的数据字符串。
        ///</summary>
        ///<param name="objhttpItem">参数列表</param>
        ///<returns>String类型的数据</returns>
        public string GetHtml(HttpItem objhttpItem)
        {
            //准备参数
            SetRequest(objhttpItem);

            //调用专门读取数据的类
            return GetHttpRequestData(objhttpItem);
        }
        #endregion
    }

    /// <summary>
    /// Http请求参考类 
    /// </summary>
    public class HttpItem
    {
        string _URL;
        /// <summary>
        /// 请求URL必须填写
        /// </summary>
        public string URL
        {
            get { return _URL; }
            set { _URL = value; }
        }

        string _Method = "GET";
        /// <summary>
        /// 请求方式默认为GET方式
        /// </summary>
        public string Method
        {
            get { return _Method; }
            set { _Method = value; }
        }

        int _Timeout = 100000;
        /// <summary>
        /// 默认请求超时时间
        /// </summary>
        public int Timeout
        {
            get { return _Timeout; }
            set { _Timeout = value; }
        }

        int _ReadWriteTimeout = 30000;
        /// <summary>
        /// 默认写入Post数据超时间
        /// </summary>
        public int ReadWriteTimeout
        {
            get { return _ReadWriteTimeout; }
            set { _ReadWriteTimeout = value; }
        }

        string _Accept = "text/html, application/xhtml+xml, */*";
        /// <summary>
        /// 请求标头值 默认为text/html, application/xhtml+xml, */*
        /// </summary>
        public string Accept
        {
            get { return _Accept; }
            set { _Accept = value; }
        }

        string _ContentType = "text/html";
        /// <summary>
        /// 请求返回类型默认 text/html
        /// </summary>
        public string ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; }
        }

        string _UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
        /// <summary>
        /// 客户端访问信息默认Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)
        /// </summary>
        public string UserAgent
        {
            get { return _UserAgent; }
            set { _UserAgent = value; }
        }

        string _Encoding = string.Empty;
        /// <summary>
        /// 返回数据编码默认为NUll,可以自动识别
        /// </summary>
        public string Encoding
        {
            get { return _Encoding; }
            set { _Encoding = value; }
        }

        string _Postdata;
        /// <summary>
        /// Post请求时要发送的Post数据
        /// </summary>
        public string Postdata
        {
            get { return _Postdata; }
            set { _Postdata = value; }
        }

        string _Cookie = string.Empty;
        /// <summary>
        /// 请求时的Cookie
        /// </summary>
        public string Cookie
        {
            get { return _Cookie; }
            set { _Cookie = value; }
        }

        string _Referer = string.Empty;
        /// <summary>
        /// 来源地址，上次访问地址
        /// </summary>
        public string Referer
        {
            get { return _Referer; }
            set { _Referer = value; }
        }

        string _CerPath = string.Empty;
        /// <summary>
        /// 证书绝对路径
        /// </summary>
        public string CerPath
        {
            get { return _CerPath; }
            set { _CerPath = value; }
        }

        CookieCollection cookiecollection = null;
        /// <summary>
        /// Cookie对象集合
        /// </summary>
        public CookieCollection CookieCollection
        {
            get { return cookiecollection; }
            set { cookiecollection = value; }
        }

        private HttpWebRequest request = null;
        /// <summary>
        /// HttpWebRequest对象用来发起请求
        /// </summary>
        public HttpWebRequest Request
        {
            get { return request; }
            set { request = value; }
        }

        private HttpWebResponse response = null;
        /// <summary>
        /// 获取影响流的数据对象
        /// </summary>
        public HttpWebResponse Response
        {
            get { return response; }
            set { response = value; }
        }

        private Boolean isToLower = true;
        /// <summary>
        /// 是否设置为全文小写
        /// </summary>
        public Boolean IsToLower
        {
            get { return isToLower; }
            set { isToLower = value; }
        }

        private StreamReader reader = null;
        /// <summary>
        ///  读取流的对象
        /// </summary>
        public StreamReader Reader
        {
            get { return reader; }
            set { reader = value; }
        }

        private Boolean allowautoredirect = true;
        /// <summary>
        /// 支持跳转页面，查询结果将是跳转后的页面
        /// </summary>
        public Boolean Allowautoredirect
        {
            get { return allowautoredirect; }
            set { allowautoredirect = value; }
        }

        private int connectionlimit = 1024;
        /// <summary>
        /// 最大连接数
        /// </summary>
        public int Connectionlimit
        {
            get { return connectionlimit; }
            set { connectionlimit = value; }
        }

        private string proxyusername = string.Empty;
        /// <summary>
        /// 代理Proxy 服务器用户名
        /// </summary>
        public string ProxyUserName
        {
            get { return proxyusername; }
            set { proxyusername = value; }
        }

        private string proxypwd = string.Empty;
        /// <summary>
        /// 代理 服务器密码
        /// </summary>
        public string ProxyPwd
        {
            get { return proxypwd; }
            set { proxypwd = value; }
        }

        private string proxyip = string.Empty;
        /// <summary>
        /// 代理 服务IP
        /// </summary>
        public string ProxyIp
        {
            get { return proxyip; }
            set { proxyip = value; }
        }
    }
}