#pragma once
#include "Registration.h"

class IntensityRegistration :
	public Registration
{
public:
	typedef itk::RegularStepGradientDescentOptimizer					OptimizerType;
	typedef itk::LinearInterpolateImageFunction<GrayImageType, double>	InterpolatorType;
	typedef itk::ImageRegistrationMethod<GrayImageType, GrayImageType>	RegistrationType;

	typedef OptimizerType::ScalesType									OptimizerScalesType;
	typedef RegistrationType::ParametersType							ParametersType;

	IntensityRegistration();
	~IntensityRegistration();

	virtual void	InitializeRegistration(RegistrationParameters registrationParams);
	virtual int		DoRegistration() { return 0; }
	//virtual void	FinalizeRegistration();

protected:
	OptimizerType::Pointer			optimizer;
	InterpolatorType::Pointer		interpolator;
	RegistrationType::Pointer		registration;

private:

};

