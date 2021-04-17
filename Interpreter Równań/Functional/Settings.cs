using System;

namespace Interpreter_Równań {

    public static class Settings {

        public static int lvlOfAproximation = 10;


        public static double Round(double result) {
            if (Math.Abs(Math.Round(result) - result) < Math.Pow(10, -1 * lvlOfAproximation)) {
                result = Math.Round(result);
            }

            return result;
        }
    }
}
