// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using RSA rsa = RSA.Create(2048); // 2048-bit key

// Export keys
string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

Console.WriteLine("Public Key:\n" + publicKey);
Console.WriteLine("Private Key:\n" + privateKey);

byte[] message = "Hello, secure world!"u8.ToArray();

// Encrypt with public key
byte[] cipher = rsa.Encrypt(message, RSAEncryptionPadding.OaepSHA256);

// Decrypt with private key
byte[] decrypted = rsa.Decrypt(cipher, RSAEncryptionPadding.OaepSHA256);

Console.WriteLine(System.Text.Encoding.UTF8.GetString(decrypted));

byte[] data = "Important document"u8.ToArray();

// Sign with private key
byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

// Verify with public key
bool isValid = rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

Console.WriteLine($"Signature valid: {isValid}");

var req = new CertificateRequest(
    "CN=example.com",
    rsa,
    HashAlgorithmName.SHA256,
    RSASignaturePadding.Pkcs1
);

// Valid for 1 year
var selfSignedCert = req.CreateSelfSigned(
    DateTimeOffset.Now,
    DateTimeOffset.Now.AddYears(1)
);

// Save to file
System.IO.File.WriteAllBytes("selfsigned.pfx", selfSignedCert.Export(X509ContentType.Pfx, "mypassword"));

var cert = new X509Certificate2("selfsigned.pfx", "mypassword");
RSA? certprivateKey = cert.GetRSAPrivateKey();
RSA? certpublicKey = cert.GetRSAPublicKey();

byte[] data2 = System.Text.Encoding.UTF8.GetBytes("Important message");
byte[]? signature2 = certprivateKey?.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

// Verify with public key
bool valid = certpublicKey != null && certpublicKey.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
Console.WriteLine("Signature 2 valid: " + valid);