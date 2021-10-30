# MyCouch.CloudantIAM #

MyCouch extended client for Cloudant IAM authentication method, mainly used for Cloudant Transaction Engine (TxE) https://cloud.ibm.com/docs/Cloudant?topic=Cloudant-overview-te

**Package** - [MyCouch.CloudantIAM](http://nuget.org/packages/mycouch.cloudantiam) | **Platforms** - .NET 4.5, netstandard2.0

Example using IAM authentication
```csharp
var client = new MyCouchCloudant("https://IAMApiKey@xxxxxxx-bluemix.cloudant.com", "mydb");
```

Example using legacy credentials, for standard offerings
```csharp
var client = new MyCouchCloudant("https://user:password@xxxxxxx-bluemix.cloudant.com", "mydb");
```

Full documentation on how to use the library at https://github.com/danielwertheim/mycouch/wiki/documentation 

[![Nuget](https://img.shields.io/nuget/v/mycouch.cloudantiam.svg)](https://www.nuget.org/packages/MyCouch.CloudantIAM/)
