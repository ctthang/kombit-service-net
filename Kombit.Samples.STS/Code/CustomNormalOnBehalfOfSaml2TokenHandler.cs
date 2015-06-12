#region

using System;
using System.IdentityModel.Tokens;

#endregion

namespace Kombit.Samples.STS.Code
{
    /// <summary>
    ///     Represent to a a custom handler which will overrider the Saml2SecurityTokenHandler.
    ///     In this samples, it assumes that a security token put on a normal OBO request is a saml2 token
    /// </summary>
    public class CustomNormalOnBehalfOfSaml2TokenHandler : Saml2SecurityTokenHandler
    {
        /// <summary>
        ///     It is to validate confirmation data. It must override the base class because the recipient of
        ///     confirmation data on OBO security token element is not null.
        /// </summary>
        /// <param name="confirmationData"></param>
        protected override void ValidateConfirmationData(Saml2SubjectConfirmationData confirmationData)
        {
            try
            {
                base.ValidateConfirmationData(confirmationData);
            }
            catch (Exception ex)
            {
                if ((!ex.Message.Contains("ID4157")) || (confirmationData.Recipient == null))
                    throw;
            }
        }
    }
}