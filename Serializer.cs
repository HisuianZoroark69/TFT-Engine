using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TFT_Engine.Components;

namespace TFT_Engine
{
    public class Element<T>
    {
        [XmlAttribute] public T value { get; set; }
    }

    public class CharInfo
    {
        public string Name { get; set; }
        public string TeamID { get; set; }
        public string HashCode { get; set; }
    }

    public class CharInfoWrapper
    {
        [XmlArrayItem("Character")]
        [XmlArray("Characters")]
        public List<CharInfo> CharInfos { get; set; }
    }

    public class StatInfo
    {
        public Element<string> CharacterHashCode;
        public Statistics stats;
    }

    public class StatInfoWrapper
    {
        /*[XmlArrayItem("Stats")]
        [XmlArray("Stat")]*/
        [XmlElement] public List<StatInfo> StatInfos { get; set; }
    }

    public class Serializer
    {
        public static string CharacterInfos(CharList c)
        {
            List<CharInfo> CharInfoTemp = new();
            foreach (var character in c)
                CharInfoTemp.Add(new CharInfo
                {
                    Name = character.Name,
                    TeamID = character.teamID.ToString(),
                    HashCode = character.GetHashCode().ToString()
                });

            XmlSerializer xs = new(typeof(CharInfoWrapper));
            CharInfoWrapper wrapper = new() {CharInfos = CharInfoTemp};
            using var writer = new StringWriter();
            xs.Serialize(writer, wrapper);
            return writer.ToString();
        }

        public static string CharacterStats(CharList c)
        {
            List<StatInfo> StatInfoTemp = new();
            foreach (var character in c)
                StatInfoTemp.Add(new StatInfo
                {
                    CharacterHashCode = new Element<string> {value = character.GetHashCode().ToString()},
                    stats = character.currentStats
                });

            XmlSerializer xs = new(typeof(StatInfoWrapper));
            StatInfoWrapper wrapper = new() {StatInfos = StatInfoTemp};
            using var writer = new StringWriter();
            xs.Serialize(writer, wrapper);
            return writer.ToString();
        }
    }
}