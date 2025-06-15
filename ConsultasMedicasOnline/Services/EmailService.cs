using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ConsultasMedicasOnline.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task SendAppointmentConfirmationAsync(string toEmail, string pacienteName, string doctorName, DateTime appointmentDate, string appointmentTime);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            var fromAddress = _configuration["EmailSettings:FromAddress"];
            var fromName = _configuration["EmailSettings:FromName"];
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"]);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(toEmail);

            using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = enableSsl;

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    // Log exception
                    Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task SendAppointmentConfirmationAsync(string toEmail, string pacienteName, string doctorName, DateTime appointmentDate, string appointmentTime)
        {
            var subject = "Agendamento de Consulta - Aguardando Aprovação";

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                    <div style='text-align: center; padding: 10px; background-color: #f0f9ff; border-radius: 5px;'>
                        <h2 style='color: #1e40af;'>Agendamento Recebido</h2>
                    </div>
                    
                    <div style='padding: 20px;'>
                        <p>Olá, <strong>{pacienteName}</strong>!</p>
                        
                        <p>Recebemos seu agendamento de consulta com <strong>Dr. {doctorName}</strong>.</p>
                        
                        <p>Detalhes da consulta:</p>
                        <ul style='background-color: #f8fafc; padding: 15px; border-radius: 5px;'>
                            <li><strong>Data:</strong> {appointmentDate.ToString("dd/MM/yyyy")} ({appointmentDate.ToString("dddd", new System.Globalization.CultureInfo("pt-BR"))})</li>
                            <li><strong>Horário:</strong> {appointmentTime}</li>
                            <li><strong>Status:</strong> <span style='color: #ca8a04; font-weight: bold;'>Aguardando aprovação</span></li>
                        </ul>
                        
                        <p>Você receberá outro e-mail quando sua consulta for <strong>confirmada</strong> pela nossa equipe.</p>
                        
                        <div style='margin-top: 30px; padding: 15px; background-color: #fef9c3; border-radius: 5px;'>
                            <p style='margin: 0; color: #854d0e;'><strong>Importante:</strong> Caso precise cancelar ou remarcar, entre em contato conosco com pelo menos 24 horas de antecedência.</p>
                        </div>
                    </div>
                    
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0;'>
                        <p style='color: #64748b; font-size: 14px;'>Este é um e-mail automático. Por favor, não responda diretamente.</p>
                        <p style='color: #64748b; font-size: 14px;'>© {DateTime.Now.Year} Consultas Médicas Online - Todos os direitos reservados.</p>
                    </div>
                </div>
            ";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
