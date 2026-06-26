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


        [HttpPost("chicken-agreement")]
        public IActionResult ChickenAgreement(ChickenHandoverModel model)
        {
            /*
            var pdfs = models.Select(m => new ChickenHandoverDocument(m).GeneratePdf());

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                int i = 1;
                foreach (var pdf in pdfs)
                {
                    var entry = zip.CreateEntry($"Abgabevereinbarung_{i++}.pdf");
                    using var s = entry.Open();
                    s.Write(pdf);
                }
            }
            */

//return File(ms.ToArray(), "application/zip", "documents.zip");

            var pdf = new ChickenHandoverDocument(model).GeneratePdf();

            return File(pdf, "application/pdf", "Abgabevereinbarung.pdf");
        }
    }
}
