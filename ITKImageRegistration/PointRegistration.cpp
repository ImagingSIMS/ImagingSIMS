#include "PointRegistration.h"


PointRegistration::PointRegistration()
{
}


PointRegistration::~PointRegistration()
{
}

void PointRegistration::InitializeRegistration(RegistrationParameters registrationParameters)
{
	Registration::InitializeRegistration(registrationParameters);

	//Read points
	fixedPointContainer = readPointFile(regParams.FixedPointListPath);
	movingPointContainer = readPointFile(regParams.MovingPointListPath);

	if (!fixedPointContainer | !movingPointContainer) return;

	fixedPointSet = PointSetType::New();
	movingPointSet = PointSetType::New();

	fixedPointSet->SetPoints(fixedPointContainer);
	movingPointSet->SetPoints(movingPointContainer);

	cout << "Fixed points: " << fixedPointSet->GetNumberOfPoints() <<
		" - Moving points: " << movingPointSet->GetNumberOfPoints() << endl;

	metric = MetricType::New();

	optimizer = OptimizerType::New();
	optimizer->SetUseCostFunctionGradient(false);
	optimizer->SetNumberOfIterations(regParams.MaximumIterations);
	optimizer->SetValueTolerance(regParams.ValueTolerance);
	optimizer->SetGradientTolerance(regParams.GradientTolerance);
	optimizer->SetEpsilonFunction(regParams.EpsilonFunction);

	optimizer->AddObserver(itk::IterationEvent(), observer);

	registration = RegistrationType::New();
	registration->SetMetric(metric);
	registration->SetOptimizer(optimizer);
	registration->SetFixedPointSet(fixedPointSet);
	registration->SetMovingPointSet(movingPointSet);

	if (hasROIDefined(&regParams, grayFixedImage))
	{
		cout << "Warning: Region of interest has been defined for the fixed image but is not supported in point-based registration methods." << endl;
	}
}

PointsContainer::Pointer PointRegistration::readPointFile(char* fileName)
{
	ifstream file;
	file.open(fileName);

	if (file.fail())
	{
		cerr << "Error opening points file: " << fileName << endl;
		return nullptr;
	}

	unsigned int pointId = 0;

	PointsContainer::Pointer container = PointsContainer::New();

	PointType point;
	file >> point;

	while (!file.eof())
	{
		container->InsertElement(pointId, point);
		file >> point;
		pointId++;
	}

	return container;
}