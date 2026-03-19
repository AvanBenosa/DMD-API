namespace DMD.API.Storage
{
    public interface IClinicStorageService
    {
        string RootPath { get; }
        string RequestPath { get; }

        Task<(string FileName, string FilePath)> SaveClinicFileAsync(
            int clinicId,
            IFormFile file,
            CancellationToken cancellationToken,
            params string[] pathSegments);

        void DeleteClinicFileIfOwned(string? filePath, int clinicId, params string[] pathSegments);
    }
}
