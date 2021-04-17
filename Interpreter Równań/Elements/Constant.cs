using System;
using System.Linq;

namespace Interpreter_Równań {

    public enum ConstantType { e, pi, phi, rand }

    public class Constant: Element {

        ConstantType constant;
        public double power;

        public static string[] array = new string[] { "e", "pi", "phi", "rand" };

        public Constant(ConstantType _constant, int _power) {
            elementType = ElementType.Constant;
            constant = _constant;
            power = _power;
        }


        public static string ConstantTypeToString(ConstantType constant) {
            return array[(int)constant];
        }

        public static ConstantType? StringToConstantType(string input, int startingIndex = 0, int count = -1) {
            if (count < 0) {
                count = input.Length - startingIndex;
            }
            foreach (var item in array) {
                if (input.ToLower().IndexOf(item, startingIndex, count) > -1) {
                    return (ConstantType)(Constant.array.ToList().IndexOf(item));
                }
            }
            return null;
        }


        public Number Execute() {
            switch (constant) {
            case ConstantType.e:
                return new Number(Math.Pow(Math.E, power));
            case ConstantType.pi:
                return new Number(Math.Pow(Math.PI, power));
            case ConstantType.phi:
                return new Number(Math.Pow(1.61803398875, power));
            case ConstantType.rand:
                return new Number(Math.Pow(new Random().NextDouble(), power));
            default:
                return null;
            }
        }


        public override string ToString() {
            return ConstantTypeToString(constant);
        }

    }
}
