using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using api.Configuration;
using api.DTO;
using api.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthManagementController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IOptionsMonitor<JwtConfig> _optionsMonitor;
        private readonly IConfiguration _configuration;

        public AuthManagementController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IOptionsMonitor<JwtConfig> optionsMonitor, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _optionsMonitor = optionsMonitor;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterationDto user)
        {
            
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

                if (existingUser != null)
                {

                    return BadRequest(new
                    {
                        message = "Email address already exists",
                        StatusCode = 400,
                        IsSuccessful = false
                    });
                }

                var newUser = new AppUser()
                {
                    PhoneNumber = user.PhoneNumber,
                    UserName = user.Email,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };

                var isCreated = await _userManager.CreateAsync(newUser);
                if (isCreated.Succeeded)
                { 

                    // Generate an email confirmation token for the new user
                    var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

                    // Construct the email verification link with the token and user ID
                    var verificationUrl = $"{Request.Scheme}://{Request.Host}/api/AuthManagement/VerifyEmail?userId={newUser.Id}&token={WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmationToken))}";

                    // Construct the SendGrid email message
                    var msg = new SendGridMessage();
                    msg.SetFrom(new EmailAddress("noreply@example.com", "Xpay"));
                    msg.AddTo(new EmailAddress(newUser.Email, $"{newUser.FirstName} {newUser.LastName}"));
                    msg.SetSubject("Please confirm your email address");
                    msg.AddContent(MimeType.Html, $@"<p>Dear {newUser.FirstName},</p>
                                                    <p>Thank you for registering with Xpay. To complete your registration, please click on the following link to verify your email address:</p>
                                                    <p><a href='{verificationUrl}'>Verify Email</a></p>
                                                    <p>If you did not register with Xpay, please ignore this email.</p>");

                    // Send the email using the SendGrid client
                    var sendGridClient = new SendGridClient(_configuration.GetSection("SendGrid:Api_Key").Value);
                    var response = await sendGridClient.SendEmailAsync(msg);


                    if (response.StatusCode >= HttpStatusCode.OK && response.StatusCode < HttpStatusCode.Ambiguous)
                    {
                        return Ok(new
                        {
                            Message = "Email sent successfully", 
                            StatusCode = response.StatusCode,
                            IsSuccessful = true,
                        });
                    }
                    
                    return BadRequest(new
                    {
                        message = "Couldn't send confirmation mail",
                        StatusCode = (int)response.StatusCode,
                        IsSuccessful = false,
                        Errors = isCreated.Errors.Select(x => x.Description).ToList()
                    });
                }
            }

            return BadRequest( new
            {   message = "Invalid payload",
                StatusCode = 400,
                IsSuccessful = false
            });
        }
    }
}












