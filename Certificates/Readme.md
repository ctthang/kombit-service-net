# Certificates

## STS public certificate (Token Signing Certificate)
- **Certificate**: `saml.adgangsstyring.projekt-stoettesystemerne.cer`
- **Thumbprint**: `01 69 9B 6C 50 39 6A C0 60 C8 AE 9B C3 D4 B3 43 C9 B5 7E B1`
- **Usage**: The STS Token Signing Certificate

## Service Consumer certificate (1)
- **Certificate**: `FOCES_valid.p12`
- **Password**: `Test1234`
- **Thumbprint**: `15 3E 97 1C 6B AE CC 4E 4E C6 8D 82 69 30 39 01 65 4A 02 A6`
- **Usage**: The client certificate which is used for negotiating a security token from Identify*STS

## Service Consumer Certificate (2)
- **Certificate**: `VOCES_valid.p12`
- **Password**: `Test1234`
- **Thumbprint**: `46 78 23 72 45 FE DC 80 59 D1 13 67 59 55 DF B8 70 D3 6B F4`
- **Usage**: The client certificate which is used for negotiating a bootstrap token in normal OBO test or used in OBO proxy test

## Non-authorized Service Consumer Certificate
- **Certificate**: `CA95B2F383BEF8144500CD74B88BC42CD3DE936C.p12`
- **Password**: `Test1234`
- **Thumbprint**: `CA 95 B2 F3 83 BE F8 14 45 00 CD 74 B8 8B C4 2C D3 DE 93 6C`
- **Usage**: Use in one test case SendIssueRequestWhichWillBeFailedBecauseOfAuthentication which is a client credential and it is not mapped to any user in Identify.
