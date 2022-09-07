namespace NeeView
{
    public class MainViewLockerKey
    {
        private readonly MainViewLockerMediator _mediator;

        public MainViewLockerKey(MainViewLockerMediator mediator)
        {
            _mediator = mediator;
        }

        public void Lock()
        {
            _mediator.Lock(this);
        }

        public void Unlock(double delayMilliseconds)
        {
            _mediator.Unlock(this, delayMilliseconds);
        }
    }
}
