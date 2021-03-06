﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;

namespace ESFA.DC.ILR.ValidationService.Providers
{
    public class AzureStorageCompressedFileContentStringProviderService : IMessageStreamProviderService
    {
        private readonly IPreValidationContext _preValidationContext;
        private readonly ILogger _logger;
        private readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;
        private readonly IValidationErrorHandler _validationErrorHandler;
        private readonly IDateTimeProvider _dateTimeProvider;

        public AzureStorageCompressedFileContentStringProviderService(
            IPreValidationContext preValidationContext,
            ILogger logger,
            IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService,
            IValidationErrorHandler validationErrorHandler,
            IDateTimeProvider dateTimeProvider)
        {
            _preValidationContext = preValidationContext;
            _logger = logger;
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
            _validationErrorHandler = validationErrorHandler;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Stream> Provide(CancellationToken cancellationToken)
        {
            var startDateTime = _dateTimeProvider.GetNowUtc();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            MemoryStream outputStream = new MemoryStream();

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await _streamableKeyValuePersistenceService.GetAsync(_preValidationContext.Input, memoryStream, cancellationToken);

                    using (ZipArchive archive = new ZipArchive(memoryStream))
                    {
                        List<ZipArchiveEntry> xmlFiles = archive.Entries.Where(x =>
                            x.Name.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase)).ToList();

                        if (xmlFiles.Count == 0)
                        {
                            _logger.LogWarning(
                                $"Zip file contains no xml file, throwing: jobId: {_preValidationContext.JobId}, file name: {_preValidationContext.Input}");
                            _validationErrorHandler.Handle("ZIP_EMPTY");
                            return null;
                        }

                        if (xmlFiles.Count > 1)
                        {
                            _logger.LogWarning(
                                $"Zip file contains more than one file, throwing: jobId: {_preValidationContext.JobId}, file name: {_preValidationContext.Input}");
                            _validationErrorHandler.Handle("ZIP_TOO_MANY_FILES");
                            return null;
                        }

                        ZipArchiveEntry zippedFile = xmlFiles.First();
                        using (Stream stream = zippedFile.Open())
                        {
                            await stream.CopyToAsync(outputStream, 81920, cancellationToken);
                        }

                        string xmlFileName = $"{ExtractUkrpn(_preValidationContext.Input)}/{zippedFile.Name}";
                        _preValidationContext.Input = xmlFileName;
                        await _streamableKeyValuePersistenceService.SaveAsync(
                            xmlFileName,
                            outputStream,
                            cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Failed to extract the zip file from storage, throwing: jobId: {_preValidationContext.JobId}, file name: {_preValidationContext.Input}",
                    ex);
                // Todo: Missing story
                _validationErrorHandler.Handle("ZIP_CORRUPT");
                return null;
            }

            stopwatch.Stop();

            var processTimes = new StringBuilder();

            processTimes.Append("Start Time: ");
            processTimes.AppendLine(startDateTime.ToString(CultureInfo.InvariantCulture));
            processTimes.Append("Total Time: ");
            processTimes.AppendLine((DateTime.UtcNow - startDateTime).TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

            _logger.LogDebug($"Blob download: {processTimes}");

            return outputStream;
        }

        private string ExtractUkrpn(string fileName)
        {
            if (fileName.Contains("/"))
            {
                return fileName.Split('/')[0];
            }

            return string.Empty;
        }
    }
}