﻿using System.ComponentModel.DataAnnotations;

namespace Core.DTO.Authentication;
public class LoginRequest
{
    [Required(ErrorMessage = "Email can't be blank")]
    [EmailAddress(ErrorMessage = "Provide a valid email please")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password can't be blank")]
    public string Password { get; set; } = string.Empty;

}