﻿using System.ComponentModel.DataAnnotations;

namespace FoodDeliveryApplication.Models
{
    public class LoginDetails
    {
        [Required(ErrorMessage="UserName is Required")]
        public string UserName { get; set; }


        [Required(ErrorMessage = "Email is Required")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password is Required")]
        public string Password { get; set; }
    }
}
