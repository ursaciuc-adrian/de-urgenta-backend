﻿using System;
using System.Threading.Tasks;
using DeUrgenta.Domain;
using DeUrgenta.Domain.Entities;
using DeUrgenta.Tests.Helpers;
using DeUrgenta.Tests.Helpers.Builders;
using DeUrgenta.User.Api.Commands;
using DeUrgenta.User.Api.Validators;
using FluentAssertions;
using Xunit;

namespace DeUrgenta.User.Api.Tests.Validators
{
    [Collection(TestsConstants.DbCollectionName)]
    public class RejectGroupInviteValidatorShould
    {
        private readonly DeUrgentaContext _dbContext;

        public RejectGroupInviteValidatorShould(DatabaseFixture fixture)
        {
            _dbContext = fixture.Context;
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("my-weird-sub")]
        public async Task Invalidate_request_when_no_user_found_by_sub(string sub)
        {
            // Arrange
            var sut = new RejectGroupInviteValidator(_dbContext);

            // Act
            var isValid = await sut.IsValidAsync(new RejectGroupInvite(sub, Guid.NewGuid()));

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public async Task Invalidate_when_invite_is_for_other_user()
        {
            var sut = new RejectGroupInviteValidator(_dbContext);

            // Arrange
            var userSub = Guid.NewGuid().ToString();
            var user = new UserBuilder().WithSub(userSub).Build();

            var otherUserSub = Guid.NewGuid().ToString();
            var otherUser = new UserBuilder().WithSub(otherUserSub).Build();

            var adminSub = Guid.NewGuid().ToString();
            var admin = new UserBuilder().WithSub(adminSub).Build();

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Users.AddAsync(admin);
            await _dbContext.Users.AddAsync(otherUser);

            var group = new GroupBuilder().WithAdmin(admin).Build();

            await _dbContext.Groups.AddAsync(group);

            var groupInvite = new GroupInvite {InvitationReceiver = user, InvitationSender = admin, Group = group};

            await _dbContext.GroupInvites.AddAsync(groupInvite);
            await _dbContext.SaveChangesAsync();

            // Act
            var isValid = await sut.IsValidAsync(new RejectGroupInvite(userSub, Guid.NewGuid()));

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public async Task Invalidate_when_invite_does_not_exist()
        {
            var sut = new RejectGroupInviteValidator(_dbContext);

            // Arrange
            var userSub = Guid.NewGuid().ToString();
            var user = new UserBuilder().WithSub(userSub).Build();

            var adminSub = Guid.NewGuid().ToString();
            var admin = new UserBuilder().WithSub(adminSub).Build();

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Users.AddAsync(admin);

            var group = new GroupBuilder().WithAdmin(admin).Build();

            await _dbContext.Groups.AddAsync(group);

            await _dbContext.SaveChangesAsync();

            // Act
            var isValid = await sut.IsValidAsync(new RejectGroupInvite(userSub, Guid.NewGuid()));

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public async Task Validate_when_an_invite_exists()
        {
            var sut = new RejectGroupInviteValidator(_dbContext);

            // Arrange
            var userSub = Guid.NewGuid().ToString();
            var user = new UserBuilder().WithSub(userSub).Build();

            var adminSub = Guid.NewGuid().ToString();
            var admin = new UserBuilder().WithSub(adminSub).Build();

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Users.AddAsync(admin);

            var group = new GroupBuilder().WithAdmin(admin).Build();

            var groupInvite = new GroupInvite {InvitationReceiver = user, InvitationSender = admin, Group = group};
            await _dbContext.GroupInvites.AddAsync(groupInvite);
            await _dbContext.Groups.AddAsync(group);

            await _dbContext.SaveChangesAsync();

            // Act
            var isValid = await sut.IsValidAsync(new RejectGroupInvite(userSub, groupInvite.Id));

            // Assert
            isValid.Should().BeTrue();
        }
    }
}