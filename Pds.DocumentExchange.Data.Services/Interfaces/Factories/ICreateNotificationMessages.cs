using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Factories
{
    /// <summary>
    /// i create (document exchange) notification messages.
    /// </summary>
    public interface ICreateNotificationMessages
    {
        /// <summary>
        /// create...
        /// </summary>
        /// <param name="messageHeader">the message header.</param>
        /// <param name="messageBody">the message body.</param>
        /// <param name="useStandardTemplate">use standard template.</param>
        /// <param name="parentBatchId">The parent batch identifier.</param>
        /// <param name="ukprn">The ukprn.</param>
        /// <param name="emailMessageType">Message type which will be mapped to Notify email templates.</param>
        /// <param name="emailPersonalisation">Tokens to be replaced in Notify email templates..</param>
        /// <returns>a (document exchange) notification message.</returns>
        Task<INotificationMessage> Create(
            INotificationMessageHeader messageHeader,
            string messageBody,
            bool useStandardTemplate,
            string parentBatchId,
            int ukprn,
            string emailMessageType = null,
            Dictionary<string, dynamic> emailPersonalisation = null);
    }
}