# Introduction
This is a soap service that simulates processing requests and sending responses for a WS-Trust call which a user system (Anvendersystem) can send.
It will accept issue request with input is a security token request and will return a request security token response.
- Signing: It is using X509 signing credential with Sha1 algorithm
- Encrypting: No encryption
- This service has 2 endpoints
-- Mex endpoint: to publish metadata
-- Certificate endpoint: for listening to issue request which will accept a certificate credential over http protocol.

# Configuration
- BaseAddress: the production address which deploy sts service
- StsServiceCertificateThumbprint: thumbprint of a certificate which is used as service certificate for certificate endpoint
- StsSigningCertificateThumbprint: thumbprint of a certificate which is used to sign the message
- SigningAlgorithm: a signing algorithm used to sign assertion
- HeaderSigningAlgorithm: a signing algorithm used to sign header of soap message
- AValidClientCertificateThumbprint: thumbprint of a certificate which is used to imitate a valid client credential which will be accepted to receive a security token
- MaximumTokenLifetime: maximum token life time of issued token in minutes
- BppValue: a base-64 encoded value of a basic privilege profile (bpp) xml value. A sample of bpp can be found on ..\Resources\bpp.xml
- SoapMessageLogLocation: a folder to store all the received request to this service and its response to client
- serilog:minimum-level: specify the level of logging.  Log files are stored in the Logs\ folder.

# Running
The application can be run under IIS or Visual Studio 2013's IIS Express.

# Unittest
To run unittest, the stock certificate (CertificateIdp.p12) found in the Certificates folder must be imported to LocalMachine\My. Remember to grant access to it for the user that runs Visual Studio. Note that when you want to write test to run against a real site hosted under IIS, remember to grant access for the identity app pool account.

Unittest has two main sets of test cases:
- One set tests the WsTrustSecurityTokenService class.
- One set tests the WsTrustServiceConfiguration class.

# How to call the service
Sample code which demonstrates how to call the service can be found in the Kombit.Samples.Consumer project.
