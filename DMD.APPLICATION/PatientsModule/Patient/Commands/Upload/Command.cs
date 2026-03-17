using ClosedXML.Excel;
using DMD.APPLICATION.PatientsModule.Patient.Models;
using DMD.APPLICATION.Responses;
using DMD.DOMAIN.Entities.Patients;
using DMD.DOMAIN.Enums;
using DMD.PERSISTENCE.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Annotations;

namespace DMD.APPLICATION.PatientsModule.Patient.Commands.Upload
{
    [JsonSchema("UploadPatientCommand")]
    public class Command : IRequest<Response>
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
    }

    public class CommandHandler : IRequestHandler<Command, Response>
    {
        private static readonly string[] SupportedHeaders =
        [
            "FirstName",
            "LastName",
            "MiddleName",
            "EmailAddress",
            "BirthDate",
            "ContactNumber",
            "Address",
            "Suffix",
            "Occupation",
            "Religion",
            "BloodType",
            "CivilStatus",
        ];

        private readonly DmdDbContext dbContext;

        public CommandHandler(DmdDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.FileContent.Length == 0)
                {
                    return new BadRequestResponse("No file uploaded.");
                }

                var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
                if (extension != ".xlsx")
                {
                    return new BadRequestResponse("Invalid file type. Only .xlsx files are allowed.");
                }

                using var stream = new MemoryStream(request.FileContent);
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.FirstOrDefault(x => x.Name.Equals("Patients", StringComparison.OrdinalIgnoreCase))
                    ?? workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    return new BadRequestResponse("The uploaded workbook does not contain any worksheets.");
                }

                var firstUsedRow = worksheet.FirstRowUsed();
                if (firstUsedRow == null)
                {
                    return new BadRequestResponse("The uploaded worksheet is empty.");
                }

                var headerMap = BuildHeaderMap(firstUsedRow);
                if (!headerMap.ContainsKey("FirstName") || !headerMap.ContainsKey("LastName"))
                {
                    return new BadRequestResponse("The worksheet must contain FirstName and LastName columns.");
                }

                var result = new PatientUploadResultModel();
                var patientsToCreate = new List<PatientInfo>();
                var today = DateTime.Today;
                var sequence = await dbContext.PatientInfos.CountAsync(x => x.CreatedAt.Date == today, cancellationToken);

                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    if (IsEmptyRow(row, headerMap))
                    {
                        continue;
                    }

                    result.TotalRows++;

                    var firstName = GetCellString(row, headerMap, "FirstName");
                    var lastName = GetCellString(row, headerMap, "LastName");

                    if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Row {row.RowNumber()}: FirstName and LastName are required.");
                        continue;
                    }

                    if (!TryParseBirthDate(row, headerMap, out var birthDate, out var birthDateError))
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Row {row.RowNumber()}: {birthDateError}");
                        continue;
                    }

                    if (!TryParseEnum(GetCellString(row, headerMap, "Suffix"), Suffix.None, out Suffix suffix))
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Row {row.RowNumber()}: Invalid Suffix value.");
                        continue;
                    }

                    if (!TryParseEnum(GetCellString(row, headerMap, "CivilStatus"), CivilStatus.None, out CivilStatus civilStatus))
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Row {row.RowNumber()}: Invalid CivilStatus value.");
                        continue;
                    }

                    if (!TryParseEnum(GetCellString(row, headerMap, "BloodType"), BloodTypes.A_Positive, out BloodTypes bloodType))
                    {
                        result.SkippedCount++;
                        result.Errors.Add($"Row {row.RowNumber()}: Invalid BloodType value.");
                        continue;
                    }

                    sequence++;
                    patientsToCreate.Add(new PatientInfo
                    {
                        PatientNumber = $"DMD-{today:yyyyMMdd}-{sequence:D4}",
                        FirstName = firstName,
                        LastName = lastName,
                        MiddleName = GetCellString(row, headerMap, "MiddleName"),
                        EmailAddress = GetCellString(row, headerMap, "EmailAddress"),
                        BirthDate = birthDate,
                        ContactNumber = GetCellString(row, headerMap, "ContactNumber"),
                        Address = GetCellString(row, headerMap, "Address"),
                        Suffix = suffix,
                        Occupation = GetCellString(row, headerMap, "Occupation"),
                        Religion = GetCellString(row, headerMap, "Religion"),
                        BloodType = bloodType,
                        CivilStatus = civilStatus,
                        ProfilePicture = string.Empty
                    });
                }

                if (patientsToCreate.Count > 0)
                {
                    dbContext.PatientInfos.AddRange(patientsToCreate);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                result.ImportedCount = patientsToCreate.Count;
                return new SuccessResponse<PatientUploadResultModel>(result);
            }
            catch (Exception error)
            {
                return new BadRequestResponse(error.GetBaseException().Message);
            }
            finally
            {
                await dbContext.DisposeAsync();
            }
        }

        private static Dictionary<string, int> BuildHeaderMap(IXLRow headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var cell in headerRow.CellsUsed())
            {
                var headerName = cell.GetString().Trim();
                if (!string.IsNullOrWhiteSpace(headerName) && SupportedHeaders.Contains(headerName, StringComparer.OrdinalIgnoreCase))
                {
                    map[headerName] = cell.Address.ColumnNumber;
                }
            }

            return map;
        }

        private static bool IsEmptyRow(IXLRow row, Dictionary<string, int> headerMap)
        {
            return headerMap.Values.All(columnNumber => string.IsNullOrWhiteSpace(row.Cell(columnNumber).GetString()));
        }

        private static string GetCellString(IXLRow row, Dictionary<string, int> headerMap, string headerName)
        {
            if (!headerMap.TryGetValue(headerName, out var columnNumber))
            {
                return string.Empty;
            }

            return row.Cell(columnNumber).GetString().Trim();
        }

        private static bool TryParseBirthDate(
            IXLRow row,
            Dictionary<string, int> headerMap,
            out DateTime? birthDate,
            out string errorMessage)
        {
            birthDate = null;
            errorMessage = string.Empty;

            if (!headerMap.TryGetValue("BirthDate", out var columnNumber))
            {
                return true;
            }

            var cell = row.Cell(columnNumber);
            if (cell.IsEmpty())
            {
                return true;
            }

            if (cell.TryGetValue<DateTime>(out var dateValue))
            {
                birthDate = dateValue.Date;
                return true;
            }

            var textValue = cell.GetString().Trim();
            if (DateTime.TryParse(textValue, out var parsedDate))
            {
                birthDate = parsedDate.Date;
                return true;
            }

            errorMessage = "BirthDate must be a valid Excel date or YYYY-MM-DD string.";
            return false;
        }

        private static bool TryParseEnum<TEnum>(string rawValue, TEnum defaultValue, out TEnum parsedValue)
            where TEnum : struct
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                parsedValue = defaultValue;
                return true;
            }

            return Enum.TryParse(rawValue, true, out parsedValue);
        }
    }
}
