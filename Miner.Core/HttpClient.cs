using System;
using System.Text;
using System.Net;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.GZip;
using System.Collections;

namespace Miner
{
    public class HttpClient
    {
        public static CookieCollection ToCookies(string CookiesString)
        {
            CookieCollection cookies = new CookieCollection();
            foreach (string str in CookiesString.Split(';'))
            {
                string s = str.Trim();
                int i = s.IndexOf('=');
                cookies.Add(new Cookie(s.Substring(0, i), s.Substring(i + 1, s.Length - i - 1)));
            }
            return cookies;
        }

        //public static CookieCollection ToCookies(string CookiesString)
        //{
        //    CookieCollection cookies = new CookieCollection();
        //    foreach (string str in CookiesString.Split(';'))
        //    {
        //        string s = str.Trim();
        //        int i = s.IndexOf('=');
        //        cookies.Add(new Cookie(s.Substring(0, i), s.Substring(i + 1, s.Length - i - 1)));
        //    }
        //    return cookies;
        //}

        public static string ToString(CookieCollection cookies)
        {
            string[] cookiestring = new string[cookies.Count];
            for (int i = 0; i < cookies.Count; i++)
            {
                cookiestring[i] = cookies[i].Name + "=" + cookies[i].Value;
            }
            string CookiesString = string.Join(";", cookiestring);
            return CookiesString;
        }

        private HttpWebRequest CreateRequest()
        {
            HttpWebRequest request = null;
            request = (HttpWebRequest)HttpWebRequest.Create(this._UriString);
            request.Accept = "*/*";
            request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.0; zh-CN; rv:1.9.0.3) Gecko/2008092417 Firefox/3.0.3";            
            request.AllowAutoRedirect = this._AllowAutoRedirect;
            request.CookieContainer = new CookieContainer();
            request.KeepAlive = true;
            request.Referer = this._Referer;

            if (this._IsProxy)
            {
                string _ProxyHost = this._ProxyHost;
                int _ProxyPort = this._ProxyPort;
                string ProxyUser = this._ProxyUser;
                string ProxyPassword = this._ProxyPassword;
                //string ProxyDomain = "";
                System.Net.WebProxy oWebProxy = new System.Net.WebProxy(_ProxyHost, _ProxyPort);
                oWebProxy.Credentials = new NetworkCredential(ProxyUser, ProxyPassword);
                request.Proxy = oWebProxy;
            }

            if (this._CookiePost != null)
            {
                System.Uri u = new Uri(_UriString);

                foreach (System.Net.Cookie c in _CookiePost)
                {
                    c.Domain = u.Host;
                }
                request.CookieContainer.PerDomainCapacity = 50;
                request.CookieContainer.Add(_CookiePost);
            }

            if (_PostData != null && _PostData.Length > 0)
            {
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";

                byte[] buffer = this._Encoding.GetBytes(this._PostData);
                request.ContentLength = buffer.Length;
                System.IO.Stream stream = null;
                try
                {
                    stream = request.GetRequestStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    this._Err = ex.Message;
                }
                finally
                {
                    if (stream != null) { stream.Close(); }
                }
            }

            return request; 
        }

        public System.Drawing.Image GetImage()
        {
            System.Drawing.Image image = null;
            HttpWebRequest request = CreateRequest();
            //request.ContentType = "image/png";
            request.Headers.Add("Accept-Language", "zh-cn");
            request.Headers.Add("Accept-Encoding: gzip, deflate");
            request.Method = "GET"; 
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                image = System.Drawing.Image.FromStream(response.GetResponseStream());//ֱ����Ϊstream����ͼ��
                if (response.Cookies.Count > 0)
                {
                    this._CookieGet = response.Cookies;
                }
            }
            catch (System.Exception ex)
            {
                this._Err = ex.Message;
                Log.LogError(ex,"");
            }
            return image;
        }

        public string GetString()
        {

            HttpWebRequest request = CreateRequest();
            HttpWebResponse response = null; ;
            System.IO.StreamReader reader = null;

            try
            {
                response = (HttpWebResponse)request.GetResponse();

                //Console.WriteLine(response.ContentType);
                //Console.WriteLine(response.CharacterSet);

                if (response.ContentEncoding == "gzip")
                {
                    reader = new System.IO.StreamReader(new GZipInputStream(response.GetResponseStream()), this._Encoding);
                }
                else
                {
                    if (response.ContentEncoding == "deflate")
                    {
                        reader = new System.IO.StreamReader(new InflaterInputStream(response.GetResponseStream()), this._Encoding);
                    }
                }

                if (reader == null)
                {
                    reader = new System.IO.StreamReader(response.GetResponseStream(), this._Encoding);
                }
                
                this._ResHtml = reader.ReadToEnd();
            }
            catch (System.Exception ex)
            {
                this._Err = ex.Message;
                return "";
            }
            finally
            {
                if (reader != null) { reader.Close(); }
            }

            this._StatusCode = response.StatusCode.ToString();

            if (this._StatusCode == "302")
            {
                this._ResHtml = response.Headers["location"];
            }

            if (response.Cookies.Count > 0)
            {
                this._CookieGet = response.Cookies;
                //string strCookie = response.Headers["Set-Cookie"];
                //this._CookieGet = new CookieCollection();
                //AddCookieWithCookieHead(this._CookieGet, strCookie, new Uri(this._UriString).Host);

            }
            return this.ResHtml;
        }

        #region �Ӱ������ Cookie ���ַ�����ȡ�� CookieCollection ������
        private void AddCookieWithCookieHead(CookieCollection cookieCol, string cookieHead, string defaultDomain)
        {
             if(cookieHead == null)
                 return;
             string[] ary=cookieHead.Split(';');
             for(int i = 0;i < ary.Length ;i++)
             {
                Cookie ck = GetCookieFromString(ary[i].Trim(),defaultDomain);
                if(ck != null)
                {
                    cookieCol.Add(ck);
                }
           }
        }
        #endregion

        #region ��ȡĳһ�� Cookie �ַ����� Cookie ������
        private Cookie GetCookieFromString(string cookieString, string defaultDomain)
        {
           string[] ary=cookieString.Split(',');
           Hashtable hs=new Hashtable();
           for(int i=0;i < ary.Length;i++)
           {
                string s=ary[i].Trim();
                int index = s.IndexOf("=");
                if(index > 0 && index < s.Length)    
                {
                     hs.Add(s.Substring(0,index),s.Substring(index + 1));
                }
           }
           Cookie ck=new Cookie();
           foreach(object Key in hs.Keys)
           {
            if(Key.ToString()=="path")
                ck.Path=hs[Key].ToString();

            else if(Key.ToString()=="expires")
            {
                ck.Expires=DateTime.Parse(hs[Key].ToString());
            } 
            else if(Key.ToString()=="domain")
                ck.Domain=hs[Key].ToString();
            else
            {
                 ck.Name=Key.ToString();
                 ck.Value=hs[Key].ToString();
            }
           }
           if(ck.Name == "")
               return null;
           if(ck.Domain == "")
               ck.Domain = defaultDomain;
           return ck;
        }
        #endregion








        #region Ŀ���ַ
        private string _UriString;
        /// <summary>
        /// ����http�ĵ�ַ
        /// </summary>
        public string UriString
        {
            get
            {
                return _UriString;
            }
            set
            {
                _UriString = value;
            }
        }
        #endregion

        #region ��Դ��ַ
        private string _Referer;
        /// <summary>
        /// ��ǰҳ������õ�ַ
        /// </summary>
        public string Referer
        {
            get
            {
                return _Referer;
            }
            set
            {
                _Referer = value;
            }
        }
        #endregion

        #region ��������
        private string _PostData;
        /// <summary>
        /// ���ͳ�ȥ������
        /// </summary>
        public string PostData
        {
            get { return this._PostData; }
            set { this._PostData = value; }
        }
        #endregion

        #region �Ƿ���ת
        private bool _AllowAutoRedirect;
        /// <summary>
        /// ���ͳ�ȥ������
        /// </summary>
        public bool AllowAutoRedirect
        {
            get { return this._AllowAutoRedirect; }
            set { this._AllowAutoRedirect = value; }
        }
        #endregion        

        #region Ҫ���͵�cookie����
        private System.Net.CookieCollection _CookiePost;
        /// <summary>
        /// ���͵�cookie����
        /// </summary>
        public System.Net.CookieCollection CookiePost
        {
            get
            {
                return _CookiePost;
            }
            set { _CookiePost = value; }
        }
        #endregion

        #region ��ȡ��cookie����
        private System.Net.CookieCollection _CookieGet;
        /// <summary>
        /// ���͵�cookie����
        /// </summary>
        public System.Net.CookieCollection CookieGet
        {
            get
            {
                return _CookieGet;
            }
        }
        #endregion

        #region �Ƿ��ͳɹ�
        private bool _Succeed;
        /// <summary>
        /// �Ƿ�ִ�гɹ�
        /// </summary>
        public bool Succeed
        {
            get { return _Succeed; }
            set { _Succeed = value; }
        }
        #endregion

        #region ��Ӧ��html���
        private string _ResHtml;
        /// <summary>
        /// ���ص�html��������ı���ʽ
        /// </summary>
        public string ResHtml
        {
            get
            {

                return _ResHtml;
            }
        }
        #endregion

        #region ��Ӧ��image���
        private System.Drawing.Image _ResImage;
        /// <summary>
        /// ���ص�html��������ı���ʽ
        /// </summary>
        public System.Drawing.Image ResImage
        {
            get
            {

                return _ResImage;
            }
        }
        #endregion

        #region ��Ӧ��
        private string _StatusCode;
        /// <summary>
        /// ��Ӧ����
        /// </summary>
        public string StatusCode
        {
            get { return _StatusCode; }
            set { _StatusCode = value; }
        }
        #endregion

        #region �����ı�
        private string _Err;
        /// <summary>
        /// �����ı�
        /// </summary>
        public string Err
        {
            get { return _Err; }
            set { _Err = value; }
        }
        #endregion

        #region ����
        private System.Text.Encoding _Encoding = System.Text.Encoding.Default;
        public System.Text.Encoding Encoding
        {
            get { return _Encoding; }
            set { _Encoding = value; }
        }
        #endregion

        #region �Ƿ�ʹ�ô���
        private bool _IsProxy = false;
        /// <summary>
        /// �Ƿ�ʹ�ô���
        /// </summary>
        public bool IsProxy
        {
            get { return this._IsProxy; }
            set { this._IsProxy = value; }
        }

        private string _ProxyHost;
        /// <summary>
        /// ����IP
        /// </summary>
        public string ProxyHost
        {
            get { return this._ProxyHost; }
            set { this._ProxyHost = value; }
        }

        private int _ProxyPort;
        /// <summary>
        /// ����˿�
        /// </summary>
        public int ProxyPort
        {
            get { return this._ProxyPort; }
            set { this._ProxyPort = value; }
        }


        private string _ProxyUser;

        public string ProxyUser
        {
            get { return _ProxyUser; }
            set { _ProxyUser = value; }
        }
        private string _ProxyPassword;

        public string ProxyPassword
        {
            get { return _ProxyPassword; }
            set { _ProxyPassword = value; }
        } 
        #endregion
    }
}