using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Miner
{
    public class Proxy51:Proxy
    {
        public override List<ProxyData> GetProxy()
        {
            List<ProxyData> list = new List<ProxyData>();

            HttpClient client = new HttpClient();
            client.UriString = "http://51dai.li/http_anonymous.html";
            client.Referer = "http://51dai.li";
            client.AllowAutoRedirect = true;
            client.Encoding = Encoding.UTF8;

            string html = client.GetString();

            if (string.IsNullOrEmpty(html))
                return null;

            Regex r = new Regex(@"<td>(.+?)</td>[\s\S]*?<td width=""60"">(.+?)</td>[\s\S]*?<td width=""60"">(.+?)</td>", RegexOptions.IgnoreCase | RegexOptions.Multiline);


            MatchCollection mc = r.Matches(html);

            
            
            foreach (Match m in mc)
            {
                if (m.Groups[3].Value.ToUpper().Equals("CN"))
                {
                    ProxyData pd = new ProxyData();
                    pd.ProxyHost = m.Groups[1].Value;
                    pd.ProxyPort = Convert.ToInt32(m.Groups[2].Value);
                    if (!list.Contains(pd))
                        list.Add(pd);
                }
            }

            
            

            return list;
        }
    }
}
