#include "CenterRigidStd.h"


CenterRigidStd::CenterRigidStd()
{
}


CenterRigidStd::~CenterRigidStd()
{
}

int CenterRigidStd::DoRegistration()
{
	typedef itk::CenteredRigid2DTransform<double>											TransformType;
	typedef itk::MeanSquaresImageToImageMetric<GrayImageType, GrayImageType>				MetricType;
	typedef itk::CenteredTransformInitializer<TransformType, GrayImageType, GrayImageType>	TransformInitializerType;

	TransformType::Pointer				transform = TransformType::New();
	MetricType::Pointer					metric = MetricType::New();
	TransformInitializerType::Pointer	initializer = TransformInitializerType::New();

	registration->SetMetric(metric);
	registration->SetTransform(transform);

	if (regParams.UseCenterForRotation) {

		GrayImageType::SpacingType spacing = grayMovingImage->GetSpacing();
		GrayImageType::PointType origin = grayMovingImage->GetOrigin();
		GrayImageType::RegionType region = grayMovingImage->GetLargestPossibleRegion();
		GrayImageType::SizeType size = region.GetSize();

		TransformType::InputPointType center;
		center[0] = origin[0] + spacing[0] * size[0] / 2.0;
		center[1] = origin[1] + spacing[1] * size[1] / 2.0;
		transform->SetCenter(center);
	}
	else {

		initializer->SetTransform(transform);
		initializer->SetFixedImage(grayFixedImage);
		initializer->SetMovingImage(grayMovingImage);

		initializer->MomentsOn();
		initializer->InitializeTransform();
	}

	TransformType::OutputVectorType initialTranslation;
	initialTranslation[0] = regParams.TranslationX;
	initialTranslation[1] = regParams.TranslationY;
	transform->SetTranslation(initialTranslation);

	transform->SetAngle(regParams.Angle);

	registration->SetInitialTransformParameters(transform->GetParameters());

	OptimizerScalesType optimizerScales(transform->GetNumberOfParameters());

	optimizerScales[0] = rotationScale;
	optimizerScales[1] = translationScale;
	optimizerScales[2] = translationScale;
	optimizerScales[3] = translationScale;
	optimizerScales[4] = translationScale;

	optimizer->SetScales(optimizerScales);

	try
	{
		registration->Update();
	}
	catch (itk::ExceptionObject &err)
	{
		cerr << "ExceptionObject caught!" << endl;
		cerr << err << endl;
		cout << "Exception caught during execution: " << err << endl;
		return 0;
	}

	OptimizerType::ParametersType finalParameters = registration->GetLastTransformParameters();

	const double finalAngle = finalParameters[0];
	const double finalRotationCenterX = finalParameters[1];
	const double finalRotationCenterY = finalParameters[2];
	const double finalTranslationX = finalParameters[3];
	const double finalTranslationY = finalParameters[4];

	const unsigned int numberOfIterations = optimizer->GetCurrentIteration();

	const double bestValue = optimizer->GetValue();
	const double finaleAngleInDegrees = finalAngle*180.0 / vnl_math::pi;

	char delim = ';';

	cout << ResultsLineStarter << finalTranslationX << delim << finalTranslationY << delim << finalAngle << delim << finalRotationCenterX << delim
		<< finalRotationCenterY << delim << numberOfIterations << delim
		<< bestValue << endl;

	cout << "Cropping and aligning input images." << endl;

	TransformType::Pointer finalTransform = TransformType::New();
	finalTransform->SetParameters(finalParameters);
	finalTransform->SetFixedParameters(transform->GetFixedParameters());

	TransformImages(finalTransform, colorMovingImage, colorFixedImage);
	return 1;
}
