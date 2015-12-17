#include "TranslationMM.h"


TranslationMM::TranslationMM()
{
}


TranslationMM::~TranslationMM()
{
}

int TranslationMM::DoRegistration()
{
	typedef itk::TranslationTransform<double, Dimension>									TransformType;
	typedef itk::MattesMutualInformationImageToImageMetric<GrayImageType, GrayImageType>	MetricType;
	
	MetricType::Pointer			metric = MetricType::New();
	TransformType::Pointer		transform = TransformType::New();

	registration->SetMetric(metric);
	registration->SetTransform(transform);

	metric->SetNumberOfHistogramBins(regParams.NumberBins);
	metric->SetNumberOfSpatialSamples(regParams.NumberSamples);

	ParametersType initialParameters(transform->GetNumberOfParameters());
	initialParameters[0] = regParams.TranslationX;
	initialParameters[1] = regParams.TranslationY;
	registration->SetInitialTransformParameters(initialParameters);

	OptimizerScalesType optimizerScales(transform->GetNumberOfParameters());
	optimizerScales[0] = translationScale;
	optimizerScales[1] = translationScale;

	optimizer->SetScales(optimizerScales);

	//MinimizeOn for multi-modal
	optimizer->MinimizeOn();
	//Set UseAllPixels for multi-modal
	if (regParams.UseAllPixels){ metric->UseAllPixelsOn(); }

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
	const double numberIterations = optimizer->GetCurrentIteration();
	const double bestValue = optimizer->GetValue();

	char delim = ';';

	cout << ResultsLineStarter << TranslationAlongX << delim << TranslationAlongY << delim << numberIterations << delim << bestValue << endl;
	cout << "Cropping and aligning input images." << endl;

	TransformType::Pointer finalTransform = TransformType::New();
	finalTransform->SetParameters(finalParameters);
	finalTransform->SetFixedParameters(transform->GetFixedParameters());

	TransformImages(finalTransform, colorMovingImage, colorFixedImage);
	return 1;
}