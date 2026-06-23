namespace Pds.DocumentExchange.Data.Services.DTOs
{
    /// <summary>
    /// Class representing an agency team.
    /// </summary>
    public class AgencyTeam
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the team name, e.g. 'DocumentExchangeAdministratorFundingCentre'.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the team's email address.
        /// </summary>
        public string EmailAddress { get; set; }
    }
}