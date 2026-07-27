using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pulse.API.Controllers
{
    public record UploadResponse(string Url, string FileName, long SizeBytes);

    [Authorize]
    [ApiController]
    [Route("api/uploads")]
    public class UploadsController(IWebHostEnvironment environment) : ApiControllerBase
    {
        private const long MaxSizeBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf", ".txt", ".doc", ".docx", ".zip", ".fig"
        };

        [HttpPost]
        [RequestSizeLimit(MaxSizeBytes)]
        public async Task<ActionResult<UploadResponse>> Upload(IFormFile file, CancellationToken ct)
        {
            if (file.Length == 0)
            {
                return BadRequest("File is empty.");
            }

            if (file.Length > MaxSizeBytes)
            {
                return BadRequest("File exceeds the 10 MB limit.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest($"File type '{extension}' is not allowed.");
            }

            var webRootPath = environment.WebRootPath;
            if (string.IsNullOrEmpty(webRootPath))
            {
                webRootPath = Path.Combine(environment.ContentRootPath, "wwwroot");
            }

            var uploadsRoot = Path.Combine(webRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsRoot, storedFileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, ct);
            }

            return Ok(new UploadResponse($"/uploads/{storedFileName}", file.FileName, file.Length));
        }
    }
}
