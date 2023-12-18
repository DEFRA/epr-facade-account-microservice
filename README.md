# facade-account-microservice

This repostory contains the code for the facade of the Account Management domain for Extended Producer Responsibility for Packaging (EPR).

## Prerequisites

* .NET 6.0 SDK.
* Any IDE able to open projects written in c# 10, like Visual Studio or VS Code.
* Access to epr-packaging-common NuGet package repository.

## Setup

In Visual Studio, or similar IDE:

1. Open ~/src/FacadeAccountCreation/FacadeAccountCreation.sln
2. Build the project, if you have permission to access epr-packaging-common NuGet package repository, it will download any missing package.

## Running in development

Valid Company id for Dev 200 response: 00214174

## Running tests

Using either the IDE, or dotnet CLI run `FacadeAccountCreation.UnitTests`.

## Contributing to this project

Please read the [contribution guidelines](/CONTRIBUTING.md) before submitting a pull request.

## Licence

THIS INFORMATION IS LICENSED UNDER THE CONDITIONS OF THE OPEN GOVERNMENT LICENCE found at:

<http://www.nationalarchives.gov.uk/doc/open-government-licence/version/3>

The following attribution statement MUST be cited in your products and applications when using this information.

>Contains public sector information licensed under the Open Government licence v3

### About the licence

The Open Government Licence (OGL) was developed by the Controller of Her Majesty's Stationery Office (HMSO) to enable information providers in the public sector to license the use and re-use of their information under a common open licence.

It is designed to encourage use and re-use of information freely and flexibly, with only a few conditions.
