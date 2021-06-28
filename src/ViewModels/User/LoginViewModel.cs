using System.ComponentModel.DataAnnotations;

namespace SampleMvcApp.ViewModels.User
{
    public class LoginViewModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
