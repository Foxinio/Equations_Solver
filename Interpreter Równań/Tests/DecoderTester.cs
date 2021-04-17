using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter_Równań.Tests {
    class DecoderTester {

        public static bool testEquationDecoder(string input) {
            try {
                InputEquationDecoder equation = new InputEquationDecoder(input);
                List<Element> decodedEquation = equation.DecodeEquation();
                return input == string.Join("",equation.splitedInputEquation) && input == Element.EquationToString(decodedEquation);
            }
            catch (Exception e) {
                MainTester.SaveErrorRaport(e, input);
                return false;
            }
        }

    }
}
