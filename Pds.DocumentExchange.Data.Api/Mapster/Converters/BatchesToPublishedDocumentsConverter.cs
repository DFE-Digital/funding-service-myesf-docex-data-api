using Pds.DocumentExchange.Data.Api.Models.SupportTools;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Pds.DocumentExchange.Data.Api.Mapster.Converters
{
    /// <summary>
    /// The batches to published documents converter.
    /// </summary>
    public class BatchesToPublishedDocumentsConverter
    {
        public IEnumerable<PublishedDocument> Convert(IEnumerable<BatchMetadata> source)
        {
            if (source == null)
            {
                return Enumerable.Empty<PublishedDocument>();
            }

            return source.SelectMany(batch => batch.Files).Select(file => new PublishedDocument
            {
                FileName = file.FileName,
                FileType = file.ProductIdentifier,
                Year = file.Metadata.GetValueOrDefault("Year", string.Empty),
                ToUKPRN = file.ToUkprn,
                Version = file.Version,
                VirusScanSuccessful = file.VirusScanSuccessful,
                EmailPrepared = file?.History?.Any(h => h.Action == FileAction.EmailSent) == true
            });
        }
    }
}