# Certificates

## STS public certificate (Token Signing Certificate)
- **Certificate**: `saml.adgangsstyring.projekt-stoettesystemerne.cer`
- **Thumbprint**: `01 69 9B 6C 50 39 6A C0 60 C8 AE 9B C3 D4 B3 43 C9 B5 7E B1`
- **Usage**: The STS Token Signing Certificate

## Service Consumer certificate (1)
- **Certificate**: `FOCES_valid.p12`
- **Password**: `Test1234`
- **Thumbprint**: `2A 4D 45 2C 55 CF 56 72 2A 78 5E DF 01 31 C7 15 8F F9 6E D1`
- **Usage**: The client certificate which is used for negotiating a security token from Identify*STS

## Service Consumer Certificate (2)
- **Certificate**: `VOCES_valid.p12`
- **Password**: `Test1234`
- **Thumbprint**: `80 A5 37 37 EA 01 E9 77 94 96 2B 84 9C 0F 7C 8F 60 17 38 9E`
- **Usage**: The client certificate which is used for negotiating a bootstrap token in normal OBO test or used in OBO proxy test

## Non-authorized Service Consumer Certificate
- **Certificate**: `CA95B2F383BEF8144500CD74B88BC42CD3DE936C.p12`
- **Password**: `Test1234`
- **Thumbprint**: `CA 95 B2 F3 83 BE F8 14 45 00 CD 74 B8 8B C4 2C D3 DE 93 6C`
- **Usage**: Use in one test case SendIssueRequestWhichWillBeFailedBecauseOfAuthentication which is a client credential and it is not mapped to any user in Identify.
