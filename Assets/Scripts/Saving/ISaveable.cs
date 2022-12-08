namespace RPG.Saving
{
    /* This interface is used so that SaveableEntity does not require dependecies such as Mover, 
    Fighter etc when states of these components need to be captured or restored*/
    public interface ISaveable 
    {
        object CaptureState();
        void RestoreState(object state);
    }
}