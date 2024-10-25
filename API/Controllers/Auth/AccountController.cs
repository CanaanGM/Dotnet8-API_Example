// Ignore Spelling: jwt

using Core.DTO.Authentication;
using Core.Identity;
using Core.Services;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IJwtService _jwtService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AccountController(
        UserManager<ApplicationUser> userManager
        , SignInManager<ApplicationUser> signInManager
        , RoleManager<ApplicationRole> roleManager
        , IJwtService jwtService
        , IDateTimeProvider dateTimeProvider)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtService = jwtService;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// checks if the information provided to the guard is true
    /// </summary>
    /// <exception cref="ArgumentException">throws the papers in the face of the citizen if the info is false!</exception>
    private void ValidateModelState()
    {
        if (!ModelState.IsValid)
        {
            var errorMessage = string.Join(" | ", ModelState.Values.SelectMany(modelState => modelState.Errors).Select(err => err.ErrorMessage));

            throw new ArgumentException(errorMessage);
        }
    }

    /// <summary>
    /// Creates a new citizen in our city, providing them with an id and a token needed to refresh the id
    /// </summary>
    /// <param name="registerDTO">user information</param>
    /// <returns>User with token and refresh token</returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResponse>> RegisterAsync(RegisterDTO registerDTO)
    {
        // validation thru checking the model state, can be validated in other ways, but this is an example

        try
        {
            ValidateModelState();

            //* we're validating in the dto, this should be fine
            var user = new ApplicationUser
            {
                Email = registerDTO.Email,
                PhoneNumber = registerDTO.Phone,
                UserName = registerDTO.Email,
                DisplayName = registerDTO.DisplayName
            };

            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded)
            {
                // get all the errors in a string and throw it in the face of the client ! （づ￣3￣）づ╭❤️～
                var identityResultErrors = string.Join(" | ", result.Errors.Select(x => x.Description));
                throw new Exception(identityResultErrors);
            }

            // sign in
            // false means the cookie will be deleted when the browser is closed
            await _signInManager.SignInAsync(user, isPersistent: false);
            var authenticationReponse = _jwtService.CreateJwtToken(user);

            user.RefreshToken = authenticationReponse.RefreshToken;
            user.RefreshTokenExpirationDateTime = authenticationReponse.RefreshTokenExpirationDateTime;
            await _userManager.UpdateAsync(user);

            return Ok(authenticationReponse);

        }
        // can catch more specifically but meh
        catch (Exception ex)
        {
            // log it 
            // remember you could override it's factory and add in a middle ware for it
            return Problem(ex.Message);
        }

    }


    /// <summary>
    /// Sings a song of a user attempting to enter the system.
    /// it allows them in, signs their papers and gives them an token to refresh said id
    /// </summary>
    /// <param name="loginDTO">user email and password</param>
    /// <returns>User with token and refresh token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(LoginDTO loginDTO)
    {

        try
        {
            ValidateModelState();

            // this is very convenient compared to pure sql albeit less control.
            var result = await _signInManager.PasswordSignInAsync(
                loginDTO.Email,
                loginDTO.Password,
                isPersistent: false,
                lockoutOnFailure: false
                );

            if (!result.Succeeded)
                throw new Exception("Invalid email and password");

            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user is null) return NoContent();

            // update the user's refresh token
            var authenticationReponse = _jwtService.CreateJwtToken(user);
            user.RefreshToken = authenticationReponse.RefreshToken;
            user.RefreshTokenExpirationDateTime = authenticationReponse.RefreshTokenExpirationDateTime;
            await _userManager.UpdateAsync(user);

            return Ok(authenticationReponse);

        }
        catch (Exception ex)
        {
            // log it 
            // remember you could override it's factory and add in a middle ware for it
            return Problem(ex.Message);
        }

    }


    /// <summary>
    /// invalidates the user Token and claims, making them unauthenticated for future requests
    /// </summary>
    /// <returns>204</returns>

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    /// <summary>
    /// for use from the user register DTO, it checks if the email is taken or not.
    /// `Ok()` cause the request was correct and successful
    /// </summary>
    /// <param name="email">a string in the shape of email: jane@doe.come</param>
    /// <returns>true if the email is not already registered, otherwise false.</returns>
    [HttpGet]
    public async Task<IActionResult> IsEmailAvailable(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        return user is null ? Ok(true) : Ok(false);
    }

}
