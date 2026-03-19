using DMD.API.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Security.Claims;

namespace DMD.API.Controllers.Storage
{
    [ApiController]
    [Route("api/dmd/storage")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StorageController : ControllerBase
    {
        private readonly IClinicStorageService clinicStorageService;
        private readonly FileExtensionContentTypeProvider contentTypeProvider = new();

        public StorageController(IClinicStorageService clinicStorageService)
        {
            this.clinicStorageService = clinicStorageService;
        }

        [AllowAnonymous]
        [HttpGet("/storage/{*filePath}")]
        public IActionResult GetPublicStorageFile(string filePath)
        {
            var normalizedFilePath = NormalizePath(filePath);
            if (!TryResolvePath(normalizedFilePath, out var clinicId, out var segments, out var physicalPath))
            {
                return NotFound();
            }

            var isPublicClinicBanner =
                segments.Length >= 5 &&
                string.Equals(segments[2], "clinic-assets", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(segments[3], "banners", StringComparison.OrdinalIgnoreCase);

            if (!isPublicClinicBanner)
            {
                return Unauthorized();
            }

            return CreatePhysicalFileResult(physicalPath);
        }

        [HttpGet("{*filePath}")]
        public IActionResult GetProtectedStorageFile(string filePath)
        {
            var normalizedFilePath = NormalizePath(filePath);
            if (!TryResolvePath(normalizedFilePath, out var clinicId, out var segments, out var physicalPath))
            {
                return NotFound();
            }

            var isPublicClinicBanner =
                segments.Length >= 5 &&
                string.Equals(segments[2], "clinic-assets", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(segments[3], "banners", StringComparison.OrdinalIgnoreCase);

            if (isPublicClinicBanner)
            {
                return CreatePhysicalFileResult(physicalPath);
            }

            var clinicIdClaim = User.FindFirstValue("clinicId");
            if (!int.TryParse(clinicIdClaim, out var authenticatedClinicId) || authenticatedClinicId != clinicId)
            {
                return Forbid();
            }

            return CreatePhysicalFileResult(physicalPath);
        }

        private IActionResult CreatePhysicalFileResult(string physicalPath)
        {
            if (!contentTypeProvider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(physicalPath, contentType);
        }

        private bool TryResolvePath(
            string normalizedFilePath,
            out int clinicId,
            out string[] segments,
            out string physicalPath)
        {
            clinicId = 0;
            segments = Array.Empty<string>();
            physicalPath = string.Empty;

            if (string.IsNullOrWhiteSpace(normalizedFilePath))
            {
                return false;
            }

            segments = normalizedFilePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 5 || !string.Equals(segments[0], "clinics", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!int.TryParse(segments[1], out clinicId))
            {
                return false;
            }

            var combinedPath = Path.Combine(
                clinicStorageService.RootPath,
                normalizedFilePath.Replace('/', Path.DirectorySeparatorChar));
            var fullPath = Path.GetFullPath(combinedPath);
            var storageRoot = Path.GetFullPath(clinicStorageService.RootPath);

            if (!fullPath.StartsWith(storageRoot, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return false;
            }

            physicalPath = fullPath;
            return true;
        }

        private static string NormalizePath(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return string.Empty;
            }

            return filePath
                .Replace('\\', '/')
                .Trim()
                .TrimStart('/');
        }
    }
}
