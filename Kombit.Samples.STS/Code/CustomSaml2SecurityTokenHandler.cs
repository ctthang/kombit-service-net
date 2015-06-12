using System;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;

namespace Kombit.Samples.STS.Code
{
    /// <summary>
    ///     A custom saml2 security token handler which will do some custom process when generating saml2 token responsed to
    ///     client
    /// </summary>
    public class CustomSaml2SecurityTokenHandler : Saml2SecurityTokenHandler
    {
        private readonly RequestSecurityToken rst;

        /// <summary>
        ///     Initializes a new instance of the CustomSaml2SecurityTokenHandler class with a RequestSecurityToken
        /// </summary>
        /// <param name="rst"></param>
        public CustomSaml2SecurityTokenHandler(RequestSecurityToken rst)
        {
            this.rst = rst;
        }

        /// <summary>
        ///     Override CreateToken method to decorate the issued token before sending to client
        /// </summary>
        /// <param name="tokenDescriptor"></param>
        /// <returns></returns>
        public override SecurityToken CreateToken(SecurityTokenDescriptor tokenDescriptor)
        {
            var subject = CreateSamlSubject(tokenDescriptor);
            Saml2SubjectConfirmationData subjectConfirmationData = null;

            Saml2SubjectConfirmation subjectConfirmation = null;
            if ((subject.SubjectConfirmations != null) && (subject.SubjectConfirmations.Count > 0))
            {
                subjectConfirmation = subject.SubjectConfirmations[0];
                subjectConfirmationData = subjectConfirmation.SubjectConfirmationData;
            }

            if (subjectConfirmationData == null)
            {
                subjectConfirmationData = new Saml2SubjectConfirmationData();
                if (subjectConfirmation != null)
                    subjectConfirmation.SubjectConfirmationData = subjectConfirmationData;
                else
                {
                    var confirmationMethod = "urn:oasis:names:tc:SAML:2.0:cm:bearer";
                    if (rst.KeyType == Common.WSTrust13.Constants.KeyTypes.Symmetric)
                        confirmationMethod = "urn:oasis:names:tc:SAML:2.0:cm:holder-of-key";
                    subjectConfirmation = new Saml2SubjectConfirmation(new Uri(confirmationMethod),
                        subjectConfirmationData);
                    subject.SubjectConfirmations.Add(subjectConfirmation);
                }
            }

            if (rst.OnBehalfOf != null)
            {
                // on behalf of: subjectConfirmation.NameId = identity of the message sender, e.g. sender's certificate distinguished name
                subjectConfirmation.NameIdentifier = new Saml2NameIdentifier(subject.NameId.Value,
                    new Uri("urn:oasis:names:tc:SAML:2.0:nameidformat:entity"));
                Saml2SecurityToken saml2SecurityToken = rst.OnBehalfOf.GetSecurityToken() as Saml2SecurityToken;
                // Subject.Nameid = nameid of the on behalf of token
                if (saml2SecurityToken != null)
                {
                    var nameId = saml2SecurityToken.Assertion.Subject.NameId;
                    subject.NameId = new Saml2NameIdentifier(nameId.Value, nameId.Format);
                }
                else
                {
                    var obTokenAsProxy = rst.OnBehalfOf.GetSecurityToken() as X509SecurityToken;
                    if (obTokenAsProxy != null)
                    {
                        subject.NameId = new Saml2NameIdentifier(Constants.GetSubjectNameId(obTokenAsProxy.Certificate),
                            new Uri("urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName"));
                    }
                }
            }

            //Make sure that the recipient on Subject confirmation data is appliesTo of RST
            subjectConfirmationData.Recipient = rst.AppliesTo.Uri;

            //Make sure that the NotOnOrAfter on Subject confirmation data is set
            if ((rst.Lifetime != null) && (rst.Lifetime.Expires != null))
                subjectConfirmationData.NotOnOrAfter = rst.Lifetime.Expires;
            else
                subjectConfirmationData.NotOnOrAfter = DateTime.Now.Add(new TimeSpan(0,
                    Constants.MaximumTokenLifetime, 0));
            //subjectConfirmationData.KeyIdentifiers.Add(new SecurityKeyIdentifier());

            //Initialize a new assertion with is actually similar to assertion created by base class. 
            //One important purpose is to set NameFormat of all of its  Attributes to "urn:oasis:names:tc:SAML:2.0:attrname-format:basic".
            var assertion = new Saml2Assertion(CreateIssuerNameIdentifier(tokenDescriptor))
            {
                Subject = subject,
                SigningCredentials = GetSigningCredentials(tokenDescriptor),
                Conditions =
                    CreateConditions(tokenDescriptor.Lifetime, tokenDescriptor.AppliesToAddress, tokenDescriptor),
                Advice = CreateAdvice(tokenDescriptor)
            };
            var enumerable = CreateStatements(tokenDescriptor);
            foreach (var statement in enumerable)
            {
                var attributeStatement = statement as Saml2AttributeStatement;
                if (attributeStatement != null)
                {
                    foreach (var saml2Attribute in attributeStatement.Attributes)
                    {
                        saml2Attribute.NameFormat = new Uri("urn:oasis:names:tc:SAML:2.0:attrname-format:basic",
                            UriKind.RelativeOrAbsolute);
                    }
                    assertion.Statements.Add(attributeStatement);
                }
                else
                {
                    assertion.Statements.Add(statement);
                }
            }
            assertion.Issuer.Format = new Uri("urn:oasis:names:tc:SAML:2.0:nameidformat:entity");
            return new Saml2SecurityToken(assertion);
        }

        /// <summary>
        ///     Override CreateSamlSubject method to set format of subject's nameId to
        ///     "urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName"
        /// </summary>
        /// <param name="tokenDescriptor"></param>
        /// <returns></returns>
        protected override Saml2Subject CreateSamlSubject(SecurityTokenDescriptor tokenDescriptor)
        {
            var result = base.CreateSamlSubject(tokenDescriptor);
            if (result.NameId != null)
                result.NameId.Format = new Uri("urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName",
                    UriKind.RelativeOrAbsolute);
            return result;
        }
    }
}