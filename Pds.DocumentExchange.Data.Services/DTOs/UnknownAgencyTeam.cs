namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Class representing an unknown agency team.
    /// </summary>
    public class UnknownAgencyTeam : AgencyTeam
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownAgencyTeam"/> class.
        /// </summary>
        public UnknownAgencyTeam()
        {
            Identifier = "UnknownAgencyTeam";
            Name = "[Unknown team]";
            EmailAddress = string.Empty;
        }
    }
}