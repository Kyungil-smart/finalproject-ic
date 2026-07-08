
using Cysharp.Threading.Tasks;

public interface ISaveManager
{
    public int CurrentSlot { get; }
    public void LoadAllSlots();
    public bool IsEmpty(int slot);
    public SaveMeta GetMeta(int slot);
    public void Save();
    public UniTask Load(int slot);
    public void SelectSlot(int slot);
    public void DeleteSlot(int slot);
    public void ResetAll();
}