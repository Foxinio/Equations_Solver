
namespace Interpreter_Równań {

    public class Factorial: Element {         // nie w użyciu


        public Factorial() {
            this.elementType = ElementType.Factorial;
        }


        public Number Execute(Number argument) {
            return new Function(FunctionType.fact, 0).Execute(new Number[] { argument });
        }

    }
}
