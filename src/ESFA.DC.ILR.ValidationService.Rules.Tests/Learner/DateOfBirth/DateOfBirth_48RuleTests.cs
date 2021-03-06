﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.Tests.Model;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Constants;
using ESFA.DC.ILR.ValidationService.Rules.Derived.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Learner.DateOfBirth;
using ESFA.DC.ILR.ValidationService.Rules.Query.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Tests.Abstract;
using FluentAssertions;
using Moq;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Rules.Tests.Learner.DateOfBirth
{
    public class DateOfBirth_48RuleTests : AbstractRuleTests<DateOfBirth_48Rule>
    {
        [Fact]
        public void RuleName()
        {
            NewRule().RuleName.Should().Be("DateOfBirth_48");
        }

        [Fact]
        public void LearnerConditionMet_True()
        {
            NewRule().LearnerConditionMet(new DateTime(2001, 8, 10)).Should().BeFalse();
        }

        [Fact]
        public void LearnerConditionMet_False()
        {
            NewRule().LearnerConditionMet(null).Should().BeTrue();
        }

        [Fact]
        public void DD07ConditionMet_True()
        {
            var progType = 23;

            var dd07Mock = new Mock<IDD07>();

            dd07Mock.Setup(dd => dd.IsApprenticeship(progType)).Returns(true);

            NewRule(dd07: dd07Mock.Object).DD07ConditionMet(progType).Should().BeTrue();
        }

        [Fact]
        public void DD07ConditionMet_False()
        {
            var progType = 25;

            var dd07Mock = new Mock<IDD07>();

            dd07Mock.Setup(dd => dd.IsApprenticeship(progType)).Returns(false);

            NewRule(dd07: dd07Mock.Object).DD07ConditionMet(progType).Should().BeFalse();
        }

        [Fact]
        public void DD07ConditionMet_False_Null()
        {
            var dd07Mock = new Mock<IDD07>();

            dd07Mock.Setup(dd => dd.IsApprenticeship(null)).Returns(false);

            NewRule(dd07: dd07Mock.Object).DD07ConditionMet(null).Should().BeFalse();
        }

        [Fact]
        public void DD04ConditionMet_True()
        {
            NewRule().DD04ConditionMet(new DateTime(2016, 9, 1), new DateTime(2016, 9, 2)).Should().BeTrue();
        }

        [Fact]
        public void DD04ConditionMet_False()
        {
            NewRule().DD04ConditionMet(new DateTime(2017, 9, 1), new DateTime(2016, 10, 1)).Should().BeFalse();
        }

        [Fact]
        public void DD04ConditionMet_Null()
        {
            NewRule().DD04ConditionMet(null, new DateTime(2016, 6, 30)).Should().BeFalse();
        }

        [Fact]
        public void ConditionMet_True()
        {
            int? progType = 23;
            var dd07Mock = new Mock<IDD07>();

            dd07Mock.Setup(dd => dd.IsApprenticeship(progType)).Returns(true);

            NewRule(dd07: dd07Mock.Object).ConditionMet(progType, new DateTime(2016, 9, 1), new DateTime(2016, 9, 2)).Should().BeTrue();
        }

        [Fact]
        public void ConditionMet_False()
        {
            int? progType = 25;
            NewRule().ConditionMet(progType, new DateTime(2016, 7, 1), new DateTime(2016, 9, 2)).Should().BeFalse();
        }

        [Fact]
        public void ConditionMet_Null()
        {
            NewRule().ConditionMet(null, null, new DateTime(2016, 9, 2)).Should().BeFalse();
        }

        [Fact]
        public void Validate_Error()
        {
            DateTime dateOfBirth = new DateTime(2000, 9, 2);
            DateTime dd04Date = new DateTime(2016, 9, 1);
            DateTime lastFridayOfJune = new DateTime(2017, 9, 2);

            ILearner learner = new TestLearner()
            {
                DateOfBirthNullable = dateOfBirth,
                LearningDeliveries = new TestLearningDelivery[]
                {
                    new TestLearningDelivery()
                    {
                        ProgTypeNullable = 23,
                        AimType = 1,
                        FworkCodeNullable = 2,
                        PwayCodeNullable = 1,
                        LearnStartDate = new DateTime(2016, 8, 1)
                    }
                }
            };

            var dd07Mock = new Mock<IDD07>();
            var dd04Mock = new Mock<IDD04>();
            var datetimeQueryServiceMock = new Mock<IDateTimeQueryService>();
            var academicYearQueryServiceMock = new Mock<IAcademicYearQueryService>();

            dd07Mock.Setup(dd => dd.IsApprenticeship(23)).Returns(true);
            dd04Mock.Setup(dd => dd.Derive(learner.LearningDeliveries, learner.LearningDeliveries.FirstOrDefault())).Returns(dd04Date);
            datetimeQueryServiceMock.Setup(dd => dd.DateAddYears(dateOfBirth, 16)).Returns(dd04Date);
            academicYearQueryServiceMock.Setup(dd => dd.LastFridayInJuneForDateInAcademicYear(dd04Date)).Returns(lastFridayOfJune);

            using (var validateionErrorHandlerMock = BuildValidationErrorHandlerMockForError())
            {
                NewRule(
                    dd04: dd04Mock.Object,
                    dd07: dd07Mock.Object,
                    dateTimeQueryService: datetimeQueryServiceMock.Object,
                    academicYearQueryService: academicYearQueryServiceMock.Object,
                    validationErrorHandler: validateionErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void Validate_NoError()
        {
            DateTime dateOfBirth = new DateTime(2004, 9, 2);
            DateTime dd04Date = new DateTime(2018, 9, 1);
            DateTime lastFridayOfJune = new DateTime(2019, 9, 2);

            ILearner learner = new TestLearner()
            {
                DateOfBirthNullable = dateOfBirth,
                LearningDeliveries = new TestLearningDelivery[]
                {
                    new TestLearningDelivery()
                    {
                        ProgTypeNullable = 25,
                        AimType = 1,
                        FworkCodeNullable = 2,
                        PwayCodeNullable = 1,
                        LearnStartDate = new DateTime(2018, 8, 1)
                    }
                }
            };

            var dd07Mock = new Mock<IDD07>();
            var dd04Mock = new Mock<IDD04>();
            var datetimeQueryServiceMock = new Mock<IDateTimeQueryService>();
            var academicYearQueryServiceMock = new Mock<IAcademicYearQueryService>();

            dd07Mock.Setup(dd => dd.IsApprenticeship(23)).Returns(true);
            dd04Mock.Setup(dd => dd.Derive(learner.LearningDeliveries, learner.LearningDeliveries.FirstOrDefault())).Returns(dd04Date);
            datetimeQueryServiceMock.Setup(dd => dd.DateAddYears(dateOfBirth, 16)).Returns(dd04Date);
            academicYearQueryServiceMock.Setup(dd => dd.LastFridayInJuneForDateInAcademicYear(dd04Date)).Returns(lastFridayOfJune);

            using (var validateionErrorHandlerMock = BuildValidationErrorHandlerMockForNoError())
            {
                NewRule(
                    dd04: dd04Mock.Object,
                    dd07: dd07Mock.Object,
                    dateTimeQueryService: datetimeQueryServiceMock.Object,
                    academicYearQueryService: academicYearQueryServiceMock.Object,
                    validationErrorHandler: validateionErrorHandlerMock.Object).Validate(learner);
            }
        }

        [Fact]
        public void BuildErrorMessageParameters()
        {
            var validationErrorHandlerMock = new Mock<IValidationErrorHandler>();

            validationErrorHandlerMock.Setup(veh => veh.BuildErrorMessageParameter(PropertyNameConstants.DateOfBirth, "01/01/2001")).Verifiable();
            validationErrorHandlerMock.Setup(veh => veh.BuildErrorMessageParameter(PropertyNameConstants.LearnStartDate, "01/08/2016")).Verifiable();

            NewRule(validationErrorHandler: validationErrorHandlerMock.Object).BuildErrorMessageParameters(new DateTime(2001, 1, 1), new DateTime(2016, 08, 01));

            validationErrorHandlerMock.Verify();
        }

        private DateOfBirth_48Rule NewRule(
            IDD07 dd07 = null,
            IDD04 dd04 = null,
            IDateTimeQueryService dateTimeQueryService = null,
            IAcademicYearQueryService academicYearQueryService = null,
            IValidationErrorHandler validationErrorHandler = null)
        {
            return new DateOfBirth_48Rule(
                dd07: dd07,
                dd04: dd04,
                dateTimeQueryService: dateTimeQueryService,
                academicYearQueryService: academicYearQueryService,
                validationErrorHandler: validationErrorHandler);
        }
    }
}
