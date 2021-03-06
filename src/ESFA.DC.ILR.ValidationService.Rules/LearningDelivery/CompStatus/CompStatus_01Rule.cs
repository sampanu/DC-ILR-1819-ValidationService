﻿using System.Collections.Generic;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Data.Internal.CompStatus.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Rules.Abstract;
using ESFA.DC.ILR.ValidationService.Rules.Constants;

namespace ESFA.DC.ILR.ValidationService.Rules.LearningDelivery.CompStatus
{
    public class CompStatus_01Rule : AbstractRule, IRule<ILearner>
    {
        private readonly ICompStatusDataService _compStatusInternalDataService;

        public CompStatus_01Rule(ICompStatusDataService compStatusInternalDataService, IValidationErrorHandler validationErrorHandler)
            : base(validationErrorHandler, RuleNameConstants.CompStatus_01)
        {
            _compStatusInternalDataService = compStatusInternalDataService;
        }

        public void Validate(ILearner objectToValidate)
        {
            foreach (var learningDelivery in objectToValidate.LearningDeliveries)
            {
                if (ConditionMet(learningDelivery.CompStatus))
                {
                    HandleValidationError(objectToValidate.LearnRefNumber, learningDelivery.AimSeqNumber, BuildErrorMessageParameters(learningDelivery.CompStatus));
                }
            }
        }

        public bool ConditionMet(int compStatus)
        {
            return !_compStatusInternalDataService.Exists(compStatus);
        }

        public IEnumerable<IErrorMessageParameter> BuildErrorMessageParameters(int compStatus)
        {
            return new[]
            {
                BuildErrorMessageParameter(PropertyNameConstants.CompStatus, compStatus)
            };
        }
    }
}
