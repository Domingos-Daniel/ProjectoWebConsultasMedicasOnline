using ConsultasMedicasOnline.Models;
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
        Task SendStatusChangeNotificationAsync(string toEmail, string pacienteName, string doctorName, DateTime appointmentDate, StatusConsulta status);
        Task SendProntuarioCreatedNotificationAsync(string toEmail, string pacienteName, string doctorName, int prontuarioId, DateTime consultaDate);
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

        public async Task SendStatusChangeNotificationAsync(string toEmail, string pacienteName, string doctorName, DateTime appointmentDate, StatusConsulta status)
        {
            var subject = GetStatusChangeSubject(status);
            var body = GetStatusChangeEmailBody(pacienteName, doctorName, appointmentDate, status);

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendProntuarioCreatedNotificationAsync(string toEmail, string pacienteName, string doctorName, int prontuarioId, DateTime consultaDate)
        {
            var subject = "Seu prontuário médico está disponível";
            var baseUrl = _configuration["ApplicationUrl"] ?? "https://localhost:5001";
            
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                    <div style='text-align: center; padding: 15px; background-color: #f0f9ff; border-radius: 5px;'>
                        <h1 style='color: #1e40af; margin: 0;'>MedConsulta</h1>
                        <p style='color: #64748b; margin: 5px 0 0;'>Sistema de Consultas Médicas Online</p>
                    </div>
                    
                    <div style='padding: 20px;'>
                        <p>Olá, <strong>{pacienteName}</strong>!</p>
                        
                        <p>O <strong>Dr. {doctorName}</strong> concluiu o seu prontuário médico da consulta realizada em <strong>{consultaDate.ToString("dd/MM/yyyy")}</strong>.</p>
                        
                        <p>Você pode acessar seu prontuário completo através do link abaixo:</p>
                        
                        <div style='text-align: center; margin: 25px 0;'>
                            <a href='{baseUrl}/Prontuarios/Details/{prontuarioId}' 
                               style='background-color: #1e40af; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                                Visualizar Prontuário
                            </a>
                        </div>
                        
                        <div style='margin-top: 30px; padding: 15px; background-color: #f0f9ff; border-radius: 5px;'>
                            <p style='margin: 0; color: #1e40af;'><strong>Importante:</strong> O prontuário contém informações importantes sobre seu diagnóstico, tratamentos prescritos e orientações médicas.</p>
                        </div>
                    </div>
                    
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0;'>
                        <p style='color: #64748b; font-size: 14px;'>© {DateTime.Now.Year} MedConsulta - Sistema de Consultas Médicas Online</p>
                    </div>
                </div>
            ";
            
            await SendEmailAsync(toEmail, subject, body);
        }

        private string GetStatusChangeSubject(StatusConsulta status)
        {
            return status switch
            {
                StatusConsulta.Confirmada => "Sua consulta foi confirmada!",
                StatusConsulta.Cancelada => "Sua consulta foi cancelada",
                StatusConsulta.Concluida => "Sua consulta foi concluída - Feedback",
                StatusConsulta.EmAndamento => "Sua consulta está em andamento",
                StatusConsulta.NaoCompareceu => "Registro de falta em sua consulta",
                _ => "Atualização sobre sua consulta médica"
            };
        }

        private string GetStatusChangeEmailBody(string pacienteName, string doctorName, DateTime appointmentDate, StatusConsulta status)
        {
            string statusText = status switch
            {
                StatusConsulta.Confirmada => "confirmada",
                StatusConsulta.Cancelada => "cancelada",
                StatusConsulta.Concluida => "concluída",
                StatusConsulta.EmAndamento => "em andamento",
                StatusConsulta.Agendada => "agendada",
                StatusConsulta.NaoCompareceu => "marcada como falta",
                _ => status.ToString().ToLower()
            };

            string statusColor = status switch
            {
                StatusConsulta.Confirmada => "#15803d",  // green-700
                StatusConsulta.Cancelada => "#b91c1c",   // red-700
                StatusConsulta.Concluida => "#1d4ed8",   // blue-700
                StatusConsulta.EmAndamento => "#0891b2", // cyan-700
                StatusConsulta.Agendada => "#a16207",    // yellow-700
                StatusConsulta.NaoCompareceu => "#be123c", // rose-700
                _ => "#525252"                           // gray-600
            };

            string bgColor = status switch
            {
                StatusConsulta.Confirmada => "#f0fdf4",  // green-50
                StatusConsulta.Cancelada => "#fef2f2",   // red-50
                StatusConsulta.Concluida => "#eff6ff",   // blue-50
                StatusConsulta.EmAndamento => "#ecfeff", // cyan-50
                StatusConsulta.Agendada => "#fefce8",    // yellow-50
                StatusConsulta.NaoCompareceu => "#fff1f2", // rose-50
                _ => "#f8fafc"                           // gray-50
            };

            string additionalInfo = status switch
            {
                StatusConsulta.Confirmada => "<p>Por favor, chegue com 15 minutos de antecedência. Traga seus documentos e exames anteriores, se houver.</p>",
                StatusConsulta.Cancelada => "<p>Se desejar reagendar, acesse nosso sistema ou entre em contato conosco.</p>",
                StatusConsulta.Concluida => "<p>Agradecemos sua visita! Se necessário, não esqueça de agendar sua consulta de retorno.</p>",
                StatusConsulta.EmAndamento => "<p>O médico já está lhe aguardando. Se sua consulta for online, acesse o link da videochamada.</p>",
                StatusConsulta.Agendada => "<p>Aguardando confirmação do médico. Você receberá uma notificação quando houver atualização.</p>",
                StatusConsulta.NaoCompareceu => "<p>Foi registrada sua ausência na consulta. Se precisar reagendar, entre em contato conosco.</p>",
                _ => ""
            };

            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 5px;'>
                    <div style='text-align: center; padding: 10px; background-color: {bgColor}; border-radius: 5px;'>
                        <h2 style='color: {statusColor};'>Consulta {statusText.ToUpper()}</h2>
                    </div>
                    
                    <div style='padding: 20px;'>
                        <p>Olá, <strong>{pacienteName}</strong>!</p>
                        
                        <p>Sua consulta com <strong>Dr. {doctorName}</strong> foi <strong>{statusText}</strong>.</p>
                        
                        <div style='background-color: #f8fafc; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p><strong>Data/Hora:</strong> {appointmentDate.ToString("dd/MM/yyyy HH:mm")}</p>
                            <p><strong>Status:</strong> <span style='color: {statusColor}; font-weight: bold;'>{statusText.ToUpper()}</span></p>
                        </div>
                        
                        {additionalInfo}
                    </div>
                    
                    <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #e0e0e0;'>
                        <p style='color: #64748b; font-size: 14px;'>Este é um e-mail automático. Por favor, não responda diretamente.</p>
                        <p style='color: #64748b; font-size: 14px;'>© {DateTime.Now.Year} Consultas Médicas Online - Todos os direitos reservados.</p>
                    </div>
                </div>
            ";
        }
    }
}
