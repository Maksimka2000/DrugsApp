using DrugsApp.Features.Common.Limits;
using ErrorOr;
using FluentAssertions;

namespace DrugsApp.UnitTests.Common;

public sealed class UserRecordLimitsTests
{
    [Fact]
    public void Limits_MatchTheAgreedCaps()
    {
        UserRecordLimits.MAX_PROFILE_RECORDS_PER_TYPE.Should().Be(50);
        UserRecordLimits.MAX_REGULAR_DRUGS.Should().Be(40);
        UserRecordLimits.MAX_REGULAR_SUPPLEMENTS.Should().Be(60);
        UserRecordLimits.MAX_REGULAR_FOODS.Should().Be(100);
        UserRecordLimits.MAX_MEAL_TIMES.Should().Be(20);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(49)]
    public void EnsureBelowLimit_WhenBelowCap_Succeeds(int currentCount)
    {
        ErrorOr<Success> result = UserRecordLimits.EnsureBelowLimit(currentCount, 50, "DrugAllergy");

        result.IsError.Should().BeFalse();
    }

    [Theory]
    [InlineData(50)]
    [InlineData(51)]
    public void EnsureBelowLimit_WhenAtOrAboveCap_ReturnsConflictWithCap(int currentCount)
    {
        ErrorOr<Success> result = UserRecordLimits.EnsureBelowLimit(currentCount, 50, "DrugAllergy");

        result.IsError.Should().BeTrue();
        Error error = result.FirstError;
        error.Type.Should().Be(ErrorType.Conflict);
        error.Code.Should().Be("DrugAllergy.LimitReached");
        error.Metadata!["field"].Should().Be("limit");
        error.Metadata["maxCount"].Should().Be(50);
        error.Metadata["resourceType"].Should().Be("DrugAllergy");
    }
}
