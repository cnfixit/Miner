using System;
using System.Collections.Generic;
using System.Text;

namespace Miner
{
        

    public abstract class Proxy : IProxy
    {
        public abstract List<ProxyData> GetProxy();
        public event ProxyListHasGotEventHandler ProxyListHasGot;

        public virtual void GetProxyList()
        {
            List<ProxyData> list = GetProxy();
            if (ProxyListHasGot != null)
                ProxyListHasGot(this, new ProxyListEventArgs(list));
        }

    }

    public delegate void ProxyListHasGotEventHandler(object sender, ProxyListEventArgs e);

    public interface IProxy
    {
        List<ProxyData> GetProxy();
        void GetProxyList();
        event ProxyListHasGotEventHandler ProxyListHasGot;
    }


    public class ProxyListEventArgs:EventArgs
    {

        public ProxyListEventArgs(List<ProxyData> list)
        {
            this._ProxyList = list;
        }
        List<ProxyData> _ProxyList = new List<ProxyData>();

        public List<ProxyData> ProxyList
        {
            get { return _ProxyList; }
            set { _ProxyList = value; }
        }
    }
}
