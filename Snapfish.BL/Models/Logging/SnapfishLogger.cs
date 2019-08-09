using System;
using System.Collections.Generic;
using System.IO;

namespace Snapfish.BL.Models.Logging
{
    //IF U WANT TO LOG TO FOR INSTANCE STDOUT: _logger.AddWriter(new StreamWriter(Console.OpenStandardOutput()), LoggingFlags.All);
    //STDERR: readonly SnapfishLogger _logger = new SnapfishLogger(new StreamWriter(Console.OpenStandardError()));
    public class SnapfishLogger
    {
        private const string DateTimeFormat = "%yyyy-%MM-%dd %HH:%mm:%ss %zz";
        private readonly Dictionary<StreamWriter, LoggingFlags> _writers;
        private string _identifier;

        public SnapfishLogger(StreamWriter writer = null, LoggingFlags mask = LoggingFlags.Default,
            string identifier = "")
        {
            _writers = new Dictionary<StreamWriter, LoggingFlags>();
            AddWriter(writer, mask);
            _identifier = identifier;
        }

        public void AddWriter(StreamWriter writer, LoggingFlags mask = LoggingFlags.Default)
        {
            if (writer != null)
            {
                SetMask(writer, mask);
            }
            else
            {
                Error("Tried to add already added writer: " + (StreamWriter) null, "LOGGER");
            }
        }

        public void Close()
        {
            foreach (var writer in _writers.Keys)
            {
                writer.Close();
            }
        }

        private void SetMask(StreamWriter writer, LoggingFlags mask)
        {
            _writers[writer] = mask;
        }

        public LoggingFlags GetMask(StreamWriter writer)
        {
            return _writers[writer];
        }

        public string GetIdentifier()
        {
            return _identifier;
        }

        public void SetIdentifier(string identifier)
        {
            _identifier = identifier;
        }

        private void Log(LoggingFlags tag, string message, string prefix = "")
        {
            foreach (var writer in _writers.Keys)
            {
                if ((_writers[writer] & tag) == 0)
                {
                    continue;
                }

                var timestamp = DateTime.Now.ToString(DateTimeFormat);
                //writer.Write(String.Join("\t", timestamp, _identifier, tag.ToString("X"), prefix, message.Trim(), Environment.NewLine)); BETTER INTEROPERABILITY BY WRITTING MESSAGE AS HEX FOR PASING
                writer.Write(String.Join("\t", timestamp, _identifier, tag.ToString(), prefix, message.Trim(),
                    Environment.NewLine));
                writer.Flush();
            }
        }

        public void Panic(string message, string prefix = "")
        {
            Emergency(message, prefix);
            Environment.Exit(1);
        }

        public void Throw(Exception exception, string message, string prefix = "")
        {
            Error(message, prefix);
            throw exception;
        }

        public void Emergency(string message, string prefix = "")
        {
            Log(LoggingFlags.Emergency, message, prefix);
        }

        public void Alert(string message, string prefix = "")
        {
            Log(LoggingFlags.Alert, message, prefix);
        }

        public void Critical(string message, string prefix = "")
        {
            Log(LoggingFlags.Critical, message, prefix);
        }

        public void Error(string message, string prefix = "")
        {
            Log(LoggingFlags.Error, message, prefix);
        }

        public void Warn(string message, string prefix = "")
        {
            Log(LoggingFlags.Warn, message, prefix);
        }

        public void Notice(string message, string prefix = "")
        {
            Log(LoggingFlags.Notice, message, prefix);
        }

        public void Info(string message, string prefix = "")
        {
            Log(LoggingFlags.Info, message, prefix);
        }

        public void Debug(string message, string prefix = "")
        {
            Log(LoggingFlags.Debug, message, prefix);
        }
    }
}