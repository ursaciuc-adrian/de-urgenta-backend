﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeUrgenta.Common.Validation;
using DeUrgenta.Domain;
using DeUrgenta.Group.Api.Options;
using DeUrgenta.Group.Api.Queries;
using DeUrgenta.Group.Api.QueryHandlers;
using DeUrgenta.Tests.Helpers;
using DeUrgenta.Tests.Helpers.Builders;
using NSubstitute;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DeUrgenta.Group.Api.Tests.QueriesHandlers
{
    [Collection(TestsConstants.DbCollectionName)]
    public class GetAdministeredGroupsHandlerShould
    {
        private readonly DeUrgentaContext _dbContext;
        private readonly IOptions<GroupsConfig> _groupsConfig;

        public GetAdministeredGroupsHandlerShould(DatabaseFixture fixture)
        {
            _dbContext = fixture.Context;
            var options = new GroupsConfig {UsersLimit = 35};
            _groupsConfig = Microsoft.Extensions.Options.Options.Create<GroupsConfig>(options);
        }

        [Fact]
        public async Task Return_failed_result_when_validation_fails()
        {
            // Arrange
            var validator = Substitute.For<IValidateRequest<GetAdministeredGroups>>();
            validator
                .IsValidAsync(Arg.Any<GetAdministeredGroups>())
                .Returns(Task.FromResult(false));

            var sut = new GetAdministeredGroupsHandler(validator, _dbContext, _groupsConfig);

            // Act
            var result = await sut.Handle(new GetAdministeredGroups("a-sub"), CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task Return_max_number_of_users()
        {
            // Arrange
            var validator = Substitute.For<IValidateRequest<GetAdministeredGroups>>();
            validator
                .IsValidAsync(Arg.Any<GetAdministeredGroups>())
                .Returns(Task.FromResult(true));

            var userId = Guid.NewGuid();
            var userSub = TestDataProviders.RandomString();
            var user = new UserBuilder().WithId(userId).WithSub(userSub).Build();
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var group = new GroupBuilder().WithAdmin(user).Build();
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();

            var sut = new GetAdministeredGroupsHandler(validator, _dbContext, _groupsConfig);

            // Act
            var result = await sut.Handle(new GetAdministeredGroups(userSub), CancellationToken.None);

            // Assert
            result.Value.Where(g => g.Id.Equals(group.Id)).First().MaxNumberOfMembers.Should().Be(35);
        }
    }
}