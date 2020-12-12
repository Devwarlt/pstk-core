using System;
using System.Net.Mail;

namespace PSTk.Core.Tools.Google
{
    /// <summary>
    /// Represents a mail message used by <see cref="GMailCentral"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public sealed class GMailMessage
    {
        private readonly MailAddress to;

        /// <summary>
        /// Create a new instance of <see cref="GMailMessage"/>.
        /// </summary>
        /// <param name="to"></param>
        public GMailMessage(string to)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentNullException(nameof(to));

            this.to = new MailAddress(to);
        }

        /// <summary>
        /// Build a <see cref="MailMessage"/>.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isBodyHtml"></param>
        /// <returns></returns>
        public MailMessage Build(string subject, string body, bool isBodyHtml = false)
            => new MailMessage(null, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            };
    }
}
