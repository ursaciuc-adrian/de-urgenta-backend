﻿using System.Threading.Tasks;
using DeUrgenta.Domain;
using DeUrgenta.Events.Api.Queries;
using DeUrgenta.Events.Api.Validators;
using DeUrgenta.Tests.Helpers;
using Shouldly;
using Xunit;

namespace DeUrgenta.Events.Api.Tests.Validators
{
    [Collection(TestsConstants.DbCollectionName)]
    public class GetEventValidatorShould
    {
        private readonly DeUrgentaContext _dbContext;

        public GetEventValidatorShould(DatabaseFixture fixture)
        {
            _dbContext = fixture.Context;
        }

        [Theory]
        [InlineData(null)]
        [InlineData(-1)]
        public async Task ShouldInvalidateWhenInvalidEventTypeId(int? eventTypeId)
        {
            // Arrange
            var sut = new GetEventValidator(_dbContext);

            // Act
            bool isValid = await sut.IsValidAsync(new GetEvent(new Models.EventModelRequest { EventTypeId = eventTypeId }));

            // Assert
            isValid.ShouldBeFalse();
        }

        [Theory]
        [InlineData(1)]
        public async Task ShouldValidateWhenValidEventTypeId(int? eventTypeId)
        {
            // Arrange
            var sut = new GetEventValidator(_dbContext);

            // Act
            bool isValid = await sut.IsValidAsync(new GetEvent(new Models.EventModelRequest { EventTypeId = eventTypeId }));

            // Assert
            isValid.ShouldBeTrue();
        }
    }
}