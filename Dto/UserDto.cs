using System.ComponentModel.DataAnnotations;

namespace AspNetDemoPortalAPI.Dto
{
    public class UserDto
    {
        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "That doesn't look like a valid email.")]
        public String Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public String Password { get; set; }
    }
}
