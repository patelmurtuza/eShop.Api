using eShopIndia.API.Utilities;
using eShopIndia.Entity.BaseEntities;
using ExceptionHandler;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace eShopIndia.API.Common
{
    [Route("v1/[controller]")]
    public class BaseController : Controller
    {
        protected static string GenericSessionErrorMsg = Messages.GenericSessionErrorMsg;
        protected static string GeneriLargeDataErrorMsg = Messages.GeneriLargeDataErrorMsg;
        public string GlobalSessionSourcePath = "";
        private ExceptionWriteLog Logger = null;
        private ExceptionMessage ExceptionMessage = null;
        protected readonly AppSettings _config;
        protected string LogFormat = "TXT";
        protected int RowsLimit = 2500;

        public BaseController(IOptions<AppSettings> config)
        {
            _config = config.Value;
            _config.RowsLimit = RowsLimit;
        }

        protected void Log(Exception ex)
        {
            ExceptionMessage = new ExceptionMessage(Messages.ExceptionMessageEnter);
            if (Logger == null)
                Logger = new ExceptionWriteLog();
            Logger.LogException(ExceptionMessage, LogFormat, ex);
        }

        public void LogBaseControllerMessage(Exception ex, ExceptionMessage message)
        {
            message.Fail();
            message.LastMessage = ex.Message;
            if (message.UserMessages.Count == 0)
                message.LastUserMessage = GenericSessionErrorMsg;
            if (Logger == null)
                Logger = new ExceptionWriteLog();
            Logger.LogException(message, LogFormat, ex);

        }

        protected bool NotValidIdentity()
        {
            return false; //TBD
        }

        protected void SessionExpired(ExceptionMessage message)
        {
            message.Fail();
        }

        [HttpGet("sendsms")]
        public bool SendSMS(string MobileNo, string Sbody)
        {
            bool result;
            try
            {
                ISMTPService CommunicationChannel = new SMTPService(_config.SMSsettings.SMSUserId, _config.SMSsettings.SMSPassword, _config.SMSsettings.SenderId, _config.SMSsettings.RouteId);
                result = CommunicationChannel.SendSMS(Sbody, MobileNo);
                return result;
            }
            catch
            {
                return true;
            }
        }

        [HttpGet("sendemail")]
        protected bool SendEmail(EmailEntity entity)
        {
            bool result;
            try
            {
                ISMTPService CommunicationChannel = new SMTPService(_config.Emailsettings.SmtpAddress, _config.Emailsettings.EmailFrom, _config.Emailsettings.Displayname, _config.Emailsettings.PortNo, _config.Emailsettings.MailPassword);
                result = CommunicationChannel.SendMail(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
    }
}
