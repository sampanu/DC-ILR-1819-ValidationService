﻿using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.ProgType;
using ESFA.DC.ILR.ValidationService.Rules.Utility;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ESFA.DC.ILR.ValidationService.Rules.Tests.LearningDelivery.ProgType
{
    /// <summary>
    /// from version 1.1 validation spread sheet
    /// </summary>
    public class ProgType_02RuleTests
    {
        /// <summary>
        /// New rule with null message handler throws.
        /// </summary>
        [Fact]
        public void NewRuleWithNullMessageHandlerThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new ProgType_02Rule(null));
        }

        /// <summary>
        /// Rule name 1, matches a literal.
        /// </summary>
        [Fact]
        public void RuleName1()
        {
            // arrange
            var sut = NewRule();

            // act/assert
            sut.RuleName.Should().Be("ProgType_02");
        }

        /// <summary>
        /// Rule name 2, matches the constant.
        /// </summary>
        [Fact]
        public void RuleName2()
        {
            // arrange
            var sut = NewRule();

            // act/assert
            sut.RuleName.Should().Be(ProgType_02Rule.Name);
        }

        /// <summary>
        /// Rule name 3 test, account for potential false positives.
        /// </summary>
        [Fact]
        public void RuleName3()
        {
            // arrange
            var sut = NewRule();

            // act/assert
            sut.RuleName.Should().NotBe("SomeOtherRuleName_07");
        }

        /// <summary>
        /// Validate with null learner throws.
        /// </summary>
        [Fact]
        public void ValidateWithNullLearnerThrows()
        {
            // arrange
            var sut = NewRule();

            // act/assert
            Assert.Throws<ArgumentNullException>(() => sut.Validate(null));
        }

        /// <summary>
        /// Condition met with null learning delivery returns true.
        /// </summary>
        [Fact]
        public void ConditionMetWithNullLearningDeliveryReturnsTrue()
        {
            // arrange
            var sut = NewRule();

            // act
            var result = sut.ConditionMet(null);

            // assert
            Assert.True(result);
        }

        /// <summary>
        /// Condition met with learning delivery and null prog type returns false.
        /// </summary>
        [Fact]
        public void ConditionMetWithLearningDeliveryNullProgTypeReturnsTrue()
        {
            // arrange
            var sut = NewRule();
            var mockDelivery = new Mock<ILearningDelivery>();

            // act
            var result = sut.ConditionMet(mockDelivery.Object);

            // assert
            Assert.True(result);
        }

        /// <summary>
        /// Condition met with learning delivery and prog type returns false.
        /// </summary>
        [Fact]
        public void ConditionMetWithLearningDeliveryAndProgTypeReturnsFalse()
        {
            // arrange
            var sut = NewRule();
            var mockDelivery = new Mock<ILearningDelivery>();
            mockDelivery.SetupGet(x => x.ProgTypeNullable).Returns(1);

            // act
            var result = sut.ConditionMet(mockDelivery.Object);

            // assert
            Assert.False(result);
        }

        /// <summary>
        /// Invalid item raises validation message.
        /// </summary>
        /// <param name="aimType">Type of aim.</param>
        /// <param name="aimSeqNumber">The aim seq number.</param>
        /// <param name="progType">Programme type</param>
        [Theory]
        [InlineData(TypeOfAim.AimNotPartOfAProgramme, 2, 1)]
        public void InvalidItemRaisesValidationMessage(int aimType, int aimSeqNumber, int? progType)
        {
            // arrange
            const string LearnRefNumber = "123456789X";

            var mockLearner = new Mock<ILearner>();
            mockLearner
                .SetupGet(x => x.LearnRefNumber)
                .Returns(LearnRefNumber);

            var mockDelivery = new Mock<ILearningDelivery>();
            mockDelivery
                .SetupGet(x => x.AimType)
                .Returns(aimType);
            mockDelivery
                .SetupGet(x => x.AimSeqNumber)
                .Returns(aimSeqNumber);
            mockDelivery
                .SetupGet(x => x.ProgTypeNullable)
                .Returns(progType);

            var deliveries = Collection.Empty<ILearningDelivery>();
            deliveries.Add(mockDelivery.Object);
            mockLearner
                .SetupGet(x => x.LearningDeliveries)
                .Returns(deliveries.AsSafeReadOnlyList());

            var mockHandler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);
            mockHandler.Setup(x => x.Handle(
                Moq.It.Is<string>(y => y == ProgType_02Rule.Name),
                Moq.It.Is<string>(y => y == LearnRefNumber),
                aimSeqNumber,
                Moq.It.IsAny<IEnumerable<IErrorMessageParameter>>()));

            mockHandler
                .Setup(x => x.BuildErrorMessageParameter(
                    Moq.It.Is<string>(y => y == ProgType_02Rule.MessagePropertyName),
                    Moq.It.Is<object>(y => y == mockDelivery.Object)))
                .Returns(new Mock<IErrorMessageParameter>().Object);

            var sut = new ProgType_02Rule(mockHandler.Object);

            // act
            sut.Validate(mockLearner.Object);

            // assert
            mockHandler.VerifyAll();
        }

        /// <summary>
        /// Valid item does not raise a validation message.
        /// </summary>
        /// <param name="aimType">Type of aim.</param>
        /// <param name="aimSeqNumber">The aim seq number.</param>
        /// <param name="progType">Programme type</param>
        [Theory]
        [InlineData(TypeOfAim.ProgrammeAim, 1, 2)] // valid because it's 'out of scope'
        [InlineData(TypeOfAim.ComponentAimInAProgramme, 2, 3)] // valid because it's 'out of scope'
        [InlineData(TypeOfAim.ProgrammeAim, 3, null)] // valid because it's 'out of scope'
        [InlineData(TypeOfAim.ComponentAimInAProgramme, 4, null)] // valid because it's 'out of scope'
        [InlineData(TypeOfAim.AimNotPartOfAProgramme, 5, null)]
        [InlineData(TypeOfAim.CoreAim16To19ExcludingApprenticeships, 6, 1)] // valid because it's 'out of scope'
        [InlineData(TypeOfAim.CoreAim16To19ExcludingApprenticeships, 7, null)] // valid because it's 'out of scope'
        public void ValidItemDoesNotRaiseAValidationMessage(int aimType, int aimSeqNumber, int? progType)
        {
            // arrange
            const string LearnRefNumber = "123456789X";

            var mockLearner = new Mock<ILearner>();
            mockLearner
                .SetupGet(x => x.LearnRefNumber)
                .Returns(LearnRefNumber);

            var mockDelivery = new Mock<ILearningDelivery>();
            mockDelivery
                .SetupGet(x => x.AimType)
                .Returns(aimType);
            mockDelivery
                .SetupGet(x => x.AimSeqNumber)
                .Returns(aimSeqNumber);
            mockDelivery
                .SetupGet(x => x.ProgTypeNullable)
                .Returns(progType);

            var deliveries = Collection.Empty<ILearningDelivery>();
            deliveries.Add(mockDelivery.Object);
            mockLearner
                .SetupGet(x => x.LearningDeliveries)
                .Returns(deliveries.AsSafeReadOnlyList());

            var mockHandler = new Mock<IValidationErrorHandler>(MockBehavior.Strict);

            var sut = new ProgType_02Rule(mockHandler.Object);

            // act
            sut.Validate(mockLearner.Object);

            // assert
            mockHandler.VerifyAll();
        }

        /// <summary>
        /// New rule.
        /// </summary>
        /// <returns>a constructed and mocked up validation rule</returns>
        public ProgType_02Rule NewRule()
        {
            var mock = new Mock<IValidationErrorHandler>();

            return new ProgType_02Rule(mock.Object);
        }
    }
}
