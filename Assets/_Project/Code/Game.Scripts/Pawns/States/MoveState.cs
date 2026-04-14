namespace Code.Game.Scripts.Pawns.States
{
    public class MoveState : LinearMixerPawnState
    {
        private float speed;

        public float Speed
        {
            get => speed;
            set
            {
                speed = value;
                CheckSpeed();
                if (state != null)
                    state.Parameter = value;
            }
        }

        public override Priority StatePriority => Priority.Low;


        private void CheckSpeed()
        {
            if (Speed <= 0.1f)
            {
                if (StateMachine.CurrentState == this)
                    StateMachine.TrySetDefaultState();
            }
            else
            {
                StateMachine.TrySetState(this);
            }
        }
    }
}