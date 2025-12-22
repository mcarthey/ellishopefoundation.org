using EllisHope.Models.Domain;

namespace EllisHope.Services;

/// <summary>
/// Service for generating email templates
/// </summary>
public interface IEmailTemplateService
{
    // Application workflow templates
    string GenerateApplicationSubmittedEmail(ClientApplication application);
    string GenerateApplicationUnderReviewEmail(ClientApplication application);
    string GenerateNewApplicationNotificationEmail(ClientApplication application, string boardMemberName);
    string GenerateVoteRequestEmail(ClientApplication application, string boardMemberName);
    string GenerateQuorumReachedEmail(ClientApplication application);
    string GenerateApplicationApprovedEmail(ClientApplication application);
    string GenerateApplicationRejectedEmail(ClientApplication application);
    string GenerateInformationRequestedEmail(ClientApplication application, string requestDetails);

    // Account-related templates
    string GenerateWelcomeEmail(string firstName);
    string GeneratePasswordResetEmail(string firstName, string resetUrl);
    string GeneratePasswordChangedEmail(string firstName);
}

public class EmailTemplateService : IEmailTemplateService
{
    private readonly string _baseUrl;

    public EmailTemplateService(IConfiguration configuration)
    {
        _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://ellishope.org";
    }

    public string GenerateApplicationSubmittedEmail(ClientApplication application)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Application Submitted Successfully!</h1>
        </div>
        <div class='content'>
            <p>Dear {application.FirstName},</p>
            
            <p>Thank you for submitting your application to the Ellis Hope Foundation!</p>
            
            <p>We have received your application (#<strong>{application.Id}</strong>) and our board will review it carefully.</p>
            
            <p><strong>What happens next?</strong></p>
            <ol>
                <li>Our board members will review your application</li>
                <li>They may request additional information if needed</li>
                <li>You'll be notified of the decision via email</li>
            </ol>
            
            <p>You can track the status of your application at any time:</p>
            <p style='text-align: center;'>
                <a href='{_baseUrl}/MyApplications/Details/{application.Id}' class='button'>
                    View Application Status
                </a>
            </p>
            
            <p>Thank you for your interest in our program!</p>
            
            <p>Best regards,<br/>
            <strong>The Ellis Hope Foundation Team</strong></p>
        </div>
        <div class='footer'>
            <p>Ellis Hope Foundation | Empowering Health, Fitness, and Hope</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GenerateApplicationUnderReviewEmail(ClientApplication application)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #17a2b8; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Application Under Review</h1>
        </div>
        <div class='content'>
            <p>Dear {application.FirstName},</p>
            
            <p>Good news! Your application (#<strong>{application.Id}</strong>) is now being reviewed by our board members.</p>
            
            <p>Our board will carefully consider your application and may reach out if they need any additional information.</p>
            
            <p>You can check the status of your application at any time:</p>
            <p style='text-align: center;'>
                <a href='{_baseUrl}/MyApplications/Details/{application.Id}' class='button'>
                    View Application
                </a>
            </p>
            
            <p>We appreciate your patience as we review your application!</p>
            
            <p>Best regards,<br/>
            <strong>The Ellis Hope Foundation Team</strong></p>
        </div>
        <div class='footer'>
            <p>Ellis Hope Foundation | Empowering Health, Fitness, and Hope</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GenerateNewApplicationNotificationEmail(ClientApplication application, string boardMemberName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ffc107; color: #333; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; }}
        .info-box {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #007bff; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? New Application Received</h1>
        </div>
        <div class='content'>
            <p>Dear {boardMemberName},</p>
            
            <p>A new application has been submitted and is ready for your review!</p>
            
            <div class='info-box'>
                <p><strong>Application ID:</strong> #{application.Id}</p>
                <p><strong>Applicant:</strong> {application.FullName}</p>
                <p><strong>Funding Types:</strong> {string.Join(", ", application.FundingTypesList)}</p>
                <p><strong>Estimated Monthly Cost:</strong> ${application.EstimatedMonthlyCost:N2}</p>
                <p><strong>Submitted:</strong> {DateTime.Now:MMMM dd, yyyy}</p>
            </div>
            
            <p>Please review the application and cast your vote:</p>
            <p style='text-align: center;'>
                <a href='{_baseUrl}/Admin/Applications/Review/{application.Id}' class='button'>
                    Review Application
                </a>
            </p>
            
            <p>Thank you for your service to the Ellis Hope Foundation!</p>
            
            <p>Best regards,<br/>
            <strong>Ellis Hope Foundation</strong></p>
        </div>
        <div class='footer'>
            <p>Ellis Hope Foundation | Board Member Portal</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GenerateVoteRequestEmail(ClientApplication application, string boardMemberName)
    {
        return GenerateNewApplicationNotificationEmail(application, boardMemberName);
    }

    public string GenerateQuorumReachedEmail(ClientApplication application)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>? Quorum Reached</h1>
        </div>
        <div class='content'>
            <p>Dear Board Member,</p>
            
            <p>All required votes have been received for application #<strong>{application.Id}</strong> ({application.FullName}).</p>
            
            <p>The application is now ready for final decision by the administrator.</p>
            
            <p style='text-align: center;'>
                <a href='{_baseUrl}/Admin/Applications/Details/{application.Id}' class='button'>
                    View Application
                </a>
            </p>
            
            <p>Thank you for your participation!</p>
            
            <p>Best regards,<br/>
            <strong>Ellis Hope Foundation</strong></p>
        </div>
        <div class='footer'>
            <p>Ellis Hope Foundation</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GenerateApplicationApprovedEmail(ClientApplication application)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; }}
        .success-box {{ background-color: #d4edda; border: 1px solid #c3e6cb; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Congratulations!</h1>
        </div>
        <div class='content'>
            <p>Dear {application.FirstName},</p>
            
            <div class='success-box'>
                <h2 style='color: #155724; margin-top: 0;'>Your application has been APPROVED!</h2>
            </div>
            
            <p>We are thrilled to inform you that your application (#<strong>{application.Id}</strong>) has been approved by our board of directors!</p>
            
            {(application.ApprovedMonthlyAmount.HasValue ? $"<p><strong>Approved Monthly Support:</strong> ${application.ApprovedMonthlyAmount:N2}</p>" : "")}
            
            <p>{application.DecisionMessage}</p>
            
            <p><strong>Next Steps:</strong></p>
            <ol>
                <li>We will contact you within 5 business days to schedule your initial consultation</li>
                <li>Complete any required health screenings</li>
                <li>Meet with your assigned trainer and nutritionist</li>
                <li>Begin your fitness journey!</li>
            </ol>
            
            <p style='text-align: center;'>
                <a href='{_baseUrl}/MyApplications/Details/{application.Id}' class='button'>
                    View Application Details
                </a>
            </p>
            
            <p>We look forward to supporting you on your path to better health and wellness!</p>
            
            <p>Congratulations again!</p>
            
            <p>Best regards,<br/>
            <strong>The Ellis Hope Foundation Team</strong></p>
        </div>
        <div class='footer'>
            <p>Ellis Hope Foundation | Empowering Health, Fitness, and Hope</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GenerateApplicationRejectedEmail(ClientApplication application)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #6c757d; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Application Decision</h1>
        </div>
        <div class='content'>
            <p>Dear {application.FirstName},</p>
            
            <p>Thank you for your application (#<strong>{application.Id}</strong>) to the Ellis Hope Foundation.</p>
            
            <p>After careful consideration by our board, we regret to inform you that we are unable to approve your application at this time.</p>
            
            {(!string.IsNullOrEmpty(application.DecisionMessage) ? $"<p>{application.DecisionMessage}</p>" : "")}
            
            <p>We encourage you to reapply in the future if your circumstances change. Our goal is to help as many people as possible, and we wish you the very best in your health and fitness journey.</p>
            
            <p>If you have any questions, please don't hesitate to contact us.</p>
            
            <p>Thank you for your interest in our program.</p>
            
            <p>Best regards,<br/>
            <strong>The Ellis Hope Foundation Team</strong></p>
        </div>
        <div class='footer'>
            <p>Ellis Hope Foundation | Empowering Health, Fitness, and Hope</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GenerateInformationRequestedEmail(ClientApplication application, string requestDetails)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ffc107; color: #333; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; }}
        .request-box {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Additional Information Needed</h1>
        </div>
        <div class='content'>
            <p>Dear {application.FirstName},</p>
            
            <p>Our board has reviewed your application (#<strong>{application.Id}</strong>) and would like to request some additional information to help us make an informed decision.</p>
            
            <div class='request-box'>
                <p><strong>Information Requested:</strong></p>
                <p>{requestDetails}</p>
            </div>
            
            <p>Please provide the requested information at your earliest convenience:</p>
            <p style='text-align: center;'>
                <a href='{_baseUrl}/MyApplications/Details/{application.Id}' class='button'>
                    Respond to Request
                </a>
            </p>
            
            <p>Thank you for your patience as we review your application!</p>
            
            <p>Best regards,<br/>
            <strong>The Ellis Hope Foundation Team</strong></p>
        </div>
        <div class='footer'>
            <p>Ellis Hope Foundation | Empowering Health, Fitness, and Hope</p>
        </div>
    </div>
</body>
</html>";
    }

    #region Account Email Templates

    public string GenerateWelcomeEmail(string firstName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #c53040; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #c53040; color: white; text-decoration: none; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Ellis Hope Foundation!</h1>
        </div>
        <div class='content'>
            <p>Hi {firstName},</p>

            <p>Thank you for creating an account with us. We're excited to have you as part of our community!</p>

            <p><strong>You can now:</strong></p>
            <ul>
                <li>Apply for support programs</li>
                <li>View upcoming events</li>
                <li>Explore volunteer opportunities</li>
                <li>Stay updated with our latest news</li>
            </ul>

            <p style='text-align: center;'>
                <a href='{_baseUrl}/Admin/Account/Login' class='button'>
                    Sign In to Your Account
                </a>
            </p>

            <p>If you have any questions, please don't hesitate to contact us.</p>

            <p>Best regards,<br/>
            <strong>The Ellis Hope Foundation Team</strong></p>
        </div>
        <div class='footer'>
            <p>Ellis Hope Foundation | Empowering Health, Fitness, and Hope</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GeneratePasswordResetEmail(string firstName, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #c53040; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #c53040; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }}
        .warning-box {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hi {firstName},</p>

            <p>We received a request to reset your password for your Ellis Hope Foundation account.</p>

            <p style='text-align: center; margin: 30px 0;'>
                <a href='{resetUrl}' class='button'>
                    Reset Your Password
                </a>
            </p>

            <div class='warning-box'>
                <p><strong>Important:</strong></p>
                <ul style='margin-bottom: 0;'>
                    <li>This link will expire in 24 hours</li>
                    <li>If you didn't request this reset, please ignore this email</li>
                    <li>Your password will remain unchanged until you create a new one</li>
                </ul>
            </div>

            <p>If the button above doesn't work, copy and paste this link into your browser:</p>
            <p style='word-break: break-all; font-size: 12px; color: #666;'>{resetUrl}</p>

            <p>Best regards,<br/>
            <strong>The Ellis Hope Foundation Team</strong></p>
        </div>
        <div class='footer'>
            <p>Ellis Hope Foundation | Empowering Health, Fitness, and Hope</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    public string GeneratePasswordChangedEmail(string firstName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f8f9fa; }}
        .success-box {{ background-color: #d4edda; border: 1px solid #c3e6cb; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .warning-box {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; margin: 15px 0; border-radius: 5px; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Changed Successfully</h1>
        </div>
        <div class='content'>
            <p>Hi {firstName},</p>

            <div class='success-box'>
                <p style='margin: 0;'><strong>Your password has been successfully changed.</strong></p>
            </div>

            <p>You can now use your new password to sign in to your account.</p>

            <p style='text-align: center;'>
                <a href='{_baseUrl}/Admin/Account/Login' class='button'>
                    Sign In Now
                </a>
            </p>

            <div class='warning-box'>
                <p style='margin: 0;'><strong>Didn't make this change?</strong> If you didn't change your password, please contact us immediately as your account may have been compromised.</p>
            </div>

            <p>Best regards,<br/>
            <strong>The Ellis Hope Foundation Team</strong></p>
        </div>
        <div class='footer'>
            <p>Ellis Hope Foundation | Empowering Health, Fitness, and Hope</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }

    #endregion
}
