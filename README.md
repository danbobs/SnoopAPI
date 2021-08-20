# SnoopAPI

## Important

Upon opening the solution, set the Snoop.API.EncryptionService, Snoop.API.APIGateway and Snoop.Background.KeyRotation as startup projects.

## Projects

#### Snoop.API.EncryptionService
* DI is currently set to use an AES encrypter and S3 as a keystore. Initial stub implementations did fake encryption and stored in a local file.
* AWS credentials configured in startup,cs but not published in github (will be present in supplied zuip). In real life would use IAM roles obviously. The credential are only good for writing/reading to a bucket.

####  Snoop.API.APIGateway
* Calls the encryption service with a http client
* Http client is wrapped to make unit testing easier

####  Snoop.Common
* Not much in here but if I had more time I'd pull common services  into here

####  Snoop.Background.KeyRotation
* Hosted background service that periodically calls the encryption service to rotate the  keys
* Configured (in appsettings.json) to have interval of 60 seconds.

#### SnoopAPI.UnitTests
* Some sample unit tests (not exhaustive). Tests the following
	* SymmetricEncrypter class
	* EncryptionServiceController - uses HttpClient. Controller dependencies are mocked out with a WebHostBuilder

## VSCode Rest Client files
* See "RestClientTests" directory
