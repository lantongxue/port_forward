using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;

namespace PortForward
{
    public class ForwardItemManage
    {
        XmlDocument doc = new XmlDocument();
        XmlElement root = null;
        string _xml = Path.Combine(Environment.CurrentDirectory, "config.xml");

        public ForwardItemManage()
        {
            if(!File.Exists(_xml))
            {
                _InitXml();
            }
            try
            {
                doc.Load(_xml);
            }
            catch(XmlException)
            {
                _InitXml();
                doc.Load(_xml);
            }
            root = doc.DocumentElement;
        }

        private void _InitXml()
        {
            File.WriteAllText(_xml, "<?xml version=\"1.0\" encoding=\"utf-8\" ?><root></root>");
        }

        public void Add(ForwardItem forward)
        {
            XmlElement node = doc.CreateElement("forward");
            PropertyInfo[] properties = forward.GetType().GetProperties();
            foreach(PropertyInfo property in properties)
            {
                object[] attrs = property.GetCustomAttributes(typeof(SaveAttribute), false);
                if(attrs.Length == 0)
                {
                    continue;
                }
                XmlElement attr = doc.CreateElement(property.Name);
                attr.InnerText = property.GetValue(forward, null).ToString();
                node.AppendChild(attr);
            }
            root.AppendChild(node);
            doc.Save(_xml);
        }

        public void Remove(ForwardItem forward)
        {
            XmlNodeList list = root.SelectNodes("forward");
            foreach(XmlNode node in list)
            {
                XmlNode id = node.SelectSingleNode("Id");
                if(id == null)
                {
                    continue;
                }
                if(id.InnerText == forward.Id)
                {
                    root.RemoveChild(node);
                    break;
                }
            }
            doc.Save(_xml);
        }

        public void Update(ForwardItem forward)
        {
            XmlNodeList list = root.SelectNodes("forward");
            foreach (XmlNode node in list)
            {
                XmlNode id = node.SelectSingleNode("Id");
                if (id == null)
                {
                    continue;
                }
                if (id.InnerText == forward.Id)
                {
                    PropertyInfo[] properties = forward.GetType().GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        object[] attrs = property.GetCustomAttributes(typeof(SaveAttribute), false);
                        if (attrs.Length == 0)
                        {
                            continue;
                        }
                        XmlNode attr = node.SelectSingleNode(property.Name);
                        if(attr != null)
                        {
                            attr.InnerText = property.GetValue(forward, null).ToString();
                        }
                    }
                    break;
                }
            }
            doc.Save(_xml);
        }

        public List<ForwardItem> Select()
        {
            List<ForwardItem> forwardItems = new List<ForwardItem>();
            XmlNodeList list = root.SelectNodes("forward");
            foreach (XmlNode node in list)
            {
                ForwardItem forward = new ForwardItem();
                Type t = forward.GetType();
                foreach (XmlNode attr in node.ChildNodes)
                {
                    object value = attr.InnerText;
                    if(attr.Name == "LocalListenPort" || attr.Name == "RemotePort")
                    {
                        value = Convert.ToInt32(value);
                    }
                    if (attr.Name == "TotalUpload" || attr.Name == "TotalDownload")
                    {
                        value = long.Parse(value.ToString());
                    }
                    if (attr.Name == "Protocol")
                    {
                        switch(value.ToString())
                        {
                            case "Tcp":
                                value = ForwardProtocol.Tcp;
                                break;
                            case "Udp":
                                value = ForwardProtocol.Udp;
                                break;
                        }
                    }
                    if (attr.Name == "State")
                    {
                        switch (value.ToString())
                        {
                            case "Runing":
                                value = ForwardState.Runing;
                                break;
                            case "Stopped":
                                value = ForwardState.Stopped;
                                break;
                        }
                    }
                    t.GetProperty(attr.Name).SetValue(forward, value, null);
                }
                forwardItems.Add(forward);
            }
            return forwardItems;
        }
    }

    public class SaveAttribute : Attribute
    {

    }
}
