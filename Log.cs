using System;

namespace ImapTray
{
    static class Log
    {
        public enum Severity
        {
            Debug,
            Info,
            Warn,
            Erorr
        }

        public class Message
        {
            public readonly DateTime when;
            public readonly Severity severity;
            public readonly string msg;

            public Message(DateTime when, Severity severity, string msg)
            {
                this.when = when;
                this.severity = severity;
                this.msg = msg;
            }
        }

        private static readonly object Lock = new object();
        private static Message[] _messages = { };

        public static event OnLogChanged onLogChanged;

        public delegate void OnLogChanged(Message[] messages);

        public static void Add(Severity level, string format, params object[] args)
        {
            string message = String.Format(format, args);
            lock (Lock)
            {
                Array.Resize(ref _messages, _messages.Length + 1);
                _messages[_messages.Length - 1] = new Message(DateTime.Now, level, message);
            }
            if (onLogChanged != null)
            {
                onLogChanged(_messages);
            }
        }

        public static void Debug(string message, params object[] args)
        {
            Add(Severity.Debug, message, args);
        }

        public static void Info(string message, params object[] args)
        {
            Add(Severity.Info, message, args);
        }

        public static void Warn(string message, params object[] args)
        {
            Add(Severity.Warn, message, args);
        }

        public static void Error(string message, params object[] args)
        {
            Add(Severity.Warn, message, args);
        }

        public static Message[] ListAll()
        {
            return _messages;
        }

        public static void Clear()
        {
            _messages = new Message[] { };
            if (onLogChanged != null)
            {
                onLogChanged(_messages);
            }
        }
    }
}
