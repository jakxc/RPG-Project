namespace RPG.Control
{
    public interface IRaycastable 
    {
        CursorType GetCursorType();
        
        /*PlayerController is pass as parameter to allow player to take action on IRaycastables.
        E.g For WeaponPickup, allow Interactor on player to perform action*/
        bool HandleRaycast(PlayerController controller); 
    }
}
