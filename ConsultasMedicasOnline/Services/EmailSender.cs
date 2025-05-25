using Microsoft.AspNetCore.Identity.UI.Services;

namespace ConsultasMedicasOnline.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Por enquanto, apenas registra no console para desenvolvimento
            // Em produção, você deve implementar o envio real de email
            Console.WriteLine($"Email para: {email}");
            Console.WriteLine($"Assunto: {subject}");
            Console.WriteLine($"Mensagem: {htmlMessage}");
            
            return Task.CompletedTask;
        }
    }
}
