using System;
using System.Globalization;
using System.Threading;

namespace PCBFrez
{
    public class GCodeConversion
    {
        public struct LastPosition
        {
            public long xPosition { get; set; }
            public long yPosition { get; set; }
            public long zPosition { get; set; }
        }
        private enum NameOfDimension { X, Y, Z}

        public LastPosition lastPosition = new LastPosition();
        private int numberOfDimensionsInGCode;
        private bool xTrue, yTrue, zTrue;
        private string directionX, directionY, directionZ;

        public string GetConvertedSteps(string gCodeStep, int stepToOneMM, string logPatch)
        {
            string[] resultTable = null;
            string[] stepTable = null;
            string result = null;

            try
            {
                stepTable = gCodeStep.Split(new char[] { ' ' });
                resultTable = Convertion(stepTable, stepToOneMM, logPatch);

                result = "S;";
                switch (numberOfDimensionsInGCode)
                {
                    case 1:
                        {
                            if (xTrue)
                                result += "X;" + resultTable[0] + ";" + directionX + ";";
                            if (yTrue)
                                result += "Y;" + resultTable[0] + ";" + directionY + ";";
                            if (zTrue)
                                result += "Z;" + resultTable[0] + ";" + directionZ + ";";
                            numberOfDimensionsInGCode = 0;
                            xTrue = false; yTrue = false; zTrue = false;
                            break;
                        }
                    case 2:
                        {
                            if (xTrue && yTrue)
                                result += "X;" + resultTable[0] + ";" + directionX + ";" + "Y;" + resultTable[1] + ";" + directionY + ";";
                            if (xTrue && zTrue)
                                result += "X;" + resultTable[0] + ";" + directionX + ";" + "Z;" + resultTable[1] + ";" + directionZ + ";";
                            if (yTrue && zTrue)
                                result += "Y;" + resultTable[0] + ";" + directionY + ";" + "Z;" + resultTable[1] + ";" + directionZ + ";";

                            numberOfDimensionsInGCode = 0;
                            xTrue = false; yTrue = false; zTrue = false;
                            break;
                        }
                    case 3:
                        {
                            if (xTrue && yTrue && zTrue)
                                result += "X;" + resultTable[0] + ";" + directionX + ";" + "Y;" + resultTable[1] + ";" + directionY + ";" + "Z;" + resultTable[2] + ";" + directionZ + ";";

                            numberOfDimensionsInGCode = 0;
                            xTrue = false; yTrue = false; zTrue = false;
                            break;
                        }
                    default:
                        {
                            foreach (string x in resultTable)
                            {
                                result += x;
                            }
                            numberOfDimensionsInGCode = 0;
                            xTrue = false; yTrue = false; zTrue = false;
                            break;
                        }
                }
                result += "END;";
                return result;
            }
            catch (Exception ex)
            {
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
                return ex.Message;
            }

        }
        private string[] Convertion(string[] gCodeStep, int stepToOneMM, string logPatch)
        {
            string[] result;
            long xPosition = 0, yPosition = 0, zPosition = 0;

            string currentCulture = Thread.CurrentThread.CurrentCulture.Name;
            CultureInfo ci = new CultureInfo(currentCulture);
            ci.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = ci;

            try
            {
                if (gCodeStep[0] == "G01" || gCodeStep[0] == "G00")
                {
                    directionX = string.Empty; directionY = string.Empty; directionZ = string.Empty;

                    for (int j = 0; j < gCodeStep.Length; j++)
                    {
                        switch (gCodeStep[j][0])
                        {
                            case 'X':
                                {
                                    ConversionOneStep(gCodeStep[j], stepToOneMM, xPosition, NameOfDimension.X);
                                    break;
                                }
                            case 'Y':
                                {
                                    ConversionOneStep(gCodeStep[j], stepToOneMM, xPosition, NameOfDimension.Y);
                                    break;
                                }
                            case 'Z':
                                {
                                    ConversionOneStep(gCodeStep[j], stepToOneMM, xPosition, NameOfDimension.Z);
                                    break;
                                }
                        }
                    }
                    int dimensionsCounter = 0;

                    result = new string[numberOfDimensionsInGCode];
                    if (xTrue)
                    {
                        result[dimensionsCounter] = xPosition.ToString();
                        dimensionsCounter++;
                    }
                    if (yTrue)
                    {
                        result[dimensionsCounter] = yPosition.ToString();
                        dimensionsCounter++;
                    }
                    if (zTrue)
                    {
                        result[dimensionsCounter] = zPosition.ToString();
                    }

                    return result;
                }

                result = new string[1];
                result[0] = "nothing;";
                return result;
            }
            catch (Exception ex)
            {
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
                result = null;
                return result;
            }
        }
        private void ConversionOneStep(string gCodeStep, int stepToOneMM, long position, NameOfDimension dimension)
        {
            double valueOfGCodeStepInNumber = GetValueFromOneGCodeStep(gCodeStep, stepToOneMM);

            switch (dimension)
            {
                case NameOfDimension.X:
                    {
                        position = Convert.ToInt64(valueOfGCodeStepInNumber) - lastPosition.xPosition;
                        directionX = MoveDirection(position);
                        lastPosition.xPosition = Convert.ToInt64(valueOfGCodeStepInNumber);
                        if (position < 0)
                        {
                            position *= -1;
                        }
                        numberOfDimensionsInGCode++;
                        xTrue = true;
                        break;
                    }
                case NameOfDimension.Y:
                    {
                        position = Convert.ToInt64(valueOfGCodeStepInNumber) - lastPosition.yPosition;
                        directionY = MoveDirection(position);
                        lastPosition.yPosition = Convert.ToInt64(valueOfGCodeStepInNumber);
                        if (position < 0)
                        {
                            position *= -1;
                        }
                        numberOfDimensionsInGCode++;
                        yTrue = true;
                        break;
                    }
                case NameOfDimension.Z:
                    {
                        position = Convert.ToInt64(valueOfGCodeStepInNumber) - lastPosition.zPosition;
                        directionZ = MoveDirection(position);
                        lastPosition.zPosition = Convert.ToInt64(valueOfGCodeStepInNumber);
                        if (position < 0)
                        {
                            position *= -1;
                        }
                        numberOfDimensionsInGCode++;
                        zTrue = true;
                        break;
                    }
            }
        }
        private string MoveDirection(long steps)
        {
            if (steps <= 0)
            {
                return "R";
            }
            else
            {
                return "L";
            }
        }
        private double GetValueFromOneGCodeStep(string gCodeStep, int stepToOneMM)
        {
            string valueInGCodeStep = string.Empty;

            for (int i = 1; i < gCodeStep.Length; i++)
            {
                valueInGCodeStep += gCodeStep[i];
            }

            if (valueInGCodeStep[valueInGCodeStep.Length - 1] == '\n')
            {
                string clearValueOfStep = string.Empty;

                for (int i = 0; i < valueInGCodeStep.Length - 2; i++)
                {
                    clearValueOfStep += valueInGCodeStep[i];
                }
                return Convert.ToDouble(clearValueOfStep) * stepToOneMM;
            }
            else
            {
                return Convert.ToDouble(valueInGCodeStep) * stepToOneMM;
            }
        }
    }
}