namespace Interpreter_Równań {

    public class Number: Element {
        public double value { get; }

        public Number(double _quantity) {
            elementType = ElementType.Number;
            value = _quantity;
        }

        public override string ToString() {
            return value.ToString();
        }
    }

}
