using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;

namespace WebApi.Controllers.ControllersImpl
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {

        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpGet("test")]
        public IActionResult Test(string name = "World")
        {
            var pdf = _documentService.GenerateTestPdf(name);

            return File(
                pdf,
                "application/pdf",
                "Test.pdf");
        }
    }
}
