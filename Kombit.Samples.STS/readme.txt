# Introduction

Document reference: D.03.07.00011

This is a soap service that simulates processing requests (RST) and sending responses (RSTR) for a WS-Trust call which a service consumer (Anvendersystem) can send.

The service accepts issue requests where the input is a security token request (RST) and will return a response with an security token (RSTR).

# Prerequisites
This document requires that the following prerequisites are satisfied:

- Setting up the .Net-based samples according to the guide “All_guideline_setup sites IIS.docx”
- Logging is done to the folder c:\temp. This folder must exist for logging to work.

# Configuring The Test Stub

## Test Stub Properties
- Signing: It is using X509 signing credential with Sha1 algorithm
- The returned RSTR contains a SAML2.0 Assertion element. That is, the RSTR contains an un-encrypted assertion. Assertion encryption is NOT used.
- This service has 2 endpoints
	- MEX endpoint used to publish metadata
	- Certificate endpoint used for processing RST messages and accepts a certificate credential over the HTTP protocol.

## IIS Web Site
This guideline assumes that the url of the STS Test Stub is:
[https://adgangsstyringeksempler.projekt-stoettesystemerne.dk/STS](https://adgangsstyringeksempler.projekt-stoettesystemerne.dk/STS)


## Detailed Configuration
Some changes to the properties in the configuration file STS\web.config may be required:
- BaseAddress: the production address which deploy sts service
- StsServiceCertificateThumbprint: thumbprint of the certificate used by this STS Test stub as the service certificate for the certificate-endpoint. In this sample, the public part of the token signing certificate is located in the Certifcates folder.
- StsSigningCertificateThumbprint: thumbprint of a certificate which is used to sign the message. In this sample, the public part of the token signing certificate is located in the Certifcates folder.
- SigningAlgorithm: a signing algorithm used to sign assertion.
- HeaderSigningAlgorithm: a signing algorithm used to sign header of soap message
- AValidClientCertificateThumbprint: thumbprint of a certificate which is used to imitate a valid client credential which will be accepted to receive a security token
- MaximumTokenLifetime: maximum token life time of issued token in minutes
- BppValue: a base-64 encoded value of a basic privilege profile (bpp) xml value. A sample of bpp can be found on ..\Resources\bpp.xml
- SoapMessageLogLocation: a folder to store all the received request to this service and its response to client
- serilog:minimum-level: specify the level of logging.  Log files are stored in the Logs\ folder.

# Unit Tests
To run the unit tests, all p12-files in the Certificates folder must be imported to LocalMachine\My. Remember to grant read-access to the private key for the application pool identity.

The unit tests have two main sets of test cases:
- One set tests the WsTrustSecurityTokenService class.
- One set tests the WsTrustServiceConfiguration class.

# Using The STS Test Stub
Browsing the following URL will present a greeting page, that shows how to invoke the service:
[https://adgangsstyringeksempler.projekt-stoettesystemerne.dk/STS](https://adgangsstyringeksempler.projekt-stoettesystemerne.dk/STS)

Sample code which also demonstrates can be found in the class:
`Kombit.Samples.Consumer.Consumer`

The following test case demonstrates how to call the STS and then use the issued token to call a service:
`SendRstAndThenExecuteServiceServiceSuccessfully`

EOF
