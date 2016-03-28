using System;

namespace ElBruno.LightNotifier.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var lightController = new LightController();
            
            Console.WriteLine("Press Enter to turn ON the light");
            lightController.TurnLight(true);
            Console.ReadLine();

            Console.WriteLine("Press Enter to turn OFF the light");
            lightController.TurnLight(false);
            Console.ReadLine();

            Console.WriteLine("Press Enter to turn ON the light");
            lightController.TurnLight(true);
            Console.ReadLine();

            Console.WriteLine("Press Enter to turn OFF the light");
            lightController.TurnLight(false);
            Console.ReadLine();

        }
    }
}
