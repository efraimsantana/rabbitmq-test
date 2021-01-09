using System.Net.Mail;
using System.Threading.Tasks;
using MoveFile.API.Infra;
using MoveFile.API.Model;
using RabbitMQ.Client;

namespace MoveFile.API.Services
{
    public class MoveFileAppService : IMoveFileAppService
    {
        private readonly IModel rabbitMqModel;
        private readonly IQueueConsumer<FileViewModel> queueConsumer;
        private const string queueName = "s3";

        public MoveFileAppService(IModel rabbitMqModel, 
                                  IQueueConsumer<FileViewModel> queueConsumer)
        {
            this.rabbitMqModel = rabbitMqModel;
            this.queueConsumer = queueConsumer;
        }

        public void Consumer()
        {
            queueConsumer.Start(queueName, a =>
            {   
                using (var smtp = new SmtpClient())
                {
                    smtp.Host = "localhost";
                    smtp.Port = 1025;
                    smtp.EnableSsl = false;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential("from@gmail.com", "password");

                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress("from@gmail.com");
                        mail.To.Add(new MailAddress("to@gmail.com"));
                        mail.CC.Add(new MailAddress("cc@gmail.com"));
                        mail.Subject = "File moved";
                        mail.Body = "File moved successfully. ";

                        var task = Task.Run(async () => await smtp.SendMailAsync(mail));
                        Task.WaitAll(task);
                    }
                }
            });
        }
    }
}
