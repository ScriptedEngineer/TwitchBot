using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TwitchBot
{
    public class MySave
    {
        public static MySave Current = new MySave();
        public SerializableDictionary<string, string> Names { get; set; }
        public string Streamer { get; set; }
        private static Random Rand = new Random();
        public bool[] Bools { get; set; }
        public int[] Nums { get; set; }
        private MySave()
        {
            Names = new SerializableDictionary<string, string>();
            Bools = new bool[3];
            Nums = new int[1];
            Nums[0] = 0;
        }
        public static void Load()
        {
            if (File.Exists("save.xml"))
            {
                try
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(MySave));
                    using (FileStream fs = new FileStream("save.xml", FileMode.OpenOrCreate))
                    {
                        Current = (MySave)formatter.Deserialize(fs);
                    }
                }
                catch
                {
                    File.Copy("save.xml", "save_errored"+ Rand.Next()+ ".xml");
                    Current = new MySave();
                    Save();
                }
            }
        }
        public static void Save()
        {
            MySave X = Current;
            XmlSerializer formatter = new XmlSerializer(typeof(MySave));
            if(File.Exists("save.xml"))File.Delete("save.xml");
            using (FileStream fs = new FileStream("save.xml", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, X);
            }
        }
    }
    [XmlRoot("dictionary")]
    public class SerializableDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public SerializableDictionary() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public SerializableDictionary(int capacity) : base(capacity) { }
        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");

                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("item");

                writer.WriteStartElement("key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }
        #endregion
    }
}
