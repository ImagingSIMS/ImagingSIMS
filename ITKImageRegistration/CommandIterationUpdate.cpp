#include "CommandIterationUpdate.h"

CommandIterationUpdate::CommandIterationUpdate()
{

}


CommandIterationUpdate::~CommandIterationUpdate()
{
	if (writer.is_open())
	{
		writer.close();
		std::cout << "RegistrationOutput.csv closed." << std::endl;
	}
}

void CommandIterationUpdate::Execute(itk::Object *object, const itk::EventObject & event)
{
	Execute((const itk::Object *)object, event);
}
void CommandIterationUpdate::Execute(const itk::Object *object, const itk::EventObject & event)
{
	int iteration;
	ParametersType currentPosition;
	GradientValueType gradientValue;
	LevenbergValueType levenbergValue;
	AmoebaValueType	amoebaValue;

	//const char* type = typeid(object).name();
	//const char* typeRS = typeid(itk::RegularStepGradientDescentOptimizer).name();
	//const char* typeLM = typeid(itk::LevenbergMarquardtOptimizer).name();

	GradientOptimizerPointer optimizerGradient;
	LevenbergOptimizerPointer optimizerLevenberg;
	AmoebaOptimizerPointer optimizerAmoeba;

	if (!isLevenberg && !isGradient && !isAmoeba)
	{
		optimizerGradient = dynamic_cast<GradientOptimizerPointer>(object);
		optimizerLevenberg = dynamic_cast<LevenbergOptimizerPointer>(object);
		optimizerAmoeba = dynamic_cast<AmoebaOptimizerPointer>(object);

		if (optimizerGradient)
		{
			isGradient = true;
		}
		else if (optimizerLevenberg)
		{
			isLevenberg = true;
		}
		if (optimizerAmoeba)
		{
			isAmoeba = true;
		}
	}

	if (isGradient)
	{
		GradientOptimizerPointer optimizer =
			dynamic_cast<GradientOptimizerPointer>(object);
		if (!optimizer)
		{
			std::cout << "Optimizer null" << std::endl;
			return;
		}
		if (!itk::IterationEvent().CheckEvent(&event))
		{
			return;
		}

		// Is successful, so update iteration conter
		iterationCounter++;

		iteration = optimizer->GetCurrentIteration();
		currentPosition = optimizer->GetCurrentPosition();
		gradientValue = optimizer->GetValue();
	}
	else if (isLevenberg)
	{
		LevenbergOptimizerPointer optimizer =
			dynamic_cast<LevenbergOptimizerPointer>(object);
		if (!optimizer)
		{
			std::cout << "Optimizer null" << std::endl;
			return;
		}
		if (!itk::IterationEvent().CheckEvent(&event))
		{
			return;
		}

		// Is successful, so update iteration counter
		iterationCounter++;

		iteration = iterationCounter;
		currentPosition = optimizer->GetCachedCurrentPosition();
		levenbergValue = optimizer->GetCachedValue();
	}
	else if (isAmoeba)
	{
		AmoebaOptimizerPointer optimizer =
			dynamic_cast<AmoebaOptimizerPointer>(object);
		if (!optimizer)
		{
			std::cout << "Optimizer null" << std::endl;
			return;
		}
		if (!itk::IterationEvent().CheckEvent(&event))
		{
			return;
		}

		// Is successful, so update iteration counter
		iterationCounter++;

		iteration = iterationCounter;
		currentPosition = optimizer->GetCachedCurrentPosition();
		amoebaValue = optimizer->GetCachedValue();
	}


	if (!writer.is_open() && !failedOpenWriter)
	{
		writer.open("Plugins/RegistrationOutput.csv", std::ios::out);
		if (writer.is_open())
		{
			std::cout << "Output file opened: Plugins\\RegistrationOutput.csv" << std::endl;
		}
		else
		{
			writer.open("RegistrationOutput.csv");

			if (writer.is_open())
			{
				std::cout << "Output file opened: RegistrationOutput.csv" << std::endl;
			}
			else
			{
				std::cout << "Output file failed to open." << std::endl;
				failedOpenWriter = true;
			}
		}
	}

	//Write to console
	std::cout << iteration << " = ";
	if (isLevenberg) std::cout << levenbergValue << " : ";
	else if (isGradient)std::cout << gradientValue << " : ";
	else if (isAmoeba)std::cout << amoebaValue << " : ";
	std::cout << currentPosition << std::endl;

	//Write to file
	int length = currentPosition.GetSize();
	int iteration1 = iteration + 1;
	writer << iteration << ',';
	if (isLevenberg)writer << levenbergValue << ',';
	else if (isGradient)writer << gradientValue << ',';
	else if (isAmoeba)writer << amoebaValue << ",";
	else writer << ',';
	for (int i = 0; i < length; i++)
	{
		writer << currentPosition.GetElement(i) << ',';
	}
	writer << std::endl;
}