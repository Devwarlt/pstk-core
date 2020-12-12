using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace PSTk.Core.Tools.Google
{
    /// <summary>
    /// Manage easy email delivery using Google Mail SMTP services.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public sealed class GMailCentral
    {
        /// <summary>
        /// When an error occur while sending or processing a mail.
        /// </summary>
        public EventHandler<Exception> OnError;

        /// <summary>
        /// When successfully send mail.
        /// </summary>
        public EventHandler OnSuccess;

        private const string GmailPattern = @"^[0-9a-zA-Z_]+@gmail\.com$";
        private const string SMTP_HOST = "smtp.gmail.com";
        private const int SMTP_PORT = 587;

        private readonly string email;
        private readonly GMailValidator mailValidator;
        private readonly string secretToken;
        private readonly int timeout;

        /// <summary>
        /// Create a new instance of <see cref="GMailCentral"/>.
        /// <para>
        /// Note: <paramref name="secretToken"/> is the third-part authentication token that can be generated at
        /// Google Account. Regular password can be used instead.
        /// </para>
        /// </summary>
        /// <param name="email"></param>
        /// <param name="secretToken"></param>
        /// <param name="timeout"></param>
        public GMailCentral(string email, string secretToken, uint timeout = 15000)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));
            if (string.IsNullOrWhiteSpace(secretToken))
                throw new ArgumentNullException(nameof(secretToken));

            this.email = email;
            this.secretToken = secretToken;
            this.timeout = (int)timeout;
            mailValidator = new GMailValidator(email);
        }

        /// <summary>
        /// Verify if <paramref name="email"/> is a valid GMail using <see cref="GMailValidator"/> and <see cref="Regex"/>
        /// pattern recognition.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool IsValid(string email)
        {
            if (!Regex.IsMatch(email, GmailPattern))
            {
                OnError?.Invoke(this, new Exception("Invalid email, only Google emails are supported for this register platform."));
                return false;
            }

            return mailValidator.IsValid(email);
        }

        /// <summary>
        /// Asynchronously send mail.
        /// </summary>
        /// <param name="message"></param>
        public void SendMailAsync(MailMessage message)
        {
            message.From = new MailAddress(email);
            var smtp = new SmtpClient()
            {
                Host = SMTP_HOST,
                Port = SMTP_PORT,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(message.From.Address, secretToken),
                Timeout = timeout
            };
            smtp.SendCompleted += OnSendCompleted;

            try { smtp.SendAsync(message, message.To[0].Address); }
            catch (Exception e) { OnError?.Invoke(this, e); }
        }

        private void OnSendCompleted(object sender, AsyncCompletedEventArgs args)
        {
            var email = (string)args.UserState;
            if (args.Cancelled)
            {
                OnError?.Invoke(this, new TimeoutException($"Failed to send mail to '{email}'!", args.Error));
                return;
            }

            OnSuccess?.Invoke(this, null);
        }
    }
}
