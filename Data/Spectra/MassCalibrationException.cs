using System;

namespace ImagingSIMS.Data.Spectra
{
    public class MassCalibrationException : Exception
    {
        bool missingSlope;
        bool missingInt;
        bool negativeSlope;

        string reason;

        public bool MissingSlope { get { return missingSlope; } }
        public bool MissingInt { get { return missingInt; } }
        public bool NegativeSlope { get { return negativeSlope; } }
        public string Reason { get { return reason; } }

        public MassCalibrationException(string Message)
            : base(Message)
        {
            reason = "None specified.";
        }
        public MassCalibrationException(string Message, bool MissingSlope, bool MissingIntercept)
            : base(Message)
        {
            missingSlope = MissingSlope;
            missingInt = MissingIntercept;

            if (missingSlope && missingInt)
            {
                reason = "Slope and y-intercept parameters are missing.";
            }
            else
            {
                if (missingSlope)
                {
                    reason = "Slope parameter is missing.";
                }
                else if (missingInt)
                {
                    reason = "Y-intercept parameter is missing.";
                }
            }
        }
        public MassCalibrationException(string Message, bool NegativeSlope)
            : base(Message)
        {
            negativeSlope = NegativeSlope;

            reason = "A negative slope value was entered.";
        }
    }
}
