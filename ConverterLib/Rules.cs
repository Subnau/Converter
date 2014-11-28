using System.Collections.Generic;
using System.Xml.Serialization;

namespace ConverterLib
{

    [XmlInclude(typeof(SimpleBinding))]
    [XmlInclude(typeof(DicBinding))]
    [XmlInclude(typeof(ListBinding))]
    public class Rule
    {
        [XmlAttribute]
        public string Description { get; set; }

        public PropBinding Source;

        public PropBinding Dest;
    }

    public abstract class PropBinding
    {

    }

    public class SimpleBinding : PropBinding
    {
        [XmlAttribute]
        public string PropName { get; set; }
    }

    public class DicBinding : PropBinding
    {
        [XmlAttribute]
        public string PropName { get; set; }

        [XmlAttribute]
        public string Key { get; set; }
    }

    public class ListBinding : PropBinding
    {
        [XmlAttribute]
        public string PropName { get; set; }

        [XmlAttribute]
        public string ListName { get; set; }

        [XmlAttribute]
        public int ItemIndex { get; set; }

        public List<ListFilterOrInit> ListFilterOrInits { get; set; }
    }

    public class ListFilterOrInit
    {
        [XmlAttribute]
        public string PropName { get; set; }

        [XmlAttribute]
        public string Condition { get; set; }

        [XmlAttribute]
        public string PropValue { get; set; }
    }


}
