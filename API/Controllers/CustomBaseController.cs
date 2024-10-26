﻿// Ignore Spelling: Todos todo

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class CustomBaseController : ControllerBase
{

    public CustomBaseController()
    {
    }



}
