# Introduction
This is a soap service that simulates service system which is authenticated by a security token issued by a sts. 
 has a simple Ping operation which has no input and will return a configured text message.

# Configuration
- ServiceAddress: The production address where user context service is deployed
- ServiceServiceCertificateThumbprint: thumprint of a certificate which is used as service certificate for service endpoint. The certificate must exist in LocalMachine\My.
- StsSigningCertificateThumbprint: thumprint of a certificate which is issuer of the received security token.
- HeaderSigningAlgorithm: a signing algorithm used to sign header of soap message
- ResponseMessage: The response message of operation Ping to client 
- SoapMessageLogLocation: a folder to store all the received request to this service and its response to client
- serilog:minimum-level: specify the level of logging.  Log files are stored in the Logs\ folder.

# Running
The application can be run under IIS or Visual Studio 2013's IIS Express.

# How to call the service
- Sample code which demonstrates how to call the service can be found in the Kombit.Samples.Consumer project.
