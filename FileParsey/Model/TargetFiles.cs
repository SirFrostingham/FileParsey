using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FileParsey.Model
{
    [XmlRoot("files")]
    public class TargetFiles
    {
        public TargetFiles()
        {
            Files = new List<string>();
        }

        [XmlElement("file")]
        public List<string> Files { get; set; }
    }
}