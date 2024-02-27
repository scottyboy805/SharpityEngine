namespace SharpityEngine
{
    public enum LogFilter
    {
        None = 0,
        Graphics,
        Content,
        Input,
        Physics,
        Audio,
        Network,
        Game,
        Script,

        Editor = 31,
    }

    public static class Debug
    {
        // Type
        public enum LogType
        {
            Info = 1,
            Warning,
            Error,
            Exception,
        }

        public interface ILogger : IDisposable
        {
            // Methods
            void OnMessage(in LogMessage message);
        }

        public sealed class ConsoleLogger : ILogger
        {
            // Private
#if !SIMPLE2D_WEB
            private Dictionary<LogFilter, ConsoleColor> filterColors = new Dictionary<LogFilter, ConsoleColor>
            {
                { LogFilter.Graphics, ConsoleColor.Green },
                { LogFilter.Content, ConsoleColor.Cyan },
                { LogFilter.Input, ConsoleColor.Magenta },
                { LogFilter.Physics, ConsoleColor.DarkCyan },
                { LogFilter.Audio, ConsoleColor.DarkGreen },
                { LogFilter.Network, ConsoleColor.Blue },
                { LogFilter.Game, ConsoleColor.DarkBlue },
                { LogFilter.Script, ConsoleColor.DarkMagenta },
                { LogFilter.Editor, ConsoleColor.DarkYellow },
            };
#endif

            // Methods
            public void OnMessage(in LogMessage message)
            {
#if !SIMPLE2D_WEB
                ConsoleColor color = ConsoleColor.White;
#endif
                string prefix = "INFO: ";

                switch (message.type)
                {
                    case LogType.Warning:
                        {
#if !SIMPLE2D_WEB
                            color = ConsoleColor.DarkYellow;
#endif
                            prefix = "WARNING: ";
                            break;
                        }

                    case LogType.Error:
                    case LogType.Exception:
                        {
#if !SIMPLE2D_WEB
                            color = ConsoleColor.Red;
#endif
                            prefix = "ERROR: ";
                            break;
                        }
                }

#if !SIMPLE2D_WEB
                // Change color
                Console.ForegroundColor = color;
#endif

                // Write message
                Console.Write(prefix);

                if (message.filter != LogFilter.None)
                {
                    // Get the filter color
#if !SIMPLE2D_WEB
                    Console.ForegroundColor = filterColors[message.filter];
#endif
                    Console.Write("[");
                    Console.Write(message.filter);
                    Console.Write("]: ");

#if !SIMPLE2D_WEB
                    // Revert back to original message color
                    Console.ForegroundColor = color;
#endif
                }

                if (message.sender != null)
                {
                    Console.Write("(");
                    Console.Write(message.sender);
                    Console.Write("): ");
                }

                Console.WriteLine(message.msg);

#if !SIMPLE2D_WEB
                // Return to white color
                Console.ForegroundColor = ConsoleColor.White;
#endif
            }

            public void Dispose() { }
        }

        public sealed class FileLogger : ILogger
        {
            // Private
            private TextWriter writer = null;
            private bool includeDateTime = false;

            // Constructor
            public FileLogger(string filePath, bool includeDateTime)
            {
                this.includeDateTime = includeDateTime;
                try
                {
                    writer = File.CreateText(filePath);
                    writer = TextWriter.Synchronized(writer);
                }
                catch
                {
                    writer = File.CreateText(Path.GetTempFileName());
                    writer = TextWriter.Synchronized(writer);
                }
            }

            // Methods
            public void OnMessage(in LogMessage message)
            {
                if (includeDateTime == true)
                {
                    writer.Write(DateTime.Now.ToString());
                    writer.Write(" ");
                }

                switch (message.type)
                {
                    case LogType.Info:
                        {
                            // Write severity
                            writer.Write("[INFO]: ");

                            // Write filter
                            if (message.filter != LogFilter.None)
                                writer.Write("({0}): ", message.filter.ToString().ToUpper());

                            // Write message
                            writer.WriteLine(message.msg);

                            // Write stack trace
                            if (message.stackTrace != null)
                                writer.WriteLine(message.stackTrace);

                            // Allow some spacing to make the log easier to read
                            if (forceStackTrace == true)
                                writer.WriteLine();
                            break;
                        }

                    case LogType.Warning:
                        {
                            // Write severity
                            writer.Write("[WARNING]: ");

                            // Write filter
                            if (message.filter != LogFilter.None)
                                writer.Write("({0}): ", message.filter.ToString().ToUpper());

                            // Write message
                            writer.WriteLine(message.msg);

                            // Write stack trace
                            if (message.stackTrace != null)
                                writer.WriteLine(message.stackTrace);

                            // Allow some spacing to make the log easier to read
                            if (forceStackTrace == true)
                                writer.WriteLine();
                            break;
                        }

                    case LogType.Error:
                        {
                            // Write severity
                            writer.Write("[ERROR]: ");

                            // Write filter
                            if (message.filter != LogFilter.None)
                                writer.Write("({0}): ", message.filter.ToString().ToUpper());

                            // Write message
                            writer.WriteLine(message.msg);

                            // Write stack trace
                            if (message.stackTrace != null)
                                writer.WriteLine(message.stackTrace);

                            // Allow some spacing to make the log easier to read
                            if (forceStackTrace == true)
                                writer.WriteLine();
                            break;
                        }

                    case LogType.Exception:
                        {
                            // Write severity
                            writer.Write("[EXCEPTION]: ");

                            // Write filter
                            if (message.filter != LogFilter.None)
                                writer.Write("({0}): ", message.filter.ToString().ToUpper());

                            // Write message
                            writer.WriteLine(message.msg);

                            // Write stack trace
                            if (message.stackTrace != null)
                                writer.WriteLine(message.stackTrace);

                            // Allow some spacing to make the log easier to read
                            if (forceStackTrace == true)
                                writer.WriteLine();
                            break;
                        }
                }
            }

            public void Dispose()
            {
                writer.Dispose();
                writer = null;
            }
        }

        public struct LogMessage
        {
            // Public
            public LogType type;
            public object sender;
            public string msg;
            public string stackTrace;
            public LogFilter filter;

            // Methods
            public override string ToString()
            {
                string tagMsg = (filter != LogFilter.None)
                    ? string.Format(" [{0}]: ", filter.ToString())
                    : string.Empty;

                string senderMsg = (sender != null)
                    ? string.Format("({0}) - ", sender)
                    : string.Empty;

                return string.Concat(type.ToString().ToUpper(), ": ", tagMsg, senderMsg, msg);
            }
        }

        // Events
        public static GameEvent<LogMessage> OnMessage = new GameEvent<LogMessage>();

        // Private
#if !SIMPLE2D_WEB
        private static bool threadAbort = false;
        private static ManualResetEvent threadAbortEvent = new ManualResetEvent(false);
        private static Queue<LogMessage> messages = new Queue<LogMessage>();
#endif
        private static List<ILogger> loggers = new List<ILogger>();

        // Public
#if DEBUG
        public static bool forceStackTrace = true;
#else
        public static bool forceStackTrace = false;
#endif

        // Constructor
        static Debug()
        {
#if !SIMPLE2D_WEB
            // Start logging thread
            ThreadPool.QueueUserWorkItem(LogThreadMain);
#endif
        }

        // Methods
        public static void AddLogger(ILogger logger)
        {
            if (logger == null)
                return;

            // Check for already added
            if (loggers.Contains(logger) == false)
            {
                lock (loggers)
                {
                    // Register logger
                    loggers.Add(logger);
                }
            }
        }

        public static void RemoveLogger(ILogger logger)
        {
            if (logger == null)
                return;

            // Check for added
            if (loggers.Contains(logger) == true)
            {
                lock (loggers)
                {
                    // Remove logger
                    loggers.Remove(logger);
                }
            }
        }

        public static void Log(string msg)
        {
            Log(null, msg);
        }

        public static void Log(object sender, string msg)
        {
            // Create the message
            LogMessage m = new LogMessage
            {
                type = LogType.Info,
                sender = sender,
                msg = msg,
                filter = LogFilter.None,
                stackTrace = (forceStackTrace == true) ? Environment.StackTrace : null,
            };

#if !SIMPLE2D_WEB
            lock (messages)
            {
                messages.Enqueue(m);
            }
#else
            DispatchMessage(m);
#endif
        }

        public static void Log(LogFilter filter, string msg)
        {
            Log(filter, null, msg);
        }

        public static void Log(LogFilter filter, object sender, string msg)
        {
            // Create the message
            LogMessage m = new LogMessage
            {
                type = LogType.Info,
                sender = sender,
                msg = msg,
                filter = filter,
                stackTrace = (forceStackTrace == true) ? Environment.StackTrace : null,
            };

#if !SIMPLE2D_WEB
            lock (messages)
            {
                messages.Enqueue(m);
            }
#else
            DispatchMessage(m);
#endif
        }

        public static void LogF(string format, params object[] args)
        {
            LogF((object)null, format, args);
        }

        public static void LogF(object sender, string format, params object[] args)
        {
            LogF(sender, string.Format(format, args));
        }

        public static void LogF(LogFilter filter, string format, params object[] args)
        {
            LogF(filter, (object)null, format, args);
        }

        public static void LogF(LogFilter filter, object sender, string format, params object[] args)
        {
            Log(filter, sender, string.Format(format, args));
        }

        public static void LogWarning(string msg)
        {
            LogWarning(null, msg);
        }

        public static void LogWarning(object sender, string msg)
        {
            LogMessage m = new LogMessage
            {
                type = LogType.Warning,
                sender = sender,
                msg = msg,
                filter = LogFilter.None,
                stackTrace = (forceStackTrace == true) ? Environment.StackTrace : null,
            };

#if !SIMPLE2D_WEB
            lock (messages)
            {
                messages.Enqueue(m);
            }
#else
            DispatchMessage(m);
#endif
        }

        public static void LogWarning(LogFilter filter, string msg)
        {
            LogWarning(filter, null, msg);
        }

        public static void LogWarning(LogFilter filter, object sender, string msg)
        {
            LogMessage m = new LogMessage
            {
                type = LogType.Warning,
                sender = sender,
                msg = msg,
                filter = filter,
                stackTrace = (forceStackTrace == true) ? Environment.StackTrace : null,
            };

#if !SIMPLE2D_WEB
            lock (messages)
            {
                messages.Enqueue(m);
            }
#else
            DispatchMessage(m);
#endif
        }

        public static void LogWarningF(string format, params object[] args)
        {
            LogWarningF((object)null, format, args);
        }

        public static void LogWarningF(object sender, string format, params object[] args)
        {
            LogWarning(sender, string.Format(format, args));
        }

        public static void LogWarningF(LogFilter filter, string format, params object[] args)
        {
            LogWarningF(filter, (object)null, format, args);
        }

        public static void LogWarningF(LogFilter filter, object sender, string format, params object[] args)
        {
            LogWarning(filter, sender, string.Format(format, args));
        }

        public static void LogError(string msg)
        {
            LogError(null, msg);
        }

        public static void LogError(object sender, string msg)
        {
            LogMessage m = new LogMessage
            {
                type = LogType.Error,
                sender = sender,
                msg = msg,
                filter = LogFilter.None,
                stackTrace = Environment.StackTrace,
            };

#if !SIMPLE2D_WEB
            lock (messages)
            {
                messages.Enqueue(m);
            }
#else
            DispatchMessage(m);
#endif
        }

        public static void LogError(LogFilter filter, string msg)
        {
            LogError(filter, null, msg);
        }

        public static void LogError(LogFilter filter, object sender, string msg)
        {
            LogMessage m = new LogMessage
            {
                type = LogType.Error,
                sender = sender,
                msg = msg,
                filter = filter,
                stackTrace = Environment.StackTrace,
            };

#if !SIMPLE2D_WEB
            lock (messages)
            {
                messages.Enqueue(m);
            }
#else
            DispatchMessage(m);
#endif
        }

        public static void LogErrorF(string format, params object[] args)
        {
            LogErrorF((object)null, format, args);
        }

        public static void LogErrorF(object sender, string format, params object[] args)
        {
            LogError(sender, string.Format(format, args));
        }

        public static void LogErrorF(LogFilter filter, string format, params object[] args)
        {
            LogErrorF(filter, (object)null, format, args);
        }

        public static void LogErrorF(LogFilter filter, object sender, string format, params object[] args)
        {
            LogError(filter, sender, string.Format(format, args));
        }

        public static void LogException(Exception e)
        {
            LogException(null, e);

#if DEBUG
            //throw e;
            //Debugger.Break();
#endif
        }

        public static void LogException(object sender, Exception e)
        {
            // Get message and stack trace
            string msg, trace;
            GetExceptionMessage(e, out msg, out trace);

            LogMessage m = new LogMessage
            {
                type = LogType.Exception,
                sender = sender,
                msg = msg,
                stackTrace = trace,
            };

#if !SIMPLE2D_WEB
            lock (messages)
            {
                messages.Enqueue(m);
            }
#else
            DispatchMessage(m);
#endif
        }

        internal static void Terminate()
        {
#if !SIMPLE2D_WEB
            // Stop logging thread
            threadAbort = true;

            // Wait for thread
            threadAbortEvent.WaitOne();
            threadAbortEvent.Dispose();
#endif

            foreach (ILogger logger in loggers)
                logger.Dispose();
        }

        private static void GetExceptionMessage(Exception e, out string message, out string stackTrace)
        {
            message = e.Message;

            if (e.StackTrace != null)
            {
                stackTrace = e.StackTrace;
            }
            else
            {
                stackTrace = Environment.StackTrace;
            }

        }

#if !SIMPLE2D_WEB
        private static void LogThreadMain(object state)
        {
            LogMessage message = default;

            // Loop forever
            while (threadAbort == false || messages.Count > 0)
            {
                // Process queued messages
                while (messages.Count > 0)
                {
                    lock (messages)
                    {
                        // Fetch item
                        message = messages.Dequeue();
                    }

                    // Dispatch to all loggers
                    DispatchMessage(message);
                }

                // Allow thread to sleep
                Thread.Sleep(16);
            }

            // Thread is about to exit
            threadAbortEvent.Set();
        }
#endif

        private static void DispatchMessage(in LogMessage message)
        {
            // Dispatch to all loggers
            foreach (ILogger logger in loggers)
            {
                try
                {
                    logger.OnMessage(message);
                }
                catch (Exception e)
                {
                    // Logger caused an exception - remove it and log the exception (Remove is important so we don't cause log a loop)
                    //RemoveLogger(loggers[i]);
                    Debug.LogException(e);
                }
            }

            // Dispatch all events
            OnMessage.Raise(message);
        }
    }
}