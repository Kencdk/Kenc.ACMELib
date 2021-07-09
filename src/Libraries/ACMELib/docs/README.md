# About #
Kenc.Acmelib is a fully featured client for interacting with services supporting the [ACME protocol](https://datatracker.ietf.org/doc/html/rfc8555) for certificate acquisition, such as LetsEncrypt.

# How to use #
```C#
// create an RSA crypto service provider and create a key based off of it.
var rsaCryptoServiceProvider = new RSACryptoServiceProvider(2048);
var rsaKey = RSA.Create(rsaCryptoServiceProvider.ExportParameters(true));

// create a client and initialize it.
var acmeClient = new ACMEClient(new Uri(ACMEEnvironment.StagingV2), rsaKey, new HttpClient());
ACMEDirectory directory = await acmeClient.InitializeAsync();

var account = await acmeClient.RegisterAsync(new[] { "mailto:email@domain.test" });
```

for a full example of ordering, validation and exporting the requested certificate, see the [Examples folder on GitHub](https://github.com/Kencdk/Kenc.ACMELib/tree/main/src/Examples).