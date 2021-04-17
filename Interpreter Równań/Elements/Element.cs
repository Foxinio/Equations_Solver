using System.Collections.Generic;

namespace Interpreter_Równań {

    public enum ElementType {
        Unknown,    // nie obslugiwane
        Number,
        Operation,
        Equality,   // w wiekszosci nie obslugiwane
        Brackets,
        Function,
        Parameter,  // w wiekszosci nie oblsugiwane
        Constant,
        Comma,
        Factorial
    };

    public abstract class Element {

        public ElementType elementType { get; set; }

        public static string EquationToString(List<Element> list) {
            string result = string.Empty;
            foreach (var item in list) {
                result += item.ToString();
            }
            return result;
        }
    }



}
