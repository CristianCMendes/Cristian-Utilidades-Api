using Microsoft.AspNetCore.Mvc;

namespace Utilidades.Api.Controllers;

// Criado só pra redirecionar para o openapi
public class IndexController : ControllerBase {
    [Route("/")]
    [HttpGet]
    public IActionResult Index() => Redirect("/openapi/v1.json");
};