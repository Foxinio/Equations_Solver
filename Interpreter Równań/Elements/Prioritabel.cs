
namespace Interpreter_Równań {
    public abstract class Prioritabel: Element {
        abstract public int priority { get; set; }

        abstract public Number Execute(Number[] arguments);
    }
}
