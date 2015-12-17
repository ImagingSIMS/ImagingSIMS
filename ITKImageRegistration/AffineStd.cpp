#include "AffineStd.h"


AffineStd::AffineStd()
{
}


AffineStd::~AffineStd()
{
}

int AffineStd::DoRegistration()
{
	typedef itk::AffineTransform<double, Dimension>											TransformType;
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

	registration->SetInitialTransformParameters(transform->GetParameters());

	// Calculate matrix coefficients for rotational and scale transforms

	vnl_matrix<double> transformMatrix(2, 2);

	// Set matrix coefficients
	itk::OptimizerParameters<double> setParameters = transform->GetParameters();
	

	OptimizerScalesType optimizerScales(transform->GetNumberOfParameters());

	optimizerScales[0] = scaleScale;
	optimizerScales[1] = rotationScale;
	optimizerScales[2] = translationScale;
	optimizerScales[3] = translationScale;
	optimizerScales[4] = translationScale;
	optimizerScales[5] = translationScale;

	optimizer->SetScales(optimizerScales);
	optimizer->MinimizeOn();

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

	cout << "Registration complete." << endl;

	OptimizerType::ParametersType finalParameters = registration->GetLastTransformParameters();

	//const double finalScale = finalParameters[0];
	//const double finalAngle = finalParameters[1];
	//const double finalRotationCenterX = finalParameters[2];
	//const double finalRotationCenterY = finalParameters[3];
	const double finalRotationCenterX = transform->GetCenter()[0];
	const double finalRotationCenterY = transform->GetCenter()[1];
	const double finalTranslationX = finalParameters[4];
	const double finalTranslationY = finalParameters[5];

	const unsigned int numberOfIterations = optimizer->GetCurrentIteration();

	const double bestValue = optimizer->GetValue();
	//const double finaleAngleInDegrees = finalAngle*180.0 / vnl_math::pi;

	vnl_matrix<double> p(2, 2);
	p[0][0] = (double)finalParameters[0];
	p[0][1] = (double)finalParameters[1];
	p[1][0] = (double)finalParameters[2];
	p[1][1] = (double)finalParameters[3];
	vnl_svd<double> svd(p);
	vnl_matrix<double> r(2, 2);
	r = svd.U() * vnl_transpose(svd.V());
	double finalAngle = vcl_asin(r[1][0]);

	const double finalScale1 = svd.W(0);
	const double finalScale2 = svd.W(1);

	char delim = ';';

	cout << ResultsLineStarter << finalTranslationX << delim << finalTranslationY << delim << finalAngle << delim << finalRotationCenterX << delim
		<< finalRotationCenterY << delim << finalScale1 << delim << finalScale2 << delim << numberOfIterations << delim
		<< bestValue << endl;

	cout << "Cropping and aligning input images." << endl;

	TransformType::Pointer finalTransform = TransformType::New();
	finalTransform->SetParameters(finalParameters);
	finalTransform->SetFixedParameters(transform->GetFixedParameters());

	TransformImages(finalTransform, colorMovingImage, colorFixedImage);

	return 1;
}