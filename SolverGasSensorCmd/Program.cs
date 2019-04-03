using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverGasSensorCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            PolarSolver solver = new PolarSolver(128, 128, 10000000);

            solver.HeatCapacity = 1005 * 1.293; //J/m³/K (fluid)
            solver.HeatConductivity = 0.0262;//0.0262; //W/m/K (fluid)
            solver.TimeSpan = 0.05; //s
            solver.TubeRadius = 0.002; //m
            solver.FilamentRadius = 4.5E-06; //m
            solver.WolframHeatCapacity = 139 * 19300; //J/m³/K (filament)
            solver.StartFilamentTemp = 20; // K (difference to tube)

            solver.SetStartValues();
            solver.Solve();


            //Output results
            string line = "";

            for (int j = 0; j < solver.ElementCount; j += 8)
            {
                line += j.ToString() + "; ";
            }
            System.Diagnostics.Debug.WriteLine(solver.TimeSteps.ToString() + "; " + line);

            for (int i = 0; i < solver.TimeSteps; i ++)
            {
                line = "";

                for (int j = 0; j < solver.ElementCount ; j += 8)
                {
                    line += solver.GetResult(j, i) + "; ";
                }
                System.Diagnostics.Debug.WriteLine(i.ToString() + "; " + line);

            }
        }
    }
}
