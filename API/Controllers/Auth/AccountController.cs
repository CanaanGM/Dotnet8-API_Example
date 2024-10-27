// Ignore Spelling: jwt

using API.Controllers.v1;

using Core.DTO.Authentication;
using Core.Identity;
using Core.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace API.Controllers.Auth;


[AllowAnonymous]
public class AccountController : CustomBaseController
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
    /// <param name="RegisterRequest">user information</param>
    /// <returns>User with token and refresh token. <see cref="AuthenticationResponse"/></returns>
    [HttpPost("register")]
    public async Task<ActionResult<AuthenticationResponse>> RegisterAsync(RegisterRequest registerRequest)
    {
        // validation thru checking the model state, can be validated in other ways, but this is an example

        try
        {
            ValidateModelState();

            //* we're validating in the dto, this should be fine
            var user = new ApplicationUser
            {
                Email = registerRequest.Email,
                PhoneNumber = registerRequest.Phone,
                UserName = registerRequest.Email,
                DisplayName = registerRequest.DisplayName
            };

            var result = await _userManager.CreateAsync(user, registerRequest.Password);
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
    /// <param name="loginRequest">user email and password <see cref="LoginRequest"/></param>
    /// <returns>User with token and refresh token. <see cref="AuthenticationResponse"/></returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(LoginRequest loginRequest)
    {

        try
        {
            ValidateModelState();

            // this is very convenient compared to pure sql albeit less control.
            var result = await _signInManager.PasswordSignInAsync(
                loginRequest.Email,
                loginRequest.Password,
                isPersistent: false,
                lockoutOnFailure: false
                );

            if (!result.Succeeded)
                throw new Exception("Invalid email and password");

            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
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


    /// <summary>
    /// Refresh the token as per the client request, remember, the client is the FrontEnd software not the user.
    /// </summary>
    /// <param name="request"> a refresh token request <see cref="RefreshTokenRequest"/> </param>
    /// <returns>newly issued jwt w/ refresh token <see cref="AuthenticationResponse"/> </returns>

    [HttpPost("refresh-token")]
    public async Task<IActionResult> GenerateNewAccessToken(RefreshTokenRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest("Invalid client request");

        // Extract the access token and refresh token
        var jwtToken = request.Token;
        var refreshToken = request.RefreshToken;

        // Validate the JWT and get the principal (user info) from it
        var principal = _jwtService.GetPrincipleFromJwtToken(jwtToken);
        if (principal is null)
            return BadRequest("Invalid JWT access token");

        // Get the email (or other claim) from the JWT
        var userEmail = principal.FindFirstValue(ClaimTypes.Email);
        if (userEmail is null)
            return BadRequest("Invalid JWT token claims");

        // Find the user in the database
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpirationDateTime <= _dateTimeProvider.UtcNow)
        {
            return Unauthorized("Invalid or expired refresh token");
        }

        // Generate a new access token and refresh token
        var newJwtToken = _jwtService.CreateJwtToken(user);

        // Update the user's refresh token and expiration time
        user.RefreshToken = newJwtToken.RefreshToken;  // New refresh token
        user.RefreshTokenExpirationDateTime = newJwtToken.RefreshTokenExpirationDateTime; // New expiration
        await _userManager.UpdateAsync(user);

        // Return the new tokens
        return Ok(newJwtToken);
    }

    /// <summary>
    /// Throws an exception to test the Custom Problem details
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpGet("throw~!")]

    public Task<IActionResult> TestProblemDetails(CancellationToken cancellationToken)
    {
        throw new Exception("Something intentionally went Wrong ~!");
    }

}
