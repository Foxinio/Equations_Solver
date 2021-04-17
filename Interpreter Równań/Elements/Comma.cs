
namespace Interpreter_Równań {

    public class Comma: Element {

        public const int MAX_COMMAS = 1;

        public Comma() {
            this.elementType = ElementType.Comma;
        }


        public override string ToString() {
            return ",";
        }
    }
}
