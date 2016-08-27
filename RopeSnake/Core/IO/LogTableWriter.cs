using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    internal sealed class LogTableWriter
    {
        public sealed class ColumnHeader
        {
            public string Name { get; private set; }
            public int Width { get; private set; }

            public ColumnHeader(string name, int width)
            {
                Name = name;
                Width = width;
            }
        }

        private StreamWriter _writer;
        private List<ColumnHeader> _headers = new List<ColumnHeader>();

        public LogTableWriter(StreamWriter writer)
        {
            _writer = writer;
        }

        public void AddHeader(string name, int width)
        {
            _headers.Add(new ColumnHeader(name, width));
        }

        private void WriteIndent(int indent)
        {
            _writer.Write(new string(' ', indent));
        }

        public void WriteHeader(int indent)
        {
            var builder = new StringBuilder();

            WriteIndent(indent);
            int lineWidth = 0;
            foreach (var header in _headers)
            {
                builder.Append(header.Name.PadRight(header.Width, ' ').Substring(0, header.Width));
                lineWidth += header.Width;
            }
            _writer.WriteLine(builder.ToString().TrimEnd());

            WriteIndent(indent);
            _writer.WriteLine(new string('-', lineWidth));
        }

        public void WriteLine(int indent, params object[] fields)
        {
            if (fields.Length != _headers.Count)
                throw new ArgumentException(nameof(fields));

            var builder = new StringBuilder();

            WriteIndent(indent);
            for (int i = 0; i < fields.Length; i++)
            {
                var header = _headers[i];
                builder.Append(fields[i].ToString().PadRight(header.Width, ' ').Substring(0, header.Width));
            }
            _writer.WriteLine(builder.ToString().TrimEnd());
        }
    }
}
