﻿using EmailSender.Extensions;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;


namespace MailSender.Core
{
    public class Email
    {
        private SmtpClient _smtp;
        private MailMessage _mail;

        private string _hostSmtp;
        private bool _enableSsl;
        private int _port;
        private string _senderEmail;
        private string _senderEmailPassword;
        private string _senderName;

        public Email(EmailParams emailParams)
        {
            _hostSmtp = emailParams.HostSmtp;
            _enableSsl = emailParams.EnableSsl;
            _port = emailParams.Port;
            _senderEmail = emailParams.SenderEmail;
            _senderEmailPassword = emailParams.SenderEmailPassword;
            _senderName = emailParams.SenderName;
        }
        public async Task Send(string subject, string body, string to,Attachment attachment)
        {            
            _mail = new MailMessage();
            _mail.From = new MailAddress(_senderEmail, _senderName);
            _mail.To.Add(new MailAddress(to));
            if (attachment!=null)
                _mail.Attachments.Add(attachment);
            _mail.IsBodyHtml = true;
            _mail.Subject = subject;
            _mail.BodyEncoding = Encoding.UTF8;
            _mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body.StripHTML(), null, MediaTypeNames.Text.Plain));
            _mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString($@"
                                                                                    <html>
                                                                                        <head>
                                                                                        </head>
                                                                                        <body>
                                                                                            <div style='font-size: 16px padding: 10px; font-family: Arial;'>
                                                                                                {body}
                                                                                            </div>
                                                                                        </body>
                                                                                        </html>", 
                                                                                        null, MediaTypeNames.Text.Html));

            _smtp = new SmtpClient
            {
                Host = _hostSmtp,
                EnableSsl = _enableSsl,
                Port = _port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_senderEmail, _senderEmailPassword)

            };
            _smtp.SendCompleted += OnSendCompleted;
            await _smtp.SendMailAsync(_mail);
        }

        private void OnSendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _smtp.Dispose();
            _mail.Dispose();
        }
    }
}
