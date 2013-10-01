using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SqlServer_Files
{
    public abstract class TelemetryDataFile
    {

        #region Private Fields

        private List<string> _lines;
        //private List<LocationEvents> _events;

        #endregion

        #region Public API

        #region Constructors

        protected TelemetryDataFile(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException("path", "path must not be null or empty");
            ReadLines(path);
            if (_lines.Count == 0)
                throw new InvalidDataException("File at path has no lines");
        }

        protected TelemetryDataFile(Byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentNullException("bytes", "byte array must not be null or empty");
            ReadLines(bytes);
            if (_lines.Count == 0)
                throw new InvalidDataException("Byte array has no lines");
        }

        protected TelemetryDataFile(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "stream must not be null");
            ReadLines(stream);
            if (_lines.Count == 0)
                throw new InvalidDataException("stream has no lines");
        }

        #endregion

        #region Public Methods

        public String[] Lines
        {
            get
            {
                return _lines.ToArray();
            }
        }


        public abstract bool IsValidData
        {
            get;
        }

        /*
        public IEnumerable<string> GetAntennas()
        {
            if (_events == null)
                _events = GetLocationEvents(_lines).ToList();
            return _events.Select(t => t.PlatformId).Distinct();
        }

        public IEnumerable<string> GetTags(string program)
        {
            if (_events == null)
                _events = GetLocationEvents(_lines).ToList();
            return _events.Where(t => t.ProgramId == program).Select(t => t.PlatformId).Distinct();
        }

        public IEnumerable<LocationEvents> GetLocationEvents()
        {
            if (_events == null)
                _events = GetLocationEvents(_lines).ToList();
            return _events.ToArray();
        }

        public DateTime FirstLocationEvents(string tag)
        {
            if (_events == null)
                _events = GetLocationEvents(_lines).ToList();
            return (from t in _events
                    where t.PlatformId == platform
                    orderby t.DateTime
                    select t.DateTime).First();
        }

        public DateTime LastLocationEvents(string tag)
        {
            if (_events == null)
                _events = GetLocationEvents(_lines).ToList();
            return (from t in _events
                    where t.PlatformId == platform
                    orderby t.DateTime descending
                    select t.DateTime).First();
        }
        */
        #endregion

        #endregion

        #region Private Methods

//        abstract protected IEnumerable<LocationEvents> GetLocationEvents(IEnumerable<string> lines);

        #region Line Readers

        private void ReadLines(string path)
        {
            _lines = File.ReadAllLines(path).ToList();
        }

        private void ReadLines(Byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, 0, bytes.Length))
                _lines = ReadLines(stream, Encoding.UTF8).ToList();
        }

        private void ReadLines(Stream stream)
        {
            _lines = ReadLines(stream, Encoding.UTF8).ToList();
        }

        private static IEnumerable<string> ReadLines(Stream stream, Encoding enc)
        {
            using (var reader = new StreamReader(stream, enc))
                while (!reader.EndOfStream)
                    yield return reader.ReadLine();
        }

        #endregion

        #endregion

    }
}
