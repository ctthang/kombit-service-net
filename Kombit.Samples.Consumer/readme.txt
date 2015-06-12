Introduction
	This is an application that simulates how to send issue request and process the response from a WS-Trust service. 
		+ How to generate security token request 
		+ How to sign the security token request
		+ How to send the request to Ws-trust service. In the sample, we test it against our STS test stub.
		+ How to process the response from ws-trust service
		+ How to handle error if there is from ws-trust service

	It also simulates how to use the issued token to send request to another service and process the response from that service.
		+ How to send the request to a web service with issued token
		+ How to process the response from this web service
		
Configuration
	- BaseAddress: the production address where STS and service is deployed.
	- AValidClientCertificateThumbprint: thumprint of a certificate which is used to imitate a valid client credential which will be accepted to receive a security token. The certificate must exist in LocalMachine\My.
	- StsServiceCertificateThumbprint: thumprint of a certificate which is used as service certificate for certificate endpoint. The certificate must exist in LocalMachine\My.
	- StsServiceCertificateDNSIdentity: the dns identity of sts service certificate.
	- StsCertificateEndpoint: the certificate endpoint address.
	- StsMexEndpoint: The mex endpoint address.
	- AValidOnBehalfOfCertificateThumbprint: thumbprint of a certificate which is used on proxy onbehalfof element or used as client certificate to request onbehalfof token.
	
	- ServiceAddress: the service which will accept request authenticated by issued token. In this sample, it is the address of deployed service "service".
	- ServiceServiceCertificateThumbprint: the service certificate of the above service. The certificate must exist in LocalMachine\My.
	- ServiceServiceCertificateDNSIdentity: the dns identity of service endpoint.
	- ExpectedResponseMessage: The expected response message from service service.
	- BppValue: Expected Bpp value in base64 encoded format.
	- SoapMessageLogLocation: a folder to store all the soap message sent and received to sts and service.
	- serilog:minimum-level: specify the level of logging.  Log files are stored in the Logs\ folder.

Running
	- The sample is shipped with two certificates: CertificateAnvendersystem.p12 and CertificateIdp.p12. Import them to LocalMachine\My.
	- The application can be run under IIS or Visual Studio 2013's IIS Express.
	- When running under IISExpress:
		+ Grant access to private keys of the certificates for the user account that is being used to run Visual Studio.
		+ Set multiple start up for the Service and STS projects. Start them (Ctrl + F5) before execute unit tests.