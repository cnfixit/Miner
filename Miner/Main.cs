using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Reflection;
using System.Threading;

namespace Miner
{
    public partial class Main : Form
    {
        private bool flag = false;
        ManualResetEvent stop = new ManualResetEvent(false);
        string path = Application.StartupPath + @"\res\";
        


        public Main()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!flag)
            {
                //start
                stop.Reset();
                this.start();
                this.btnStart.Text = "停止";
            }
            else
            {
                //stop
                stop.Set();
                this.btnStart.Text = "开始";
            }
            flag = !flag;


            //HttpClient client = new HttpClient();
            //client.IsProxy = true;
            //client.ProxyHost = "116.228.235.5";
            //client.ProxyPort = 80;

            //client.AllowAutoRedirect = true;

            //client.UriString = "http://www.nalati.com/tools/getcity.php";

            //string res = client.GetString();

            //this.lbRes.Items.Add(client.StatusCode + res);

            // string path = Application.StartupPath + @"\res\";
            // string[] files = Directory.GetFiles(path, "*.dll");
            // IProxy pro = null;
            // foreach (string s in files)
            // {
            //     pro = GetPluginInterface(s);
            //     if(pro != null)
            //        this.lbRes.Items.Add("Load item:" + pro.GetProxy());
            // }

            // List<ProxyData> list = pro.GetProxy();

            //if(list != null)
            //{
            //    foreach (ProxyData pd in list)
            //    {

            //    }
            //}
            
        }

        private void start()
        {
            Thread t = new Thread(new ThreadStart(getproxylist));
            t.Start();
        }




        private void getproxylist()
        {
            string[] files = Directory.GetFiles(path, "*.dll");
            IProxy pro = null;
            foreach (string s in files)
            {
                pro = GetPluginInterface(s);
                if (pro != null)
                {
                    //采集代理地址并储存
                    pro.ProxyListHasGot += new ProxyListHasGotEventHandler(pro_ProxyListHasGot);
                    pro.GetProxyList();
                    

                }
            }
        }

        void pro_ProxyListHasGot(object sender, ProxyListEventArgs e)
        {
            this.lbRes.Items.Add("Load item:" + e.ProxyList.Count.ToString());
            //throw new NotImplementedException();
        }

      



        /// <summary>
        /// 获取指定动态库IProcedure实例
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        IProxy GetPluginInterface(string filepath)
        {
            if (!File.Exists(filepath))
                return null;

            Type type = null;
            IProxy ip = null;
            string fullname = string.Empty;

            try
            {
                Assembly ass = Assembly.LoadFile(filepath);
                Type[] tps = ass.GetTypes();


                foreach (Type t in tps)
                {
                    if (t.IsSubclassOf(typeof(Proxy)))
                    {
                        fullname = t.FullName;
                        type = t;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(fullname))
                {
                    ip = ass.CreateInstance(fullname) as IProxy;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ip;
        }
    }
}
