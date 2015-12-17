#pragma once
#include "Registration.h"

#include "itkEuclideanDistancePointMetric.h"
#include "itkPointSetToPointSetRegistrationMethod.h"

typedef itk::PointSet<float, Dimension>									PointSetType;
typedef PointSetType::PointType											PointType;
typedef PointSetType::PointsContainer									PointsContainer;

class PointRegistration :
	public Registration
{
public:
	//Metric
	typedef itk::EuclideanDistancePointMetric<PointSetType,
												PointSetType>			MetricType;
	typedef MetricType::TransformType									TransformBaseType;
	typedef TransformBaseType::ParametersType							ParametersType;
	typedef TransformBaseType::JacobianType								JacobianType;

	//Optimizer
	typedef itk::LevenbergMarquardtOptimizer							OptimizerType;

	//Registration
	typedef itk::PointSetToPointSetRegistrationMethod<PointSetType,
														PointSetType>	RegistrationType;

	PointRegistration();
	~PointRegistration();

	virtual void	InitializeRegistration(RegistrationParameters registrationParameters);
	virtual int		DoRegistration() { return 0; }

protected:
	PointSetType::Pointer			fixedPointSet;
	PointSetType::Pointer			movingPointSet;
	PointsContainer::Pointer		fixedPointContainer;
	PointsContainer::Pointer		movingPointContainer;

	MetricType::Pointer				metric;
	OptimizerType::Pointer			optimizer;
	RegistrationType::Pointer		registration;

	PointsContainer::Pointer readPointFile(char* fileName);
};

