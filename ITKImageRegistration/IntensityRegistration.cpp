#include "IntensityRegistration.h"


IntensityRegistration::IntensityRegistration()
{
}


IntensityRegistration::~IntensityRegistration()
{
}

void IntensityRegistration::InitializeRegistration(RegistrationParameters registrationParameters)
{
	Registration::InitializeRegistration(registrationParameters);

	optimizer = OptimizerType::New();
	interpolator = InterpolatorType::New();
	registration = RegistrationType::New();

	registration->SetOptimizer(optimizer);
	registration->SetInterpolator(interpolator);

	registration->SetFixedImage(grayFixedImage);
	registration->SetMovingImage(grayMovingImage);

	if (!hasROIDefined(&registrationParameters, grayFixedImage))
	{
		registration->SetFixedImageRegion(grayFixedImage->GetBufferedRegion());
	}
	else
	{
		GrayImageType::RegionType roi;
		GrayImageType::IndexType roiIndex;
		GrayImageType::SizeType roiSize;

		roiIndex[0] = registrationParameters.ROIFixedLeft;
		roiIndex[1] = registrationParameters.ROIFixedTop;
		roi.SetIndex(roiIndex);

		roiSize[0] = registrationParameters.ROIFixedRight - registrationParameters.ROIFixedLeft;
		roiSize[1] = registrationParameters.ROIFixedBottom - registrationParameters.ROIFixedTop;
		roi.SetSize(roiSize);
		
		registration->SetFixedImageRegion(roi);
	}

	optimizer->SetRelaxationFactor(regParams.RelaxationFactor);
	optimizer->SetMaximumStepLength(regParams.MaxStepLength);
	optimizer->SetMinimumStepLength(regParams.MinStepLength);
	optimizer->SetNumberOfIterations(regParams.MaximumIterations);

	optimizer->AddObserver(itk::IterationEvent(), observer);
}
