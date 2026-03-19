namespace DMD.API.Storage
{
    public class LocalClinicStorageService : IClinicStorageService
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IConfiguration configuration;

        public LocalClinicStorageService(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.configuration = configuration;
        }

        public string RequestPath => NormalizeRequestPath(
            configuration["Storage:RequestPath"] ?? "/storage");

        public string RootPath
        {
            get
            {
                var configuredRootPath = configuration["Storage:RootPath"];
                if (string.IsNullOrWhiteSpace(configuredRootPath))
                {
                    return Path.Combine(webHostEnvironment.ContentRootPath, "storage");
                }

                if (Path.IsPathRooted(configuredRootPath))
                {
                    return configuredRootPath;
                }

                return Path.Combine(webHostEnvironment.ContentRootPath, configuredRootPath);
            }
        }

        public async Task<(string FileName, string FilePath)> SaveClinicFileAsync(
            int clinicId,
            IFormFile file,
            CancellationToken cancellationToken,
            params string[] pathSegments)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var generatedFileName = $"{Guid.NewGuid():N}{extension}";
            var clinicSegments = BuildClinicSegments(clinicId, pathSegments);
            var folderPath = Path.Combine(RootPath, Path.Combine(clinicSegments));
            Directory.CreateDirectory(folderPath);

            var physicalPath = Path.Combine(folderPath, generatedFileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var publicPath = BuildPublicPath(generatedFileName, clinicSegments);
            return (generatedFileName, publicPath);
        }

        public void DeleteClinicFileIfOwned(string? filePath, int clinicId, params string[] pathSegments)
        {
            var normalizedPath = NormalizeIncomingPath(filePath);
            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return;
            }

            var clinicSegments = BuildClinicSegments(clinicId, pathSegments);
            var expectedPrefix = BuildPublicDirectoryPath(clinicSegments);
            if (!normalizedPath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var fileName = Path.GetFileName(normalizedPath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var physicalPath = Path.Combine(RootPath, Path.Combine(clinicSegments), fileName);
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }

        private string[] BuildClinicSegments(int clinicId, params string[] pathSegments)
        {
            var safeSegments = pathSegments
                .Where(segment => !string.IsNullOrWhiteSpace(segment))
                .Select(SanitizeSegment)
                .Where(segment => !string.IsNullOrWhiteSpace(segment))
                .ToList();

            safeSegments.Insert(0, clinicId.ToString());
            safeSegments.Insert(0, "clinics");

            return safeSegments.ToArray();
        }

        private string BuildPublicDirectoryPath(IEnumerable<string> segments)
        {
            return $"{RequestPath}/{string.Join("/", segments)}";
        }

        private string BuildPublicPath(string fileName, IEnumerable<string> segments)
        {
            return $"{BuildPublicDirectoryPath(segments)}/{fileName}";
        }

        private static string SanitizeSegment(string value)
        {
            var invalidCharacters = Path.GetInvalidFileNameChars();
            var sanitized = new string(value
                .Trim()
                .Where(character => !invalidCharacters.Contains(character) && character != '/' && character != '\\')
                .ToArray());

            return string.IsNullOrWhiteSpace(sanitized) ? string.Empty : sanitized;
        }

        private static string NormalizeRequestPath(string value)
        {
            var normalized = value.Replace('\\', '/').Trim();
            if (!normalized.StartsWith('/'))
            {
                normalized = $"/{normalized}";
            }

            return normalized.TrimEnd('/');
        }

        private static string NormalizeIncomingPath(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.AbsolutePath.Replace('\\', '/').Trim();
            }

            return value.Replace('\\', '/').Trim();
        }
    }
}
