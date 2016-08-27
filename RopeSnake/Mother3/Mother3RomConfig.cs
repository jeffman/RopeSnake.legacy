using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using RopeSnake.Gba;
using RopeSnake.Mother3.Text;
using Newtonsoft.Json;

namespace RopeSnake.Mother3
{
    public sealed class Mother3RomConfig
    {
        [JsonProperty]
        public string Version { get; private set; }

        [JsonProperty]
        public Dictionary<string, HashSet<int>> References { get; private set; }

        [JsonProperty]
        public Dictionary<string, List<Range>> FreeRanges { get; private set; }

        [JsonProperty]
        public ScriptEncodingParameters ScriptEncodingParameters { get; private set; }

        [JsonProperty]
        public List<ControlCode> ControlCodes { get; private set; }

        [JsonProperty]
        public Dictionary<short, ContextString> CharLookup { get; private set; }

        [JsonProperty]
        public Dictionary<short, ContextString> SaturnLookup { get; private set; }

        [JsonProperty]
        public Dictionary<string, object> Parameters { get; private set; }

        [JsonIgnore]
        public ReverseLookup ReverseCharLookup { get; private set; }

        [JsonIgnore]
        public ReverseLookup ReverseSaturnLookup { get; private set; }

        [JsonIgnore]
        private static Encoding sjisEncoding = Encoding.GetEncoding(932);

        [JsonIgnore]
        public bool IsJapanese => (Version == "jp");

        public IEnumerable<int> GetReferences(string key)
        {
            return References[key];
        }

        public int GetOffset(string key, Block romData)
        {
            var references = GetReferences(key);
            var offsets = new HashSet<int>();
            var stream = romData.ToBinaryStream();

            foreach (int reference in references)
            {
                stream.Position = reference;
                int offset = stream.ReadGbaPointer();
                offsets.Add(offset);
            }

            if (offsets.Count != 1)
                throw new Exception("Inconsistent offsets");

            return offsets.First();
        }

        public T GetParameter<T>(string key)
        {
            return (T)Convert.ChangeType(Parameters[key], typeof(T));
        }

        public void SetParameter(string key, object value)
        {
            Parameters[key] = value;
        }

        public void UpdateLookups()
        {
            if (CharLookup != null)
            {
                ReverseCharLookup = new ReverseLookup(CharLookup);
            }

            if (SaturnLookup != null)
            {
                ReverseSaturnLookup = new ReverseLookup(SaturnLookup);
            }
        }

        public void AddJapaneseCharsToLookup(Block romData)
        {
            if (CharLookup == null)
                throw new InvalidOperationException(nameof(CharLookup));

            int offset = GetOffset("Text.MainFont", romData);
            var stream = romData.ToBinaryStream(offset);

            for (short i = 0; i < 7332; i++)
            {
                if (CharLookup.ContainsKey(i))
                {
                    stream.Position += 22;
                    continue;
                }

                byte[] sjis = { stream.ReadByte(), stream.ReadByte() };
                stream.Position += 20;
                string value = sjisEncoding.GetString(sjis);
                CharLookup.Add(i, new ContextString(value));
            }
        }

        public void ReadEncodingPadData(Block romData)
        {
            var stream = romData.ToBinaryStream(ScriptEncodingParameters.EvenPadAddress);
            ScriptEncodingParameters.EvenPad = new byte[ScriptEncodingParameters.EvenPadModulus];
            stream.ReadBytes(ScriptEncodingParameters.EvenPad, 0, ScriptEncodingParameters.EvenPadModulus);

            stream.Position = ScriptEncodingParameters.OddPadAddress;
            ScriptEncodingParameters.OddPad = new byte[ScriptEncodingParameters.OddPadModulus];
            stream.ReadBytes(ScriptEncodingParameters.OddPad, 0, ScriptEncodingParameters.OddPadModulus);
        }
    }
}
