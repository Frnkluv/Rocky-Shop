using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Rocky.Utility
{
    public class EmailSender : IEmailSender
    {
        // для получения доступа к ключам из аппсетингс (ридонли поле, св-во, конструктор)
        private readonly IConfiguration _configuration;
        public MailJetSettings _mailJetSettings { get; set; }   // для настройки почты

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // далее иду в Execute и даю значение _mailJetSettings


        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }

        public async Task Execute(string email, string subject, string body)
        {
            /* установить nuget packege Mailjet.API (ключи в MailJetClient взяты в лк на сайте) и
             * !!! после вставки добавить в STARTUP сервис ( services.AddTransient<IEmailSender, EmailSender>(); )
             * в папке Areas/.../ в Register добавится код в методе OnPostAsync, который вызывает EmailSender (начинается с var callBackUrl)
             * 
             * Ключи надо хранить в appsettings.json. -> Пишу их там -> создаю в утилити класс, делаю св-ва с таким же именем как в json 
             * и тут (в MailjetClient) указываю эти свойства.
             */


            _mailJetSettings = _configuration.GetSection("MailJet").Get<MailJetSettings>();
            /* в GetSection("название секции")
             * Get<MailJetSettings>() => в скобках это, значит все вс-ва MailJetSettings будут заполнены
             */

            // вставка с сайта mailjet + свои корректировки:
            MailjetClient client = new MailjetClient(_mailJetSettings.ApiKey, _mailJetSettings.SecretKey)
            {
                Version = ApiVersion.V3_1,
            };
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
             .Property(Send.Messages, new JArray {
     new JObject {
      {
       "From",
       new JObject {
        {"Email", "egormed52@proton.me"},
        {"Name", "Egor"}
       }
      }, {
       "To",
       new JArray {
        new JObject {
         {
          "Email",
          email
         }, {
          "Name",
          "TripDev"
         }
        }
       }
      }, {
       "Subject",
       subject
      },  {
       "HTMLPart",
       body
      }
     }
             });
            await client.PostAsync(request);
        }
    }
}
