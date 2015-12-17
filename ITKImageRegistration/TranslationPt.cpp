#include "TranslationPt.h"


TranslationPt::TranslationPt()
{
}


TranslationPt::~TranslationPt()
{
}

int TranslationPt::DoRegistration()
{
	typedef itk::TranslationTransform<double, Dimension>					TransformType;

	TransformType::Pointer transform = TransformType::New();
	transform->SetIdentity();
	registration->SetTransform(transform);

	OptimizerType::ScalesType scales(transform->GetNumberOfParameters());
	scales.Fill(1.0);

	optimizer->SetScales(scales);

	ParametersType initialParameters(transform->GetNumberOfParameters());
	initialParameters[0] = regParams.TranslationX;
	initialParameters[1] = regParams.TranslationY;
	registration->SetInitialTransformParameters(initialParameters);

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

	ParametersType finalParameters = registration->GetLastTransformParameters();

	const double TranslationAlongX = finalParameters[0];
	const double TranslationAlongY = finalParameters[1];
	const double numberIterations = observer->GetNumberIterations();

	itk::MultipleValuedNonLinearOptimizer::MeasureType bestValue = optimizer->GetValue();

	char delim = ';';

	cout << ResultsLineStarter << TranslationAlongX << delim << TranslationAlongY << delim << numberIterations << delim << bestValue << endl;
	cout << "Cropping and aligning input images." << endl;

	TransformType::Pointer finalTransform = TransformType::New();
	finalTransform->SetParameters(finalParameters);
	finalTransform->SetFixedParameters(transform->GetFixedParameters());

	TransformImages(finalTransform, colorMovingImage, colorFixedImage);
	return 1;
}
