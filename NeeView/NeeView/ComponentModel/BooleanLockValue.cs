namespace NeeView.ComponentModel
{
    public class BooleanLockValue : LockValue<bool>
    {
        public BooleanLockValue() : this(false)
        {
        }

        public BooleanLockValue(bool value) : base(value)
        {
        }

        public bool IsSet => Value;

        public void Set()
        {
            Value = true;
        }

        public void Reset()
        {
            Value = false;
        }
    }


}
