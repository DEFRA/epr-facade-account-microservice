// Global using directives
global using Microsoft.VisualStudio.TestTools.UnitTesting;

global using System.Net;
global using System.Security.Claims;
global using System.Text.Json;
global using AutoFixture;
global using AutoFixture.AutoMoq;
global using FacadeAccountCreation.API.Controllers;
global using FacadeAccountCreation.Core.Models.CreateAccount;
global using FacadeAccountCreation.Core.Models.Messaging;
global using FacadeAccountCreation.Core.Services.Messaging;
global using FacadeAccountCreation.UnitTests.TestHelpers;
global using FluentAssertions;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Logging.Abstractions;
global using Microsoft.Extensions.Options;
global using Microsoft.Identity.Web;
global using Moq;
global using Moq.Protected;
global using Notify.Models.Responses;