﻿using System.Collections.Generic;
using ESFA.DC.ILR.ValidationService.Interface;
using IAcademicYear = ESFA.DC.ILR.ValidationService.Data.Internal.AcademicYear.Interface.IAcademicYear;

namespace ESFA.DC.ILR.ValidationService.Data.Interface
{
    public interface IInternalDataCache
    {
        IAcademicYear AcademicYear { get; }

        IReadOnlyCollection<int> AimTypes { get; }

        IReadOnlyCollection<int> CompStatuses { get; }

        IReadOnlyCollection<int> EmpOutcomes { get; }

        IReadOnlyCollection<int> FundModels { get; }
    }
}
