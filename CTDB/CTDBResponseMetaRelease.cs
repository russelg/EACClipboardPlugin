using System;
using System.Xml.Serialization;

namespace Edamame.ClipboardPlugin
{
    [Serializable]
    public class CTDBResponseMetaRelease
    {
        public CTDBResponseMetaRelease()
        {
        }

        public CTDBResponseMetaRelease(CTDBResponseMetaRelease src)
        {
            this.date = src.date;
            this.country = src.country;
        }

        [XmlAttribute]
        public string date { get; set; }
        [XmlAttribute]
        public string country { get; set; }
    }
}
