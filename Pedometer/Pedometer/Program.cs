using System;
using System.Threading;
using Microsoft.SPOT;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Math = System.Math;

namespace Pedometer
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            Debug.Print("Program Started");

            new Thread(WatchForSteps).Start();
        }

        void WatchForSteps()
        {
            const int WINDOW = 100;
            var queue = new double[WINDOW];
            double total = 0;
            var currentIdx = 0;
            int x, y, z;
            double mag;

            const double MAG_THRESH = 300;
            var isAbove = false;
            var stepCount = 0;

            while (true)
            {
                accelG248.GetXYZ(out x, out y, out z);
                mag = Math.Sqrt(x*x + y*y + z*y);
                if(Double.IsNaN(mag)) continue;
                
                // High pass filter
                total -= queue[currentIdx];
                queue[currentIdx] = mag;
                total += mag;
                currentIdx++;
                if (currentIdx == WINDOW)
                {
                    currentIdx = 0;
                }
                //var mag1 = mag;
                mag = total/WINDOW;
                //Debug.Print(x + " | " + y + " | " + z + " | " + mag1 + " | " + mag);

                // Zero crossing
                if (isAbove)
                {
                    if (mag >= MAG_THRESH) continue;

                    isAbove = false;
                    stepCount++;
                    var outIdx = stepCount/7.15;
                    for (var j = 0; j <= 6; j++)
                    {
                        ledStrip.SetLED(j, j < outIdx);
                    }
                }
                else
                {
                    isAbove = mag > MAG_THRESH;
                }
            }
        }
    }
}
