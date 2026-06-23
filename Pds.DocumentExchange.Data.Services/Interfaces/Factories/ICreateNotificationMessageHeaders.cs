using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Interfaces.Factories
{
    /// <summary>
    /// i create document exchange message headers.
    /// </summary>
    public interface ICreateNotificationMessageHeaders
    {
        /// <summary>
        /// create...
        /// </summary>
        /// <param name="subject">the subject.</param>
        /// <param name="from">from.</param>
        /// <param name="to">to.</param>
        /// <param name="name">name.</param>
        /// <returns>a notification message header.</returns>
        Task<INotificationMessageHeader> Create(string subject, string from, string to, string name);

        /// <summary>
        /// create...
        /// </summary>
        /// <param name="subject">the subject.</param>
        /// <param name="from">from.</param>
        /// <param name="to">to.</param>
        /// <param name="names">names.</param>
        /// <returns>a notification message header.</returns>
        Task<INotificationMessageHeader> Create(string subject, string from, IReadOnlyCollection<string> to, IReadOnlyCollection<string> names);
    }
}