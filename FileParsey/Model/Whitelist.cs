using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FileParsey.Model
{
    [XmlRoot("whitelist")]
    public class Whitelist
    {
        public Whitelist()
        {
            Items = new List<string>();
        }

        [XmlElement("item")]
        public List<string> Items { get; set; }
    }
}