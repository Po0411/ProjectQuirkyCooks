public interface IInteractable
{
    string GetInteractText();
    void Interact(InventoryManager inventory);
    void Interact(); // 본문 없이 선언만
}