using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverGasSensorCmd
{
    class PolarSolver
    {
        public double HeatCapacity = 1005 * 1.293; //J/m³/K
        public double HeatConductivity = 0.0262;//0.0262; //W/m/K
        public double TimeSpan = 0.0423298; //s
        public double TubeRadius = 0.002; //m
        public double FilamentRadius = 4.52E-06; //W/m
        public double WolframHeatCapacity = 139 * 19300; //J/m^2/K

        //public double FilamentHeatingPwr = 20.2239 * 0.0262 * 2 * Math.PI / Math.Log(0.002 / 0.000160900763696387); //W/m
        public double StartFilamentTemp = 20.2239; // K (difference)

        public double IterationAbort = 1E-11;

        public int ElementCount;
        public int TimeSteps;
        public int IterationSteps;

        private double[,] Elements;

        public PolarSolver(int ElementCount, int TimeSteps, int IterationSteps)
        {
            this.ElementCount = ElementCount;
            this.TimeSteps = TimeSteps;
            this.IterationSteps = IterationSteps;

            Elements = new double[ElementCount, TimeSteps];

            SetStartValues();
        }

        public void SetStartValues()
        {
            double R;
            double h = (TubeRadius - FilamentRadius) / (ElementCount - 1);

            for (int i = 0; i < ElementCount; i++)
            {
                R = Math.Log(FilamentRadius) + i * Math.Log(TubeRadius - FilamentRadius) / ElementCount;
                Elements[i, 0] = StartFilamentTemp * (ElementCount - i - 1) / (ElementCount - 1);
            }
        }

        public void Solve()
        {
            double R;
            double h = Math.Log(TubeRadius/FilamentRadius) / (ElementCount - 1);
            double h2 = h * h;
            double a = HeatConductivity / HeatCapacity;
            double deltar = FilamentRadius * (Math.Exp(h) - 1);
            double FilamentHeatCapacity = WolframHeatCapacity * (FilamentRadius * FilamentRadius) * Math.PI; //J/m/K
            double k = HeatConductivity * (FilamentRadius * 2 * Math.PI) / FilamentHeatCapacity / deltar;
            double dt = TimeSpan / TimeSteps;
            double[] last1Value = new double[ElementCount];
            double[] last2Value = new double[ElementCount];
            double maxErr = 0;
            double dErr;
            int i = 0;
            int j = 0;
            double LogFilamentRadius = Math.Log(FilamentRadius);
            double LogTubeFilRadius = Math.Log(TubeRadius / FilamentRadius);
            double Exp2R;

            for (int t = 1; t < TimeSteps; t++)
            {
                for (i = 1; i < ElementCount - 1; i++)
                {
                    if (t < 2)
                        Elements[i, t] = Elements[i, t - 1];
                    else if (t < 3)
                        Elements[i, t] = 2 * Elements[i, t - 1] - Elements[i, t - 2];
                    else
                        Elements[i, t] = 3 * Elements[i, t - 1] - 3 * Elements[i, t - 2] + Elements[i, t - 3];
                }

                for (j = 0; j < IterationSteps; j++)
                {
                    maxErr = 0;
                    Elements[0, t] = (Elements[0, t - 1] + Elements[1, t] * k * dt) / (k * dt + 1);

                    for (i = 1; i < ElementCount - 1; i++)
                    {
                        R = LogFilamentRadius + i * LogTubeFilRadius / (ElementCount - 1);

                        Exp2R = Math.Exp(2 * R);

                        Elements[i, t] = (a * h * dt * Math.Exp(3 * R) * (Elements[i + 1, t] - Elements[i - 1, t]) +
                                         2 * a * dt * (Elements[i - 1, t] + Elements[i + 1, t]) +
                                         2 * h2 * Exp2R * Elements[i, t - 1]) / (4 * a * dt + 2 * h2 * Exp2R);

                        if (j > 1)
                        {
                            dErr = Math.Abs(last2Value[i] - 2 * last1Value[i] + Elements[i, t]);

                            if (dErr > maxErr)
                                maxErr = dErr;
                        }
                        last2Value[i] = last1Value[i];
                        last1Value[i] = Elements[i, t];
                    }

                    if (maxErr < IterationAbort && j > 1)
                        break;
                }
                //System.Diagnostics.Debug.WriteLine((maxErr).ToString() + "     " + j.ToString() + "   t=" + t.ToString());
                //System.Diagnostics.Debug.Write(".");
                
            }
        }

        public double GetResult(int ElementNumber, int TimeStep)
        {
            return Elements[ElementNumber, TimeStep];
        }
    }
}
