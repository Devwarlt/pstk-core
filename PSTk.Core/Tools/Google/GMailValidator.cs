using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace PSTk.Core.Tools.Google
{
    /// <summary>
    /// Allow support to validates email using GMail SMTP services.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public sealed class GMailValidator
    {
        private const string CRLF = "\r\n";
        private const string SMTP_DNS = "gmail-smtp-in.l.google.com";
        private const int SMTP_PORT = 25;
        private const int STMP_INVALID_RESPONSE = 550;

        private readonly string emailRegistry;

        /// <summary>
        /// Create new instance of <see cref="GMailValidator"/> using <paramref name="emailRegistry"/> (valid Google account).
        /// </summary>
        /// <param name="emailRegistry"></param>
        public GMailValidator(string emailRegistry)
        {
            if (string.IsNullOrWhiteSpace(emailRegistry))
                throw new ArgumentNullException(nameof(emailRegistry));

            this.emailRegistry = emailRegistry;
        }

        /// <summary>
        /// Verify if <paramref name="email"/> is a valid GMail.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool IsValid(string email)
        {
            using (var client = new TcpClient(SMTP_DNS, SMTP_PORT))
            {
                using (var stream = client.GetStream())
                {
                    using (var rdr = new StreamReader(stream))
                    {
                        PerformHELO(stream, rdr);
                        PerformMAIL(emailRegistry, stream, rdr);
                        PerformRCPT(stream, rdr, email, out var isValid);
                        PerformQUITE(stream);
                        client.Close();
                        return isValid;
                    }
                }
            }
        }

        private static void PerformHELO(NetworkStream stream, StreamReader rdr)
        {
            rdr.ReadLine();
            WriteUTF(stream, "HELO LGSGMV");
        }

        private static void PerformMAIL(string emailRegistry, NetworkStream stream, StreamReader rdr)
        {
            rdr.ReadLine();
            WriteUTF(stream, $"MAIL FROM:<{emailRegistry}>");
        }

        private static void PerformQUITE(NetworkStream stream) => WriteUTF(stream, "QUITE");

        private static void PerformRCPT(NetworkStream stream, StreamReader rdr, string email, out bool isValid)
        {
            rdr.ReadLine();
            WriteUTF(stream, $"RCPT TO:<{email}>");
            var code = rdr.ReadLine();
            isValid = int.Parse(code.Substring(0, 3)) != STMP_INVALID_RESPONSE;
        }

        private static void WriteUTF(NetworkStream stream, string message)
        {
            var buffer = Encoding.ASCII.GetBytes(message + CRLF);
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}
