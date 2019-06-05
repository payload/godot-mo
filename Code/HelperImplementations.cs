using System.Collections.Generic;

abstract class AbstractDuty : Duty {
    IEnumerator<object> MyEnumerator;

    protected AbstractDuty() {
        MyEnumerator = Enumerator();
    }

    public bool Tick() {
        return MyEnumerator.MoveNext();
    }

    abstract protected IEnumerator<object> Enumerator();
}
