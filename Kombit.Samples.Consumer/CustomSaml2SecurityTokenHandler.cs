using System;
using System.IdentityModel.Tokens;
using System.Xml;
namespace Kombit.Samples.Consumer
{
    /// <summary>
    ///     A custom Saml2SecurityTokenHandler which can write a SecurityTokenReference whose SecurityTokenReference:Id is
    ///     different from the KeyIdentifier
    /// </summary>
    public class StrReferenceSaml2SecurityTokenHandler : Saml2SecurityTokenHandler
    {
        /// <summary>
        ///     Writes a SecurityTokenReference element. This handler is only required when the
        ///     <see cref="CustomizeIdStrIssuedSecurityTokenParameters" /> is used.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="securityKeyIdentifierClause"></param>
        public override void WriteKeyIdentifierClause(XmlWriter writer,
            SecurityKeyIdentifierClause securityKeyIdentifierClause)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (securityKeyIdentifierClause == null)
            {
                throw new ArgumentNullException("securityKeyIdentifierClause");
            }


            if (!securityKeyIdentifierClause.Id.StartsWith("_str"))
            {
                base.WriteKeyIdentifierClause(writer, securityKeyIdentifierClause);
                return;
            }

            // Default WCF:

            //  <o:SecurityTokenReference b:TokenType="http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0" u:Id="_f23ef5f3-9efb-40f0-bf38-758d3a9589db" xmlns:b="http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd">
            //      <o:KeyIdentifier ValueType="http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID">_f23ef5f3-9efb-40f0-bf38-758d3a9589db</o:KeyIdentifier>
            //  </o:SecurityTokenReference>

            // This handler produces:

            //  <o:SecurityTokenReference b:TokenType="http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0" u:Id="_str_f23ef5f3-9efb-40f0-bf38-758d3a9589db" xmlns:b="http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd">
            //      <o:KeyIdentifier ValueType="http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID">_f23ef5f3-9efb-40f0-bf38-758d3a9589db</o:KeyIdentifier>
            //  </o:SecurityTokenReference>

            // Note the difference between the two u:Id attributes.

            var samlClause = securityKeyIdentifierClause as Saml2AssertionKeyIdentifierClause;

            // <wsse:SecurityTokenReference>
            writer.WriteStartElement("SecurityTokenReference",
                "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");


            // @wsse11:TokenType
            writer.WriteAttributeString("TokenType",
                "http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd",
                "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0");

            // <wsse:KeyIdentifier>
            writer.WriteStartElement("KeyIdentifier",
                "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

            // @ValueType
            writer.WriteAttributeString("ValueType",
                "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLID");

            // ID is the string content
            writer.WriteString(samlClause.Id.Replace("_str", string.Empty));

            // </wsse:KeyIdentifier>
            writer.WriteEndElement();

            // </wsse:SecurityTokenReference>
            writer.WriteEndElement();
        }
    }
}