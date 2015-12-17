#pragma once
#include <fstream>
#include <typeinfo>
#include "itkCommand.h"
#include "itkRegularStepGradientDescentOptimizer.h"
#include "itkLevenbergMarquardtOptimizer.h"
#include "itkAmoebaOptimizer.h"

class CommandIterationUpdate :
	public itk::Command
{
public:
	typedef CommandIterationUpdate								Self;
	typedef itk::Command										SuperClass;
	typedef itk::SmartPointer<Self>								Pointer;
	typedef itk::Optimizer::ParametersType						ParametersType;
	typedef itk::RegularStepGradientDescentOptimizer			GradientOptimizerType;
	typedef itk::LevenbergMarquardtOptimizer					LevenbergOptimizerType;
	typedef itk::AmoebaOptimizer								AmoebaOptimizerType;
	typedef const GradientOptimizerType*						GradientOptimizerPointer;
	typedef const LevenbergOptimizerType*						LevenbergOptimizerPointer;
	typedef const AmoebaOptimizerType*							AmoebaOptimizerPointer;
	typedef itk::SingleValuedNonLinearOptimizer::MeasureType	GradientValueType;
	typedef itk::MultipleValuedNonLinearOptimizer::MeasureType	LevenbergValueType;
	typedef itk::SingleValuedNonLinearOptimizer::MeasureType	AmoebaValueType;

	itkNewMacro(Self);

	void Execute(itk::Object *caller, const itk::EventObject & event);
	void Execute(const itk::Object * object, const itk::EventObject & event);

	int GetNumberIterations(){ return iterationCounter; }

	~CommandIterationUpdate();
protected:
	bool failedOpenWriter = false;
	std::ofstream writer;

	bool isLevenberg = false;
	bool isGradient = false;
	bool isAmoeba = false;

	// Secondary method for tracking number of iterations since
	// some optimizer types do not provide the current iteration
	int iterationCounter = 0;

	CommandIterationUpdate();
};

