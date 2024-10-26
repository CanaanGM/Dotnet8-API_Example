using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;

namespace Core.DTO.Authentication;
public class RegisterRequest
{
    [Required(ErrorMessage = "User Name can't be blank")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email can't be blank")]
    [EmailAddress(ErrorMessage = "Type a proper email address")]

    // will invoke IsEmailAvailable in AccountController and expects a boolean to be returned
    [Remote(action: "IsEmailAvailable", controller: "Account", ErrorMessage = "Email is already used")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone can't be blank")]
    [Phone(ErrorMessage = "Type a proper phone number")]
    public string Phone { get; set; } = string.Empty;


    [Required(ErrorMessage = "Password can't be blank")]
    public string Password { get; set; } = string.Empty;


    [Required(ErrorMessage = "Confirm Password can't be blank")]
    [Compare("Password", ErrorMessage = "The passwords don't match!")]
    public string ConfirmPassword { get; set; } = string.Empty;
}