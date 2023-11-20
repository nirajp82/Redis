using Microsoft.Extensions.Logging;

namespace ConsoleLogger
{
    // Custom TextWriter that redirects writes to the logger
    public class LoggerTextWriter : System.IO.TextWriter
    {
        private readonly ILogger _logger;

        public LoggerTextWriter(ILogger logger)
        {
            _logger = logger;
        }

        public override void Write(char value)
        {
            _logger.LogInformation(value.ToString());
        }

        public override void Write(string value)
        {
            _logger.LogInformation(value);
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
    }
}
