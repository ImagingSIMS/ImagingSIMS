using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSIMS.ImageRegistration
{
    public static class Extensions
    {
        public static TransformTypes ToTransformType(this ImageRegistrationTypes regType)
        {
            int value = (int)regType;
            TransformTypes transform = (TransformTypes)value;
            return transform;
        }
        public static ImageRegistrationTypes ToRegistrationType(this TransformTypes transformType)
        {
            int value = (int)transformType;
            ImageRegistrationTypes regType = (ImageRegistrationTypes)value;
            return regType;
        }
    }
}
